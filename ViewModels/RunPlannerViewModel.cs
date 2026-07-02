using RaidersVault.Models;

namespace RaidersVault.ViewModels;

public class RunPlannerViewModel
{
    public string SelectedMap { get; set; } = "Dam Battlegrounds";

    public string SelectedCondition { get; set; } = "Standard Patrol";

    public string SelectedGoal { get; set; } = "Loot Run";

    public string SelectedStyle { get; set; } = "Balanced";

    public int TotalSkillPoints { get; set; } = 20;

    public List<string> Maps { get; set; } = new();

    public List<string> Conditions { get; set; } = new();

    public List<string> Goals { get; set; } = new();

    public List<string> Styles { get; set; } = new();

    public string Summary { get; set; } = string.Empty;

    public string SkillFocus { get; set; } = string.Empty;

    public string BlueprintTarget { get; set; } = string.Empty;

    public string ObjectiveTarget { get; set; } = string.Empty;

    public string RouteAdvice { get; set; } = string.Empty;

    public string RiskLevel { get; set; } = "Moderate";

    public int PriorityScore { get; set; } = 70;

    public string ExtractionWindow { get; set; } = "Extract once primary objective and one blueprint lead are checked.";

    public List<string> RouteStops { get; set; } = new();

    public List<string> ThreatNotes { get; set; } = new();

    public List<string> OperatorTips { get; set; } = new();

    public List<string> CompanionSignals { get; set; } = new();

    public OptimalLoadoutViewModel? Loadout { get; set; }

    public List<SkillRecommendationItem> SuggestedSkills { get; set; } = new();

    public List<Blueprint> SuggestedBlueprints { get; set; } = new();

    public List<Quest> SuggestedQuests { get; set; } = new();

    public bool HasRecommendations =>
        SuggestedBlueprints.Any()
        || SuggestedQuests.Any();
}
