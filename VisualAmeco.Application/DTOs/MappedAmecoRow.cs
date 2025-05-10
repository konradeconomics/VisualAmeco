namespace VisualAmeco.Application.DTOs;

public class MappedAmecoRow
{
    public string ChapterName { get; set; } = default!;
    public string SubchapterName { get; set; } = default!;
    
    public string VariableCode { get; set; } = default!;
    public string VariableName { get; set; } = default!;
    
    public string UnitCode { get; set; } = string.Empty;       // Renamed
    
    public string UnitDescription { get; set; } = string.Empty;
    
    public string CountryCode { get; set; } = default!;
    public string CountryName { get; set; } = default!;

    public List<YearValue> Values { get; set; } = new();
    
    public string? TRN { get; set; }
    public string? AGG { get; set; }
    public string? REF { get; set; }
}

public class YearValue
{
    public int Year { get; set; }
    public decimal Amount { get; set; }
}