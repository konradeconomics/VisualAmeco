

using VisualAmeco.Application.DTOs;

namespace VisualAmeco.Application.Interfaces;

/// <summary>
/// Defines the contract for a service that retrieves AMECO indicator data.
/// </summary>
public interface IIndicatorService
{
    /// <summary>
    /// Retrieves a collection of indicators, typically including variable details,
    /// country details, and time-series values.
    /// </summary>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>An asynchronous task returning an enumerable collection of IndicatorDto objects.</returns>
    Task<IEnumerable<IndicatorDto>> GetIndicatorsAsync(CancellationToken cancellationToken = default);

    // Future methods might include filtering/pagination:
    // Task<IEnumerable<IndicatorDto>> GetIndicatorsAsync(string? countryCode = null, string? variableCode = null, int? startYear = null, int? endYear = null, CancellationToken cancellationToken = default);
    // Task<IndicatorDto?> GetIndicatorAsync(string variableCode, string countryCode, CancellationToken cancellationToken = default);
}