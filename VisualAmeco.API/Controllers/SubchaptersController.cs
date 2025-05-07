using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using VisualAmeco.Application.DTOs;

namespace VisualAmeco.API.Controllers;

/// <summary>
/// API endpoint for retrieving Subchapter lookup data.
/// </summary>
[ApiController]
[Route("api/[controller]")] // Routes requests to /api/subchapters
public class SubchaptersController : ControllerBase
{
    private readonly ILookupService _lookupService;
    private readonly ILogger<SubchaptersController> _logger;

    public SubchaptersController(ILookupService lookupService, ILogger<SubchaptersController> logger)
    {
        _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a list of available subchapters, optionally filtered by chapter ID.
    /// </summary>
    /// <param name="chapterId">Optional ID of the chapter to filter subchapters by.</param>
    /// <returns>A list of subchapters.</returns>
    /// <response code="200">Returns the list of subchapters.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet] // Handles GET /api/subchapters and GET /api/subchapters?chapterId=X
    [ProducesResponseType(typeof(IEnumerable<SubchapterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SubchapterDto>>> GetAllSubchapters(
        [FromQuery] int? chapterId = null)
    {
        try
        {
            _logger.LogInformation("GET /api/subchapters invoked with filter: ChapterId={ChapterId}",
                chapterId?.ToString() ?? "N/A");
            var subchapters = await _lookupService.GetSubchaptersAsync(chapterId);
            _logger.LogInformation("Returning {Count} subchapters.", subchapters.Count());
            return Ok(subchapters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting subchapters with filter: ChapterId={ChapterId}",
                chapterId?.ToString() ?? "N/A");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An internal error occurred while retrieving subchapters.");
        }
    }
}