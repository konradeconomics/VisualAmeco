using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using VisualAmeco.Application.DTOs;

namespace VisualAmeco.API.Controllers;

/// <summary>
/// API endpoint for retrieving Chapter lookup data.
/// </summary>
[ApiController]
[Route("api/[controller]")] // Routes requests to /api/chapters
public class ChaptersController : ControllerBase
{
    private readonly ILookupService _lookupService;
    private readonly ILogger<ChaptersController> _logger;

    public ChaptersController(ILookupService lookupService, ILogger<ChaptersController> logger)
    {
        _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a list of all available chapters.
    /// </summary>
    /// <returns>A list of chapters.</returns>
    /// <response code="200">Returns the list of chapters.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet] // Handles GET /api/chapters
    [ProducesResponseType(typeof(IEnumerable<ChapterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ChapterDto>>> GetAllChapters()
    {
        try
        {
            _logger.LogInformation("GET /api/chapters invoked.");
            var chapters = await _lookupService.GetAllChaptersAsync();
            _logger.LogInformation("Returning {Count} chapters.", chapters.Count());
            return Ok(chapters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all chapters.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An internal error occurred while retrieving chapters.");
        }
    }
}