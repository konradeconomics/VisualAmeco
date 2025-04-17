namespace VisualAmeco.Parser;

public abstract class FileReader
{
    protected string FilePath { get; }

    public FileReader(string filePath)
    {
        FilePath = filePath;
    }

    public abstract Task<IEnumerable<string[]>> ReadFileAsync();
}