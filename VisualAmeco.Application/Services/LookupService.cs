using Application.Interfaces;
using Microsoft.Extensions.Logging;
using VisualAmeco.Application.DTOs;
using VisualAmeco.Core.Interfaces;

namespace Application.Services;

public class LookupService : ILookupService
{
    private readonly ICountryRepository _countryRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly ILogger<LookupService> _logger;
    
    public LookupService(
        ICountryRepository countryRepository,
        IChapterRepository chapterRepository,
        ILogger<LookupService> logger)
    {
        _countryRepository = countryRepository ?? throw new ArgumentNullException(nameof(countryRepository));
        _chapterRepository = chapterRepository ?? throw new ArgumentNullException(nameof(chapterRepository));
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

    /// <summary>
    /// Retrieves a list of all available chapters.
    /// </summary>
    public async Task<IEnumerable<ChapterDto>> GetAllChaptersAsync()
    {
        _logger.LogInformation("Fetching all chapters.");
        try
        {
            var chapters = await _chapterRepository.GetAllAsync();

            var chapterDtos = chapters
                .Select(c => new ChapterDto
                {
                    Id = c.Id,
                    Name = c.Name ?? "N/A"
                })
                .OrderBy(c => c.Id)
                .ToList();

            _logger.LogInformation("Returning {Count} chapters.", chapterDtos.Count);
            return chapterDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching all chapters.");
            throw;
        }
    }
}