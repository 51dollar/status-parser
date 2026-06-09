using System.Diagnostics;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using StatusParser.Console.Configuration;

namespace StatusParser.Console.Services;

public sealed class ExcelWriter
{
    private readonly AppConfig _config;
    private readonly ILogger _logger;

    public ExcelWriter(AppConfig config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    public (int startRow, int lastNumber) GetExistingFileState(string outputPath, int columnCount)
    {
        try
        {
            using var package = new ExcelPackage(new FileInfo(outputPath));
            var worksheet = package.Workbook.Worksheets[0];

            if (worksheet.Dimension == null)
                return (1, 0);

            var existingRows = worksheet.Dimension.Rows;
            var lastNumber = 0;

            if (existingRows > 1)
            {
                var lastCell = worksheet.Cells[existingRows, 1];
                if (lastCell.Value != null)
                    int.TryParse(lastCell.Value.ToString(), out lastNumber);
            }

            return (existingRows + 1, lastNumber);
        }
        catch
        {
            _logger.Warn($"  Could not read existing file, starting fresh.");
            return (1, 0);
        }
    }

    public void WriteOutput(string outputPath, List<object[]> rows, int columnCount, int startRow)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var outDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            {
                _logger.Info($"  Creating output directory: {outDir}");
                Directory.CreateDirectory(outDir);
            }

            var isNewFile = !File.Exists(outputPath);
            using var package = isNewFile ? new ExcelPackage() : new ExcelPackage(new FileInfo(outputPath));

            ExcelWorksheet worksheet;
            int dataStartRow;

            if (isNewFile)
            {
                worksheet = package.Workbook.Worksheets.Add("Результат");
                dataStartRow = 2;

                for (var c = 0; c < columnCount; c++)
                    worksheet.Cells[1, c + 1].Value = _config.ColumnMappingRules[c].TargetColumnName;

                var headerRange = worksheet.Cells[1, 1, 1, columnCount];
                headerRange.Style.Font.Bold = true;
                ApplyHeaderColor(headerRange);
            }
            else
            {
                worksheet = package.Workbook.Worksheets[0];
                dataStartRow = startRow;
                _logger.Info($"  Appending to existing file (start row: {dataStartRow})");
            }

            var totalRows = rows.Count;
            if (totalRows > 0)
            {
                _logger.Info($"  Building {totalRows}x{columnCount} data array...");
                var data = new object[totalRows, columnCount];
                for (var i = 0; i < totalRows; i++)
                    for (var c = 0; c < columnCount; c++)
                        data[i, c] = rows[i][c] ?? string.Empty;

                var endRow = dataStartRow + totalRows - 1;
                _logger.Info($"  Writing rows {dataStartRow}..{endRow} to worksheet...");
                worksheet.Cells[dataStartRow, 1, endRow, columnCount].Value = data;
            }

            _logger.Info($"  Saving to disk...");
            package.SaveAs(new FileInfo(outputPath));

            sw.Stop();
            var fileSize = new FileInfo(outputPath).Length / 1024.0;
            _logger.Info($"  File saved ({fileSize:F1} KB, {sw.Elapsed.TotalSeconds:F2}s)");
            _logger.Info("Done.");
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "writing output file");
            throw;
        }
    }

    private void ApplyHeaderColor(ExcelRange headerRange)
    {
        var colorName = _config.OutputTableHeaderBackgroundColor?.Trim().ToLowerInvariant() ?? "yellow";
        var color = colorName switch
        {
            "yellow" => Color.Yellow,
            "green" => Color.Green,
            "red" => Color.Red,
            "blue" => Color.Blue,
            "cyan" => Color.Cyan,
            "magenta" => Color.Magenta,
            "orange" => Color.Orange,
            "gray" => Color.Gray,
            "lightgray" => Color.LightGray,
            "darkgray" => Color.DarkGray,
            "lightblue" => Color.LightBlue,
            "lightgreen" => Color.LightGreen,
            _ => Color.Yellow
        };

        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
        headerRange.Style.Fill.BackgroundColor.SetColor(color);
    }
}
