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

    /// <summary>
    /// Sets up a unique SQLite database and seeds it with test data
    /// before each test runs.
    /// </summary>
    [SetUp]
    public async Task SetupDatabase()
    {
        
        _dbContextOptions = new DbContextOptionsBuilder<VisualAmecoDbContext>()
            .UseSqlite($"Filename={TestContext.CurrentContext.Test.Name}.db") // Unique DB file per test
            .Options;

        // Create context instance
        _context = new VisualAmecoDbContext(_dbContextOptions);
        
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        // --- Seed Data ---
        await SeedDatabaseAsync();

        _repository = new ValueRepository(_context);

        TestContext.WriteLine($"Database seeded and repository created for test: {TestContext.CurrentContext.Test.Name}");
    }

    /// <summary>
    /// Helper method to seed the database with related entities.
    /// </summary>
    private async Task SeedDatabaseAsync()
    {
        var chapter1 = new Chapter { Id = SeedChapterId, Name = "Seed Chapter" };
        var subchapter1 = new Subchapter { Id = SeedSubchapterId, Name = "Seed Subchapter", ChapterId = SeedChapterId /*, Chapter = chapter1 */ }; // Link via FK
        var variable1 = new Variable { Id = SeedVariableId1, Code = "VAR1", Name = "Seed Var 1", Unit = "U1", SubChapterId = SeedSubchapterId /*, Subchapter = subchapter1 */ }; // Link via FK
        var variable2 = new Variable { Id = SeedVariableId2, Code = "VAR2", Name = "Seed Var 2", Unit = "U2", SubChapterId = SeedSubchapterId /*, Subchapter = subchapter1 */ }; // Link via FK
        var country1 = new Country { Id = SeedCountryId1, Code = "C1", Name = "Seed Country 1" };
        var country2 = new Country { Id = SeedCountryId2, Code = "C2", Name = "Seed Country 2" };
        
        _context.Chapters.Add(chapter1);
        _context.Subchapters.Add(subchapter1);
        _context.Variables.AddRange(variable1, variable2);
        _context.Countries.AddRange(country1, country2);
        await _context.SaveChangesAsync();

        _context.Values.AddRange(
            new Value { VariableId = SeedVariableId1, CountryId = SeedCountryId1, Year = 2020, Amount = 100m },
            new Value { VariableId = SeedVariableId1, CountryId = SeedCountryId1, Year = 2021, Amount = 110m },
            new Value { VariableId = SeedVariableId2, CountryId = SeedCountryId1, Year = 2020, Amount = 200m },
            new Value { VariableId = SeedVariableId1, CountryId = SeedCountryId2, Year = 2020, Amount = 150m }
        );

        await _context.SaveChangesAsync();
    }


    /// <summary>
    /// Disposes the DbContext after each test to release the database connection/file lock.
    /// </summary>
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
        catch(Exception ex)
        {
            TestContext.WriteLine($"Warning: Could not delete test database file '{TestContext.CurrentContext.Test.Name}.db'. Error: {ex.Message}");
        }
    }


    /// <summary>
    /// Verifies that GetAllWithDetailsAsync retrieves all Value records
    /// and correctly includes (eagerly loads) the related Variable, Country,
    /// Subchapter, and Chapter entities from the test database.
    /// </summary>
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
        
        // This confirms the Include/ThenInclude worked against the test DB.
        foreach (var value in results)
        {
            Assert.IsNotNull(value.Variable, $"Value ID {value.Id} should have Variable loaded.");
            Assert.IsNotNull(value.Country, $"Value ID {value.Id} should have Country loaded.");
            Assert.IsNotNull(value.Variable?.SubChapter, $"Value ID {value.Id}'s Variable should have Subchapter loaded.");
            Assert.IsNotNull(value.Variable?.SubChapter?.Chapter, $"Value ID {value.Id}'s Subchapter should have Chapter loaded.");

            if (value.VariableId == SeedVariableId1 && value.CountryId == SeedCountryId1 && value.Year == 2020)
            {
                Assert.AreEqual("VAR1", value.Variable.Code, "Variable code mismatch.");
                Assert.AreEqual("C1", value.Country.Code, "Country code mismatch.");
                Assert.AreEqual("Seed Subchapter", value.Variable.SubChapter.Name, "Subchapter name mismatch.");
                Assert.AreEqual("Seed Chapter", value.Variable.SubChapter.Chapter.Name, "Chapter name mismatch.");
                Assert.AreEqual(100m, value.Amount, "Amount mismatch.");
            }
        }

        var distinctVariableIds = results.Select(v => v.VariableId).Distinct().ToList();
        var distinctCountryIds = results.Select(v => v.CountryId).Distinct().ToList();
        Assert.AreEqual(2, distinctVariableIds.Count, "Should represent 2 distinct Variables.");
        Assert.AreEqual(2, distinctCountryIds.Count, "Should represent 2 distinct Countries.");
    }
    /// <summary>
    /// Verifies that GetAllWithDetailsAsync returns an empty list when no Value records exist in the test database.
    /// </summary>
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
        Assert.IsNotNull(results, "Result list should not be null.");
        Assert.IsEmpty(results, "Result list should be empty when database has no Value records.");
    }
}