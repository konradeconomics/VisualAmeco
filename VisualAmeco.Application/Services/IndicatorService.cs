using Application.Interfaces;
using Microsoft.Extensions.Logging;
using VisualAmeco.Application.DTOs;
using VisualAmeco.Application.Interfaces;
using VisualAmeco.Core.Interfaces;

namespace Application.Services;

/// <summary>
/// Service responsible for retrieving and formatting AMECO indicator data.
/// </summary>
public class IndicatorService : IIndicatorService
{
    
    private readonly IValueRepository _valueRepository;
    private readonly ILogger<IndicatorService> _logger;

    
    public IndicatorService(IValueRepository valueRepository, ILogger<IndicatorService> logger)
    {
        _valueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Retrieves a collection of indicators, typically including related details.
    /// </summary>
    /// <param name="countryCode">Optional country code filter.</param>
    /// <param name="variableCode">Optional variable code filter.</param>
    /// <param name="chapterName">Optional chapter name filter.</param>
    /// <param name="subchapterName">Optional subchapter name filter.</param>
    /// <param name="years">Optional list of specific years to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of IndicatorDto objects.</returns>
    public async Task<IEnumerable<IndicatorDto>> GetIndicatorsAsync(
            string? countryCode = null,
            string? variableCode = null,
            string? chapterName = null,
            string? subchapterName = null,
            List<int>? years = null,
            CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching indicators with filters: Country={Country}, Variable={Variable}, Chapter={Chapter}, Subchapter={Subchapter}, Years={Years}",
            countryCode ?? "N/A", variableCode ?? "N/A", chapterName ?? "N/A", subchapterName ?? "N/A", years != null ? string.Join(",", years) : "N/A");

        try
        {
            // Call the repository method, passing filters through
            var filteredValuesWithDetails = await _valueRepository.GetFilteredWithDetailsAsync(
                countryCode,
                variableCode,
                chapterName,
                subchapterName,
                years, // Pass the list of years
                cancellationToken);

            if (filteredValuesWithDetails == null || !filteredValuesWithDetails.Any())
            {
                _logger.LogInformation("No indicator values found matching the specified filters.");
                return Enumerable.Empty<IndicatorDto>();
            }
            _logger.LogDebug("Retrieved {ValueCount} filtered value records from repository.", filteredValuesWithDetails.Count);

            // Grouping and Mapping logic
            var indicatorDtos = filteredValuesWithDetails
                // *** REINSTATED .Where() CLAUSE ***
                // Filter out records where essential navigation properties are null
                // BEFORE grouping and mapping to prevent NullReferenceExceptions.
                .Where(v => v.Variable != null &&
                            v.Country != null &&
                            v.Variable.SubChapter != null &&
                            v.Variable.SubChapter.Chapter != null)
                .GroupBy(v => new { v.VariableId, v.CountryId })
                .Select(group =>
                {
                    // Access related entities. Now safer due to the .Where() clause above.
                    var firstValueInGroup = group.First();
                    var variable = firstValueInGroup.Variable!; // Can use ! more confidently now
                    var country = firstValueInGroup.Country!;
                    var subchapter = variable.SubChapter!;
                    var chapter = subchapter.Chapter!;

                    // Handle potential nulls when accessing properties for the DTO (using ?. for extra safety)
                    return new IndicatorDto
                    {
                        VariableCode = variable.Code ?? "N/A",
                        VariableName = variable.Name ?? "N/A",
                        Unit = variable.Unit ?? "N/A",
                        SubchapterName = subchapter.Name ?? "N/A",
                        ChapterName = chapter.Name ?? "N/A",
                        CountryCode = country.Code ?? "N/A",
                        CountryName = country.Name ?? "N/A",
                        Values = group.Select(value => new YearValueDto
                            {
                                Year = value.Year,
                                Amount = value.Amount
                            })
                            .OrderBy(yv => yv.Year)
                            .ToList()
                    };
                })
                .OrderBy(dto => dto.ChapterName)
                .ThenBy(dto => dto.SubchapterName)
                .ThenBy(dto => dto.VariableCode)
                .ThenBy(dto => dto.CountryName)
                .ToList();

            _logger.LogInformation("Mapped and returning {IndicatorCount} filtered indicators.", indicatorDtos.Count);
            return indicatorDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching filtered indicators.");
            throw; // Re-throw
        }
    }
}