namespace VisualAmeco.Core.Interfaces;

/// <summary>
/// Reads a SINGLE CSV file, returning its header and rows.
/// </summary>
public interface ICsvFileReader
{
    /// <summary>
    /// Reads the specified CSV file.
    /// </summary>
    /// <param name="filePath">Path to the CSV file.</param>
    /// <returns>A tuple containing the header array and a list of row arrays, or null if the file cannot be read.</returns>
    Task<(string[]? Header, List<string[]> Rows)?> ReadSingleFileAsync(string filePath);
}