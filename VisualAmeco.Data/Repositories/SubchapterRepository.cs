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
    
    public async Task<IEnumerable<Subchapter>> GetAllAsync()
    {
        return await _context.Subchapters
            .AsNoTracking()
            .OrderBy(s => s.ChapterId)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subchapter>> GetByChapterAsync(int chapterId)
    {
        return await _context.Subchapters
            .Where(s => s.ChapterId == chapterId)
            .OrderBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
    }
}