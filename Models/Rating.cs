namespace AIImageGuide.Models;

public class Rating
{
    public int ImageId { get; set; }
    public Image Image { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int Value { get; set; }
}
