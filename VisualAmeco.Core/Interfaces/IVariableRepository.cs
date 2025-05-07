using VisualAmeco.Core.Entities;

namespace VisualAmeco.Core.Interfaces;

/// <summary>
/// Interface defining repository operations for Variable entities.
/// </summary>
public interface IVariableRepository
{
    /// <summary>
    /// Gets a specific Variable entity by its unique code.
    /// </summary>
    /// <param name="code">The variable code.</param>
    /// <returns>The matching Variable entity or null if not found.</returns>
    Task<Variable?> GetByCodeAsync(string code);

    /// <summary>
    /// Stages a Variable entity for addition to the data store.
    /// </summary>
    /// <param name="variable">The Variable entity to add.</param>
    Task AddAsync(Variable variable);

    /// <summary>
    /// Gets a list of Variable entities, optionally filtered by chapterId and/or subchapterId.
    /// Includes the related Subchapter for mapping SubchapterName.
    /// </summary>
    /// <param name="chapterId">Optional ID of the chapter to filter by.</param>
    /// <param name="subchapterId">Optional ID of the subchapter to filter by.</param>
    /// <returns>An asynchronous task returning an enumerable collection of filtered Variable entities.</returns>
    Task<IEnumerable<Variable>> GetFilteredAsync(int? chapterId = null, int? subchapterId = null);
}