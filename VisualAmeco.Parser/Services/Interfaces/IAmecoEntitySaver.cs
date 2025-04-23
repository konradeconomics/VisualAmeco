using VisualAmeco.Parser.Models;

namespace VisualAmeco.Parser.Services.Interfaces;

public interface IAmecoEntitySaver
{
    Task SaveAsync(MappedAmecoRow row);
}