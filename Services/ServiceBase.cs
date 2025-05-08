using AIImageGuide.Data;

namespace AIImageGuide.Services;

public abstract class ServiceBase(AppDbContext context)
{
    protected readonly AppDbContext _context = context;
}
