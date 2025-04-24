namespace VisualAmeco.Application.Interfaces;

public interface ICsvHeaderValidator
{
    bool TryValidate(string[] header, out Dictionary<string, int> columnIndices, out List<string> yearColumns);
}