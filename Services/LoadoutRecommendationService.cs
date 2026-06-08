using RaidersVault.ViewModels;

namespace RaidersVault.Services;

public class LoadoutRecommendationService
{
    public OptimalLoadoutViewModel Build(
        string map,
        string condition,
        string playstyle,
        int totalSkillPoints)
    {
        totalSkillPoints = Math.Max(0, totalSkillPoints);

        var loadout = new OptimalLoadoutViewModel
        {
            Map = map,
            Condition = condition,
            Playstyle = playstyle,
            TotalSkillPoints = totalSkillPoints
        };

        ApplyBaseKit(loadout);
        ApplyConditionRules(loadout);
        ApplyMapRules(loadout);
        ApplySkillSynergy(loadout);
        ScoreLoadout(loadout);

        return loadout;
    }

    private static void ApplyBaseKit(OptimalLoadoutViewModel l)
    {
        if (l.Playstyle == "PvE")
        {
            l.PrimaryWeapon = "Renegade";
            l.SecondaryWeapon = "Burletta";
            l.Shield = "Medium Shield";
            l.Augment = "Looting Mk. 3 (Safekeeper)";

            l.QuickUseItems.AddRange(new[]
            {
                "Sterilized Bandage",
                "Shield Recharger",
                "Adrenaline Shot",
                "Lure Grenade",
                "Smoke Grenade"
            });

            l.WhyThisLoadout.Add(
                "PvE favors safe ARC clearing, recovery, and carrying objective loot without forcing unnecessary raider fights.");
        }
        else if (l.Playstyle == "PvP")
        {
            l.PrimaryWeapon = "Tempest";
            l.SecondaryWeapon = "Il Toro";
            l.Shield = "Heavy Shield";
            l.Augment = "Combat Mk. 3 (Aggressive)";

            l.QuickUseItems.AddRange(new[]
            {
                "Vita Shot",
                "Surge Shield Recharger",
                "Adrenaline Shot",
                "Smoke Grenade",
                "Showstopper",
                "Tagging Grenade"
            });

            l.WhyThisLoadout.Add(
                "PvP favors burst pressure, fast reset tools, tracking utility, and stronger shielding for player fights.");
        }
        else
        {
            l.PrimaryWeapon = "Rattler";
            l.SecondaryWeapon = "Venator";
            l.Shield = "Medium Shield";
            l.Augment = "Tactical Mk. 2";

            l.QuickUseItems.AddRange(new[]
            {
                "Herbal Bandage",
                "Shield Recharger",
                "Adrenaline Shot",
                "Smoke Grenade",
                "Light Impact Grenade"
            });

            l.WhyThisLoadout.Add(
                "Balanced entry keeps a flexible mid-range weapon, a reliable sidearm, recovery, and enough utility to fight or disengage.");
        }
    }

    private static void ApplyConditionRules(OptimalLoadoutViewModel l)
    {
        switch (l.Condition)
        {
            case "Standard":
            case "Standard Patrol":
                l.WhyThisLoadout.Add(
                    "Standard Patrol does not require a special-condition kit, so the recommendation stays flexible and cost-aware.");
                break;

            case "Night":
            case "Night Raid":
                l.SecondaryWeapon = l.Playstyle == "PvP" ? "Vulcano" : "Venator";
                AddQuick(l, "Green Light Stick", "Tagging Grenade", "Photoelectric Cloak");
                l.WhyThisLoadout.Add(
                    "Night Raid increases ambush risk, so the kit adds tracking, visibility, and either close-range control or a reliable sidearm.");
                break;

            case "Electromagnetic Storm":
                l.PrimaryWeapon = l.Playstyle == "PvE" ? "Bettina" : "Tempest";
                AddQuick(l, "Surge Shield Recharger", "Adrenaline Shot", "Snap Hook");
                l.WhyThisLoadout.Add(
                    "Storm runs need durable weapons, emergency shield recovery, and movement tools because rotations are less predictable.");
                break;

            case "Hurricane":
                l.Shield = l.Playstyle == "PvP" ? "Heavy Shield" : "Medium Shield";
                AddQuick(l, "Zipline", "Adrenaline Shot", "Sterilized Bandage");
                l.WhyThisLoadout.Add(
                    "Hurricane conditions reward stamina and reposition tools, so the kit adds movement and extra recovery.");
                l.WhyThisLoadout.Add(
                    "Hurricane also enables First Wave / Hurricane Chest routes, so the kit favors fast cache checks over long fights.");
                break;

            case "Cold Snap":
                l.Shield = l.Playstyle == "PvP" ? "Heavy Shield" : "Medium Shield";
                AddQuick(l, "Vita Shot", "Sterilized Bandage", "Adrenaline Shot", "Smoke Grenade");
                l.WhyThisLoadout.Add(
                    "Cold Snap favors recovery and safer rotations because cold exposure and reduced visibility punish long outdoor routes.");
                break;

            case "Close Scrutiny":
                AddQuick(l, "Smoke Grenade", "Noisemaker", "Tagging Grenade");
                l.WhyThisLoadout.Add(
                    "Close Scrutiny should be treated like a watched route; smoke and sensors help avoid getting pinned.");
                break;

            case "Hidden Bunker":
            case "Locked Gate":
                l.SecondaryWeapon = l.Playstyle == "PvE" ? "Il Toro" : l.SecondaryWeapon;
                AddQuick(l, "Door Blocker", "Barricade Kit", "Smoke Grenade");
                l.WhyThisLoadout.Add(
                    "Interior objective conditions benefit from door control, deployable cover, and a close-range backup.");
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
                l.Augment = l.Playstyle == "PvP"
                    ? l.Augment
                    : "Looting Mk. 3 (Safekeeper)";

                AddQuick(l, "Smoke Grenade", "Zipline", "Adrenaline Shot");
                l.WhyThisLoadout.Add(
                    "Loot-heavy conditions attract other players, so this kit keeps escape utility while preserving inventory value.");
                break;

            case "Matriarch":
            case "Harvester":
                l.PrimaryWeapon = l.Playstyle == "PvP" ? "Tempest" : "Bettina";
                l.SecondaryWeapon = l.Playstyle == "PvE" ? "Hullcracker" : l.SecondaryWeapon;
                AddQuick(l, "Seeker Grenade", "Wolfpack", "Shield Recharger", "Vita Shot");
                l.WhyThisLoadout.Add(
                    "Major ARC event conditions need higher damage, extra ammo planning, healing, and shield sustain.");
                break;
        }
    }

    private static void ApplyMapRules(OptimalLoadoutViewModel l)
    {
        switch (l.Map)
        {
            case "Buried City":
                AddQuick(l, "Barricade Kit", "Smoke Grenade");
                l.WhyThisLoadout.Add(
                    "Buried City creates sudden close-to-mid range fights, so the kit adds cover and smoke for street crossings.");
                break;

            case "Spaceport":
                l.PrimaryWeapon = l.Playstyle == "PvP" ? "Tempest" : l.PrimaryWeapon;
                AddQuick(l, "Binoculars", "Smoke Grenade");
                l.WhyThisLoadout.Add(
                    "Spaceport has longer approaches and large structures, so scouting and smoke help with safer movement.");
                break;

            case "Riven Tides":
                l.PrimaryWeapon = l.Playstyle == "PvP" ? "Tempest" : l.PrimaryWeapon;
                AddQuick(l, "Dockmaster's Detector", "Smoke Grenade", "Zipline");
                l.WhyThisLoadout.Add(
                    "Riven Tides benefits from beachcombing utility, fast dockyard movement, and smoke for exposed coastal rotations.");
                break;

            case "The Blue Gate":
                AddQuick(l, "Zipline", "Adrenaline Shot");
                l.WhyThisLoadout.Add(
                    "The Blue Gate benefits from mobility tools because longer rotations can punish slow kits.");
                break;

            case "Stella Montis":
                AddQuick(l, "Snap Hook", "Tagging Grenade");
                l.WhyThisLoadout.Add(
                    "Stella Montis is handled cautiously with extra reposition and information tools.");
                break;

            default:
                AddQuick(l, "Lure Grenade");
                l.WhyThisLoadout.Add(
                    "Dam Battlegrounds works well with a general kit that can handle materials, ARC pressure, and basic player contact.");
                break;
        }
    }

    private static void ApplySkillSynergy(OptimalLoadoutViewModel l)
    {
        var tier = l.TotalSkillPoints switch
        {
            >= 36 => "late",
            >= 15 => "mid",
            _ => "early"
        };

        if (l.Playstyle == "PvE")
        {
            l.SkillSynergies.Add("Survival focus: healing, safe extraction, and carrying quest loot.");
            l.SkillSynergies.Add("Conditioning focus: stamina and weight management for longer rotations.");
        }
        else if (l.Playstyle == "PvP")
        {
            l.SkillSynergies.Add("Mobility focus: sprinting, repositioning, and fight resets.");
            l.SkillSynergies.Add("Conditioning focus: stamina sustain for chasing or escaping raiders.");
        }
        else
        {
            l.SkillSynergies.Add("Balanced focus: split early points between survival consistency and mobility.");
            l.SkillSynergies.Add("Do not overcommit to a late branch until the current point total supports it.");
        }

        if (tier == "late")
        {
            l.SkillSynergies.Add("At 36+ points, the recommendation can support late-branch skills and heavier gear choices.");
        }
        else if (tier == "mid")
        {
            l.SkillSynergies.Add("At 15+ points, mid-branch skills are reasonable, but avoid assuming late unlocks.");
        }
        else
        {
            l.SkillSynergies.Add("Below 15 points, the loadout stays conservative because the skill tree cannot fully support risky gear yet.");

            if (l.Shield == "Heavy Shield")
            {
                l.Shield = "Medium Shield";
            }

            if (l.Augment.Contains("Mk. 3"))
            {
                l.Augment = l.Playstyle == "PvP" ? "Combat Mk. 2" : "Tactical Mk. 2";
            }
        }
    }

    private static void ScoreLoadout(OptimalLoadoutViewModel l)
    {
        var score = 60;

        if (l.TotalSkillPoints >= 15)
        {
            score += 10;
        }

        if (l.TotalSkillPoints >= 36)
        {
            score += 10;
        }

        if (l.QuickUseItems.Contains("Smoke Grenade"))
        {
            score += 5;
        }

        if (l.QuickUseItems.Any(x => x.Contains("Shield Recharger")))
        {
            score += 5;
        }

        if (l.QuickUseItems.Contains("Adrenaline Shot"))
        {
            score += 5;
        }

        if ((l.Condition is "Night" or "Night Raid" or "Close Scrutiny")
            && l.QuickUseItems.Contains("Tagging Grenade"))
        {
            score += 5;
        }

        if ((l.Condition is "Cold Snap" or "Hurricane")
            && l.QuickUseItems.Contains("Adrenaline Shot"))
        {
            score += 5;
        }

        if ((l.Condition is "Matriarch" or "Harvester")
            && (l.QuickUseItems.Contains("Seeker Grenade")
                || l.QuickUseItems.Contains("Wolfpack")))
        {
            score += 5;
        }

        l.Score = Math.Min(score, 100);

        l.Caution = l.Score >= 90
            ? "Strong match for the selected condition and point total."
            : l.Score >= 75
                ? "Good match, but adjust if your stash cannot support the listed gear."
                : "Conservative recommendation. Add more skill points or better gear before forcing fights.";
    }

    private static void AddQuick(
        OptimalLoadoutViewModel l,
        params string[] items)
    {
        foreach (var item in items)
        {
            if (!l.QuickUseItems.Contains(item, StringComparer.OrdinalIgnoreCase))
            {
                l.QuickUseItems.Add(item);
            }
        }
    }
}