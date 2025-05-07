using Application.Interfaces;
using Microsoft.Extensions.Logging;
using VisualAmeco.Application.DTOs;
using VisualAmeco.Core.Interfaces;

namespace Application.Services;

public class LookupService : ILookupService
{
    private readonly ICountryRepository _countryRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVariableRepository _variableRepository;
    private readonly ILogger<LookupService> _logger;
    
    public LookupService(
        ICountryRepository countryRepository,
        IChapterRepository chapterRepository,
        IVariableRepository variableRepository,
        ILogger<LookupService> logger)
    {
        _countryRepository = countryRepository ?? throw new ArgumentNullException(nameof(countryRepository));
        _chapterRepository = chapterRepository ?? throw new ArgumentNullException(nameof(chapterRepository));
        _variableRepository = variableRepository ?? throw new ArgumentNullException(nameof(variableRepository));
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

    /// <summary>
    /// Retrieves a list of variables, optionally filtered by chapter and/or subchapter.
    /// </summary>
    public async Task<IEnumerable<VariableDto>> GetVariablesAsync(int? chapterId = null, int? subchapterId = null)
    {
        _logger.LogInformation("Fetching variables with filters: ChapterId={ChapterId}, SubchapterId={SubchapterId}",
            chapterId?.ToString() ?? "N/A",
            subchapterId?.ToString() ?? "N/A");
        try
        {
            var variables = await _variableRepository.GetFilteredAsync(chapterId, subchapterId);
            
            var variableDtos = variables
                .Select(v => new VariableDto
                {
                    Code = v.Code ?? "N/A",
                    Name = v.Name ?? "N/A",
                    Unit = v.Unit ?? "N/A",
                    SubchapterId = v.SubChapterId,
                    SubchapterName = v.SubChapter?.Name ?? "N/A"
                })
                .ToList();

            _logger.LogInformation("Returning {Count} variables.", variableDtos.Count);
            return variableDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An error occurred while fetching variables with filters: ChapterId={ChapterId}, SubchapterId={SubchapterId}",
                chapterId, subchapterId);
            throw;
        }
    }
}