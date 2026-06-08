namespace RaidersVault.ViewModels;

public class BlueprintConditionAlertViewModel
{
    public int BlueprintId { get; set; }

    public string BlueprintName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string ActiveMap { get; set; } = string.Empty;

    public string ActiveCondition { get; set; } = string.Empty;

    public string MatchType { get; set; } = string.Empty;

    public string ContainerType { get; set; } = string.Empty;

    public string BestAreas { get; set; } = string.Empty;

    public string FarmingRoute { get; set; } = string.Empty;

    public string LoadoutSummary { get; set; } = string.Empty;

    public int PriorityScore { get; set; }

    public bool HasLoadoutSummary =>
        !string.IsNullOrWhiteSpace(LoadoutSummary);

    public bool HasFarmingRoute =>
        !string.IsNullOrWhiteSpace(FarmingRoute);
}