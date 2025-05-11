namespace AIImageGuide.Models;

public class Image
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public string FilePath { get; set; }
    public DateTime UploadDate { get; set; }
    public List<Rating> Ratings { get; set; } = new List<Rating>();
    public List<Comment> Comments { get; set; } = new List<Comment>();
}
