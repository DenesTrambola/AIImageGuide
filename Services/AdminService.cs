using AIImageGuide.Data;
using AIImageGuide.Models;

namespace AIImageGuide.Services;

public class AdminService : ServiceBase
{
    public AdminService(AppDbContext context) : base(context) { }

    public List<User> GetAllUsers()
    {
        return _context.Users.ToList();
    }

    public bool BlockUser(int userId, bool block)
    {
        var user = _context.Users.Find(userId);
        if (user == null || user.Role == "Admin") return false;
        user.IsBlocked = block;
        _context.SaveChanges();
        return true;
    }

    public bool ChangeRole(int userId, string newRole)
    {
        if (!newRole.Equals("Admin") && !newRole.Equals("Registered")) return false;
        var user = _context.Users.Find(userId);
        if (user == null) return false;
        user.Role = newRole;
        _context.SaveChanges();
        return true;
    }

    public bool DeleteUser(int userId)
    {
        var user = _context.Users.Find(userId);
        if (user == null || user.Role == "Admin") return false;
        _context.Users.Remove(user);
        _context.SaveChanges();
        return true;
    }
}
