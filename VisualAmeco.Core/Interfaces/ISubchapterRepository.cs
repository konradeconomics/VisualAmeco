using VisualAmeco.Core.Entities;

namespace VisualAmeco.Core.Interfaces;

public interface ISubchapterRepository
{
    Task<Subchapter?> GetByNameAsync(string name, int chapterId);
    Task AddAsync(Subchapter subchapter);
}