using VisualAmeco.Core.Entities;

namespace VisualAmeco.Core.Interfaces;

/// <summary>
/// Interface defining repository operations for Subchapter entities.
/// </summary>
public interface ISubchapterRepository
{
    /// <summary>
    /// Gets a specific Subchapter entity by its name and associated chapter ID.
    /// </summary>
    /// <param name="name">The subchapter name.</param>
    /// <param name="chapterId">The ID of the parent chapter.</param>
    /// <returns>The matching Subchapter entity or null if not found.</returns>
    Task<Subchapter?> GetByNameAsync(string name, int chapterId);

    /// <summary>
    /// Stages a Subchapter entity for addition to the data store.
    /// </summary>
    /// <param name="subchapter">The Subchapter entity to add.</param>
    Task AddAsync(Subchapter subchapter);

    // --- NEW METHODS ADDED ---
    /// <summary>
    /// Gets all Subchapter entities from the data store.
    /// </summary>
    /// <returns>An asynchronous task returning an enumerable collection of all Subchapter entities.</returns>
    Task<IEnumerable<Subchapter>> GetAllAsync();

    /// <summary>
    /// Gets all Subchapter entities belonging to a specific Chapter.
    /// </summary>
    /// <param name="chapterId">The ID of the parent chapter.</param>
    /// <returns>An asynchronous task returning an enumerable collection of matching Subchapter entities.</returns>
    Task<IEnumerable<Subchapter>> GetByChapterAsync(int chapterId);
}