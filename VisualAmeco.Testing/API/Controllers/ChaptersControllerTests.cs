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
/// Unit tests for the ChaptersController.
/// </summary>
[TestFixture]
public class ChaptersControllerTests
{
    private Mock<ILookupService> _mockLookupService = null!;
    private ILogger<ChaptersController> _logger = null!;
    private ChaptersController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockLookupService = new Mock<ILookupService>();
        _logger = NullLogger<ChaptersController>.Instance;

        _controller = new ChaptersController(
            _mockLookupService.Object,
            _logger
        );
    }

    private List<ChapterDto> CreateSampleChapterDtos(int count = 2)
    {
        var list = new List<ChapterDto>();
        for (int i = 1; i <= count; i++)
        {
            list.Add(new ChapterDto { Id = i, Name = $"Chapter {i}" });
        }

        return list;
    }

    /// <summary>
    /// Tests GetAllChapters returns OkObjectResult with data when service succeeds.
    /// </summary>
    [Test]
    public async Task GetAllChapters_WhenServiceReturnsData_ReturnsOkObjectResultWithData()
    {
        // Arrange
        var expectedChapters = CreateSampleChapterDtos(3);
        _mockLookupService
            .Setup(s => s.GetAllChaptersAsync())
            .ReturnsAsync(expectedChapters);

        // Act
        var actionResult = await _controller.GetAllChapters();

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result, "Should return OkObjectResult.");
        var okResult = actionResult.Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode, "Status code should be 200 OK.");
        Assert.IsInstanceOf<IEnumerable<ChapterDto>>(okResult?.Value, "Value should be IEnumerable<ChapterDto>.");
        var actualChapters = okResult?.Value as IEnumerable<ChapterDto>;
        Assert.IsNotNull(actualChapters, "Actual chapters list should not be null.");
        Assert.AreEqual(expectedChapters.Count, actualChapters?.Count(), "Chapter counts should match.");

        _mockLookupService.Verify(s => s.GetAllChaptersAsync(), Times.Once);
    }

    /// <summary>
    /// Tests GetAllChapters returns OkObjectResult with empty list when service returns empty.
    /// </summary>
    [Test]
    public async Task GetAllChapters_WhenServiceReturnsEmpty_ReturnsOkObjectResultWithEmptyList()
    {
        // Arrange
        var emptyList = new List<ChapterDto>();
        _mockLookupService
            .Setup(s => s.GetAllChaptersAsync())
            .ReturnsAsync(emptyList);

        // Act
        var actionResult = await _controller.GetAllChapters();

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode);
        Assert.IsInstanceOf<IEnumerable<ChapterDto>>(okResult?.Value);
        var actualChapters = okResult?.Value as IEnumerable<ChapterDto>;
        Assert.IsNotNull(actualChapters);
        Assert.IsFalse(actualChapters?.Any(), "Returned list should be empty.");

        _mockLookupService.Verify(s => s.GetAllChaptersAsync(), Times.Once);
    }

    /// <summary>
    /// Tests GetAllChapters returns StatusCode 500 when service throws exception.
    /// </summary>
    [Test]
    public async Task GetAllChapters_WhenServiceThrowsException_ReturnsStatusInternalServerError()
    {
        // Arrange
        var testException = new InvalidOperationException("Service error");
        _mockLookupService
            .Setup(s => s.GetAllChaptersAsync())
            .ThrowsAsync(testException);

        // Act
        var actionResult = await _controller.GetAllChapters();

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult.Result);
        var objectResult = actionResult.Result as ObjectResult;
        Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        Assert.AreEqual("An internal error occurred while retrieving chapters.", objectResult?.Value);

        _mockLookupService.Verify(s => s.GetAllChaptersAsync(), Times.Once);
    }
}