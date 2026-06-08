namespace RaidersVault.ViewModels;

public class MapConditionRecommendationViewModel
{
    public string SelectedMap { get; set; } = "Dam Battlegrounds";

    public string SelectedCondition { get; set; } = "Standard Patrol";

    public string SelectedStyle { get; set; } = "Balanced";

    public int TotalSkillPoints { get; set; }

    public List<string> Maps { get; set; } = new();

    public List<string> Conditions { get; set; } = new();

    public List<string> Styles { get; set; } = new();

    public string Summary { get; set; } = string.Empty;

    public string SkillTreeFocus { get; set; } = string.Empty;

    public string RiskLevel { get; set; } = string.Empty;

    public List<MapConditionLoadoutItem> LoadoutItems { get; set; } = new();

    public OptimalLoadoutViewModel OptimalLoadout { get; set; } = new();

    public List<BlueprintConditionAlertViewModel> BlueprintAlerts { get; set; } = new();

    public bool HasLoadoutItems =>
        LoadoutItems.Any();

    public bool HasBlueprintAlerts =>
        BlueprintAlerts.Any();
}