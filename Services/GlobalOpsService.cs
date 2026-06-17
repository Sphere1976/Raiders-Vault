using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;
using RaidersVault.ViewModels;

namespace RaidersVault.Services;

public class GlobalOpsService
{
    private readonly RaidersVaultContext _db;
    private readonly ArcRaidersLiveOpsService _liveOpsService;

    public GlobalOpsService(
        RaidersVaultContext db,
        ArcRaidersLiveOpsService liveOpsService)
    {
        _db = db;
        _liveOpsService = liveOpsService;
    }

    public async Task<GlobalOpsViewModel> BuildDashboardAsync()
    {
        var profile = await _db.PlayerProfiles.FirstOrDefaultAsync();
        var quests = await _db.Quests.ToListAsync();
        var blueprints = await _db.Blueprints.ToListAsync();
        var inventory = await _db.InventoryItems.ToListAsync();
        var weeklyTrials = await _db.WeeklyTrials.ToListAsync();
        var liveOps = await _liveOpsService.BuildAsync();

        var completedQuests = quests.Count(q =>
            q.Status.Equals("Complete", StringComparison.OrdinalIgnoreCase) ||
            q.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));

        var collectedBlueprints = blueprints.Count(IsCollected);
        var stockedItems = inventory.Count(x => x.Needed == 0);
        var totalVaultValue = inventory.Sum(x => x.CurrentCount * x.SellValue);
        var criticalGaps = inventory.Count(x => x.Priority == "Critical");
        var missingHighValueBlueprints = blueprints.Count(x =>
            !IsCollected(x) &&
            (x.Category == "Weapon" || x.Category == "Augment"));

        var questPercent = Percentage(completedQuests, quests.Count);
        var blueprintPercent = Percentage(collectedBlueprints, blueprints.Count);
        var inventoryPercent = Percentage(stockedItems, inventory.Count);
        var riskSignals = criticalGaps + missingHighValueBlueprints;

        return new GlobalOpsViewModel
        {
            PlayerName = profile?.PlayerName ?? "Raider",
            PreferredPlaystyle = profile?.PreferredPlaystyle ?? "Balanced",
            DefaultMap = profile?.DefaultMap ?? "Dam Battlegrounds",
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            QuestCompletionPercent = questPercent,
            BlueprintCompletionPercent = blueprintPercent,
            InventoryReadinessPercent = inventoryPercent,
            OverallReadinessPercent = (int)Math.Round((questPercent + blueprintPercent + inventoryPercent) / 3d),
            ReadinessTier = BuildReadinessTier(questPercent, blueprintPercent, inventoryPercent, riskSignals),
            TotalVaultValue = totalVaultValue,
            ActiveRiskSignals = riskSignals,
            Regions = BuildRegions(profile?.PreferredPlaystyle ?? "Balanced", profile?.DefaultMap ?? "Dam Battlegrounds"),
            NeededItems = BuildNeededItems(inventory),
            MarketSignals = BuildMarketSignals(inventory),
            BlueprintTargets = BuildBlueprintTargets(blueprints),
            TrialSignals = BuildTrialSignals(weeklyTrials),
            LocalizationSignals = BuildLocalizationSignals(),
            LiveOps = liveOps,
            Capabilities = BuildCapabilities()
        };
    }

    public string BuildCsv(GlobalOpsViewModel dashboard)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Section,Name,Category,Signal,Source,Score");

        foreach (var item in dashboard.MarketSignals)
        {
            AppendRow(csv, "Marketplace", item.Name, item.Category, item.Signal, item.Source, item.Score.ToString(CultureInfo.InvariantCulture));
        }

        foreach (var item in dashboard.NeededItems)
        {
            AppendRow(csv, "Needed Items", item.Name, item.Category, item.Signal, item.Source, item.Score.ToString(CultureInfo.InvariantCulture));
        }

        foreach (var item in dashboard.BlueprintTargets)
        {
            AppendRow(csv, "Blueprint Targets", item.Name, item.Category, item.Signal, item.Source, item.Score.ToString(CultureInfo.InvariantCulture));
        }

        foreach (var item in dashboard.TrialSignals)
        {
            AppendRow(csv, "Weekly Trials", item.Name, item.Category, item.Signal, item.Source, item.Score.ToString(CultureInfo.InvariantCulture));
        }

        foreach (var region in dashboard.Regions)
        {
            AppendRow(csv, "Regions", region.Region, region.Locale, region.RecommendedFocus, region.PrimeWindow, region.RiskLevel);
        }

        return csv.ToString();
    }

    private static List<MarketInsightItem> BuildNeededItems(IEnumerable<InventoryItem> inventory)
    {
        return inventory
            .Where(x => x.Needed > 0)
            .OrderByDescending(x => x.Priority == "Critical")
            .ThenByDescending(x => x.Needed)
            .ThenByDescending(x => x.SellValue)
            .Take(6)
            .Select(x => new MarketInsightItem
            {
                Name = x.Name,
                Category = x.Category,
                Signal = $"{x.Needed} short of keep target",
                Source = x.BestSource,
                Score = x.SellValue * Math.Max(x.Needed, 1)
            })
            .ToList();
    }

    private static List<MarketInsightItem> BuildMarketSignals(IEnumerable<InventoryItem> inventory)
    {
        return inventory
            .OrderByDescending(x => x.Favorite)
            .ThenByDescending(x => x.SellValue)
            .ThenByDescending(x => x.Needed)
            .Take(8)
            .Select(x => new MarketInsightItem
            {
                Name = x.Name,
                Category = x.Rarity,
                Signal = x.Needed > 0 ? "Hold and farm" : "Surplus watch",
                Source = x.UsedFor,
                Score = x.SellValue
            })
            .ToList();
    }

    private static List<MarketInsightItem> BuildBlueprintTargets(IEnumerable<Blueprint> blueprints)
    {
        return blueprints
            .Where(x => !IsCollected(x))
            .OrderByDescending(x => x.Category == "Weapon" || x.Category == "Augment")
            .ThenByDescending(x => ConditionWeight(x.WhereToGet))
            .ThenBy(x => x.Name)
            .Take(8)
            .Select(x => new MarketInsightItem
            {
                Name = x.Name,
                Category = x.Category,
                Signal = BuildBlueprintSignal(x.WhereToGet),
                Source = x.WhereToGet ?? "Any Map",
                Score = 50 + ConditionWeight(x.WhereToGet)
            })
            .ToList();
    }

    private static List<MarketInsightItem> BuildTrialSignals(IEnumerable<WeeklyTrial> weeklyTrials)
    {
        return weeklyTrials
            .OrderByDescending(x => x.TargetScore)
            .ThenBy(x => x.ActionsNeeded)
            .Take(5)
            .Select(x => new MarketInsightItem
            {
                Name = x.Name,
                Category = x.ObjectiveType,
                Signal = $"{x.ActionsNeeded} actions for {x.TargetScore} score",
                Source = x.BestMap,
                Score = x.TargetScore
            })
            .ToList();
    }

    private static string BuildBlueprintSignal(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return "General pool";
        }

        if (source.Contains("Night", StringComparison.OrdinalIgnoreCase) ||
            source.Contains("Storm", StringComparison.OrdinalIgnoreCase) ||
            source.Contains("Hurricane", StringComparison.OrdinalIgnoreCase))
        {
            return "Condition-gated farm";
        }

        if (source.Contains("Quest", StringComparison.OrdinalIgnoreCase))
        {
            return "Quest route";
        }

        return "Standard farm";
    }

    private static int ConditionWeight(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return 0;
        }

        var score = 0;
        if (source.Contains("Hurricane", StringComparison.OrdinalIgnoreCase)) score += 35;
        if (source.Contains("Night", StringComparison.OrdinalIgnoreCase)) score += 25;
        if (source.Contains("Storm", StringComparison.OrdinalIgnoreCase)) score += 20;
        if (source.Contains("Locked", StringComparison.OrdinalIgnoreCase)) score += 15;
        if (source.Contains("Quest", StringComparison.OrdinalIgnoreCase)) score += 10;

        return score;
    }

    private static bool IsCollected(Blueprint blueprint)
    {
        return blueprint.Collected ||
               blueprint.CollectionStatus.Equals("Collected", StringComparison.OrdinalIgnoreCase) ||
               blueprint.CollectionStatus.Equals("Obtained", StringComparison.OrdinalIgnoreCase);
    }

    private static int Percentage(int value, int total)
    {
        return total == 0 ? 0 : (int)Math.Round(value / (double)total * 100);
    }

    private static string BuildReadinessTier(
        int questPercent,
        int blueprintPercent,
        int inventoryPercent,
        int riskSignals)
    {
        var average = (questPercent + blueprintPercent + inventoryPercent) / 3d;

        if (average >= 80 && riskSignals <= 5) return "Enterprise Ready";
        if (average >= 55 && riskSignals <= 20) return "Scaling";
        if (average >= 35) return "Operational";
        return "Needs Attention";
    }

    private static List<RegionOpsCard> BuildRegions(string playstyle, string defaultMap)
    {
        var now = DateTimeOffset.UtcNow;

        return new List<RegionOpsCard>
        {
            BuildRegion("Americas", "en-US / es-US", now, -4, "18:00-23:00", defaultMap, playstyle, "Medium"),
            BuildRegion("Europe", "en-GB / de-DE / fr-FR", now, 1, "19:00-00:00", "Buried City", "Balanced", "High"),
            BuildRegion("Asia Pacific", "ja-JP / ko-KR / zh-CN", now, 9, "20:00-01:00", "Spaceport", "PvE", "Medium"),
            BuildRegion("Oceania", "en-AU", now, 10, "18:00-22:00", "Riven Tides", "Blueprint Farming", "Variable")
        };
    }

    private static RegionOpsCard BuildRegion(
        string region,
        string locale,
        DateTimeOffset utcNow,
        int utcOffset,
        string primeWindow,
        string map,
        string focus,
        string riskLevel)
    {
        return new RegionOpsCard
        {
            Region = region,
            Locale = locale,
            LocalTime = utcNow.ToOffset(TimeSpan.FromHours(utcOffset)).ToString("HH:mm"),
            PrimeWindow = primeWindow,
            RecommendedFocus = $"{focus} on {map}",
            RiskLevel = riskLevel
        };
    }

    private static List<LocalizationSignal> BuildLocalizationSignals()
    {
        return new List<LocalizationSignal>
        {
            new() { Region = "Americas", Languages = "English, Spanish", Currency = "USD", SupportWindow = "18:00-02:00 UTC" },
            new() { Region = "Europe", Languages = "English, German, French", Currency = "EUR", SupportWindow = "16:00-00:00 UTC" },
            new() { Region = "Asia Pacific", Languages = "Japanese, Korean, Chinese", Currency = "Regional", SupportWindow = "10:00-18:00 UTC" },
            new() { Region = "Oceania", Languages = "English", Currency = "AUD", SupportWindow = "08:00-14:00 UTC" }
        };
    }

    private static List<EnterpriseCapability> BuildCapabilities()
    {
        return new List<EnterpriseCapability>
        {
            new() { Name = "Global readiness board", Status = "Live", Detail = "Regional prime windows, local time awareness, and recommended operational focus." },
            new() { Name = "Marketplace intelligence", Status = "Live", Detail = "Local value scoring from rarity, sell value, favorites, and inventory gaps." },
            new() { Name = "Needed-items tracker", Status = "Live", Detail = "Shortage-first prioritization connected to sources and crafting use cases." },
            new() { Name = "CSV export", Status = "Live", Detail = "Download marketplace, item, blueprint, trial, and region signals for external analysis." },
            new() { Name = "Versioned JSON API", Status = "Live", Detail = "Expose dashboard readiness data at /api/v1/global-ops for companion integrations." },
            new() { Name = "Embark live ops feed", Status = "Live", Detail = "Official ARC Raiders news and map-condition banners are surfaced from Embark sources with safe fallback data." },
            new() { Name = "Governance posture", Status = "Ready", Detail = "Health checks, security headers, production cookies, deployment docs, and quality scripts." },
            new() { Name = "Worldwide scale path", Status = "Planned", Detail = "Identity, managed database, audit logging, observability, and team workspaces." }
        };
    }

    private static void AppendRow(
        StringBuilder csv,
        string section,
        string name,
        string category,
        string signal,
        string source,
        string score)
    {
        csv
            .Append(Escape(section)).Append(',')
            .Append(Escape(name)).Append(',')
            .Append(Escape(category)).Append(',')
            .Append(Escape(signal)).Append(',')
            .Append(Escape(source)).Append(',')
            .Append(Escape(score)).AppendLine();
    }

    private static string Escape(string value)
    {
        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}
