using AIImageGuide.Data;
using AIImageGuide.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace AIImageGuide.Services;

public class ImageService : ServiceBase
{
    private readonly string _imageFolder;

    public ImageService(AppDbContext context) : base(context)
    {
        _imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
        if (!Directory.Exists(_imageFolder))
            Directory.CreateDirectory(_imageFolder);
    }

    public (bool Success, string Message) UploadImage(string title, string description, int categoryId, string filePath, int userId)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 100)
            return (false, "Title must be 1-100 characters.");
        if (description?.Length > 500)
            return (false, "Description cannot exceed 500 characters.");
        if (!_context.Categories.Any(c => c.Id == categoryId))
            return (false, "Invalid category.");
        if (!_context.Users.Any(u => u.Id == userId && !u.IsBlocked))
            return (false, "User not found or blocked.");
        if (!File.Exists(filePath) || !new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(filePath).ToLower()))
            return (false, "Invalid file. Only .jpg, .jpeg, .png allowed.");
        if (new FileInfo(filePath).Length > 5 * 1024 * 1024)
            return (false, "File size must be under 5MB.");

        try
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(filePath);
            string destinationPath = Path.Combine(_imageFolder, fileName);
            File.Copy(filePath, destinationPath, true);

            var image = new Image
            {
                UserId = userId,
                Title = title,
                Description = description,
                CategoryId = categoryId,
                FilePath = destinationPath,
                UploadDate = DateTime.UtcNow
            };

            _context.Images.Add(image);
            _context.SaveChanges();
            return (true, "Image uploaded successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Upload failed: {ex.Message}");
        }
    }

    public (List<Image> Images, int TotalPages) GetImages(int? categoryId = null, string sortBy = "UploadDate", int page = 1, int pageSize = 6)
    {
        var query = _context.Images.Include(i => i.User).Include(i => i.Category).Include(i => i.Ratings).AsQueryable();
        if (categoryId.HasValue)
            query = query.Where(i => i.CategoryId == categoryId.Value);

        query = sortBy switch
        {
            "Rating" => query.OrderByDescending(i => i.Ratings.Any() ? i.Ratings.Average(r => r.Value) : 0),
            "UploadDate" => query.OrderByDescending(i => i.UploadDate),
            _ => query.OrderByDescending(i => i.UploadDate)
        };

        int totalImages = query.Count();
        int totalPages = (int)Math.Ceiling((double)totalImages / pageSize);

        var images = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (images, totalPages);
    }

    public (List<Image> Images, int TotalPages) SearchImages(string query, int? categoryId = null, int page = 1, int pageSize = 6)
    {
        var searchQuery = _context.Images
            .Include(i => i.User)
            .Include(i => i.Category)
            .Include(i => i.Ratings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.ToLower();
            searchQuery = searchQuery.Where(i =>
                i.Title.ToLower().Contains(query) ||
                (i.Description != null && i.Description.ToLower().Contains(query)) ||
                i.Category.Name.ToLower().Contains(query));
        }

        if (categoryId.HasValue)
            searchQuery = searchQuery.Where(i => i.CategoryId == categoryId.Value);

        int totalImages = searchQuery.Count();
        int totalPages = (int)Math.Ceiling((double)totalImages / pageSize);

        var images = searchQuery
            .OrderByDescending(i => i.UploadDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (images, totalPages);
    }

    public (List<Image> Images, int TotalPages) GetUserImages(int userId, int page = 1, int pageSize = 6)
    {
        var query = _context.Images
            .Include(i => i.User)
            .Include(i => i.Category)
            .Include(i => i.Ratings)
            .Where(i => i.UserId == userId);

        int totalImages = query.Count();
        int totalPages = (int)Math.Ceiling((double)totalImages / pageSize);

        var images = query
            .OrderByDescending(i => i.UploadDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (images, totalPages);
    }

    public (bool Success, string Message) DeleteImage(int imageId, int userId, bool isAdmin)
    {
        var image = _context.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
            return (false, "Image not found.");

        if (image.UserId != userId && !isAdmin)
            return (false, "Only the image owner or an admin can delete this image.");

        try
        {
            _context.Ratings.RemoveRange(_context.Ratings.Where(r => r.ImageId == imageId));
            _context.Comments.RemoveRange(_context.Comments.Where(c => c.ImageId == imageId));
            _context.Images.Remove(image);

            if (File.Exists(image.FilePath))
                File.Delete(image.FilePath);

            _context.SaveChanges();
            return (true, "Image deleted successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Deletion failed: {ex.Message}");
        }
    }

    public (bool Success, string Message) AddRating(int imageId, int userId, int value)
    {
        if (!_context.Images.Any(i => i.Id == imageId))
            return (false, "Image not found.");
        if (!_context.Users.Any(u => u.Id == userId && !u.IsBlocked))
            return (false, "User not found or blocked.");
        if (value < 1 || value > 5)
            return (false, "Rating must be 1-5.");

        var existingRating = _context.Ratings.FirstOrDefault(r => r.ImageId == imageId && r.UserId == userId);
        if (existingRating != null)
        {
            existingRating.Value = value;
        }
        else
        {
            _context.Ratings.Add(new Rating { ImageId = imageId, UserId = userId, Value = value });
        }

        _context.SaveChanges();
        return (true, "Rating added.");
    }

    public (bool Success, string Message) AddComment(int imageId, int userId, string content)
    {
        if (!_context.Images.Any(i => i.Id == imageId))
            return (false, "Image not found.");
        if (!_context.Users.Any(u => u.Id == userId && !u.IsBlocked))
            return (false, "User not found or blocked.");
        if (string.IsNullOrWhiteSpace(content) || content.Length > 500)
            return (false, "Comment must be 1-500 characters.");

        _context.Comments.Add(new Comment
        {
            ImageId = imageId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
        return (true, "Comment added.");
    }

    public List<Category> GetCategories()
    {
        return _context.Categories.ToList();
    }
}
