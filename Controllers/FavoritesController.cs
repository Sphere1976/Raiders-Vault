using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;
using RaidersVault.Services;

namespace RaidersVault.Controllers;

public class FavoritesController : BaseController
{
    private readonly RaidersVaultContext _context;

    public FavoritesController(RaidersVaultContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var favorites = await _context.FavoriteItems
            .OrderBy(x => x.ItemType)
            .ThenBy(x => x.DisplayName)
            .ToListAsync();

        return View(favorites);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(
        string itemType,
        int itemId,
        string displayName,
        string? returnController,
        string? returnAction)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        itemType = InputSanitizer.CleanOptional(itemType) ?? "Item";
        displayName = InputSanitizer.CleanOptional(displayName) ?? itemType;

        var existing = await _context.FavoriteItems
            .FirstOrDefaultAsync(x => x.ItemType == itemType && x.ItemId == itemId);

        if (existing == null)
        {
            var favorite = new FavoriteItem
            {
                ItemType = itemType,
                ItemId = itemId,
                DisplayName = displayName
            };

            _context.FavoriteItems.Add(favorite);
            TempData["Message"] = "Item pinned to favorites.";
        }
        else
        {
            _context.FavoriteItems.Remove(existing);
            TempData["Message"] = "Item removed from favorites.";
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(
            string.IsNullOrWhiteSpace(returnAction) ? "Index" : returnAction,
            string.IsNullOrWhiteSpace(returnController) ? "Home" : returnController);
    }
}