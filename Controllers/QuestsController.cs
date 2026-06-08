using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;
using RaidersVault.Services;

namespace RaidersVault.Controllers;

public class QuestsController : BaseController
{
    private readonly RaidersVaultContext _context;

    public QuestsController(RaidersVaultContext context)
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

        var query = _context.Quests.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string cleanSearch = searchTerm.Trim().ToLower();

            query = query.Where(x =>
                (x.Name ?? "").ToLower().Contains(cleanSearch) ||
                (x.Status ?? "").ToLower().Contains(cleanSearch) ||
                (x.Priority ?? "").ToLower().Contains(cleanSearch) ||
                (x.RelatedActivity ?? "").ToLower().Contains(cleanSearch) ||
                (x.CompletionNotes ?? "").ToLower().Contains(cleanSearch) ||
                (x.Notes ?? "").ToLower().Contains(cleanSearch));
        }

        ViewBag.SearchTerm = searchTerm;

        var quests = await query
            .OrderBy(x => x.Name)
            .ToListAsync();

        foreach (var quest in quests)
        {
            NormalizeQuestStatus(quest);
        }

        await _context.SaveChangesAsync();

        return View(quests);
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
    public async Task<IActionResult> Create(Quest quest)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        CleanQuest(quest);

        ModelState.Clear();
        TryValidateModel(quest);

        if (!ModelState.IsValid)
        {
            return View(quest);
        }

        quest.CreatedAt = DateTime.Now;
        quest.UpdatedAt = DateTime.Now;

        _context.Quests.Add(quest);

        await _context.SaveChangesAsync();

        TempData["Message"] = "Quest created successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var quest = await _context.Quests.FindAsync(id);

        if (quest == null)
        {
            return NotFound();
        }

        NormalizeQuestStatus(quest);

        await _context.SaveChangesAsync();

        return View(quest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Quest quest)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        if (id != quest.Id)
        {
            return NotFound();
        }

        CleanQuest(quest);

        ModelState.Clear();
        TryValidateModel(quest);

        if (!ModelState.IsValid)
        {
            return View(quest);
        }

        quest.UpdatedAt = DateTime.Now;

        _context.Quests.Update(quest);

        await _context.SaveChangesAsync();

        TempData["Message"] = "Quest updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleComplete(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var quest = await _context.Quests.FindAsync(id);

        if (quest == null)
        {
            return NotFound();
        }

        NormalizeQuestStatus(quest);

        if (IsQuestComplete(quest))
        {
            quest.Status = "In Progress";
            TempData["Message"] = "Quest marked incomplete.";
        }
        else
        {
            quest.Status = "Complete";
            TempData["Message"] = "Quest marked complete.";
        }

        quest.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var quest = await _context.Quests.FindAsync(id);

        if (quest == null)
        {
            return NotFound();
        }

        NormalizeQuestStatus(quest);

        await _context.SaveChangesAsync();

        return View(quest);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var quest = await _context.Quests.FindAsync(id);

        if (quest != null)
        {
            _context.Quests.Remove(quest);

            await _context.SaveChangesAsync();
        }

        TempData["Message"] = "Quest deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private static void CleanQuest(Quest quest)
    {
        quest.Name = InputSanitizer.Clean(quest.Name);
        quest.Status = InputSanitizer.Clean(quest.Status);
        quest.Priority = InputSanitizer.Clean(quest.Priority);

        quest.RelatedActivity =
            InputSanitizer.CleanOptional(quest.RelatedActivity);

        quest.CompletionNotes =
            InputSanitizer.CleanOptional(quest.CompletionNotes);

        quest.Notes =
            InputSanitizer.CleanOptional(quest.Notes);

        NormalizeQuestStatus(quest);
    }

    private static bool IsQuestComplete(Quest quest)
    {
        return string.Equals(
                   quest.Status,
                   "Complete",
                   StringComparison.OrdinalIgnoreCase)
               || string.Equals(
                   quest.Status,
                   "Completed",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static void NormalizeQuestStatus(Quest quest)
    {
        if (IsQuestComplete(quest))
        {
            quest.Status = "Complete";
            return;
        }

        if (string.Equals(
            quest.Status,
            "Tracking",
            StringComparison.OrdinalIgnoreCase))
        {
            quest.Status = "Tracking";
            return;
        }

        quest.Status = "In Progress";
    }
}