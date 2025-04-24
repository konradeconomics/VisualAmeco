using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using VisualAmeco.Application.Interfaces;
using VisualAmeco.Core.Interfaces;

namespace VisualAmeco.Parser.Parsers;

/// <summary>
/// Reads AMECO CSV files using CsvHelper, processing one file at a time.
/// Implements the ICsvFileReader interface for the refactored design.
/// </summary>
public class CsvFileReader : ICsvFileReader
{
    private readonly ILogger<CsvFileReader> _logger;

    public CsvFileReader(ILogger<CsvFileReader> logger)
    {
        _logger = logger;
    }

    public async Task<(string[]? Header, List<string[]> Rows)?> ReadSingleFileAsync(string filePath)
    {
        _logger.LogDebug("Attempting to read CSV file: {FilePath}", filePath);
        string[]? header = null;
        var rows = new List<string[]>();

        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                await csv.ReadAsync();
                if (csv.ReadHeader())
                {
                    header = csv.HeaderRecord;
                    if (header == null)
                    {
                        _logger.LogWarning("CsvReader read header successfully, but HeaderRecord is null in file: {FilePath}", filePath);
                        return null;
                    }
                    _logger.LogDebug("Read header with {Count} columns from {FilePath}", header.Length, filePath);

                    while (await csv.ReadAsync())
                    {
                        var record = new List<string>();
                        for (int i = 0; i < header.Length; i++)
                        {
                            var fieldValue = csv.GetField(i);
                            record.Add(fieldValue ?? "");
                        }
                        rows.Add(record.ToArray());
                    }

                    _logger.LogDebug("Read {Count} data rows from {FilePath}", rows.Count, filePath);
                    return (header, rows);
                }
                else
                {
                    _logger.LogWarning("No header record found in CSV file: {FilePath}", filePath);
                    return null;
                }
            }
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "IO error reading CSV file '{FilePath}'", filePath);
            return null;
        }
        catch (CsvHelper.CsvHelperException csvEx)
        {
            _logger.LogError(csvEx, "CSV parsing error in file '{FilePath}' at Row {Row}", filePath, csvEx.Context?.Parser?.Row ?? 0);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error reading CSV file '{FilePath}'", filePath);
            return null;
        }
    }
}