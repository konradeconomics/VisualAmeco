using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Interfaces;
using VisualAmeco.Data.Contexts;
using VisualAmeco.Parser.Utilities;

namespace VisualAmeco.Parser.Parsers;

public class AmecoCsvParser : IAmecoCsvParser
{
    private readonly ILogger<AmecoCsvParser> _logger;
    private readonly CsvFileReader _csvFileReader;
    private readonly IChapterRepository _chapterRepository;
    private readonly ISubchapterRepository _subchapterRepository;
    private readonly IVariableRepository _variableRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IValueRepository _valueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AmecoCsvParser(
        ILogger<AmecoCsvParser> logger,
        CsvFileReader csvFileReader,
        IChapterRepository chapterRepository,
        ISubchapterRepository subchapterRepository,
        IVariableRepository variableRepository,
        ICountryRepository countryRepository,
        IValueRepository valueRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _csvFileReader = csvFileReader;
        _chapterRepository = chapterRepository;
        _subchapterRepository = subchapterRepository;
        _variableRepository = variableRepository;
        _countryRepository = countryRepository;
        _valueRepository = valueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ParseAndSaveAsync(string filePath)
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
                .Where(x => !int.TryParse(x.col, out _))
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
                .Skip(columnIndices.Count)
                .Where(col => int.TryParse(col, out _))
                .ToList();

            foreach (var row in records.Skip(1))
            {
                string subchapterName = row[columnIndices["SUB-CHAPTER"]];
                if (!SubchapterToChapterMap.Mapping.TryGetValue(subchapterName, out var chapterName))
                {
                    _logger.LogWarning($"No chapter mapping for subchapter '{subchapterName}'. Using 'Unknown Chapter'.");
                    chapterName = "Unknown Chapter";
                }

                var chapter = await _chapterRepository.GetByNameAsync(chapterName)
                              ?? new Chapter { Name = chapterName };
                await _chapterRepository.AddAsync(chapter);

                var subchapter = await _subchapterRepository.GetByNameAsync(subchapterName, chapter.Id)
                                 ?? new Subchapter { Name = subchapterName, ChapterId = chapter.Id };
                await _subchapterRepository.AddAsync(subchapter);

                string variableCode = row[columnIndices["CODE"]];
                string variableName = row[columnIndices["TITLE"]];
                string unit = row[columnIndices["UNIT"]];
                var variable = await _variableRepository.GetByCodeAsync(variableCode)
                               ?? new Variable
                               {
                                   Code = variableCode,
                                   Name = variableName,
                                   Unit = unit,
                                   SubChapterId = subchapter.Id
                               };
                await _variableRepository.AddAsync(variable);
                
                string countryCode = row[columnIndices["CNTRY"]];
                string countryName = row[columnIndices["COUNTRY"]];
                var country = await _countryRepository.GetByCodeAsync(countryCode)
                              ?? new Country { Code = countryCode, Name = countryName };
                await _countryRepository.AddAsync(country);

                foreach (var yearColumn in yearColumns)
                {
                    int year = int.Parse(yearColumn);
                    decimal amount = decimal.TryParse(row[header.ToList().IndexOf(yearColumn)], out var val) ? val : 0;

                    var value = new Value
                    {
                        VariableId = variable.Id,
                        CountryId = country.Id,
                        Year = year,
                        Month = null,
                        Amount = amount,
                        IsMonthly = false
                    };

                    await _valueRepository.AddAsync(value);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while parsing file: {filePath}");
            return false;
        }
    }
}