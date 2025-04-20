using VisualAmeco.Core.Entities;

namespace VisualAmeco.Core.Interfaces;

public interface ICountryRepository
{
    Task<Country?> GetByCodeAsync(string code);
    Task AddAsync(Country country);
}