namespace VisualAmeco.Core.Interfaces;

public interface ICsvFileReader
{
    public void SetFilePaths(List<string> filePaths);
    Task<IEnumerable<string[]>> ReadFileAsync();
}