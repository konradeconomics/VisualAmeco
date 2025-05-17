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


    /// <summary>
    /// Tests that MapAsync correctly maps a valid row to MappedAmecoRow,
    /// including the new UnitCode, UnitDescription, TRN, AGG, and REF fields.
    /// </summary>
    [Test]
    public async Task MapAsync_ValidRow_ReturnsSuccessAndCorrectData()
    {
        // Arrange
        var header = new[] // Simulates header processed by AmecoCsvParser
        {
            "SUB-CHAPTER", "TITLE", "CODE", "UNIT_CODE", "CNTRY", "COUNTRY", // Standard fields
            "UNIT_DESCRIPTION", "TRN", "AGG", "REF", // New/Split fields
            "2020", "2021" // Year fields
        };

        var row = new[]
        {
            "07.1 GDP", "GDP per capita", "GDPPC", "0", "DE", "Germany", // Standard values
            "National currency", "1", "0", "0", // Values for new/split fields
            "1000.50", "1050.25" // Year values
        };

        var colIndices = new Dictionary<string, int>
        {
            { "SUB-CHAPTER", 0 }, { "TITLE", 1 }, { "CODE", 2 },
            { "UNIT_CODE", 3 },
            { "CNTRY", 4 }, { "COUNTRY", 5 },
            { "UNIT_DESCRIPTION", 6 },
            { "TRN", 7 }, { "AGG", 8 }, { "REF", 9 },
        };

        var yearCols = new List<string> { "2020", "2021" };
        var expectedChapter = "Gross Domestic Product";

        // Act
        var result = await _mapper.MapAsync(row, header, colIndices, yearCols, expectedChapter);

        // Assert
        Assert.IsTrue(result.IsSuccess, "Mapping should succeed for valid data.");
        Assert.IsNotNull(result.Value, "Mapped value should not be null on success.");

        var mappedValue = result.Value!;
        Assert.AreEqual(expectedChapter, mappedValue.ChapterName);
        Assert.AreEqual("07.1 GDP", mappedValue.SubchapterName);
        Assert.AreEqual("GDPPC", mappedValue.VariableCode);
        Assert.AreEqual("GDP per capita", mappedValue.VariableName);
        Assert.AreEqual("0", mappedValue.UnitCode, "UnitCode should be mapped correctly.");
        Assert.AreEqual("National currency", mappedValue.UnitDescription,
            "UnitDescription should be mapped correctly.");
        Assert.AreEqual("DE", mappedValue.CountryCode);
        Assert.AreEqual("Germany", mappedValue.CountryName);
        Assert.AreEqual("1", mappedValue.TRN);
        Assert.AreEqual("0", mappedValue.AGG);
        Assert.AreEqual("0", mappedValue.REF);

        Assert.AreEqual(2, mappedValue.Values.Count);
        Assert.AreEqual(2020, mappedValue.Values[0].Year);
        Assert.AreEqual(1000.50m, mappedValue.Values[0].Amount);
        Assert.AreEqual(2021, mappedValue.Values[1].Year);
        Assert.AreEqual(1050.25m, mappedValue.Values[1].Amount);
    }

    /// <summary>
    /// Tests that MapAsync returns Fail when a required column (e.g., UNIT_CODE) is missing from colIndices or row.
    /// </summary>
    [Test]
    public async Task MapAsync_RowTooShortForRequiredColumn_ReturnsFailure()
    {
        // Arrange
        var header = new[] { "SUB-CHAPTER", "TITLE", "CODE", "UNIT_CODE", "UNIT_DESCRIPTION", "TRN", "AGG", "REF", "CNTRY", "COUNTRY", "2020" };
        var row = new[] { "07.1 GDP", "GDP per capita", "GDPPC" };
        var colIndices = new Dictionary<string, int>
        {
            { "SUB-CHAPTER", 0 }, { "TITLE", 1 }, { "CODE", 2 },
            { "UNIT_CODE", 3 }, { "UNIT_DESCRIPTION", 4 },
            { "TRN", 5}, {"AGG", 6}, {"REF", 7},
            { "CNTRY", 8 }, { "COUNTRY", 9 }
        };
        var yearCols = new List<string> { "2020" };
        var chapterNameContext = "Gross Domestic Product";

        // Act
        var result = await _mapper.MapAsync(row, header, colIndices, yearCols, chapterNameContext);

        // Assert
        Assert.IsFalse(result.IsSuccess, "Mapping should fail if row is too short for required columns.");
        Assert.IsNotNull(result.ErrorMessage, "Error message should be provided on failure.");
        StringAssert.Contains("invalid UNIT_CODE index/data", result.ErrorMessage, "Error message should indicate missing UNIT_CODE data.");
        TestContext.WriteLine($"Expected Failure: {result.ErrorMessage}");
    }

    /// <summary>
    /// Tests that if a row is too short for a specific year column, that year value is skipped,
    /// but the overall mapping can still succeed if other data is valid.
    /// </summary>
    [Test]
    public async Task MapAsync_RowTooShortForYearColumn_SkipsYearValue()
    {
        // Arrange
        var header = new[] { "SUB-CHAPTER", "TITLE", "CODE", "UNIT_CODE", "UNIT_DESCRIPTION", "TRN", "AGG", "REF", "CNTRY", "COUNTRY", "2020", "2021" };
        var row = new[] { "07.1 GDP", "GDP per capita", "GDPPC", "0", "Nat. Curr.", "1", "0", "0", "DE", "Germany", "1000.50" /* Missing 2021 value */ };
        var colIndices = new Dictionary<string, int>
        {
            { "SUB-CHAPTER", 0 }, { "TITLE", 1 }, { "CODE", 2 },
            { "UNIT_CODE", 3 }, { "UNIT_DESCRIPTION", 4 },
            { "TRN", 5}, {"AGG", 6}, {"REF", 7},
            { "CNTRY", 8 }, { "COUNTRY", 9 }
        };
        var yearCols = new List<string> { "2020", "2021" };
        var chapterNameContext = "Gross Domestic Product";

        // Act
        var result = await _mapper.MapAsync(row, header, colIndices, yearCols, chapterNameContext);

        // Assert
        Assert.IsTrue(result.IsSuccess, "Mapping should still succeed overall.");
        Assert.IsNotNull(result.Value, "Mapped value should not be null.");
        var mappedValue = result.Value!;
        Assert.AreEqual(chapterNameContext, mappedValue.ChapterName);
        Assert.AreEqual(1, mappedValue.Values.Count, "Should only contain value for the year present in the row.");
        Assert.AreEqual(2020, mappedValue.Values[0].Year, "The valid year should be present.");
        Assert.AreEqual(1000.50m, mappedValue.Values[0].Amount, "The valid amount should be present.");
    }

    /// <summary>
    /// Tests that an invalid decimal value for a year results in an amount of 0 for that year,
    /// but the overall mapping succeeds.
    /// </summary>
    [Test]
    public async Task MapAsync_InvalidDecimalForYearValue_ReturnsZeroAmount()
    {
        // Arrange
        var header = new[] { "SUB-CHAPTER", "TITLE", "CODE", "UNIT_CODE", "UNIT_DESCRIPTION", "TRN", "AGG", "REF", "CNTRY", "COUNTRY", "2020", "2021" };
        var row = new[] { "07.1 GDP", "GDP per capita", "GDPPC", "0", "Nat. Curr.", "1", "0", "0", "DE", "Germany", "1000.50", "invalid-decimal" };
        var colIndices = new Dictionary<string, int>
        {
            { "SUB-CHAPTER", 0 }, { "TITLE", 1 }, { "CODE", 2 },
            { "UNIT_CODE", 3 }, { "UNIT_DESCRIPTION", 4 },
            { "TRN", 5}, {"AGG", 6}, {"REF", 7},
            { "CNTRY", 8 }, { "COUNTRY", 9 }
        };
        var yearCols = new List<string> { "2020", "2021" };
        var chapterNameContext = "Gross Domestic Product";

        // Act
        var result = await _mapper.MapAsync(row, header, colIndices, yearCols, chapterNameContext);

        // Assert
        Assert.IsTrue(result.IsSuccess, "Mapping should succeed even with unparseable decimal.");
        Assert.IsNotNull(result.Value, "Mapped value should not be null.");
        var mappedValue = result.Value!;
        Assert.AreEqual(chapterNameContext, mappedValue.ChapterName);
        Assert.AreEqual(2, mappedValue.Values.Count, "Should attempt to process both year values.");

        var invalidYearValue = mappedValue.Values.FirstOrDefault(v => v.Year == 2021);
        Assert.IsNotNull(invalidYearValue, "Should have an entry for 2021.");
        Assert.AreEqual(0m, invalidYearValue!.Amount, "Amount should default to 0 for unparseable decimal.");

        var validYearValue = mappedValue.Values.FirstOrDefault(v => v.Year == 2020);
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