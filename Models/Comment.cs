using System.ComponentModel.DataAnnotations;

namespace AIImageGuide.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public int ImageId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required, MaxLength(500)]
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Image Image { get; set; }
    public User User { get; set; }
}
