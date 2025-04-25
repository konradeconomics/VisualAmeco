using VisualAmeco.Core.Entities;

namespace VisualAmeco.Core.Interfaces;

public interface IChapterRepository
{
    Task<Chapter?> GetByNameAsync(string name);
    Task AddAsync(Chapter chapter);
}