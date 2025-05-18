using AIImageGuide.Data;
using AIImageGuide.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AIImageGuide.Services;

public class UserService : ServiceBase
{
    public User? CurrentUser { get; private set; }

    public UserService(AppDbContext context) : base(context)
    {
        var authToken = Properties.Settings.Default.AuthToken;
        var userId = Properties.Settings.Default.UserId;
        if (!string.IsNullOrEmpty(authToken) && userId > 0)
        {
            CurrentUser = context.Users.Find(userId);
            if (CurrentUser == null || CurrentUser.IsBlocked)
            {
                Properties.Settings.Default.AuthToken = null;
                Properties.Settings.Default.UserId = 0;
                Properties.Settings.Default.Save();
            }
        }
    }

    public (bool Success, string Message) Register(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 50)
            return (false, "Ім'я користувача має містити від 3 до 50 символів.");
        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return (false, "Недійсний формат електронної адреси.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Пароль має містити щонайменше 6 символів.");

        if (_context.Users.Any(u => u.Username == username))
            return (false, "Ім'я користувача вже існує.");
        if (_context.Users.Any(u => u.Email == email))
            return (false, "Електронна адреса вже існує.");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Registered"
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        return (true, "Реєстрація успішна.");
    }

    public (bool Success, User User, string Message) Login(string usernameOrEmail, string password, bool rememberMe)
    {
        var user = _context.Users
            .FirstOrDefault(u => (u.Username == usernameOrEmail || u.Email == usernameOrEmail) && !u.IsBlocked);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, null, "Недійсне ім'я користувача або пароль.");

        CurrentUser = user;

        if (rememberMe)
        {
            Properties.Settings.Default.AuthToken = Guid.NewGuid().ToString();
            Properties.Settings.Default.UserId = user.Id;
            Properties.Settings.Default.Save();
        }

        return (true, user, "Вхід успішний.");
    }

    public User? GetUserById(int userId)
    {
        return _context.Users.Find(userId);
    }

    public void Logout()
    {
        CurrentUser = null;

        Properties.Settings.Default.AuthToken = null;
        Properties.Settings.Default.UserId = 0;
        Properties.Settings.Default.Save();
    }

    public List<Image> GetUserImages(int userId, int? categoryId = null, string sortBy = "UploadDate")
    {
        var query = _context.Images
            .Include(i => i.User)
            .Include(i => i.Category)
            .Include(i => i.Ratings)
            .Where(i => i.UserId == userId)
            .AsQueryable();
        if (categoryId.HasValue)
            query = query.Where(i => i.CategoryId == categoryId.Value);
        query = sortBy switch
        {
            "Rating" => query.OrderByDescending(i => i.Ratings.Any() ? i.Ratings.Average(r => r.Value) : 0),
            "UploadDate" => query.OrderByDescending(i => i.UploadDate),
            _ => query.OrderByDescending(i => i.UploadDate)
        };
        return query.ToList();
    }

    public List<Rating> GetUserRatings(int userId)
    {
        return _context.Ratings
            .Include(r => r.Image)
            .ThenInclude(i => i.User)
            .Where(r => r.UserId == userId)
            .ToList();
    }

    public List<Comment> GetUserComments(int userId)
    {
        return _context.Comments
            .Include(c => c.Image)
            .ThenInclude(i => i.User)
            .Where(c => c.UserId == userId)
            .ToList();
    }
}
