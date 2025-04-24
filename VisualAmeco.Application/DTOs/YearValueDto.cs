namespace VisualAmeco.Application.DTOs;

/// <summary>
/// Represents a single time-series value for a specific year.
/// Used as part of the IndicatorDto.
/// </summary>
public class YearValueDto
{
    /// <summary>
    /// The year of the data point.
    /// </summary>
    /// <example>2023</example>
    public int Year { get; set; }

    /// <summary>
    /// The numerical value/amount for the specified year.
    /// </summary>
    /// <example>12345.67</example>
    public decimal Amount { get; set; }
}