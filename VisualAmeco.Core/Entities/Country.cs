namespace VisualAmeco.Core.Entities;

public class Country
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    
    public ICollection<Value> Values { get; set; } = new List<Value>();
}