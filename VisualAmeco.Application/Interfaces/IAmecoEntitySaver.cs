using VisualAmeco.Application.DTOs;

namespace VisualAmeco.Application.Interfaces;

public interface IAmecoEntitySaver
{
    Task SaveAsync(MappedAmecoRow row);
}