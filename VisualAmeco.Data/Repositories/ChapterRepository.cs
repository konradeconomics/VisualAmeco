using Microsoft.EntityFrameworkCore;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Interfaces;
using VisualAmeco.Data.Contexts;

namespace VisualAmeco.Data.Repositories;

public class ChapterRepository : IChapterRepository
{
    private readonly VisualAmecoDbContext _context;

    public ChapterRepository(VisualAmecoDbContext context)
    {
        _context = context;
    }

    public async Task<Chapter?> GetByNameAsync(string name)
        => await _context.Chapters.FirstOrDefaultAsync(c => c.Name == name);

    public async Task AddAsync(Chapter chapter)
        => await _context.Chapters.AddAsync(chapter);
    
    public async Task<IEnumerable<Chapter>> GetAllAsync()
    {
        return await _context.Chapters
            .AsNoTracking()
            .ToListAsync();
    }
}