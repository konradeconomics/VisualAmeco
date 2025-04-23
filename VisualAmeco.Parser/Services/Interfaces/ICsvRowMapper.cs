using VisualAmeco.Parser.Models;

namespace VisualAmeco.Parser.Services.Interfaces;

public interface ICsvRowMapper
{
    Task<MapResult<MappedAmecoRow>> MapAsync(string[] row, string[] header, Dictionary<string, int> colIndices, List<string> yearCols);
}