using VisualAmeco.Application.DTOs;

namespace VisualAmeco.Application.Interfaces;

/// <summary>
/// Maps a single row of CSV data based on header/context.
/// </summary>
public interface ICsvRowMapper
{
    /// <summary>
    /// Maps a CSV row to a MappedAmecoRow.
    /// </summary>
    /// <param name="row">The data row.</param>
    /// <param name="header">The header row.</param>
    /// <param name="colIndices">Dictionary mapping column names to indices.</param>
    /// <param name="yearCols">List of year column names.</param>
    /// <param name="chapterName">The chapter name determined from the file context.</param> <<< NEW
    /// <returns>A MapResult containing the mapped row or an error.</returns>
    Task<MapResult<MappedAmecoRow>> MapAsync(
        string[] row,
        string[] header,
        Dictionary<string, int> colIndices,
        List<string> yearCols,
        string chapterName // <<< NEW
    );
}