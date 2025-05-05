using VisualAmeco.Core.Entities;

namespace VisualAmeco.Core.Interfaces;

public interface ICountryRepository
{
    /// <summary>
    /// Gets a specific Country entity by its unique code.
    /// </summary>
    /// <param name="code">The country code.</param>
    /// <returns>The matching Country entity or null if not found.</returns>
    Task<Country?> GetByCodeAsync(string code);
    
    /// <summary>
    /// Stages a Country entity for addition to the data store.
    /// </summary>
    /// <param name="country">The Country entity to add.</param>
    Task AddAsync(Country country);
    
    /// <summary>
    /// Gets all Country entities from the data store.
    /// </summary>
    /// <returns>An asynchronous task returning an enumerable collection of all Country entities.</returns>
    Task<IEnumerable<Country>> GetAllAsync();
}