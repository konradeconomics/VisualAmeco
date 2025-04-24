using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using VisualAmeco.Parser.Services;

namespace VisualAmeco.Testing.Parser.Services;

[TestFixture]
public class CsvRowMapperTests
{
    private ILogger<CsvRowMapper> _logger = null!;
    private CsvRowMapper _mapper = null!; 

    [SetUp]
    public void Setup()
    {
        _logger = NullLogger<CsvRowMapper>.Instance;
        _mapper = new CsvRowMapper(_logger);
        Console.WriteLine("CsvRowMapper instance created for test.");
    }


    [Test]
    public async Task MapAsync_ValidRow_ReturnsSuccessAndCorrectData()
    {
        // Arrange
        var header = new[]
        {
            "SUB-CHAPTER", "TITLE", "CODE", "UNIT", "CNTRY", "COUNTRY",
            "2020", "2021"
        };

        var row = new[]
        {
            "07.1 GDP", "GDP per capita", "GDPPC", "EUR", "DE", "Germany",
            "1000.50", "1050.25"
        };

        var colIndices = new Dictionary<string, int>
        {
            { "SUB-CHAPTER", 0 }, { "TITLE", 1 }, { "CODE", 2 }, { "UNIT", 3 },
            { "CNTRY", 4 }, { "COUNTRY", 5 }
        };

        var yearCols = new List<string> { "2020", "2021" };

        var expectedChapter = "Gross Domestic Product";

        // Act
        var result = await _mapper.MapAsync(row, header, colIndices, yearCols, expectedChapter);

        if (!result.IsSuccess)
        {
            Console.WriteLine($"Mapping failed: {result.ErrorMessage}");
        }

        // Assert
        Assert.IsTrue(result.IsSuccess, "Mapping should succeed for valid data.");
        Assert.IsNotNull(result.Value, "Mapped value should not be null on success.");

        // Assert specific properties
        Assert.AreEqual(expectedChapter, result.Value!.ChapterName, "ChapterName should match the input context."); // <<< NEW ASSERTION
        Assert.AreEqual("07.1 GDP", result.Value.SubchapterName, "SubchapterName should be mapped correctly.");
        Assert.AreEqual("GDPPC", result.Value.VariableCode, "VariableCode should be mapped correctly.");
        Assert.AreEqual("GDP per capita", result.Value.VariableName, "VariableName should be mapped correctly.");
        Assert.AreEqual("EUR", result.Value.Unit, "Unit should be mapped correctly.");
        Assert.AreEqual("DE", result.Value.CountryCode, "CountryCode should be mapped correctly.");
        Assert.AreEqual("Germany", result.Value.CountryName, "CountryName should be mapped correctly.");

        // Assert year values
        Assert.AreEqual(2, result.Value.Values.Count, "Should have correct number of year values.");
        Assert.AreEqual(2020, result.Value.Values[0].Year, "First year should be correct.");
        Assert.AreEqual(1000.50m, result.Value.Values[0].Amount, "First amount should be parsed correctly.");
        Assert.AreEqual(2021, result.Value.Values[1].Year, "Second year should be correct.");
        Assert.AreEqual(1050.25m, result.Value.Values[1].Amount, "Second amount should be parsed correctly.");
    }

    [Test]
    public async Task MapAsync_RowTooShortForRequiredColumn_ReturnsFailure()
    {
        // Arrange
        var header = new[] { "SUB-CHAPTER", "TITLE", "CODE", "UNIT", "CNTRY", "COUNTRY", "2020" };
        var row = new[] { "07.1 GDP", "GDP per capita", "GDPPC" };
        var colIndices = new Dictionary<string, int>
        {
            { "SUB-CHAPTER", 0 }, { "TITLE", 1 }, { "CODE", 2 },
            { "UNIT", 3 }, { "CNTRY", 4 }, { "COUNTRY", 5 }
        };
        var yearCols = new List<string> { "2020" };
        var chapterNameContext = "Gross Domestic Product"; // Context still provided

        // Act
        var result = await _mapper.MapAsync(row, header, colIndices, yearCols, chapterNameContext);

        // Assert
        Assert.IsFalse(result.IsSuccess, "Mapping should fail if row is too short for required columns.");
        Assert.IsNotNull(result.ErrorMessage, "Error message should be provided on failure.");
        StringAssert.Contains("invalid UNIT index/data", result.ErrorMessage, "Error message should indicate missing data.");
        Console.WriteLine($"Expected Failure: {result.ErrorMessage}");
    }

     [Test]
    public async Task MapAsync_RowTooShortForYearColumn_SkipsYearValue()
    {
        // Arrange
        var header = new[] { "SUB-CHAPTER", "TITLE", "CODE", "UNIT", "CNTRY", "COUNTRY", "2020", "2021" };
        var row = new[] { "07.1 GDP", "GDP per capita", "GDPPC", "EUR", "DE", "Germany", "1000.50" };
        var colIndices = new Dictionary<string, int>
        {
            { "SUB-CHAPTER", 0 }, { "TITLE", 1 }, { "CODE", 2 }, { "UNIT", 3 },
            { "CNTRY", 4 }, { "COUNTRY", 5 }
        };
        var yearCols = new List<string> { "2020", "2021" }; // Expecting both years
        var chapterNameContext = "Gross Domestic Product";

        // Act
        var result = await _mapper.MapAsync(row, header, colIndices, yearCols, chapterNameContext);

        // Assert
        Assert.IsTrue(result.IsSuccess, "Mapping should still succeed overall.");
        Assert.IsNotNull(result.Value, "Mapped value should not be null.");
        Assert.AreEqual(chapterNameContext, result.Value!.ChapterName, "ChapterName should be set.");
        Assert.AreEqual(1, result.Value.Values.Count, "Should only contain value for the year present in the row.");
        Assert.AreEqual(2020, result.Value.Values[0].Year, "The valid year should be present.");
        Assert.AreEqual(1000.50m, result.Value.Values[0].Amount, "The valid amount should be present.");
    }

    [Test]
    public async Task MapAsync_InvalidDecimalForYearValue_ReturnsZeroAmount()
    {
        // Arrange
        var header = new[] { "SUB-CHAPTER", "TITLE", "CODE", "UNIT", "CNTRY", "COUNTRY", "2020", "2021" };
        var row = new[] { "07.1 GDP", "GDP per capita", "GDPPC", "EUR", "DE", "Germany", "1000.50", "invalid-decimal" };
        var colIndices = new Dictionary<string, int>
        {
            { "SUB-CHAPTER", 0 }, { "TITLE", 1 }, { "CODE", 2 }, { "UNIT", 3 },
            { "CNTRY", 4 }, { "COUNTRY", 5 }
        };
        var yearCols = new List<string> { "2020", "2021" };
        var chapterNameContext = "Gross Domestic Product";

        // Act
        var result = await _mapper.MapAsync(row, header, colIndices, yearCols, chapterNameContext);

        // Assert
        Assert.IsTrue(result.IsSuccess, "Mapping should succeed even with unparseable decimal.");
        Assert.IsNotNull(result.Value, "Mapped value should not be null.");
        Assert.AreEqual(chapterNameContext, result.Value!.ChapterName, "ChapterName should be set correctly."); // Check chapter name
        Assert.AreEqual(2, result.Value.Values.Count, "Should attempt to process both year values.");

        var invalidYearValue = result.Value.Values.FirstOrDefault(v => v.Year == 2021);
        Assert.IsNotNull(invalidYearValue, "Should have an entry for 2021.");
        Assert.AreEqual(0m, invalidYearValue!.Amount, "Amount should default to 0 for unparseable decimal.");

        var validYearValue = result.Value.Values.FirstOrDefault(v => v.Year == 2020);
        Assert.IsNotNull(validYearValue, "Should have an entry for 2020.");
        Assert.AreEqual(1000.50m, validYearValue!.Amount, "Valid amount should be parsed correctly.");
    }

    [Test]
    public async Task MapAsync_YearColumnNotInHeader_ReturnsFailure()
    {
        // Arrange
        var header = new[] {
            "SUB-CHAPTER", "TITLE", "CODE", "UNIT", "CNTRY", "COUNTRY",
            "2020"
        };
        var row = new[] {
            "07.1 GDP", "GDP per capita", "GDPPC", "EUR", "DE", "Germany",
            "1000.50"
        };

        var colIndices = new Dictionary<string, int>
        {
             { "SUB-CHAPTER", 0 }, { "TITLE", 1 }, { "CODE", 2 }, { "UNIT", 3 },
             { "CNTRY", 4 }, { "COUNTRY", 5 }
        };

        var yearCols = new List<string> { "2020", "2021" };
        var chapterNameContext = "Gross Domestic Product";

        // Act
        var result = await _mapper.MapAsync(row, header, colIndices, yearCols, chapterNameContext);

        if (!result.IsSuccess)
        {
             Console.WriteLine($"Test expected failure as year '2021' not in header. Actual Error: {result.ErrorMessage}");
        }

        // Assert
        Assert.IsFalse(result.IsSuccess, "Mapping should FAIL when a requested year column is not found in the header and the current logic attempts to access it.");
        Assert.IsNull(result.Value, "Result value should be null on failure.");
        Assert.IsNotNull(result.ErrorMessage, "An error message should be provided on failure.");
        Assert.IsNotEmpty(result.ErrorMessage, "Error message should not be empty.");
    }
}