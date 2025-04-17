namespace VisualAmeco.Core.Entities;

public class Subchapter
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    
    public int ChapterId { get; set; }
    public Chapter Chapter { get; set; } = default!;
    
    public ICollection<Variable> Variables { get; set; } = new List<Variable>();
}