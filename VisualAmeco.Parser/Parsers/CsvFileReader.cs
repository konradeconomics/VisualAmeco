using System.Globalization;
using CsvHelper;
using VisualAmeco.Core.Interfaces;

namespace VisualAmeco.Parser.Parsers;

public class CsvFileReader : ICsvFileReader
{
    private List<string> _filePaths;

    public CsvFileReader()
    {
        _filePaths = new List<string>();
    }

    public void SetFilePaths(List<string> filePaths)
    {
        _filePaths = filePaths;
    }

    public async Task<IEnumerable<string[]>> ReadFileAsync()
    {
        var allRecords = new List<string[]>();

        foreach (var filePath in _filePaths)
        {
            var records = await ReadCsvFileAsync(filePath);
            allRecords.AddRange(records);
        }

        return allRecords;
    }

    private async Task<IEnumerable<string[]>> ReadCsvFileAsync(string filePath)
    {
        var records = new List<string[]>();
        try
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                await csv.ReadAsync();
                csv.ReadHeader();
                while (await csv.ReadAsync())
                {
                    var record = new List<string>();
                    for (int i = 0; csv.TryGetField(i, out string? value); i++)
                    {
                        record.Add(value ?? "");
                    }
                    records.Add(record.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading CSV file '{filePath}': {ex.Message}");
            return Enumerable.Empty<string[]>();
        }

        return records;
    }
}