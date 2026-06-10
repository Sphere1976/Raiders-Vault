using RaidersVault.Data;
using RaidersVault.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RaidersVault.Services;

public static class DbInitializer
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public static void Seed(RaidersVaultContext db)
    {
        if (!db.UserAccounts.Any())
        {
            db.UserAccounts.Add(new UserAccount
            {
                Username = "admin",
                PasswordHash = HashPassword("password")
            });
        }

        if (!db.PlayerProfiles.Any())
        {
            db.PlayerProfiles.Add(new PlayerProfile
            {
                PlayerName = "Raider",
                PreferredPlaystyle = "Balanced",
                DefaultMap = "Dam Battlegrounds",
                CurrentSkillPoints = 20,
                Notes = "Default planning profile for demo and evaluation."
            });
        }

        if (!db.Loadouts.Any())
        {
            db.Loadouts.AddRange(
                new Loadout
                {
                    Name = "Silent Scavenger",
                    ActivityType = "Map Run",
                    MapOrEvent = "Buried City / Night Raid",
                    FocusArea = "PvE stealth looting",
                    RiskLevel = "Low",
                    GearNotes = "Primary: suppressed rifle; Secondary: pistol; Shield: light shield; Augment: loot or mobility augment; Quick Use: medkit, smoke grenade, shield battery, zipline.",
                    Notes = "Built for quiet blueprint and materials runs. Avoid long fights, loot residential containers, and extract once the quest item or blueprint target is found."
                },
                new Loadout
                {
                    Name = "ARC Breaker",
                    ActivityType = "Event",
                    MapOrEvent = "Dam Battlegrounds / High ARC Activity",
                    FocusArea = "PvE ARC clear",
                    RiskLevel = "Medium",
                    GearNotes = "Primary: rifle or DMR; Secondary: SMG; Shield: medium shield; Augment: survival or ammo augment; Quick Use: medkit, shield battery, EMP grenade, explosive grenade.",
                    Notes = "Use this when quests require ARC parts, ARC kills, or machine-heavy paths. Keep EMP/explosives for pressure relief instead of opening every fight with them."
                },
                new Loadout
                {
                    Name = "Locked Gate Farmer",
                    ActivityType = "Blueprint Farming",
                    MapOrEvent = "Blue Gate / Locked Gate",
                    FocusArea = "Rare containers and weapon blueprints",
                    RiskLevel = "High",
                    GearNotes = "Primary: assault rifle; Secondary: shotgun; Shield: heavy shield; Augment: loot or combat augment; Quick Use: medkit, shield battery, smoke grenade, breach tool, grenade.",
                    Notes = "Designed for high-value locked routes. Push the target containers early, use smoke for exits, and do not overstay once the farm target is checked."
                },
                new Loadout
                {
                    Name = "Close Scrutiny Hunter",
                    ActivityType = "Event",
                    MapOrEvent = "Spaceport / Close Scrutiny",
                    FocusArea = "High-risk event drops",
                    RiskLevel = "High",
                    GearNotes = "Primary: battle rifle; Secondary: SMG; Shield: heavy shield; Augment: combat augment; Quick Use: medkit, shield battery, smoke grenade, EMP grenade, scan/sensor item.",
                    Notes = "For contesting rare event routes. Play slower than a normal PvP kit and only commit after checking sightlines and extraction distance."
                },
                new Loadout
                {
                    Name = "Balanced Squad Runner",
                    ActivityType = "Map Run",
                    MapOrEvent = "Any Map / Standard Patrol",
                    FocusArea = "Balanced objectives and looting",
                    RiskLevel = "Medium",
                    GearNotes = "Primary: assault rifle; Secondary: pistol or SMG; Shield: medium shield; Augment: balanced utility augment; Quick Use: medkit, shield battery, smoke grenade, stamina item.",
                    Notes = "General-purpose kit for mixed objectives, casual squad runs, and learning routes. Good default when the map condition is not worth building around."
                },
                new Loadout
                {
                    Name = "PvP Intercept Kit",
                    ActivityType = "Map Run",
                    MapOrEvent = "Buried City / High Traffic Routes",
                    FocusArea = "PvP pressure and player defense",
                    RiskLevel = "High",
                    GearNotes = "Primary: assault rifle; Secondary: shotgun; Shield: heavy shield; Augment: combat or mobility augment; Quick Use: medkit, shield battery, flash grenade, smoke grenade, explosive grenade.",
                    Notes = "Built for player contact around rooftops, extraction paths, and contested interiors. Keep one smoke for disengage instead of using every item aggressively."
                },
                new Loadout
                {
                    Name = "Stella Montis Safe Run",
                    ActivityType = "Map Run",
                    MapOrEvent = "Stella Montis / Standard or Night",
                    FocusArea = "Objective completion and indoor looting",
                    RiskLevel = "Medium",
                    GearNotes = "Primary: compact rifle; Secondary: SMG; Shield: medium shield; Augment: mobility or survival augment; Quick Use: medkit, shield battery, smoke grenade, stamina item.",
                    Notes = "Stella Montis has fewer condition options, so this kit focuses on flexible indoor movement and safe objective completion."
                }
            );
        }

        if (!db.Quests.Any())
        {
            db.Quests.AddRange(
                new Quest { Name = "01. Picking Up The Pieces", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Go to any POI with a loot-category icon, grab 3 containers. Best early spot is Hydroponic Dome Complex (Dam Battlegrounds). Tip: Hit 3 lockers fast, then rotate straight to extract. No hero fights." },
                new Quest { Name = "02. Clearer Skies", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Drop 3 ARC enemies, bring back 3 ARC Alloy for Shani on Dam Battlegrounds. Tip: Farm easy ARC near Dam’s mid lanes, loot alloy, leave. Alloy matters more than loot here." },
                new Quest { Name = "03. Trash Into Treasure", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Collect 6 Wires and 1 Battery in Research and Admin Building (Dam Battlegrounds). Tip: Check server racks, maintenance shelves, and side closets. Batteries like to hide low." },
                new Quest { Name = "04. Off The Radar", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: In one round, visit a Field Depot and fix the roof antenna. Works on Dam, Spaceport, Buried City depots. Tip: Roof is exposed. Smoke first, repair second. Then bail." },
                new Quest { Name = "05. A Bad Feeling", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Find and loot any ARC Probe or ARC Courier across Dam, Spaceport, Buried City. Tip: Probes are small and easy to miss. Scan the ground around open lanes and broken paths." },
                new Quest { Name = "06. The Right Tool", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Kill a Fireball, a Hornet, and a Turret on any of the main maps. Tip: Don’t force it in one raid. Take the kills as you get them, then extract." },
                new Quest { Name = "07. Hatch Repairs", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Fix the leaking hydraulic pipes at a Raider Hatch, then search nearby for the hatch key. Tip: The pipe interact points sit on the hatch frame edges. Do a slow circle, you’ll see it." },
                new Quest { Name = "08. Safe Passage", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Apollo. Objective: Kill 2 ARC enemies using any explosive grenade, easiest around Research and Administration (Dam Battlegrounds). Tip: Pull ARC into a doorway, grenade the choke. Do not stand in the open lane." },
                new Quest { Name = "09. Down To Earth", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: In one round, hit a Field Depot, deliver a Field Crate to a Field Depot, then claim the drop reward. Tip: Do depot first. Then call the drop. If you loot too long, you die tired." },
                new Quest { Name = "10. The Trifecta", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Kill Hornet, Snitch, Wasp, and bring back their parts for Shani. Best loop is Testing Annex building (Dam Battlegrounds). Tip: Don’t chase across the whole map. Stick to one ARC-dense lane and reset if it’s quiet." },
                new Quest { Name = "11. A Better Use", Status = "Not Started", Priority = "Medium", RelatedActivity = "Blue Gate", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Call in a Supply Drop from a Call Station, then loot it. Works on all maps including Blue Gate. Tip: Clear sightlines first. The drop noise pulls people like a beacon." },
                new Quest { Name = "12. What Goes Around", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Apollo. Objective: Kill any ARC enemy using a Fireball Burner, easiest around Research and Administration (Dam). Tip: Tag with the burner, then finish with the same weapon. Don’t let teammates steal the last hit." },
                new Quest { Name = "13. Sparks Fly", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Apollo. Objective: Kill a Hornet using Trigger ’Nade or Snap Blast. Tip: Hornets hover and stall. Toss the nade where it will pause, not where it is right now." },
                new Quest { Name = "14. Greasing Her Palms", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Visit the locked room in Water Treatment Control (Dam Battlegrounds), then scope the rocket thrusters outside Rocket Assembly (Spaceport), then check the barricaded area on Floor 6 of the Space Travel Building (Buried City, JKV logo). Tip: The thruster marker can be wrong. The actual thrusters are far north of it, about 300m." },
                new Quest { Name = "15. A First Foothold", Status = "Not Started", Priority = "Medium", RelatedActivity = "Blue Gate", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Stabilize the Ridgeline observation deck, enable comms by Olive Grove, rotate the church roof dishes north of Data Vault, nail roof plates near Trapper’s Glade. All on Blue Gate. Tip: Bring mobility. Ridgeline to Olive Grove is a long, exposed rotate if you walk it." },
                new Quest { Name = "16. Dormant Barons", Status = "Not Started", Priority = "Medium", RelatedActivity = "Blue Gate", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Loot a Baron husk and it’s on Spaceport, Dam Battlegrounds, The Blue Gate. Tip: You’re loud when looting. Clear the nearby brush first, then commit." },
                new Quest { Name = "17. Mixed Signals", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Kill an ARC Surveyor, grab 1 Surveyor Vault. Tip: Surveyors punish greed. Loot the vault and rotate out, don’t double back." },
                new Quest { Name = "18. What We Left Behind", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Loot 2 containers in the Raider Camp under the Parking Garage (Buried City), then search South Swamp Outpost (Dam), then search Bilguun’s Hideout by Container Storage (Spaceport). Tip: This is a route quest. Pick the safest spawn map, then finish the others in later raids." },
                new Quest { Name = "19. Doctor’s Orders", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Bring 2 Antiseptic, 1 Syringe, 1 Durable Cloth, 1 Great Mullein, mainly from Pharmacies (Buried City). Tip: Safe-pocket the Great Mullein. It’s the one people forget, then die with." },
                new Quest { Name = "20. Medical Merchandise", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Lance. Objective: Loot exam-room containers in Departure Building (Spaceport), loot hospital containers in Buried City, then loot the medical room in Research & Administration (Dam). Tip: Close doors behind you in Departure Building. You want the extra second." },
                new Quest { Name = "21. A Reveal in Ruins", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Lance. Objective: Find an ESR Analyzer in a Buried City pharmacy, then deliver it to Lance. Tip: Check counters first. Pharmacy spawns love counters and back desks." },
                new Quest { Name = "22. Broken Monument", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: At Scrapyard (Dam Battlegrounds), collect compass, tape, rations, deliver to Tian Wen. Tip: Sweep in a loop. Vehicles first, then cylinders, then the Raider camp." },
                new Quest { Name = "23. Marked for Death", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Reach Su Durante Warehouses (Outskirts, Buried City). Tip: Long lanes and angles. Smoke and tight corners win here." },
                new Quest { Name = "24. Straight Record", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: At Victory Ridge (Dam Battlegrounds), find the EMP trap, flip the three switches, shut it down. Tip: Memorize switch placement. You want zero wandering once you start." },
                new Quest { Name = "25. A Lay of the Land", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Go Jiangsu Warehouse (Spaceport), grab shipping notes, find scanners on upper floor of Control Tower A6, deliver LiDAR Scanners to Shani. Tip: Scanners are inside the tower offices, not on the roof railing." },
                new Quest { Name = "26. Market Correction", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Find the cache near Marano Station (Buried City). Tip: Approach from cover. That station gets watched." },
                new Quest { Name = "27. Keeping the Memory", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Reach the wreckage in Formicai Hills (Dam Battlegrounds). Tip: Use the ravine route. The hilltops get scoped." },
                new Quest { Name = "28. Reduced to Rubble", Status = "Not Started", Priority = "Medium", RelatedActivity = "Blue Gate", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Photo Collapsed Highway, travel through Broken Earth, inspect ARC wrecks, all in the Highway Collapse region (Blue Gate). Tip: Photo prompt is picky. Stand centerline, face the break, then snap." },
                new Quest { Name = "29. With a Trace", Status = "Not Started", Priority = "Medium", RelatedActivity = "Blue Gate", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Go to Barren Clearing (Blue Gate), investigate who dropped the ARC machines. Tip: Wide open ground. Short sprints, long scans." },
                new Quest { Name = "30. Eyes on the Prize", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Find the roof terrace south west of Southern Station (Old Town, Buried City), rewire the solar panel with 3 Wires. Tip: Bring wires in safe pocket. You don’t want to re-loot for a basic step." },
                new Quest { Name = "31. Echoes of Victory Ridge", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: At Victory Ridge, take plans from the hideout under the broken highway, deliver Major Aiva’s Patch to Celeste. Tip: The under-highway hideout is a magnet. Go early, leave early." },
                new Quest { Name = "32. Industrial Espionage", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Find Tian Wen’s weapon cache near the Gas Station (Outskirts), move the Burletta to the rival cache in Industrial Zone (Buried City). Tip: Treat the Burletta like a mission item. No extra looting detours." },
                new Quest { Name = "33. Unexpected Initiative", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Grandioso Apartments rooftop for Fertilizer, then Piazza Roma rooftop gardens for Water Pump, deliver both. Tip: Mobility wins. Snap hook or ziplines keep this from turning into a marathon." },
                new Quest { Name = "34. A Symbol of Unification", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: At Formicai Outpost (Dam), find the flag and raise it overlooking the red lake. Tip: Clear nearby ARCs first. The raise animation locks you in." },
                new Quest { Name = "35. Celeste’s Journals", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Grab journals from South Swamp Outpost and the north outpost overlooking Red Lakes, then turn in. Tip: North journal is near the overlook edges. South is usually inside the shack clutter." },
                new Quest { Name = "36. Back on Top", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Mark 4 landmarks across maps, Pattern House, white lookout tower (Warehouse Complex area), South Trench Tower, and the mural building in Buried Properties. Tip: Binoculars make this safe. Mark from range, skip the fight." },
                new Quest { Name = "37. The Major’s Footlocker", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Search Ruby Residences (northwest of The Dam) for Major Aiva’s mementos, deliver to Tian Wen. Tip: Ruby Residences is tight and echo-y. Slow down and listen." },
                new Quest { Name = "38. Out of the Shadows", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Kill a Rocketeer, loot a Rocketeer Driver, on Testing Annex Building (Dam). Tip: Don’t peek the open yard when it’s spooling rockets. Wait the burst, then punish." },
                new Quest { Name = "39. Eyes in the Sky", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Install LiDAR scanners at Dam Control Tower, Spaceport Communications Tower, and the Galleria sign (Buried City). Tip: Smoke your install. You’re a statue for a second." },
                new Quest { Name = "40. Our Presence Up There", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Pattern House (Dam), flip the power switch, install the antenna on the roof. Tip: Flip switch first. Then roof. If you reverse it, you waste time." },
                new Quest { Name = "41. Communication Hideout", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Red Tower (Old Town, Buried City), find battery cell, install it, power the generator, boot the terminal. Tip: Battery cells hide on floors and low shelves. Look down, not just at tables." },
                new Quest { Name = "42. After Rain Comes", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Fix flooded solar panels near Grandioso Apartments, needs 5 Wires and 2 Batteries. Tip: Pre-loot wires and batteries before you go there. Saves a whole raid." },
                new Quest { Name = "43. A Balanced Harvest", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Go Research & Administration (Dam), find Lab 1 upstairs above reception. Tip: Lab doors look similar. Follow signage and glass corridors." },
                new Quest { Name = "44. Untended Garden", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: In one round, hit Hydroponic Dome Complex (Dam) data archive, then upload at any Field Depot terminal. Tip: Dome first, depot second, extract third. That’s the whole plan." },
                new Quest { Name = "45. The Root of the Matter", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Go Research Building (Buried City), find the seed vault room with the “great view”, deliver seed sample. Tip: That room is usually upper-level with a big window line. Sweep corners." },
                new Quest { Name = "46. Water Troubles", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Find Flood Access Tunnel under Red Lake Balcony, sample the intake at Red Lake Berm (Dam). Tip: Approach from rocks, not the open berm. It’s safer and faster." },
                new Quest { Name = "47. Into the Fray", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Kill a Leaper, loot the Leaper Pulse Unit, around Research & Administration (Dam). Tip: Aim where it lands, not where it jumps from. Leapers punish panic sprays." },
                new Quest { Name = "48. Source of the Contamination", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Water Treatment Building (Dam), check Flood Spill Intake by the swamp, investigate suspicious objects. Tip: Pair this with Water Troubles, same danger zone. One trip, two wins." },
                new Quest { Name = "49. Switching the Supply", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Go under Spaceport, find tunnels below launch towers, turn the valve. Tip: The valve is easy to miss. Look for thick pipe junctions and a small interact prompt." },
                new Quest { Name = "50. A Warm Place to Rest", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Apollo. Objective: Abandoned Highway (Buried City), find camp, follow red markers, inspect the grave. Tip: Don’t skip markers. It can fail the chain and waste the raid." },
                new Quest { Name = "51. Prescriptions of the Past", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Lance. Objective: Departure Building (Spaceport), find the Medical Exam Room, search for records. Tip: Records often sit on desks, not crates. Check the obvious surfaces first." },
                new Quest { Name = "52. Power Out", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Electrical Substation south of Spaceport by the Checkpoint, carry fuse or battery back, flip the fuse box switch. Tip: Clear the route before you pick up the carry item. You move slow with it." },
                new Quest { Name = "53. Flickering Threat", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: North Complex Elevator (Dam), repair generator, use ventilation shaft, hit the power switch under the stairs. Tip: The switch is low and hidden. Crouch and scan under the stair landing." },
                new Quest { Name = "54. Bees!", Status = "Not Started", Priority = "Medium", RelatedActivity = "Blue Gate", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Olive Grove (Blue Gate), search bee hives around the grove. Tip: Hives blend into trees. Check mid-trunk height, not the canopy." },
                new Quest { Name = "55. Espresso", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Apollo. Objective: Salvage espresso machine parts for Apollo in Plaza Rosa (Buried City). Tip: Look inside cafe kitchens and back counters. Corners, always corners." },
                new Quest { Name = "56. Life of a Pharmacist", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Lance. Objective: At Arbusto Farmacia (Buried City), interact with 4 documentation points. Tip: Do a slow lap. Each prompt is a different spot, shelves, counter, wall, back area." },
                new Quest { Name = "57. Tribute to Toledo", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Get a Power Rod for Celeste, any map. Tip: If you ever see a Power Rod while looting, stash it. This quest hits a lot of people." },
                new Quest { Name = "58. Digging Up Dirt", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Santa Maria Houses (Old Town, Buried City), find the dead drop in the courtyard. Tip: Courtyard containers are sneaky. Check planters and low boxes near stairs." },
                new Quest { Name = "59. Turnabout", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: North Trench Tower (Spaceport), upload the blackmail files. Tip: Clear lower floors first. Tower fights snowball fast if you rush." },
                new Quest { Name = "60. Building a Library", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Apollo. Objective: Library (City Center, Buried City), find romance, detective, adventure books, deliver all 3. Tip: Books are on shelves and tables, not just loot crates. Check reading areas." },
                new Quest { Name = "61. A New Type of Plant", Status = "Not Started", Priority = "Medium", RelatedActivity = "Dam Battlegrounds", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Lance. Objective: Find the plant near Baron Husk (Old Battleground, Dam), deliver the toxic plant to Lance. Tip: The plant is ground-level near the husk, easy to walk past. Slow down and scan." },
                new Quest { Name = "62. Armored Transports", Status = "Not Started", Priority = "Medium", RelatedActivity = "Blue Gate", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Blue Gate Checkpoint, loot guard huts for the patrol key card, go Traffic Tunnel, unlock the armored patrol car rear door. Tip: Loot both huts. If you only hit one, you’ll get unlucky and waste time." },
                new Quest { Name = "63. Lost in Transmission", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: In one round, reach Control Tower A6 (Spaceport), make it to the top. Tip: Go early. Late raid, squads camp A6 like it’s a tax booth." },
                new Quest { Name = "64. Snap and Salvage", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Photo any rover in the Sandbox, check papers in Security Checkpoint by the Lobby, deliver Magnetron and Flow Controller. Tip: Rover photo counts even with partial view. Aim for the chassis and wheels." },
                new Quest { Name = "65. In My Image", Status = "Not Started", Priority = "Medium", RelatedActivity = "Stella Montis", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Lance. Objective: Deploy to Stella Montis, search 3 androids. Tip: Listen for servo steps and robotic voice lines. Androids give themselves away." },
                new Quest { Name = "66. Cold Storage", Status = "Not Started", Priority = "Medium", RelatedActivity = "Stella Montis", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: In one round, loot any J Kozma Ventures container in Stella Montis, then deliver the rare books to Shani. Tip: JKV containers are the branded crates in freight lanes. Hit them first, then get out." },
                new Quest { Name = "67. A Toxic Trail", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Go back to the water intake below Water Treatment Control, search the swamp for barrel clues, photo the barrel truck, check the truck for info. Tip: Intake first, swamp second. Swamp gets messy later in the raid." },
                new Quest { Name = "68. The Stench of Corruption", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Grab the Flushing Terminal Key from Departure Building lockers, then use it in the Spaceport tunnel to override the bypass protocol. Tip: Do lockers in a quick pass, then commit to the tunnel. Don’t do tunnel first." },
                new Quest { Name = "69. The Clean Dream", Status = "Not Started", Priority = "Medium", RelatedActivity = "Blue Gate", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Spaceport tunnels container sweep, monitor filtration system, then Maintenance Bunker (Blue Gate) to monitor purification, then photo the bunker blueprints. Tip: Monitoring takes time. Clear the room, then interact, not the other way around." },
                new Quest { Name = "70. Paving the Way", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Enter an ENLICA building and read the note, go Piazza Roma apartment block over the Convinio, reach rooftop garden, then pull data from the researcher flat. Tip: Mark your entry stairwell. Rooftop garden routes can turn you around fast." },
                new Quest { Name = "71. Deciphering the Data", Status = "Not Started", Priority = "Medium", RelatedActivity = "Spaceport", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Use the Magnetic Decryptor in Fuel Control Building, then again on the top floor of the Arrival Building (Spaceport). Tip: Do the Arrival Building step first if you spawn nearby. That place gets contested." },
                new Quest { Name = "72. Groundbreaking", Status = "Not Started", Priority = "Medium", RelatedActivity = "Blue Gate", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Apollo. Objective: Get into the secured room at Pilgrim’s Peak (Blue Gate), collect construction docs, identify the target building from the whiteboard, photo the deserted housing project. Tip: Photo prompt needs the housing project in view, not the whiteboard itself." },
                new Quest { Name = "73. A Prime Specimen", Status = "Not Started", Priority = "Medium", RelatedActivity = "Blue Gate", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Get 2 ARC Powercells, then interact with an ARC Deforester on Blue Gate. Tip: You can interact and leave. Don’t commit to a full fight if your kit is light." },
                new Quest { Name = "74. With a View", Status = "Not Started", Priority = "Medium", RelatedActivity = "Stella Montis", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Get a Rotary Encoder, go to control rooms near the Assembly Line (Stella Montis), use it to flip the server switch. Tip: Pick one control-room route and stick to it. Zigzagging burns time and meds." },
                new Quest { Name = "75. The League", Status = "Not Started", Priority = "Medium", RelatedActivity = "Buried City", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Apollo. Objective: Bring Apollo a deflated football and a bicycle pump, photo the goal by Water Towers (Dam), photo football magazines in any Buried City kiosk. Tip: Kiosks are quick. Check the first one you see, don’t save it for last." },
                new Quest { Name = "76. Combat Recon", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Scope cover spots in Parking Garage stairs, by buses near Marano Park, and Main Street attics, kill 2 Spotters, deliver Spotter Relay to Shani. Tip: Do the “scope” steps first while it’s quiet, then hunt Spotters after." },
                new Quest { Name = "77. On Deaf Ears", Status = "Not Started", Priority = "Medium", RelatedActivity = "Stella Montis", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Lance. Objective: In Stella Montis, check guest logs on reception computers, find lecture notes, check Medical Research computers for prototype info, grab printed shipping logs in Assembly Workshops. Tip: Reception PCs can be blocked by chairs. Move around the desk until the prompt pops." },
                new Quest { Name = "78. Bombing Run", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Kill a Bombardier, deliver Bombardier Cell to Shani. Tip: Wait out its burst pattern, then finish. Loot cell instantly, don’t admire the drop." },
                new Quest { Name = "79. Movie Night", Status = "Not Started", Priority = "Medium", RelatedActivity = "Stella Montis", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Apollo. Objective: Bring Apollo a Portable TV, loot the Cultural Archives (Stella Montis) for old movie tapes, deliver the stack to Apollo. Tip: Cultural Archives has dense loot tables. Stay focused on tape spawns and leave." },
                new Quest { Name = "80. On the Map", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Complete the map-related survey task. Tip: Use the quest tracker and map markers to avoid wasting raid time." },
                new Quest { Name = "81. Stable Housing", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Complete the housing investigation task from the Shrouded Sky update. Tip: Move carefully through residential areas and extract after finishing the interaction." },
                new Quest { Name = "82. Worth Your Salt", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Apollo. Objective: Complete the resource recovery task from the Shrouded Sky update. Tip: Prioritize the quest item over extra loot." },
                new Quest { Name = "83. Keeping an Eye Out", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Complete the observation/recon task from the Shrouded Sky update. Tip: Clear the area before interacting with the objective." },
                new Quest { Name = "84. A Dead End", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Complete the investigation task from the Shrouded Sky update. Tip: Watch corners and avoid staying too long after the objective is complete." },
                new Quest { Name = "85. A Rising Tide", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Complete the final Shrouded Sky quest step. Tip: Treat this as a route quest and extract once the required interaction is done." },
                new Quest { Name = "86. Outstanding Balance", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Apollo. Objective: Complete the Flashpoint update task. Tip: Keep the objective short and avoid side fights." },
                new Quest { Name = "87. Clamoring for Attention", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Complete the Flashpoint update investigation. Tip: Expect contested areas and bring smoke or mobility." },
                new Quest { Name = "88. Dust on the Wires", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Complete the Flashpoint wiring/data task. Tip: Bring wires or tech materials if the quest requires them." },
                new Quest { Name = "89. Test Case", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Lance. Objective: Complete the Flashpoint testing task. Tip: Finish the required interaction first, then loot only if the area is quiet." },
                new Quest { Name = "90. Settled in Full", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Tian Wen. Objective: Complete the Flashpoint settlement task. Tip: Avoid over-looting after progress updates." },
                new Quest { Name = "91. Waking the Grid", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Shani. Objective: Complete the Flashpoint power/grid task. Tip: Clear the room before using terminals or switches." },
                new Quest { Name = "92. Fragmented Logs", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Lance. Objective: Complete the Flashpoint log recovery task. Tip: Check desks, terminals, and document-heavy rooms." },
                new Quest { Name = "93. Furtive Meetings", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Celeste. Objective: Complete the Flashpoint meeting investigation. Tip: Move through the route quickly and avoid unnecessary gunfire." },
                new Quest { Name = "94. Last Entry", Status = "Not Started", Priority = "Medium", RelatedActivity = "Any Map", CompletionNotes = "Mark as Complete after finishing this quest.", Notes = "Quest Giver: Lance. Objective: Complete the final Flashpoint quest entry. Tip: Extract immediately after completing the last required interaction." }
            );
        }

        if (!db.Blueprints.Any())
        {
            db.Blueprints.AddRange(
                new Blueprint { Name = "Angled Grip II", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mechanical Components; 3x Duct Tape", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Reduces horizontal recoil." },
                new Blueprint { Name = "Angled Grip III", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 5x Duct Tape", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Improved angled grip blueprint." },
                new Blueprint { Name = "Anvil", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "5x Mechanical Components; 5x Simple Gun Parts", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Heavy ammo revolver." },
                new Blueprint { Name = "Aphelion Rifle", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Magnetic Accelerator; 3x Complex Gun Parts; 1x Matriarch Reactor", WhereToGet = "Random Drop - Stella Montis", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Legendary rifle blueprint." },
                new Blueprint { Name = "Barricade Kit", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "1x Mechanical Components", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Placeable barricade item." },
                new Blueprint { Name = "Bettina", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Advanced Mechanical Components; 3x Heavy Gun Parts; 3x Canister", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Heavy ammo auto rifle." },
                new Blueprint { Name = "Blaze Grenade", Category = "Grenade", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "1x Explosive Compound; 2x Oil", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Impact grenade that spreads fire." },
                new Blueprint { Name = "Blue Light Stick", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Chemicals", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Blue utility light stick." },
                new Blueprint { Name = "Bobcat", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Advanced Mechanical Components; 3x Light Gun Parts", WhereToGet = "Random Drop - Hurricane / Locked Gate", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Quick-firing SMG." },
                new Blueprint { Name = "Burletta", Category = "Weapon", CollectionStatus = "Collected", Collected = true, RecipeMaterials = "3x Mechanical Components; 3x Simple Gun Parts", WhereToGet = "Quest Reward - Industrial Espionage", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Uncommon pistol blueprint." },
                new Blueprint { Name = "Canto", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Advanced Mechanical Components; 3x Light Gun Parts; 2x Wires", WhereToGet = "Random Drop - Hurricane", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Rare SMG blueprint tied to Hurricane cache farming." },
                new Blueprint { Name = "Combat Mk. 3 (Flanking)", Category = "Augment", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Advanced Electrical Components; 3x Processor", WhereToGet = "Random Drop - Blue Gate", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Flanking combat augment." },
                new Blueprint { Name = "Combat Mk.3 (Aggressive)", Category = "Augment", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Advanced Electrical Components; 3x Processor", WhereToGet = "Random Drop - Blue Gate", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Aggressive combat augment." },
                new Blueprint { Name = "Compensator II", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mechanical Components; 4x Wires", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Weapon compensator mod." },
                new Blueprint { Name = "Compensator III", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 8x Wires", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Improved weapon compensator mod." },
                new Blueprint { Name = "Complex Gun Parts", Category = "Material", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Light Gun Parts; 2x Medium Gun Parts; 2x Heavy Gun Parts", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Used for higher-tier weapon crafting." },
                new Blueprint { Name = "Crash Mat", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "Unknown / check tracker", WhereToGet = "Random Drop - Riven Tides", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Quick-use blueprint." },
                new Blueprint { Name = "Deadline", Category = "Mine", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Explosive Compound; 2x ARC Circuitry", WhereToGet = "Random Drop - Stella Montis", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Delayed explosive mine." },
                new Blueprint { Name = "Defibrillator", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "9x Plastic Parts; 1x Moss", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Revives a fallen teammate." },
                new Blueprint { Name = "Dolabra", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Shredder Gyro; 3x Magnetic Accelerator; 2x Vaporizer Regulator", WhereToGet = "Found Inside Assessors", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Legendary shotgun blueprint." },
                new Blueprint { Name = "Equalizer", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Magnetic Accelerator; 3x Complex Gun Parts; 1x Queen Reactor", WhereToGet = "Found Inside Harvesters", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Legendary beam rifle." },
                new Blueprint { Name = "Explosive Mine", Category = "Mine", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "1x Explosive Compound; 1x Sensors", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Explosive area mine." },
                new Blueprint { Name = "Extended Barrel II", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mechanical Components; 4x Wires", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Extended barrel weapon mod." },
                new Blueprint { Name = "Extended Barrel III", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 8x Wires", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Improved extended barrel mod." },
                new Blueprint { Name = "Extended Light Mag II", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mechanical Components; 3x Steel Spring", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Light ammo magazine mod." },
                new Blueprint { Name = "Extended Light Mag III", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 5x Steel Spring", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Improved light ammo magazine mod." },
                new Blueprint { Name = "Extended Medium Mag II", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mechanical Components; 3x Steel Spring", WhereToGet = "Random Drop - Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Medium ammo magazine mod." },
                new Blueprint { Name = "Extended Medium Mag III", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 5x Steel Spring", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Improved medium ammo magazine mod." },
                new Blueprint { Name = "Extended Shotgun Mag II", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mechanical Components; 3x Steel Spring", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Shotgun magazine mod." },
                new Blueprint { Name = "Extended Shotgun Mag III", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 5x Steel Spring", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Improved shotgun magazine mod." },
                new Blueprint { Name = "Fireworks Box", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "1x Explosive Compound; 3x Pop Trigger", WhereToGet = "Quest Reward - Test Case", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Shoots fireworks into the sky." },
                new Blueprint { Name = "Gas Mine", Category = "Mine", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "4x Chemicals; 2x Rubber Parts", WhereToGet = "Random Drop - Stella Montis", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Stamina-draining mine." },
                new Blueprint { Name = "Green Light Stick", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Chemicals", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Green utility light stick." },
                new Blueprint { Name = "Heavy Gun Parts", Category = "Material", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "4x Simple Gun Parts", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Heavy weapon crafting material." },
                new Blueprint { Name = "Hullcracker", Category = "Weapon", CollectionStatus = "Collected", Collected = true, RecipeMaterials = "1x Magnetic Accelerator; 3x Heavy Gun Parts; 1x Exodus Modules", WhereToGet = "Quest Reward - The Major's Footlocker", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Grenade launcher effective against ARCs." },
                new Blueprint { Name = "Il Toro", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "5x Mechanical Components; 6x Simple Gun Parts", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Hard-hitting shotgun." },
                new Blueprint { Name = "Jolt Mine", Category = "Mine", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "1x Electrical Components; 1x Battery", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Stun mine." },
                new Blueprint { Name = "Jupiter", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Magnetic Accelerator; 3x Complex Gun Parts; 1x Queen Reactor", WhereToGet = "Found Inside Harvesters", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Legendary bolt-action rifle." },
                new Blueprint { Name = "Light Gun Parts", Category = "Material", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "4x Simple Gun Parts", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Light weapon crafting material." },
                new Blueprint { Name = "Lightweight Stock", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 5x Duct Tape", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Weapon stock mod." },
                new Blueprint { Name = "Looting MK. 3 (Safekeeper)", Category = "Augment", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Advanced Electrical Components; 3x Processor", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Safekeeper looting augment." },
                new Blueprint { Name = "Looting MK. 3 (Survivor)", Category = "Augment", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Advanced Electrical Components; 3x Processor", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Survivor looting augment." },
                new Blueprint { Name = "Lure Grenade", Category = "Grenade", CollectionStatus = "Collected", Collected = true, RecipeMaterials = "1x Speaker Component; 1x Electrical Components", WhereToGet = "Quest Reward - Greasing Her Palms", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Draws ARC attention." },
                new Blueprint { Name = "Medium Gun Parts", Category = "Material", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "4x Simple Gun Parts", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Medium weapon crafting material." },
                new Blueprint { Name = "Muzzle Brake II", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mechanical Components; 4x Wires", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Recoil-control muzzle mod." },
                new Blueprint { Name = "Muzzle Brake III", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 8x Wires", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Improved recoil-control muzzle mod." },
                new Blueprint { Name = "Osprey", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Advanced Mechanical Components; 3x Medium Gun Parts; 7x Wires", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Medium ammo sniper rifle." },
                new Blueprint { Name = "Padded Stock", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 5x Duct Tape", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid / Hidden Bunker", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Stock mod for recoil and dispersion control." },
                new Blueprint { Name = "Powered Descender", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "Unknown / check tracker", WhereToGet = "Random Drop - Riven Tides", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Quick-use movement blueprint." },
                new Blueprint { Name = "Pulse Mine", Category = "Mine", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "1x Crude Explosives; 1x Wires", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Knockback pulse mine." },
                new Blueprint { Name = "Rascal", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "Unknown / check tracker", WhereToGet = "Random Drop - Riven Tides", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Weapon blueprint." },
                new Blueprint { Name = "Red Light Stick", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Chemicals", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Red utility light stick." },
                new Blueprint { Name = "Remote Raider Flare", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Chemicals; 4x Rubber Parts", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Imitates a downed-state flare." },
                new Blueprint { Name = "Seeker Grenade", Category = "Grenade", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "1x Crude Explosives; 2x ARC Alloy", WhereToGet = "Stella Montis", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Self-targeting grenade against ARCs." },
                new Blueprint { Name = "Shotgun Choke II", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mechanical Components; 4x Wires", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Shotgun spread-control mod." },
                new Blueprint { Name = "Shotgun Choke III", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 8x Wires", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid / Hidden Bunker", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Improved shotgun spread-control mod." },
                new Blueprint { Name = "Shotgun Silencer", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 8x Wires", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid / Hidden Bunker", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Shotgun sound reduction mod." },
                new Blueprint { Name = "Showstopper", Category = "Grenade", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "1x Advanced Electrical Components; 1x Voltage Converter", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Throwable stun explosive." },
                new Blueprint { Name = "Silencer I", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mechanical Components; 4x Wires", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Basic weapon silencer." },
                new Blueprint { Name = "Silencer II", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 8x Wires", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Improved weapon silencer." },
                new Blueprint { Name = "Smoke Grenade", Category = "Grenade", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "14x Chemicals; 1x Canister", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Creates a smoke cloud." },
                new Blueprint { Name = "Snap Hook", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Power Rod; 3x Rope; 1x Exodus Modules", WhereToGet = "Random Drop - Electromagnetic Storm", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Grapple-style movement item." },
                new Blueprint { Name = "Stable Stock II", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mechanical Components; 3x Duct Tape", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Stock stability mod." },
                new Blueprint { Name = "Stable Stock III", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 5x Duct Tape", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Improved stock stability mod." },
                new Blueprint { Name = "Surge Coil", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "Unknown / check tracker", WhereToGet = "Random Drop - Electromagnetic Storm", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Quick-use blueprint." },
                new Blueprint { Name = "Tactical Mk. 3 (Revival)", Category = "Augment", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Advanced Electrical Components; 3x Processor", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Revival tactical augment." },
                new Blueprint { Name = "Tactical MK.3 (Defensive)", Category = "Augment", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Advanced Electrical Components; 3x Processor", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Defensive tactical augment." },
                new Blueprint { Name = "Tactical MK.3 (Healing)", Category = "Augment", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Advanced Electrical Components; 3x Processor", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Healing tactical augment." },
                new Blueprint { Name = "Tactical MK.3 (Smoke)", Category = "Augment", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Advanced Electrical Components; 3x Processor", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Smoke tactical augment." },
                new Blueprint { Name = "Tagging Grenade", Category = "Grenade", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "1x Electrical Components; 1x Sensors", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Marks raiders and ARCs." },
                new Blueprint { Name = "Tempest", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Advanced Mechanical Components; 3x Medium Gun Parts; 3x Canister", WhereToGet = "Random Drop - Night Raid / Hurricane", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Fast medium ammo auto rifle." },
                new Blueprint { Name = "Torrente", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Advanced Mechanical Components; 3x Medium Gun Parts; 6x Steel Spring", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Light machine gun." },
                new Blueprint { Name = "Trailblazer Grenade", Category = "Grenade", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "1x Explosive Compound; 1x Synthesized Fuel", WhereToGet = "Random Drop - Stella Montis", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Creates an ignitable trail." },
                new Blueprint { Name = "Trigger Nade", Category = "Grenade", CollectionStatus = "Collected", Collected = true, RecipeMaterials = "2x Crude Explosives; 1x Processor", WhereToGet = "Quest Reward - Sparks Fly", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Remote-triggered explosive." },
                new Blueprint { Name = "Venator", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Advanced Mechanical Components; 3x Medium Gun Parts; 5x Magnet", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Medium ammo pistol." },
                new Blueprint { Name = "Vertical Grip II", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mechanical Components; 3x Duct Tape", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Vertical recoil grip mod." },
                new Blueprint { Name = "Vertical Grip III", Category = "Mod", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Mod Components; 5x Duct Tape", WhereToGet = "Random Drop - Electromagnetic Storm / Locked Gate / Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Improved vertical recoil grip mod." },
                new Blueprint { Name = "Vita Shot", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Antiseptic; 1x Syringe", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Health recovery quick-use item." },
                new Blueprint { Name = "Vita Spray", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Antiseptic; 1x Canister", WhereToGet = "Quest Reward - Worth Your Salt", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Health recovery spray." },
                new Blueprint { Name = "Vulcano", Category = "Weapon", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "1x Magnetic Accelerator; 3x Heavy Gun Parts; 1x Exodus Modules", WhereToGet = "Random Drop - Hurricane / Hidden Bunker", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Semi-auto shotgun." },
                new Blueprint { Name = "White Flag", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "Unknown / check tracker", WhereToGet = "Random Drop - Riven Tides", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Quick-use blueprint." },
                new Blueprint { Name = "Wolfpack", Category = "Grenade", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "2x Explosive Compound; 2x Sensors", WhereToGet = "Random Drop - Night Raid", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Self-targeting ARC grenade." },
                new Blueprint { Name = "Yellow Light Stick", Category = "Quick Use", CollectionStatus = "Not Collected", Collected = false, RecipeMaterials = "3x Chemicals", WhereToGet = "Random Drop - Any Map", SourceNotes = "GamesRadar Blueprint Locations", Notes = "Yellow utility light stick." }
            );

            db.SaveChanges();
        }

        if (!db.RivenTidesRecords.Any())
        {
            db.RivenTidesRecords.AddRange(
                new RivenTidesRecord { Name = "Panorama Azzurro Resort", RecordType = "Zone Note", Zone = "Hotel and Resort", RiskLevel = "High", RecommendedTool = "Smoke Grenade and mobility kit", LootFocus = "Interior loot, resort route notes, blueprint checks", Notes = "Use this as the main Riven Tides resort planning entry. Mark complete after the route has been reviewed." },
                new RivenTidesRecord { Name = "Exodus Port Dockyard", RecordType = "Zone Note", Zone = "Port / Dockyard", RiskLevel = "Medium", RecommendedTool = "Balanced loadout", LootFocus = "Industrial containers, port authority pathing", Notes = "Good route for steady loot checks without committing to the highest-risk resort interiors." },
                new RivenTidesRecord { Name = "Beachcombing Route", RecordType = "Beachcombing", Zone = "Beachfront", RiskLevel = "Medium", RecommendedTool = "Dockmaster Detector", LootFocus = "Buried loot and quick extraction planning", Notes = "Track beachcombing notes separately so tool-based loot runs do not get mixed into normal blueprint farming." },
                new RivenTidesRecord { Name = "ARC Turbine Prep", RecordType = "Boss Prep", Zone = "Open Coast / Vertical Sightlines", RiskLevel = "High", RecommendedTool = "EMP, shield battery, ranged primary", LootFocus = "Boss encounter planning", Notes = "Use a durability-safe loadout and avoid overpacking rare gear if the goal is only scouting the encounter." },
                new RivenTidesRecord { Name = "Last Resort Event", RecordType = "Event", Zone = "Riven Tides", RiskLevel = "Medium", RecommendedTool = "Any reliable extraction kit", LootFocus = "Event progress and reward notes", Notes = "Use this entry to track event progress, XP goals, and reward reminders." },
                new RivenTidesRecord { Name = "White Flag", RecordType = "Blueprint", Zone = "Riven Tides", RiskLevel = "Medium", RecommendedTool = "Blueprint farming kit", LootFocus = "Expansion blueprint collection", Notes = "Added as a Riven Tides blueprint goal for the expansion checklist." },
                new RivenTidesRecord { Name = "Powered Descender", RecordType = "Blueprint", Zone = "Riven Tides", RiskLevel = "Medium", RecommendedTool = "Blueprint farming kit", LootFocus = "Expansion blueprint collection", Notes = "Added as a Riven Tides blueprint goal for the expansion checklist." },
                new RivenTidesRecord { Name = "Crash Mat", RecordType = "Blueprint", Zone = "Riven Tides", RiskLevel = "Medium", RecommendedTool = "Blueprint farming kit", LootFocus = "Expansion blueprint collection", Notes = "Added as a Riven Tides blueprint goal for the expansion checklist." },
                new RivenTidesRecord { Name = "Tactical Mk. 3 Smoke Augment", RecordType = "Blueprint", Zone = "Riven Tides", RiskLevel = "High", RecommendedTool = "High-value blueprint farming kit", LootFocus = "Expansion blueprint collection", Notes = "Added as a Riven Tides blueprint goal for the expansion checklist." }
            );
        }


        if (!db.InventoryItems.Any())
        {
            db.InventoryItems.AddRange(
                new InventoryItem { Name = "ARC Alloy", Category = "Material", Rarity = "Common", BestSource = "ARC enemies on Dam Battlegrounds", UsedFor = "Quest turn-ins, grenades, and crafting", KeepTarget = 12, CurrentCount = 3, SellValue = 80, Notes = "High-volume material. Keep a reserve before selling extras." },
                new InventoryItem { Name = "Battery", Category = "Material", Rarity = "Common", BestSource = "Research/Admin shelves and electrical rooms", UsedFor = "Jolt Mine, quests, workshop upgrades", KeepTarget = 8, CurrentCount = 1, SellValue = 60, Notes = "Prioritize early because several systems depend on it." },
                new InventoryItem { Name = "Wires", Category = "Material", Rarity = "Common", BestSource = "Server racks and maintenance shelves", UsedFor = "Muzzle Brake, Silencer, Shotgun Choke", KeepTarget = 20, CurrentCount = 7, SellValue = 45, Notes = "Always worth grabbing because weapon mods consume a lot." },
                new InventoryItem { Name = "Magnetic Accelerator", Category = "Rare Component", Rarity = "Epic", BestSource = "High-value technical containers", UsedFor = "Jupiter and Hullcracker blueprint chains", KeepTarget = 3, CurrentCount = 0, SellValue = 450, Favorite = true, Notes = "Critical rare component. Do not sell until major weapons are crafted." },
                new InventoryItem { Name = "Queen Reactor", Category = "Rare Component", Rarity = "Legendary", BestSource = "Harvester/Queen-related loot paths", UsedFor = "Legendary weapon crafting", KeepTarget = 1, CurrentCount = 0, SellValue = 900, Favorite = true, Notes = "Treat as a top-priority extraction item." },
                new InventoryItem { Name = "Rope", Category = "Utility", Rarity = "Common", BestSource = "Residential containers", UsedFor = "Snap Hook and movement quick-use crafting", KeepTarget = 6, CurrentCount = 2, SellValue = 35, Notes = "Low-value but important when chasing movement tools." },
                new InventoryItem { Name = "Speaker Component", Category = "Electronics", Rarity = "Rare", BestSource = "Computers and electronics boxes", UsedFor = "Lure Grenade and scanner utility crafting", KeepTarget = 4, CurrentCount = 0, SellValue = 240, Notes = "Good target for computer-search routes." },
                new InventoryItem { Name = "Advanced Electrical Components", Category = "Electronics", Rarity = "Epic", BestSource = "Locked Gate, Night, and technical containers", UsedFor = "Showstopper, augments, high-tier upgrades", KeepTarget = 5, CurrentCount = 1, SellValue = 380, Notes = "Farm under major conditions when possible." });
        }

        if (!db.IntelGuides.Any())
        {
            db.IntelGuides.AddRange(
                new IntelGuide { Name = "Residential Blueprint Sweep", GuideType = "Blueprint Route", MapName = "Blue Gate", MapCondition = "Standard / Locked Gate", Difficulty = "Medium", RecommendedRoute = "Village interiors to apartments to extract.", LootFocus = "Wardrobes, cupboards, desks, residential containers, attachment blueprint pools.", RiskWarning = "Locked Gate routes attract PvP. Avoid over-looting after the target cluster is checked.", Notes = "Built around container-type logic instead of chasing one exact spawn." },
                new IntelGuide { Name = "Technical Computer Sprint", GuideType = "Trial Route", MapName = "Spaceport", MapCondition = "Standard", Difficulty = "Low", RecommendedRoute = "Office lanes to terminal rooms to side extract.", LootFocus = "Computers, electronics, processors, speaker components, weekly computer-search scoring.", RiskWarning = "Open hallways can become crossfire lanes. Smoke exposed rotations.", Notes = "Pairs well with Search Computers and electronics farming." },
                new IntelGuide { Name = "Night Raid Rare Pool", GuideType = "Condition Farm", MapName = "Dam Battlegrounds", MapCondition = "Night Raid", Difficulty = "High", RecommendedRoute = "Research/Admin edge route to locked rooms to immediate extraction.", LootFocus = "Advanced electrical components, high-tier mod pools, major-condition blueprint rolls.", RiskWarning = "Night increases route uncertainty and ambush risk. Bring escape utility.", Notes = "Use only when the target item justifies the risk." });
        }

        if (!db.WeeklyTrials.Any())
        {
            db.WeeklyTrials.AddRange(
                new WeeklyTrial { Name = "Search Computers", ObjectiveType = "Loot", TargetScore = 3000, ScorePerAction = 1000, BestMap = "Spaceport", Strategy = "Route through office and terminal interiors, search three computers, then extract.", Notes = "Companion-style trial optimization." },
                new WeeklyTrial { Name = "Loot Weapon Crates", ObjectiveType = "Loot", TargetScore = 3000, ScorePerAction = 1000, BestMap = "Dam Battlegrounds", Strategy = "Hit known crate lanes first, avoid optional fights, and leave once three crates are secured.", Notes = "Use a balanced kit with smoke." },
                new WeeklyTrial { Name = "Damage ARC Enemies", ObjectiveType = "Combat", TargetScore = 3000, ScorePerAction = 500, BestMap = "Dam Battlegrounds", Strategy = "Pull ARC enemies into controlled choke points and use grenades or sustained rifle fire.", Notes = "ARC Breaker loadout recommended." });
        }

        SeedMapConditionOptions(db);
        SeedSkills(db);

        db.SaveChanges();
    }


    private static void SeedMapConditionOptions(RaidersVaultContext db)
    {
        if (db.MapConditionOptions.Any())
        {
            return;
        }

        var mapConditions = new Dictionary<string, string[]>
        {
            ["Dam Battlegrounds"] = new[]
            {
                "Standard Patrol",
                "Prospecting Probes",
                "Husk Graveyard",
                "Uncovered Caches",
                "Lush Blooms",
                "Harvester",
                "Matriarch",
                "Night Raid",
                "Electromagnetic Storm",
                "Hurricane",
                "Cold Snap",
                "Close Scrutiny"
            },
            ["Buried City"] = new[]
            {
                "Standard Patrol",
                "Prospecting Probes",
                "Husk Graveyard",
                "Uncovered Caches",
                "Lush Blooms",
                "Matriarch",
                "Night Raid",
                "Electromagnetic Storm",
                "Hurricane",
                "Cold Snap",
                "Close Scrutiny"
            },
            ["Spaceport"] = new[]
            {
                "Standard Patrol",
                "Prospecting Probes",
                "Husk Graveyard",
                "Uncovered Caches",
                "Lush Blooms",
                "Launch Tower Loot",
                "Harvester",
                "Night Raid",
                "Electromagnetic Storm",
                "Hidden Bunker",
                "Hurricane",
                "Cold Snap",
                "Close Scrutiny"
            },
            ["The Blue Gate"] = new[]
            {
                "Standard Patrol",
                "Prospecting Probes",
                "Husk Graveyard",
                "Uncovered Caches",
                "Lush Blooms",
                "Harvester",
                "Matriarch",
                "Night Raid",
                "Electromagnetic Storm",
                "Locked Gate",
                "Hurricane",
                "Cold Snap",
                "Close Scrutiny"
            },
            ["Stella Montis"] = new[]
            {
                "Standard Patrol",
                "Night Raid"
            },
            ["Riven Tides"] = new[]
            {
                "Standard",
                "Night",
                "Husk Graveyard",
                "Beachcomber"
            }
        };

        foreach (var map in mapConditions)
        {
            for (var i = 0; i < map.Value.Length; i++)
            {
                db.MapConditionOptions.Add(new MapConditionOption
                {
                    MapName = map.Key,
                    ConditionName = map.Value[i],
                    DisplayOrder = i + 1
                });
            }
        }
    }

    private static void SeedSkills(RaidersVaultContext db)
    {
        if (db.Skills.Any())
        {
            return;
        }

        var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "arc_raiders_skills.json");

        if (!File.Exists(dataPath))
        {
            dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "arc_raiders_skills.json");
        }

        if (!File.Exists(dataPath))
        {
            return;
        }

        var json = File.ReadAllText(dataPath);
        var skills = JsonSerializer.Deserialize<List<Skill>>(
            json,
            JsonOptions);

        if (skills == null)
        {
            return;
        }

        foreach (var skill in skills)
        {
            skill.CurrentPoints = 0;
            if (string.IsNullOrWhiteSpace(skill.Requires))
            {
                skill.Requires = null;
            }
        }

        db.Skills.AddRange(skills);
    }

    public static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);

        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }
}
