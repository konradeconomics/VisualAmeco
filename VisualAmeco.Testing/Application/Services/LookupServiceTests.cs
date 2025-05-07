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
    private Mock<IVariableRepository> _mockVariableRepo = null!;

    private ILogger<LookupService> _logger = null!;
    private LookupService _service = null!;
    
    private Chapter _chapter1 = null!;
    private Subchapter _subchapter1 = null!;
    private Subchapter _subchapter2 = null!;
    private Variable _variable1 = null!;
    private Variable _variable2 = null!;
    private Variable _variable3 = null!;


    [SetUp]
    public void Setup()
    {
        _mockCountryRepo = new Mock<ICountryRepository>();
        _mockChapterRepo = new Mock<IChapterRepository>();
        _mockVariableRepo = new Mock<IVariableRepository>();
        _logger = NullLogger<LookupService>.Instance;

        _service = new LookupService(
            _mockCountryRepo.Object,
            _mockChapterRepo.Object,
            _mockVariableRepo.Object,
            _logger
        );
        
        _chapter1 = new Chapter { Id = 10, Name = "Chapter One" };
        _subchapter1 = new Subchapter { Id = 101, Name = "Subchapter 1.1", ChapterId = 10, Chapter = _chapter1 };
        _subchapter2 = new Subchapter { Id = 102, Name = "Subchapter 1.2", ChapterId = 10, Chapter = _chapter1 };
        _variable1 = new Variable { Id = 1010, Code = "VAR1", Name = "Variable One", Unit = "U1", SubChapterId = 101, SubChapter = _subchapter1 };
        _variable2 = new Variable { Id = 1011, Code = "VAR2", Name = "Variable Two", Unit = "U2", SubChapterId = 101, SubChapter = _subchapter1 };
        _variable3 = new Variable { Id = 1012, Code = "VAR3", Name = "Variable Three", Unit = "U3", SubChapterId = 102, SubChapter = _subchapter2 };
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
    
    /// <summary>
    /// Tests GetVariablesAsync with no filters returns all variables mapped to DTOs.
    /// </summary>
    [Test]
    public async Task GetVariablesAsync_NoFilters_ReturnsAllVariableDtos()
    {
        // Arrange
        var mockVariables = new List<Variable> { _variable1, _variable2, _variable3 };
        _mockVariableRepo.Setup(r => r.GetFilteredAsync(null, null)).ReturnsAsync(mockVariables);

        // Act
        var result = await _service.GetVariablesAsync();
        var resultList = result.ToList();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, resultList.Count, "Should return all 3 variables.");
        Assert.IsTrue(resultList.Any(v => v.Code == "VAR1" && v.SubchapterName == "Subchapter 1.1"));
        Assert.IsTrue(resultList.Any(v => v.Code == "VAR2" && v.SubchapterName == "Subchapter 1.1"));
        Assert.IsTrue(resultList.Any(v => v.Code == "VAR3" && v.SubchapterName == "Subchapter 1.2"));
        _mockVariableRepo.Verify(r => r.GetFilteredAsync(null, null), Times.Once); // Verify repo interaction
    }

    /// <summary>
    /// Tests GetVariablesAsync filtering by chapterId correctly calls repo and maps results.
    /// </summary>
    [Test]
    public async Task GetVariablesAsync_FilterByChapterId_CallsRepoAndReturnsDtos()
    {
        // Arrange
        int filterChapterId = 10;
        // Assume repo returns all variables for this chapter
        var mockVariables = new List<Variable> { _variable1, _variable2, _variable3 };
        _mockVariableRepo.Setup(r => r.GetFilteredAsync(filterChapterId, null)).ReturnsAsync(mockVariables);

        // Act
        var result = await _service.GetVariablesAsync(chapterId: filterChapterId);
        var resultList = result.ToList();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, resultList.Count); // All variables belong to chapter 10 in setup
        Assert.IsTrue(resultList.All(v => v.Code.StartsWith("VAR")));
        _mockVariableRepo.Verify(r => r.GetFilteredAsync(filterChapterId, null), Times.Once); // Verify repo called with correct filter
    }

    /// <summary>
    /// Tests GetVariablesAsync filtering by subchapterId correctly calls repo and maps results.
    /// </summary>
    [Test]
    public async Task GetVariablesAsync_FilterBySubchapterId_CallsRepoAndReturnsDtos()
    {
        // Arrange
        int filterSubchapterId = 101; // Only VAR1 and VAR2 belong to this one
        var mockVariables = new List<Variable> { _variable1, _variable2 }; // Repo returns only matching vars
        _mockVariableRepo.Setup(r => r.GetFilteredAsync(null, filterSubchapterId)).ReturnsAsync(mockVariables);

        // Act
        var result = await _service.GetVariablesAsync(subchapterId: filterSubchapterId);
        var resultList = result.ToList();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, resultList.Count, "Should only return variables for subchapter 101.");
        Assert.IsTrue(resultList.Any(v => v.Code == "VAR1"));
        Assert.IsTrue(resultList.Any(v => v.Code == "VAR2"));
        Assert.IsFalse(resultList.Any(v => v.Code == "VAR3")); // Verify VAR3 is not included
        _mockVariableRepo.Verify(r => r.GetFilteredAsync(null, filterSubchapterId), Times.Once); // Verify repo called with correct filter
    }

    /// <summary>
    /// Tests GetVariablesAsync filtering by both chapterId and subchapterId.
    /// </summary>
    [Test]
    public async Task GetVariablesAsync_FilterByChapterAndSubchapterId_CallsRepoAndReturnsDtos()
    {
        // Arrange
        int filterChapterId = 10;
        int filterSubchapterId = 102; // Only VAR3 belongs to this one
        var mockVariables = new List<Variable> { _variable3 }; // Repo returns only matching var
        _mockVariableRepo.Setup(r => r.GetFilteredAsync(filterChapterId, filterSubchapterId)).ReturnsAsync(mockVariables);

        // Act
        var result = await _service.GetVariablesAsync(chapterId: filterChapterId, subchapterId: filterSubchapterId);
        var resultList = result.ToList();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, resultList.Count, "Should only return variable for subchapter 102.");
        Assert.AreEqual("VAR3", resultList[0].Code);
        _mockVariableRepo.Verify(r => r.GetFilteredAsync(filterChapterId, filterSubchapterId), Times.Once); // Verify repo called with correct filters
    }

    /// <summary>
    /// Tests GetVariablesAsync returns empty list when repository returns empty.
    /// </summary>
    [Test]
    public async Task GetVariablesAsync_WhenRepoReturnsEmpty_ReturnsEmptyList()
    {
        // Arrange
        var mockVariables = new List<Variable>();
        _mockVariableRepo.Setup(r => r.GetFilteredAsync(It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(mockVariables);

        // Act
        var result = await _service.GetVariablesAsync(); // Call without filters

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Any());
        _mockVariableRepo.Verify(r => r.GetFilteredAsync(null, null), Times.Once); // Verify repo called
    }

    /// <summary>
    /// Tests GetVariablesAsync propagates exceptions from the repository.
    /// </summary>
    [Test]
    public void GetVariablesAsync_WhenRepoThrows_ThrowsException()
    {
        // Arrange
        var testException = new InvalidOperationException("DB Error Variables");
        _mockVariableRepo.Setup(r => r.GetFilteredAsync(It.IsAny<int?>(), It.IsAny<int?>())).ThrowsAsync(testException);

        // Act & Assert
        var thrown = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GetVariablesAsync());
        Assert.AreEqual(testException.Message, thrown?.Message);
        _mockVariableRepo.Verify(r => r.GetFilteredAsync(null, null), Times.Once);
    }

    /// <summary>
    /// Tests mapping handles null Subchapter navigation property gracefully.
    /// </summary>
    [Test]
    public async Task GetVariablesAsync_WhenVariableHasNullSubchapter_MapsSubchapterNameToNA()
    {
        // Arrange
        var variableWithNullSub = new Variable
        {
            Id = 999, Code = "VAR_NULL", Name = "Var Null Sub", Unit = "U", SubChapterId = 99, SubChapter = null!
        }; // Simulate missing include
        var mockVariables = new List<Variable> { variableWithNullSub };
        _mockVariableRepo.Setup(r => r.GetFilteredAsync(null, null)).ReturnsAsync(mockVariables);

        // Act
        var result = await _service.GetVariablesAsync();
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(1, resultList.Count);
        Assert.AreEqual("VAR_NULL", resultList[0].Code);
        Assert.AreEqual("N/A", resultList[0].SubchapterName,
            "SubchapterName should default to N/A if Subchapter entity is null.");
        _mockVariableRepo.Verify(r => r.GetFilteredAsync(null, null), Times.Once);
    }
}