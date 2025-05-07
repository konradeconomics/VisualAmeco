using Application.Interfaces;
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
    /// Gets AMECO indicators, optionally filtered by specified criteria.
    /// </summary>
    /// <param name="countryCode">Optional country code (e.g., DE, EU27) to filter by.</param>
    /// <param name="variableCode">Optional variable code (e.g., NPTD) to filter by.</param>
    /// <param name="chapterName">Optional chapter name to filter by.</param>
    /// <param name="subchapterName">Optional subchapter name to filter by.</param>
    /// <param name="years">Optional list of specific years to include.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of matching indicators in JSON format.</returns>
    /// <response code="200">Returns the list of (filtered) indicators.</response>
    /// <response code="500">If an internal server error occurs during data retrieval.</response>
    [HttpGet] // Handles GET requests to /api/indicators
    [ProducesResponseType(typeof(IEnumerable<IndicatorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // *** UPDATED Method Signature to accept filters from query string ***
    public async Task<ActionResult<IEnumerable<IndicatorDto>>> GetIndicators(
        [FromQuery] string? countryCode = null,
        [FromQuery] string? variableCode = null,
        [FromQuery] string? chapterName = null,
        [FromQuery] string? subchapterName = null,
        [FromQuery] List<int>? years = null, // Binds from ?years=2020&years=2021 etc.
        CancellationToken cancellationToken = default) // Keep CancellationToken last
    {
        try
        {
            // Log received filters
            _logger.LogInformation("GET /api/indicators invoked with filters: Country={Country}, Variable={Variable}, Chapter={Chapter}, Subchapter={Subchapter}, Years={Years}",
                countryCode ?? "N/A", variableCode ?? "N/A", chapterName ?? "N/A", subchapterName ?? "N/A", years != null ? string.Join(",", years) : "N/A");

            // 1. Call the application service, passing the filter parameters
            var indicators = await _indicatorService.GetIndicatorsAsync(
                countryCode,
                variableCode,
                chapterName,
                subchapterName,
                years, // Pass the list of years
                cancellationToken);

            // 2. Return the data
            _logger.LogInformation("Returning {Count} indicators based on filters.", indicators.Count());
            return Ok(indicators);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting filtered indicators.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred while retrieving indicators.");
        }
    }
    
    /// <summary>
    /// Gets a specific AMECO indicator time series by variable and country codes.
    /// </summary>
    /// <param name="variableCode">The unique variable code (e.g., NPTD).</param>
    /// <param name="countryCode">The unique country code (e.g., DE, EU27).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The requested indicator or a Not Found response.</returns>
    /// <response code="200">Returns the specific indicator data.</response>
    /// <response code="404">If the indicator for the specified variable/country code combination is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("{variableCode}/{countryCode}")] // Handles GET /api/indicators/CODE/COUNTRY
    [ProducesResponseType(typeof(IndicatorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IndicatorDto>> GetSpecificIndicator(
        string variableCode,
        string countryCode,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /api/indicators/{VariableCode}/{CountryCode} invoked.", variableCode, countryCode);

            var indicator = await _indicatorService.GetSpecificIndicatorAsync(variableCode, countryCode, cancellationToken);

            if (indicator == null)
            {
                _logger.LogInformation("Indicator not found for Variable={VariableCode}, Country={CountryCode}.", variableCode, countryCode);
                return NotFound();
            }

            _logger.LogInformation("Returning specific indicator for Variable={VariableCode}, Country={CountryCode}.", variableCode, countryCode);
            return Ok(indicator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting specific indicator for Variable={VariableCode}, Country={CountryCode}.", variableCode, countryCode);
            return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred while retrieving the indicator.");
        }
    }
}