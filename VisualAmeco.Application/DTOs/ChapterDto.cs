namespace VisualAmeco.Application.DTOs;

/// <summary>
/// Data Transfer Object representing a Chapter.
/// </summary>
public class ChapterDto
{
    /// <summary>
    /// The unique identifier for the chapter.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; } // Assuming integer ID

    /// <summary>
    /// The name of the chapter.
    /// </summary>
    /// <example>Population And Employment</example>
    public string Name { get; set; } = string.Empty;
}