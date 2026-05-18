namespace StatusParser.Console.Services;

public interface ILogger
{
    void Info(string message);
    void Error(string message);
    void Warn(string message);
    void LogException(Exception ex, string context);
}
