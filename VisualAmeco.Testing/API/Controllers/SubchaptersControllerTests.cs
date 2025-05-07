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
/// Unit tests for the SubchaptersController.
/// </summary>
[TestFixture]
public class SubchaptersControllerTests
{
    private Mock<ILookupService> _mockLookupService = null!;
    private ILogger<SubchaptersController> _logger = null!;
    private SubchaptersController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockLookupService = new Mock<ILookupService>();
        _logger = NullLogger<SubchaptersController>.Instance;

        // Inject the ILookupService mock
        _controller = new SubchaptersController(
            _mockLookupService.Object,
            _logger
        );
    }

    // Helper to create sample DTO data
    private List<SubchapterDto> CreateSampleSubchapterDtos(int chapterId, int count = 2)
    {
        var list = new List<SubchapterDto>();
        for (int i = 1; i <= count; i++)
        {
            list.Add(new SubchapterDto
                { Id = (chapterId * 10) + i, Name = $"Subchapter {chapterId}.{i}", ChapterId = chapterId });
        }

        return list;
    }

    /// <summary>
    /// Tests GetAllSubchapters returns OkObjectResult with data when service succeeds (no filter).
    /// </summary>
    [Test]
    public async Task GetAllSubchapters_NoFilter_WhenServiceReturnsData_ReturnsOkObjectResultWithData()
    {
        // Arrange
        var expectedSubchapters = CreateSampleSubchapterDtos(1, 3);
        _mockLookupService
            .Setup(s => s.GetSubchaptersAsync(null))
            .ReturnsAsync(expectedSubchapters);

        // Act
        var actionResult = await _controller.GetAllSubchapters(null);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode);
        Assert.IsInstanceOf<IEnumerable<SubchapterDto>>(okResult?.Value);
        var actualSubchapters = okResult?.Value as IEnumerable<SubchapterDto>;
        Assert.IsNotNull(actualSubchapters);
        Assert.AreEqual(expectedSubchapters.Count, actualSubchapters?.Count());

        _mockLookupService.Verify(s => s.GetSubchaptersAsync(null), Times.Once);
    }

    /// <summary>
    /// Tests GetAllSubchapters returns OkObjectResult with empty list when service returns empty (no filter).
    /// </summary>
    [Test]
    public async Task GetAllSubchapters_NoFilter_WhenServiceReturnsEmpty_ReturnsOkObjectResultWithEmptyList()
    {
        // Arrange
        var emptyList = new List<SubchapterDto>();
        _mockLookupService
            .Setup(s => s.GetSubchaptersAsync(null))
            .ReturnsAsync(emptyList);

        // Act
        var actionResult = await _controller.GetAllSubchapters(null);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode);
        Assert.IsInstanceOf<IEnumerable<SubchapterDto>>(okResult?.Value);
        var actualSubchapters = okResult?.Value as IEnumerable<SubchapterDto>;
        Assert.IsNotNull(actualSubchapters);
        Assert.IsFalse(actualSubchapters?.Any());

        _mockLookupService.Verify(s => s.GetSubchaptersAsync(null), Times.Once);
    }

    /// <summary>
    /// Tests GetAllSubchapters correctly passes chapterId filter to the service.
    /// </summary>
    [Test]
    public async Task GetAllSubchapters_WithChapterIdFilter_PassesFilterToService()
    {
        // Arrange
        int? filterChapterId = 5;
        var expectedResult = CreateSampleSubchapterDtos(filterChapterId.Value, 2); // Service returns some data

        // Setup the service mock to expect this specific filter
        _mockLookupService
            .Setup(s => s.GetSubchaptersAsync(filterChapterId))
            .ReturnsAsync(expectedResult)
            .Verifiable(); 

        // Act
        var actionResult = await _controller.GetAllSubchapters(chapterId: filterChapterId);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);

        _mockLookupService.Verify();
    }

    /// <summary>
    /// Tests GetAllSubchapters returns StatusCode 500 when service throws exception.
    /// </summary>
    [Test]
    public async Task GetAllSubchapters_WhenServiceThrowsException_ReturnsStatusInternalServerError()
    {
        // Arrange
        var testException = new InvalidOperationException("Service error");
        _mockLookupService
            .Setup(s => s.GetSubchaptersAsync(It.IsAny<int?>()))
            .ThrowsAsync(testException);

        // Act
        var actionResult = await _controller.GetAllSubchapters(null);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult.Result);
        var objectResult = actionResult.Result as ObjectResult;
        Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        Assert.AreEqual("An internal error occurred while retrieving subchapters.", objectResult?.Value);

        _mockLookupService.Verify(s => s.GetSubchaptersAsync(null), Times.Once);
    }
}