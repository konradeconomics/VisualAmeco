using VisualAmeco.Parser.Parsers;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Parser Console App");

        var csvReader = new CsvFileReader("../../../dummy.csv");
        var csvRecords = await csvReader.ReadFileAsync();
        Console.WriteLine("\nCSV Records:");
        foreach (var record in csvRecords)
        {
            Console.WriteLine(string.Join(" | ", record));
        }
    }
}