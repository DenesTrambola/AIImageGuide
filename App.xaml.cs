using AIImageGuide.Data;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace AIImageGuide;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=image-guide.db");

        using (var context = new AppDbContext(optionsBuilder.Options))
        {
            context.Database.Migrate();
        }
    }
}
