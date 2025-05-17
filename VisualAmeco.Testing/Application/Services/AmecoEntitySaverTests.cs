using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VisualAmeco.Application.DTOs;
using VisualAmeco.Application.Services;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Interfaces;

namespace VisualAmeco.Testing.Application.Services;

[TestFixture]
public class AmecoEntitySaverTests
{
    // Mocks for all dependencies
    private Mock<IChapterRepository> _mockChapterRepo = null!;
    private Mock<ISubchapterRepository> _mockSubchapterRepo = null!;
    private Mock<IVariableRepository> _mockVariableRepo = null!;
    private Mock<ICountryRepository> _mockCountryRepo = null!;
    private Mock<IValueRepository> _mockValueRepo = null!;
    private Mock<IUnitOfWork> _mockUnitOfWork = null!;
    private Mock<ILogger<AmecoEntitySaver>> _mockLogger = null!;

    // Instance of the class under test
    private AmecoEntitySaver _entitySaver = null!;

    // Constants for test data
    private const string TestChapterName = "Test Chapter";
    private const string TestSubchapterName = "Test Subchapter";
    private const string TestVariableCode = "TESTCODE";
    private const string TestVariableName = "Test Variable Name";
    private const string TestUnit = "Test Unit";
    private const string TestUnitDescription = "Test Unit Description";
    private const string TestCountryCode = "TC";
    private const string TestCountryName = "Test Country";
    private const int TestYear2020 = 2020;
    private const decimal TestAmount2020 = 123.45m;
    private const int TestYear2021 = 2021;
    private const decimal TestAmount2021 = 678.90m;

    // Assumed Entity IDs
    private const int ChapterId = 1;
    private const int SubchapterId = 11;
    private const int VariableId = 111;
    private const int CountryId = 1111;


    [SetUp]
    public void Setup()
    {
        // Create new mock instances for each test
        _mockChapterRepo = new Mock<IChapterRepository>();
        _mockSubchapterRepo = new Mock<ISubchapterRepository>();
        _mockVariableRepo = new Mock<IVariableRepository>();
        _mockCountryRepo = new Mock<ICountryRepository>();
        _mockValueRepo = new Mock<IValueRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<AmecoEntitySaver>>();

        // Create the saver instance, injecting the mock objects
        _entitySaver = new AmecoEntitySaver(
            _mockChapterRepo.Object,
            _mockSubchapterRepo.Object,
            _mockVariableRepo.Object,
            _mockCountryRepo.Object,
            _mockValueRepo.Object,
            _mockUnitOfWork.Object
            , _mockLogger.Object
        );

        // Default: Assume AddAsync completes successfully for all repos
        _mockChapterRepo.Setup(r => r.AddAsync(It.IsAny<Chapter>())).Returns(Task.CompletedTask);
        _mockSubchapterRepo.Setup(r => r.AddAsync(It.IsAny<Subchapter>())).Returns(Task.CompletedTask);
        _mockVariableRepo.Setup(r => r.AddAsync(It.IsAny<Variable>())).Returns(Task.CompletedTask);
        _mockCountryRepo.Setup(r => r.AddAsync(It.IsAny<Country>())).Returns(Task.CompletedTask);
        _mockValueRepo.Setup(r => r.AddAsync(It.IsAny<Value>())).Returns(Task.CompletedTask);

        // Default: Assume SaveChangesAsync completes successfully
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .Returns(Task.FromResult(1));
    }

    // Helper to create consistent input data
    private MappedAmecoRow CreateSampleMappedRow()
    {
        return new MappedAmecoRow
        {
            ChapterName = TestChapterName,
            SubchapterName = TestSubchapterName,
            VariableCode = TestVariableCode,
            VariableName = TestVariableName,
            UnitCode = TestUnit,
            UnitDescription = TestUnitDescription,
            CountryCode = TestCountryCode,
            CountryName = TestCountryName,
            Values = new List<YearValue>
            {
                new YearValue { Year = TestYear2020, Amount = TestAmount2020 },
                new YearValue { Year = TestYear2021, Amount = TestAmount2021 }
            }
        };
    }
    
    /// <summary>
    /// Tests the scenario where all related entities (Chapter, Subchapter, Variable, Country)
    /// are new and need to be added.
    /// </summary>
    [Test]
    public async Task SaveAsync_AllEntitiesAreNew_AddsAllEntitiesAndSavesChanges()
    {
        // Arrange
        var mappedRow = CreateSampleMappedRow();
        Chapter? capturedChapter = null;
        Subchapter? capturedSubchapter = null;
        Variable? capturedVariable = null;
        Country? capturedCountry = null;


        // Setup Mocks: All "Get" methods return null (entity doesn't exist)
        _mockChapterRepo.Setup(r => r.GetByNameAsync(TestChapterName)).ReturnsAsync((Chapter?)null);
        // Simulate ID assignment using Callback
        _mockChapterRepo.Setup(r => r.AddAsync(It.IsAny<Chapter>()))
            .Callback<Chapter>(c => { c.Id = ChapterId; capturedChapter = c; }) // Capture and assign ID
            .Returns(Task.CompletedTask);

        _mockSubchapterRepo.Setup(r => r.GetByNameAsync(TestSubchapterName, It.IsAny<int>())).ReturnsAsync((Subchapter?)null); // Use It.IsAny for potentially 0 ID
        _mockSubchapterRepo.Setup(r => r.AddAsync(It.IsAny<Subchapter>()))
            .Callback<Subchapter>(s => { s.Id = SubchapterId; capturedSubchapter = s; })
            .Returns(Task.CompletedTask);

        _mockVariableRepo.Setup(r => r.GetByCodeAsync(TestVariableCode)).ReturnsAsync((Variable?)null);
        _mockVariableRepo.Setup(r => r.AddAsync(It.IsAny<Variable>()))
            .Callback<Variable>(v => { v.Id = VariableId; capturedVariable = v; })
            .Returns(Task.CompletedTask);

        _mockCountryRepo.Setup(r => r.GetByCodeAsync(TestCountryCode)).ReturnsAsync((Country?)null);
        _mockCountryRepo.Setup(r => r.AddAsync(It.IsAny<Country>()))
            .Callback<Country>(c => { c.Id = CountryId; capturedCountry = c; })
            .Returns(Task.CompletedTask);


        // Act
        await _entitySaver.SaveAsync(mappedRow);

        // Assert / Verify Mock Interactions

        // Verify Gets were called
        _mockChapterRepo.Verify(r => r.GetByNameAsync(TestChapterName), Times.Once);
        // Verify Subchapter Get was called
        _mockSubchapterRepo.Verify(r => r.GetByNameAsync(TestSubchapterName, It.IsAny<int>()), Times.Once);
        _mockVariableRepo.Verify(r => r.GetByCodeAsync(TestVariableCode), Times.Once);
        _mockCountryRepo.Verify(r => r.GetByCodeAsync(TestCountryCode), Times.Once);

        // Verify Adds were called with correct data
        _mockChapterRepo.Verify(r => r.AddAsync(It.Is<Chapter>(c => c.Name == TestChapterName)), Times.Once);
        _mockSubchapterRepo.Verify(r => r.AddAsync(It.Is<Subchapter>(s => s.Name == TestSubchapterName && s.Chapter == capturedChapter)), Times.Once);
        _mockVariableRepo.Verify(r => r.AddAsync(It.Is<Variable>(v => v.Code == TestVariableCode && v.Name == TestVariableName && v.UnitCode == TestUnit && v.UnitDescription == TestUnitDescription && v.SubChapter == capturedSubchapter)), Times.Once);
        _mockCountryRepo.Verify(r => r.AddAsync(It.Is<Country>(c => c.Code == TestCountryCode && c.Name == TestCountryName)), Times.Once);

        // Verify Value adds were called twice (once for each year) linking to the NEWLY created parents
        _mockValueRepo.Verify(r => r.AddAsync(It.Is<Value>(v =>
            v.Variable == capturedVariable && // Check navigation property link
            v.Country == capturedCountry &&   // Check navigation property link
            v.Year == TestYear2020 &&
            v.Amount == TestAmount2020)), Times.Once);
        _mockValueRepo.Verify(r => r.AddAsync(It.Is<Value>(v =>
            v.Variable == capturedVariable &&
            v.Country == capturedCountry &&
            v.Year == TestYear2021 &&
            v.Amount == TestAmount2021)), Times.Once);
        _mockValueRepo.Verify(r => r.AddAsync(It.IsAny<Value>()), Times.Exactly(mappedRow.Values.Count));


        // Verify SaveChangesAsync was called once at the end
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests the scenario where all related parent entities already exist in the database.
    /// </summary>
    [Test]
    public async Task SaveAsync_AllParentEntitiesExist_AddsOnlyValuesAndSavesChanges()
    {
        // Arrange
        var mappedRow = CreateSampleMappedRow();

        // Setup Mocks: All "Get" methods return existing entities
        var existingChapter = new Chapter { Id = ChapterId, Name = TestChapterName };
        var existingSubchapter = new Subchapter { Id = SubchapterId, Name = TestSubchapterName, ChapterId = ChapterId, Chapter = existingChapter }; // Include nav prop if needed by saver
        var existingVariable = new Variable { Id = VariableId, Code = TestVariableCode, Name = "Old Name", UnitCode = "0", UnitDescription = "Old Unit Description", SubChapterId = SubchapterId, SubChapter = existingSubchapter };
        var existingCountry = new Country { Id = CountryId, Code = TestCountryCode, Name = TestCountryName };

        _mockChapterRepo.Setup(r => r.GetByNameAsync(TestChapterName)).ReturnsAsync(existingChapter);
        _mockSubchapterRepo.Setup(r => r.GetByNameAsync(TestSubchapterName, ChapterId)).ReturnsAsync(existingSubchapter);
        _mockVariableRepo.Setup(r => r.GetByCodeAsync(TestVariableCode)).ReturnsAsync(existingVariable);
        _mockCountryRepo.Setup(r => r.GetByCodeAsync(TestCountryCode)).ReturnsAsync(existingCountry);

        // Act
        await _entitySaver.SaveAsync(mappedRow);

        // Assert / Verify Mock Interactions

        // Verify Gets were called
        _mockChapterRepo.Verify(r => r.GetByNameAsync(TestChapterName), Times.Once);
        _mockSubchapterRepo.Verify(r => r.GetByNameAsync(TestSubchapterName, ChapterId), Times.Once);
        _mockVariableRepo.Verify(r => r.GetByCodeAsync(TestVariableCode), Times.Once);
        _mockCountryRepo.Verify(r => r.GetByCodeAsync(TestCountryCode), Times.Once);

        _mockChapterRepo.Verify(r => r.AddAsync(It.IsAny<Chapter>()), Times.Never);
        _mockSubchapterRepo.Verify(r => r.AddAsync(It.IsAny<Subchapter>()), Times.Never);
        _mockVariableRepo.Verify(r => r.AddAsync(It.IsAny<Variable>()), Times.Never);
        _mockCountryRepo.Verify(r => r.AddAsync(It.IsAny<Country>()), Times.Never);


        // Verify Value adds were called twice with correct EXISTING parent FKs/Nav properties
        _mockValueRepo.Verify(r => r.AddAsync(It.Is<Value>(v =>
            v.Variable == existingVariable &&
            v.Country == existingCountry && 
            v.Year == TestYear2020 &&
            v.Amount == TestAmount2020)), Times.Once);
        _mockValueRepo.Verify(r => r.AddAsync(It.Is<Value>(v =>
            v.Variable == existingVariable &&
            v.Country == existingCountry &&
            v.Year == TestYear2021 &&
            v.Amount == TestAmount2021)), Times.Once);
        _mockValueRepo.Verify(r => r.AddAsync(It.IsAny<Value>()), Times.Exactly(mappedRow.Values.Count));

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that SaveChanges is called even if the input row has no year values to save.
    /// </summary>
    [Test]
    public async Task SaveAsync_RowWithNoValues_StillCallsGettersAndSavesChanges()
    {
        // Arrange
        var mappedRow = CreateSampleMappedRow();
        mappedRow.Values.Clear();

        // Setup as if entities are new
        _mockChapterRepo.Setup(r => r.GetByNameAsync(TestChapterName)).ReturnsAsync((Chapter?)null);
        _mockChapterRepo.Setup(r => r.AddAsync(It.IsAny<Chapter>())).Callback<Chapter>(c => c.Id = ChapterId).Returns(Task.CompletedTask);
        _mockSubchapterRepo.Setup(r => r.GetByNameAsync(TestSubchapterName, ChapterId)).ReturnsAsync((Subchapter?)null);
        _mockSubchapterRepo.Setup(r => r.AddAsync(It.IsAny<Subchapter>())).Callback<Subchapter>(s => s.Id = SubchapterId).Returns(Task.CompletedTask);
        _mockVariableRepo.Setup(r => r.GetByCodeAsync(TestVariableCode)).ReturnsAsync((Variable?)null);
        _mockVariableRepo.Setup(r => r.AddAsync(It.IsAny<Variable>())).Callback<Variable>(v => v.Id = VariableId).Returns(Task.CompletedTask);
        _mockCountryRepo.Setup(r => r.GetByCodeAsync(TestCountryCode)).ReturnsAsync((Country?)null);
        _mockCountryRepo.Setup(r => r.AddAsync(It.IsAny<Country>())).Callback<Country>(c => c.Id = CountryId).Returns(Task.CompletedTask);

        // Act
        await _entitySaver.SaveAsync(mappedRow);

        // Assert
        _mockChapterRepo.Verify(r => r.GetByNameAsync(TestChapterName), Times.Once);
        _mockChapterRepo.Verify(r => r.AddAsync(It.IsAny<Chapter>()), Times.Once);

        // Verify ValueRepo.AddAsync was NEVER called
        _mockValueRepo.Verify(r => r.AddAsync(It.IsAny<Value>()), Times.Never);

        // Verify SaveChangesAsync was still called once
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }


    /// <summary>
    /// Tests that if fetching a parent entity fails, the exception propagates
    /// and SaveChanges is not called.
    /// </summary>
    [Test]
    public void SaveAsync_RepoGetThrowsException_ExceptionPropagatesAndSaveChangesNotCalled()
    {
        // Arrange
        var mappedRow = CreateSampleMappedRow();
        var testException = new InvalidOperationException("Database connection failed");

        // Setup Mock: Chapter Get throws an exception
        _mockChapterRepo.Setup(r => r.GetByNameAsync(TestChapterName)).ThrowsAsync(testException);

        // Act & Assert
        // Assert that the call to SaveAsync throws the specific exception
        var thrownException = Assert.ThrowsAsync<InvalidOperationException>(async () => await _entitySaver.SaveAsync(mappedRow));
        Assert.AreEqual(testException.Message, thrownException.Message);

        // Verify SaveChangesAsync was NEVER called
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that if saving changes fails, the exception propagates.
    /// </summary>
    [Test]
    public void SaveAsync_UnitOfWorkThrowsException_ExceptionPropagates()
    {
        // Arrange
        var mappedRow = CreateSampleMappedRow();
        var testException = new TimeoutException("Save timed out");

        // Setup as if entities are new (Get returns null, Add sets Id)
        _mockChapterRepo.Setup(r => r.GetByNameAsync(TestChapterName)).ReturnsAsync((Chapter?)null);
        _mockChapterRepo.Setup(r => r.AddAsync(It.IsAny<Chapter>())).Callback<Chapter>(c => c.Id = ChapterId).Returns(Task.CompletedTask);
        _mockSubchapterRepo.Setup(r => r.GetByNameAsync(TestSubchapterName, ChapterId)).ReturnsAsync((Subchapter?)null);
        _mockSubchapterRepo.Setup(r => r.AddAsync(It.IsAny<Subchapter>())).Callback<Subchapter>(s => s.Id = SubchapterId).Returns(Task.CompletedTask);
        _mockVariableRepo.Setup(r => r.GetByCodeAsync(TestVariableCode)).ReturnsAsync((Variable?)null);
        _mockVariableRepo.Setup(r => r.AddAsync(It.IsAny<Variable>())).Callback<Variable>(v => v.Id = VariableId).Returns(Task.CompletedTask);
        _mockCountryRepo.Setup(r => r.GetByCodeAsync(TestCountryCode)).ReturnsAsync((Country?)null);
        _mockCountryRepo.Setup(r => r.AddAsync(It.IsAny<Country>())).Callback<Country>(c => c.Id = CountryId).Returns(Task.CompletedTask);
        _mockValueRepo.Setup(r => r.AddAsync(It.IsAny<Value>())).Returns(Task.CompletedTask);


        // Setup Mock: SaveChangesAsync throws an exception
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ThrowsAsync(testException);

        // Act & Assert
        var thrownException = Assert.ThrowsAsync<TimeoutException>(async () => await _entitySaver.SaveAsync(mappedRow));
        Assert.AreEqual(testException.Message, thrownException.Message);

        // Optionally verify that Add methods were called before the exception
        _mockValueRepo.Verify(r => r.AddAsync(It.IsAny<Value>()), Times.Exactly(mappedRow.Values.Count));
    }
}