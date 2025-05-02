using Microsoft.EntityFrameworkCore;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Interfaces;
using VisualAmeco.Data.Contexts;

namespace VisualAmeco.Data.Repositories;

public class ValueRepository : IValueRepository
{
    private readonly VisualAmecoDbContext _context;
    
    public ValueRepository(VisualAmecoDbContext context)
    {
        _context = context;
    }

    public async Task<Value?> GetAsync(int variableId, int countryId, int year, int? month)
        => await _context.Values
            .FirstOrDefaultAsync(v => v.VariableId == variableId && v.CountryId == countryId && v.Year == year && v.Month == month);

    public async Task AddAsync(Value value)
        => await _context.Values.AddAsync(value);
    
    /// <summary>
    /// Gets all Value entities including related Variable, Country, Subchapter, and Chapter details.
    /// Uses eager loading via EF Core Include/ThenInclude.
    /// </summary>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>A list of Value entities with related data included.</returns>
    public async Task<List<Value>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.Values
            .Include(value => value.Variable)
            .ThenInclude(variable => variable.SubChapter)
            .ThenInclude(subchapter => subchapter.Chapter)
            .Include(value => value.Country);

        var values = await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return values;
    }
    
    public async Task<List<Value>> GetFilteredWithDetailsAsync(
            string? countryCode = null,
            string? variableCode = null,
            string? chapterName = null,
            string? subchapterName = null,
            List<int>? years = null,
            CancellationToken cancellationToken = default)
    {
        var query = _context.Values
            .Include(v => v.Variable)
            .ThenInclude(va => va.SubChapter)
            .ThenInclude(s => s.Chapter)
            .Include(v => v.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(countryCode))
        {
            query = query.Where(v => v.Country.Code.ToUpper() == countryCode.ToUpper());
        }
        if (!string.IsNullOrWhiteSpace(variableCode))
        {
            query = query.Where(v => v.Variable.Code.ToUpper() == variableCode.ToUpper());
        }
        if (!string.IsNullOrWhiteSpace(chapterName))
        {
            query = query.Where(v => v.Variable.SubChapter.Chapter.Name == chapterName);
        }
        if (!string.IsNullOrWhiteSpace(subchapterName))
        {
            query = query.Where(v => v.Variable.SubChapter.Name == subchapterName);
        }

        if (years != null && years.Any())
        {
            query = query.Where(v => years.Contains(v.Year));
        }


        var values = await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return values;
    }
}