using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class Blueprint : TrackedRecord
{
    [Required(ErrorMessage = "Item type is required.")]
    [StringLength(
        75,
        ErrorMessage = "Item type must be 75 characters or less.")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Collection status is required.")]
    [StringLength(
        75,
        ErrorMessage = "Collection status must be 75 characters or less.")]
    public string CollectionStatus { get; set; } = string.Empty;

    [StringLength(
        500,
        ErrorMessage = "Recipe materials must be 500 characters or less.")]
    public string? RecipeMaterials { get; set; }

    [StringLength(
        150,
        ErrorMessage = "Where to get must be 150 characters or less.")]
    public string? WhereToGet { get; set; }

    [StringLength(
        100,
        ErrorMessage = "Source notes must be 100 characters or less.")]
    public string? SourceNotes { get; set; }

    public bool Collected { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.Today;

    public override string GetSummary()
    {
        return $"{Name} is a {Category} blueprint with status {CollectionStatus}.";
    }
}