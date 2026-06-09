namespace StatusParser.Console.Models;

public sealed record SheetData(
    object[,] Data,
    int RowCount,
    int ColumnCount,
    string SheetName,
    Dictionary<string, int> HeaderIndex
);
