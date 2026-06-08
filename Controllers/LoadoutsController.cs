using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;
using RaidersVault.Services;

namespace RaidersVault.Controllers;

public class LoadoutsController : BaseController
{
    private readonly RaidersVaultContext _context;

    public LoadoutsController(RaidersVaultContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        searchTerm = InputSanitizer.CleanOptional(searchTerm);

        var query = _context.Loadouts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x =>
                x.Name.Contains(searchTerm) ||
                x.ActivityType.Contains(searchTerm) ||
                x.MapOrEvent.Contains(searchTerm) ||
                (x.FocusArea != null && x.FocusArea.Contains(searchTerm)) ||
                (x.GearNotes != null && x.GearNotes.Contains(searchTerm)));
        }

        ViewBag.SearchTerm = searchTerm;

        var loadouts = await query
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(loadouts);
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
    public async Task<IActionResult> Create(Loadout loadout)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        CleanLoadout(loadout);

        ModelState.Clear();
        TryValidateModel(loadout);

        if (!ModelState.IsValid)
        {
            return View(loadout);
        }

        loadout.CreatedAt = DateTime.Now;
        loadout.UpdatedAt = DateTime.Now;

        _context.Loadouts.Add(loadout);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Loadout created successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var loadout = await _context.Loadouts.FindAsync(id);

        if (loadout == null)
        {
            return NotFound();
        }

        return View(loadout);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Loadout loadout)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        if (id != loadout.Id)
        {
            return NotFound();
        }

        CleanLoadout(loadout);

        ModelState.Clear();
        TryValidateModel(loadout);

        if (!ModelState.IsValid)
        {
            return View(loadout);
        }

        loadout.UpdatedAt = DateTime.Now;

        _context.Loadouts.Update(loadout);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Loadout updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var loadout = await _context.Loadouts.FindAsync(id);

        if (loadout == null)
        {
            return NotFound();
        }

        return View(loadout);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var loadout = await _context.Loadouts.FindAsync(id);

        if (loadout != null)
        {
            _context.Loadouts.Remove(loadout);
            await _context.SaveChangesAsync();
        }

        TempData["Message"] = "Loadout deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private static void CleanLoadout(Loadout loadout)
    {
        loadout.Name = InputSanitizer.Clean(loadout.Name);
        loadout.ActivityType = InputSanitizer.Clean(loadout.ActivityType);
        loadout.MapOrEvent = InputSanitizer.Clean(loadout.MapOrEvent);
        loadout.FocusArea = InputSanitizer.CleanOptional(loadout.FocusArea);
        loadout.RiskLevel = InputSanitizer.CleanOptional(loadout.RiskLevel);
        loadout.GearNotes = InputSanitizer.CleanOptional(loadout.GearNotes);
        loadout.Notes = InputSanitizer.CleanOptional(loadout.Notes);
    }
}