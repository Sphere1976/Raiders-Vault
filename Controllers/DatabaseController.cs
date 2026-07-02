using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;
using RaidersVault.ViewModels;

namespace RaidersVault.Controllers;

public class DatabaseController : BaseController
{
    private readonly RaidersVaultContext _db;
    public DatabaseController(RaidersVaultContext db) => _db = db;

    public async Task<IActionResult> Index(
        string? search,
        string? category,
        string? rarity,
        string? priority,
        string? source,
        string? focus,
        string sort = "needed",
        bool hideStocked = false,
        int page = 1,
        int pageSize = 72)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 24, 120);

        var allItems = _db.InventoryItems.AsNoTracking();
        var items = allItems.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            items = items.Where(x => x.Name.Contains(search) || x.UsedFor.Contains(search) || x.BestSource.Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(category))
        {
            items = items.Where(x => x.Category == category);
        }
        if (!string.IsNullOrWhiteSpace(rarity))
        {
            items = items.Where(x => x.Rarity == rarity);
        }
        if (!string.IsNullOrWhiteSpace(source))
        {
            items = items.Where(x => x.BestSource == source);
        }
        if (hideStocked)
        {
            items = items.Where(x => x.CurrentCount < x.KeepTarget);
        }

        var filteredItems = await items.ToListAsync();
        filteredItems = ApplyFocus(filteredItems, focus);

        if (!string.IsNullOrWhiteSpace(priority))
        {
            filteredItems = filteredItems
                .Where(x => string.Equals(x.Priority, priority, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        ViewBag.Search = search;
        ViewBag.Category = category;
        ViewBag.Rarity = rarity;
        ViewBag.Priority = priority;
        ViewBag.Source = source;
        ViewBag.Focus = focus;
        ViewBag.Sort = sort;
        ViewBag.HideStocked = hideStocked;

        var totalItems = filteredItems.Count;
        var neededItems = filteredItems.Count(x => x.CurrentCount < x.KeepTarget);
        var criticalItems = filteredItems.Count(x =>
            x.CurrentCount < x.KeepTarget &&
            (x.Rarity == "Epic" || x.Rarity == "Legendary"));
        var vaultValue = filteredItems.Sum(x => x.SellValue * x.CurrentCount);
        var totalNeededUnits = filteredItems.Sum(x => x.Needed);
        var stockedItems = filteredItems.Count(x => x.Needed == 0);
        var readinessPercent = totalItems == 0 ? 100 : (int)Math.Round((stockedItems / (double)totalItems) * 100);
        var topFarmTarget = filteredItems
            .Where(x => x.Needed > 0)
            .OrderByDescending(PlanningScore)
            .ThenByDescending(x => x.Needed)
            .ThenByDescending(x => x.SellValue)
            .FirstOrDefault();
        var runValue = filteredItems
            .Where(x => x.Needed > 0)
            .Sum(x => Math.Max(x.Needed, 1) * Math.Max(x.SellValue, 1));
        var favoriteGaps = filteredItems.Count(x => x.Favorite && x.Needed > 0);
        var criticalRouteCount = filteredItems
            .Where(x => x.Needed > 0 && x.Priority == "Critical")
            .Select(x => x.BestSource)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Min(page, totalPages);

        ViewBag.Categories = await allItems.Select(x => x.Category).Distinct().OrderBy(x => x).ToListAsync();
        ViewBag.Rarities = await allItems.Select(x => x.Rarity).Distinct().OrderBy(x => x).ToListAsync();
        ViewBag.Sources = await allItems.Select(x => x.BestSource).Distinct().OrderBy(x => x).ToListAsync();
        ViewBag.TotalItems = totalItems;
        ViewBag.NeededItems = neededItems;
        ViewBag.CriticalItems = criticalItems;
        ViewBag.VaultValue = vaultValue;
        ViewBag.TotalNeededUnits = totalNeededUnits;
        ViewBag.StockedItems = stockedItems;
        ViewBag.ReadinessPercent = readinessPercent;
        ViewBag.RunValue = runValue;
        ViewBag.FavoriteGaps = favoriteGaps;
        ViewBag.CriticalRouteCount = criticalRouteCount;
        ViewBag.TopFarmTargetName = topFarmTarget?.Name ?? "No urgent gaps";
        ViewBag.TopFarmTargetSource = topFarmTarget?.BestSource ?? "Inventory is on target";
        ViewBag.TopFarmTargetNeed = topFarmTarget?.Needed ?? 0;
        ViewBag.TopFarmTargetRarity = topFarmTarget?.Rarity ?? "Stocked";
        ViewBag.SourceClusters = BuildSourceClusters(filteredItems);
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;

        var sortedItems = sort switch
        {
            "name" => filteredItems.OrderBy(x => x.Name),
            "value" => filteredItems.OrderByDescending(x => x.SellValue * Math.Max(x.CurrentCount, 1)).ThenBy(x => x.Name),
            "efficiency" => filteredItems.OrderByDescending(PlanningScore).ThenByDescending(x => x.Needed).ThenBy(x => x.Name),
            "rarity" => filteredItems.OrderByDescending(x => RarityWeight(x.Rarity)).ThenByDescending(x => x.Needed).ThenBy(x => x.Name),
            "source" => filteredItems.OrderBy(x => x.BestSource).ThenByDescending(x => x.Needed).ThenBy(x => x.Name),
            _ => filteredItems.OrderByDescending(x => x.Favorite).ThenByDescending(x => x.Priority == "Critical").ThenByDescending(x => x.Needed).ThenBy(x => x.Name)
        };

        var inventoryItems = sortedItems
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return View(inventoryItems);
    }

    private static int RarityWeight(string rarity) => rarity switch
    {
        "Legendary" => 4,
        "Epic" => 3,
        "Rare" => 2,
        "Uncommon" => 1,
        _ => 0
    };

    private static List<InventoryItem> ApplyFocus(List<InventoryItem> items, string? focus) => focus switch
    {
        "critical" => items.Where(x => x.Priority == "Critical").ToList(),
        "favorites" => items.Where(x => x.Favorite).ToList(),
        "highValue" => items.Where(x => x.SellValue >= 250 || x.Rarity is "Epic" or "Legendary").ToList(),
        "farmNow" => items.Where(x => x.Needed > 0).OrderByDescending(PlanningScore).ToList(),
        "lowStock" => items.Where(x => x.KeepTarget > 0 && x.CurrentCount <= Math.Ceiling(x.KeepTarget * .35)).ToList(),
        _ => items
    };

    private static int PlanningScore(InventoryItem item)
    {
        if (item.Needed <= 0)
        {
            return 0;
        }

        var rarityScore = RarityWeight(item.Rarity) * 100;
        var favoriteScore = item.Favorite ? 120 : 0;
        var gapScore = item.Needed * 18;
        var valueScore = Math.Min(item.SellValue, 900) / 4;

        return rarityScore + favoriteScore + gapScore + valueScore;
    }

    private static List<DatabaseSourceClusterViewModel> BuildSourceClusters(IEnumerable<InventoryItem> items)
    {
        return items
            .Where(x => x.Needed > 0)
            .GroupBy(x => string.IsNullOrWhiteSpace(x.BestSource) ? "Unknown Source" : x.BestSource)
            .Select(group =>
            {
                var ordered = group
                    .OrderByDescending(PlanningScore)
                    .ThenByDescending(x => x.Needed)
                    .ToList();
                var topItem = ordered.FirstOrDefault();
                var criticalCount = ordered.Count(x => x.Priority == "Critical");
                var neededUnits = ordered.Sum(x => x.Needed);

                return new DatabaseSourceClusterViewModel
                {
                    Source = group.Key,
                    ItemCount = ordered.Count,
                    NeededUnits = neededUnits,
                    CriticalCount = criticalCount,
                    TotalValue = ordered.Sum(x => Math.Max(x.SellValue, 1) * Math.Max(x.Needed, 1)),
                    TopItemName = topItem?.Name ?? "No target",
                    Recommendation = BuildRouteRecommendation(group.Key, criticalCount, neededUnits)
                };
            })
            .OrderByDescending(x => x.CriticalCount)
            .ThenByDescending(x => x.NeededUnits)
            .ThenByDescending(x => x.TotalValue)
            .Take(6)
            .ToList();
    }

    private static string BuildRouteRecommendation(string source, int criticalCount, int neededUnits)
    {
        var lowerSource = source.ToLowerInvariant();
        var pressure = criticalCount > 0 ? "Run early and extract once the critical target is secured." : "Use as a secondary sweep after your main objective.";

        if (lowerSource.Contains("locked") || lowerSource.Contains("night") || lowerSource.Contains("storm") || lowerSource.Contains("bunker"))
        {
            return $"{pressure} Bring smoke, mobility, and a high-value safe pocket plan.";
        }

        if (lowerSource.Contains("computer") || lowerSource.Contains("electronic") || lowerSource.Contains("technical"))
        {
            return $"{pressure} Chain offices, terminals, and utility rooms before taking optional fights.";
        }

        if (lowerSource.Contains("residential") || lowerSource.Contains("apartment") || lowerSource.Contains("kitchen"))
        {
            return $"{pressure} Sweep interiors quickly and leave before the route turns noisy.";
        }

        return neededUnits >= 10
            ? $"{pressure} This route can clear multiple stash gaps in one run."
            : $"{pressure} Good focused pickup route for a short session.";
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
