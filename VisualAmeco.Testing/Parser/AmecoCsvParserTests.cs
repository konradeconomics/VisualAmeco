using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnitAssert = NUnit.Framework.Assert;
using VisualAmeco.Data.Contexts;
using VisualAmeco.Data.Repositories;
using VisualAmeco.Parser.Parsers;

namespace VisualAmeco.Testing.Parser;

[TestFixture]
public class AmecoCsvParserTests
{
    private VisualAmecoDbContext _context;
    private AmecoCsvParser _parser;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<VisualAmecoDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        _context = new VisualAmecoDbContext(options);

        var csvData = new List<string[]>
        {
            new[] { "SERIES", "CNTRY", "TRN", "AGG", "UNIT", "REF", "CODE", "SUB-CHAPTER", "TITLE", "COUNTRY", "2020" },
            new[] { "1", "DE", "T1", "A", "EUR", "R1", "GDP", "MACRO", "Gross Domestic Product", "Germany", "123.45" }
        };

        var csvReaderMock = new Mock<CsvFileReader>();
        csvReaderMock.Setup(r => r.ReadFileAsync()).ReturnsAsync(csvData);

        _parser = new AmecoCsvParser(
            Mock.Of<ILogger<AmecoCsvParser>>(),
            csvReaderMock.Object,
            new ChapterRepository(_context),
            new SubchapterRepository(_context),
            new VariableRepository(_context),
            new CountryRepository(_context),
            new ValueRepository(_context),
            new UnitOfWork(_context)
        );
    }

    [Test]
    public async Task ParseAndSaveAsync_ValidCsv_AddsEntities()
    {
        // Arrange
        var filePaths = new List<string> { "TestCSVs/test_ameco_valid_1row.csv" };

        // Act
        var result = await _parser.ParseAndSaveAsync(filePaths);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, await _context.Chapters.CountAsync());
        Assert.AreEqual(1, await _context.Subchapters.CountAsync());
        Assert.AreEqual(1, await _context.Variables.CountAsync());
        Assert.AreEqual(1, await _context.Countries.CountAsync());
        Assert.AreEqual(1, await _context.Values.CountAsync());
    }

    [TearDown]
    public void Cleanup()
    {
        _context.Dispose();
    }
}