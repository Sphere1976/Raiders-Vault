using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class Quest : TrackedRecord
{
    [Required(ErrorMessage = "Status is required.")]
    [StringLength(
        75,
        ErrorMessage = "Status must be 75 characters or less.")]
    public string Status { get; set; } = string.Empty;

    [Required(ErrorMessage = "Priority is required.")]
    [StringLength(
        75,
        ErrorMessage = "Priority must be 75 characters or less.")]
    public string Priority { get; set; } = string.Empty;

    [StringLength(
        75,
        ErrorMessage = "Related activity must be 75 characters or less.")]
    public string? RelatedActivity { get; set; }

    [StringLength(
        250,
        ErrorMessage = "Completion notes must be 250 characters or less.")]
    public string? CompletionNotes { get; set; }

    public override string GetSummary()
    {
        return $"{Name} is currently marked as {Status} with {Priority} priority.";
    }
}