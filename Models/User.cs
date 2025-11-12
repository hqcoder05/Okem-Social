using System.ComponentModel.DataAnnotations;

namespace okem_social.Models;

public class User
{
    public int Id { get; set; }

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = "";

    [Required, MaxLength(120)]
    public string FullName { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [MaxLength(40)]                  
    public string? Nickname { get; set; }

    [MaxLength(300)]
    public string? Bio { get; set; }

    public DateTime? AvatarUpdatedAt { get; set; }

    public Role Role { get; set; } = Role.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}