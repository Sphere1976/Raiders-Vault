using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class WeeklyTrial : TrackedRecord
{
    [Required, StringLength(80)] public string ObjectiveType { get; set; } = "Loot";
    public int TargetScore { get; set; } = 3000;
    public int ScorePerAction { get; set; } = 1000;
    [StringLength(80)] public string BestMap { get; set; } = "Any Map";
    [StringLength(500)] public string Strategy { get; set; } = string.Empty;
    public int ActionsNeeded => ScorePerAction <= 0 ? 0 : (int)Math.Ceiling((double)TargetScore / ScorePerAction);
}
