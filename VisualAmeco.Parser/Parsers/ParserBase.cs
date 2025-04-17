using Microsoft.Extensions.Logging;
using VisualAmeco.Core.Entities;

namespace VisualAmeco.Parser.Parsers;

public abstract class ParserBase
{
    protected readonly ILogger _logger;

    public ParserBase(ILogger logger)
    {
        _logger = logger;
    }

    public abstract Task<bool> ParseAndSaveAsync(string filePath);

    protected abstract Task<Chapter?> FindOrCreateChapterAsync(string chapterName);
    protected abstract Task<Subchapter?> FindOrCreateSubchapterAsync(string subchapterName, Chapter chapter);
    protected abstract Task<Variable?> FindOrCreateVariableAsync(string variableCode, string variableName, string? unit, Subchapter subchapter);
    protected abstract Task<Country?> FindOrCreateCountryAsync(string countryCode, string countryName);
    protected abstract Task<Value> CreateValueAsync(Variable variable, Country country, int year, int? month, decimal amount, bool isMonthly);
}