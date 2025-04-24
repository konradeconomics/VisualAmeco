using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using VisualAmeco.Core.Interfaces;
using VisualAmeco.Parser.Models;
using VisualAmeco.Parser.Parsers;
using VisualAmeco.Parser.Services.Interfaces;

namespace VisualAmeco.Testing.Parser.Parsers;

[TestFixture]
public class AmecoCsvParserTests{
    // --- Mocks for Dependencies ---
    private Mock<ICsvFileReader> _mockFileReader = null!;
    private Mock<ICsvRowMapper> _mockRowMapper = null!;
    private Mock<IAmecoEntitySaver> _mockEntitySaver = null!;
    private ILogger<AmecoCsvParser> _logger = NullLogger<AmecoCsvParser>.Instance;

    private AmecoCsvParser _parser = null!;

    private string _tempDirectory = null!;

    // Common test data elements
    private readonly string[] _validHeader = {
        // Required columns
        "CODE", "SUB-CHAPTER", "TITLE", "UNIT", "CNTRY", "COUNTRY",
        "SERIES", "TRN", "EXTRA_DATA",
        "2020", "2021", "2022"
    };

    [SetUp]
    public void Setup()
    {
        // Create fresh mocks
        _mockFileReader = new Mock<ICsvFileReader>();
        _mockRowMapper = new Mock<ICsvRowMapper>();
        _mockEntitySaver = new Mock<IAmecoEntitySaver>();

        // Create the parser instance
        _parser = new AmecoCsvParser(
            _mockFileReader.Object,
            _mockRowMapper.Object,
            _mockEntitySaver.Object,
            _logger
        );

        _tempDirectory = Path.Combine(Path.GetTempPath(), $"ParserTests_{TestContext.CurrentContext.Test.ID}");
        Directory.CreateDirectory(_tempDirectory);
        TestContext.WriteLine($"Created temp directory: {_tempDirectory}");

        // Default mock setups if any...
        _mockEntitySaver.Setup(s => s.SaveAsync(It.IsAny<MappedAmecoRow>()))
            .Returns(Task.CompletedTask);
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up the temporary directory after each test
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
                TestContext.WriteLine($"Deleted temp directory: {_tempDirectory}");
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"ERROR cleaning up temp directory '{_tempDirectory}': {ex.Message}");
        }
    }
    
    /// <summary>
    /// Sets up the mock row mapper to return a specific successful MapResult
    /// when MapAsync is called with a specific row array content.
    /// Allows other arguments (header, indices, etc.) to be flexible.
    /// </summary>
    /// <param name="specificRow">The exact string array representing the row to match.</param>
    /// <param name="resultToReturn">The MappedAmecoRow to return within a Success result.</param>
    private void SetupMapperSuccessForRow(string[] specificRow, MappedAmecoRow resultToReturn)
    {
        _mockRowMapper.Setup(m => m.MapAsync(
                // Use It.Is<T> with SequenceEqual for matching array content
                It.Is<string[]>(r => r.SequenceEqual(specificRow)),
                It.IsAny<string[]>(), // header
                It.IsAny<Dictionary<string, int>>(), // colIndices
                It.IsAny<List<string>>(), // yearCols
                It.IsAny<string>() // chapterName
            ))
            .ReturnsAsync(MapResult<MappedAmecoRow>.Success(resultToReturn));
    }

    // --- Helper to setup file reader mock for success ---
    private void SetupFileReaderSuccess(string filePath, string[] header, List<string[]> rows)
    {
        _mockFileReader.Setup(r => r.ReadSingleFileAsync(filePath))
            .ReturnsAsync((header, rows)); // Return tuple
    }

    // --- Helper to setup file reader mock for failure ---
    private void SetupFileReaderReturnsNull(string filePath)
    {
        _mockFileReader.Setup(r => r.ReadSingleFileAsync(filePath))
            .ReturnsAsync( ((string[]? Header, List<string[]> Rows)?) null ); // Return null tuple
    }

    // --- Helper to create a sample mapped row ---
    private MappedAmecoRow CreateMappedRow(string code, string chapter = "Test Chapter") =>
        new MappedAmecoRow { VariableCode = code, ChapterName = chapter /* Add other relevant props if needed */ };



    /// <summary>
    /// Tests that ParseAndSaveAsync returns false and does nothing if the input file list is null.
    /// </summary>
    [Test]
    public async Task ParseAndSaveAsync_NullFileList_ReturnsFalse()
    {
        // Arrange
        List<string>? filePaths = null;

        // Act
        var result = await _parser.ParseAndSaveAsync(filePaths!); // Use ! for test clarity

        // Assert
        Assert.IsFalse(result, "Should return false for null input.");
        _mockFileReader.Verify(r => r.ReadSingleFileAsync(It.IsAny<string>()), Times.Never, "File reader should not be called.");
        _mockRowMapper.Verify(m => m.MapAsync(It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Never, "Row mapper should not be called.");
        _mockEntitySaver.Verify(s => s.SaveAsync(It.IsAny<MappedAmecoRow>()), Times.Never, "Entity saver should not be called.");
    }

    /// <summary>
    /// Tests that ParseAndSaveAsync returns false and does nothing if the input file list is empty.
    /// </summary>
    [Test]
    public async Task ParseAndSaveAsync_EmptyFileList_ReturnsFalse()
    {
        // Arrange
        var filePaths = new List<string>();

        // Act
        var result = await _parser.ParseAndSaveAsync(filePaths);

        // Assert
        Assert.IsFalse(result, "Should return false for empty input list.");
        _mockFileReader.Verify(r => r.ReadSingleFileAsync(It.IsAny<string>()), Times.Never, "File reader should not be called.");
    }

    /// <summary>
    /// Tests the happy path where one valid file is provided, read, mapped, and saved successfully.
    /// </summary>
    [Test]
    public async Task ParseAndSaveAsync_OneValidFile_ReadsMapsSavesAndReturnsTrue()
    {
        // Arrange
        var fileName = "AMECO1.csv";
        var filePath = Path.Combine(_tempDirectory, fileName);
        var filePaths = new List<string> { filePath };
        var expectedChapterName = "Population And Employment";

        File.WriteAllText(filePath, string.Empty);
        TestContext.WriteLine($"Ensured empty file exists at: {filePath}");


        // Define mock data returned by the reader for this file path
        var header = _validHeader;
        // Ensure row data matches the structure expected by _validHeader/Indices/Years
        var row1 = new[] { "CODE1", "Sub1", "Title1", "Unit1", "C1", "Country1", "Extra1", "100", "110" };
        var row2 = new[] { "CODE2", "Sub2", "Title2", "Unit2", "C2", "Country2", "Extra2", "200", "220" };
        var rows = new List<string[]> { row1, row2 };

        // Define the objects returned by the mapper
        var mappedRow1 = CreateMappedRow("CODE1", expectedChapterName);
        var mappedRow2 = CreateMappedRow("CODE2", expectedChapterName);

        // Setup Mocks
        SetupFileReaderSuccess(filePath, header, rows);
        _mockRowMapper.Setup(m => m.MapAsync(
                            It.Is<string[]>(r => r.SequenceEqual(row1)), header,
                            It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), expectedChapterName))
                      .ReturnsAsync(MapResult<MappedAmecoRow>.Success(mappedRow1));
        _mockRowMapper.Setup(m => m.MapAsync(
                            It.Is<string[]>(r => r.SequenceEqual(row2)), header,
                            It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), expectedChapterName))
                      .ReturnsAsync(MapResult<MappedAmecoRow>.Success(mappedRow2));

        // Act
        var result = await _parser.ParseAndSaveAsync(filePaths);

        // Assert
        Assert.IsTrue(result, "Should return true for successful processing.");

        // Verify interactions
        _mockFileReader.Verify(r => r.ReadSingleFileAsync(filePath), Times.Once);
        // Verify mapper called for each row with correct arguments
        _mockRowMapper.Verify(m => m.MapAsync(row1, header, It.IsAny<Dictionary<string,int>>(), It.IsAny<List<string>>(), expectedChapterName), Times.Once);
        _mockRowMapper.Verify(m => m.MapAsync(row2, header, It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), expectedChapterName), Times.Once);
        // Verify saver called for each successfully mapped row
        _mockEntitySaver.Verify(s => s.SaveAsync(mappedRow1), Times.Once);
        _mockEntitySaver.Verify(s => s.SaveAsync(mappedRow2), Times.Once);
        _mockEntitySaver.Verify(s => s.SaveAsync(It.IsAny<MappedAmecoRow>()), Times.Exactly(2));
    }

    /// <summary>
    /// Tests that if the file reader returns null (e.g., file not found by reader, or read error),
    /// the parser skips processing rows for that file and returns false if no other files were processed.
    /// </summary>
    [Test]
    public async Task ParseAndSaveAsync_FileReaderReturnsNull_SkipsFileAndReturnsFalse()
    {
        // Arrange
        // Use the temp directory created in SetUp for consistent testing
        var fileName = "AMECO_BadRead.csv";
        var filePath = Path.Combine(_tempDirectory, fileName);
        var filePaths = new List<string> { filePath };

        File.WriteAllText(filePath, string.Empty); // Create empty file
        TestContext.WriteLine($"Created empty placeholder file for test: {filePath}");

        // Setup mocks: Configure the Reader mock to return null for this path,
        // simulating a failure during the actual read attempt.
        SetupFileReaderReturnsNull(filePath);

        // Act
        var result = await _parser.ParseAndSaveAsync(filePaths);

        // Assert
        Assert.IsFalse(result, "Should return false as no files were successfully processed.");

        // Verify the reader *was* called now, because File.Exists passed
        _mockFileReader.Verify(r => r.ReadSingleFileAsync(filePath), Times.Once);

        // Verify subsequent steps were skipped as expected after reader returned null
        _mockRowMapper.Verify(m => m.MapAsync(It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Never, "Mapper should not be called if reader returns null.");
        _mockEntitySaver.Verify(s => s.SaveAsync(It.IsAny<MappedAmecoRow>()), Times.Never, "Saver should not be called if reader returns null.");
    }

    /// <summary>
    /// Tests that if the header is invalid (missing a required column), the parser skips the file
    /// and returns false overall because a fatal error occurred.
    /// </summary>
    [Test]
    public async Task ParseAndSaveAsync_InvalidHeader_SkipsFileAndReturnsFalse()
    {
        // Arrange
        var fileName = "AMECO_BadHeader.csv";
        var filePath = Path.Combine(_tempDirectory, fileName);
        var filePaths = new List<string> { filePath };

        File.WriteAllText(filePath, string.Empty);
        TestContext.WriteLine($"Ensured empty file exists at: {filePath}");

        // Header is missing "CODE", which is required by ProcessHeader logic
        var invalidHeader = new[] { "SUB-CHAPTER", "TITLE", "UNIT", "CNTRY", "COUNTRY", "2020" };
        // Provide some dummy rows for the reader to return along with the bad header
        var rows = new List<string[]> { new[] { "Sub1", "Title1", "Unit1", "C1", "Country1", "100" } };

        // Setup the mock reader to successfully return the *invalid* header and dummy rows
        SetupFileReaderSuccess(filePath, invalidHeader, rows); // Assumes SetupFileReaderSuccess helper exists

        // Act
        // Parser should now call reader, get bad header, fail ProcessHeader,
        // log error, set encounteredFatalError=true, and skip row processing.
        var result = await _parser.ParseAndSaveAsync(filePaths);

        // Assert
        Assert.IsFalse(result, "Should return false as header validation failed (fatal error).");

        // Verify the reader *was* called (because File.Exists passed)
        _mockFileReader.Verify(r => r.ReadSingleFileAsync(filePath), Times.Once);

        // Verify mapper/saver were not called because header validation failed before the row loop
        _mockRowMapper.Verify(m => m.MapAsync(It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Never, "Mapper should not be called if header is invalid.");
        _mockEntitySaver.Verify(s => s.SaveAsync(It.IsAny<MappedAmecoRow>()), Times.Never, "Saver should not be called if header is invalid.");
    }

    /// <summary>
    /// Tests that if the row mapper fails for a row, the saver is not called for that row,
    /// but processing continues, and the overall result is true (mapping errors are not fatal).
    /// </summary>
    [Test]
    public async Task ParseAndSaveAsync_MapperFailsForRow_SkipsSaverForFailingRowAndReturnsTrue()
    {
        // Arrange
        var fileName = "AMECO1.csv";
        var filePath = Path.Combine(_tempDirectory, fileName);
        var filePaths = new List<string> { filePath };
        var expectedChapterName = "Population And Employment"; // Derived from AMECO1

        File.WriteAllText(filePath, string.Empty);
        TestContext.WriteLine($"Ensured empty file exists at: {filePath}");

        // Define valid header and rows for the reader mock
        var header = _validHeader;
        var row1 = new[] { "CODE1", "Sub1", "Title1", "Unit1", "C1", "Country1", "100", "110" };
        var row2 = new[] { "CODE2", "Sub2", "Title2", "Unit2", "C2", "Country2", "200", "220" };
        var rows = new List<string[]> { row1, row2 };

        // Define the mapped object for the successful row
        var mappedRow1 = CreateMappedRow("CODE1", expectedChapterName);

        // Setup Mocks
        SetupFileReaderSuccess(filePath, header, rows);

        // Setup mapper: Success for row1, Fail for row2
        // Using It.IsAny for non-row arguments for simplicity in this setup
        _mockRowMapper.Setup(m => m.MapAsync(
                            It.Is<string[]>(r => r.SequenceEqual(row1)),
                            header,
                            It.IsAny<Dictionary<string, int>>(),
                            It.IsAny<List<string>>(),
                            expectedChapterName))
                      .ReturnsAsync(MapResult<MappedAmecoRow>.Success(mappedRow1));
        _mockRowMapper.Setup(m => m.MapAsync(
                            It.Is<string[]>(r => r.SequenceEqual(row2)),
                            header,
                            It.IsAny<Dictionary<string, int>>(),
                            It.IsAny<List<string>>(),
                            expectedChapterName))
                      .ReturnsAsync(MapResult<MappedAmecoRow>.Fail("Test mapping failed"));

        // Act
        // Parser should read file, process header, map row1 (success), map row2 (fail),
        // save mappedRow1, skip saving for row2, finish file, return true.
        var result = await _parser.ParseAndSaveAsync(filePaths);

        // Assert
        Assert.IsTrue(result, "Should return true overall, as mapping errors are not fatal.");

        // Verify interactions
        _mockFileReader.Verify(r => r.ReadSingleFileAsync(filePath), Times.Once);
        // Verify mapper called for both rows
        _mockRowMapper.Verify(m => m.MapAsync(row1, header, It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), expectedChapterName), Times.Once);
        _mockRowMapper.Verify(m => m.MapAsync(row2, header, It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), expectedChapterName), Times.Once);
        // Verify saver called ONLY for the successful row (mappedRow1)
        _mockEntitySaver.Verify(s => s.SaveAsync(mappedRow1), Times.Once);
        _mockEntitySaver.Verify(s => s.SaveAsync(It.Is<MappedAmecoRow>(r => r.VariableCode == "CODE2")), Times.Never); // Not called for row 2
        _mockEntitySaver.Verify(s => s.SaveAsync(It.IsAny<MappedAmecoRow>()), Times.Exactly(1)); // Called only once total
    }

    /// <summary>
    /// Tests that if the entity saver throws an exception for a row, the parser catches it, logs it,
    /// continues processing other rows/files, and returns true overall (saver errors are not fatal).
    /// </summary>
    [Test]
    public async Task ParseAndSaveAsync_SaverThrowsException_CatchesLogsContinuesAndReturnsTrue()
    {
        // Arrange
        var fileName = "AMECO1.csv";
        var filePath = Path.Combine(_tempDirectory, fileName);
        var filePaths = new List<string> { filePath };
        var expectedChapterName = "Population And Employment"; // Derived from AMECO1

        File.WriteAllText(filePath, string.Empty);
        TestContext.WriteLine($"Ensured empty file exists at: {filePath}");

        // Define valid header and rows for the reader mock
        var header = _validHeader;
        // Adjust rows to match _validHeader length
        var row1 = new[] { "CODE1", "Sub1", "Title1", "Unit1", "C1", "Country1", "Extra1", "100" };
        var row2 = new[] { "CODE2", "Sub2", "Title2", "Unit2", "C2", "Country2", "Extra2", "200" };
        var rows = new List<string[]> { row1, row2 };

        // Define the mapped objects
        var mappedRow1 = CreateMappedRow("CODE1", expectedChapterName);
        var mappedRow2 = CreateMappedRow("CODE2", expectedChapterName);
        var testException = new InvalidOperationException("Simulated DB error");

        // Setup Mocks
        SetupFileReaderSuccess(filePath, header, rows);
        // Setup mapper to succeed for both rows
        SetupMapperSuccessForRow(row1, mappedRow1);
        SetupMapperSuccessForRow(row2, mappedRow2);

        // Setup saver: Throw for mappedRow1, succeed for mappedRow2 (default setup handles success)
        _mockEntitySaver.Setup(s => s.SaveAsync(mappedRow1)).ThrowsAsync(testException);
        

        // Act
        // Parser should: Read file, Process Header, Map row1(OK), Save row1(Throws), Catch/Log (row level),
        // Map row2(OK), Save row2(OK), Finish File loop, Increment totalFilesProcessed, Return true.
        var result = await _parser.ParseAndSaveAsync(filePaths);

        // Assert
        Assert.IsTrue(result, "Should return true overall, as saver errors for a row are not fatal to the process.");

        // Verify interactions occurred as expected
        _mockFileReader.Verify(r => r.ReadSingleFileAsync(filePath), Times.Once);
        _mockRowMapper.Verify(m => m.MapAsync(It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Exactly(2)); // Mapper called for both rows
        // Verify saver was ATTEMPTED for both rows
        _mockEntitySaver.Verify(s => s.SaveAsync(mappedRow1), Times.Once); // Called, but threw exception (caught by parser)
        _mockEntitySaver.Verify(s => s.SaveAsync(mappedRow2), Times.Once); // Called and succeeded (mock didn't throw)
    }
    
    /// <summary>
    /// Tests processing multiple files where one succeeds, one has an invalid header (fatal),
    /// and one has a row mapping error (non-fatal). Expects the overall result to be false
    /// due to the fatal header error, but verifies correct processing attempts.
    /// </summary>
    [Test]
    public async Task ParseAndSaveAsync_MultipleFiles_HandlesMixedResultsAndReturnsFalseOnFatalError()
    {
        // Arrange
        var fileName1 = "AMECO1.csv"; // Success
        var fileName2 = "AMECO_BadHeader.csv"; // Invalid Header -> Fatal Error
        var fileName3 = "AMECO3.csv"; // Mapper fails for one row -> Non-Fatal

        var filePath1 = Path.Combine(_tempDirectory, fileName1);
        var filePath2 = Path.Combine(_tempDirectory, fileName2);
        var filePath3 = Path.Combine(_tempDirectory, fileName3);
        var filePaths = new List<string> { filePath1, filePath2, filePath3 };

        var chapterName1 = "Population And Employment"; // From AMECO1
        var chapterName3 = "Capital Formation and Saving, Total Economy and Sectors"; // From AMECO3

        // Create placeholder files
        File.WriteAllText(filePath1, string.Empty);
        File.WriteAllText(filePath2, string.Empty);
        File.WriteAllText(filePath3, string.Empty);
        TestContext.WriteLine($"Created placeholder files at: {filePath1}, {filePath2}, {filePath3}");

        // --- Mock File Data ---
        // File 1: Valid header, 1 row
        var header1 = _validHeader;
        var row1_1 = new[] { "CODE11", "Sub11", "Title11", "Unit1", "C1", "Country1", "Extra1", "100", "110" };
        var rows1 = new List<string[]> { row1_1 };
        var mappedRow1_1 = CreateMappedRow("CODE11", chapterName1);

        // File 2: Invalid header (missing 'CODE')
        var header2_invalid = new[] { "SUB-CHAPTER", "TITLE", "UNIT", "CNTRY", "COUNTRY", "2020" };
        var rows2 = new List<string[]> { new[] { "Sub2", "Title2", "Unit2", "C2", "Country2", "200" } }; // Dummy row

        // File 3: Valid header, 2 rows (row1 ok, row2 fails mapping)
        var header3 = _validHeader;
        var row3_1 = new[] { "CODE31", "Sub31", "Title31", "Unit3", "C3", "Country3", "Extra3", "310", "311" };
        var row3_2 = new[] { "CODE32", "Sub32", "Title32", "Unit3", "C3", "Country3", "Extra3", "320", "322" }; // Mapper will fail this one
        var rows3 = new List<string[]> { row3_1, row3_2 };
        var mappedRow3_1 = CreateMappedRow("CODE31", chapterName3);

        // --- Setup Mocks ---
        // Reader
        SetupFileReaderSuccess(filePath1, header1, rows1);
        SetupFileReaderSuccess(filePath2, header2_invalid, rows2);
        SetupFileReaderSuccess(filePath3, header3, rows3);
        // Mapper
        _mockRowMapper.Setup(m => m.MapAsync(It.Is<string[]>(r => r.SequenceEqual(row1_1)), header1, It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), chapterName1))
                      .ReturnsAsync(MapResult<MappedAmecoRow>.Success(mappedRow1_1));
        _mockRowMapper.Setup(m => m.MapAsync(It.Is<string[]>(r => r.SequenceEqual(row3_1)), header3, It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), chapterName3))
                      .ReturnsAsync(MapResult<MappedAmecoRow>.Success(mappedRow3_1));
        _mockRowMapper.Setup(m => m.MapAsync(It.Is<string[]>(r => r.SequenceEqual(row3_2)), header3, It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), chapterName3))
                      .ReturnsAsync(MapResult<MappedAmecoRow>.Fail("Mapper failed for row 3_2"));
        // Saver (only expect calls for successfully mapped rows)
        _mockEntitySaver.Setup(s => s.SaveAsync(mappedRow1_1)).Returns(Task.CompletedTask);
        _mockEntitySaver.Setup(s => s.SaveAsync(mappedRow3_1)).Returns(Task.CompletedTask);


        // Act
        var result = await _parser.ParseAndSaveAsync(filePaths);

        // Assert
        // Expect False because file 2 had a fatal header error
        Assert.IsFalse(result, "Should return false overall due to fatal header error in one file.");

        // Verify reader was called for all files
        _mockFileReader.Verify(r => r.ReadSingleFileAsync(filePath1), Times.Once);
        _mockFileReader.Verify(r => r.ReadSingleFileAsync(filePath2), Times.Once);
        _mockFileReader.Verify(r => r.ReadSingleFileAsync(filePath3), Times.Once);

        // Verify mapper was called only for rows in files with valid headers (file 1 and file 3)
        _mockRowMapper.Verify(m => m.MapAsync(It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Exactly(3)); // row1_1, row3_1, row3_2
        _mockRowMapper.Verify(m => m.MapAsync(It.IsAny<string[]>(), header2_invalid, It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), It.IsAny<string>()), Times.Never); // Not called for file 2

        // Verify saver was called only for rows that were successfully mapped (row1_1 and row3_1)
         _mockEntitySaver.Verify(s => s.SaveAsync(It.IsAny<MappedAmecoRow>()), Times.Exactly(2));
         _mockEntitySaver.Verify(s => s.SaveAsync(mappedRow1_1), Times.Once);
         _mockEntitySaver.Verify(s => s.SaveAsync(mappedRow3_1), Times.Once);
         _mockEntitySaver.Verify(s => s.SaveAsync(It.Is<MappedAmecoRow>(r => r.VariableCode == "CODE32")), Times.Never); // Not called for failed map
    }


    /// <summary>
    /// Tests how DetermineChapterName (via the parser) handles various filename formats,
    /// verifying the correct chapter name context is passed to the row mapper.
    /// </summary>
    [Test]
    public async Task ParseAndSaveAsync_FilenameEdgeCases_PassesCorrectChapterNameToMapper()
    {
        // Arrange
        var chapter10Name = "Balances with the Rest of the World"; // From ChapterNumberToName[10]
        var unknownChapter99 = "Unknown Chapter (99)";
        var unknownChapterDefault = "Unknown Chapter"; // Default return value

        // Define filenames and expected chapter names
        var filesToTest = new Dictionary<string, string>
        {
            { "AMECO10.csv", chapter10Name },
            { "AMECO99.csv", unknownChapter99 },
            { "AMCO1.csv", unknownChapterDefault },
            { "AMECOXYZ.csv", unknownChapterDefault },
            { "DataFile.csv", unknownChapterDefault }
        };

        var filePaths = new List<string>();
        var header = _validHeader; // Use a valid header for all
        // Adjust row to match _validHeader length
        var sampleRow = new[] { "CODE", "Sub", "Title", "Unit", "C", "Country", "Extra", "100", "110" };
        var rows = new List<string[]> { sampleRow };
        var sampleMappedRow = CreateMappedRow("CODE"); // Content doesn't matter much here

        // Create files and setup mocks for each filename
        foreach (var kvp in filesToTest)
        {
            var fileName = kvp.Key;
            var filePath = Path.Combine(_tempDirectory, fileName);
            filePaths.Add(filePath);

            File.WriteAllText(filePath, string.Empty); // Create placeholder file
            SetupFileReaderSuccess(filePath, header, rows); // Setup reader for this file

            // Setup mapper ONCE to succeed for the sampleRow REGARDLESS of chapter name input.
            // We will VERIFY the correct chapter name was passed later.
            _mockRowMapper.Setup(m => m.MapAsync(
                                It.Is<string[]>(r => r.SequenceEqual(sampleRow)),
                                header,
                                It.IsAny<Dictionary<string, int>>(),
                                It.IsAny<List<string>>(),
                                It.IsAny<string>() // Accept any chapter name in this general setup
                            ))
                          .ReturnsAsync(MapResult<MappedAmecoRow>.Success(sampleMappedRow));
        }

        // Act
        var result = await _parser.ParseAndSaveAsync(filePaths);

        // Assert
        Assert.IsTrue(result, "Should return true as no fatal errors occurred.");

        // Verify Reader was called for all files
        _mockFileReader.Verify(r => r.ReadSingleFileAsync(It.IsAny<string>()), Times.Exactly(filesToTest.Count));

        // Verify Mapper was called the correct number of times total
        _mockRowMapper.Verify(m => m.MapAsync(
                            sampleRow, header, It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(), It.IsAny<string>()
                            ), Times.Exactly(filesToTest.Count), "Mapper should be called once per file.");

        // *** FIX: Verify specific chapter name counts individually AFTER the loop ***
        _mockRowMapper.Verify(m => m.MapAsync(
                            sampleRow, header, It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(),
                            chapter10Name // "Balances..."
                            ), Times.Once, $"Expected chapter '{chapter10Name}' called once.");

        _mockRowMapper.Verify(m => m.MapAsync(
                            sampleRow, header, It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(),
                            unknownChapter99 // "Unknown Chapter (99)"
                            ), Times.Once, $"Expected chapter '{unknownChapter99}' called once.");

        _mockRowMapper.Verify(m => m.MapAsync(
                            sampleRow, header, It.IsAny<Dictionary<string, int>>(), It.IsAny<List<string>>(),
                            unknownChapterDefault // "Unknown Chapter"
                            ), Times.Exactly(3), $"Expected chapter '{unknownChapterDefault}' called three times.");


        // Verify Saver was called for each (since mapping succeeded for all)
        _mockEntitySaver.Verify(s => s.SaveAsync(It.IsAny<MappedAmecoRow>()), Times.Exactly(filesToTest.Count));
    }
}