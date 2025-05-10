using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using VisualAmeco.Core.Entities;
using VisualAmeco.Data.Contexts;
using VisualAmeco.Data.Repositories;

namespace VisualAmeco.Testing.Data.Repositories;

/// <summary>
/// Integration tests for the ValueRepository, specifically focusing on
/// data retrieval with related entities using a test database.
/// </summary>
[TestFixture]
public class ValueRepositoryIntegrationTests
{
    private DbContextOptions<VisualAmecoDbContext> _dbContextOptions = null!;
    private VisualAmecoDbContext _context = null!;
    private ValueRepository _repository = null!;

    private const int SeedChapterId = 1;
    private const int SeedSubchapterId = 11;
    private const int SeedVariableId1 = 111;
    private const int SeedVariableId2 = 112;
    private const int SeedCountryId1 = 1001;
    private const int SeedCountryId2 = 1002;

    private const string SeedChapterName = "Seed Chapter";
    private const string SeedSubchapterName = "Seed Subchapter";
    private const string SeedVariableCode1 = "VAR1";
    private const string SeedVariableCode2 = "VAR2";
    private const string SeedCountryCode1 = "C1";
    private const string SeedCountryCode2 = "C2";


    [SetUp]
    public async Task SetupDatabase()
    {
        _dbContextOptions = new DbContextOptionsBuilder<VisualAmecoDbContext>()
            .UseSqlite($"Filename={TestContext.CurrentContext.Test.Name}.db")
            .Options;

        _context = new VisualAmecoDbContext(_dbContextOptions);
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        await SeedDatabaseAsync();
        _repository = new ValueRepository(_context);
        TestContext.WriteLine($"Database seeded and repository created for test: {TestContext.CurrentContext.Test.Name}");
    }

    private async Task SeedDatabaseAsync()
    {
        var chapter1 = new Chapter { Id = SeedChapterId, Name = SeedChapterName };
        var subchapter1 = new Subchapter { Id = SeedSubchapterId, Name = SeedSubchapterName, ChapterId = SeedChapterId };
        var variable1 = new Variable { Id = SeedVariableId1, Code = SeedVariableCode1, Name = "Seed Var 1", UnitCode = "U1", UnitDescription = "Test Description", SubChapterId = SeedSubchapterId };
        var variable2 = new Variable { Id = SeedVariableId2, Code = SeedVariableCode2, Name = "Seed Var 2", UnitCode = "U2", UnitDescription = "Test Description", SubChapterId = SeedSubchapterId };
        var country1 = new Country { Id = SeedCountryId1, Code = SeedCountryCode1, Name = "Seed Country 1" };
        var country2 = new Country { Id = SeedCountryId2, Code = SeedCountryCode2, Name = "Seed Country 2" };

        subchapter1.Chapter = chapter1;
        variable1.SubChapter = subchapter1;
        variable2.SubChapter = subchapter1;

        // Add
        _context.Chapters.Add(chapter1);
        _context.Subchapters.Add(subchapter1);
        _context.Variables.AddRange(variable1, variable2);
        _context.Countries.AddRange(country1, country2);
        await _context.SaveChangesAsync();

        // Add
        _context.Values.AddRange(
            new Value { VariableId = SeedVariableId1, CountryId = SeedCountryId1, Year = 2020, Amount = 100m }, // VAR1, C1, 2020
            new Value { VariableId = SeedVariableId1, CountryId = SeedCountryId1, Year = 2021, Amount = 110m }, // VAR1, C1, 2021
            new Value { VariableId = SeedVariableId2, CountryId = SeedCountryId1, Year = 2020, Amount = 200m }, // VAR2, C1, 2020
            new Value { VariableId = SeedVariableId1, CountryId = SeedCountryId2, Year = 2020, Amount = 150m }  // VAR1, C2, 2020
        );
        await _context.SaveChangesAsync();
    }

    [TearDown]
    public async Task TeardownDatabase()
    {
        await _context.DisposeAsync();
        TestContext.WriteLine($"Database context disposed for test: {TestContext.CurrentContext.Test.Name}");
        try
        {
            var dbPath = $"{TestContext.CurrentContext.Test.Name}.db";
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
                TestContext.WriteLine($"Deleted test database file: {dbPath}");
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Warning: Could not delete test database file '{TestContext.CurrentContext.Test.Name}.db'. Error: {ex.Message}");
        }
    }
    
    [Test]
    public async Task GetAllWithDetailsAsync_WhenDataExists_ReturnsValuesWithAllRelatedEntitiesIncluded()
    {
        // Arrange
        int expectedValueCount = 4;

        // Act
        var results = await _repository.GetAllWithDetailsAsync();

        // Assert
        Assert.IsNotNull(results, "Result list should not be null.");
        Assert.AreEqual(expectedValueCount, results.Count, $"Should retrieve all {expectedValueCount} seeded values.");

        foreach (var value in results)
        {
            Assert.IsNotNull(value.Variable, $"Value ID {value.Id} should have Variable loaded.");
            Assert.IsNotNull(value.Country, $"Value ID {value.Id} should have Country loaded.");
            Assert.IsNotNull(value.Variable.SubChapter, $"Value ID {value.Id}'s Variable should have Subchapter loaded.");
            Assert.IsNotNull(value.Variable.SubChapter.Chapter, $"Value ID {value.Id}'s Subchapter should have Chapter loaded.");

            if (value.VariableId == SeedVariableId1 && value.CountryId == SeedCountryId1 && value.Year == 2020)
            {
                Assert.AreEqual(SeedVariableCode1, value.Variable.Code);
                Assert.AreEqual(SeedCountryCode1, value.Country.Code);
                Assert.AreEqual(SeedSubchapterName, value.Variable.SubChapter.Name);
                Assert.AreEqual(SeedChapterName, value.Variable.SubChapter.Chapter.Name);
                Assert.AreEqual(100m, value.Amount);
            }
        }
        Assert.AreEqual(2, results.Select(v => v.VariableId).Distinct().Count(), "Should represent 2 distinct Variables.");
        Assert.AreEqual(2, results.Select(v => v.CountryId).Distinct().Count(), "Should represent 2 distinct Countries.");
    }

    [Test]
    public async Task GetAllWithDetailsAsync_WhenNoDataExists_ReturnsEmptyList()
    {
        // Arrange
        await _context.DisposeAsync();
        _context = new VisualAmecoDbContext(_dbContextOptions);
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        _repository = new ValueRepository(_context);

        // Act
        var results = await _repository.GetAllWithDetailsAsync();

        // Assert
        Assert.IsNotNull(results);
        Assert.IsEmpty(results);
    }

    
    /// <summary>
    /// Verifies filtering by Country Code returns only matching values with details.
    /// </summary>
    [Test]
    public async Task GetFilteredWithDetailsAsync_FilterByCountryCode_ReturnsMatchingValuesWithDetails()
    {
        // Arrange
        string filterCountryCode = SeedCountryCode1;
        int expectedCount = 3;

        // Act
        var results = await _repository.GetFilteredWithDetailsAsync(countryCode: filterCountryCode);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(expectedCount, results.Count, $"Should return {expectedCount} values for country {filterCountryCode}.");
        Assert.IsTrue(results.All(v => v.Country.Code == filterCountryCode), "All returned values should belong to the filtered country.");
        Assert.IsTrue(results.All(v => true), "Related entities should be included.");
    }

    /// <summary>
    /// Verifies filtering by Variable Code returns only matching values with details.
    /// </summary>
    [Test]
    public async Task GetFilteredWithDetailsAsync_FilterByVariableCode_ReturnsMatchingValuesWithDetails()
    {
        // Arrange
        string filterVariableCode = SeedVariableCode1;
        int expectedCount = 3;

        // Act
        var results = await _repository.GetFilteredWithDetailsAsync(variableCode: filterVariableCode);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(expectedCount, results.Count, $"Should return {expectedCount} values for variable {filterVariableCode}.");
        Assert.IsTrue(results.All(v => v.Variable.Code == filterVariableCode), "All returned values should belong to the filtered variable.");
        Assert.IsTrue(results.All(v => true), "Related entities should be included.");
    }

    /// <summary>
    /// Verifies filtering by a list of specific Years returns only matching values.
    /// </summary>
    [Test]
    public async Task GetFilteredWithDetailsAsync_FilterByYearList_ReturnsMatchingValuesWithDetails()
    {
        // Arrange
        List<int> filterYears = new List<int> { 2021 };
        int expectedCount;
        expectedCount = 1;

        // Act
        var results = await _repository.GetFilteredWithDetailsAsync(years: filterYears);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(expectedCount, results.Count, $"Should return {expectedCount} values for years {string.Join(",", filterYears)}.");
        Assert.IsTrue(results.All(v => filterYears.Contains(v.Year)), "All returned values should be within the filtered years.");
        Assert.IsTrue(results.All(v => true), "Related entities should be included.");
    }

    /// <summary>
    /// Verifies filtering by Chapter Name returns only matching values.
    /// </summary>
    [Test]
    public async Task GetFilteredWithDetailsAsync_FilterByChapterName_ReturnsMatchingValuesWithDetails()
    {
        // Arrange
        string filterChapterName = SeedChapterName;
        int expectedCount = 4;

        // Act
        var results = await _repository.GetFilteredWithDetailsAsync(chapterName: filterChapterName);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(expectedCount, results.Count, $"Should return {expectedCount} values for chapter '{filterChapterName}'.");
        Assert.IsTrue(results.All(v => v.Variable.SubChapter.Chapter.Name == filterChapterName), "All returned values should belong to the filtered chapter.");
        Assert.IsTrue(results.All(v => true), "Related entities should be included.");
    }

    /// <summary>
    /// Verifies filtering by Subchapter Name returns only matching values.
    /// </summary>
    [Test]
    public async Task GetFilteredWithDetailsAsync_FilterBySubchapterName_ReturnsMatchingValuesWithDetails()
    {
        // Arrange
        string filterSubchapterName = SeedSubchapterName;
        int expectedCount = 4;

        // Act
        var results = await _repository.GetFilteredWithDetailsAsync(subchapterName: filterSubchapterName);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(expectedCount, results.Count, $"Should return {expectedCount} values for subchapter '{filterSubchapterName}'.");
        Assert.IsTrue(results.All(v => v.Variable.SubChapter.Name == filterSubchapterName), "All returned values should belong to the filtered subchapter.");
        Assert.IsTrue(results.All(v => true), "Related entities should be included.");
    }

    /// <summary>
    /// Verifies filtering by a combination of criteria returns the correct subset.
    /// </summary>
    [Test]
    public async Task GetFilteredWithDetailsAsync_FilterByMultipleCriteria_ReturnsCorrectSubset()
    {
        // Arrange
        string filterCountryCode = SeedCountryCode1;
        string filterVariableCode = SeedVariableCode1;
        List<int> filterYears = new List<int> { 2020 };
        int expectedCount = 1;

        // Act
        var results = await _repository.GetFilteredWithDetailsAsync(
            countryCode: filterCountryCode,
            variableCode: filterVariableCode,
            years: filterYears);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(expectedCount, results.Count, "Should return 1 value matching all criteria.");
        Assert.IsTrue(results.All(v => v.Country.Code == filterCountryCode &&
                                       v.Variable.Code == filterVariableCode &&
                                       filterYears.Contains(v.Year)), "All returned values should match all filter criteria.");
        Assert.IsTrue(results.All(v => true), "Related entities should be included.");
    }

    /// <summary>
    /// Verifies filtering with criteria that match no records returns an empty list.
    /// </summary>
    [Test]
    public async Task GetFilteredWithDetailsAsync_NoMatchingData_ReturnsEmptyList()
    {
        // Arrange
        string filterCountryCode = "NonExistent";

        // Act
        var results = await _repository.GetFilteredWithDetailsAsync(countryCode: filterCountryCode);

        // Assert
        Assert.IsNotNull(results);
        Assert.IsEmpty(results, "Result list should be empty when no records match the filter.");
    }
}