using AIImageGuide.Models;
using Microsoft.EntityFrameworkCore;

namespace AIImageGuide.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User: Unique indexes for Username and Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Image: Foreign keys
        modelBuilder.Entity<Image>()
            .HasOne(i => i.User)
            .WithMany(u => u.Images)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Image>()
            .HasOne(i => i.Category)
            .WithMany(c => c.Images)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Image>()
            .HasMany(i => i.Comments)
            .WithOne(c => c.Image)
            .HasForeignKey(c => c.ImageId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Image>()
            .HasMany(i => i.Ratings)
            .WithOne(r => r.Image)
            .HasForeignKey(r => r.ImageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category: Unique index for Name
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // Rating: Composite key and foreign keys
        modelBuilder.Entity<Rating>()
            .HasKey(r => new { r.ImageId, r.UserId });
        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Image)
            .WithMany(i => i.Ratings)
            .HasForeignKey(r => r.ImageId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Rating>()
            .HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Comment: Foreign keys
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Image)
            .WithMany(i => i.Comments)
            .HasForeignKey(c => c.ImageId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
