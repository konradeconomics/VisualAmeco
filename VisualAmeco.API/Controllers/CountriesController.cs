using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using VisualAmeco.Application.DTOs;

namespace VisualAmeco.API.Controllers;

/// <summary>
/// API endpoint for retrieving Country lookup data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CountriesController : ControllerBase
{
    private readonly ILookupService _lookupService;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(ILookupService lookupService, ILogger<CountriesController> logger)
    {
        _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a list of all available countries.
    /// </summary>
    /// <returns>A list of countries.</returns>
    /// <response code="200">Returns the list of countries.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet] // Handles GET /api/countries
    [ProducesResponseType(typeof(IEnumerable<CountryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CountryDto>>> GetAllCountries()
    {
        try
        {
            _logger.LogInformation("GET /api/countries invoked.");
            var countries = await _lookupService.GetAllCountriesAsync();
            _logger.LogInformation("Returning {Count} countries.", countries.Count());
            return Ok(countries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all countries.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred while retrieving countries.");
        }
    }
}