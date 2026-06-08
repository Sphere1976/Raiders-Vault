using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class FavoriteItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Item type is required.")]
    [StringLength(
        30,
        ErrorMessage = "Item type must be 30 characters or less.")]
    public string ItemType { get; set; } = string.Empty;

    public int ItemId { get; set; }

    [Required(ErrorMessage = "Display name is required.")]
    [StringLength(
        100,
        ErrorMessage = "Display name must be 100 characters or less.")]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(
        250,
        ErrorMessage = "Notes must be 250 characters or less.")]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}