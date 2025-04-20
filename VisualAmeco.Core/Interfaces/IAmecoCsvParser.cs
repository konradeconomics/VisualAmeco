namespace VisualAmeco.Core.Interfaces;

public interface IAmecoCsvParser
{
    Task<bool> ParseAndSaveAsync(string filePath);
}