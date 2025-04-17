namespace VisualAmeco.Core.Entities;

public class Variable
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Unit { get; set; } = default!;
    
    public int SubChapterId { get; set; }
    public Subchapter SubChapter { get; set; } = default!;

    public ICollection<Value> Values { get; set; } = new List<Value>();
}