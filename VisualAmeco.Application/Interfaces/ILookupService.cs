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
    
    /// <summary>
    /// Retrieves a list of variables, optionally filtered by chapter and/or subchapter.
    /// </summary>
    /// <param name="chapterId">Optional ID of the chapter to filter by.</param>
    /// <param name="subchapterId">Optional ID of the subchapter to filter by.</param>
    /// <returns>An asynchronous task returning an enumerable collection of VariableDto objects.</returns>
    Task<IEnumerable<VariableDto>> GetVariablesAsync(int? chapterId = null, int? subchapterId = null);
    
    /// <summary>
    /// Retrieves a list of subchapters, optionally filtered by chapter ID.
    /// </summary>
    /// <param name="chapterId">Optional ID of the chapter to filter subchapters by.</param>
    /// <returns>An asynchronous task returning an enumerable collection of SubchapterDto objects.</returns>
    Task<IEnumerable<SubchapterDto>> GetSubchaptersAsync(int? chapterId = null);
}