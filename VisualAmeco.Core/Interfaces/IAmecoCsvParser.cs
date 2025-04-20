namespace VisualAmeco.Core.Interfaces;

public interface IAmecoCsvParser
{
    Task<bool> ParseAndSaveAsync(List<string> filePaths);
}