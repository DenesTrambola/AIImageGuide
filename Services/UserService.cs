using AIImageGuide.Data;
using AIImageGuide.Models;
using System.Text.RegularExpressions;

namespace AIImageGuide.Services;

public class UserService : ServiceBase
{
    public UserService(AppDbContext context) : base(context) { }

    public (bool Success, string Message) Register(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 50)
            return (false, "Username must be 3-50 characters.");
        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return (false, "Invalid email format.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters.");

        if (_context.Users.Any(u => u.Username == username))
            return (false, "Username already exists.");
        if (_context.Users.Any(u => u.Email == email))
            return (false, "Email already exists.");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Registered"
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        return (true, "Registration successful.");
    }

    public (bool Success, User User, string Message) Login(string usernameOrEmail, string password, bool rememberMe)
    {
        var user = _context.Users
            .FirstOrDefault(u => (u.Username == usernameOrEmail || u.Email == usernameOrEmail) && !u.IsBlocked);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, null, "Invalid username/email or password.");

        if (rememberMe)
        {
            // Store secure token (simplified for demo)
            Properties.Settings.Default.AuthToken = Guid.NewGuid().ToString();
            Properties.Settings.Default.UserId = user.Id;
            Properties.Settings.Default.Save();
        }

        return (true, user, "Login successful.");
    }

    public User? GetCurrentUser()
    {
        if (!string.IsNullOrEmpty(Properties.Settings.Default.AuthToken))
        {
            return _context.Users.Find(Properties.Settings.Default.UserId);
        }
        return null;
    }

    public void Logout()
    {
        Properties.Settings.Default.AuthToken = null;
        Properties.Settings.Default.UserId = 0;
        Properties.Settings.Default.Save();
    }
}
