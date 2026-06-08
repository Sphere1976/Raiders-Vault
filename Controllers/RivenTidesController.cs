using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;
using RaidersVault.Services;

namespace RaidersVault.Controllers;

public class RivenTidesController : BaseController
{
    private readonly RaidersVaultContext _context;

    public RivenTidesController(RaidersVaultContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? searchTerm,
        string? recordType)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        searchTerm = InputSanitizer.CleanOptional(searchTerm);
        recordType = InputSanitizer.CleanOptional(recordType);

        var query = _context.RivenTidesRecords.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x =>
                x.Name.Contains(searchTerm) ||
                x.RecordType.Contains(searchTerm) ||
                x.Zone.Contains(searchTerm) ||
                x.RiskLevel.Contains(searchTerm) ||
                (x.RecommendedTool != null &&
                 x.RecommendedTool.Contains(searchTerm)) ||
                (x.LootFocus != null &&
                 x.LootFocus.Contains(searchTerm)) ||
                (x.Notes != null &&
                 x.Notes.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(recordType))
        {
            query = query.Where(x => x.RecordType == recordType);
        }

        ViewBag.SearchTerm = searchTerm;
        ViewBag.RecordType = recordType;

        ViewBag.TotalRecords =
            await _context.RivenTidesRecords.CountAsync();

        ViewBag.CompletedRecords =
            await _context.RivenTidesRecords.CountAsync(x => x.Completed);

        ViewBag.RecordTypes =
            await _context.RivenTidesRecords
                .Select(x => x.RecordType)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

        var records = await query
            .OrderBy(x => x.RecordType)
            .ThenBy(x => x.Zone)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return View(records);
    }

    public IActionResult Create()
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        return View(new RivenTidesRecord
        {
            RecordType = "Zone Note",
            Zone = "Riven Tides",
            RiskLevel = "Medium"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RivenTidesRecord record)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        CleanRecord(record);

        ModelState.Clear();
        TryValidateModel(record);

        if (!ModelState.IsValid)
        {
            return View(record);
        }

        record.CreatedAt = DateTime.Now;
        record.UpdatedAt = DateTime.Now;

        _context.RivenTidesRecords.Add(record);

        await _context.SaveChangesAsync();

        TempData["Message"] =
            "Riven Tides entry created successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var record = await _context.RivenTidesRecords.FindAsync(id);

        if (record == null)
        {
            return NotFound();
        }

        return View(record);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RivenTidesRecord record)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        if (id != record.Id)
        {
            return NotFound();
        }

        CleanRecord(record);

        ModelState.Clear();
        TryValidateModel(record);

        if (!ModelState.IsValid)
        {
            return View(record);
        }

        record.UpdatedAt = DateTime.Now;

        _context.RivenTidesRecords.Update(record);

        await _context.SaveChangesAsync();

        TempData["Message"] =
            "Riven Tides entry updated successfully.";

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

        var record = await _context.RivenTidesRecords.FindAsync(id);

        if (record == null)
        {
            return NotFound();
        }

        record.Completed = !record.Completed;
        record.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Message"] = record.Completed
            ? "Riven Tides entry marked complete."
            : "Riven Tides entry marked active.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var record = await _context.RivenTidesRecords.FindAsync(id);

        if (record == null)
        {
            return NotFound();
        }

        return View(record);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var record = await _context.RivenTidesRecords.FindAsync(id);

        if (record != null)
        {
            _context.RivenTidesRecords.Remove(record);

            await _context.SaveChangesAsync();
        }

        TempData["Message"] =
            "Riven Tides entry deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private static void CleanRecord(RivenTidesRecord record)
    {
        record.Name = InputSanitizer.Clean(record.Name);
        record.RecordType = InputSanitizer.Clean(record.RecordType);
        record.Zone = InputSanitizer.Clean(record.Zone);
        record.RiskLevel = InputSanitizer.Clean(record.RiskLevel);

        record.RecommendedTool =
            InputSanitizer.CleanOptional(record.RecommendedTool);

        record.LootFocus =
            InputSanitizer.CleanOptional(record.LootFocus);

        record.Notes =
            InputSanitizer.CleanOptional(record.Notes);
    }
}