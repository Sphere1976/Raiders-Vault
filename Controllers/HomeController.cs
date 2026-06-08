using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;

namespace RaidersVault.Controllers;

public class HomeController : BaseController
{
    private readonly RaidersVaultContext _context;

    public HomeController(RaidersVaultContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var profile = await _context.PlayerProfiles.FirstOrDefaultAsync();

        var totalQuests = await _context.Quests.CountAsync();

        var completedQuests = await _context.Quests.CountAsync(q =>
            q.Status == "Complete" ||
            q.Status == "Completed");

        var totalBlueprints = await _context.Blueprints.CountAsync();

        var collectedBlueprints = await _context.Blueprints.CountAsync(b =>
            b.Collected ||
            b.CollectionStatus == "Collected" ||
            b.CollectionStatus == "Obtained");

        var trackedBlueprint = await _context.Blueprints.FirstOrDefaultAsync(b =>
            !b.Collected &&
            (
                b.CollectionStatus == "Tracking" ||
                b.CollectionStatus == "Wanted" ||
                b.CollectionStatus == "Needed"
            ));

        ViewBag.LoadoutCount = await _context.Loadouts.CountAsync();
        ViewBag.QuestCount = totalQuests;
        ViewBag.BlueprintCount = totalBlueprints;

        ViewBag.CompletedQuestCount = completedQuests;
        ViewBag.ActiveQuestCount = totalQuests - completedQuests;

        ViewBag.CollectedBlueprintCount = collectedBlueprints;
        ViewBag.RemainingBlueprintCount = totalBlueprints - collectedBlueprints;

        ViewBag.QuestCompletionPercent =
            totalQuests == 0
                ? 0
                : (int)Math.Round((completedQuests / (double)totalQuests) * 100);

        ViewBag.BlueprintCompletionPercent =
            totalBlueprints == 0
                ? 0
                : (int)Math.Round((collectedBlueprints / (double)totalBlueprints) * 100);

        ViewBag.PlayerName = profile?.PlayerName ?? "Raider";
        ViewBag.DefaultMap = profile?.DefaultMap ?? "Dam Battlegrounds";
        ViewBag.PreferredPlaystyle = profile?.PreferredPlaystyle ?? "Balanced";
        ViewBag.CurrentSkillPoints = profile?.CurrentSkillPoints ?? 20;

        ViewBag.FavoriteCount = await _context.FavoriteItems.CountAsync();

        ViewBag.NextRunTitle =
            trackedBlueprint == null
                ? "Balanced loot and objective run"
                : $"Farm {trackedBlueprint.Name}";

        ViewBag.NextRunMap =
            trackedBlueprint != null &&
            (trackedBlueprint.WhereToGet ?? "").Contains("Hurricane")
                ? "Dam Battlegrounds or The Blue Gate"
                : ViewBag.DefaultMap;

        ViewBag.NextRunCondition =
            trackedBlueprint != null &&
            (trackedBlueprint.WhereToGet ?? "").Contains("Hurricane")
                ? "Hurricane"
                : "Standard Patrol or Night Raid";

        ViewBag.NextRunGoal =
            trackedBlueprint == null
                ? "Complete Quests"
                : "Farm Blueprint";

        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}