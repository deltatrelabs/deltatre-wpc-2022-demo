namespace Deltatre.ObjectDetector.Viewer.UI.DI;

using Deltatre.ObjectDetector.Viewer.UI.Models;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Splat;
using System.IO;

public static class LoggingBootstrapper
{
    public static void RegisterLogging(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton(() =>
        {
            var config = resolver.GetRequiredService<LoggingConfiguration>();
            var logFilePath = GetLogFileName(config);
            var logger = new LoggerConfiguration()
                .MinimumLevel.Override("Default", GetSerilogLevelFromString(config.DefaultLogLevel))
                .MinimumLevel.Override("Microsoft", GetSerilogLevelFromString(config.MicrosoftLogLevel))
                .WriteTo.Console()
                .WriteTo.File(logFilePath, fileSizeLimitBytes: config.LimitBytes)
                .CreateLogger();
            var factory = new SerilogLoggerFactory(logger);
            return factory.CreateLogger("Default");
        });
    }

    private static LogEventLevel GetSerilogLevelFromString(string logLevel)
    {
        return logLevel switch
        {
            "Debug" => LogEventLevel.Debug,
            "Fatal" => LogEventLevel.Fatal,
            "Verbose" => LogEventLevel.Verbose,
            "Information" => LogEventLevel.Information,
            "Warning" => LogEventLevel.Warning,
            _ => LogEventLevel.Debug
        };
    }

    private static string GetLogFileName(LoggingConfiguration config)
    {
        string logDirectory = Directory.GetCurrentDirectory();

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        return Path.Combine(logDirectory, config.LogFileName);
    }
}