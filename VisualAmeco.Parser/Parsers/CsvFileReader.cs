using System.Globalization;
using CsvHelper;

namespace VisualAmeco.Parser.Parsers;

public class CsvFileReader : FileReader
{
    public CsvFileReader(string filePath) : base(filePath)
    {
    }

    public override async Task<IEnumerable<string[]>> ReadFileAsync()
    {
        var records = new List<string[]>();
        try
        {
            using (var reader = new StreamReader(FilePath))
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
            Console.WriteLine($"Error reading CSV file '{FilePath}': {ex.Message}");
            return Enumerable.Empty<string[]>();
        }
        return records;
    }
}