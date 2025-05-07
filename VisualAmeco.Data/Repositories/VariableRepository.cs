using Microsoft.EntityFrameworkCore;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Interfaces;
using VisualAmeco.Data.Contexts;

namespace VisualAmeco.Data.Repositories;

public class VariableRepository : IVariableRepository
{
    private readonly VisualAmecoDbContext _context;
    
    
    public VariableRepository(VisualAmecoDbContext context)
    {
        _context = context;
    }
    
    public async Task<Variable?> GetByCodeAsync(string code)
        => await _context.Variables
            .FirstOrDefaultAsync(v => v.Code == code);
    
    public async Task AddAsync(Variable variable)
        => await _context.Variables.AddAsync(variable);
    
    public async Task<IEnumerable<Variable>> GetFilteredAsync(int? chapterId = null, int? subchapterId = null)
    {
        var query = _context.Variables
            .Include(v => v.SubChapter)
            .AsNoTracking()
            .AsQueryable();

        if (subchapterId.HasValue)
        {
            query = query.Where(v => v.SubChapterId == subchapterId.Value);
        }
        else if (chapterId.HasValue)
        {
            query = query.Where(v => v.SubChapter != null && v.SubChapter.ChapterId == chapterId.Value);
        }

        return await query.OrderBy(v => v.Name).ToListAsync();
    }
}