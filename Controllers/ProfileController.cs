using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;
using RaidersVault.Services;

namespace RaidersVault.Controllers;

public class ProfileController : BaseController
{
    private readonly RaidersVaultContext _context;

    public ProfileController(RaidersVaultContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var profile = await GetProfileAsync();

        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PlayerProfile profile)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        profile.PlayerName = InputSanitizer.Clean(profile.PlayerName);
        profile.PreferredPlaystyle = NormalizeStyle(profile.PreferredPlaystyle);
        profile.DefaultMap = InputSanitizer.Clean(profile.DefaultMap);
        profile.Notes = InputSanitizer.CleanOptional(profile.Notes);
        profile.CurrentSkillPoints = Math.Clamp(profile.CurrentSkillPoints, 0, 120);

        ModelState.Clear();
        TryValidateModel(profile);

        if (!ModelState.IsValid)
        {
            return View(profile);
        }

        var existing = await GetProfileAsync();

        existing.PlayerName = profile.PlayerName;
        existing.PreferredPlaystyle = profile.PreferredPlaystyle;
        existing.DefaultMap = profile.DefaultMap;
        existing.CurrentSkillPoints = profile.CurrentSkillPoints;
        existing.Notes = profile.Notes;

        await _context.SaveChangesAsync();

        TempData["Message"] = "Profile settings saved.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<PlayerProfile> GetProfileAsync()
    {
        var profile = await _context.PlayerProfiles.FirstOrDefaultAsync();

        if (profile != null)
        {
            return profile;
        }

        profile = new PlayerProfile();

        _context.PlayerProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return profile;
    }

    private static string NormalizeStyle(string? style)
    {
        if (string.Equals(style, "PvE", StringComparison.OrdinalIgnoreCase))
        {
            return "PvE";
        }

        if (string.Equals(style, "PvP", StringComparison.OrdinalIgnoreCase))
        {
            return "PvP";
        }

        return "Balanced";
    }
}