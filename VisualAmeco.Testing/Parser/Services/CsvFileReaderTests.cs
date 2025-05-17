using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using VisualAmeco.Parser.Parsers;

namespace VisualAmeco.Testing.Parser.Services;

[TestFixture]
public class CsvFileReaderTests
{ 
    private ILogger<CsvFileReader> _logger = null!;
    private CsvFileReader _fileReader = null!; // Instance of the class under test
    private string _tempDirectory = null!; // Path for temporary test files

    [SetUp]
    public void Setup()
    {
        // Initialize logger (using NullLogger for unit tests)
        _logger = NullLogger<CsvFileReader>.Instance;

        // Create instance of the file reader
        _fileReader = new CsvFileReader(_logger);

        // Create a unique temporary directory for this test run
        // Using Path.Combine ensures cross-platform compatibility
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"CsvReaderTests_{TestContext.CurrentContext.Test.ID}");
        Directory.CreateDirectory(_tempDirectory);
        TestContext.WriteLine($"Created temp directory: {_tempDirectory}");
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up the temporary directory after each test
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true); // true for recursive delete
                TestContext.WriteLine($"Deleted temp directory: {_tempDirectory}");
            }
        }
        catch (Exception ex)
        {
            // Log cleanup errors, but don't let them fail the test run itself
            TestContext.WriteLine($"ERROR cleaning up temp directory '{_tempDirectory}': {ex.Message}");
        }
    }

    // --- Helper Method ---
    /// <summary>
    /// Creates a temporary CSV file with the given content.
    /// </summary>
    /// <param name="csvContent">The string content to write to the file.</param>
    /// <param name="fileName">Optional filename.</param>
    /// <returns>The full path to the created temporary file.</returns>
    private string CreateTemporaryCsvFile(string csvContent, string fileName = "test.csv")
    {
        var fullPath = Path.Combine(_tempDirectory, fileName);
        // Use WriteAllText which defaults to UTF-8, usually fine for CSV. Specify encoding if needed.
        File.WriteAllText(fullPath, csvContent);
        TestContext.WriteLine($"Created temp file with {csvContent.Length} chars: {fullPath}");
        return fullPath;
    }
    

    /// <summary>
    /// Checks that a valid CSV file with a header, multiple data rows (including quoted commas and empty fields)
    /// is read correctly, returning the expected header array and list of row arrays.
    /// </summary>
    [Test]
    public async Task ReadSingleFileAsync_ValidFileWithData_ReturnsHeaderAndRows()
    {
        // Arrange
        var csvContent = @"Code,Country,SubChapter,2020,2021
GDPPC,Germany,GDP Data,""1,000.50"",1050.25
NPTD,France,Population Data,50000,51000";
        var filePath = CreateTemporaryCsvFile(csvContent);

        var expectedHeader = new[] { "Code", "Country", "SubChapter", "2020", "2021" };
        var expectedRow1 = new[] { "GDPPC", "Germany", "GDP Data", "1,000.50", "1050.25" };
        var expectedRow2 = new[] { "NPTD", "France", "Population Data", "50000", "51000" };

        // Act
        var result = await _fileReader.ReadSingleFileAsync(filePath);

        // Assert
        Assert.IsNotNull(result, "Result tuple should not be null for a valid file.");
        // Header Asserts
        Assert.IsNotNull(result.Value.Header, "Header array should not be null.");
        CollectionAssert.AreEqual(expectedHeader, result.Value.Header, "Header content should match expected.");
        // Rows Asserts
        Assert.IsNotNull(result.Value.Rows, "Rows list should not be null.");
        Assert.AreEqual(2, result.Value.Rows.Count, "Should have read 2 data rows.");
        CollectionAssert.AreEqual(expectedRow1, result.Value.Rows[0], "Row 1 content should match.");
        CollectionAssert.AreEqual(expectedRow2, result.Value.Rows[1], "Row 2 content should match.");
    }

    /// <summary>
    /// Verifies that reading a completely empty file results in a null return value,
    /// indicating failure to read the expected header/structure.
    /// </summary>
    [Test]
    public async Task ReadSingleFileAsync_EmptyFile_ReturnsNull()
    {
        // Arrange
        var csvContent = ""; // Completely empty file
        var filePath = CreateTemporaryCsvFile(csvContent, "empty.csv");

        // Act
        var result = await _fileReader.ReadSingleFileAsync(filePath);

        // Assert
        // Expecting null because CsvReader likely throws error or can't read header
        Assert.IsNull(result, "Result should be null for an empty file (cannot read header).");
    }

    /// <summary>
    /// Tests reading a CSV file containing only a header row. Expects the header to be
    /// returned correctly and the list of data rows to be empty.
    /// </summary>
    [Test]
    public async Task ReadSingleFileAsync_FileWithOnlyHeader_ReturnsHeaderAndEmptyRowsList()
    {
        // Arrange
        var csvContent = "ID,Name,Value"; // Header row only
        var filePath = CreateTemporaryCsvFile(csvContent, "header_only.csv");
        var expectedHeader = new[] { "ID", "Name", "Value" };

        // Act
        var result = await _fileReader.ReadSingleFileAsync(filePath);

        // Assert
        Assert.IsNotNull(result, "Result tuple should not be null.");
        Assert.IsNotNull(result.Value.Header, "Header array should not be null.");
        CollectionAssert.AreEqual(expectedHeader, result.Value.Header, "Header content should match.");
        Assert.IsNotNull(result.Value.Rows, "Rows list should not be null.");
        Assert.IsEmpty(result.Value.Rows, "Rows list should be empty when only header exists.");
    }

    /// <summary>
    /// Ensures that attempting to read a file that does not exist results in a null return value,
    /// indicating the file read operation failed gracefully (exception caught internally).
    /// </summary>
    [Test]
    public async Task ReadSingleFileAsync_FileNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentFilePath = Path.Combine(_tempDirectory, "i_dont_exist.csv");
        // Ensure file really doesn't exist
        if (File.Exists(nonExistentFilePath)) File.Delete(nonExistentFilePath);

        // Act
        var result = await _fileReader.ReadSingleFileAsync(nonExistentFilePath);

        // Assert
        Assert.IsNull(result, "Result should be null when file does not exist.");
    }

    /// <summary>
    /// Verifies that if a data row has fewer columns than the header, the resulting
    /// string array for that row is padded with empty strings to match the header's length.
    /// </summary>
    [Test]
    public async Task ReadSingleFileAsync_RowWithFewerColumnsThanHeader_PadsWithEmptyStrings()
    {
        // Arrange
        var csvContent = @"ColA,ColB,ColC
ValA1,ValB1"; // Second row is shorter than header
        var filePath = CreateTemporaryCsvFile(csvContent, "jagged.csv");

        // Act
        var result = await _fileReader.ReadSingleFileAsync(filePath);

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.AreEqual(3, result.Value.Header?.Length, "Header should have 3 columns.");
        Assert.AreEqual(1, result.Value.Rows.Count, "Should have 1 data row.");
        // Check that the shorter row was padded to match header length
        CollectionAssert.AreEqual(new[] { "ValA1", "ValB1", "" }, result.Value.Rows[0], "Shorter row should be padded.");
    }

    /// <summary>
    /// Verifies that if a data row has more columns than the header, the resulting
    /// string array for that row is truncated to match the header's length, ignoring extra fields.
    /// </summary>
    [Test]
    public async Task ReadSingleFileAsync_RowWithMoreColumnsThanHeader_TruncatesToHeaderLength()
    {
        // Arrange
        var csvContent = @"ColA,ColB
ValA1,ValB1,ValC1";
        var filePath = CreateTemporaryCsvFile(csvContent, "long_row.csv");

        // Act
        var result = await _fileReader.ReadSingleFileAsync(filePath);

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.AreEqual(2, result.Value.Header?.Length, "Header should have 2 columns.");
        Assert.AreEqual(1, result.Value.Rows.Count, "Should have 1 data row.");
        CollectionAssert.AreEqual(new[] { "ValA1", "ValB1" }, result.Value.Rows[0], "Longer row should be truncated.");
    }

    /// <summary>
    /// Tests that fields containing the literal value "NA" are read correctly as the string "NA".
    /// </summary>
    [Test]
    public async Task ReadSingleFileAsync_RowWithNAValues_ReadsNAAsString()
    {
        // Arrange
        var csvContent = @"Code,Year1,Year2
CODE1,100,NA
CODE2,NA,200";
        var filePath = CreateTemporaryCsvFile(csvContent, "na_values.csv");
        var expectedHeader = new[] { "Code", "Year1", "Year2" };

        // Act
        var result = await _fileReader.ReadSingleFileAsync(filePath);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Value.Rows.Count);
        // Verify "NA" is read as a string
        CollectionAssert.AreEqual(new[] { "CODE1", "100", "NA" }, result.Value.Rows[0]);
        CollectionAssert.AreEqual(new[] { "CODE2", "NA", "200" }, result.Value.Rows[1]);
    }

    /// <summary>
    /// Checks handling of CSV files where the header row contains trailing commas.
    /// Expects empty strings in the returned header array and corresponding fields read from data rows up to the header length.
    /// </summary>
    [Test]
    public async Task ReadSingleFileAsync_HeaderWithTrailingCommas_ReadsEmptyHeadersAndFields()
    {
        // Arrange
        // Simulates header like ...,ColD,,
        var csvContent = @"ColA,ColB,ColC,ColD,,
ValA1,ValB1,ValC1,ValD1,,
ValA2,ValB2,,,ValE2 Ignored,ValF2 Ignored";
        var filePath = CreateTemporaryCsvFile(csvContent, "trailing_commas.csv");
        // CsvHelper reads headers including the empty ones from trailing commas
        var expectedHeader = new[] { "ColA", "ColB", "ColC", "ColD", "", "" };

        // Act
        var result = await _fileReader.ReadSingleFileAsync(filePath);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Value.Header);
        CollectionAssert.AreEqual(expectedHeader, result.Value.Header, "Header should include empty strings.");

        Assert.AreEqual(2, result.Value.Rows.Count);
        // Check first row (matches header length exactly)
        CollectionAssert.AreEqual(new[] { "ValA1", "ValB1", "ValC1", "ValD1", "", "" }, result.Value.Rows[0]);
        CollectionAssert.AreEqual(new[] { "ValA2", "ValB2", "", "", "ValE2 Ignored", "ValF2 Ignored" }, result.Value.Rows[1]);
    }
}