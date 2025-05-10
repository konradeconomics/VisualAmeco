using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using VisualAmeco.API.Controllers;
using VisualAmeco.Application.DTOs;

namespace VisualAmeco.Testing.API.Controllers;

/// <summary>
/// Unit tests for the IndicatorsController.
/// </summary>
[TestFixture]
public class IndicatorsControllerTests
{
    private Mock<IIndicatorService> _mockIndicatorService = null!;
    private ILogger<IndicatorsController> _logger = null!;
    private IndicatorsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockIndicatorService = new Mock<IIndicatorService>();
        _logger = NullLogger<IndicatorsController>.Instance;
        _controller = new IndicatorsController(
            _mockIndicatorService.Object,
            _logger
        );
    }

    private List<IndicatorDto> CreateSampleIndicatorDtos(int count = 2)
    {
        var list = new List<IndicatorDto>();
        for (int i = 1; i <= count; i++) { /* ... (same as before) ... */ }
        return list;
    }
    
    private IndicatorDto CreateSingleSampleDto(string varCode = "VAR1", string countryCode = "C1")
    {
        return new IndicatorDto
        {
            VariableCode = varCode,
            VariableName = $"Variable {varCode}",
            CountryCode = countryCode,
            CountryName = $"Country {countryCode}",
            ChapterName = "Chapter",
            SubchapterName = "Sub",
            UnitCode = "0",
            UnitDescription = "Unit",
            Values = new List<YearValueDto> { new YearValueDto { Year = 2020, Amount = 100m } }
        };
    }

    /// <summary>
    /// Tests that GetIndicators returns OkObjectResult with data when the service succeeds (no filters).
    /// </summary>
    [Test]
    public async Task GetIndicators_WhenServiceReturnsData_ReturnsOkObjectResultWithData()
    {
        // Arrange
        var expectedIndicators = CreateSampleIndicatorDtos(3);
        _mockIndicatorService
            .Setup(s => s.GetIndicatorsAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<List<int>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedIndicators);

        // Act
        var actionResult = await _controller.GetIndicators(null, null, null, null, null, CancellationToken.None);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode);
        Assert.IsInstanceOf<IEnumerable<IndicatorDto>>(okResult?.Value);
        var actualIndicators = okResult?.Value as IEnumerable<IndicatorDto>;
        Assert.IsNotNull(actualIndicators);
        Assert.AreEqual(expectedIndicators.Count, actualIndicators?.Count());

        _mockIndicatorService.Verify(s => s.GetIndicatorsAsync(
            null, null, null, null, null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetIndicators returns OkObjectResult with an empty list when the service returns empty (no filters).
    /// </summary>
    [Test]
    public async Task GetIndicators_WhenServiceReturnsEmpty_ReturnsOkObjectResultWithEmptyList()
    {
        // Arrange
        var emptyList = new List<IndicatorDto>();
        _mockIndicatorService
            .Setup(s => s.GetIndicatorsAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<List<int>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var actionResult = await _controller.GetIndicators(null, null, null, null, null, CancellationToken.None);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode);
        Assert.IsInstanceOf<IEnumerable<IndicatorDto>>(okResult?.Value);
        var actualIndicators = okResult?.Value as IEnumerable<IndicatorDto>;
        Assert.IsNotNull(actualIndicators);
        Assert.IsFalse(actualIndicators?.Any());

        _mockIndicatorService.Verify(s => s.GetIndicatorsAsync(
            null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetIndicators returns StatusCode 500 when the service throws an exception (no filters).
    /// </summary>
    [Test]
    public async Task GetIndicators_WhenServiceThrowsException_ReturnsStatusInternalServerError()
    {
        // Arrange
        var testException = new InvalidOperationException("Service layer error");
        _mockIndicatorService
            .Setup(s => s.GetIndicatorsAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<List<int>?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(testException);

        // Act
        var actionResult = await _controller.GetIndicators(null, null, null, null, null, CancellationToken.None);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult.Result);
        var objectResult = actionResult.Result as ObjectResult;
        Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        Assert.AreEqual("An internal error occurred while retrieving indicators.", objectResult?.Value);

        _mockIndicatorService.Verify(s => s.GetIndicatorsAsync(
            null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }


    /// <summary>
    /// Tests that GetIndicators correctly passes filter parameters from the query string
    /// to the IIndicatorService.
    /// </summary>
    [Test]
    public async Task GetIndicators_WithFilterParameters_PassesFiltersToService()
    {
        // Arrange
        string expectedCountry = "DE";
        string expectedVariable = "NPTD";
        string expectedChapter = "TestChapter";
        string expectedSubchapter = "TestSub";
        List<int> expectedYears = new List<int> { 2020, 2021 };
        var expectedResult = CreateSampleIndicatorDtos(1); // Service returns some data

        _mockIndicatorService
            .Setup(s => s.GetIndicatorsAsync(
                expectedCountry,
                expectedVariable,
                expectedChapter,
                expectedSubchapter,
                It.Is<List<int>>(list => list.SequenceEqual(expectedYears)), // Match the list content
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var actionResult = await _controller.GetIndicators(
            countryCode: expectedCountry,
            variableCode: expectedVariable,
            chapterName: expectedChapter,
            subchapterName: expectedSubchapter,
            years: expectedYears,
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);

        _mockIndicatorService.Verify();
    }

    /// <summary>
    /// Tests that GetIndicators correctly handles null or empty filter parameters,
    /// passing nulls to the IIndicatorService.
    /// </summary>
    [Test]
    public async Task GetIndicators_WithNullOrEmptyFilters_PassesNullsToService()
    {
        // Arrange
        string? countryCode = null;
        string variableCode = "";
        string chapterName = "SomeChapter";
        string? subchapterName = null;
        List<int> years = new List<int>();
        var expectedResult = CreateSampleIndicatorDtos(1);

        _mockIndicatorService
            .Setup(s => s.GetIndicatorsAsync(
                countryCode,
                variableCode,
                chapterName,
                subchapterName,
                It.Is<List<int>>(list => !list.Any()),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var actionResult = await _controller.GetIndicators(
            countryCode: countryCode,
            variableCode: variableCode,
            chapterName: chapterName,
            subchapterName: subchapterName,
            years: years,
            cancellationToken: CancellationToken.None);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        _mockIndicatorService.Verify();
    }
    
    /// <summary>
    /// Tests that GetSpecificIndicator returns OkObjectResult with data when the service finds the indicator.
    /// </summary>
    [Test]
    public async Task GetSpecificIndicator_WhenServiceReturnsData_ReturnsOkObjectResultWithData()
    {
        // Arrange
        string targetVariableCode = "VAR1";
        string targetCountryCode = "C1";
        var expectedIndicator = CreateSingleSampleDto(targetVariableCode, targetCountryCode);

        _mockIndicatorService
            .Setup(s => s.GetSpecificIndicatorAsync(targetVariableCode, targetCountryCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedIndicator);

        // Act
        var actionResult = await _controller.GetSpecificIndicator(targetVariableCode, targetCountryCode, CancellationToken.None);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result, "Should return OkObjectResult.");
        var okResult = actionResult.Result as OkObjectResult;

        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode, "Status code should be 200 OK.");

        Assert.IsInstanceOf<IndicatorDto>(okResult?.Value, "Value should be IndicatorDto.");
        var actualIndicator = okResult?.Value as IndicatorDto;
        Assert.IsNotNull(actualIndicator, "Actual indicator should not be null.");
        Assert.AreEqual(expectedIndicator.VariableCode, actualIndicator?.VariableCode);
        Assert.AreEqual(expectedIndicator.CountryCode, actualIndicator?.CountryCode);

        _mockIndicatorService.Verify(s => s.GetSpecificIndicatorAsync(targetVariableCode, targetCountryCode, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    /// <summary>
    /// Tests that GetSpecificIndicator returns NotFoundResult when the service returns null.
    /// </summary>
    [Test]
    public async Task GetSpecificIndicator_WhenServiceReturnsNull_ReturnsNotFoundResult()
    {
        // Arrange
        string targetVariableCode = "VAR_NONE";
        string targetCountryCode = "C_NONE";

        _mockIndicatorService
            .Setup(s => s.GetSpecificIndicatorAsync(targetVariableCode, targetCountryCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndicatorDto?)null);

        // Act
        var actionResult = await _controller.GetSpecificIndicator(targetVariableCode, targetCountryCode, CancellationToken.None);

        // Assert
        Assert.IsInstanceOf<NotFoundResult>(actionResult.Result, "Should return NotFoundResult.");
        var notFoundResult = actionResult.Result as NotFoundResult;

        Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult?.StatusCode, "Status code should be 404 Not Found.");

        _mockIndicatorService.Verify(s => s.GetSpecificIndicatorAsync(targetVariableCode, targetCountryCode, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    /// <summary>
    /// Tests that GetSpecificIndicator returns StatusCode 500 when the service throws an exception.
    /// </summary>
    [Test]
    public async Task GetSpecificIndicator_WhenServiceThrowsException_ReturnsStatusInternalServerError()
    {
        // Arrange
        string targetVariableCode = "VAR_EX";
        string targetCountryCode = "C_EX";
        var testException = new TimeoutException("Service timeout");

        _mockIndicatorService
            .Setup(s => s.GetSpecificIndicatorAsync(targetVariableCode, targetCountryCode, It.IsAny<CancellationToken>()))
            .ThrowsAsync(testException); // Setup mock service to throw

        // Act
        var actionResult = await _controller.GetSpecificIndicator(targetVariableCode, targetCountryCode, CancellationToken.None);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult.Result, "Should return ObjectResult on exception.");
        var objectResult = actionResult.Result as ObjectResult;

        Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult?.StatusCode, "Status code should be 500.");

        Assert.AreEqual("An internal error occurred while retrieving the indicator.", objectResult?.Value);

        _mockIndicatorService.Verify(s => s.GetSpecificIndicatorAsync(targetVariableCode, targetCountryCode, It.IsAny<CancellationToken>()), Times.Once);
    }
}