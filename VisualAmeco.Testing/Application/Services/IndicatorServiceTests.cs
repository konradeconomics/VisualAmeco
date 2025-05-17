using Application.Services;
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

    private Chapter _chapter1 = null!;
    private Subchapter _subchapter1 = null!;
    private Variable _variable1 = null!;
    private Variable _variable2 = null!;
    private Country _country1 = null!;
    private Country _country2 = null!;


    [SetUp]
    public void Setup()
    {
        _mockValueRepo = new Mock<IValueRepository>();
        _logger = NullLogger<IndicatorService>.Instance;

        _service = new IndicatorService(
            _mockValueRepo.Object,
            _logger
        );

        _chapter1 = new Chapter { Id = 10, Name = "Chapter One" };
        _subchapter1 = new Subchapter { Id = 101, Name = "Subchapter 1.1", ChapterId = 10, Chapter = _chapter1 };
        _variable1 = new Variable { Id = 1010, Code = "VAR1", Name = "Variable One", UnitCode = "0", UnitDescription = "Unit", SubChapterId = 101, SubChapter = _subchapter1 };
        _variable2 = new Variable { Id = 1011, Code = "VAR2", Name = "Variable Two", UnitCode = "0", UnitDescription = "Unit", SubChapterId = 101, SubChapter = _subchapter1 };
        _country1 = new Country { Id = 20, Code = "C1", Name = "Country One" };
        _country2 = new Country { Id = 21, Code = "C2", Name = "Country Two" };
    }

    private Value CreateTestValue(int id, int year, decimal amount, Variable variable, Country country)
    {
        return new Value
        {
            Id = id,
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
    /// Tests GetIndicatorsAsync with no filters, verifying correct grouping and mapping.
    /// </summary>
    [Test]
    public async Task GetIndicatorsAsync_WhenDataExists_NoFilters_ReturnsMappedAndGroupedIndicators()
    {
        // Arrange
        var mockValues = new List<Value>
        {
            CreateTestValue(1, 2021, 100m, _variable1, _country1),
            CreateTestValue(2, 2020, 90m, _variable1, _country1),
            CreateTestValue(3, 2021, 210m, _variable2, _country1),
            CreateTestValue(4, 2021, 150m, _variable1, _country2),
        };

        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                null, null, null, null, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockValues);

        // Act
        var result = await _service.GetIndicatorsAsync();
        var resultList = result.ToList();

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.AreEqual(3, resultList.Count, "Should return 3 unique indicators.");

        var indicator1 = resultList.FirstOrDefault(i => i.VariableCode == "VAR1" && i.CountryCode == "C1");
        Assert.IsNotNull(indicator1, "Indicator VAR1/C1 not found.");
        Assert.AreEqual("Variable One", indicator1!.VariableName);
        Assert.AreEqual("Country One", indicator1.CountryName);
        Assert.AreEqual("Chapter One", indicator1.ChapterName);
        Assert.AreEqual(2, indicator1.Values.Count, "VAR1/C1 should have 2 values.");
        Assert.AreEqual(2020, indicator1.Values[0].Year, "Values should be ordered by year.");
        Assert.AreEqual(90m, indicator1.Values[0].Amount);
        Assert.AreEqual(2021, indicator1.Values[1].Year);
        Assert.AreEqual(100m, indicator1.Values[1].Amount);

        _mockValueRepo.Verify(r => r.GetFilteredWithDetailsAsync(
            null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests GetIndicatorsAsync returns empty when repository returns empty (no filters).
    /// </summary>
    [Test]
    public async Task GetIndicatorsAsync_WhenRepoReturnsEmpty_NoFilters_ReturnsEmptyList()
    {
        // Arrange
        var mockValues = new List<Value>();
        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockValues);

        // Act
        var result = await _service.GetIndicatorsAsync();

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.IsFalse(result.Any(), "Result should be an empty enumerable.");
        _mockValueRepo.Verify(r => r.GetFilteredWithDetailsAsync(
            null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests GetIndicatorsAsync returns empty when repository returns null (no filters).
    /// </summary>
    [Test]
    public async Task GetIndicatorsAsync_WhenRepoReturnsNull_NoFilters_ReturnsEmptyList()
    {
        // Arrange
        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                null, null, null, null, null, It.IsAny<CancellationToken>()))!
            .ReturnsAsync((List<Value>?)null);

        // Act
        var result = await _service.GetIndicatorsAsync();

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.IsFalse(result.Any(), "Result should be an empty enumerable.");
        _mockValueRepo.Verify(r => r.GetFilteredWithDetailsAsync(
            null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests GetIndicatorsAsync filters out records with missing required navigation properties.
    /// </summary>
    [Test]
    public async Task GetIndicatorsAsync_WhenDataHasMissingRelations_FiltersOutInvalidData()
    {
        // Arrange
        var mockValues = new List<Value>
        {
            CreateTestValue(1, 2021, 100m, _variable1, _country1), // Valid
            new Value { Id = 2, Year = 2020, Amount = 90m, VariableId = 999, CountryId = _country1.Id, Country = _country1, Variable = null! }, // Missing Variable
            new Value { Id = 3, Year = 2021, Amount = 110m, VariableId = _variable1.Id, CountryId = 999, Variable = _variable1, Country = null! }, // Missing Country
            CreateTestValue(4, 2022, 120m, new Variable{Id=1011, Code="V_NO_SUB", SubChapter=null!}, _country1), // Missing Subchapter
        };
        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockValues);

        // Act
        var result = await _service.GetIndicatorsAsync();
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(1, resultList.Count, "Should only return 1 indicator after filtering invalid data.");
        Assert.AreEqual("VAR1", resultList[0].VariableCode);
        _mockValueRepo.Verify(r => r.GetFilteredWithDetailsAsync(
            null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetIndicatorsAsync correctly passes provided filter values
    /// to the repository's GetFilteredWithDetailsAsync method.
    /// </summary>
    [Test]
    public async Task GetIndicatorsAsync_WithFilters_PassesFiltersToRepository()
    {
        // Arrange
        string countryFilter = "C1";
        string variableFilter = "VAR1";
        string chapterFilter = "Chapter One";
        string subchapterFilter = "Subchapter 1.1";
        List<int> yearFilter = new List<int> { 2020, 2022 };
        var mockResultData = new List<Value>();

        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                countryFilter,
                variableFilter,
                chapterFilter,
                subchapterFilter,
                It.Is<List<int>>(list => list != null && list.SequenceEqual(yearFilter)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResultData)
            .Verifiable();

        // Act
        await _service.GetIndicatorsAsync(
            countryCode: countryFilter,
            variableCode: variableFilter,
            chapterName: chapterFilter,
            subchapterName: subchapterFilter,
            years: yearFilter,
            cancellationToken: CancellationToken.None);

        // Assert
        _mockValueRepo.Verify();
    }

    /// <summary>
    /// Tests that GetIndicatorsAsync returns only the data matching the filters provided,
    /// based on the mock repository's filtered response.
    /// </summary>
    [Test]
    public async Task GetIndicatorsAsync_WithFilters_ReturnsCorrectlyFilteredDtos()
    {
        // Arrange
        string countryFilter = "C1";
        List<int> yearFilter = new List<int> { 2020 };

        var repoFilteredValues = new List<Value>
        {
            CreateTestValue(2, 2020, 90m, _variable1, _country1),
            CreateTestValue(3, 2020, 200m, _variable2, _country1),
        };

        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                countryFilter,
                null,
                null,
                null,
                It.Is<List<int>>(list => list != null && list.SequenceEqual(yearFilter)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoFilteredValues);

        // Act
        var result = await _service.GetIndicatorsAsync(
            countryCode: countryFilter,
            years: yearFilter);
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(2, resultList.Count, "Should return 2 indicators matching filters (VAR1/C1 and VAR2/C1).");

        var indicator1 = resultList.FirstOrDefault(i => i.VariableCode == "VAR1");
        Assert.IsNotNull(indicator1, "VAR1/C1 not found.");
        Assert.AreEqual(1, indicator1!.Values.Count, "VAR1/C1 should only have 1 value (year 2020).");
        Assert.AreEqual(2020, indicator1.Values[0].Year);
        Assert.AreEqual(90m, indicator1.Values[0].Amount);

        var indicator2 = resultList.FirstOrDefault(i => i.VariableCode == "VAR2");
        Assert.IsNotNull(indicator2, "VAR2/C1 not found.");
        Assert.AreEqual(1, indicator2!.Values.Count, "VAR2/C1 should only have 1 value (year 2020).");
        Assert.AreEqual(2020, indicator2.Values[0].Year);
        Assert.AreEqual(200m, indicator2.Values[0].Amount);

        _mockValueRepo.Verify(r => r.GetFilteredWithDetailsAsync(
            countryFilter, null, null, null, yearFilter, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    /// <summary>
    /// Tests that GetIndicatorsAsync re-throws exceptions from the repository.
    /// </summary>
    [Test]
    public void GetIndicatorsAsync_WhenRepoThrows_ThrowsException()
    {
        // Arrange
        var testException = new InvalidOperationException("Repo failed");
        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(testException);

        // Act & Assert
        var thrown = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GetIndicatorsAsync());
        Assert.AreEqual(testException.Message, thrown?.Message);
        _mockValueRepo.Verify(r => r.GetFilteredWithDetailsAsync(
            null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    /// <summary>
    /// Tests that GetSpecificIndicatorAsync returns the correct DTO when the repository finds data.
    /// </summary>
    [Test]
    public async Task GetSpecificIndicatorAsync_WhenDataExists_ReturnsMappedIndicatorDto()
    {
        // Arrange
        string targetVariableCode = "VAR1";
        string targetCountryCode = "C1";
        var mockValues = new List<Value>
        {
            CreateTestValue(1, 2021, 100m, _variable1, _country1),
            CreateTestValue(2, 2020, 90m, _variable1, _country1) // Include multiple years for same indicator
        };

        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                targetCountryCode, targetVariableCode, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockValues);

        // Act
        var result = await _service.GetSpecificIndicatorAsync(targetVariableCode, targetCountryCode);

        // Assert
        Assert.IsNotNull(result, "Result DTO should not be null when data exists.");
        Assert.AreEqual(targetVariableCode, result!.VariableCode);
        Assert.AreEqual("Variable One", result.VariableName);
        Assert.AreEqual("0", result.UnitCode);
        Assert.AreEqual("Unit", result.UnitDescription);
        Assert.AreEqual("Subchapter 1.1", result.SubchapterName);
        Assert.AreEqual("Chapter One", result.ChapterName);
        Assert.AreEqual(targetCountryCode, result.CountryCode);
        Assert.AreEqual("Country One", result.CountryName);
        Assert.AreEqual(2, result.Values.Count, "Should contain values for both years.");
        Assert.AreEqual(2020, result.Values[0].Year, "Values should be ordered by year."); // Verify ordering
        Assert.AreEqual(90m, result.Values[0].Amount);
        Assert.AreEqual(2021, result.Values[1].Year);
        Assert.AreEqual(100m, result.Values[1].Amount);

        // Verify repository was called correctly
        _mockValueRepo.Verify(r => r.GetFilteredWithDetailsAsync(
            targetCountryCode, targetVariableCode, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    /// <summary>
    /// Tests that GetSpecificIndicatorAsync returns null when the repository returns an empty list.
    /// </summary>
    [Test]
    public async Task GetSpecificIndicatorAsync_WhenRepoReturnsEmpty_ReturnsNull()
    {
        // Arrange
        string targetVariableCode = "VAR_NONE";
        string targetCountryCode = "C_NONE";
        var mockValues = new List<Value>(); // Empty list

        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                targetCountryCode, targetVariableCode, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockValues);

        // Act
        var result = await _service.GetSpecificIndicatorAsync(targetVariableCode, targetCountryCode);

        // Assert
        Assert.IsNull(result, "Result should be null when repository returns no data.");
        _mockValueRepo.Verify(r => r.GetFilteredWithDetailsAsync(
            targetCountryCode, targetVariableCode, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    /// <summary>
    /// Tests that GetSpecificIndicatorAsync returns null when the repository returns null.
    /// </summary>
    [Test]
    public async Task GetSpecificIndicatorAsync_WhenRepoReturnsNull_ReturnsNull()
    {
        // Arrange
        string targetVariableCode = "VAR_ERR";
        string targetCountryCode = "C_ERR";

        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                targetCountryCode, targetVariableCode, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Value>?)null);

        // Act
        var result = await _service.GetSpecificIndicatorAsync(targetVariableCode, targetCountryCode);

        // Assert
        Assert.IsNull(result, "Result should be null when repository returns null.");
        _mockValueRepo.Verify(r => r.GetFilteredWithDetailsAsync(
            targetCountryCode, targetVariableCode, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    /// <summary>
    /// Tests that GetSpecificIndicatorAsync returns null if essential related data is missing,
    /// even if Value records are returned by the repository.
    /// </summary>
    [Test]
    public async Task GetSpecificIndicatorAsync_WhenRepoReturnsDataWithMissingRelations_ReturnsNull()
    {
        // Arrange
        string targetVariableCode = "VAR_BAD";
        string targetCountryCode = "C_BAD";
        var mockValues = new List<Value> {
            new Value { Id = 5, Year = 2020, Amount = 50m, VariableId = 999, CountryId = 999, Variable = null!, Country = new Country{Id=999, Code=targetCountryCode}}
        };

        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                targetCountryCode, targetVariableCode, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockValues);

        // Act
        var result = await _service.GetSpecificIndicatorAsync(targetVariableCode, targetCountryCode);

        // Assert
        Assert.IsNull(result, "Result should be null when essential related data (like Variable) is missing.");
        _mockValueRepo.Verify(r => r.GetFilteredWithDetailsAsync(
            targetCountryCode, targetVariableCode, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    /// <summary>
    /// Tests that GetSpecificIndicatorAsync propagates exceptions from the repository.
    /// </summary>
    [Test]
    public void GetSpecificIndicatorAsync_WhenRepoThrows_ThrowsException()
    {
        // Arrange
        string targetVariableCode = "VAR_EX";
        string targetCountryCode = "C_EX";
        var testException = new InvalidOperationException("Repo failed");

        _mockValueRepo.Setup(r => r.GetFilteredWithDetailsAsync(
                targetCountryCode, targetVariableCode, null, null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(testException);

        // Act & Assert
        var thrown = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetSpecificIndicatorAsync(targetVariableCode, targetCountryCode));
        Assert.AreEqual(testException.Message, thrown?.Message);
        _mockValueRepo.Verify(r => r.GetFilteredWithDetailsAsync(
            targetCountryCode, targetVariableCode, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}