using System.Globalization;
using Microsoft.Extensions.Logging;

namespace VisualAmeco.Parser.Parsers;

public class AmecoCsvParser : ParserBase
{
    public AmecoCsvParser(ILogger logger) : base(logger) {}

    public override async Task<bool> ParseAndSaveAsync(string filePath)
    {
        var reader = new CsvFileReader(filePath);
        var rows = await reader.ReadFileAsync();
        if(!rows.Any()) return false;
        
        var header = rows[0];
        var dataRows = rows.Skip(1);
        
        int firstYearIndex = Array.FindIndex(header, h => int.TryParse(h, out _));
        var yearColumns = header.Skip(firstYearIndex).ToList();
        
        foreach (var row in dataRows)
        {
            try
            {
                string title = row[0];
                string code = row[1];
                string countryName = row[2];
                string? unit = row[3];

                var chapter = await FindOrCreateChapterAsync("Ameco"); // maybe use title or section logic
                var subchapter = await FindOrCreateSubchapterAsync("Default", chapter); // or extract from title
                var variable = await FindOrCreateVariableAsync(code, title, unit, subchapter);
                var country = await FindOrCreateCountryAsync(countryName, countryName); // assuming name == code for now

                for (int i = firstYearIndex; i < row.Length; i++)
                {
                    if (decimal.TryParse(row[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    {
                        int year = int.Parse(header[i]); // e.g., "1960"
                        var record = await CreateValueAsync(variable, country, year, null, value, isMonthly: false);

                        // You can insert into the DB here, or collect and batch later
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to parse row: {ex.Message}");
            }
        }

        return true;
    }
}