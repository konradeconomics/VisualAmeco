using Microsoft.EntityFrameworkCore;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Interfaces;
using VisualAmeco.Data.Contexts;

namespace VisualAmeco.Data.Repositories;

public class VariableRepository : IVariableRepository
{
    private readonly VisualAmecoDbContext _context;
    
    
    public VariableRepository(VisualAmecoDbContext context)
    {
        _context = context;
    }
    
    public async Task<Variable?> GetByCodeAsync(string code)
        => await _context.Variables
            .FirstOrDefaultAsync(v => v.Code == code);
    
    public async Task AddAsync(Variable variable)
        => await _context.Variables.AddAsync(variable);
}