using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using VisualAmeco.Application.Services;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Interfaces;

namespace VisualAmeco.Testing.Application.Services;

[TestFixture]
public class IndicatorServiceTests
{
    private Mock<IValueRepository> _mockValueRepo = null!;
    private ILogger<IndicatorService> _logger = null!;
    private IndicatorService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockValueRepo = new Mock<IValueRepository>();
        _logger = NullLogger<IndicatorService>.Instance; // Use NullLogger for unit tests

        _service = new IndicatorService(
            _mockValueRepo.Object,
            _logger
        );
    }

    // Helper method to create nested test entities easily
    private Value CreateTestValue(int id, int year, decimal amount, Variable variable, Country country)
    {
        return new Value
        {
            Id = id, // Give values unique IDs for clarity if needed
            Year = year,
            Amount = amount,
            VariableId = variable.Id,
            Variable = variable,
            CountryId = country.Id,
            Country = country,
            IsMonthly = false,
            Month = null
        };
    }

    /// <summary>
    /// Tests that GetIndicatorsAsync correctly groups Values by Variable/Country
    /// and maps them to IndicatorDtos with correct details and ordered year values.
    /// </summary>
    [Test]
    public async Task GetIndicatorsAsync_WhenDataExists_ReturnsMappedAndGroupedIndicators()
    {
        // Arrange
        // --- Mock Entities ---
        var chapter1 = new Chapter { Id = 10, Name = "Chapter One" };
        var subchapter1 = new Subchapter { Id = 101, Name = "Subchapter 1.1", ChapterId = 10, Chapter = chapter1 };
        var variable1 = new Variable { Id = 1010, Code = "VAR1", Name = "Variable One", Unit = "Units", SubChapterId = 101, SubChapter = subchapter1 };
        var variable2 = new Variable { Id = 1011, Code = "VAR2", Name = "Variable Two", Unit = "Units", SubChapterId = 101, SubChapter = subchapter1 };
        var country1 = new Country { Id = 20, Code = "C1", Name = "Country One" };
        var country2 = new Country { Id = 21, Code = "C2", Name = "Country Two" };

        // --- Mock Data from Repository ---
        // Represents the flat list returned by GetAllWithDetailsAsync
        var mockValues = new List<Value>
        {
            // Indicator 1: VAR1 / C1
            CreateTestValue(1, 2021, 100m, variable1, country1),
            CreateTestValue(2, 2020, 90m, variable1, country1), // Intentionally out of order
            // Indicator 2: VAR2 / C1
            CreateTestValue(3, 2021, 210m, variable2, country1),
            // Indicator 3: VAR1 / C2
            CreateTestValue(4, 2021, 150m, variable1, country2),
        };

        _mockValueRepo.Setup(r => r.GetAllWithDetailsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockValues);

        // Act
        var result = await _service.GetIndicatorsAsync();
        var resultList = result.ToList(); // Convert to list for easier inspection

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.AreEqual(3, resultList.Count, "Should return 3 unique indicators (Var+Country combos).");

        // Check Indicator 1 (VAR1 / C1)
        var indicator1 = resultList.FirstOrDefault(i => i.VariableCode == "VAR1" && i.CountryCode == "C1");
        Assert.IsNotNull(indicator1, "Indicator VAR1/C1 not found.");
        Assert.AreEqual("Variable One", indicator1!.VariableName);
        Assert.AreEqual("Units", indicator1.Unit);
        Assert.AreEqual("Subchapter 1.1", indicator1.SubchapterName);
        Assert.AreEqual("Chapter One", indicator1.ChapterName);
        Assert.AreEqual("Country One", indicator1.CountryName);
        Assert.AreEqual(2, indicator1.Values.Count, "VAR1/C1 should have 2 values.");
        // Check if values are ordered by year
        Assert.AreEqual(2020, indicator1.Values[0].Year, "First value year should be 2020.");
        Assert.AreEqual(90m, indicator1.Values[0].Amount, "First value amount should be 90.");
        Assert.AreEqual(2021, indicator1.Values[1].Year, "Second value year should be 2021.");
        Assert.AreEqual(100m, indicator1.Values[1].Amount, "Second value amount should be 100.");

        // Check Indicator 2 (VAR2 / C1)
        var indicator2 = resultList.FirstOrDefault(i => i.VariableCode == "VAR2" && i.CountryCode == "C1");
        Assert.IsNotNull(indicator2, "Indicator VAR2/C1 not found.");
        Assert.AreEqual("Variable Two", indicator2!.VariableName);
        Assert.AreEqual("Country One", indicator2.CountryName);
        Assert.AreEqual(1, indicator2.Values.Count, "VAR2/C1 should have 1 value.");
        Assert.AreEqual(2021, indicator2.Values[0].Year);
        Assert.AreEqual(210m, indicator2.Values[0].Amount);

        // Check Indicator 3 (VAR1 / C2)
        var indicator3 = resultList.FirstOrDefault(i => i.VariableCode == "VAR1" && i.CountryCode == "C2");
        Assert.IsNotNull(indicator3, "Indicator VAR1/C2 not found.");
        Assert.AreEqual("Variable One", indicator3!.VariableName);
        Assert.AreEqual("Country Two", indicator3.CountryName);
        Assert.AreEqual(1, indicator3.Values.Count, "VAR1/C2 should have 1 value.");
        Assert.AreEqual(2021, indicator3.Values[0].Year);
        Assert.AreEqual(150m, indicator3.Values[0].Amount);

        // Verify repository was called
        _mockValueRepo.Verify(r => r.GetAllWithDetailsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetIndicatorsAsync returns an empty list when the repository returns an empty list.
    /// </summary>
    [Test]
    public async Task GetIndicatorsAsync_WhenRepoReturnsEmpty_ReturnsEmptyList()
    {
        // Arrange
        var mockValues = new List<Value>(); // Empty list
        _mockValueRepo.Setup(r => r.GetAllWithDetailsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockValues);

        // Act
        var result = await _service.GetIndicatorsAsync();

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.IsFalse(result.Any(), "Result should be an empty enumerable.");
        _mockValueRepo.Verify(r => r.GetAllWithDetailsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetIndicatorsAsync returns an empty list when the repository returns null.
    /// </summary>
    [Test]
    public async Task GetIndicatorsAsync_WhenRepoReturnsNull_ReturnsEmptyList()
    {
        // Arrange
        _mockValueRepo.Setup(r => r.GetAllWithDetailsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Value>?)null); // Return null Task result

        // Act
        var result = await _service.GetIndicatorsAsync();

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.IsFalse(result.Any(), "Result should be an empty enumerable.");
        _mockValueRepo.Verify(r => r.GetAllWithDetailsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetIndicatorsAsync filters out Value records with missing required navigation properties.
    /// </summary>
    [Test]
    public async Task GetIndicatorsAsync_WhenDataHasMissingRelations_FiltersOutInvalidData()
    {
        // Arrange
        var chapter1 = new Chapter { Id = 10, Name = "Chapter One" };
        var subchapter1 = new Subchapter { Id = 101, Name = "Subchapter 1.1", ChapterId = 10, Chapter = chapter1 };
        var variable1 = new Variable { Id = 1010, Code = "VAR1", Name = "Variable One", Unit = "Units", SubChapterId = 101, SubChapter = subchapter1 };
        var country1 = new Country { Id = 20, Code = "C1", Name = "Country One" };

        var mockValues = new List<Value>
        {
            // Valid record
            CreateTestValue(1, 2021, 100m, variable1, country1),
            // Invalid record (missing Variable)
            new Value { Id = 2, Year = 2020, Amount = 90m, VariableId = 999, CountryId = country1.Id, Country = country1, Variable = null! },
            // Invalid record (missing Country)
            new Value { Id = 3, Year = 2021, Amount = 110m, VariableId = variable1.Id, CountryId = 999, Variable = variable1, Country = null! },
            // Invalid record (missing Subchapter on Variable)
            CreateTestValue(4, 2022, 120m, new Variable{Id=1011, Code="V_NO_SUB", SubChapter=null!}, country1),
        };
        _mockValueRepo.Setup(r => r.GetAllWithDetailsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockValues);

        // Act
        var result = await _service.GetIndicatorsAsync();
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(1, resultList.Count, "Should only return 1 indicator after filtering invalid data.");
        Assert.AreEqual("VAR1", resultList[0].VariableCode); // Check it's the valid one
        _mockValueRepo.Verify(r => r.GetAllWithDetailsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}