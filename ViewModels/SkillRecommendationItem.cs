namespace RaidersVault.ViewModels;

public class SkillRecommendationItem
{
    public string Branch { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int MaxPoints { get; set; }

    public int RecommendedPoints { get; set; }

    public string Requires { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public bool HasRequirement =>
        !string.IsNullOrWhiteSpace(Requires);
}