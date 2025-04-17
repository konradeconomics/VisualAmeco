namespace VisualAmeco.Core.Entities;

public class Chapter
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    
    public ICollection<Subchapter> Subchapters { get; set; } = new List<Subchapter>();
}