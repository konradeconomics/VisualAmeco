using Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Interfaces;

namespace VisualAmeco.Testing.Application.Services;

[TestFixture]
public class LookupServiceTests
{
    private Mock<ICountryRepository> _mockCountryRepo = null!;
    private Mock<IChapterRepository> _mockChapterRepo = null!;

    private ILogger<LookupService> _logger = null!;
    private LookupService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockCountryRepo = new Mock<ICountryRepository>();
        _mockChapterRepo = new Mock<IChapterRepository>();
        _logger = NullLogger<LookupService>.Instance;

        _service = new LookupService(
            _mockCountryRepo.Object,
            _mockChapterRepo.Object,
            _logger
        );
    }

    /// <summary>
    /// Tests GetAllCountriesAsync returns mapped DTOs when repository returns data.
    /// </summary>
    [Test]
    public async Task GetAllCountriesAsync_WhenRepoReturnsData_ReturnsCountryDtos()
    {
        // Arrange
        var mockCountries = new List<Country>
        {
            new Country { Id = 1, Code = "DE", Name = "Germany" },
            new Country { Id = 2, Code = "FR", Name = "France" }
        };
        _mockCountryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(mockCountries);

        // Act
        var result = await _service.GetAllCountriesAsync();
        var resultList = result.ToList();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, resultList.Count);
        Assert.IsTrue(resultList.Any(c => c.Code == "DE" && c.Name == "Germany"));
        Assert.IsTrue(resultList.Any(c => c.Code == "FR" && c.Name == "France"));
        _mockCountryRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// Tests GetAllCountriesAsync returns empty list when repository returns empty.
    /// </summary>
    [Test]
    public async Task GetAllCountriesAsync_WhenRepoReturnsEmpty_ReturnsEmptyList()
    {
        // Arrange
        var mockCountries = new List<Country>();
        _mockCountryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(mockCountries);

        // Act
        var result = await _service.GetAllCountriesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Any());
        _mockCountryRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// Tests GetAllCountriesAsync propagates exceptions from the repository.
    /// </summary>
    [Test]
    public void GetAllCountriesAsync_WhenRepoThrows_ThrowsException()
    {
        // Arrange
        var testException = new InvalidOperationException("DB Error");
        _mockCountryRepo.Setup(r => r.GetAllAsync()).ThrowsAsync(testException);

        // Act & Assert
        var thrown = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GetAllCountriesAsync());
        Assert.AreEqual(testException.Message, thrown?.Message);
        _mockCountryRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }
    
    /// <summary>
    /// Tests GetAllChaptersAsync returns mapped DTOs when repository returns data.
    /// </summary>
    [Test]
    public async Task GetAllChaptersAsync_WhenRepoReturnsData_ReturnsChapterDtos()
    {
        // Arrange
        var mockChapters = new List<Chapter>
        {
            new Chapter { Id = 1, Name = "Chapter One" },
            new Chapter { Id = 2, Name = "Chapter Two" }
        };
        _mockChapterRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(mockChapters);

        // Act
        var result = await _service.GetAllChaptersAsync();
        Assert.IsNotNull(result, "Service result should not be null.");
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(2, resultList.Count, "List should contain 2 chapters.");
        // Optional: Check if list contains nulls if concerned
        // Assert.IsTrue(resultList.All(c => c != null), "All ChapterDto elements in the list should be non-null.");

        // Use simple Any() calls to verify content
        Assert.IsTrue(resultList.Any(c => c.Id == 1 && c.Name == "Chapter One"), "Chapter 1 not found or incorrect.");
        Assert.IsTrue(resultList.Any(c => c.Id == 2 && c.Name == "Chapter Two"), "Chapter 2 not found or incorrect.");

        _mockChapterRepo.Verify(r => r.GetAllAsync(), Times.Once); // Verify repo interaction
    }

    /// <summary>
    /// Tests GetAllChaptersAsync returns empty list when repository returns empty.
    /// </summary>
    [Test]
    public async Task GetAllChaptersAsync_WhenRepoReturnsEmpty_ReturnsEmptyList()
    {
        // Arrange
        var mockChapters = new List<Chapter>();
        _mockChapterRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(mockChapters);

        // Act
        var result = await _service.GetAllChaptersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Any()); // Check the IEnumerable is empty
        _mockChapterRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// Tests GetAllChaptersAsync propagates exceptions from the repository.
    /// </summary>
    [Test]
    public void GetAllChaptersAsync_WhenRepoThrows_ThrowsException()
    {
        // Arrange
        var testException = new InvalidOperationException("DB Error Chapters");
        _mockChapterRepo.Setup(r => r.GetAllAsync()).ThrowsAsync(testException);

        // Act & Assert
        var thrown =
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GetAllChaptersAsync());
        Assert.AreEqual(testException.Message, thrown?.Message);
        _mockChapterRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }
}