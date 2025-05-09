namespace VisualAmeco.Application.Interfaces;

/// <summary>
/// Saves the mapped AMECO entity.
/// </summary>
public interface IAmecoCsvParser
{
    Task<bool> ParseAndSaveAsync(List<string> filePaths);
}