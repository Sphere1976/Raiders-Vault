using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public abstract class TrackedRecord
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(
        100,
        ErrorMessage = "Name must be 100 characters or less.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(
        500,
        ErrorMessage = "Notes must be 500 characters or less.")]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public virtual string GetSummary()
    {
        return $"{Name} was last updated on {UpdatedAt:d}.";
    }
}