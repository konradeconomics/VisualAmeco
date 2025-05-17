using System.Globalization;
using Microsoft.Extensions.Logging;
using VisualAmeco.Application.Interfaces;


namespace VisualAmeco.Application.Services;

public class AmecoCsvParser : IAmecoCsvParser
{
    private readonly ICsvFileReader _csvFileReader;
    private readonly ICsvRowMapper _csvRowMapper;
    private readonly IAmecoEntitySaver _entitySaver;
    private readonly ILogger<AmecoCsvParser> _logger;

    private static readonly IReadOnlyDictionary<int, string> ChapterNumberToName = new Dictionary<int, string>
    {
        { 1, "Population And Employment" }, { 2, "Consumption" },
        { 3, "Capital Formation and Saving, Total Economy and Sectors" },
        { 4, "Domestic and Final Demand" }, { 5, "National Income" }, { 6, "Domestic Product" },
        { 7, "Gross Domestic Product (Income Approach), Labour Costs" }, { 8, "Capital Stock" },
        { 9, "Exports and Imports of Goods and Services, National Accounts" },
        { 10, "Balances with the Rest of the World" }, { 11, "Foreign Trade" },
        { 12, "National Accounts by Branch of Activity" },
        { 13, "Monetary Variables" }, { 14, "Corporations (S11 + S12)" }, { 15, "Households And Npish (S14 + S15)" },
        { 16, "General Government (S13)" }, { 17, "Cyclical Adjustment of Public Finance Variables" },
        { 18, "Gross Public Debt" }
    };

    private static readonly string[] RequiredColumns = new[]
    {
        "CODE", "SUB-CHAPTER", "TITLE", "UNIT_CODE", "UNIT_DESCRIPTION", "CNTRY", "COUNTRY",
        "TRN", "AGG", "REF"
    };

    public AmecoCsvParser(
        ICsvFileReader csvFileReader,
        ICsvRowMapper csvRowMapper,
        IAmecoEntitySaver entitySaver,
        ILogger<AmecoCsvParser> logger)
    {
        _csvFileReader = csvFileReader ?? throw new ArgumentNullException(nameof(csvFileReader));
        _csvRowMapper = csvRowMapper ?? throw new ArgumentNullException(nameof(csvRowMapper));
        _entitySaver = entitySaver ?? throw new ArgumentNullException(nameof(entitySaver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ParseAndSaveAsync(List<string> filePaths)
    {
        if (filePaths == null || !filePaths.Any())
        {
            _logger.LogWarning("No file paths provided for parsing.");
            return false;
        }

        int totalRowsProcessedOverall = 0;
        int totalRowsSavedOverall = 0;
        int totalFilesSuccessfullyProcessed = 0;
        bool encounteredAnyFatalErrorInBatch = false;

        foreach (var filePath in filePaths)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("File not found: {FilePath}. Skipping.", filePath);
                encounteredAnyFatalErrorInBatch = true;
                continue;
            }

            string chapterName = DetermineChapterName(filePath);
            _logger.LogInformation("Processing File: {FilePath} as Chapter: [{ChapterName}]", filePath, chapterName);

            try
            {
                var fileData = await _csvFileReader.ReadSingleFileAsync(filePath);

                if (fileData == null || fileData.Value.Header == null || !fileData.Value.Rows.Any())
                {
                    _logger.LogWarning("No header or data read (or reader failed) for file: {FilePath}. Skipping.",
                        filePath);
                    continue;
                }

                var (header, rows) = fileData.Value;

                var headerProcessingResult = ProcessHeader(header, filePath);
                if (!headerProcessingResult.IsValid)
                {
                    _logger.LogError("Header validation failed for {FilePath}. Skipping file. Reason: {Reason}",
                        filePath, headerProcessingResult.ErrorMessage);
                    encounteredAnyFatalErrorInBatch = true; // Header failure is fatal for overall success
                    continue;
                }

                var columnIndices = headerProcessingResult.ColumnIndices;
                var yearColumns = headerProcessingResult.YearColumns;

                int fileRowsProcessed = 0;
                int fileRowsSaved = 0;
                foreach (var row in rows)
                {
                    totalRowsProcessedOverall++;
                    fileRowsProcessed++;
                    try
                    {
                        var mapResult =
                            await _csvRowMapper.MapAsync(row, header, columnIndices, yearColumns, chapterName);

                        if (mapResult.IsSuccess && mapResult.Value != null)
                        {
                            await _entitySaver.SaveAsync(mapResult.Value);
                            totalRowsSavedOverall++;
                            fileRowsSaved++;
                        }
                        else
                        {
                            _logger.LogWarning("Skipping row {RowNum} in {FilePath} due to mapping error: {Error}",
                                fileRowsProcessed, filePath, mapResult.ErrorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing row {RowNum} in {FilePath}", fileRowsProcessed,
                            filePath);
                    }
                }

                _logger.LogInformation(
                    "Finished processing {FilePath}. Processed: {ProcessedCount} rows, Saved: {SavedCount} entities.",
                    filePath, fileRowsProcessed, fileRowsSaved);
                totalFilesSuccessfullyProcessed++;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,
                    ">>> UNEXPECTED FILE-LEVEL EXCEPTION CAUGHT FOR {FilePath}. Processing stopped for this file. <<<",
                    filePath);
            }
        }

        _logger.LogInformation(
            "Parsing and saving task completed. Files Successfully Processed: {FileCount}, Total CSV Rows Processed: {RowCount}, Total Entities Saved: {SavedCount}, Fatal Errors Encountered: {FatalErrorFlag}",
            totalFilesSuccessfullyProcessed, totalRowsProcessedOverall, totalRowsSavedOverall,
            encounteredAnyFatalErrorInBatch);
        return totalFilesSuccessfullyProcessed > 0 && !encounteredAnyFatalErrorInBatch;
    }

    private string DetermineChapterName(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        if (fileName != null && fileName.StartsWith("AMECO", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(fileName.AsSpan(5), NumberStyles.None, CultureInfo.InvariantCulture,
                    out int chapterNumber))
            {
                return ChapterNumberToName.GetValueOrDefault(chapterNumber, $"Unknown Chapter ({chapterNumber})");
            }
        }

        _logger.LogWarning("Could not determine chapter number from filename format: {FileName}", fileName);
        return "Unknown Chapter";
    }

    private (bool IsValid, Dictionary<string, int> ColumnIndices, List<string> YearColumns, string ErrorMessage)
        ProcessHeader(string[] header, string filePath)
    {
        var columnIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var yearColumns = new List<string>();
        bool foundFirstYear = false;
        bool firstUnitColumnFound = false;

        for (int i = 0; i < header.Length; i++)
        {
            var col = header[i]?.Trim();
            if (string.IsNullOrWhiteSpace(col))
            {
                _logger.LogWarning("Blank column header found at index {Index} in file {FilePath}", i, filePath);
                continue;
            }

            if (int.TryParse(col, out _))
            {
                foundFirstYear = true;
                yearColumns.Add(col);
            }
            else if (!foundFirstYear)
            {
                if (col.Equals("UNIT", StringComparison.OrdinalIgnoreCase))
                {
                    if (!firstUnitColumnFound)
                    {
                        if (!columnIndices.TryAdd("UNIT_CODE", i))
                        {
                            _logger.LogWarning(
                                "Could not add 'UNIT_CODE' for column '{Column}' at index {Index} in file {FilePath}.",
                                col, i, filePath);
                        }

                        firstUnitColumnFound = true;
                    }
                    else
                    {
                        if (!columnIndices.TryAdd("UNIT_DESCRIPTION", i))
                        {
                            _logger.LogWarning(
                                "Could not add 'UNIT_DESCRIPTION' for column '{Column}' at index {Index} in file {FilePath}.",
                                col, i, filePath);
                        }
                    }
                }
                else
                {
                    if (!columnIndices.TryAdd(col, i))
                    {
                        _logger.LogWarning(
                            "Duplicate non-numeric column header '{Column}' (not UNIT) found in file {FilePath} at index {Index}. Using first occurrence.",
                            col, filePath, i);
                    }
                }
            }
            else if (foundFirstYear)
            {
                _logger.LogWarning(
                    "Non-numeric column header '{Column}' found after year columns started in file {FilePath} at index {Index}.",
                    col, filePath, i);
                if (!columnIndices.TryAdd(col, i))
                {
                    _logger.LogWarning(
                        "Duplicate trailing non-numeric column header '{Column}' in file {FilePath} at index {Index}.",
                        col, filePath, i);
                }
            }
        }

        foreach (var required in RequiredColumns)
        {
            if (!columnIndices.ContainsKey(required))
            {
                return (false, columnIndices, yearColumns, $"Missing required column: {required}");
            }
        }

        if (!yearColumns.Any())
        {
            return (false, columnIndices, yearColumns, "No year columns found in header.");
        }

        return (true, columnIndices, yearColumns, string.Empty);
    }
}