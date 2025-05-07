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
            var filteredValuesWithDetails = await _valueRepository.GetFilteredWithDetailsAsync(
                countryCode,
                variableCode,
                chapterName,
                subchapterName,
                years,
                cancellationToken);

            if (filteredValuesWithDetails == null || !filteredValuesWithDetails.Any())
            {
                _logger.LogInformation("No indicator values found matching the specified filters.");
                return Enumerable.Empty<IndicatorDto>();
            }
            _logger.LogDebug("Retrieved {ValueCount} filtered value records from repository.", filteredValuesWithDetails.Count);

            var indicatorDtos = filteredValuesWithDetails
                .Where(v => v.Variable != null &&
                            v.Country != null &&
                            v.Variable.SubChapter != null &&
                            v.Variable.SubChapter.Chapter != null)
                .Where(v => true)
                .GroupBy(v => new { v.VariableId, v.CountryId })
                .Select(group =>
                {
                    var firstValueInGroup = group.First();
                    var variable = firstValueInGroup.Variable!; // Can use ! more confidently now
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

            _logger.LogInformation("Mapped and returning {IndicatorCount} filtered indicators.", indicatorDtos.Count);
            return indicatorDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching filtered indicators.");
            throw;
        }
    }
    
    /// <summary>
    /// Retrieves a single specific indicator time series by variable code and country code.
    /// </summary>
    public async Task<IndicatorDto?> GetSpecificIndicatorAsync(
        string variableCode,
        string countryCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching specific indicator: Variable={VariableCode}, Country={CountryCode}", variableCode, countryCode);

        var filteredValues = await _valueRepository.GetFilteredWithDetailsAsync(
            countryCode: countryCode,
            variableCode: variableCode,
            cancellationToken: cancellationToken);

        if (filteredValues == null)
        {
            _logger.LogInformation("Specific indicator repository query returned null: Variable={VariableCode}, Country={CountryCode}", variableCode, countryCode);
            return null;
        }
        if (!filteredValues.Any())
        {
            _logger.LogInformation("Specific indicator not found (empty list): Variable={VariableCode}, Country={CountryCode}", variableCode, countryCode);
            return null;
        }

        var firstValue = filteredValues.First();
        var variable = firstValue.Variable;
        if (variable == null)
        {
            _logger.LogWarning("Data integrity issue: Missing Variable entity for VariableCode={VariableCode}, CountryCode={CountryCode}", variableCode, countryCode);
            return null;
        }

        var country = firstValue.Country;
        if (country == null)
        {
            _logger.LogWarning("Data integrity issue: Missing Country entity for VariableCode={VariableCode}, CountryCode={CountryCode}", variableCode, countryCode);
            return null;
        }

        var subchapter = variable.SubChapter;
        if (subchapter == null)
        {
            _logger.LogWarning("Data integrity issue: Missing Subchapter entity for VariableCode={VariableCode}, CountryCode={CountryCode}", variableCode, countryCode);
            return null;
        }

        var chapter = subchapter.Chapter;
        if (chapter == null)
        {
            _logger.LogWarning("Data integrity issue: Missing Chapter entity for VariableCode={VariableCode}, CountryCode={CountryCode}", variableCode, countryCode);
            return null;
        }

        var indicatorDto = new IndicatorDto
        {
            VariableCode = variable.Code ?? "N/A",
            VariableName = variable.Name ?? "N/A",
            Unit = variable.Unit ?? "N/A",
            SubchapterName = subchapter.Name ?? "N/A",
            ChapterName = chapter.Name ?? "N/A",
            CountryCode = country.Code ?? "N/A",
            CountryName = country.Name ?? "N/A",
            Values = filteredValues.Select(value => new YearValueDto
                {
                    Year = value.Year,
                    Amount = value.Amount
                })
                .OrderBy(yv => yv.Year)
                .ToList()    
        };

        _logger.LogInformation("Specific indicator found and mapped: Variable={VariableCode}, Country={CountryCode}", variableCode, countryCode);
        return indicatorDto;
    }
}