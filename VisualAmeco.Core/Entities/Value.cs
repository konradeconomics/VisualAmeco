namespace VisualAmeco.Core.Entities;

public class Value
{
    public int Id { get; set; }
    
    public int VariableId { get; set; }
    public Variable Variable { get; set; } = default!;
    
    public int CountryId { get; set; }
    public Country Country { get; set; } = default!;
    
    public int Year { get; set; }
    public int? Month { get; set; }
    
    public decimal Amount { get; set; }
    
    public bool IsMonthly { get; set; }
}