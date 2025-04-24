using NUnit.Framework;
using VisualAmeco.Parser.Services;

namespace VisualAmeco.Testing.Parser.Services;

[TestFixture]
public class CsvHeaderValidatorTests
{
    private CsvHeaderValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CsvHeaderValidator();
    }

    /// <summary>
    /// Tests validation with a header that contains all required columns,
    /// some extra non-year columns, and some year columns. Expects success
    /// and correct population of indices and year lists.
    /// </summary>
    [Test]
    public void TryValidate_ValidHeader_ReturnsTrueAndPopulatesOutputs()
    {
        // Arrange
        var header = new[] { "SERIES", "CNTRY", "TRN", "AGG", "UNIT", "REF", "CODE", "SUB-CHAPTER", "TITLE", "COUNTRY", "EXTRA_COL", "1999", "2000", "2001" };
        var expectedIndices = new Dictionary<string, int>
        {
            { "SERIES", 0 }, { "CNTRY", 1 }, { "TRN", 2 }, { "AGG", 3 }, { "UNIT", 4 },
            { "REF", 5 }, { "CODE", 6 }, { "SUB-CHAPTER", 7 }, { "TITLE", 8 }, { "COUNTRY", 9 },
            { "EXTRA_COL", 10 }
        };
        var expectedYears = new List<string> { "1999", "2000", "2001" };

        // Act
        var isValid = _validator.TryValidate(header, out var actualIndices, out var actualYears);

        // Assert
        Assert.IsTrue(isValid, "Validation should pass for a valid header.");
        // Use CollectionAssert.AreEquivalent for dictionaries as item order doesn't matter.
        CollectionAssert.AreEquivalent(expectedIndices, actualIndices, "Column indices dictionary should contain expected non-year columns and indices.");
        // Use CollectionAssert.AreEqual for lists where order matters.
        CollectionAssert.AreEqual(expectedYears, actualYears, "Year columns list should contain expected years in order.");
    }

    /// <summary>
    /// Tests validation with a header missing one of the required columns ("CODE").
    /// Expects validation to fail (return false).
    /// </summary>
    [Test]
    public void TryValidate_MissingRequiredColumn_ReturnsFalse()
    {
        // Arrange
        // Missing "CODE"
        var header = new[] { "SERIES", "CNTRY", "TRN", "AGG", "UNIT", "REF", /*"CODE",*/ "SUB-CHAPTER", "TITLE", "COUNTRY", "1999", "2000" };

        // Act
        var isValid = _validator.TryValidate(header, out var actualIndices, out var actualYears);

        // Assert
        Assert.IsFalse(isValid, "Validation should fail when a required column is missing.");
        // Optional: Verify outputs are still populated with what *was* found
        Assert.IsNotNull(actualIndices);
        Assert.IsNotNull(actualYears);
        Assert.IsFalse(actualIndices.ContainsKey("CODE"), "Missing required column 'CODE' should not be in indices.");
    }

    /// <summary>
    /// Tests validation with a header containing all required columns but no year columns.
    /// Expects validation to pass (return true) and the year list to be empty.
    /// </summary>
    [Test]
    public void TryValidate_NoYearColumns_ReturnsTrueAndEmptyYearList()
    {
        // Arrange
        var header = new[] { "SERIES", "CNTRY", "TRN", "AGG", "UNIT", "REF", "CODE", "SUB-CHAPTER", "TITLE", "COUNTRY", "NOTE" };
        var expectedIndicesCount = 11; // All columns are non-numeric

        // Act
        var isValid = _validator.TryValidate(header, out var actualIndices, out var actualYears);

        // Assert
        Assert.IsTrue(isValid, "Validation should pass if all required columns are present, even with no year columns.");
        Assert.AreEqual(expectedIndicesCount, actualIndices.Count, "All columns should be in indices.");
        Assert.IsNotNull(actualYears, "Year columns list should not be null.");
        Assert.IsEmpty(actualYears, "Year columns list should be empty.");
    }

    /// <summary>
    /// Tests validation with an empty header array. Expects validation to fail.
    /// </summary>
    [Test]
    public void TryValidate_EmptyHeader_ReturnsFalse()
    {
        // Arrange
        var header = System.Array.Empty<string>();

        // Act
        var isValid = _validator.TryValidate(header, out var actualIndices, out var actualYears);

        // Assert
        Assert.IsFalse(isValid, "Validation should fail for an empty header.");
        Assert.IsEmpty(actualIndices, "Indices should be empty.");
        Assert.IsEmpty(actualYears, "Years should be empty.");
    }

    /// <summary>
    /// Tests that passing a null header array throws an ArgumentNullException
    /// due to the underlying LINQ operations.
    /// </summary>
    [Test]
    public void TryValidate_NullHeader_ThrowsArgumentNullException()
    {
        // Arrange
        string[]? header = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => _validator.TryValidate(header!, out _, out _));
    }

    /// <summary>
    /// Tests validation when a required column exists but with different casing ("code" vs "CODE").
    /// Expects validation to fail because the default dictionary key comparer is case-sensitive.
    /// </summary>
    [Test]
    public void TryValidate_CaseMismatchInRequiredColumn_ReturnsFalse()
    {
        // Arrange
        var header = new[] { "SERIES", "CNTRY", "TRN", "AGG", "UNIT", "REF", "code", "SUB-CHAPTER", "TITLE", "COUNTRY", "1999", "2000" };

        // Act
        var isValid = _validator.TryValidate(header, out var actualIndices, out var actualYears);

        // Assert
        Assert.IsFalse(isValid, "Validation should fail if required column casing doesn't match (default comparer).");
        // Verify that the lowercase version was added, but the required uppercase one wasn't found
        Assert.IsTrue(actualIndices.ContainsKey("code"), "Index dictionary should contain the lowercase 'code'.");
        Assert.IsFalse(actualIndices.ContainsKey("CODE"), "Index dictionary should NOT contain the uppercase 'CODE'.");
    }

    /// <summary>
    /// Tests the logic assumption that years only appear after non-numeric columns.
    /// Uses a header where numeric/year columns are interspersed with non-numeric ones.
    /// Expects validation to pass (if all required are present) and years to be extracted
    /// only from the part of the header *after* the calculated non-numeric count.
    /// </summary>
    [Test]
    public void TryValidate_InterspersedColumns_ExtractsYearsCorrectlyAndValidates()
    {
        // Arrange: Header with years before the last non-year columns
        // All required columns ARE present in the non-numeric set.
        var header = new[] { "SERIES", "1999", "CNTRY", "2000", "TRN", "AGG", "UNIT", "REF", "CODE", "SUB-CHAPTER", "TITLE", "COUNTRY", "2001", "NOTE", "2002"};
        // Validator finds these non-numeric columns and their indices:
        var expectedIndices = new Dictionary<string, int>
        {
            { "SERIES", 0 }, { "CNTRY", 2 }, { "TRN", 4 }, { "AGG", 5 }, { "UNIT", 6 }, { "REF", 7 },
            { "CODE", 8 }, { "SUB-CHAPTER", 9 }, { "TITLE", 10 }, { "COUNTRY", 11 }, {"NOTE", 13} // 11 entries
        };
        // It checks these remaining items for parsable integers.
        var expectedYears = new List<string> { "2001", "2002" };

        // Act
        var isValid = _validator.TryValidate(header, out var actualIndices, out var actualYears);

        // Assert
        Assert.IsTrue(isValid, "Validation should pass as all required columns were found among non-numeric ones.");
        CollectionAssert.AreEquivalent(expectedIndices, actualIndices, "Indices should include all non-numeric columns regardless of position.");
        CollectionAssert.AreEqual(expectedYears, actualYears, "Years should only include numeric columns found *after* the count of identified non-numeric columns.");
    }
}