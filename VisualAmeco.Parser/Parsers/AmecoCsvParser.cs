using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VisualAmeco.Core.Entities;
using VisualAmeco.Data.Contexts;
using VisualAmeco.Parser.Utilities;

namespace VisualAmeco.Parser.Parsers;

public class AmecoCsvParser : ParserBase
{
    private readonly VisualAmecoDbContext _context;
    private readonly CsvFileReader _csvFileReader;

    public AmecoCsvParser(ILogger logger, VisualAmecoDbContext context, CsvFileReader csvFileReader)
        : base(logger)
    {
        _context = context;
        _csvFileReader = csvFileReader;
    }

    public async Task ProcessAmecoFiles(List<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            _logger.LogInformation($"Processing file: {filePath}");
            bool success = await ParseAndSaveAsync(filePath);
            if (success)
            {
                _logger.LogInformation($"Successfully processed: {filePath}");
            }
            else
            {
                _logger.LogError($"Failed to process: {filePath}");
            }
        }
    }
    
    public override async Task<bool> ParseAndSaveAsync(string filePath)
    {
        try
        {
            var records = await _csvFileReader.ReadFileAsync();

            if (!records.Any())
            {
                _logger.LogError("No records found in the CSV file.");
                return false;
            }

            var header = records.First();

            var columnIndices = header
                .Select((col, index) => new { col, index })
                .Where(x => !int.TryParse(x.col, out _)) // Exclude year columns
                .ToDictionary(x => x.col, x => x.index);

            var requiredColumns = new[] { "SERIES", "CNTRY", "TRN", "AGG", "UNIT", "REF", "CODE", "SUB-CHAPTER", "TITLE", "COUNTRY" };
            foreach (var requiredColumn in requiredColumns)
            {
                if (!columnIndices.ContainsKey(requiredColumn))
                {
                    _logger.LogError($"Missing required column: {requiredColumn}");
                    return false;
                }
            }

            var yearColumns = header
                .Skip(columnIndices.Count) // Skip the fixed columns
                .Where(col => int.TryParse(col, out _)) // Filter to keep only the year columns
                .ToList();

            foreach (var row in records.Skip(1))
            {
                // Use the column name lookup to find the correct column
                string series = row[columnIndices["SERIES"]];
                string countryCode = row[columnIndices["CNTRY"]];
                string transformationType = row[columnIndices["TRN"]];
                string aggregationMode = row[columnIndices["AGG"]];
                string unit = row[columnIndices["UNIT"]];
                string referenceCode = row[columnIndices["REF"]];
                string variableCode = row[columnIndices["CODE"]];
                string subchapterNameFromCsv = row[columnIndices["SUB-CHAPTER"]]; // Use the value from the CSV
                string variableName = row[columnIndices["TITLE"]];
                string countryName = row[columnIndices["COUNTRY"]];

                // 1. Look up the Chapter name using the SubchapterToChapterMap
                if (!SubchapterToChapterMap.Mapping.TryGetValue(subchapterNameFromCsv, out var chapterName))
                {
                    _logger.LogWarning($"Could not find a chapter mapping for subchapter: {subchapterNameFromCsv}. Using 'Unknown Chapter'.");
                    chapterName = "Unknown Chapter";
                }

                // 2. Find or create the Chapter
                var chapter = await FindOrCreateChapterAsync(chapterName);
                if (chapter == null) continue;

                // 3. Find or create the Subchapter based on the SUB-CHAPTER name and chapter
                var subchapter = await FindOrCreateSubchapterAsync(subchapterNameFromCsv, chapter);
                if (subchapter == null) continue;

                // 4. Find or create the Variable using the CODE, TITLE, UNIT, TRN, AGG, and REF
                var variable = await FindOrCreateVariableAsync(variableCode, variableName, unit, subchapter);
                if (variable == null) continue;

                // 5. Find or create the Country based on the CNTRY code
                var country = await FindOrCreateCountryAsync(countryCode, countryName);
                if (country == null) continue;

                // 6. Parse the dynamic record and create corresponding Value entities for each year
                for (int i = 0; i < yearColumns.Count; i++)
                {
                    var yearColumn = yearColumns[i];
                    int year = int.Parse(yearColumn); // Convert year string to integer
                    decimal amount = decimal.TryParse(row[columnIndices[yearColumn]], out var parsedAmount) ? parsedAmount : 0;

                    await CreateValueAsync(variable, country, year, null, amount, isMonthly: false);
                }
            }

            await _context.SaveChangesAsync(); // Commit changes to the database
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while parsing CSV: {ex.Message}");
            return false;
        }
    }

    protected override async Task<Chapter?> FindOrCreateChapterAsync(string chapterName)
    {
        var chapter = await _context.Chapters
            .FirstOrDefaultAsync(c => c.Name == chapterName);

        if (chapter == null)
        {
            chapter = new Chapter { Name = chapterName };
            _context.Chapters.Add(chapter);
        }

        return chapter;
    }

    protected override async Task<Subchapter?> FindOrCreateSubchapterAsync(string subchapterName, Chapter chapter)
    {
        var subchapter = await _context.Subchapters
            .FirstOrDefaultAsync(s => s.Name == subchapterName && s.ChapterId == chapter.Id);

        if (subchapter == null)
        {
            subchapter = new Subchapter
            {
                Name = subchapterName,
                ChapterId = chapter.Id
            };
            _context.Subchapters.Add(subchapter);
        }

        return subchapter;
    }

    protected override async Task<Variable?> FindOrCreateVariableAsync(string variableCode, string variableName, string? unit, Subchapter subchapter)
    {
        var variable = await _context.Variables
            .FirstOrDefaultAsync(v => v.Code == variableCode);

        if (variable == null)
        {
            variable = new Variable
            {
                Code = variableCode,
                Name = variableName,
                Unit = unit,
                SubChapterId = subchapter.Id
            };
            _context.Variables.Add(variable);
        }

        return variable;
    }

    protected override async Task<Country?> FindOrCreateCountryAsync(string countryCode, string countryName)
    {
        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Code == countryCode);

        if (country == null)
        {
            country = new Country
            {
                Code = countryCode,
                Name = countryName
            };
            _context.Countries.Add(country);
        }

        return country;
    }

    protected override async Task<Value> CreateValueAsync(Variable variable, Country country, int year, int? month, decimal amount, bool isMonthly)
    {
        var value = new Value
        {
            VariableId = variable.Id,
            CountryId = country.Id,
            Year = year,
            Month = month,
            Amount = amount,
            IsMonthly = isMonthly
        };

        _context.Values.Add(value);
        return value;
    }
}