using Microsoft.Extensions.Logging;
using VisualAmeco.Core.Interfaces;
using VisualAmeco.Parser.Services.Interfaces;

namespace VisualAmeco.Parser.Parsers;

public class AmecoCsvParser : IAmecoCsvParser
{
    private readonly ICsvFileReader _csvFileReader;
    private readonly ICsvRowMapper _csvRowMapper;
    private readonly IAmecoEntitySaver _entitySaver;
    private readonly ILogger<AmecoCsvParser> _logger;

    public AmecoCsvParser(
        ICsvFileReader csvFileReader,
        ICsvRowMapper csvRowMapper,
        IAmecoEntitySaver entitySaver,
        ILogger<AmecoCsvParser> logger)
    {
        _csvFileReader = csvFileReader;
        _csvRowMapper = csvRowMapper;
        _entitySaver = entitySaver;
        _logger = logger;
    }

    public async Task<bool> ParseAndSaveAsync(List<string> filePaths)
    {
        var records = await _csvFileReader.ReadFileAsync(filePaths);

        if (!records.Any())
        {
            _logger.LogWarning("No data found in the provided CSV files.");
            return false;
        }

        var header = records.First();
        var columnIndices = header
            .Select((col, index) => new { col, index })
            .Where(x => !int.TryParse(x.col, out _))
            .ToDictionary(x => x.col, x => x.index);

        var requiredColumns = new[]
        {
            "SERIES", "CNTRY", "TRN", "AGG", "UNIT", "REF",
            "CODE", "SUB-CHAPTER", "TITLE", "COUNTRY"
        };

        foreach (var required in requiredColumns)
        {
            if (!columnIndices.ContainsKey(required))
            {
                _logger.LogError($"Missing required column: {required}");
                return false;
            }
        }

        var yearColumns = header
            .Skip(columnIndices.Count)
            .Where(col => int.TryParse(col, out _))
            .ToList();

        foreach (var row in records.Skip(1))
        {
            try
            {
                var mapResult = await _csvRowMapper.MapAsync(row, header, columnIndices, yearColumns);

                if (!mapResult.IsSuccess)
                {
                    _logger.LogWarning($"Skipping row due to mapping error: {mapResult.ErrorMessage}");
                    continue;
                }

                await _entitySaver.SaveAsync(mapResult.Value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing row");
            }
        }

        _logger.LogInformation("Parsing and saving completed successfully.");
        return true;
    }
}