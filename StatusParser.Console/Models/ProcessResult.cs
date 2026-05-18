namespace StatusParser.Console.Models;

public sealed class ProcessResult
{
    public int TotalFiles { get; set; }
    public int TotalRowsMatched { get; set; }
    public string? OutputFilePath { get; set; }
}
