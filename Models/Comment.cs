namespace AIImageGuide.Models;

public class Comment
{
    public int Id { get; set; }
    public int ImageId { get; set; }
    public Image Image { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}
