using RaidersVault.Models;
using RaidersVault.ViewModels;

namespace RaidersVault.Services;

public class BlueprintRecommendationService
{
    public BlueprintFarmPlanViewModel BuildFarmPlan(Blueprint blueprint, string playstyle)
    {
        playstyle = NormalizePlaystyle(playstyle);

        var plan = new BlueprintFarmPlanViewModel
        {
            BlueprintId = blueprint.Id,
            BlueprintName = blueprint.Name,
            Category = blueprint.Category,
            Playstyle = playstyle
        };

        ApplySpecificBlueprintRules(blueprint, plan);
        ApplyCategoryFallbacks(blueprint, plan);
        ApplyConditionRules(plan);
        ApplyPlaystyleLoadout(plan);
        BuildExplanation(plan);

        return plan;
    }

    private static string NormalizePlaystyle(string? playstyle)
    {
        if (string.Equals(playstyle, "PvE", StringComparison.OrdinalIgnoreCase))
        {
            return "PvE";
        }

        if (string.Equals(playstyle, "PvP", StringComparison.OrdinalIgnoreCase))
        {
            return "PvP";
        }

        return "Balanced";
    }

    private static void ApplySpecificBlueprintRules(
        Blueprint blueprint,
        BlueprintFarmPlanViewModel plan)
    {
        var name = blueprint.Name.ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(blueprint.WhereToGet)
            && blueprint.WhereToGet.Contains(
                "quest reward",
                StringComparison.OrdinalIgnoreCase))
        {
            Set(
                plan,
                "Quest Reward",
                95,
                "Quest Reward",
                "Quest completion",
                "Matching quest reward",
                "Questline reward path",
                "Finish the listed questline instead of farming random containers.");

            return;
        }

        if (name.Contains("aphelion"))
        {
            Set(
                plan,
                "Legendary / Boss Event",
                80,
                "Dam Battlegrounds, Spaceport, or Blue Gate",
                "Matriarch",
                "Matriarch core and boss reward container",
                "Matriarch spawn routes",
                "Wait for Matriarch, defeat the boss, and secure the core immediately.");

            return;
        }

        if (name.Contains("dolabra"))
        {
            Set(
                plan,
                "Legendary / Event",
                75,
                "Any valid Close Scrutiny map",
                "Close Scrutiny",
                "ARC Assessor containers",
                "Assessor encounter areas",
                "Enter Close Scrutiny and breach or loot Assessors after clearing the area.");

            return;
        }

        if (name.Contains("canto")
            || name.Contains("bobcat"))
        {
            Set(
                plan,
                "Epic / Hurricane Cache",
                72,
                "Any valid Hurricane map",
                "Hurricane",
                "Hurricane Chests / First Wave Caches",
                "Hurricane Chest / First Wave Cache routes",
                "Track Hurricane, enter early, and hit Hurricane Chests / First Wave Caches before other raiders clear them.");

            return;
        }

        if (name.Contains("wolfpack")
            || name.Contains("tempest"))
        {
            Set(
                plan,
                "Epic / Night Route",
                68,
                "Dam Battlegrounds or Blue Gate",
                "Night Raid",
                "Raider containers and weapon cases",
                "Residential and raider-cache dense routes",
                "Queue Night Raid and focus Raider Caches, Weapon Cases, and Ammo Crates.");

            return;
        }

        if (name.Contains("vulcano"))
        {
            Set(
                plan,
                "Epic / Bunker Route",
                66,
                "Blue Gate",
                "Hidden Bunker",
                "Breachable containers",
                "Bunker and underground breach rooms",
                "Farm Hidden Bunker and bring close-range tools for interior fights.");

            return;
        }

        if (name.Contains("equalizer"))
        {
            Set(
                plan,
                "Legendary / Harvester Event",
                78,
                "Any valid Harvester map",
                "Harvester",
                "Event reward containers",
                "Harvester objective area",
                "Only chase this when Harvester is active and bring ARC-damage tools.");

            return;
        }

        if (name.Contains("anvil")
            || name.Contains("bettina")
            || name.Contains("medium gun parts"))
        {
            Set(
                plan,
                "Weapon Pool",
                64,
                "Dam Battlegrounds",
                "Uncovered Caches",
                "Raider containers - Raider Caches, Weapon Cases, Ammo Crates",
                "Dam cache routes and raider backpack clusters",
                "Farm Uncovered Caches first; if unavailable, loot every Raider Backpack, Ammo Crate, and Weapon Case.");

            return;
        }
    }

    private static void ApplyCategoryFallbacks(
        Blueprint blueprint,
        BlueprintFarmPlanViewModel plan)
    {
        if (!string.IsNullOrWhiteSpace(plan.BestMap))
        {
            return;
        }

        var name = blueprint.Name.ToLowerInvariant();
        var category = blueprint.Category.ToLowerInvariant();

        if (category.Contains("mod")
            && (name.Contains(" ii")
                || name.EndsWith("i")
                || name.Contains("silencer i")))
        {
            Set(
                plan,
                "Tier 2 Attachment",
                58,
                "Blue Gate or Buried City",
                "Standard Patrol",
                "Residential containers",
                "Blue Gate Village, Buried City apartments, Dam residences",
                "Run normal maps and search wardrobes, cupboards, suitcases, desks, and trash cans.");

            return;
        }

        if (category.Contains("mod")
            || name.Contains(" iii")
            || name.Contains("padded stock")
            || name.Contains("extended barrel"))
        {
            Set(
                plan,
                "Tier 3 Attachment",
                61,
                "Blue Gate or Dam Battlegrounds",
                "Night Raid",
                "Residential containers and high-value breach rooms",
                "Blue Gate Village, Dam Control Tower, high-value rooms",
                "Prioritize higher-value conditions and hit dense residential or breachable high-value containers.");

            return;
        }

        if (category.Contains("weapon")
            || category.Contains("ammo")
            || category.Contains("material"))
        {
            Set(
                plan,
                "Weapon / Raider Pool",
                57,
                "Dam Battlegrounds",
                "Uncovered Caches",
                "Raider containers",
                "Raider Caches, Weapon Cases, Ammo Crates, Raider Backpacks",
                "Farm Uncovered Caches when active, otherwise chain raider-cache and weapon-case locations.");

            return;
        }

        if (category.Contains("augment"))
        {
            Set(
                plan,
                "Rare Augment",
                54,
                "Spaceport or Blue Gate",
                "Locked Gate",
                "Electrical and augment-style containers",
                "Admin buildings, tech centers, server rooms, breachable wall cabinets",
                "Farm tech-heavy interiors and electrical containers during higher-value conditions.");

            return;
        }

        if (category.Contains("grenade")
            || category.Contains("mine"))
        {
            Set(
                plan,
                "Utility Explosive",
                52,
                "Blue Gate or Dam Battlegrounds",
                "Electromagnetic Storm",
                "Industrial containers",
                "Maintenance Wing, Primary Facility, red lockers, breachable raider boxes",
                "Loot industrial containers during a major condition and avoid long open rotations.");

            return;
        }

        if (category.Contains("quick"))
        {
            if (name.Contains("vita")
                || name.Contains("defibrillator")
                || name.Contains("bandage"))
            {
                Set(
                    plan,
                    "Medical Utility",
                    50,
                    "Buried City or Stella Montis",
                    "Night Raid",
                    "Medical containers",
                    "Hospitals, pharmacies, medical research areas, white pull-out drawers",
                    "Target medical containers during Night Raid or other higher-value conditions.");

                return;
            }

            if (name.Contains("light stick"))
            {
                Set(
                    plan,
                    "Low-tier Utility",
                    55,
                    "Blue Gate",
                    "Standard Patrol",
                    "Generic, residential, and augment-style containers",
                    "Blue Gate generic lockers and residential routes",
                    "Do not force major events for light sticks; run low-risk standard routes.");

                return;
            }

            Set(
                plan,
                "Utility Item",
                48,
                "Spaceport or Blue Gate",
                "Standard Patrol",
                "Electrical, residential, and utility containers",
                "Admin buildings, residential rooms, and utility cabinets",
                "Search common utility containers and treat it as a low-risk farming target.");

            return;
        }

        Set(
            plan,
            "General Blueprint",
            45,
            "Dam Battlegrounds",
            "Standard Patrol",
            "Mixed loot containers",
            "General high-density loot route",
            "Use standard routes and open every relevant container type you pass.");
    }

    private static void ApplyConditionRules(BlueprintFarmPlanViewModel plan)
    {
        plan.ValidMaps.Clear();
        plan.RequiredConditions.Clear();
        plan.OptimalConditions.Clear();

        var allStandardMaps = new[]
        {
            "Dam Battlegrounds",
            "Buried City",
            "Riven Tides",
            "Spaceport",
            "Blue Gate",
            "Stella Montis"
        };

        var eventMaps = new[]
        {
            "Dam Battlegrounds",
            "Buried City",
            "Riven Tides",
            "Spaceport",
            "Blue Gate"
        };

        if (plan.BestMap.Contains(
            "Stella Montis",
            StringComparison.OrdinalIgnoreCase))
        {
            plan.ValidMaps.Add("Stella Montis");

            plan.RequiredConditions.AddRange(
                new[]
                {
                    "Standard Patrol",
                    "Night Raid"
                });

            plan.OptimalConditions.Add("Night Raid");

            plan.BestCondition =
                plan.BestCondition == "Standard Patrol"
                    ? "Standard Patrol"
                    : "Night Raid";

            plan.ConditionNote =
                "Stella Montis is limited in this project to Standard Patrol and Night Raid, so major map conditions are not recommended there.";

            return;
        }

        if (plan.BestCondition == "Quest completion")
        {
            plan.ValidMaps.Add("Quest dependent");
            plan.RequiredConditions.Add("Quest completion");
            plan.OptimalConditions.Add("Follow quest objective");

            plan.ConditionNote =
                "This blueprint is handled as a quest reward, so the quest objective matters more than map conditions.";

            return;
        }

        if (plan.BestCondition == "Matriarch")
        {
            plan.ValidMaps.AddRange(
                new[]
                {
                    "Dam Battlegrounds",
                    "Spaceport",
                    "Blue Gate"
                });

            plan.RequiredConditions.Add("Matriarch");
            plan.OptimalConditions.Add("Matriarch");

            plan.ConditionNote =
                "Matriarch is treated as a boss/event route and is not offered on Stella Montis.";

            return;
        }

        if (plan.BestCondition == "Standard Patrol")
        {
            plan.ValidMaps.AddRange(allStandardMaps);

            plan.OptimalConditions.AddRange(
                new[]
                {
                    "Standard Patrol",
                    "Night Raid"
                });

            plan.ConditionNote =
                "No condition is required. Standard Patrol keeps the run safer, while Night Raid can improve the route if the player accepts more risk.";

            return;
        }

        if (plan.BestCondition == "Night Raid")
        {
            plan.ValidMaps.AddRange(allStandardMaps);

            plan.OptimalConditions.AddRange(
                new[]
                {
                    "Night Raid",
                    "Locked Gate",
                    "Electromagnetic Storm"
                });

            plan.ConditionNote =
                "Night Raid is the preferred condition, but Locked Gate or Electromagnetic Storm can be acceptable substitutes for higher-tier loot routes.";

            return;
        }

        if (plan.BestCondition == "Hurricane")
        {
            plan.ValidMaps.AddRange(eventMaps);
            plan.RequiredConditions.Add("Hurricane");
            plan.OptimalConditions.Add("Hurricane");

            plan.ConditionNote =
                "This blueprint plan depends on Hurricane Chest / First Wave Cache routing. Do not recommend Stella Montis for this route because Stella Montis is not offered as a Hurricane map in this planner.";

            return;
        }

        if (plan.BestCondition == "Close Scrutiny")
        {
            plan.ValidMaps.AddRange(eventMaps);
            plan.RequiredConditions.Add("Close Scrutiny");
            plan.OptimalConditions.Add("Close Scrutiny");

            plan.ConditionNote =
                "This is an event-gated plan. The player should wait for Close Scrutiny on a valid outdoor map.";

            return;
        }

        if (plan.BestCondition == "Hidden Bunker")
        {
            plan.ValidMaps.Add("Blue Gate");
            plan.RequiredConditions.Add("Hidden Bunker");
            plan.OptimalConditions.Add("Hidden Bunker");

            plan.ConditionNote =
                "Hidden Bunker is treated as a Blue Gate route in this project. It should not be offered on Stella Montis.";

            return;
        }

        if (plan.BestCondition == "Harvester")
        {
            plan.ValidMaps.AddRange(eventMaps);
            plan.RequiredConditions.Add("Harvester");
            plan.OptimalConditions.Add("Harvester");

            plan.ConditionNote =
                "Harvester is an event route. Use this only when Harvester is active on a valid map.";

            return;
        }

        if (plan.BestCondition == "Locked Gate")
        {
            plan.ValidMaps.AddRange(eventMaps);
            plan.RequiredConditions.Add("Locked Gate");

            plan.OptimalConditions.AddRange(
                new[]
                {
                    "Locked Gate",
                    "Night Raid"
                });

            plan.ConditionNote =
                "Locked Gate is required for this route because the plan depends on high-value gated or breachable loot paths.";

            return;
        }

        if (plan.BestCondition == "Electromagnetic Storm")
        {
            plan.ValidMaps.AddRange(eventMaps);

            plan.OptimalConditions.AddRange(
                new[]
                {
                    "Electromagnetic Storm",
                    "Locked Gate",
                    "Night Raid"
                });

            plan.ConditionNote =
                "Electromagnetic Storm is optimal for tech and utility farming, with Locked Gate and Night Raid as backup higher-risk routes.";

            return;
        }

        if (plan.BestCondition == "Uncovered Caches")
        {
            plan.ValidMaps.AddRange(eventMaps);

            plan.OptimalConditions.AddRange(
                new[]
                {
                    "Uncovered Caches",
                    "Night Raid",
                    "Locked Gate"
                });

            plan.ConditionNote =
                "Uncovered Caches is a strong modifier for weapon and raider-cache farming, but Night Raid or Locked Gate can still be used if caches are not active.";

            return;
        }

        plan.ValidMaps.AddRange(eventMaps);
        plan.OptimalConditions.Add(plan.BestCondition);

        plan.ConditionNote =
            "Use the listed map condition when available, or fall back to Night Raid for a higher-risk general farming route.";
    }

    private static void ApplyPlaystyleLoadout(BlueprintFarmPlanViewModel plan)
    {
        if (plan.Playstyle == "PvE")
        {
            plan.LoadoutSummary =
                "Primary: Bettina or Renegade | Secondary: Burletta | Shield: Medium Shield | Augment: Looting Mk. 3 or Tactical Mk. 2";

            plan.QuickUseItems.AddRange(
                new[]
                {
                    "Smoke Grenade",
                    "Shield Recharger",
                    "Sterilized Bandage",
                    "Adrenaline Shot"
                });

            if (plan.BestCondition is "Harvester"
                or "Close Scrutiny"
                or "Matriarch")
            {
                AddUnique(plan, "Seeker Grenade");
            }
        }
        else if (plan.Playstyle == "PvP")
        {
            plan.LoadoutSummary =
                "Primary: Tempest or Rattler | Secondary: Il Toro or Vulcano | Shield: Heavy Shield | Augment: Combat Mk. 3 or Tactical Mk. 3";

            plan.QuickUseItems.AddRange(
                new[]
                {
                    "Smoke Grenade",
                    "Surge Shield Recharger",
                    "Vita Shot",
                    "Tagging Grenade"
                });

            if (plan.BestCondition is "Hidden Bunker"
                or "Locked Gate")
            {
                AddUnique(plan, "Door Blocker");
            }
        }
        else
        {
            plan.LoadoutSummary =
                "Primary: Rattler | Secondary: Venator | Shield: Medium Shield | Augment: Tactical Mk. 2";

            plan.QuickUseItems.AddRange(
                new[]
                {
                    "Smoke Grenade",
                    "Shield Recharger",
                    "Herbal Bandage",
                    "Adrenaline Shot"
                });
        }

        if (plan.BestCondition == "Hurricane")
        {
            AddUnique(plan, "Zipline");
        }

        if (plan.BestCondition == "Night Raid")
        {
            AddUnique(plan, "Green Light Stick");
        }

        if (plan.BestCondition == "Electromagnetic Storm")
        {
            AddUnique(plan, "Snap Hook");
        }

        if (plan.ContainerType.Contains(
            "Breachable",
            StringComparison.OrdinalIgnoreCase))
        {
            AddUnique(plan, "Barricade Kit");
        }
    }

    private static void BuildExplanation(BlueprintFarmPlanViewModel plan)
    {
        plan.ProbabilityLabel =
            plan.ProbabilityWeight >= 90
                ? "Quest or guaranteed reward"
                : plan.ProbabilityWeight >= 70
                    ? "Very high relative priority"
                    : plan.ProbabilityWeight >= 60
                        ? "High relative priority"
                        : plan.ProbabilityWeight >= 50
                            ? "Medium relative priority"
                            : "Low but reasonable relative priority";

        var required =
            plan.RequiredConditions.Count == 0
                ? "No hard condition is required"
                : "Required condition: "
                    + string.Join(", ", plan.RequiredConditions);

        var optimal =
            plan.OptimalConditions.Count == 0
                ? plan.BestCondition
                : string.Join(", ", plan.OptimalConditions);

        var maps =
            plan.ValidMaps.Count == 0
                ? plan.BestMap
                : string.Join(", ", plan.ValidMaps);

        plan.WhyThisPlan =
            $"This plan matches {plan.BlueprintName} to the most useful loot pool: {plan.ContainerType}. "
            + $"Run {plan.BestMap} during {plan.BestCondition} when possible. {required}. "
            + $"Optimal condition choices are {optimal}. Valid maps are {maps}. "
            + "The weight is a relative farming score, not a guaranteed drop percentage.";
    }

    private static void Set(
        BlueprintFarmPlanViewModel plan,
        string rarity,
        int weight,
        string map,
        string condition,
        string container,
        string areas,
        string route)
    {
        plan.RarityTier = rarity;
        plan.ProbabilityWeight = weight;
        plan.BestMap = map;
        plan.BestCondition = condition;
        plan.ContainerType = container;
        plan.BestAreas = areas;
        plan.FarmingRoute = route;
    }

    private static void AddUnique(
        BlueprintFarmPlanViewModel plan,
        string item)
    {
        if (!plan.QuickUseItems.Contains(
                item,
                StringComparer.OrdinalIgnoreCase))
        {
            plan.QuickUseItems.Add(item);
        }
    }

    public List<BlueprintConditionAlertViewModel> BuildConditionAlerts(
        IEnumerable<Blueprint> blueprints,
        string activeMap,
        string activeCondition,
        string playstyle)
    {
        var alerts = new List<BlueprintConditionAlertViewModel>();

        foreach (var blueprint in blueprints)
        {
            if (blueprint.Collected
                || string.Equals(
                    blueprint.CollectionStatus,
                    "Collected",
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    blueprint.CollectionStatus,
                    "Obtained",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var plan = BuildFarmPlan(blueprint, playstyle);

            if (!IsMapAllowed(plan, activeMap))
            {
                continue;
            }

            var requiredMatch =
                plan.RequiredConditions.Any(
                    x => IsSameCondition(x, activeCondition));

            var optimalMatch =
                plan.OptimalConditions.Any(
                    x => IsSameCondition(x, activeCondition));

            var bestMatch =
                IsSameCondition(
                    plan.BestCondition,
                    activeCondition);

            var hasMatch =
                requiredMatch
                || optimalMatch
                || bestMatch;

            if (!hasMatch)
            {
                continue;
            }

            alerts.Add(new BlueprintConditionAlertViewModel
            {
                BlueprintId = blueprint.Id,
                BlueprintName = blueprint.Name,
                Category = blueprint.Category,
                ActiveMap = activeMap,
                ActiveCondition = activeCondition,

                MatchType =
                    requiredMatch
                        ? "Required condition active"
                        : optimalMatch
                            ? "Optimal condition active"
                            : "Recommended condition active",

                ContainerType = plan.ContainerType,
                BestAreas = plan.BestAreas,
                FarmingRoute = plan.FarmingRoute,
                LoadoutSummary = plan.LoadoutSummary,

                PriorityScore =
                    plan.ProbabilityWeight
                    + (requiredMatch
                        ? 20
                        : optimalMatch
                            ? 10
                            : 5)
            });
        }

        return alerts
            .OrderByDescending(x => x.PriorityScore)
            .ThenBy(x => x.BlueprintName)
            .Take(8)
            .ToList();
    }

    private static bool IsMapAllowed(
        BlueprintFarmPlanViewModel plan,
        string activeMap)
    {
        if (plan.ValidMaps.Count == 0)
        {
            return plan.BestMap.Contains(
                       activeMap,
                       StringComparison.OrdinalIgnoreCase)
                   || activeMap.Contains(
                       plan.BestMap,
                       StringComparison.OrdinalIgnoreCase);
        }

        return plan.ValidMaps.Any(
            map => IsSameMap(map, activeMap));
    }

    private static bool IsSameMap(
        string plannedMap,
        string activeMap)
    {
        var planned = NormalizeMap(plannedMap);
        var active = NormalizeMap(activeMap);

        if (planned == "quest dependent")
        {
            return false;
        }

        if (planned == "any valid close scrutiny map"
            && active != "stella montis")
        {
            return true;
        }

        if (planned == "any valid hurricane map"
            && active != "stella montis")
        {
            return true;
        }

        if (planned == "any valid harvester map"
            && active != "stella montis")
        {
            return true;
        }

        return planned == active
               || planned.Contains(active)
               || active.Contains(planned);
    }

    private static string NormalizeMap(string value)
    {
        return value
            .Replace(
                "The ",
                "",
                StringComparison.OrdinalIgnoreCase)
            .Trim()
            .ToLowerInvariant();
    }

    private static bool IsSameCondition(
        string plannedCondition,
        string activeCondition)
    {
        var planned =
            plannedCondition
                .Trim()
                .ToLowerInvariant();

        var active =
            activeCondition
                .Trim()
                .ToLowerInvariant();

        if (planned == "follow quest objective"
            || planned == "quest completion")
        {
            return false;
        }

        if (planned == "closed gate")
        {
            planned = "locked gate";
        }

        if (active == "closed gate")
        {
            active = "locked gate";
        }

        return planned == active;
    }
}