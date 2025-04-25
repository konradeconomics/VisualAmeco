using Microsoft.Extensions.Logging;
using VisualAmeco.Application.DTOs;
using VisualAmeco.Application.Interfaces;
using VisualAmeco.Core.Interfaces;

namespace VisualAmeco.Application.Services;

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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of IndicatorDto objects.</returns>
    public async Task<IEnumerable<IndicatorDto>> GetIndicatorsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all indicators with details...");
        try
        {
            
            var allValuesWithDetails = await _valueRepository.GetAllWithDetailsAsync(cancellationToken);
            
            if (allValuesWithDetails == null || !allValuesWithDetails.Any())
            {
                _logger.LogInformation("No indicator values found in the repository.");
                return Enumerable.Empty<IndicatorDto>();
            }
            _logger.LogDebug("Retrieved {ValueCount} value records from repository.", allValuesWithDetails.Count);


            
            var indicatorDtos = allValuesWithDetails
                .Where(v => v.Variable != null && v.Country != null && v.Variable.SubChapter != null && v.Variable.SubChapter.Chapter != null)
                .GroupBy(v => new { v.VariableId, v.CountryId })
                .Select(group =>
                {
                    var firstValueInGroup = group.First();
                    var variable = firstValueInGroup.Variable!;
                    var country = firstValueInGroup.Country!;
                    var subchapter = variable.SubChapter!;
                    var chapter = subchapter.Chapter!;
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

            _logger.LogInformation("Mapped and returning {IndicatorCount} indicators.", indicatorDtos.Count);
            return indicatorDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching indicators.");
            throw;
        }
    }
}