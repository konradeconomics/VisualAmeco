using VisualAmeco.Core.Interfaces;
using VisualAmeco.Data.Contexts;

namespace VisualAmeco.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly VisualAmecoDbContext _context;
    
    public UnitOfWork(VisualAmecoDbContext context)
    {
        _context = context;
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}