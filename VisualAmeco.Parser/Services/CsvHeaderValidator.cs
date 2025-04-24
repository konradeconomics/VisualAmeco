using VisualAmeco.Parser.Services.Interfaces;

namespace VisualAmeco.Parser.Services;

public class CsvHeaderValidator : ICsvHeaderValidator
{
    private static readonly string[] RequiredColumns = { "SERIES", "CNTRY", "TRN", "AGG", "UNIT", "REF", "CODE", "SUB-CHAPTER", "TITLE", "COUNTRY" };

    public bool TryValidate(string[] header, out Dictionary<string, int> columnIndices, out List<string> yearColumns)
    {
        columnIndices = header
            .Select((col, index) => new { col, index })
            .Where(x => !int.TryParse(x.col, out _))
            .ToDictionary(x => x.col, x => x.index);

        yearColumns = header
            .Skip(columnIndices.Count)
            .Where(col => int.TryParse(col, out _))
            .ToList();

        return RequiredColumns.All(columnIndices.ContainsKey);
    }
}