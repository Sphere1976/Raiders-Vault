using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class UserAccount
{
    public int Id { get; set; }

    [Required]
    [StringLength(
        50,
        ErrorMessage = "Username cannot exceed 50 characters.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}