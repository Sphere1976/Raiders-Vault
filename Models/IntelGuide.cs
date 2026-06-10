using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class IntelGuide : TrackedRecord
{
    [Required, StringLength(70)] public string GuideType { get; set; } = "Route";
    [StringLength(80)] public string MapName { get; set; } = "Any Map";
    [StringLength(80)] public string MapCondition { get; set; } = "Standard";
    [StringLength(40)] public string Difficulty { get; set; } = "Medium";
    [StringLength(500)] public string RecommendedRoute { get; set; } = string.Empty;
    [StringLength(500)] public string LootFocus { get; set; } = string.Empty;
    [StringLength(500)] public string RiskWarning { get; set; } = string.Empty;
}
