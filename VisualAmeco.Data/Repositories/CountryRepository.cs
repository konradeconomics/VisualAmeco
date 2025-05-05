using Microsoft.EntityFrameworkCore;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Interfaces;
using VisualAmeco.Data.Contexts;

namespace VisualAmeco.Data.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly VisualAmecoDbContext _context;

    public CountryRepository(VisualAmecoDbContext context)
    {
        _context = context;
    }
    
    public async Task<Country?> GetByCodeAsync(string code)
        => await _context.Countries.FirstOrDefaultAsync(c => c.Code == code);
    
    public async Task AddAsync(Country country)
        => await _context.Countries.AddAsync(country);
    
    public async Task<IEnumerable<Country>> GetAllAsync()
    {
        return await _context.Countries
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}