using RaidersVault.Models;

namespace RaidersVault.ViewModels;

public class SkillsPageViewModel
{
    public List<Skill> Skills { get; set; } = new();

    public string? SelectedBranch { get; set; }

    public string SelectedEntryStyle { get; set; } = "Balanced";

    public int TotalCurrentPoints { get; set; }

    public int TotalAvailablePoints { get; set; }

    public List<SkillRecommendationItem> PveRecommendations { get; set; } = new();

    public List<SkillRecommendationItem> PvpRecommendations { get; set; } = new();

    public List<SkillRecommendationItem> BalancedRecommendations { get; set; } = new();

    public List<SkillRecommendationItem> SelectedRecommendations =>
        SelectedEntryStyle switch
        {
            "PvE" => PveRecommendations,
            "PvP" => PvpRecommendations,
            _ => BalancedRecommendations
        };
}