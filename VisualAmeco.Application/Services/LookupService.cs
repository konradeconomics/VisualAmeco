using Application.Interfaces;
using Microsoft.Extensions.Logging;
using VisualAmeco.Application.DTOs;
using VisualAmeco.Core.Interfaces;

namespace Application.Services;

public class LookupService : ILookupService
{
    private readonly ICountryRepository _countryRepository;
    private readonly ILogger<LookupService> _logger;
    
    public LookupService(
        ICountryRepository countryRepository,
        ILogger<LookupService> logger)
    {
        _countryRepository = countryRepository ?? throw new ArgumentNullException(nameof(countryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Retrieves a list of all available countries.
    /// </summary>
    public async Task<IEnumerable<CountryDto>> GetAllCountriesAsync()
    {
        _logger.LogInformation("Fetching all countries.");
        try
        {
            var countries = await _countryRepository.GetAllAsync();

            var countryDtos = countries
                .Select(c => new CountryDto
                {
                    Code = c.Code ?? "N/A",
                    Name = c.Name ?? "N/A"
                })
                .ToList();

            _logger.LogInformation("Returning {Count} countries.", countryDtos.Count);
            return countryDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching all countries.");
            throw;
        }
    }
}