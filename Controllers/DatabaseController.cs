using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;

namespace RaidersVault.Controllers;

public class DatabaseController : BaseController
{
    private readonly RaidersVaultContext _db;
    public DatabaseController(RaidersVaultContext db) => _db = db;

    public async Task<IActionResult> Index(string? search, bool hideStocked = false)
    {
        RequireLogin();
        var items = _db.InventoryItems.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            items = items.Where(x => x.Name.Contains(search) || x.UsedFor.Contains(search) || x.BestSource.Contains(search));
        }
        if (hideStocked)
        {
            items = items.Where(x => x.CurrentCount < x.KeepTarget);
        }
        ViewBag.Search = search;
        ViewBag.HideStocked = hideStocked;
        var inventoryItems = await items
            .OrderByDescending(x => x.Favorite)
            .ThenBy(x => x.Name)
            .ToListAsync();

        var sortedItems = inventoryItems
            .OrderByDescending(x => x.Favorite)
            .ThenByDescending(x => x.Needed)
            .ThenBy(x => x.Name)
            .ToList();

        return View(sortedItems);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCount(int id, int currentCount)
    {
        RequireLogin();
        var item = await _db.InventoryItems.FindAsync(id);
        if (item is null) return NotFound();
        item.CurrentCount = Math.Max(0, currentCount);
        item.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
