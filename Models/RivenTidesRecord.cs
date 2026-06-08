using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class RivenTidesRecord : TrackedRecord
{
    [Required(ErrorMessage = "Record type is required.")]
    [StringLength(
        75,
        ErrorMessage = "Record type must be 75 characters or less.")]
    public string RecordType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Zone is required.")]
    [StringLength(
        100,
        ErrorMessage = "Zone must be 100 characters or less.")]
    public string Zone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Risk level is required.")]
    [StringLength(
        50,
        ErrorMessage = "Risk level must be 50 characters or less.")]
    public string RiskLevel { get; set; } = string.Empty;

    [StringLength(
        150,
        ErrorMessage = "Recommended tool must be 150 characters or less.")]
    public string? RecommendedTool { get; set; }

    [StringLength(
        250,
        ErrorMessage = "Loot focus must be 250 characters or less.")]
    public string? LootFocus { get; set; }

    public bool Completed { get; set; }

    public override string GetSummary()
    {
        return $"{Name} is a {RecordType} entry for {Zone} with {RiskLevel} risk.";
    }
}