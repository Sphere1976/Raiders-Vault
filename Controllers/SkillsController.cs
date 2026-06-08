using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;
using RaidersVault.ViewModels;

namespace RaidersVault.Controllers;

public class SkillsController : BaseController
{
    private readonly RaidersVaultContext _context;

    public SkillsController(RaidersVaultContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? branch,
        string? entryStyle,
        int totalPoints = 0)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        if (totalPoints < 0)
        {
            totalPoints = 0;
        }

        var selectedEntryStyle = PickEntryStyle(entryStyle);

        var allSkills = await _context.Skills
            .OrderBy(x => x.Branch)
            .ThenBy(x => x.Id)
            .ToListAsync();

        var savedSkills = await _context.UserSkills.ToListAsync();

        foreach (var skill in allSkills)
        {
            var saved = savedSkills
                .FirstOrDefault(x => x.SkillId == skill.Id);

            skill.CurrentPoints = saved?.CurrentPoints ?? 0;
        }

        var totalCurrentPoints = allSkills.Sum(x => x.CurrentPoints);

        var pointsForRecommendation =
            totalPoints > 0
                ? totalPoints
                : totalCurrentPoints;

        var visibleSkills = allSkills;

        if (!string.IsNullOrWhiteSpace(branch))
        {
            visibleSkills = allSkills
                .Where(x =>
                    x.Branch.Equals(
                        branch,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var viewModel = new SkillsPageViewModel
        {
            Skills = visibleSkills,
            SelectedBranch = branch,
            SelectedEntryStyle = selectedEntryStyle,
            TotalCurrentPoints = totalCurrentPoints,
            TotalAvailablePoints = pointsForRecommendation,

            PveRecommendations =
                BuildRecommendations(
                    allSkills,
                    pointsForRecommendation,
                    GetPvePriority(),
                    "PvE"),

            PvpRecommendations =
                BuildRecommendations(
                    allSkills,
                    pointsForRecommendation,
                    GetPvpPriority(),
                    "PvP"),

            BalancedRecommendations =
                BuildRecommendations(
                    allSkills,
                    pointsForRecommendation,
                    GetBalancedPriority(),
                    "Balanced")
        };

        return View(viewModel);
    }

    private static string PickEntryStyle(string? entryStyle)
    {
        if (string.Equals(
            entryStyle,
            "PvE",
            StringComparison.OrdinalIgnoreCase))
        {
            return "PvE";
        }

        if (string.Equals(
            entryStyle,
            "PvP",
            StringComparison.OrdinalIgnoreCase))
        {
            return "PvP";
        }

        return "Balanced";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(IFormCollection form)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var skills = await _context.Skills.ToListAsync();

        foreach (var skill in skills)
        {
            var fieldName = $"points_{skill.Id}";

            if (!form.ContainsKey(fieldName))
            {
                continue;
            }

            var enteredValue = form[fieldName].ToString();

            if (!int.TryParse(enteredValue, out var points))
            {
                points = 0;
            }

            if (points < 0)
            {
                points = 0;
            }

            if (points > skill.MaxPoints)
            {
                points = skill.MaxPoints;
            }

            var savedSkill = await _context.UserSkills
                .FirstOrDefaultAsync(x => x.SkillId == skill.Id);

            if (savedSkill == null)
            {
                savedSkill = new UserSkill
                {
                    SkillId = skill.Id,
                    CurrentPoints = points
                };

                _context.UserSkills.Add(savedSkill);
            }
            else
            {
                savedSkill.CurrentPoints = points;

                _context.UserSkills.Update(savedSkill);
            }
        }

        await _context.SaveChangesAsync();

        TempData["Message"] = "Skill selections saved.";

        return RedirectToAction(nameof(Index));
    }

    private static List<SkillRecommendationItem> BuildRecommendations(
        List<Skill> skills,
        int availablePoints,
        List<SkillPriority> priorityList,
        string entryStyle)
    {
        var pointsLeft = Math.Max(0, availablePoints);

        var pointsBySkillId = new Dictionary<int, int>();

        foreach (var priority in priorityList)
        {
            if (pointsLeft <= 0)
            {
                break;
            }

            var skill = FindSkill(
                skills,
                priority.Name);

            if (skill == null)
            {
                continue;
            }

            var currentPoints = GetCurrentPoints(
                pointsBySkillId,
                skill);

            var room = skill.MaxPoints - currentPoints;

            if (room <= 0)
            {
                continue;
            }

            var pointsToAdd = Math.Min(
                room,
                pointsLeft);

            pointsBySkillId[skill.Id] =
                currentPoints + pointsToAdd;

            pointsLeft -= pointsToAdd;
        }

        SpendLeftoverPoints(
            skills,
            priorityList,
            pointsBySkillId,
            ref pointsLeft,
            entryStyle);

        return skills
            .Where(x =>
                GetCurrentPoints(
                    pointsBySkillId,
                    x) > 0)
            .OrderBy(x => BranchSort(x.Branch, entryStyle))
            .ThenBy(x => x.Id)
            .Select(x => new SkillRecommendationItem
            {
                Branch = x.Branch,
                Name = x.Name,
                MaxPoints = x.MaxPoints,
                RecommendedPoints =
                    GetCurrentPoints(
                        pointsBySkillId,
                        x),
                Requires =
                    string.IsNullOrWhiteSpace(x.Requires)
                        ? "None"
                        : x.Requires,
                Reason =
                    GetReason(
                        priorityList,
                        x,
                        entryStyle)
            })
            .ToList();
    }

    private static void SpendLeftoverPoints(
        List<Skill> skills,
        List<SkillPriority> priorityList,
        Dictionary<int, int> pointsBySkillId,
        ref int pointsLeft,
        string entryStyle)
    {
        while (pointsLeft > 0)
        {
            var before = pointsLeft;

            foreach (var priority in priorityList)
            {
                if (pointsLeft <= 0)
                {
                    break;
                }

                var skill = FindSkill(
                    skills,
                    priority.Name);

                if (skill == null)
                {
                    continue;
                }

                AddOnePointIfPossible(
                    skill,
                    pointsBySkillId,
                    ref pointsLeft);
            }

            if (pointsLeft < before)
            {
                continue;
            }

            foreach (var skill in skills
                .OrderBy(x => BranchSort(x.Branch, entryStyle))
                .ThenBy(x => x.Id))
            {
                if (pointsLeft <= 0)
                {
                    break;
                }

                AddOnePointIfPossible(
                    skill,
                    pointsBySkillId,
                    ref pointsLeft);
            }

            if (pointsLeft == before)
            {
                break;
            }
        }
    }

    private static void AddOnePointIfPossible(
        Skill skill,
        Dictionary<int, int> pointsBySkillId,
        ref int pointsLeft)
    {
        if (pointsLeft <= 0)
        {
            return;
        }

        var currentPoints = GetCurrentPoints(
            pointsBySkillId,
            skill);

        if (currentPoints >= skill.MaxPoints)
        {
            return;
        }

        pointsBySkillId[skill.Id] = currentPoints + 1;

        pointsLeft--;
    }

    private static Skill? FindSkill(
        List<Skill> skills,
        string name)
    {
        return skills.FirstOrDefault(x =>
            SameName(x.Name, name));
    }

    private static int GetCurrentPoints(
        Dictionary<int, int> pointsBySkillId,
        Skill skill)
    {
        return pointsBySkillId.TryGetValue(
            skill.Id,
            out var points)
            ? points
            : 0;
    }

    private static string GetReason(
        List<SkillPriority> priorityList,
        Skill skill,
        string entryStyle)
    {
        var priority = priorityList
            .FirstOrDefault(x =>
                SameName(x.Name, skill.Name));

        if (priority != null)
        {
            return priority.Reason;
        }

        return entryStyle switch
        {
            "PvE" =>
                "Additional point allocation for PvE planning support.",

            "PvP" =>
                "Additional point allocation for PvP planning support.",

            _ =>
                "Additional point allocation for balanced planning support."
        };
    }

    private static int BranchSort(
        string branch,
        string entryStyle)
    {
        if (entryStyle == "PvE")
        {
            return branch switch
            {
                "Survival" => 1,
                "Conditioning" => 2,
                "Mobility" => 3,
                _ => 4
            };
        }

        if (entryStyle == "PvP")
        {
            return branch switch
            {
                "Mobility" => 1,
                "Conditioning" => 2,
                "Survival" => 3,
                _ => 4
            };
        }

        return branch switch
        {
            "Survival" => 1,
            "Mobility" => 2,
            "Conditioning" => 3,
            _ => 4
        };
    }

    private static bool SameName(
        string first,
        string second)
    {
        return Normalize(first) == Normalize(second);
    }

    private static string Normalize(string value)
    {
        return value
            .Replace("’", "'")
            .Replace("`", "'")
            .Trim()
            .ToLowerInvariant();
    }

    private static List<SkillPriority> GetPvePriority()
    {
        return new List<SkillPriority>
        {
            new("Looter’s Instincts", "Early looting value for farming runs."),
            new("Silent Scavenger", "Quieter looting helps avoid unnecessary fights."),
            new("In-round Crafting", "Useful when raids run long and supplies get low."),
            new("Broad Shoulders", "More carrying value for extraction-focused runs."),
            new("Stubborn Mule", "Keeps heavy-loot runs practical."),
            new("Good As New", "Recovery value after taking damage."),
            new("Used To The Weight", "General survivability with less shield movement penalty."),
            new("Survivor's Stamina", "More stamina support for longer routes."),
            new("Nimble Climber", "Better movement for routing, escaping ARC enemies, and extracting."),
            new("Marathon Runner", "Useful stamina value for longer PvE paths."),
            new("Youthful Lungs", "Supports extended map movement."),
            new("Security Breach", "Useful utility pick for locked or secured loot areas."),
            new("Minesweeper", "Helps prevent avoidable damage during looting routes."),
            new("Traveling Tinkerer", "Extra utility for crafting and looting-focused raids."),
            new("Three Deep Breaths", "Recovery support when fights or retreats drag out."),
            new("One Raider’s Scraps", "Adds extra value to looting and resource runs."),
            new("Looter’s Luck", "Extra loot-focused value once core picks are covered.")
        };
    }

    private static List<SkillPriority> GetPvpPriority()
    {
        return new List<SkillPriority>
        {
            new("Nimble Climber", "Better escape routes, flanks, and rooftop movement."),
            new("Marathon Runner", "Helps chase, disengage, and rotate during fights."),
            new("Slip and Slide", "Faster movement when breaking line of sight."),
            new("Youthful Lungs", "Improves stamina use during longer fights."),
            new("Sturdy Ankles", "Reduces punishment from drops and aggressive rotations."),
            new("Carry The Momentum", "Keeps movement speed useful after movement actions."),
            new("Effortless Roll", "Improves defensive movement and quick repositioning."),
            new("Ready To Roll", "More frequent rolls during combat."),
            new("Used To The Weight", "Keeps shield builds from feeling too slow."),
            new("Fight Or Flight", "Returns stamina after taking player damage."),
            new("Downed But Determined", "Adds value when a revive is possible."),
            new("Back On Your Feet", "Useful team-fight recovery pick."),
            new("Agile Croucher", "Quieter close-range repositioning."),
            new("Suffer In Silence", "Reduces giveaways while hurt."),
            new("Three Deep Breaths", "Helps recover during extended PvP engagements."),
            new("Heroic Leap", "Extra movement option for aggressive repositioning."),
            new("Off The Wall", "Supports stronger vertical movement routes.")
        };
    }

    private static List<SkillPriority> GetBalancedPriority()
    {
        return new List<SkillPriority>
        {
            new("Nimble Climber", "Strong general movement pick for both PvE and PvP."),
            new("Looter’s Instincts", "Keeps the build useful for loot runs."),
            new("Used To The Weight", "General survivability pick that works in most raids."),
            new("Marathon Runner", "Stamina value for travel, fights, and extraction."),
            new("Silent Scavenger", "Makes looting safer without fully ignoring combat."),
            new("Youthful Lungs", "Helps with sustained movement across the map."),
            new("Good As New", "Recovery value after bad engagements."),
            new("Slip and Slide", "Movement pick for escaping or repositioning."),
            new("Broad Shoulders", "Adds practical loot-carrying value."),
            new("Fight Or Flight", "Combat stamina support when taking damage."),
            new("In-round Crafting", "General utility for longer raids."),
            new("Effortless Roll", "Useful movement and survival pick."),
            new("Security Breach", "Adds PvE utility without overcommitting to looting."),
            new("Suffer In Silence", "Useful stealth-survival pick when injured."),
            new("Three Deep Breaths", "General recovery support."),
            new("Stubborn Mule", "More value for extract-focused runs."),
            new("Ready To Roll", "Extra combat movement once core skills are covered.")
        };
    }

    private record SkillPriority(
        string Name,
        string Reason);
}