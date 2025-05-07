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

[TestFixture]
public class CountriesControllerTests
{
    private Mock<ILookupService> _mockLookupService = null!;
    private ILogger<CountriesController> _logger = null!;
    private CountriesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockLookupService = new Mock<ILookupService>();
        _logger = NullLogger<CountriesController>.Instance;

        _controller = new CountriesController(
            _mockLookupService.Object,
            _logger
        );
    }

    private List<CountryDto> CreateSampleCountryDtos(int count = 2)
    {
        var list = new List<CountryDto>();
        for (int i = 1; i <= count; i++)
        {
            list.Add(new CountryDto { Code = $"C{i}", Name = $"Country {i}" });
        }

        return list;
    }

    /// <summary>
    /// Tests GetAllCountries returns OkObjectResult with data when service succeeds.
    /// </summary>
    [Test]
    public async Task GetAllCountries_WhenServiceReturnsData_ReturnsOkObjectResultWithData()
    {
        // Arrange
        var expectedCountries = CreateSampleCountryDtos(3);
        _mockLookupService
            .Setup(s => s.GetAllCountriesAsync())
            .ReturnsAsync(expectedCountries);

        // Act
        var actionResult = await _controller.GetAllCountries();

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode);
        Assert.IsInstanceOf<IEnumerable<CountryDto>>(okResult?.Value);
        var actualCountries = okResult?.Value as IEnumerable<CountryDto>;
        Assert.IsNotNull(actualCountries);
        Assert.AreEqual(expectedCountries.Count, actualCountries?.Count());

        _mockLookupService.Verify(s => s.GetAllCountriesAsync(), Times.Once); // Verify service call
    }

    /// <summary>
    /// Tests GetAllCountries returns OkObjectResult with empty list when service returns empty.
    /// </summary>
    [Test]
    public async Task GetAllCountries_WhenServiceReturnsEmpty_ReturnsOkObjectResultWithEmptyList()
    {
        // Arrange
        var emptyList = new List<CountryDto>();
        _mockLookupService
            .Setup(s => s.GetAllCountriesAsync())
            .ReturnsAsync(emptyList);

        // Act
        var actionResult = await _controller.GetAllCountries();

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult.Result);
        var okResult = actionResult.Result as OkObjectResult;
        Assert.AreEqual(StatusCodes.Status200OK, okResult?.StatusCode);
        Assert.IsInstanceOf<IEnumerable<CountryDto>>(okResult?.Value);
        var actualCountries = okResult?.Value as IEnumerable<CountryDto>;
        Assert.IsNotNull(actualCountries);
        Assert.IsFalse(actualCountries?.Any());

        _mockLookupService.Verify(s => s.GetAllCountriesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests GetAllCountries returns StatusCode 500 when service throws exception.
    /// </summary>
    [Test]
    public async Task GetAllCountries_WhenServiceThrowsException_ReturnsStatusInternalServerError()
    {
        // Arrange
        var testException = new InvalidOperationException("Service error");
        _mockLookupService
            .Setup(s => s.GetAllCountriesAsync())
            .ThrowsAsync(testException);

        // Act
        var actionResult = await _controller.GetAllCountries();

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult.Result);
        var objectResult = actionResult.Result as ObjectResult;
        Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        Assert.AreEqual("An internal error occurred while retrieving countries.", objectResult?.Value);

        _mockLookupService.Verify(s => s.GetAllCountriesAsync(), Times.Once);
    }
}