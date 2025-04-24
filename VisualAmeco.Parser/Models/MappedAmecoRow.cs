namespace VisualAmeco.Parser.Models;

public class MappedAmecoRow
{
    public string ChapterName { get; set; } = default!;
    public string SubchapterName { get; set; } = default!;
    
    public string VariableCode { get; set; } = default!;
    public string VariableName { get; set; } = default!;
    public string Unit { get; set; } = default!;
    
    public string CountryCode { get; set; } = default!;
    public string CountryName { get; set; } = default!;

    public List<YearValue> Values { get; set; } = new();
}

public class YearValue
{
    public int Year { get; set; }
    public decimal Amount { get; set; }
}