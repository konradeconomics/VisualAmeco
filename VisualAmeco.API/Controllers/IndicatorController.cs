using Microsoft.AspNetCore.Mvc;
using VisualAmeco.Application.DTOs;
using VisualAmeco.Application.Interfaces;

namespace VisualAmeco.API.Controllers;


/// <summary>
/// API endpoint for retrieving AMECO indicators.
/// </summary>
[ApiController]
[Route("api/[controller]")] // Routes requests to /api/indicators
public class IndicatorsController : ControllerBase
{
    private readonly IIndicatorService _indicatorService;
    private readonly ILogger<IndicatorsController> _logger;

    // Inject the IIndicatorService
    public IndicatorsController(IIndicatorService indicatorService, ILogger<IndicatorsController> logger)
    {
        _indicatorService = indicatorService ?? throw new ArgumentNullException(nameof(indicatorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    /// <summary>
    /// Gets all available AMECO indicators with their details and time series data.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of indicators in JSON format.</returns>
    /// <response code="200">Returns the list of indicators.</response>
    /// <response code="500">If an internal server error occurs during data retrieval.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<IndicatorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<IndicatorDto>>> GetIndicators(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /api/indicators invoked.");

            var indicators = await _indicatorService.GetIndicatorsAsync(cancellationToken);
                
            _logger.LogInformation("Returning {Count} indicators.", indicators.Count()); // Use Count() for IEnumerable
            return Ok(indicators);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting indicators.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred while retrieving indicators.");
        }
    }
}