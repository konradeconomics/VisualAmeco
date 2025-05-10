using System.Globalization;
using Microsoft.Extensions.Logging;
using VisualAmeco.Application.DTOs;
using VisualAmeco.Application.Interfaces;
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
        List<string> yearCols,
        string chapterName)
    {
        const string unknownChapter = "Unknown Chapter";
        try
        {
            if (!colIndices.ContainsKey("SUB-CHAPTER") || row.Length <= colIndices["SUB-CHAPTER"])
                return Task.FromResult(MapResult<MappedAmecoRow>.Fail("Missing or invalid SUB-CHAPTER index/data"));
            if (!colIndices.ContainsKey("CODE") || row.Length <= colIndices["CODE"])
                return Task.FromResult(MapResult<MappedAmecoRow>.Fail("Missing or invalid CODE index/data"));
            if (!colIndices.ContainsKey("TITLE") || row.Length <= colIndices["TITLE"])
                return Task.FromResult(MapResult<MappedAmecoRow>.Fail("Missing or invalid TITLE index/data"));
            if (!colIndices.ContainsKey("UNIT_CODE") || row.Length <= colIndices["UNIT_CODE"])
                return Task.FromResult(MapResult<MappedAmecoRow>.Fail("Missing or invalid UNIT_CODE index/data"));
            if (!colIndices.ContainsKey("UNIT_DESCRIPTION") || row.Length <= colIndices["UNIT_DESCRIPTION"])
                return Task.FromResult(
                    MapResult<MappedAmecoRow>.Fail("Missing or invalid UNIT_DESCRIPTION index/data"));
            if (!colIndices.ContainsKey("CNTRY") || row.Length <= colIndices["CNTRY"])
                return Task.FromResult(MapResult<MappedAmecoRow>.Fail("Missing or invalid CNTRY index/data"));
            if (!colIndices.ContainsKey("COUNTRY") || row.Length <= colIndices["COUNTRY"])
                return Task.FromResult(MapResult<MappedAmecoRow>.Fail("Missing or invalid COUNTRY index/data"));
            if (!colIndices.ContainsKey("TRN") || row.Length <= colIndices["TRN"])
                return Task.FromResult(MapResult<MappedAmecoRow>.Fail("Missing or invalid TRN index/data"));
            if (!colIndices.ContainsKey("AGG") || row.Length <= colIndices["AGG"])
                return Task.FromResult(MapResult<MappedAmecoRow>.Fail("Missing or invalid AGG index/data"));
            if (!colIndices.ContainsKey("REF") || row.Length <= colIndices["REF"])
                return Task.FromResult(MapResult<MappedAmecoRow>.Fail("Missing or invalid REF index/data"));

            // --- Read Base Data ---
            var subchapterName = row[colIndices["SUB-CHAPTER"]];
            var variableCode = row[colIndices["CODE"]];
            var chapter = string.IsNullOrEmpty(chapterName) ? unknownChapter : chapterName;

            // --- Prepare Mapped Object ---
            var mapped = new MappedAmecoRow
            {
                ChapterName = chapter,
                SubchapterName = subchapterName,
                VariableCode = variableCode,
                VariableName = row[colIndices["TITLE"]],
                UnitCode = row[colIndices["UNIT_CODE"]],
                UnitDescription = row[colIndices["UNIT_DESCRIPTION"]],
                CountryCode = row[colIndices["CNTRY"]],
                CountryName = row[colIndices["COUNTRY"]],
                Values = new List<YearValue>(),
                TRN = row[colIndices["TRN"]],
                AGG = row[colIndices["AGG"]],
                REF = row[colIndices["REF"]],
            };

            // --- Process Year Values ---
            foreach (var yearStr in yearCols)
            {
                var yearIndex = Array.IndexOf(header, yearStr);

                if (yearIndex < 0)
                {
                    _logger.LogError(
                        "Requested year column '{Year}' was not found in the CSV header. This should not happen if header processing is correct. File context: '{ChapterName}'.",
                        yearStr, chapterName);
                    throw new KeyNotFoundException(
                        $"Requested year column '{yearStr}' was not found in the CSV header. This indicates an issue with header/year column alignment.");
                }

                if (row.Length <= yearIndex)
                {
                    _logger.LogWarning(
                        "Row is too short for year column '{Year}' (Index {YearIndex}). Code: {Code}. Skipping year value.",
                        yearStr, yearIndex, variableCode);
                    continue;
                }

                var parsed =
                    decimal.TryParse(row[yearIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount)
                        ? amount
                        : 0m;

                if (!int.TryParse(yearStr, out int yearInt))
                {
                    _logger.LogWarning(
                        "Could not parse year column header '{Year}' to integer. Skipping year value. Code: {Code}",
                        yearStr, variableCode);
                    continue;
                }

                mapped.Values.Add(new YearValue
                {
                    Year = yearInt,
                    Amount = parsed
                });
            }

            return Task.FromResult(MapResult<MappedAmecoRow>.Success(mapped));
        }
        catch (Exception ex)
        {
            string safeCode = "N/A";
            if (colIndices.TryGetValue("CODE", out int codeIdx) && row.Length > codeIdx)
            {
                safeCode = row[codeIdx];
            }

            string safeSubChapter = "N/A";
            if (colIndices.TryGetValue("SUB-CHAPTER", out int subIdx) && row.Length > subIdx)
            {
                safeSubChapter = row[subIdx];
            }

            _logger.LogError(ex,
                "Failed to map row. Code: {Code}, SubChapter: {SubChapter}, ChapterContext: {Chapter}. Error: {ErrorMessage}",
                safeCode,
                safeSubChapter,
                chapterName,
                ex.Message);

            return Task.FromResult(
                MapResult<MappedAmecoRow>.Fail(
                    $"Failed to parse row (Code: {safeCode}) in chapter {chapterName}: {ex.Message}"));
        }
    }
}
