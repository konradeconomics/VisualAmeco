using VisualAmeco.Application.DTOs;

namespace Application.Interfaces;

public interface ILookupService
{
    /// <summary>
    /// Retrieves a list of all available countries.
    /// </summary>
    /// <returns>An asynchronous task returning an enumerable collection of CountryDto objects.</returns>
    Task<IEnumerable<CountryDto>> GetAllCountriesAsync();
    
    /// <summary>
    /// Retrieves a list of all available chapters.
    /// </summary>
    /// <returns>An asynchronous task returning an enumerable collection of ChapterDto objects.</returns>
    Task<IEnumerable<ChapterDto>> GetAllChaptersAsync();
}