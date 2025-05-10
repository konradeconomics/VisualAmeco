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
/// Unit tests for the VariablesController.
/// </summary>
[TestFixture]
public class VariablesControllerTests
{
    private Mock<ILookupService> _mockLookupService = null!;
    private ILogger<VariablesController> _logger = null!;
    private VariablesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockLookupService = new Mock<ILookupService>();
        _logger = NullLogger<VariablesController>.Instance;

        _controller = new VariablesController(
            _mockLookupService.Object,
            _logger
        );
    }

    // Helper to create sample DTO data
    private List<VariableDto> CreateSampleVariableDtos(int count = 2)
    {
        var list = new List<VariableDto>();
        for (int i = 1; i <= count; i++)
        {
            list.Add(new VariableDto
            {
                Code = $"VAR{i}",
                Name = $"Variable {i}",
                UnitCode = $"{i}",
                UnitDescription = $"Unit {i}",
                SubchapterId = 100 + i,
                SubchapterName = $"Subchapter 1.{i}"
            });
        }

        return list;
    }

    /// <summary>
    /// Tests GetAllVariables returns OkObjectResult with data when service succeeds (no filters).
    /// </summary>
    [Test]
    public async Task GetAllVariables_NoFilters_WhenServiceReturnsData_ReturnsOkObjectResultWithData()
    {
        // Arrange
        var expectedVariables = CreateSampleVariableDtos(3);
        _mockLookupService
            .Setup(s => s.GetVariablesAsync(null, null))
            .ReturnsAsync(expectedVariables);

        // Act
        var actionResult = await _controller.GetAllVariables(null, null);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode);
        Assert.IsInstanceOf<IEnumerable<VariableDto>>(okResult?.Value);
        var actualVariables = okResult?.Value as IEnumerable<VariableDto>;
        Assert.IsNotNull(actualVariables);
        Assert.AreEqual(expectedVariables.Count, actualVariables?.Count());

        _mockLookupService.Verify(s => s.GetVariablesAsync(null, null), Times.Once); // Verify service call
    }

    /// <summary>
    /// Tests GetAllVariables returns OkObjectResult with empty list when service returns empty (no filters).
    /// </summary>
    [Test]
    public async Task GetAllVariables_NoFilters_WhenServiceReturnsEmpty_ReturnsOkObjectResultWithEmptyList()
    {
        // Arrange
        var emptyList = new List<VariableDto>();
        _mockLookupService
            .Setup(s => s.GetVariablesAsync(null, null))
            .ReturnsAsync(emptyList);

        // Act
        var actionResult = await _controller.GetAllVariables(null, null);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode);
        Assert.IsInstanceOf<IEnumerable<VariableDto>>(okResult?.Value);
        var actualVariables = okResult?.Value as IEnumerable<VariableDto>;
        Assert.IsNotNull(actualVariables);
        Assert.IsFalse(actualVariables?.Any());

        _mockLookupService.Verify(s => s.GetVariablesAsync(null, null), Times.Once);
    }

    /// <summary>
    /// Tests GetAllVariables correctly passes filter parameters to the service.
    /// </summary>
    [Test]
    public async Task GetAllVariables_WithFilters_PassesFiltersToService()
    {
        // Arrange
        int? filterChapterId = 10;
        int? filterSubchapterId = 101;
        var expectedResult = CreateSampleVariableDtos(1); // Service returns some data

        _mockLookupService
            .Setup(s => s.GetVariablesAsync(filterChapterId, filterSubchapterId))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var actionResult = await _controller.GetAllVariables(
            chapterId: filterChapterId,
            subchapterId: filterSubchapterId);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result); // Basic check it returned OK

        _mockLookupService.Verify();
    }

    /// <summary>
    /// Tests GetAllVariables returns StatusCode 500 when service throws exception.
    /// </summary>
    [Test]
    public async Task GetAllVariables_WhenServiceThrowsException_ReturnsStatusInternalServerError()
    {
        // Arrange
        var testException = new InvalidOperationException("Service error");
        _mockLookupService
            .Setup(s => s.GetVariablesAsync(It.IsAny<int?>(), It.IsAny<int?>())) // Setup with any filters
            .ThrowsAsync(testException);

        // Act
        var actionResult = await _controller.GetAllVariables(null, null); // Call without filters

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult.Result);
        var objectResult = actionResult.Result as ObjectResult;
        Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        Assert.AreEqual("An internal error occurred while retrieving variables.", objectResult?.Value);

        _mockLookupService.Verify(s => s.GetVariablesAsync(null, null), Times.Once); // Verify service was called
    }
}