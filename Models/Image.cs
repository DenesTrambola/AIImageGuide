using System.ComponentModel.DataAnnotations;

namespace AIImageGuide.Models;

public class Image
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [Required, MaxLength(255)]
    public string FilePath { get; set; }

    public int CategoryId { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public int ViewCount { get; set; } = 0;

    public User User { get; set; }
    public Category Category { get; set; }
    public List<Rating> Ratings { get; set; }
    public List<Comment> Comments { get; set; }
}
