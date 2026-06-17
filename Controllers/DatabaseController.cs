using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;

namespace RaidersVault.Controllers;

public class DatabaseController : BaseController
{
    private readonly RaidersVaultContext _db;
    public DatabaseController(RaidersVaultContext db) => _db = db;

    public async Task<IActionResult> Index(string? search, bool hideStocked = false, int page = 1, int pageSize = 72)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 24, 120);

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

        var totalItems = await items.CountAsync();
        var neededItems = await items.CountAsync(x => x.CurrentCount < x.KeepTarget);
        var criticalItems = await items.CountAsync(x =>
            x.CurrentCount < x.KeepTarget &&
            (x.Rarity == "Epic" || x.Rarity == "Legendary"));
        var vaultValue = await items.SumAsync(x => x.SellValue * x.CurrentCount);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Min(page, totalPages);

        ViewBag.TotalItems = totalItems;
        ViewBag.NeededItems = neededItems;
        ViewBag.CriticalItems = criticalItems;
        ViewBag.VaultValue = vaultValue;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;

        var inventoryItems = await items
            .OrderByDescending(x => x.Favorite)
            .ThenByDescending(x => x.KeepTarget - x.CurrentCount)
            .ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(inventoryItems);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCount(int id, int currentCount)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var item = await _db.InventoryItems.FindAsync(id);
        if (item is null) return NotFound();
        item.CurrentCount = Math.Max(0, currentCount);
        item.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return Json(new
            {
                id = item.Id,
                currentCount = item.CurrentCount,
                keepTarget = item.KeepTarget,
                needed = item.Needed,
                priority = item.Priority,
                stocked = item.Needed == 0
            });
        }

        return RedirectToAction(nameof(Index));
    }
}
