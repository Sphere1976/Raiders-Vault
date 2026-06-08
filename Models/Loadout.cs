using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class Loadout : TrackedRecord
{
    [Required(ErrorMessage = "Activity type is required.")]
    [StringLength(
        75,
        ErrorMessage = "Activity type must be 75 characters or less.")]
    public string ActivityType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Map or event is required.")]
    [StringLength(
        75,
        ErrorMessage = "Map or event must be 75 characters or less.")]
    public string MapOrEvent { get; set; } = string.Empty;

    [StringLength(
        75,
        ErrorMessage = "Focus area must be 75 characters or less.")]
    public string? FocusArea { get; set; }

    [StringLength(
        75,
        ErrorMessage = "Risk level must be 75 characters or less.")]
    public string? RiskLevel { get; set; }

    [StringLength(
        250,
        ErrorMessage = "Gear notes must be 250 characters or less.")]
    public string? GearNotes { get; set; }

    public override string GetSummary()
    {
        return $"{Name} is a {ActivityType} loadout for {MapOrEvent}.";
    }
}