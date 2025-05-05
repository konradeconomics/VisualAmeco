using VisualAmeco.Core.Entities;

namespace VisualAmeco.Core.Interfaces;

/// <summary>
/// Interface defining repository operations for Chapter entities.
/// </summary>
public interface IChapterRepository
{
    /// <summary>
    /// Gets a specific Chapter entity by its name.
    /// </summary>
    /// <param name="name">The chapter name.</param>
    /// <returns>The matching Chapter entity or null if not found.</returns>
    Task<Chapter?> GetByNameAsync(string name);

    /// <summary>
    /// Stages a Chapter entity for addition to the data store.
    /// </summary>
    /// <param name="chapter">The Chapter entity to add.</param>
    Task AddAsync(Chapter chapter);

    /// <summary>
    /// Gets all Chapter entities from the data store.
    /// </summary>
    /// <returns>An asynchronous task returning an enumerable collection of all Chapter entities.</returns>
    Task<IEnumerable<Chapter>> GetAllAsync();
}