using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaidersVault.Models;

public class Skill
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Branch { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Details { get; set; }

    [Range(1, 5)]
    public int MaxPoints { get; set; }

    [StringLength(100)]
    public string? Requires { get; set; }

    [NotMapped]
    public int CurrentPoints { get; set; }

    [NotMapped]
    public bool IsSelected =>
        CurrentPoints > 0;
}