namespace AIImageGuide.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Image> Images { get; set; } = new List<Image>();
}
