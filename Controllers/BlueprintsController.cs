using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;
using RaidersVault.Services;
using RaidersVault.ViewModels;

namespace RaidersVault.Controllers;

public class BlueprintsController : BaseController
{
    private readonly RaidersVaultContext _context;
    private readonly BlueprintRecommendationService _blueprintRecommendations;

    public BlueprintsController(
        RaidersVaultContext context,
        BlueprintRecommendationService blueprintRecommendations)
    {
        _context = context;
        _blueprintRecommendations = blueprintRecommendations;
    }

    public async Task<IActionResult> Index(
        string? searchTerm,
        string playstyle = "Balanced")
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        searchTerm = InputSanitizer.CleanOptional(searchTerm);
        playstyle = NormalizePlaystyle(playstyle);

        var query = _context.Blueprints.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string cleanSearch = searchTerm.Trim().ToLower();

            query = query.Where(x =>
                (x.Name ?? "").ToLower().Contains(cleanSearch) ||
                (x.Category ?? "").ToLower().Contains(cleanSearch) ||
                (x.CollectionStatus ?? "").ToLower().Contains(cleanSearch) ||
                (x.WhereToGet ?? "").ToLower().Contains(cleanSearch) ||
                (x.RecipeMaterials ?? "").ToLower().Contains(cleanSearch) ||
                (x.Notes ?? "").ToLower().Contains(cleanSearch));
        }

        var blueprints = await query
            .OrderBy(x => x.Name)
            .ToListAsync();

        foreach (var blueprint in blueprints)
        {
            SyncCollectedState(blueprint);
        }

        await _context.SaveChangesAsync();

        var farmPlans = blueprints.ToDictionary(
            x => x.Id,
            x => _blueprintRecommendations.BuildFarmPlan(x, playstyle));

        var model = new BlueprintIndexViewModel
        {
            SearchTerm = searchTerm,
            Playstyle = playstyle,
            Blueprints = blueprints,
            FarmPlans = farmPlans
        };

        return View(model);
    }

    public async Task<IActionResult> FarmPlan(
        int id,
        string playstyle = "Balanced")
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var blueprint = await _context.Blueprints.FindAsync(id);

        if (blueprint == null)
        {
            return NotFound();
        }

        SyncCollectedState(blueprint);

        await _context.SaveChangesAsync();

        return View(_blueprintRecommendations.BuildFarmPlan(
            blueprint,
            NormalizePlaystyle(playstyle)));
    }

    public IActionResult Create()
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Blueprint blueprint)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        CleanBlueprint(blueprint);

        ModelState.Clear();
        TryValidateModel(blueprint);

        if (!ModelState.IsValid)
        {
            return View(blueprint);
        }

        blueprint.CreatedAt = DateTime.Now;
        blueprint.UpdatedAt = DateTime.Now;

        _context.Add(blueprint);

        await _context.SaveChangesAsync();

        TempData["Message"] = "Blueprint created successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var blueprint = await _context.Blueprints.FindAsync(id);

        if (blueprint == null)
        {
            return NotFound();
        }

        SyncCollectedState(blueprint);

        await _context.SaveChangesAsync();

        return View(blueprint);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Blueprint blueprint)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        if (id != blueprint.Id)
        {
            return NotFound();
        }

        CleanBlueprint(blueprint);

        ModelState.Clear();
        TryValidateModel(blueprint);

        if (!ModelState.IsValid)
        {
            return View(blueprint);
        }

        blueprint.UpdatedAt = DateTime.Now;

        _context.Update(blueprint);

        await _context.SaveChangesAsync();

        TempData["Message"] = "Blueprint updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCollected(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var blueprint = await _context.Blueprints.FindAsync(id);

        if (blueprint == null)
        {
            return NotFound();
        }

        if (IsBlueprintCollected(blueprint))
        {
            blueprint.Collected = false;
            blueprint.CollectionStatus = "Not Collected";
            TempData["Message"] = "Blueprint marked not collected.";
        }
        else
        {
            blueprint.Collected = true;
            blueprint.CollectionStatus = "Collected";
            TempData["Message"] = "Blueprint marked collected.";
        }

        blueprint.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkTracking(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var blueprint = await _context.Blueprints.FindAsync(id);

        if (blueprint == null)
        {
            return NotFound();
        }

        blueprint.Collected = false;
        blueprint.CollectionStatus = "Tracking";
        blueprint.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Message"] = "Blueprint added to tracking.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var blueprint = await _context.Blueprints.FindAsync(id);

        if (blueprint == null)
        {
            return NotFound();
        }

        SyncCollectedState(blueprint);

        await _context.SaveChangesAsync();

        return View(blueprint);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var blueprint = await _context.Blueprints.FindAsync(id);

        if (blueprint != null)
        {
            _context.Blueprints.Remove(blueprint);
            await _context.SaveChangesAsync();
        }

        TempData["Message"] = "Blueprint deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private static string NormalizePlaystyle(string? playstyle)
    {
        if (string.Equals(
            playstyle,
            "PvE",
            StringComparison.OrdinalIgnoreCase))
        {
            return "PvE";
        }

        if (string.Equals(
            playstyle,
            "PvP",
            StringComparison.OrdinalIgnoreCase))
        {
            return "PvP";
        }

        return "Balanced";
    }

    private static void CleanBlueprint(Blueprint blueprint)
    {
        blueprint.Name = InputSanitizer.Clean(blueprint.Name);
        blueprint.Category = InputSanitizer.Clean(blueprint.Category);
        blueprint.CollectionStatus = InputSanitizer.Clean(blueprint.CollectionStatus);
        blueprint.RecipeMaterials = InputSanitizer.CleanOptional(blueprint.RecipeMaterials);
        blueprint.WhereToGet = InputSanitizer.CleanOptional(blueprint.WhereToGet);
        blueprint.SourceNotes = InputSanitizer.CleanOptional(blueprint.SourceNotes);
        blueprint.Notes = InputSanitizer.CleanOptional(blueprint.Notes);

        SyncCollectedState(blueprint);
    }

    private static bool IsBlueprintCollected(Blueprint blueprint)
    {
        return blueprint.Collected
            || string.Equals(
                blueprint.CollectionStatus,
                "Collected",
                StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                blueprint.CollectionStatus,
                "Obtained",
                StringComparison.OrdinalIgnoreCase);
    }

    private static void SyncCollectedState(Blueprint blueprint)
    {
        if (IsBlueprintCollected(blueprint))
        {
            blueprint.Collected = true;
            blueprint.CollectionStatus = "Collected";
            return;
        }

        if (string.Equals(
            blueprint.CollectionStatus,
            "Tracking",
            StringComparison.OrdinalIgnoreCase))
        {
            blueprint.Collected = false;
            blueprint.CollectionStatus = "Tracking";
            return;
        }

        blueprint.Collected = false;
        blueprint.CollectionStatus = "Not Collected";
    }
}