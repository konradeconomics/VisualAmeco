using VisualAmeco.Parser.Parsers;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Parser Console App");

        var csvReader = new CsvFileReader();
        var csvRecords = await csvReader.ReadFileAsync(filePaths: new List<string> { "path/to/your/file.csv" });
        Console.WriteLine("\nCSV Records:");
        foreach (var record in csvRecords)
        {
            Console.WriteLine(string.Join(" | ", record));
        }
    }
}