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
            return (false, "Назва має містити від 1 до 100 символів.");
        if (description?.Length > 500)
            return (false, "Опис не може перевищувати 500 символів.");
        if (!_context.Categories.Any(c => c.Id == categoryId))
            return (false, "Недійсна категорія.");
        if (!_context.Users.Any(u => u.Id == userId && !u.IsBlocked))
            return (false, "Користувача не знайдено або його не заблоковано.");
        if (!File.Exists(filePath) || !new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(filePath).ToLower()))
            return (false, "Недійсний файл. Дозволено лише .jpg, .jpeg, .png..");
        if (new FileInfo(filePath).Length > 5 * 1024 * 1024)
            return (false, "Розмір файлу має бути менше 5 МБ.");

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
            return (true, "Зображення успішно завантажено.");
        }
        catch (Exception ex)
        {
            return (false, $"Не вдалося завантажити: {ex.Message}");
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
            return (false, "Зображення не знайдено.");

        if (image.UserId != userId && !isAdmin)
            return (false, "Тільки власник зображення або адміністратор може видалити це зображення.");

        try
        {
            _context.Ratings.RemoveRange(_context.Ratings.Where(r => r.ImageId == imageId));
            _context.Comments.RemoveRange(_context.Comments.Where(c => c.ImageId == imageId));
            _context.Images.Remove(image);

            if (File.Exists(image.FilePath))
                File.Delete(image.FilePath);

            _context.SaveChanges();
            return (true, "Зображення успішно видалено.");
        }
        catch (Exception ex)
        {
            return (false, $"Видалення не вдалося: {ex.Message}");
        }
    }

    public (bool Success, string Message) AddRating(int imageId, int userId, int value)
    {
        if (!_context.Images.Any(i => i.Id == imageId))
            return (false, "Зображення не знайдено.");
        if (!_context.Users.Any(u => u.Id == userId && !u.IsBlocked))
            return (false, "Користувача не знайдено або його не заблоковано.");
        if (value < 1 || value > 5)
            return (false, "Рейтинг має бути від 1 до 5.");

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
        return (true, "Рейтинг додано.");
    }

    public (bool Success, string Message) AddComment(int imageId, int userId, string content)
    {
        if (!_context.Images.Any(i => i.Id == imageId))
            return (false, "Зображення не знайдено.");
        if (!_context.Users.Any(u => u.Id == userId && !u.IsBlocked))
            return (false, "Користувача не знайдено або його не заблоковано.");
        if (string.IsNullOrWhiteSpace(content) || content.Length > 500)
            return (false, "Коментар має містити від 1 до 500 символів.");

        _context.Comments.Add(new Comment
        {
            ImageId = imageId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
        return (true, "Коментар додано.");
    }

    public List<Category> GetCategories()
    {
        return _context.Categories.ToList();
    }
}
