

using VisualAmeco.Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Defines the contract for a service that retrieves AMECO indicator data.
/// </summary>
public interface IIndicatorService
{
    /// <summary>
    /// Retrieves a collection of indicators based on optional filters.
    /// </summary>
    /// <param name="countryCode">Optional country code filter.</param>
    /// <param name="variableCode">Optional variable code filter.</param>
    /// <param name="chapterName">Optional chapter name filter.</param>
    /// <param name="subchapterName">Optional subchapter name filter.</param>
    /// <param name="years">Optional list of specific years to include.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>An asynchronous task returning an enumerable collection of IndicatorDto objects.</returns>
    Task<IEnumerable<IndicatorDto>> GetIndicatorsAsync(
        string? countryCode = null,
        string? variableCode = null,
        string? chapterName = null,
        string? subchapterName = null,
        List<int>? years = null,
        CancellationToken cancellationToken = default);
}