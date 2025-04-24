using Microsoft.EntityFrameworkCore;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Interfaces;
using VisualAmeco.Data.Contexts;

namespace VisualAmeco.Data.Repositories;

public class SubchapterRepository : ISubchapterRepository
{
    private readonly VisualAmecoDbContext _context;

    public SubchapterRepository(VisualAmecoDbContext context)
    {
        _context = context;
    }

    public async Task<Subchapter?> GetByNameAsync(string name, int chapterId)
        => await _context.Subchapters
            .FirstOrDefaultAsync(c => c.Name == name && c.ChapterId == chapterId);
    
    public async Task AddAsync(Subchapter subchapter)
        => await _context.Subchapters.AddAsync(subchapter);
}