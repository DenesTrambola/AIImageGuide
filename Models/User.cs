using System.ComponentModel.DataAnnotations;

namespace AIImageGuide.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; }

    [Required, MaxLength(100)]
    public string Email { get; set; }

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; }

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Registered";

    public bool IsBlocked { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Image> Images { get; set; }
    public List<Rating> Ratings { get; set; }
    public List<Comment> Comments { get; set; }
}
