using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class UserSkill
{
    public int Id { get; set; }

    public int SkillId { get; set; }

    [Range(
        0,
        5,
        ErrorMessage = "Skill points for one skill must be between 0 and 5.")]
    public int CurrentPoints { get; set; }

    public Skill? Skill { get; set; }
}