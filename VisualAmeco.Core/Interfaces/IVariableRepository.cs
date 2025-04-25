using VisualAmeco.Core.Entities;

namespace VisualAmeco.Core.Interfaces;

public interface IVariableRepository
{
    Task<Variable?> GetByCodeAsync(string code);
    Task AddAsync(Variable variable);
}