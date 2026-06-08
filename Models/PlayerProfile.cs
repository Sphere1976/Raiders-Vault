using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class PlayerProfile
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Player name is required.")]
    [StringLength(
        60,
        ErrorMessage = "Player name must be 60 characters or less.")]
    public string PlayerName { get; set; } = "Raider";

    [Required(ErrorMessage = "Preferred playstyle is required.")]
    [StringLength(
        20,
        ErrorMessage = "Preferred playstyle must be 20 characters or less.")]
    public string PreferredPlaystyle { get; set; } = "Balanced";

    [Required(ErrorMessage = "Default map is required.")]
    [StringLength(
        75,
        ErrorMessage = "Default map must be 75 characters or less.")]
    public string DefaultMap { get; set; } = "Dam Battlegrounds";

    [Range(
        0,
        120,
        ErrorMessage = "Skill points must be between 0 and 120.")]
    public int CurrentSkillPoints { get; set; } = 20;

    [StringLength(
        250,
        ErrorMessage = "Notes must be 250 characters or less.")]
    public string? Notes { get; set; }
}