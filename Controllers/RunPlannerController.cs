using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Services;
using RaidersVault.ViewModels;

namespace RaidersVault.Controllers;

public class RunPlannerController : BaseController
{
    private readonly RaidersVaultContext _context;
    private readonly LoadoutRecommendationService _loadoutService;

    public RunPlannerController(
        RaidersVaultContext context,
        LoadoutRecommendationService loadoutService)
    {
        _context = context;
        _loadoutService = loadoutService;
    }

    private static readonly List<string> GoalOptions = new()
    {
        "Loot Run",
        "Quest Progression",
        "Blueprint Farming",
        "PvP Hunting",
        "ARC Hunting",
        "Extraction Practice"
    };

    private static readonly List<string> StyleOptions = new()
    {
        "PvE",
        "PvP",
        "Balanced"
    };

    public async Task<IActionResult> Index(
        string? selectedMap = null,
        string? selectedCondition = null,
        string? selectedGoal = null,
        string? selectedStyle = null,
        int? totalSkillPoints = null)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var profile = await _context.PlayerProfiles.FirstOrDefaultAsync();

        var mapConditionOptions = await LoadMapConditionOptionsAsync();

        var mapOptions = mapConditionOptions
            .Select(x => x.Key)
            .OrderBy(x => x)
            .ToList();

        selectedMap = PickValid(
            selectedMap ?? profile?.DefaultMap,
            mapOptions,
            "Dam Battlegrounds");

        var conditionOptions = GetConditionOptionsForMap(
            mapConditionOptions,
            selectedMap);

        selectedCondition = PickValid(
            selectedCondition,
            conditionOptions,
            conditionOptions.First());

        selectedGoal = PickValid(
            selectedGoal,
            GoalOptions,
            "Loot Run");

        selectedStyle = PickValid(
            selectedStyle ?? profile?.PreferredPlaystyle,
            StyleOptions,
            "Balanced");

        var skillPoints = totalSkillPoints ?? profile?.CurrentSkillPoints ?? 20;

        if (skillPoints < 0)
        {
            skillPoints = 0;
        }

        var blueprints = await _context.Blueprints
            .OrderBy(x => x.Name)
            .ToListAsync();

        var quests = await _context.Quests
            .OrderBy(x => x.Name)
            .ToListAsync();

        var vm = new RunPlannerViewModel
        {
            SelectedMap = selectedMap,
            SelectedCondition = selectedCondition,
            SelectedGoal = selectedGoal,
            SelectedStyle = selectedStyle,
            TotalSkillPoints = skillPoints,

            Maps = mapOptions,
            Conditions = conditionOptions,
            Goals = GoalOptions,
            Styles = StyleOptions
        };

        vm.Loadout = _loadoutService.Build(
            selectedMap,
            selectedCondition,
            selectedStyle,
            skillPoints);

        vm.SuggestedSkills = BuildSkillSuggestions(
            selectedStyle,
            skillPoints);

        ApplyGoalRecommendations(vm, blueprints, quests);
        BuildPlannerSummary(vm);

        return View(vm);
    }

    private async Task<Dictionary<string, List<string>>> LoadMapConditionOptionsAsync()
    {
        var rows = await _context.MapConditionOptions
            .OrderBy(x => x.MapName)
            .ThenBy(x => x.DisplayOrder)
            .ToListAsync();

        if (rows.Any())
        {
            return rows
                .GroupBy(x => x.MapName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(y => y.ConditionName).ToList());
        }

        return new Dictionary<string, List<string>>
        {
            ["Dam Battlegrounds"] = new() { "Standard Patrol" }
        };
    }

    private static List<string> GetConditionOptionsForMap(
        Dictionary<string, List<string>> mapConditionOptions,
        string map)
    {
        return mapConditionOptions.TryGetValue(map, out var conditions)
            ? conditions
            : mapConditionOptions["Dam Battlegrounds"];
    }

    private static string PickValid(
        string? value,
        List<string> options,
        string fallback)
    {
        if (!string.IsNullOrWhiteSpace(value) &&
            options.Contains(value))
        {
            return value;
        }

        return fallback;
    }

    private static List<SkillRecommendationItem> BuildSkillSuggestions(
        string selectedStyle,
        int totalSkillPoints)
    {
        if (totalSkillPoints <= 0)
        {
            return new List<SkillRecommendationItem>();
        }

        return selectedStyle switch
        {
            "PvE" => new List<SkillRecommendationItem>
            {
                new()
                {
                    Branch = "Survival",
                    Name = "Looter's Instincts",
                    RecommendedPoints = 5,
                    MaxPoints = 5,
                    Requires = "Agile Croucher",
                    Reason = "Improves looting speed and farming value."
                },
                new()
                {
                    Branch = "Survival",
                    Name = "Silent Scavenger",
                    RecommendedPoints = 5,
                    MaxPoints = 5,
                    Requires = "Looter's Instincts",
                    Reason = "Makes looting safer and quieter."
                },
                new()
                {
                    Branch = "Conditioning",
                    Name = "Used To The Weight",
                    RecommendedPoints = 5,
                    MaxPoints = 5,
                    Requires = "None",
                    Reason = "Reduces shield movement penalty."
                },
                new()
                {
                    Branch = "Mobility",
                    Name = "Nimble Climber",
                    RecommendedPoints = 5,
                    MaxPoints = 5,
                    Requires = "None",
                    Reason = "Improves routing and extraction movement."
                }
            },

            "PvP" => new List<SkillRecommendationItem>
            {
                new()
                {
                    Branch = "Mobility",
                    Name = "Nimble Climber",
                    RecommendedPoints = 5,
                    MaxPoints = 5,
                    Requires = "None",
                    Reason = "Improves flanks and repositioning."
                },
                new()
                {
                    Branch = "Mobility",
                    Name = "Marathon Runner",
                    RecommendedPoints = 5,
                    MaxPoints = 5,
                    Requires = "Nimble Climber",
                    Reason = "Supports chasing and disengaging."
                },
                new()
                {
                    Branch = "Conditioning",
                    Name = "Fight Or Flight",
                    RecommendedPoints = 5,
                    MaxPoints = 5,
                    Requires = "Blast-Born",
                    Reason = "Returns stamina after taking damage."
                }
            },

            _ => new List<SkillRecommendationItem>
            {
                new()
                {
                    Branch = "Mobility",
                    Name = "Nimble Climber",
                    RecommendedPoints = 5,
                    MaxPoints = 5,
                    Requires = "None",
                    Reason = "Strong general movement pick."
                },
                new()
                {
                    Branch = "Survival",
                    Name = "Looter's Instincts",
                    RecommendedPoints = 5,
                    MaxPoints = 5,
                    Requires = "Agile Croucher",
                    Reason = "Useful for loot runs and extraction."
                },
                new()
                {
                    Branch = "Conditioning",
                    Name = "Used To The Weight",
                    RecommendedPoints = 5,
                    MaxPoints = 5,
                    Requires = "None",
                    Reason = "Useful with shield-based kits."
                }
            }
        };
    }

    private static void ApplyGoalRecommendations(
        RunPlannerViewModel vm,
        List<Models.Blueprint> blueprints,
        List<Models.Quest> quests)
    {
        var openQuests = quests
            .Where(x => !x.Status.Contains(
                "complete",
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        var mapQuests = openQuests
            .Where(x => QuestMatchesMap(x, vm.SelectedMap))
            .Take(5)
            .ToList();

        var matchingBlueprints = blueprints
            .Where(x =>
                !x.Collected &&
                BlueprintMatchesPlan(
                    x,
                    vm.SelectedMap,
                    vm.SelectedCondition))
            .Take(5)
            .ToList();

        switch (vm.SelectedGoal)
        {
            case "Blueprint Farming":
                vm.BlueprintTarget =
                    "Focus on blueprint-heavy routes and prioritize high-value containers.";
                vm.ObjectiveTarget =
                    "Pair the run with open objectives from the selected map when possible.";
                vm.RouteAdvice =
                    "Stay mobile, avoid unnecessary fights, and rotate quickly after major loot areas.";
                break;

            case "Quest Progression":
                vm.BlueprintTarget =
                    matchingBlueprints.Any()
                        ? "Optional blueprint targets are listed below for this route."
                        : "No map or condition-specific blueprint target found for this route.";
                vm.ObjectiveTarget =
                    "Prioritize active quest locations connected to the selected map.";
                vm.RouteAdvice =
                    "Complete objectives first, then extract instead of forcing additional fights.";
                break;

            case "PvP Hunting":
                vm.BlueprintTarget =
                    matchingBlueprints.Any()
                        ? "Use the suggested blueprint list below as optional targets while moving through PvP routes."
                        : "No map or condition-specific blueprint target found for this route.";
                vm.ObjectiveTarget =
                    "Use nearby open objectives as secondary goals while moving through high-traffic areas.";
                vm.RouteAdvice =
                    "Carry recovery utility and avoid getting trapped in extended loot animations.";
                break;

            case "ARC Hunting":
                vm.BlueprintTarget =
                    matchingBlueprints.Any()
                        ? "Optional blueprint targets are listed below for this route."
                        : "No map or condition-specific blueprint target found for this route.";
                vm.ObjectiveTarget =
                    "Prioritize objectives that overlap with ARC-heavy areas or combat routes.";
                vm.RouteAdvice =
                    "Preserve shield resources and avoid wasting explosives on low-threat enemies.";
                break;

            case "Extraction Practice":
                vm.ObjectiveTarget =
                    "Focus on safer rotations, stamina management, and clean extraction paths.";
                vm.BlueprintTarget =
                    matchingBlueprints.Any()
                        ? "Optional blueprint targets are listed below for this route."
                        : "No map or condition-specific blueprint target found for this route.";
                vm.RouteAdvice =
                    "Avoid long fights and prioritize survival over loot value.";
                break;

            default:
                vm.BlueprintTarget =
                    matchingBlueprints.Any()
                        ? "Use the suggested blueprint list below as optional targets for this route."
                        : "No map or condition-specific blueprint target found for this route.";
                vm.ObjectiveTarget =
                    "Run a flexible route that supports both loot collection and safe extraction.";
                vm.RouteAdvice =
                    "Balance risk, inventory value, and positioning throughout the raid.";
                break;
        }

        vm.SuggestedBlueprints = matchingBlueprints;
        vm.SuggestedQuests = mapQuests.Any()
            ? mapQuests
            : openQuests.Take(5).ToList();
    }

    private static bool BlueprintMatchesPlan(
        Models.Blueprint blueprint,
        string map,
        string condition)
    {
        if (QuestRewardBlueprints.Contains(blueprint.Name))
        {
            return false;
        }

        if (!BlueprintRules.TryGetValue(blueprint.Name, out var rule))
        {
            return true;
        }

        var mapMatches =
            rule.Maps.Contains("All") ||
            rule.Maps.Contains(map);

        var conditionMatches =
            rule.Conditions.Contains("Any") ||
            rule.Conditions.Contains(condition);

        return mapMatches && conditionMatches;
    }

    private static bool QuestMatchesMap(
        Models.Quest quest,
        string map)
    {
        var source =
            $"{quest.RelatedActivity} {quest.Notes} {quest.CompletionNotes}";

        return source.Contains(
            map,
            StringComparison.OrdinalIgnoreCase);
    }

    private static readonly HashSet<string> QuestRewardBlueprints =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Burletta",
            "Hullcracker",
            "Lure Grenade",
            "Trigger Nade",
            "Trigger 'Nade"
        };

    private static readonly Dictionary<string, BlueprintRule> BlueprintRules =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Aphelion"] =
                new(new[] { "Stella Montis" }, new[] { "Any" }),

            ["Deadline"] =
                new(new[] { "Stella Montis" }, new[] { "Any" }),

            ["Gas Mine"] =
                new(new[] { "Stella Montis" }, new[] { "Any" }),

            ["Pulse Mine"] =
                new(new[] { "Stella Montis" }, new[] { "Any" }),

            ["Seeker Grenade"] =
                new(new[] { "Stella Montis" }, new[] { "Any" }),

            ["Trailblazer"] =
                new(new[] { "Stella Montis" }, new[] { "Any" }),

            ["Bobcat"] =
                new(new[] { "All" }, new[] { "Locked Gate", "Hurricane" }),

            ["Tempest"] =
                new(new[] { "All" }, new[] { "Night Raid", "Hurricane" }),

            ["Wolfpack"] =
                new(new[] { "All" }, new[] { "Night Raid" }),

            ["Equalizer"] =
                new(new[] { "All" }, new[] { "Harvester" }),

            ["Jupiter"] =
                new(new[] { "All" }, new[] { "Harvester" }),

            ["Dolabra"] =
                new(new[] { "All" }, new[] { "Close Scrutiny" }),

            ["Fireworks Box"] =
                new(new[] { "All" }, new[] { "Cold Snap" })
        };

    private static void BuildPlannerSummary(RunPlannerViewModel vm)
    {
        vm.SkillFocus =
            vm.SelectedStyle switch
            {
                "PvE" =>
                    "Recommended focus: Survival and Conditioning for sustain, recovery, and safer extraction.",

                "PvP" =>
                    "Recommended focus: Mobility and Conditioning for repositioning, stamina, and combat pressure.",

                _ =>
                    "Recommended focus: Balanced early investment between Survival, Mobility, and Conditioning."
            };

        vm.Summary =
            $"The current plan is configured for {vm.SelectedStyle} play on "
            + $"{vm.SelectedMap} during {vm.SelectedCondition}. "
            + $"The selected goal is {vm.SelectedGoal}, and the planner assumes "
            + $"{vm.TotalSkillPoints} total skill points.";

        vm.RiskLevel = CalculateRiskLevel(vm.SelectedStyle, vm.SelectedCondition, vm.SelectedGoal);
        vm.PriorityScore = CalculatePriorityScore(vm);
        vm.ExtractionWindow = BuildExtractionWindow(vm);
        vm.RouteStops = BuildRouteStops(vm.SelectedMap, vm.SelectedCondition, vm.SelectedGoal);
        vm.ThreatNotes = BuildThreatNotes(vm.SelectedCondition, vm.SelectedStyle);
        vm.OperatorTips = BuildOperatorTips(vm.SelectedGoal, vm.SelectedStyle);
        vm.MetaForgeSignals = BuildMetaForgeSignals(vm.SelectedMap, vm.SelectedCondition, vm.SelectedGoal);
    }

    private static string CalculateRiskLevel(
        string selectedStyle,
        string selectedCondition,
        string selectedGoal)
    {
        var score = 35;

        if (selectedStyle == "PvP") score += 25;
        if (selectedGoal is "PvP Hunting" or "ARC Hunting") score += 20;
        if (selectedCondition is "Night Raid" or "Hurricane" or "Close Scrutiny" or "Harvester" or "Matriarch") score += 20;
        if (selectedCondition is "Locked Gate" or "Electromagnetic Storm" or "Hidden Bunker") score += 12;

        return score >= 75 ? "Critical" : score >= 55 ? "Elevated" : "Controlled";
    }

    private static int CalculatePriorityScore(RunPlannerViewModel vm)
    {
        var score = 45;
        score += vm.SuggestedBlueprints.Count * 8;
        score += vm.SuggestedQuests.Count * 5;
        score += vm.SuggestedSkills.Count * 4;

        if (vm.SelectedGoal == "Blueprint Farming") score += 12;
        if (vm.SelectedCondition != "Standard Patrol") score += 8;

        return Math.Clamp(score, 0, 100);
    }

    private static string BuildExtractionWindow(RunPlannerViewModel vm)
    {
        if (vm.RiskLevel == "Critical")
        {
            return "Extract after the first major objective, rare container hit, or confirmed blueprint lead.";
        }

        if (vm.SelectedGoal == "Blueprint Farming")
        {
            return "Extract once the primary container route is cleared or inventory value outweighs the next rotation.";
        }

        if (vm.SelectedGoal == "Quest Progression")
        {
            return "Extract immediately after the quest action is complete unless a nearby blueprint lead is safe.";
        }

        return "Extract after completing one primary goal, one secondary scan, and one safe loot rotation.";
    }

    private static List<string> BuildRouteStops(
        string selectedMap,
        string selectedCondition,
        string selectedGoal)
    {
        var stops = selectedMap switch
        {
            "Dam Battlegrounds" => new List<string> { "Residential containers", "Raider cache clusters", "Dam interior rooms", "Nearest safe extract" },
            "Buried City" => new List<string> { "Apartment loot loop", "Commercial interiors", "Medical or tech rooms", "Low-noise extraction path" },
            "Blue Gate" => new List<string> { "Village interiors", "Breachable rooms", "Industrial utility containers", "Outer extraction route" },
            "Spaceport" => new List<string> { "Admin buildings", "Tech/server rooms", "Weapon case checks", "Vehicle-side extraction" },
            "Riven Tides" => new List<string> { "Coastal loot lane", "Beachcomber side checks", "Stash and utility containers", "Waterline extraction path" },
            "Stella Montis" => new List<string> { "Interior medical route", "Research containers", "High-value room checks", "Fast extract before pressure builds" },
            _ => new List<string> { "Spawn safety check", "Primary loot route", "Objective overlap", "Clean extraction" }
        };

        if (selectedCondition != "Standard Patrol")
        {
            stops.Insert(1, $"Condition objective: {selectedCondition}");
        }

        if (selectedGoal == "Blueprint Farming")
        {
            stops.Insert(0, "Confirm missing blueprint target");
        }

        return stops.Take(6).ToList();
    }

    private static List<string> BuildThreatNotes(
        string selectedCondition,
        string selectedStyle)
    {
        var notes = new List<string>
        {
            "Avoid over-looting after the first high-value container chain.",
            "Re-check extracts before committing to a long interior route."
        };

        if (selectedStyle == "PvP")
        {
            notes.Add("Expect player pressure near breach rooms, caches, and event objectives.");
        }

        if (selectedCondition is "Night Raid" or "Hurricane")
        {
            notes.Add("Visibility and rotation timing are the main failure points for this condition.");
        }

        if (selectedCondition is "Harvester" or "Matriarch" or "Close Scrutiny")
        {
            notes.Add("Treat the condition target as contested and disengage if another squad arrives first.");
        }

        return notes;
    }

    private static List<string> BuildOperatorTips(
        string selectedGoal,
        string selectedStyle)
    {
        var tips = new List<string>
        {
            "Pin the best blueprint target before entering the run.",
            "Use the report page after the run to keep progress evidence clean."
        };

        if (selectedGoal == "Blueprint Farming")
        {
            tips.Add("Do not chase low-value containers once the route stops matching the target pool.");
        }

        if (selectedStyle == "PvE")
        {
            tips.Add("Prioritize smoke, healing, and shield recovery over extra combat utility.");
        }
        else if (selectedStyle == "PvP")
        {
            tips.Add("Carry one disengage tool and one pressure tool so the kit can both fight and reset.");
        }

        return tips;
    }

    private static List<string> BuildMetaForgeSignals(
        string selectedMap,
        string selectedCondition,
        string selectedGoal)
    {
        return new List<string>
        {
            "Companion-style planning: map, condition, item target, route, and checklist stay visible together.",
            $"Intel context: {selectedMap} / {selectedCondition} / {selectedGoal}.",
            "Blueprint routing favors container pools, event conditions, and fast extraction decisions instead of generic item lists."
        };
    }

    private record BlueprintRule(
        string[] Maps,
        string[] Conditions);
}