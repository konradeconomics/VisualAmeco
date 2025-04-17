using ClosedXML.Excel;

namespace VisualAmeco.Parser.Parsers;

public class XlsxFileReader : FileReader
{
    public XlsxFileReader(string filePath) : base(filePath)
    {
    }

    public override async Task<IEnumerable<string[]>> ReadFileAsync()
    {
        var data = new List<string[]>();
        try
        {
            using (var workbook = new XLWorkbook(FilePath))
            {
                var worksheet = workbook.Worksheets.First(); // Assuming data is in the first sheet
                var firstRow = worksheet.FirstRowUsed();
                var lastRow = worksheet.LastRowUsed();
                var firstCell = firstRow.FirstCellUsed();
                var lastCell = lastRow.LastCellUsed();

                for (int row = firstRow.RowNumber(); row <= lastRow.RowNumber(); row++)
                {
                    var rowData = new List<string>();
                    for (int col = firstCell.Address.ColumnNumber; col <= lastCell.Address.ColumnNumber; col++)
                    {
                        var cell = worksheet.Cell(row, col);
                        string cellValue = "";
                        if (!cell.IsEmpty())
                        {
                            cellValue = cell.Value.ToString();
                        }

                        rowData.Add(cellValue);
                    }

                    data.Add(rowData.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading XLSX file '{FilePath}': {ex.Message}");
            return Enumerable.Empty<string[]>();
        }

        return data;
    }
}