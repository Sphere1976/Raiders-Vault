namespace RaidersVault.ViewModels;

public class BlueprintFarmPlanViewModel
{
    public int BlueprintId { get; set; }

    public string BlueprintName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Playstyle { get; set; } = "Balanced";

    public string RarityTier { get; set; } = string.Empty;

    public int ProbabilityWeight { get; set; }

    public string ProbabilityLabel { get; set; } = string.Empty;

    public string BestMap { get; set; } = string.Empty;

    public string BestCondition { get; set; } = string.Empty;

    public string ContainerType { get; set; } = string.Empty;

    public string BestAreas { get; set; } = string.Empty;

    public string FarmingRoute { get; set; } = string.Empty;

    public string LoadoutSummary { get; set; } = string.Empty;

    public string WhyThisPlan { get; set; } = string.Empty;

    public string ConditionNote { get; set; } = string.Empty;

    public List<string> RequiredConditions { get; set; } = new();

    public List<string> OptimalConditions { get; set; } = new();

    public List<string> ValidMaps { get; set; } = new();

    public List<string> QuickUseItems { get; set; } = new();

    public bool HasRequiredConditions =>
        RequiredConditions.Any();

    public bool HasOptimalConditions =>
        OptimalConditions.Any();

    public bool HasQuickUseItems =>
        QuickUseItems.Any();

    public bool HasConditionNote =>
        !string.IsNullOrWhiteSpace(ConditionNote);
}