using VisualAmeco.Core.Entities;

namespace VisualAmeco.Core.Interfaces;

public interface IValueRepository
{
    Task<Value?> GetAsync(int variableId, int countryId, int year, int? month);
    Task AddAsync(Value value);
}