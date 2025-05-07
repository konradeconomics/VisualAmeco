namespace VisualAmeco.Application.DTOs;

/// <summary>
/// Data Transfer Object representing a Subchapter.
/// </summary>
public class SubchapterDto
{
    /// <summary>
    /// The unique identifier for the subchapter.
    /// </summary>
    /// <example>11</example>
    public int Id { get; set; } // Assuming integer ID

    /// <summary>
    /// The name of the subchapter.
    /// </summary>
    /// <example>01 Population</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the chapter this subchapter belongs to.
    /// </summary>
    /// <example>1</example>
    public int ChapterId { get; set; } // Include ChapterId for context/linking
}