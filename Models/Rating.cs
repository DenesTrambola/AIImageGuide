using System.ComponentModel.DataAnnotations;

namespace AIImageGuide.Models;

public class Rating
{
    public int Id { get; set; }

    [Required]
    public int ImageId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Range(1, 5)]
    public int Value { get; set; }

    public Image Image { get; set; }
    public User User { get; set; }
}
