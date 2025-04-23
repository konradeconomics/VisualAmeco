using Microsoft.Extensions.Logging;
using VisualAmeco.Parser.Models;
using VisualAmeco.Parser.Services.Interfaces;
using VisualAmeco.Parser.Utilities;

namespace VisualAmeco.Parser.Services;

public class CsvRowMapper : ICsvRowMapper
{
    private readonly ILogger<CsvRowMapper> _logger;

    public CsvRowMapper(ILogger<CsvRowMapper> logger)
    {
        _logger = logger;
    }

    public Task<MapResult<MappedAmecoRow>> MapAsync(
        string[] row,
        string[] header,
        Dictionary<string, int> colIndices,
        List<string> yearCols)
    {
        try
        {
            var subchapter = row[colIndices["SUB-CHAPTER"]];
            var chapter = SubchapterToChapterMap.Mapping.GetValueOrDefault(subchapter, "Unknown Chapter");

            var mapped = new MappedAmecoRow
            {
                ChapterName = chapter,
                SubchapterName = subchapter,
                VariableCode = row[colIndices["CODE"]],
                VariableName = row[colIndices["TITLE"]],
                Unit = row[colIndices["UNIT"]],
                CountryCode = row[colIndices["CNTRY"]],
                CountryName = row[colIndices["COUNTRY"]],
                Values = yearCols.Select(yearStr =>
                {
                    var yearIndex = Array.IndexOf(header, yearStr);
                    var parsed = decimal.TryParse(row[yearIndex], out var amount) ? amount : 0;
                    return new YearValue
                    {
                        Year = int.Parse(yearStr),
                        Amount = parsed
                    };
                }).ToList()
            };

            return Task.FromResult(MapResult<MappedAmecoRow>.Success(mapped));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map row.");
            return Task.FromResult(MapResult<MappedAmecoRow>.Fail("Failed to parse row."));
        }
    }
}
