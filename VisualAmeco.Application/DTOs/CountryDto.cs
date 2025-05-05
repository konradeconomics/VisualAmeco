namespace VisualAmeco.Application.DTOs;

/// <summary>
/// Data Transfer Object representing a Country.
/// </summary>
public class CountryDto
{
    /// <summary>
    /// The unique code for the country (e.g., DE, EU27).
    /// </summary>
    /// <example>DE</example>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The full name of the country or region.
    /// </summary>
    /// <example>Germany</example>
    public string Name { get; set; } = string.Empty;
}