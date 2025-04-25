using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using VisualAmeco.API.Controllers;
using VisualAmeco.Application.DTOs;
using VisualAmeco.Application.Interfaces;

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

    /// <summary>
    /// Sets up mocks and the controller instance before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // Create fresh mocks for each test
        _mockIndicatorService = new Mock<IIndicatorService>();
        _logger = NullLogger<IndicatorsController>.Instance; // Use NullLogger for controller tests

        // Create the controller instance, injecting the mocks
        _controller = new IndicatorsController(
            _mockIndicatorService.Object,
            _logger
        );
    }

    // Helper to create sample DTO data
    private List<IndicatorDto> CreateSampleIndicatorDtos(int count = 2)
    {
        var list = new List<IndicatorDto>();
        for (int i = 1; i <= count; i++)
        {
            list.Add(new IndicatorDto
            {
                VariableCode = $"CODE{i}",
                VariableName = $"Variable {i}",
                CountryCode = $"C{i}",
                CountryName = $"Country {i}",
                ChapterName = "Chapter",
                SubchapterName = "Sub",
                Unit = "Units",
                Values = new List<YearValueDto> { new YearValueDto { Year = 2020, Amount = i * 100m } }
            });
        }
        return list;
    }

    /// <summary>
    /// Tests that GetIndicators returns OkObjectResult with data when the service succeeds.
    /// </summary>
    [Test]
    public async Task GetIndicators_WhenServiceReturnsData_ReturnsOkObjectResultWithData()
    {
        // Arrange
        var expectedIndicators = CreateSampleIndicatorDtos(3);
        _mockIndicatorService
            .Setup(s => s.GetIndicatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedIndicators); // Setup mock service to return data

        // Act
        var actionResult = await _controller.GetIndicators(CancellationToken.None);

        // Assert
        // 1. Check the result type is OkObjectResult
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result, "Should return OkObjectResult.");
        var okResult = actionResult.Result as OkObjectResult;

        // 2. Check the status code is 200
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode, "Status code should be 200 OK.");

        // 3. Check the returned value type and content
        Assert.IsInstanceOf<IEnumerable<IndicatorDto>>(okResult?.Value, "Value should be IEnumerable<IndicatorDto>.");
        var actualIndicators = okResult?.Value as IEnumerable<IndicatorDto>;
        Assert.IsNotNull(actualIndicators, "Actual indicators list should not be null.");
        // Compare counts
        Assert.AreEqual(expectedIndicators.Count, actualIndicators?.Count(), "Indicator counts should match.");
        // Note: Comparing actual list content equality can be complex. Count is often sufficient for controller tests.
        // If needed, use CollectionAssert.AreEquivalent or custom comparison.

        // 4. Verify the service method was called exactly once
        _mockIndicatorService.Verify(s => s.GetIndicatorsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetIndicators returns OkObjectResult with an empty list when the service returns empty.
    /// </summary>
    [Test]
    public async Task GetIndicators_WhenServiceReturnsEmpty_ReturnsOkObjectResultWithEmptyList()
    {
        // Arrange
        var emptyList = new List<IndicatorDto>();
        _mockIndicatorService
            .Setup(s => s.GetIndicatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList); // Setup mock service to return empty list

        // Act
        var actionResult = await _controller.GetIndicators(CancellationToken.None);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode);
        Assert.IsInstanceOf<IEnumerable<IndicatorDto>>(okResult?.Value);
        var actualIndicators = okResult?.Value as IEnumerable<IndicatorDto>;
        Assert.IsNotNull(actualIndicators);
        Assert.IsFalse(actualIndicators?.Any(), "Returned list should be empty."); // Check if empty

        _mockIndicatorService.Verify(s => s.GetIndicatorsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    /// <summary>
    /// Tests that GetIndicators returns StatusCode 500 when the service throws an exception.
    /// </summary>
    [Test]
    public async Task GetIndicators_WhenServiceThrowsException_ReturnsStatusInternalServerError()
    {
        // Arrange
        var testException = new InvalidOperationException("Service layer error");
        _mockIndicatorService
            .Setup(s => s.GetIndicatorsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(testException); // Setup mock service to throw

        // Act
        var actionResult = await _controller.GetIndicators(CancellationToken.None);

        // Assert
        // 1. Check the result type is ObjectResult (because controller returns StatusCode(500, "message"))
        Assert.IsInstanceOf<ObjectResult>(actionResult.Result, "Should return ObjectResult on exception.");
        var objectResult = actionResult.Result as ObjectResult;

        // 2. Check the status code is 500
        Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult?.StatusCode, "Status code should be 500.");

        // 3. Optional: Check the error message returned by the controller
        Assert.AreEqual("An internal error occurred while retrieving indicators.", objectResult?.Value);

        // 4. Verify the service method was called exactly once
        _mockIndicatorService.Verify(s => s.GetIndicatorsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}