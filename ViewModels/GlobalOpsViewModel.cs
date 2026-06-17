namespace RaidersVault.ViewModels;

public class GlobalOpsViewModel
{
    public DateTimeOffset GeneratedAtUtc { get; set; }

    public string PlayerName { get; set; } = "Raider";

    public string PreferredPlaystyle { get; set; } = "Balanced";

    public string DefaultMap { get; set; } = "Dam Battlegrounds";

    public int QuestCompletionPercent { get; set; }

    public int BlueprintCompletionPercent { get; set; }

    public int InventoryReadinessPercent { get; set; }

    public int OverallReadinessPercent { get; set; }

    public string ReadinessTier { get; set; } = "Operational";

    public int TotalVaultValue { get; set; }

    public int ActiveRiskSignals { get; set; }

    public string GlobalUsersLabel { get; set; } = "Multi-region ready";

    public List<RegionOpsCard> Regions { get; set; } = new();

    public List<MarketInsightItem> MarketSignals { get; set; } = new();

    public List<MarketInsightItem> NeededItems { get; set; } = new();

    public List<MarketInsightItem> BlueprintTargets { get; set; } = new();

    public List<MarketInsightItem> TrialSignals { get; set; } = new();

    public List<LocalizationSignal> LocalizationSignals { get; set; } = new();

    public ArcRaidersLiveOpsViewModel LiveOps { get; set; } = new();

    public List<EnterpriseCapability> Capabilities { get; set; } = new();
}

public class RegionOpsCard
{
    public string Region { get; set; } = string.Empty;

    public string Locale { get; set; } = string.Empty;

    public string LocalTime { get; set; } = string.Empty;

    public string PrimeWindow { get; set; } = string.Empty;

    public string RecommendedFocus { get; set; } = string.Empty;

    public string RiskLevel { get; set; } = "Medium";
}

public class MarketInsightItem
{
    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Signal { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public int Score { get; set; }
}

public class EnterpriseCapability
{
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = "Live";

    public string Detail { get; set; } = string.Empty;
}

public class LocalizationSignal
{
    public string Region { get; set; } = string.Empty;

    public string Languages { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public string SupportWindow { get; set; } = string.Empty;
}
