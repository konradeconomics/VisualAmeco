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
}