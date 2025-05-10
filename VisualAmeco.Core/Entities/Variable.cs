using VisualAmeco.Core.Enums;

namespace VisualAmeco.Core.Entities;

public class Variable
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    
    public string UnitCode { get; set; } = string.Empty;
    
    public string UnitDescription { get; set; } = string.Empty;
    
    public TransformationType Trn { get; set; } // Transformation over time
    public AggregationMode Agg { get; set; } // Aggregation mode
    public ReferenceCode Ref { get; set; } // Relative performance reference code

    public int SubChapterId { get; set; }
    public Subchapter SubChapter { get; set; } = default!;

    public ICollection<Value> Values { get; set; } = new List<Value>();
}