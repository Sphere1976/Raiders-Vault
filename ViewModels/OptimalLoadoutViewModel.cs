namespace RaidersVault.ViewModels;

public class OptimalLoadoutViewModel
{
    public string Playstyle { get; set; } = string.Empty;

    public string Condition { get; set; } = string.Empty;

    public string Map { get; set; } = string.Empty;

    public int TotalSkillPoints { get; set; }

    public int Score { get; set; }

    public string PrimaryWeapon { get; set; } = string.Empty;

    public string SecondaryWeapon { get; set; } = string.Empty;

    public string Shield { get; set; } = string.Empty;

    public string Augment { get; set; } = string.Empty;

    public List<string> QuickUseItems { get; set; } = new();

    public List<string> SkillSynergies { get; set; } = new();

    public List<string> WhyThisLoadout { get; set; } = new();

    public string Caution { get; set; } = string.Empty;

    public bool HasCaution =>
        !string.IsNullOrWhiteSpace(Caution);

    public bool HasRecommendations =>
        QuickUseItems.Any()
        || SkillSynergies.Any()
        || WhyThisLoadout.Any();
}