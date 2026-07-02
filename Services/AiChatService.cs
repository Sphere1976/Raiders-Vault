using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;

namespace RaidersVault.Services;

public sealed record AiChatResult(string Reply, bool UsedAi, IReadOnlyList<string> Suggestions);

public sealed class AiChatService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly RaidersVaultContext _db;
    private readonly BlueprintRecommendationService _blueprintRecommendations;
    private readonly LoadoutRecommendationService _loadoutRecommendations;
    private readonly ILogger<AiChatService> _logger;

    public AiChatService(
        HttpClient httpClient,
        IConfiguration configuration,
        RaidersVaultContext db,
        BlueprintRecommendationService blueprintRecommendations,
        LoadoutRecommendationService loadoutRecommendations,
        ILogger<AiChatService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _db = db;
        _blueprintRecommendations = blueprintRecommendations;
        _loadoutRecommendations = loadoutRecommendations;
        _logger = logger;
    }

    public async Task<AiChatResult> AskAsync(
        string message,
        string? page,
        string? user,
        CancellationToken cancellationToken = default)
    {
        message = InputSanitizer.Clean(message);
        page = InputSanitizer.CleanOptional(page);
        user = InputSanitizer.CleanOptional(user);

        if (string.IsNullOrWhiteSpace(message))
        {
            return new AiChatResult(
                "Ask me what to farm, which blueprint to chase, or how to plan your next run.",
                false,
                BuildSuggestions());
        }

        if (IsConversationRepairMessage(message.ToLowerInvariant()))
        {
            return new AiChatResult(BuildConversationRepairAnswer(), false, BuildSuggestions());
        }

        var context = await BuildVaultContextAsync(message, page, user, cancellationToken);
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BuildLocalAnswer(message, context);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = JsonContent.Create(new
            {
                model = _configuration["OpenAI:Model"] ?? Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4.1-mini",
                input = new object[]
                {
                    new
                    {
                        role = "system",
                        content = BuildSystemPrompt(context)
                    },
                    new
                    {
                        role = "user",
                        content = message
                    }
                },
                max_output_tokens = 520,
                temperature = 0.25
            }, options: JsonOptions);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI chatbot request failed with status {StatusCode}.", response.StatusCode);
                return BuildLocalAnswer(message, context);
            }

            var reply = ExtractResponseText(payload);
            if (string.IsNullOrWhiteSpace(reply))
            {
                return BuildLocalAnswer(message, context);
            }

            return new AiChatResult(reply, true, BuildSuggestions());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenAI chatbot request failed; returning local assistant response.");
            return BuildLocalAnswer(message, context);
        }
    }

    private async Task<VaultContext> BuildVaultContextAsync(
        string message,
        string? page,
        string? user,
        CancellationToken cancellationToken)
    {
        var profile = await _db.PlayerProfiles
            .AsNoTracking()
            .OrderBy(profile => profile.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var neededItems = await _db.InventoryItems
            .AsNoTracking()
            .Where(item => item.CurrentCount < item.KeepTarget)
            .OrderByDescending(item => item.Rarity == "Legendary")
            .ThenByDescending(item => item.Rarity == "Epic")
            .ThenByDescending(item => item.KeepTarget - item.CurrentCount)
            .ThenByDescending(item => item.SellValue)
            .Take(8)
            .Select(item => $"{item.Name} ({item.Rarity}, need {item.KeepTarget - item.CurrentCount}, source {item.BestSource})")
            .ToListAsync(cancellationToken);

        var blueprintTargets = await _db.Blueprints
            .AsNoTracking()
            .Where(blueprint => !blueprint.Collected)
            .OrderBy(blueprint => blueprint.CollectionStatus)
            .ThenBy(blueprint => blueprint.Name)
            .Take(6)
            .Select(blueprint => $"{blueprint.Name} ({blueprint.Category}, {blueprint.CollectionStatus}, source {blueprint.WhereToGet ?? "unknown"})")
            .ToListAsync(cancellationToken);

        var trials = await _db.WeeklyTrials
            .AsNoTracking()
            .OrderBy(trial => trial.ObjectiveType)
            .Take(4)
            .Select(trial => $"{trial.ObjectiveType}: {trial.TargetScore} target on {trial.BestMap}; {trial.Strategy}")
            .ToListAsync(cancellationToken);

        var relevantBlueprints = await BuildRelevantBlueprintsAsync(message, profile?.PreferredPlaystyle ?? "Balanced", cancellationToken);
        var relevantItems = await BuildRelevantItemsAsync(message, cancellationToken);
        var relevantIntel = await BuildRelevantIntelAsync(message, cancellationToken);
        var relevantRivenTides = await BuildRelevantRivenTidesAsync(message, cancellationToken);
        var mapRiskSummary = await BuildMapRiskSummaryAsync(cancellationToken);
        var appSurface = await BuildAppSurfaceAsync(cancellationToken);
        var appPageMatches = BuildAppPageMatches(message);
        var userPriorities = await BuildUserPrioritiesAsync(cancellationToken);
        var externalKnowledge = BuildExternalKnowledge(message);
        var pvpLoadout = _loadoutRecommendations.Build(
            profile?.DefaultMap ?? "Dam Battlegrounds",
            DetectCondition(message),
            "PvP",
            profile?.CurrentSkillPoints ?? 0);
        var pveLoadout = _loadoutRecommendations.Build(
            profile?.DefaultMap ?? "Dam Battlegrounds",
            DetectCondition(message),
            "PvE",
            profile?.CurrentSkillPoints ?? 0);
        var savedLoadouts = await BuildRelevantLoadoutsAsync(message, cancellationToken);
        var gameKnowledge = await BuildRelevantGameKnowledgeAsync(message, cancellationToken);

        return new VaultContext(
            user ?? "operator",
            page ?? "Unknown",
            profile?.PlayerName ?? "Raider",
            profile?.PreferredPlaystyle ?? "Balanced",
            profile?.DefaultMap ?? "Dam Battlegrounds",
            profile?.CurrentSkillPoints ?? 0,
            neededItems,
            blueprintTargets,
            trials,
            relevantBlueprints,
            relevantItems,
            relevantIntel,
            relevantRivenTides,
            mapRiskSummary,
            appSurface,
            appPageMatches,
            userPriorities,
            externalKnowledge,
            FormatLoadout(pvpLoadout),
            FormatLoadout(pveLoadout),
            savedLoadouts,
            gameKnowledge);
    }

    private static string BuildSystemPrompt(VaultContext context)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("You are Raiders Vault AI, an in-app ARC Raiders planning assistant.");
        prompt.AppendLine("Give concise, tactical answers grounded in the provided app context.");
        prompt.AppendLine("Help with farming priorities, route planning, inventory gaps, blueprints, trials, and loadout decisions.");
        prompt.AppendLine("Do not claim live game facts beyond this app context. If uncertain, say what to verify in-game.");
        prompt.AppendLine($"Signed-in user: {context.UserName}");
        prompt.AppendLine($"Current page: {context.CurrentPage}");
        prompt.AppendLine($"Profile: {context.PlayerName}, {context.Playstyle}, default map {context.DefaultMap}, skill points {context.SkillPoints}");
        prompt.AppendLine($"Top needed items: {string.Join("; ", context.NeededItems.DefaultIfEmpty("none"))}");
        prompt.AppendLine($"Blueprint targets: {string.Join("; ", context.BlueprintTargets.DefaultIfEmpty("none"))}");
        prompt.AppendLine($"Weekly trials: {string.Join("; ", context.WeeklyTrials.DefaultIfEmpty("none"))}");
        prompt.AppendLine($"Exact blueprint matches for this question: {string.Join("; ", context.RelevantBlueprints.Select(FormatBlueprintMatch).DefaultIfEmpty("none"))}");
        prompt.AppendLine($"Exact item matches for this question: {string.Join("; ", context.RelevantItems.DefaultIfEmpty("none"))}");
        prompt.AppendLine($"Relevant intel routes for this question: {string.Join("; ", context.RelevantIntel.DefaultIfEmpty("none"))}");
        prompt.AppendLine($"Relevant Riven Tides records for this question: {string.Join("; ", context.RelevantRivenTides.DefaultIfEmpty("none"))}");
        prompt.AppendLine($"Map danger ranking from app data: {string.Join("; ", context.MapRiskSummary.DefaultIfEmpty("none"))}");
        prompt.AppendLine($"ARC threat ranking from app data: {string.Join("; ", ArcThreats.Select(FormatArcThreat))}");
        prompt.AppendLine($"Raiders Vault app surface: {string.Join("; ", context.AppSurface.DefaultIfEmpty("none"))}");
        prompt.AppendLine($"Relevant app page matches: {string.Join("; ", context.AppPageMatches.DefaultIfEmpty("none"))}");
        prompt.AppendLine($"Current user/vault priorities: {string.Join("; ", context.UserPriorities.DefaultIfEmpty("none"))}");
        prompt.AppendLine($"External ARC Raiders / MetaForge source matches: {string.Join("; ", context.ExternalKnowledge.Select(FormatExternalKnowledge).DefaultIfEmpty("none"))}");
        prompt.AppendLine($"PvP loadout recommendation from app engine: {context.PvpLoadout}");
        prompt.AppendLine($"PvE loadout recommendation from app engine: {context.PveLoadout}");
        prompt.AppendLine($"Relevant saved loadouts: {string.Join("; ", context.RelevantLoadouts.DefaultIfEmpty("none"))}");
        prompt.AppendLine($"Best all-vault game-data matches for this question: {string.Join("; ", context.GameKnowledge.Select(FormatGameKnowledgeHit).DefaultIfEmpty("none"))}");
        return prompt.ToString();
    }

    private static AiChatResult BuildLocalAnswer(string message, VaultContext context)
    {
        var lower = message.ToLowerInvariant();
        var lead = context.NeededItems.FirstOrDefault() ?? "your highest-needed inventory item";
        var blueprint = context.BlueprintTargets.FirstOrDefault() ?? "your next uncollected blueprint";
        var trial = context.WeeklyTrials.FirstOrDefault() ?? "the weekly trial with the clearest map fit";

        string reply;
        if (IsConversationRepairMessage(lower))
        {
            reply = BuildConversationRepairAnswer();
        }
        else if (IsPvpPveComparisonQuestion(lower))
        {
            reply = BuildPvpPveComparisonAnswer(context);
        }
        else if (IsPvpLoadoutQuestion(lower))
        {
            reply = BuildPvpLoadoutAnswer(context);
        }
        else if (IsAppCoverageQuestion(lower))
        {
            reply = BuildAppCoverageAnswer(context);
        }
        else if (context.AppPageMatches.Count > 0 && IsAppPageQuestion(lower))
        {
            reply = BuildAppPageAnswer(context);
        }
        else if (context.ExternalKnowledge.Count > 0 && IsExternalKnowledgeQuestion(lower))
        {
            reply = BuildExternalKnowledgeAnswer(context.ExternalKnowledge);
        }
        else if (IsMapFeatureQuestion(lower) && context.RelevantRivenTides.Count > 0)
        {
            reply = BuildRivenTidesAnswer(context);
        }
        else if (IsArcThreatQuestion(lower))
        {
            reply = BuildArcThreatAnswer();
        }
        else if (IsMapDangerQuestion(lower))
        {
            reply = BuildMapDangerAnswer(context);
        }
        else if (context.RelevantBlueprints.Count > 0)
        {
            reply = BuildBlueprintAnswer(context.RelevantBlueprints[0], context);
        }
        else if (context.RelevantItems.Count > 0 && (lower.Contains("where") || lower.Contains("get") || lower.Contains("find") || lower.Contains("farm")))
        {
            reply = $"I found this in the item database: {context.RelevantItems[0]}. Use that source first, then check the Database page if you want to add it to a loot plan.";
        }
        else if (context.RelevantIntel.Count > 0)
        {
            reply = $"Relevant app intel: {context.RelevantIntel[0]}. Use that as the route anchor, then keep the run tight around the listed loot focus.";
        }
        else if (context.GameKnowledge.Count > 0 && IsGameKnowledgeQuestion(lower))
        {
            reply = BuildGameKnowledgeAnswer(message, context);
        }
        else if (IsPriorityQuestion(lower))
        {
            reply = BuildPriorityAnswer(context);
        }
        else if (lower.Contains("blueprint"))
        {
            reply = $"Start with {blueprint}. Pair that target with a short {context.DefaultMap} route, then extract once the recipe/source check is done instead of widening the run.";
        }
        else if (lower.Contains("trial") || lower.Contains("weekly"))
        {
            reply = $"For trials, prioritize {trial}. Build the run around one scoring action, repeat the safest loop, and only add side loot when it overlaps with needed materials.";
        }
        else if (lower.Contains("farm") || lower.Contains("item") || lower.Contains("loot"))
        {
            reply = $"Your strongest farming target is {lead}. Run a compact route through that source, bring enough space for priority materials, and stop when the target is secured.";
        }
        else if (lower.Contains("route") || lower.Contains("plan"))
        {
            reply = $"For a clean route, use {context.DefaultMap} as the anchor: first check the source for {lead}, add one blueprint stop for {blueprint}, then extract before the risk curve climbs.";
        }
        else
        {
            reply = $"I would focus on {lead}, keep {blueprint} as the secondary objective, and use {context.Playstyle.ToLowerInvariant()} routing on {context.DefaultMap}. Ask me for a farm route, blueprint plan, or trial plan and I can narrow it down.";
        }

        return new AiChatResult(reply, false, BuildSuggestions());
    }

    private async Task<IReadOnlyList<BlueprintMatch>> BuildRelevantBlueprintsAsync(
        string message,
        string playstyle,
        CancellationToken cancellationToken)
    {
        var blueprints = await _db.Blueprints
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return blueprints
            .Select(blueprint => new
            {
                Blueprint = blueprint,
                Score = MatchScore(message, blueprint.Name)
            })
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Blueprint.Name)
            .Take(4)
            .Select(match =>
            {
                var plan = _blueprintRecommendations.BuildFarmPlan(match.Blueprint, playstyle);
                return new BlueprintMatch(
                    match.Blueprint.Name,
                    match.Blueprint.Category,
                    match.Blueprint.CollectionStatus,
                    match.Blueprint.WhereToGet ?? "Unknown source",
                    match.Blueprint.RecipeMaterials ?? "Unknown recipe",
                    match.Blueprint.SourceNotes ?? "No source note",
                    match.Blueprint.Notes ?? "No notes",
                    plan.BestMap,
                    plan.BestCondition,
                    plan.ContainerType,
                    plan.BestAreas,
                    plan.FarmingRoute,
                    plan.ProbabilityLabel);
            })
            .ToList();
    }

    private async Task<IReadOnlyList<string>> BuildRelevantItemsAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var items = await _db.InventoryItems
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return items
            .Select(item => new
            {
                Item = item,
                Score = MatchScore(message, item.Name)
            })
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Item.Name)
            .Take(5)
            .Select(match => $"{match.Item.Name} ({match.Item.Rarity}, source {match.Item.BestSource}, used for {match.Item.UsedFor}, keep {match.Item.KeepTarget}, current {match.Item.CurrentCount})")
            .ToList();
    }

    private async Task<IReadOnlyList<string>> BuildRelevantIntelAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var intel = await _db.IntelGuides
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return intel
            .Select(guide => new
            {
                Guide = guide,
                Score = Math.Max(
                    MatchScore(message, guide.Name),
                    Math.Max(MatchScore(message, guide.MapName), MatchScore(message, guide.MapCondition)))
            })
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Guide.Name)
            .Take(4)
            .Select(match => $"{match.Guide.Name} ({match.Guide.GuideType}, {match.Guide.MapName}, {match.Guide.MapCondition}): {match.Guide.RecommendedRoute} Loot focus: {match.Guide.LootFocus}")
            .ToList();
    }

    private async Task<IReadOnlyList<string>> BuildRelevantRivenTidesAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var lower = message.ToLowerInvariant();
        var isRivenQuestion = lower.Contains("riven")
            || lower.Contains("tides")
            || lower.Contains("beach")
            || lower.Contains("dock")
            || lower.Contains("resort")
            || lower.Contains("turbine");

        if (!isRivenQuestion)
        {
            return Array.Empty<string>();
        }

        var records = await _db.RivenTidesRecords
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return records
            .Select(record => new
            {
                Record = record,
                Score = Math.Max(
                    MatchScore(message, record.Name),
                    Math.Max(
                        MatchScore(message, record.RecordType),
                        Math.Max(MatchScore(message, record.Zone), MatchScore(message, record.LootFocus ?? string.Empty))))
            })
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Record.RecordType == "Beachcombing" ? 0 : 1)
            .ThenBy(match => match.Record.RiskLevel == "Medium" ? 0 : 1)
            .ThenBy(match => match.Record.Name)
            .Take(8)
            .Select(match => $"{match.Record.Name} ({match.Record.RecordType}, zone {match.Record.Zone}, risk {match.Record.RiskLevel}, tool {match.Record.RecommendedTool ?? "none"}, loot focus {match.Record.LootFocus ?? "none"}): {match.Record.Notes ?? "No notes"}")
            .ToList();
    }

    private async Task<IReadOnlyList<string>> BuildMapRiskSummaryAsync(CancellationToken cancellationToken)
    {
        var conditionOptions = await _db.MapConditionOptions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var rivenRecords = await _db.RivenTidesRecords
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var loadouts = await _db.Loadouts
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var mapScores = conditionOptions
            .GroupBy(option => option.MapName)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(option => ConditionRiskScore(option.ConditionName)),
                StringComparer.OrdinalIgnoreCase);

        foreach (var record in rivenRecords)
        {
            AddMapScore(mapScores, "Riven Tides", record.RiskLevel.Equals("High", StringComparison.OrdinalIgnoreCase) ? 18 : 8);
        }

        foreach (var loadout in loadouts)
        {
            if (loadout.RiskLevel?.Equals("High", StringComparison.OrdinalIgnoreCase) == true)
            {
                AddMapScore(mapScores, loadout.MapOrEvent, 10);
            }
        }

        return mapScores
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key)
            .Take(6)
            .Select(pair => $"{pair.Key}: danger score {pair.Value}, risk notes {BuildMapRiskReason(pair.Key, conditionOptions, rivenRecords)}")
            .ToList();
    }

    private async Task<IReadOnlyList<string>> BuildAppSurfaceAsync(CancellationToken cancellationToken)
    {
        var counts = new[]
        {
            $"Inventory items: {await _db.InventoryItems.CountAsync(cancellationToken)}",
            $"Blueprints: {await _db.Blueprints.CountAsync(cancellationToken)}",
            $"Loadouts: {await _db.Loadouts.CountAsync(cancellationToken)}",
            $"Quests/Objectives: {await _db.Quests.CountAsync(cancellationToken)}",
            $"Intel guides: {await _db.IntelGuides.CountAsync(cancellationToken)}",
            $"Weekly trials: {await _db.WeeklyTrials.CountAsync(cancellationToken)}",
            $"Map condition options: {await _db.MapConditionOptions.CountAsync(cancellationToken)}",
            $"Skills: {await _db.Skills.CountAsync(cancellationToken)}"
        };

        return AppSurfacePages
            .Concat(counts)
            .ToList();
    }

    private static IReadOnlyList<string> BuildAppPageMatches(string message)
    {
        var matches = AppSurfacePages
            .Select(page => new
            {
                Page = page,
                Score = MatchScore(message, page)
            })
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Page)
            .Take(4)
            .Select(match => match.Page)
            .ToList();

        return matches;
    }

    private async Task<IReadOnlyList<string>> BuildUserPrioritiesAsync(CancellationToken cancellationToken)
    {
        var profile = await _db.PlayerProfiles
            .AsNoTracking()
            .OrderBy(profile => profile.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var topItems = await _db.InventoryItems
            .AsNoTracking()
            .Where(item => item.CurrentCount < item.KeepTarget)
            .OrderByDescending(item => item.Rarity == "Legendary")
            .ThenByDescending(item => item.Rarity == "Epic")
            .ThenByDescending(item => item.KeepTarget - item.CurrentCount)
            .Take(3)
            .Select(item => $"{item.Name}: need {item.KeepTarget - item.CurrentCount}, source {item.BestSource}")
            .ToListAsync(cancellationToken);
        var uncollectedBlueprints = await _db.Blueprints
            .AsNoTracking()
            .Where(blueprint => !blueprint.Collected)
            .OrderBy(blueprint => blueprint.CollectionStatus)
            .ThenBy(blueprint => blueprint.Name)
            .Take(3)
            .Select(blueprint => $"{blueprint.Name}: {blueprint.Category}, source {blueprint.WhereToGet ?? "unknown"}")
            .ToListAsync(cancellationToken);
        var highPriorityQuests = await _db.Quests
            .AsNoTracking()
            .Where(quest => quest.Status != "Completed")
            .OrderByDescending(quest => quest.Priority == "High")
            .ThenBy(quest => quest.Name)
            .Take(3)
            .Select(quest => $"{quest.Name}: {quest.RelatedActivity ?? "Any Map"}, {quest.Priority} priority")
            .ToListAsync(cancellationToken);
        var favorites = await _db.FavoriteItems
            .AsNoTracking()
            .OrderByDescending(favorite => favorite.CreatedAt)
            .Take(3)
            .Select(favorite => $"{favorite.DisplayName}: {favorite.ItemType} watchlist")
            .ToListAsync(cancellationToken);

        var priorities = new List<string>
        {
            $"Profile: {profile?.PlayerName ?? "Raider"}, {profile?.PreferredPlaystyle ?? "Balanced"} playstyle, default map {profile?.DefaultMap ?? "Dam Battlegrounds"}, {profile?.CurrentSkillPoints ?? 0} skill points"
        };

        priorities.AddRange(topItems.Select(item => $"Inventory priority - {item}"));
        priorities.AddRange(uncollectedBlueprints.Select(blueprint => $"Blueprint priority - {blueprint}"));
        priorities.AddRange(highPriorityQuests.Select(quest => $"Objective priority - {quest}"));
        priorities.AddRange(favorites.Select(favorite => $"Watchlist - {favorite}"));
        return priorities;
    }

    private static IReadOnlyList<ExternalKnowledgeMatch> BuildExternalKnowledge(string message)
    {
        var lower = message.ToLowerInvariant();
        var matches = ExternalKnowledgeSources
            .Select(source => new
            {
                Source = source,
                Score = Math.Max(
                    MatchScore(message, source.Title),
                    source.Keywords.Count(keyword => lower.Contains(keyword, StringComparison.OrdinalIgnoreCase)) * 20)
            })
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Source.Title)
            .Take(5)
            .Select(match => match.Source)
            .ToList();

        if (matches.Count == 0 && IsExternalKnowledgeQuestion(lower))
        {
            matches.AddRange(ExternalKnowledgeSources.Take(5));
        }

        return matches;
    }

    private async Task<IReadOnlyList<string>> BuildRelevantLoadoutsAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var loadouts = await _db.Loadouts
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return loadouts
            .Select(loadout => new
            {
                Loadout = loadout,
                Score = Math.Max(
                    MatchScore(message, loadout.Name),
                    Math.Max(MatchScore(message, loadout.ActivityType), MatchScore(message, loadout.FocusArea ?? string.Empty)))
            })
            .Where(match => match.Score > 0 || IsPvpLoadoutQuestion(message.ToLowerInvariant()) && IsPvpLoadout(match.Loadout))
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Loadout.Name)
            .Take(4)
            .Select(match => $"{match.Loadout.Name} ({match.Loadout.ActivityType}, {match.Loadout.MapOrEvent}, risk {match.Loadout.RiskLevel ?? "unknown"}): {match.Loadout.GearNotes ?? match.Loadout.Notes ?? "No gear notes"}")
            .ToList();
    }

    private async Task<IReadOnlyList<GameKnowledgeHit>> BuildRelevantGameKnowledgeAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var hits = new List<GameKnowledgeHit>();

        var items = await _db.InventoryItems
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        hits.AddRange(items.Select(item => BuildHit(
            "Item Database",
            item.Name,
            $"{item.Name} is a {item.Rarity} {item.Category}. Source: {item.BestSource}. Used for: {item.UsedFor}. Keep target: {item.KeepTarget}; current: {item.CurrentCount}; sell value: {item.SellValue}. {item.Notes}",
            $"/Database/Index",
            message,
            item.Name,
            item.Category,
            item.Rarity,
            item.BestSource,
            item.UsedFor,
            item.Notes ?? string.Empty)));

        var blueprints = await _db.Blueprints
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        hits.AddRange(blueprints.Select(blueprint => BuildHit(
            "Blueprints",
            blueprint.Name,
            $"{blueprint.Name} is a {blueprint.Category} blueprint. Status: {blueprint.CollectionStatus}. Source: {blueprint.WhereToGet ?? "Unknown"}. Recipe: {blueprint.RecipeMaterials ?? "Unknown"}. Notes: {blueprint.SourceNotes ?? blueprint.Notes ?? "None"}.",
            "/Blueprints/Index",
            message,
            blueprint.Name,
            blueprint.Category,
            blueprint.CollectionStatus,
            blueprint.WhereToGet ?? string.Empty,
            blueprint.RecipeMaterials ?? string.Empty,
            blueprint.SourceNotes ?? string.Empty,
            blueprint.Notes ?? string.Empty)));

        var quests = await _db.Quests
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        hits.AddRange(quests.Select(quest => BuildHit(
            "Objectives",
            quest.Name,
            $"{quest.Name}: status {quest.Status}, priority {quest.Priority}, activity {quest.RelatedActivity ?? "Any"}. Completion notes: {quest.CompletionNotes ?? quest.Notes ?? "None"}.",
            "/Quests/Index",
            message,
            quest.Name,
            quest.Status,
            quest.Priority,
            quest.RelatedActivity ?? string.Empty,
            quest.CompletionNotes ?? string.Empty,
            quest.Notes ?? string.Empty)));

        var trials = await _db.WeeklyTrials
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        hits.AddRange(trials.Select(trial => BuildHit(
            "Weekly Trials",
            trial.ObjectiveType,
            $"{trial.ObjectiveType}: target score {trial.TargetScore}, {trial.ScorePerAction} per action, {trial.ActionsNeeded} actions needed, best map {trial.BestMap}. Strategy: {trial.Strategy}.",
            "/Trials/Index",
            message,
            trial.ObjectiveType,
            trial.BestMap,
            trial.Strategy,
            trial.Notes ?? string.Empty)));

        var intel = await _db.IntelGuides
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        hits.AddRange(intel.Select(guide => BuildHit(
            "Intel Hub",
            guide.Name,
            $"{guide.Name}: {guide.GuideType} on {guide.MapName} during {guide.MapCondition}. Difficulty: {guide.Difficulty}. Route: {guide.RecommendedRoute}. Loot focus: {guide.LootFocus}. Risk: {guide.RiskWarning}.",
            "/Intel/Index",
            message,
            guide.Name,
            guide.GuideType,
            guide.MapName,
            guide.MapCondition,
            guide.Difficulty,
            guide.RecommendedRoute,
            guide.LootFocus,
            guide.RiskWarning,
            guide.Notes ?? string.Empty)));

        var loadouts = await _db.Loadouts
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        hits.AddRange(loadouts.Select(loadout => BuildHit(
            "Loadouts",
            loadout.Name,
            $"{loadout.Name}: {loadout.ActivityType} loadout for {loadout.MapOrEvent}. Focus: {loadout.FocusArea ?? "General"}. Risk: {loadout.RiskLevel ?? "Unknown"}. Gear: {loadout.GearNotes ?? loadout.Notes ?? "None"}.",
            "/Loadouts/Index",
            message,
            loadout.Name,
            loadout.ActivityType,
            loadout.MapOrEvent,
            loadout.FocusArea ?? string.Empty,
            loadout.RiskLevel ?? string.Empty,
            loadout.GearNotes ?? string.Empty,
            loadout.Notes ?? string.Empty)));

        var rivenRecords = await _db.RivenTidesRecords
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        hits.AddRange(rivenRecords.Select(record => BuildHit(
            "Riven Tides",
            record.Name,
            $"{record.Name}: {record.RecordType} in {record.Zone}. Risk: {record.RiskLevel}. Tool: {record.RecommendedTool ?? "None"}. Loot focus: {record.LootFocus ?? "None"}. Notes: {record.Notes ?? "None"}.",
            "/RivenTides/Index",
            message,
            record.Name,
            record.RecordType,
            record.Zone,
            record.RiskLevel,
            record.RecommendedTool ?? string.Empty,
            record.LootFocus ?? string.Empty,
            record.Notes ?? string.Empty)));

        var mapConditions = await _db.MapConditionOptions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        hits.AddRange(mapConditions.Select(condition => BuildHit(
            "Map Conditions",
            $"{condition.MapName} - {condition.ConditionName}",
            $"{condition.ConditionName} is tracked as a map condition option for {condition.MapName}. Risk score: {ConditionRiskScore(condition.ConditionName)}.",
            "/MapConditions/Index",
            message,
            condition.MapName,
            condition.ConditionName)));

        var skills = await _db.Skills
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        hits.AddRange(skills.Select(skill => BuildHit(
            "Skills",
            skill.Name,
            $"{skill.Name}: {skill.Branch} skill, max {skill.MaxPoints} points. {skill.Description} {skill.Details ?? string.Empty} Requires: {skill.Requires ?? "None"}.",
            "/Skills/Index",
            message,
            skill.Name,
            skill.Branch,
            skill.Description,
            skill.Details ?? string.Empty,
            skill.Requires ?? string.Empty)));

        var favorites = await _db.FavoriteItems
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        hits.AddRange(favorites.Select(favorite => BuildHit(
            "Watchlist",
            favorite.DisplayName,
            $"{favorite.DisplayName}: pinned {favorite.ItemType} watchlist item. Notes: {favorite.Notes ?? "None"}.",
            "/Favorites/Index",
            message,
            favorite.DisplayName,
            favorite.ItemType,
            favorite.Notes ?? string.Empty)));

        var profiles = await _db.PlayerProfiles
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        hits.AddRange(profiles.Select(profile => BuildHit(
            "Operator Profile",
            profile.PlayerName,
            $"{profile.PlayerName}: {profile.PreferredPlaystyle} playstyle, default map {profile.DefaultMap}, {profile.CurrentSkillPoints} skill points. Notes: {profile.Notes ?? "None"}.",
            "/Profile/Index",
            message,
            profile.PlayerName,
            profile.PreferredPlaystyle,
            profile.DefaultMap,
            profile.CurrentSkillPoints.ToString(),
            profile.Notes ?? string.Empty)));

        return hits
            .Where(hit => hit.Score > 0)
            .OrderByDescending(hit => hit.Score)
            .ThenBy(hit => hit.Source)
            .ThenBy(hit => hit.Title)
            .Take(8)
            .ToList();
    }

    private static string BuildBlueprintAnswer(BlueprintMatch blueprint, VaultContext context)
    {
        var builder = new StringBuilder();
        builder.Append($"{blueprint.Name} blueprint: {blueprint.WhereToGet}.");
        builder.Append($" Status in your tracker: {blueprint.Status}.");

        if (!string.IsNullOrWhiteSpace(blueprint.RecipeMaterials) &&
            !blueprint.RecipeMaterials.Contains("unknown", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append($" Crafting materials: {blueprint.RecipeMaterials}.");
        }

        if (!string.IsNullOrWhiteSpace(blueprint.BestCondition) ||
            !string.IsNullOrWhiteSpace(blueprint.ContainerType))
        {
            builder.Append($" App farm plan: run {ValueOrFallback(blueprint.BestCondition, "the matching condition")}");
            builder.Append($" on {ValueOrFallback(blueprint.BestMap, context.DefaultMap)},");
            builder.Append($" check {ValueOrFallback(blueprint.ContainerType, "the listed source containers")}");
            builder.Append($" around {ValueOrFallback(blueprint.BestAreas, "the target areas")}.");
        }

        if (!string.IsNullOrWhiteSpace(blueprint.FarmingRoute))
        {
            builder.Append($" Route note: {blueprint.FarmingRoute}");
        }

        return builder.ToString();
    }

    private static string BuildMapDangerAnswer(VaultContext context)
    {
        var topMap = context.MapRiskSummary.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(topMap))
        {
            return "I do not have enough map-risk data loaded yet. Use Map Conditions or Intel Hub to compare active conditions, route exposure, and event pressure.";
        }

        var builder = new StringBuilder();
        builder.Append($"Most dangerous map from the app data: {topMap}.");
        builder.Append(" The main danger signals are high-risk map conditions, event/boss pressure, exposed rotations, and routes that attract other raiders.");

        var riven = context.MapRiskSummary.FirstOrDefault(item => item.StartsWith("Riven Tides:", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(riven))
        {
            builder.Append($" Riven Tides note: {riven}");
            builder.Append(" Its open coastal sightlines, dockyard lanes, buried-loot traffic, and ARC Turbine prep make it one of the most punishing maps when contested.");
        }

        builder.Append(" For survival, bring smoke, a flexible mid-range weapon, shield recovery, and extract after the primary objective instead of stretching the route.");
        return builder.ToString();
    }

    private static string BuildArcThreatAnswer()
    {
        var topThreat = ArcThreats
            .OrderByDescending(threat => threat.DangerScore)
            .First();
        var runnerUp = ArcThreats
            .OrderByDescending(threat => threat.DangerScore)
            .Skip(1)
            .Take(3)
            .Select(threat => threat.Name);

        return $"Most dangerous ARC threat in the app data: {topThreat.Name}. {topThreat.WhyDangerous} Counter-plan: {topThreat.CounterPlan} Other high-danger ARC threats: {string.Join(", ", runnerUp)}. If you mean normal non-boss ARC, Leaper is the scariest because it punishes panic movement and bad spacing.";
    }

    private static string BuildPvpLoadoutAnswer(VaultContext context)
    {
        var builder = new StringBuilder();
        builder.Append($"For PvP, the app recommends {context.PvpLoadout}.");
        builder.Append(" Short answer: start with Tempest as the main PvP rifle because the app rates burst pressure and fight resets highest for player fights.");
        builder.Append(" Pair it with Il Toro for close-range pushes, or Vulcano on Night Raid/interior routes when you expect sudden close fights.");

        if (context.RelevantLoadouts.Count > 0)
        {
            builder.Append($" Saved loadout match: {context.RelevantLoadouts[0]}");
        }

        builder.Append(" Bring Heavy Shield, Combat Mk. 3 if available, Surge Shield Recharger, Adrenaline Shot, Smoke Grenade, and Tagging Grenade.");
        return builder.ToString();
    }

    private static string BuildPvpPveComparisonAnswer(VaultContext context)
    {
        var builder = new StringBuilder();
        builder.Append("PvP vs PvE from the app data: neither is universally better; the better format depends on your goal.");
        builder.Append($" PvP is better when you want player fights, contested loot, pressure, and faster fight resets. App PvP kit: {context.PvpLoadout}.");
        builder.Append($" PvE is better when you want safer progression, ARC clearing, quests, trials, inventory farming, and extraction consistency. App PvE kit: {context.PveLoadout}.");
        builder.Append($" For your current profile ({context.Playstyle} playstyle, default map {context.DefaultMap}), I would default to PvE/Balanced for progression runs and switch to PvP only when the objective is a fight, a contested condition, or defending high-value loot.");

        if (context.UserPriorities.Count > 0)
        {
            builder.Append($" Current app priority signal: {context.UserPriorities.First()}.");
        }

        return builder.ToString();
    }

    private static string BuildRivenTidesAnswer(VaultContext context)
    {
        var beachcombing = context.RelevantRivenTides
            .FirstOrDefault(record => record.Contains("Beachcombing", StringComparison.OrdinalIgnoreCase));
        var resort = context.RelevantRivenTides
            .FirstOrDefault(record => record.Contains("Panorama Azzurro", StringComparison.OrdinalIgnoreCase));
        var dockyard = context.RelevantRivenTides
            .FirstOrDefault(record => record.Contains("Dockyard", StringComparison.OrdinalIgnoreCase));
        var turbine = context.RelevantRivenTides
            .FirstOrDefault(record => record.Contains("ARC Turbine", StringComparison.OrdinalIgnoreCase));

        var builder = new StringBuilder();
        builder.Append("Best Riven Tides feature in the app data: Beachcombing.");

        if (!string.IsNullOrWhiteSpace(beachcombing))
        {
            builder.Append($" {beachcombing}");
        }

        builder.Append(" It is the most distinctive map loop because Dockmaster's Detector turns the beach into a buried-loot route with quick extraction planning instead of a normal container sweep.");

        if (!string.IsNullOrWhiteSpace(dockyard))
        {
            builder.Append($" Safer steady-loot option: {dockyard}");
        }

        if (!string.IsNullOrWhiteSpace(resort))
        {
            builder.Append($" High-risk feature: {resort}");
        }

        if (!string.IsNullOrWhiteSpace(turbine))
        {
            builder.Append($" Boss/prep feature: {turbine}");
        }

        return builder.ToString();
    }

    private static string BuildAppCoverageAnswer(VaultContext context)
    {
        var appPages = string.Join("; ", context.AppSurface.Take(AppSurfacePages.Count));
        var appCounts = string.Join("; ", context.AppSurface.Skip(AppSurfacePages.Count));
        var external = string.Join("; ", ExternalKnowledgeSources.Select(source => $"{source.Title}: {source.Url}"));

        return $"I can use the Raiders Vault app surface plus external references. App areas: {appPages}. Current local data coverage: {appCounts}. External source index: {external}. Ask about a page, item, blueprint, quest, trial, skill, map condition, MetaForge tool, or official ARC Raiders news/map condition and I will route to that source first.";
    }

    private static string BuildExternalKnowledgeAnswer(IReadOnlyList<ExternalKnowledgeMatch> matches)
    {
        var builder = new StringBuilder();
        builder.Append("I found these source areas to use: ");
        builder.Append(string.Join(" ", matches.Select(match => $"{match.Title}: {match.Summary} Source: {match.Url}")));
        return builder.ToString();
    }

    private static string BuildAppPageAnswer(VaultContext context)
    {
        var primary = context.AppPageMatches[0];
        var related = context.AppPageMatches
            .Skip(1)
            .Take(3)
            .ToList();

        var builder = new StringBuilder();
        builder.Append($"Use this app area: {primary}.");

        if (related.Count > 0)
        {
            builder.Append($" Related app areas: {string.Join("; ", related)}.");
        }

        builder.Append(" Ask a specific item, route, objective, skill, blueprint, condition, or saved loadout and I will search the matching app records first.");
        return builder.ToString();
    }

    private static string BuildPriorityAnswer(VaultContext context)
    {
        var builder = new StringBuilder();
        builder.Append("Current app-grounded priorities: ");
        builder.Append(string.Join(" ", context.UserPriorities.Take(8)));

        if (context.NeededItems.Count > 0)
        {
            builder.Append($" First run target: {context.NeededItems[0]}.");
        }

        if (context.BlueprintTargets.Count > 0)
        {
            builder.Append($" Blueprint side objective: {context.BlueprintTargets[0]}.");
        }

        if (context.WeeklyTrials.Count > 0)
        {
            builder.Append($" Trial option: {context.WeeklyTrials[0]}.");
        }

        builder.Append(" Best next move: pick one inventory target plus one overlapping blueprint/objective, then extract as soon as the primary check is done.");
        return builder.ToString();
    }

    private static string BuildConversationRepairAnswer() =>
        "I missed the mark there. Tell me the exact ARC Raiders question you want fixed, or paste the answer that looked wrong, and I will search the app data first instead of giving a generic farm plan.";

    private static string BuildGameKnowledgeAnswer(string message, VaultContext context)
    {
        var lower = message.ToLowerInvariant();
        var primary = context.GameKnowledge[0];
        var related = context.GameKnowledge
            .Skip(1)
            .Take(3)
            .Select(hit => $"{hit.Title} ({hit.Source})")
            .ToList();
        var grouped = context.GameKnowledge
            .GroupBy(hit => hit.Source)
            .OrderByDescending(group => group.Max(hit => hit.Score))
            .Take(4)
            .Select(group => $"{group.Key}: {string.Join(", ", group.Take(2).Select(hit => hit.Title))}")
            .ToList();

        var builder = new StringBuilder();
        if (lower.Contains("compare") || lower.Contains("difference") || lower.Contains("versus") || lower.Contains(" vs "))
        {
            builder.Append($"Comparison from app data: {string.Join(" | ", context.GameKnowledge.Take(4).Select(hit => $"{hit.Title} ({hit.Source}): {hit.Summary}"))}");
        }
        else if (lower.Contains("list") || lower.Contains("show") || lower.Contains("all ") || lower.Contains("options"))
        {
            builder.Append($"Best matching app records: {string.Join(" | ", grouped)}.");
        }
        else if (lower.Contains("where") || lower.Contains("get") || lower.Contains("find") || lower.Contains("farm") || lower.Contains("source"))
        {
            builder.Append($"Best app match for where to get it: {primary.Title} from {primary.Source}. {primary.Summary}");
        }
        else if (lower.Contains("counter") || lower.Contains("beat") || lower.Contains("kill") || lower.Contains("survive") || lower.Contains("avoid"))
        {
            builder.Append($"Counter-plan from app data: use {primary.Title} from {primary.Source}. {primary.Summary}");
        }
        else if (lower.Contains("best") || lower.Contains("should") || lower.Contains("recommend") || lower.Contains("priority"))
        {
            builder.Append($"Best app-grounded answer: start with {primary.Title} from {primary.Source}. {primary.Summary}");
        }
        else if (lower.Contains("how") || lower.Contains("plan") || lower.Contains("route"))
        {
            builder.Append($"Use this app record as the plan anchor: {primary.Title} from {primary.Source}. {primary.Summary}");
        }
        else
        {
            builder.Append($"From the app data: {primary.Title} ({primary.Source}). {primary.Summary}");
        }

        if (related.Count > 0)
        {
            builder.Append($" Related matches: {string.Join("; ", related)}.");
        }

        builder.Append($" Check {primary.PagePath} in the app for the full record.");
        return builder.ToString();
    }

    private static string FormatBlueprintMatch(BlueprintMatch blueprint) =>
        $"{blueprint.Name} ({blueprint.Category}, {blueprint.Status}, source {blueprint.WhereToGet}, recipe {blueprint.RecipeMaterials}, plan {blueprint.BestCondition} / {blueprint.ContainerType} / {blueprint.BestAreas})";

    private static string FormatExternalKnowledge(ExternalKnowledgeMatch match) =>
        $"{match.Title} ({match.Url}): {match.Summary}";

    private static string FormatGameKnowledgeHit(GameKnowledgeHit hit) =>
        $"{hit.Source} / {hit.Title}: {hit.Summary}";

    private static string FormatLoadout(RaidersVault.ViewModels.OptimalLoadoutViewModel loadout) =>
        $"Primary {loadout.PrimaryWeapon}; Secondary {loadout.SecondaryWeapon}; Shield {loadout.Shield}; Augment {loadout.Augment}; Quick use {string.Join(", ", loadout.QuickUseItems)}; Why {string.Join(" ", loadout.WhyThisLoadout)}";

    private static string FormatArcThreat(ArcThreat threat) =>
        $"{threat.Name}: danger score {threat.DangerScore}, {threat.WhyDangerous}, counter {threat.CounterPlan}";

    private static bool IsPvpLoadoutQuestion(string lower) =>
        (lower.Contains("pvp") || lower.Contains("player") || lower.Contains("fight") || lower.Contains("combat"))
        && (lower.Contains("gun") || lower.Contains("weapon") || lower.Contains("loadout") || lower.Contains("kit") || lower.Contains("best"));

    private static bool IsConversationRepairMessage(string lower)
    {
        var compact = Compact(lower);
        return lower.Contains("are you dumb")
            || lower.Contains("you dumb")
            || lower.Contains("stupid")
            || lower.Contains("idiot")
            || lower.Contains("that is wrong")
            || lower.Contains("that's wrong")
            || lower.Contains("still wrong")
            || lower.Contains("not right")
            || lower.Contains("not what i asked")
            || lower.Contains("bad answer")
            || lower.Contains("terrible answer")
            || lower.Contains("you are wrong")
            || compact.Contains("youredumb", StringComparison.OrdinalIgnoreCase)
            || compact.Contains("youaredumb", StringComparison.OrdinalIgnoreCase)
            || compact.Contains("youdumb", StringComparison.OrdinalIgnoreCase)
            || compact.Contains("thatswrong", StringComparison.OrdinalIgnoreCase)
            || compact.Contains("notwhatasked", StringComparison.OrdinalIgnoreCase)
            || compact.Contains("badanswer", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPvpPveComparisonQuestion(string lower) =>
        (lower.Contains("pvp") || lower.Contains("player versus player"))
        && (lower.Contains("pve") || lower.Contains("player versus environment"))
        && (lower.Contains("better")
            || lower.Contains("best")
            || lower.Contains("difference")
            || lower.Contains("compare")
            || lower.Contains("versus")
            || lower.Contains(" vs ")
            || lower.Contains("format")
            || lower.Contains("mode"));

    private static bool IsAppCoverageQuestion(string lower) =>
        lower.Contains("entire app")
        || lower.Contains("whole app")
        || lower.Contains("cover the app")
        || lower.Contains("what can you answer")
        || lower.Contains("what do you know");

    private static bool IsExternalKnowledgeQuestion(string lower) =>
        lower.Contains("metaforge")
        || lower.Contains("meta forge")
        || lower.Contains("official site")
        || lower.Contains("arc raiders website")
        || lower.Contains("arc website")
        || lower.Contains("latest news")
        || lower.Contains("map condition")
        || lower.Contains("patch note")
        || lower.Contains("guide")
        || lower.Contains("interactive map")
        || lower.Contains("marketplace");

    private static bool IsAppPageQuestion(string lower) =>
        lower.Contains("page")
        || lower.Contains("screen")
        || lower.Contains("tab")
        || lower.Contains("section")
        || lower.Contains("tool")
        || lower.Contains("what can i do")
        || lower.Contains("how do i use");

    private static bool IsMapFeatureQuestion(string lower) =>
        (lower.Contains("map") || lower.Contains("riven") || lower.Contains("tides"))
        && (lower.Contains("feature") || lower.Contains("best") || lower.Contains("special") || lower.Contains("unique") || lower.Contains("good"));

    private static bool IsArcThreatQuestion(string lower) =>
        (lower.Contains("arc") || lower.Contains("enemy") || lower.Contains("enemies") || lower.Contains("threat"))
        && (lower.Contains("dangerous") || lower.Contains("deadliest") || lower.Contains("hardest") || lower.Contains("strongest") || lower.Contains("scariest") || lower.Contains("worst"));

    private static bool IsMapDangerQuestion(string lower) =>
        lower.Contains("most dangerous")
        || lower.Contains("riskiest")
        || lower.Contains("hardest map")
        || lower.Contains("dangerous map")
        || lower.Contains("highest risk")
        || lower.Contains("most risky");

    private static bool IsGameKnowledgeQuestion(string lower) =>
        lower.Contains("arc")
        || lower.Contains("raider")
        || lower.Contains("map")
        || lower.Contains("item")
        || lower.Contains("loot")
        || lower.Contains("farm")
        || lower.Contains("quest")
        || lower.Contains("objective")
        || lower.Contains("trial")
        || lower.Contains("skill")
        || lower.Contains("blueprint")
        || lower.Contains("weapon")
        || lower.Contains("gun")
        || lower.Contains("loadout")
        || lower.Contains("condition")
        || lower.Contains("riven")
        || lower.Contains("dam")
        || lower.Contains("spaceport")
        || lower.Contains("buried")
        || lower.Contains("gate")
        || lower.Contains("stella")
        || lower.Contains("where")
        || lower.Contains("what")
        || lower.Contains("how")
        || lower.Contains("best");

    private static bool IsPriorityQuestion(string lower) =>
        lower.Contains("what should i do")
        || lower.Contains("next")
        || lower.Contains("priority")
        || lower.Contains("priorities")
        || lower.Contains("focus")
        || lower.Contains("progress")
        || lower.Contains("status")
        || lower.Contains("recommend a run");

    private static bool IsPvpLoadout(Loadout loadout) =>
        loadout.ActivityType.Contains("pvp", StringComparison.OrdinalIgnoreCase)
        || loadout.FocusArea?.Contains("pvp", StringComparison.OrdinalIgnoreCase) == true
        || loadout.Name.Contains("pvp", StringComparison.OrdinalIgnoreCase);

    private static string DetectCondition(string message)
    {
        var lower = message.ToLowerInvariant();
        if (lower.Contains("night"))
        {
            return "Night Raid";
        }

        if (lower.Contains("storm") || lower.Contains("electromagnetic"))
        {
            return "Electromagnetic Storm";
        }

        if (lower.Contains("hurricane"))
        {
            return "Hurricane";
        }

        if (lower.Contains("bunker"))
        {
            return "Hidden Bunker";
        }

        if (lower.Contains("gate"))
        {
            return "Locked Gate";
        }

        return "Standard";
    }

    private static int ConditionRiskScore(string condition) =>
        condition switch
        {
            "Matriarch" or "Harvester" or "ARC Turbine" => 24,
            "Close Scrutiny" or "Hidden Bunker" or "Locked Gate" or "Night Raid" => 18,
            "Hurricane" or "Cold Snap" or "Electromagnetic Storm" => 15,
            "Beachcombing" or "Prospecting Probes" or "Launch Tower Loot" or "Last Resort Event" or "Uncovered Caches" => 11,
            "Standard" or "Standard Patrol" => 4,
            _ => 8
        };

    private static void AddMapScore(Dictionary<string, int> scores, string map, int score)
    {
        if (scores.ContainsKey(map))
        {
            scores[map] += score;
            return;
        }

        scores[map] = score;
    }

    private static string BuildMapRiskReason(
        string map,
        IReadOnlyCollection<MapConditionOption> conditionOptions,
        IReadOnlyCollection<RivenTidesRecord> rivenRecords)
    {
        var highConditions = conditionOptions
            .Where(option => option.MapName.Equals(map, StringComparison.OrdinalIgnoreCase))
            .Where(option => ConditionRiskScore(option.ConditionName) >= 15)
            .Select(option => option.ConditionName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();

        if (map.Equals("Riven Tides", StringComparison.OrdinalIgnoreCase))
        {
            var highRecords = rivenRecords
                .Where(record => record.RiskLevel.Equals("High", StringComparison.OrdinalIgnoreCase))
                .Select(record => record.Name)
                .Take(3);
            highConditions.AddRange(highRecords);
        }

        return highConditions.Count == 0
            ? "mostly standard route risk"
            : string.Join(", ", highConditions);
    }

    private static int MatchScore(string message, string candidate)
    {
        var messageCompact = Compact(message);
        var candidateCompact = Compact(candidate);

        if (string.IsNullOrWhiteSpace(candidateCompact))
        {
            return 0;
        }

        if (messageCompact.Contains(candidateCompact, StringComparison.OrdinalIgnoreCase))
        {
            return 100 + candidateCompact.Length;
        }

        var candidateTokens = Tokenize(candidate);
        if (candidateTokens.Count == 0)
        {
            return 0;
        }

        var messageTokens = Tokenize(message);
        var matches = candidateTokens.Count(messageTokens.Contains);
        if (matches == candidateTokens.Count)
        {
            return 70 + matches;
        }

        return matches > 0 && candidateTokens.Any(token => token.Length >= 5 && messageTokens.Contains(token))
            ? 35 + matches
            : 0;
    }

    private static GameKnowledgeHit BuildHit(
        string source,
        string title,
        string summary,
        string pagePath,
        string message,
        params string[] searchableParts)
    {
        var score = searchableParts
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => MatchScore(message, part))
            .DefaultIfEmpty(0)
            .Max();

        var messageTokens = Tokenize(message);
        var combinedTokens = searchableParts
            .SelectMany(Tokenize)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var overlap = messageTokens.Count(combinedTokens.Contains);

        if (overlap > 0)
        {
            score += overlap * 8;
        }

        score += SourceIntentBoost(message, source);

        return new GameKnowledgeHit(source, title, summary, pagePath, score);
    }

    private static int SourceIntentBoost(string message, string source)
    {
        var lower = message.ToLowerInvariant();
        return source switch
        {
            "Item Database" when lower.Contains("item") || lower.Contains("loot") || lower.Contains("material") || lower.Contains("source") || lower.Contains("farm") => 24,
            "Blueprints" when lower.Contains("blueprint") || lower.Contains("recipe") || lower.Contains("craft") => 28,
            "Objectives" when lower.Contains("quest") || lower.Contains("objective") || lower.Contains("mission") || lower.Contains("complete") || lower.Contains("finish") => 28,
            "Weekly Trials" when lower.Contains("trial") || lower.Contains("weekly") || lower.Contains("score") || lower.Contains("three star") => 28,
            "Intel Hub" when lower.Contains("intel") || lower.Contains("route") || lower.Contains("safe") || lower.Contains("risk") => 24,
            "Loadouts" when lower.Contains("loadout") || lower.Contains("kit") || lower.Contains("gear") || lower.Contains("weapon") || lower.Contains("gun") => 24,
            "Riven Tides" when lower.Contains("riven") || lower.Contains("tides") || lower.Contains("beach") || lower.Contains("dock") || lower.Contains("resort") => 28,
            "Map Conditions" when lower.Contains("condition") || lower.Contains("map") || lower.Contains("night") || lower.Contains("storm") || lower.Contains("bunker") => 20,
            "Skills" when lower.Contains("skill") || lower.Contains("perk") || lower.Contains("tree") || lower.Contains("points") => 28,
            "Watchlist" when lower.Contains("watchlist") || lower.Contains("favorite") || lower.Contains("pinned") => 28,
            "Operator Profile" when lower.Contains("profile") || lower.Contains("playstyle") || lower.Contains("skill points") => 28,
            _ => 0
        };
    }

    private static HashSet<string> Tokenize(string value) =>
        value
            .ToLowerInvariant()
            .Split(
                new[] { ' ', '-', '.', '/', '\\', '_', ':', ';', ',', '?', '!', '\'', '"', '(', ')', '[', ']' },
                StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizeToken)
            .Where(token => token.Length > 2 && !IgnoredTokens.Contains(token))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static string NormalizeToken(string token)
    {
        if (token.EndsWith("ies", StringComparison.OrdinalIgnoreCase) && token.Length > 4)
        {
            return token[..^3] + "y";
        }

        if (token.EndsWith("es", StringComparison.OrdinalIgnoreCase) && token.Length > 4)
        {
            return token[..^2];
        }

        if (token.EndsWith("s", StringComparison.OrdinalIgnoreCase) && token.Length > 4)
        {
            return token[..^1];
        }

        return token;
    }

    private static string Compact(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString()
            .Replace("blueprint", string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static string ValueOrFallback(string value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static IReadOnlyList<string> BuildSuggestions() =>
        new[]
        {
            "What should I farm next?",
            "Build me a safe route.",
            "Which blueprint should I chase?",
            "Plan my weekly trial run."
        };

    private static string? ExtractResponseText(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        if (document.RootElement.TryGetProperty("output_text", out var outputText))
        {
            return outputText.GetString();
        }

        if (!document.RootElement.TryGetProperty("output", out var output) ||
            output.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) ||
                content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var text))
                {
                    return text.GetString();
                }
            }
        }

        return null;
    }

    private sealed record VaultContext(
        string UserName,
        string CurrentPage,
        string PlayerName,
        string Playstyle,
        string DefaultMap,
        int SkillPoints,
        IReadOnlyList<string> NeededItems,
        IReadOnlyList<string> BlueprintTargets,
        IReadOnlyList<string> WeeklyTrials,
        IReadOnlyList<BlueprintMatch> RelevantBlueprints,
        IReadOnlyList<string> RelevantItems,
        IReadOnlyList<string> RelevantIntel,
        IReadOnlyList<string> RelevantRivenTides,
        IReadOnlyList<string> MapRiskSummary,
        IReadOnlyList<string> AppSurface,
        IReadOnlyList<string> AppPageMatches,
        IReadOnlyList<string> UserPriorities,
        IReadOnlyList<ExternalKnowledgeMatch> ExternalKnowledge,
        string PvpLoadout,
        string PveLoadout,
        IReadOnlyList<string> RelevantLoadouts,
        IReadOnlyList<GameKnowledgeHit> GameKnowledge);

    private sealed record BlueprintMatch(
        string Name,
        string Category,
        string Status,
        string WhereToGet,
        string RecipeMaterials,
        string SourceNotes,
        string Notes,
        string BestMap,
        string BestCondition,
        string ContainerType,
        string BestAreas,
        string FarmingRoute,
        string ProbabilityLabel);

    private sealed record ExternalKnowledgeMatch(
        string Title,
        string Url,
        string Summary,
        IReadOnlyList<string> Keywords);

    private sealed record ArcThreat(
        string Name,
        int DangerScore,
        string WhyDangerous,
        string CounterPlan);

    private sealed record GameKnowledgeHit(
        string Source,
        string Title,
        string Summary,
        string PagePath,
        int Score);

    private static readonly IReadOnlyList<string> AppSurfacePages = new[]
    {
        "Command Center: dashboard, saved activity, global readiness shortcuts",
        "Global Ops: official map conditions, live operations, regional readiness, CSV/API exports",
        "AI Assistant: app-grounded chat over vault data and external source index",
        "Run Planner: route, kit, blueprint target, skill, and extraction planning",
        "Loadouts: saved tactical kits and PvP/PvE/Balanced setups",
        "Map Conditions: condition-aware loadouts, blueprint alerts, route recommendations",
        "Intel Hub: tactical route notes, map-condition farms, risk warnings",
        "Item Database: item icons, keep targets, sources, crafting uses, loot plan export",
        "Weekly Trials: score targets, maps, and strategy notes",
        "Objectives: quest tracking, priorities, and completion notes",
        "Blueprints: collection tracker, farm plans, recipes, map/condition/source logic",
        "Skills: skill tree planning and recommendations",
        "Watchlist: pinned favorites and priority tracking",
        "Operator Profile: playstyle, default map, skill points, notes",
        "Analytics: reporting across inventory, quests, blueprints, loadouts, and progress",
        "Admin Center: health, capabilities, integrations, repair and platform status"
    };

    private static readonly IReadOnlyList<ExternalKnowledgeMatch> ExternalKnowledgeSources = new[]
    {
        new ExternalKnowledgeMatch(
            "Official ARC Raiders site",
            "https://arcraiders.com/",
            "Official ARC Raiders home, platform links, game overview, latest news, and current map-condition cards.",
            new[] { "official", "arc", "arc raiders", "website", "news", "map condition", "platform", "trailer" }),
        new ExternalKnowledgeMatch(
            "Official ARC Raiders map conditions",
            "https://arcraiders.com/",
            "The official site surfaces active map conditions and latest news cards; use it when the user asks what is active now.",
            new[] { "map condition", "active", "today", "official", "live", "current" }),
        new ExternalKnowledgeMatch(
            "MetaForge ARC Raiders hub",
            "https://metaforge.app/arc-raiders",
            "MetaForge hub for ARC Raiders guides, database, maps, marketplace, profile, trackers, skill tree, weekly trials, API/tooltips, and overlay app.",
            new[] { "metaforge", "meta forge", "hub", "database", "maps", "guides", "tracker", "overlay" }),
        new ExternalKnowledgeMatch(
            "MetaForge items database",
            "https://metaforge.app/arc-raiders/database/items",
            "External item database reference for ARC Raiders items, crafting, value, and item detail lookups.",
            new[] { "metaforge", "items", "database", "crafting", "value", "item" }),
        new ExternalKnowledgeMatch(
            "MetaForge maps",
            "https://metaforge.app/arc-raiders/maps",
            "Interactive map entry point for Dam Battlegrounds, Spaceport, Buried City, Blue Gate, Stella Montis, and Riven Tides.",
            new[] { "metaforge", "map", "interactive map", "dam", "spaceport", "buried", "blue gate", "stella", "riven" }),
        new ExternalKnowledgeMatch(
            "MetaForge blueprint tracker",
            "https://metaforge.app/arc-raiders/blueprint-tracker",
            "External blueprint tracking reference for collection planning and blueprint progress.",
            new[] { "metaforge", "blueprint", "tracker", "collection" }),
        new ExternalKnowledgeMatch(
            "MetaForge weekly trials",
            "https://metaforge.app/arc-raiders/weekly-trials",
            "External weekly-trials reference for current trial guides and score routing.",
            new[] { "metaforge", "weekly", "trials", "score", "guide" }),
        new ExternalKnowledgeMatch(
            "MetaForge skill tree",
            "https://metaforge.app/arc-raiders/skill-tree",
            "External skill-tree and build-planning reference.",
            new[] { "metaforge", "skill", "skills", "build", "tree" })
    };

    private static readonly IReadOnlyList<ArcThreat> ArcThreats = new[]
    {
        new ArcThreat(
            "Matriarch",
            100,
            "Boss/event ARC pressure tied to Matriarch routes, boss reward containers, heavy damage, and major attention pull.",
            "Bring high damage, extra ammo, healing, shield sustain, and a planned extract before committing."),
        new ArcThreat(
            "Harvester",
            96,
            "Major ARC event route that drains supplies, pulls attention, and is tied to legendary crafting paths.",
            "Only chase when active, bring ARC-damage tools, extra ammo, healing, and leave once the objective is secured."),
        new ArcThreat(
            "ARC Turbine",
            90,
            "Riven Tides boss/prep threat with open coast and vertical sightline risk.",
            "Use EMP, shield battery, ranged primary, and avoid overpacking rare gear while scouting."),
        new ArcThreat(
            "Leaper",
            84,
            "Punishes panic sprays, bad spacing, and tight-room fights; the app quests call out aiming where it lands.",
            "Kite into open sightlines, hold cover, aim at the landing point, and avoid fighting it inside cramped rooms."),
        new ArcThreat(
            "Bombardier",
            78,
            "Burst-pattern ARC that can punish open peeks and greedy looting.",
            "Wait out its burst pattern, punish after the burst, loot fast, and rotate."),
        new ArcThreat(
            "Rocketeer",
            76,
            "Rocket pressure punishes open yards and long peeks.",
            "Do not peek while it spools rockets; wait the burst, then punish from cover."),
        new ArcThreat(
            "Surveyor",
            72,
            "Punishes greed and can turn a simple loot action into a bad rotation.",
            "Loot the vault or objective quickly, then rotate instead of doubling back."),
        new ArcThreat(
            "Hornet",
            60,
            "Airborne ARC that stalls and makes grenade timing and anti-air aim matter.",
            "Use long sightlines, time explosives where it pauses, and keep ammo economy clean."),
        new ArcThreat(
            "Wasp / Turret",
            45,
            "Small ARC pressure that is dangerous when ignored or stacked with other threats.",
            "Clear quickly; relevant skills can make Wasps and Turrets easier to destroy.")
    };

    private static readonly HashSet<string> IgnoredTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "where",
        "what",
        "when",
        "can",
        "get",
        "find",
        "farm",
        "blueprint",
        "item",
        "best",
        "better",
        "gun",
        "weapon",
        "loadout",
        "kit",
        "the",
        "for",
        "from",
        "with",
        "and",
        "how",
        "does"
    };
}
