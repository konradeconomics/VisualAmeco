using VisualAmeco.Core.Entities;

namespace VisualAmeco.Core.Interfaces;

/// <summary>
/// Interface defining repository operations for Value entities.
/// </summary>
public interface IValueRepository
{
    /// <summary>
    /// Gets a specific Value entity based on its composite key components.
    /// </summary>
    /// <param name="variableId">The ID of the associated variable.</param>
    /// <param name="countryId">The ID of the associated country.</param>
    /// <param name="year">The year of the value.</param>
    /// <param name="month">The month of the value (if applicable).</param>
    /// <returns>The matching Value entity or null if not found.</returns>
    Task<Value?> GetAsync(int variableId, int countryId, int year, int? month);

    /// <summary>
    /// Stages a Value entity for addition to the data store.
    /// Note: Requires UnitOfWork.SaveChangesAsync() to persist.
    /// </summary>
    /// <param name="value">The Value entity to add.</param>
    Task AddAsync(Value value);

    // --- NEW METHOD ADDED ---
    /// <summary>
    /// Gets all Value entities including related details needed for indicator display.
    /// Implementations should typically use eager loading (e.g., EF Core's Include/ThenInclude)
    /// to fetch associated Variable, Variable.Subchapter, Variable.Subchapter.Chapter,
    /// and Country entities efficiently in a single query if possible.
    /// </summary>
    /// <param name="cancellationToken">Optional token to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous task that returns a list of Value entities with related data included.</returns>
    Task<List<Value>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets Value entities based on specified filters, including related details.
    /// Implementations should use eager loading (Include/ThenInclude).
    /// </summary>
    /// <param name="countryCode">Optional country code to filter by.</param>
    /// <param name="variableCode">Optional variable code to filter by.</param>
    /// <param name="chapterName">Optional chapter name to filter by.</param>
    /// <param name="subchapterName">Optional subchapter name to filter by.</param>
    /// <param name="years">Optional list of specific years to include.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>A list of filtered Value entities with related data included.</returns>
    Task<List<Value>> GetFilteredWithDetailsAsync(
        string? countryCode = null,
        string? variableCode = null,
        string? chapterName = null,
        string? subchapterName = null,
        List<int>? years = null,
        CancellationToken cancellationToken = default);
}