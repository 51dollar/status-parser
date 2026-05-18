using System.Text.Json;
using OfficeOpenXml;
using StatusParser.Console.Configuration;
using StatusParser.Console.Services;

namespace StatusParser.Console;

public static class Program
{
    private static ILogger? _logger;

    public static void Main(string[] args)
    {
        ExcelPackage.License.SetNonCommercialPersonal("StatusParser.User");
        System.Console.CancelKeyPress += OnCancelKeyPress;

        using var logger = new ConsoleLogger();
        _logger = logger;

        try
        {
            logger.Info("Loading configuration...");

            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (!File.Exists(configPath))
            {
                logger.Error($"Configuration file not found: {configPath}");
                WaitForExit();
                return;
            }

            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json);

            if (config == null)
            {
                logger.Error("Failed to deserialize configuration.");
                WaitForExit();
                return;
            }

            var validationErrors = config.Validate();
            if (validationErrors.Count > 0)
            {
                foreach (var err in validationErrors)
                    logger.Error($"Configuration error: {err}");
                WaitForExit();
                return;
            }

            var inputFolder = config.ResolvedInputFolder;
            if (!Directory.Exists(inputFolder))
            {
                logger.Error($"Input folder does not exist: {inputFolder}");
                WaitForExit();
                return;
            }

            logger.Info("Enter output file name (without extension): ");
            var outputFileName = (System.Console.ReadLine() ?? string.Empty).Trim().Trim('\uFEFF', '\u200B');

            while (string.IsNullOrWhiteSpace(outputFileName) || outputFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                logger.Error("Invalid file name. Please enter a valid name: ");
                outputFileName = (System.Console.ReadLine() ?? string.Empty).Trim().Trim('\uFEFF', '\u200B');
            }

            var outputPath = Path.Combine(config.ResolvedOutputFolder, $"{outputFileName}.xlsx");
            logger.Info($"Output will be saved to: {outputPath}");
            logger.Info($"Input folder: {inputFolder}");

            var processor = new ExcelProcessor(config, logger);
            var result = processor.Run(outputFileName);

            System.Console.WriteLine();
            System.Console.WriteLine($"Total files processed: {result.TotalFiles}");
            System.Console.WriteLine($"Total rows matched: {result.TotalRowsMatched}");
            System.Console.WriteLine($"Output file: {result.OutputFilePath}");
            System.Console.WriteLine("Processing complete. Press ENTER to exit or ESC to close.");
        }
        catch (Exception ex)
        {
            logger.LogException(ex, "application startup");
            System.Console.Error.WriteLine("A critical error occurred. See log file for details.");
        }
        finally
        {
            WaitForExit();
        }
    }

    private static void WaitForExit()
    {
        try
        {
            while (true)
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Escape)
                    break;
            }
        }
        catch (InvalidOperationException)
        {
            // Input is redirected (e.g. piped), just exit
            System.Console.ReadLine();
        }
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        _logger?.Info("Shutdown requested by user (Ctrl+C). Exiting...");
        Environment.Exit(0);
    }
}
