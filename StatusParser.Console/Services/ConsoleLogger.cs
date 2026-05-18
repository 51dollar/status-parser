using System.Diagnostics;

namespace StatusParser.Console.Services;

public sealed class ConsoleLogger : ILogger, IDisposable
{
    private readonly string _logPath;
    private readonly StreamWriter? _fileWriter;

    public ConsoleLogger()
    {
        var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        Directory.CreateDirectory(logDir);

        var fileName = $"error_{DateTime.Now:yyyyMMdd}.log";
        _logPath = Path.Combine(logDir, fileName);

        try
        {
            _fileWriter = new StreamWriter(_logPath, append: true) { AutoFlush = true };
        }
        catch
        {
            // File logging unavailable, continue without it
        }
    }

    public void Info(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        System.Console.WriteLine(line);
    }

    public void Error(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
        System.Console.Error.WriteLine(line);
        WriteToFile(line);
    }

    public void Warn(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WARN: {message}";
        System.Console.WriteLine(line);
        WriteToFile(line);
    }

    public void LogException(Exception ex, string context)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Exception in {context}:{Environment.NewLine}{ex}";
        System.Console.Error.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error in {context}: {ex.Message}");
        WriteToFile(line);
    }

    private void WriteToFile(string line)
    {
        try
        {
            _fileWriter?.WriteLine(line);
        }
        catch
        {
            // Ignore file write failures
        }
    }

    public void Dispose()
    {
        _fileWriter?.Dispose();
    }
}
