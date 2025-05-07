using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using VisualAmeco.Application.DTOs;

namespace VisualAmeco.API.Controllers;

/// <summary>
/// API endpoint for retrieving Variable (indicator definition) lookup data.
/// </summary>
[ApiController]
[Route("api/[controller]")] // Routes requests to /api/variables
public class VariablesController : ControllerBase
{
    private readonly ILookupService _lookupService;
    private readonly ILogger<VariablesController> _logger;

    public VariablesController(ILookupService lookupService, ILogger<VariablesController> logger)
    {
        _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a list of available variables, optionally filtered by chapter and/or subchapter.
    /// </summary>
    /// <param name="chapterId">Optional ID of the chapter to filter variables by.</param>
    /// <param name="subchapterId">Optional ID of the subchapter to filter variables by.</param>
    /// <returns>A list of variables.</returns>
    /// <response code="200">Returns the list of variables.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet] // Handles GET /api/variables
    [ProducesResponseType(typeof(IEnumerable<VariableDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<VariableDto>>> GetAllVariables(
        [FromQuery] int? chapterId = null,
        [FromQuery] int? subchapterId = null)
    {
        try
        {
            _logger.LogInformation(
                "GET /api/variables invoked with filters: ChapterId={ChapterId}, SubchapterId={SubchapterId}",
                chapterId, subchapterId);
            var variables = await _lookupService.GetVariablesAsync(chapterId, subchapterId);
            _logger.LogInformation("Returning {Count} variables.", variables.Count());
            return Ok(variables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while getting variables with filters: ChapterId={ChapterId}, SubchapterId={SubchapterId}",
                chapterId, subchapterId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An internal error occurred while retrieving variables.");
        }
    }
}