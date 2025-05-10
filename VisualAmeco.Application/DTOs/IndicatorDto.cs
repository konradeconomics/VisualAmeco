namespace VisualAmeco.Application.DTOs;

/// <summary>
/// Data Transfer Object representing a specific AMECO indicator time series
/// for a particular country. Returned by the /api/indicators endpoint.
/// </summary>
public class IndicatorDto
{
    /// <summary>
    /// The unique AMECO variable code.
    /// </summary>
    /// <example>NPTD</example>
    public string VariableCode { get; set; } = string.Empty;

    /// <summary>
    /// The descriptive title or name of the variable.
    /// </summary>
    /// <example>Total population (National accounts)</example>
    public string VariableName { get; set; } = string.Empty;

    // public string Unit { get; set; } = string.Empty; // Old property
    /// <summary>
    /// The numeric code for the unit of measurement.
    /// </summary>
    /// <example>0</example>
    public string UnitCode { get; set; } = string.Empty;

    /// <summary>
    /// The descriptive string for the unit of measurement.
    /// </summary>
    /// <example>1000 persons</example>
    public string UnitDescription { get; set; } = string.Empty;

    /// <summary>
    /// The name/code of the subchapter the variable belongs to.
    /// </summary>
    /// <example>01 Population</example>
    public string SubchapterName { get; set; } = string.Empty;

    /// <summary>
    /// The name of the chapter the variable belongs to (derived during parsing).
    /// </summary>
    /// <example>Population And Employment</example>
    public string ChapterName { get; set; } = string.Empty;
    /// <summary>
    /// The country code (e.g., ISO or AMECO specific).
    /// </summary>
    /// <example>EU27</example>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// The full name of the country or region.
    /// </summary>
    /// <example>European Union</example>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// The list of yearly values associated with this specific indicator and country.
    /// This might be empty if no values exist or filtered.
    /// </summary>
    public List<YearValueDto> Values { get; set; } = new List<YearValueDto>(); // Initialize to prevent null reference
}