namespace VisualAmeco.Application.DTOs;

/// <summary>
/// Data Transfer Object representing a Variable (indicator definition).
/// </summary>
public class VariableDto
{
    /// <summary>
    /// The unique code for the variable.
    /// </summary>
    /// <example>NPTD</example>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The descriptive name/title of the variable.
    /// </summary>
    /// <example>Total population (National accounts)</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The unit of measurement for the variable.
    /// </summary>
    /// <example>1000 persons</example>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the subchapter this variable belongs to.
    /// </summary>
    /// <example>11</example>
    public int SubchapterId { get; set; } // Assuming SubchapterId is int

    /// <summary>
    /// The name of the subchapter this variable belongs to.
    /// </summary>
    /// <example>01 Population</example>
    public string SubchapterName { get; set; } = string.Empty;
}