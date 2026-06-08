using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;
using RaidersVault.Services;
using RaidersVault.ViewModels;

namespace RaidersVault.Controllers;

public class MapConditionsController : BaseController
{
    private readonly LoadoutRecommendationService _loadoutService;
    private readonly BlueprintRecommendationService _blueprintService;
    private readonly RaidersVaultContext _context;

    public MapConditionsController(
        LoadoutRecommendationService loadoutService,
        BlueprintRecommendationService blueprintService,
        RaidersVaultContext context)
    {
        _loadoutService = loadoutService;
        _blueprintService = blueprintService;
        _context = context;
    }

    private static readonly List<string> StyleOptions = new()
    {
        "PvE",
        "PvP",
        "Balanced"
    };

    public async Task<IActionResult> Index(
        string? selectedMap,
        string? selectedCondition,
        string? selectedStyle,
        int totalSkillPoints = 0)
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var mapConditionOptions =
            await LoadMapConditionOptionsAsync();

        var mapOptions = mapConditionOptions
            .Select(x => x.Key)
            .OrderBy(x => x)
            .ToList();

        var map = PickValid(selectedMap, mapOptions, "Dam Battlegrounds");

        var conditionOptions =
            GetConditionOptionsForMap(
                mapConditionOptions,
                map);

        var condition = PickValid(
            selectedCondition,
            conditionOptions,
            conditionOptions.First());

        var style = PickValid(selectedStyle, StyleOptions, "Balanced");

        if (totalSkillPoints < 0)
        {
            totalSkillPoints = 0;
        }

        var blueprints = await _context.Blueprints
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(BuildRecommendation(
            map,
            condition,
            style,
            totalSkillPoints,
            mapOptions,
            conditionOptions,
            blueprints));
    }

    private MapConditionRecommendationViewModel BuildRecommendation(
        string map,
        string condition,
        string style,
        int totalSkillPoints,
        List<string> mapOptions,
        List<string> conditionOptions,
        List<Blueprint> blueprints)
    {
        var items = new List<MapConditionLoadoutItem>();

        AddBaseItems(items, style);
        AddConditionItems(items, condition, style);
        AddMapItems(items, map);

        return new MapConditionRecommendationViewModel
        {
            SelectedMap = map,
            SelectedCondition = condition,
            SelectedStyle = style,
            TotalSkillPoints = totalSkillPoints,
            Maps = mapOptions,
            Conditions = conditionOptions,
            Styles = StyleOptions,
            Summary = $"Recommended {style} entry plan for {condition} on {map}.",
            SkillTreeFocus = BuildSkillTreeFocus(style, totalSkillPoints),
            RiskLevel = BuildRiskLevel(condition, style),
            LoadoutItems = items,
            OptimalLoadout = _loadoutService.Build(
                map,
                condition,
                style,
                totalSkillPoints),
            BlueprintAlerts = _blueprintService.BuildConditionAlerts(
                blueprints,
                map,
                condition,
                style)
        };
    }

    private static void AddBaseItems(
        List<MapConditionLoadoutItem> items,
        string style)
    {
        if (style == "PvE")
        {
            items.Add(new()
            {
                Slot = "Main weapon",
                Recommendation = "Reliable mid-range ARC-clearing weapon",
                Reason = "Clears ARC threats without risking your best gear."
            });

            items.Add(new()
            {
                Slot = "Backup",
                Recommendation = "Low-cost close-range backup",
                Reason = "Protects you inside buildings and keeps the kit affordable."
            });

            items.Add(new()
            {
                Slot = "Utility",
                Recommendation = "Extra healing plus one escape item",
                Reason = "PvE runs need recovery and a clean extraction plan."
            });

            items.Add(new()
            {
                Slot = "Bag priority",
                Recommendation = "Medium backpack with room for quest items",
                Reason = "Gives space for materials without becoming too greedy."
            });
        }
        else if (style == "PvP")
        {
            items.Add(new()
            {
                Slot = "Main weapon",
                Recommendation = "Best controlled PvP weapon you can afford",
                Reason = "PvP runs need fast, decisive damage."
            });

            items.Add(new()
            {
                Slot = "Backup",
                Recommendation = "Fast-swap close-range option",
                Reason = "Helps finish fights after pressure or reloads."
            });

            items.Add(new()
            {
                Slot = "Utility",
                Recommendation = "Grenade, smoke, and quick healing",
                Reason = "Gives push, reset, and disengage options."
            });

            items.Add(new()
            {
                Slot = "Bag priority",
                Recommendation = "Medium or light bag",
                Reason = "Avoids over-investing when the kit is already risky."
            });
        }
        else
        {
            items.Add(new()
            {
                Slot = "Main weapon",
                Recommendation = "Flexible mid-range weapon",
                Reason = "Works against both ARC and raiders."
            });

            items.Add(new()
            {
                Slot = "Backup",
                Recommendation = "Budget close-range sidearm or compact weapon",
                Reason = "Covers indoor fights without raising kit cost too much."
            });

            items.Add(new()
            {
                Slot = "Utility",
                Recommendation = "Healing, smoke, and one explosive",
                Reason = "Lets you fight, disengage, or finish objectives."
            });

            items.Add(new()
            {
                Slot = "Bag priority",
                Recommendation = "Medium backpack",
                Reason = "Balanced space for loot and survival."
            });
        }
    }

    private static void AddConditionItems(
        List<MapConditionLoadoutItem> items,
        string condition,
        string style)
    {
        switch (condition)
        {
            case "Standard":
            case "Standard Patrol":
                items.Add(new()
                {
                    Slot = "Condition adjustment",
                    Recommendation = "Use a normal-cost kit and focus on quest progress or safe loot",
                    Reason = "Standard Patrol has no special modifier, so the best value is a flexible low-risk setup."
                });
                break;

            case "Night":
            case "Night Raid":
                items.Add(new()
                {
                    Slot = "Condition adjustment",
                    Recommendation = "Play slower, carry escape utility, avoid open-lane greed",
                    Reason = "Low visibility increases ambush and extraction risk."
                });
                break;

            case "Electromagnetic Storm":
                items.Add(new()
                {
                    Slot = "Condition adjustment",
                    Recommendation = "Bring a simple dependable kit",
                    Reason = "Storm conditions should be treated as higher uncertainty."
                });
                break;

            case "Hurricane":
                items.Add(new()
                {
                    Slot = "Condition adjustment",
                    Recommendation = "Target Hurricane Chests / First Wave Caches, then extract early",
                    Reason = "Hurricane creates the special cache route used for rare blueprint farming, but wind and exposure make greedy routes dangerous."
                });

                items.Add(new()
                {
                    Slot = "Blueprint target",
                    Recommendation = "Hurricane Chests / First Wave Caches",
                    Reason = "These are the condition-specific containers tied to Hurricane blueprint farming."
                });
                break;

            case "Cold Snap":
                items.Add(new()
                {
                    Slot = "Condition adjustment",
                    Recommendation = "Carry extra healing and avoid long exposed rotations",
                    Reason = "Cold and reduced visibility make slow outside routes more punishing."
                });
                break;

            case "Hidden Bunker":
            case "Locked Gate":
                items.Add(new()
                {
                    Slot = "Condition adjustment",
                    Recommendation = "Bring objective space and a close-range interior option",
                    Reason = "These conditions favor focused looting and indoor contact."
                });
                break;

            case "Launch Tower Loot":
            case "Beachcomber":
            case "Beachcombing":
            case "Last Resort Event":
            case "ARC Turbine":
            case "Lush Blooms":
            case "Bird City":
            case "Prospecting Probes":
            case "Husk Graveyard":
            case "Uncovered Caches":
                items.Add(new()
                {
                    Slot = "Condition adjustment",
                    Recommendation = style == "PvP"
                        ? "Expect traffic and bring fight-ready utility"
                        : "Use a budget loot kit and extract once target loot is secured",
                    Reason = "Loot-focused conditions usually attract other raiders."
                });
                break;

            case "Matriarch":
            case "Harvester":
                items.Add(new()
                {
                    Slot = "Condition adjustment",
                    Recommendation = "Bring extra ammo, healing, and an exit plan",
                    Reason = "Major event conditions can drain supplies and pull attention."
                });
                break;

            default:
                items.Add(new()
                {
                    Slot = "Condition adjustment",
                    Recommendation = "Avoid loud extended fights and prioritize clean rotations",
                    Reason = "Treat the map as more watched and contested."
                });
                break;
        }
    }

    private static void AddMapItems(
        List<MapConditionLoadoutItem> items,
        string map)
    {
        switch (map)
        {
            case "Buried City":
                items.Add(new()
                {
                    Slot = "Map adjustment",
                    Recommendation = "Favor close-to-mid range and quick healing",
                    Reason = "Urban routes create sudden close fights."
                });
                break;

            case "Spaceport":
                items.Add(new()
                {
                    Slot = "Map adjustment",
                    Recommendation = "Carry smoke and a steady mid-range weapon",
                    Reason = "Large structures and open approaches punish bad rotations."
                });
                break;

            case "Riven Tides":
                items.Add(new()
                {
                    Slot = "Map adjustment",
                    Recommendation = "Bring beachcombing utility, smoke, and a flexible mid-range weapon",
                    Reason = "Riven Tides mixes open coastal sightlines, dockyard routes, buried loot checks, and ARC Turbine risk."
                });
                break;

            case "The Blue Gate":
                items.Add(new()
                {
                    Slot = "Map adjustment",
                    Recommendation = "Prioritize mobility and supplies for long rotations",
                    Reason = "Route planning matters more on spread-out paths."
                });
                break;

            case "Stella Montis":
                items.Add(new()
                {
                    Slot = "Map adjustment",
                    Recommendation = "Use a cautious balanced kit until the route is known",
                    Reason = "Treat unknown routes as high-risk until scouted."
                });
                break;

            default:
                items.Add(new()
                {
                    Slot = "Map adjustment",
                    Recommendation = "General-purpose kit with room for materials",
                    Reason = "Dam Battlegrounds works well for flexible quest and resource runs."
                });
                break;
        }
    }

    private static string BuildSkillTreeFocus(
        string style,
        int totalSkillPoints)
    {
        var tierNote = totalSkillPoints >= 36
            ? "You can plan around late-branch unlocks."
            : totalSkillPoints >= 15
                ? "You can plan around mid-branch unlocks, but not every late skill."
                : "Stay focused on early reliable skills until more points are available.";

        if (style == "PvE")
        {
            return $"Use the PvE skill recommendation path. Favor survivability, carry capacity, and objective consistency. {tierNote}";
        }

        if (style == "PvP")
        {
            return $"Use the PvP skill recommendation path. Favor mobility, fight resets, stamina, and quick repositioning. {tierNote}";
        }

        return $"Use the Balanced skill recommendation path. Split points between survival tools and mobility. {tierNote}";
    }

    private static string BuildRiskLevel(
        string condition,
        string style)
    {
        var highRiskConditions = new[]
        {
            "Night",
            "Night Raid",
            "Matriarch",
            "Harvester",
            "Electromagnetic Storm",
            "Hurricane",
            "Cold Snap"
        };

        if (style == "PvP")
        {
            return "High";
        }

        if (highRiskConditions.Contains(condition))
        {
            return style == "Balanced" ? "Medium-High" : "Medium";
        }

        return style == "PvE" ? "Low-Medium" : "Medium";
    }

    private async Task<Dictionary<string, List<string>>>
        LoadMapConditionOptionsAsync()
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
            : new List<string> { "Standard Patrol" };
    }

    private static string PickValid(
        string? requested,
        List<string> options,
        string fallback)
    {
        if (string.IsNullOrWhiteSpace(requested))
        {
            return fallback;
        }

        return options.FirstOrDefault(x =>
            x.Equals(requested, StringComparison.OrdinalIgnoreCase)) ?? fallback;
    }
}