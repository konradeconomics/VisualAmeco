namespace VisualAmeco.Core.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}