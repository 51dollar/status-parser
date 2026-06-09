using OfficeOpenXml;
using StatusParser.Console.Models;

namespace StatusParser.Console.Services;

public sealed class ExcelReader
{
    public SheetData ReadSheet(string filePath)
    {
        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets[0];
        var sheetName = worksheet.Name;

        if (worksheet.Dimension == null)
            throw new InvalidOperationException($"Worksheet is empty: {filePath}");

        var rowCount = worksheet.Dimension.Rows;
        var colCount = worksheet.Dimension.Columns;

        var range = worksheet.Cells[1, 1, rowCount, colCount];
        var data = range.Value as object[,];

        if (data == null)
            throw new InvalidOperationException($"Could not read data: {filePath}");

        var headerIndex = BuildHeaderIndex(data, colCount);

        return new SheetData(data, rowCount, colCount, sheetName, headerIndex);
    }

    public static Dictionary<string, int> BuildHeaderIndex(object[,] data, int colCount)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var c = 0; c < colCount; c++)
        {
            var name = data[0, c]?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(name) && !map.ContainsKey(name))
                map[name] = c;
        }
        return map;
    }

    public static string? GetRawNewStatusValue(object[,] data, int rowIdx, int colCount, int f10ColIdx)
    {
        if (f10ColIdx < 0 || f10ColIdx >= colCount)
            return null;

        var maxCol = Math.Min(f10ColIdx + 3, colCount);
        for (var c = f10ColIdx; c < maxCol; c++)
        {
            var val = data[rowIdx, c]?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(val))
                return val;
        }
        return null;
    }
}
