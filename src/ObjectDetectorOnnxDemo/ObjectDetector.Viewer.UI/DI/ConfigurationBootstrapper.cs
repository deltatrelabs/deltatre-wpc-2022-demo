namespace Deltatre.ObjectDetector.Viewer.UI.DI;

using Deltatre.ObjectDetector.Viewer.UI.Models;
using Microsoft.Extensions.Configuration;
using Splat;
using System;

public static class ConfigurationBootstrapper
{
    public static void RegisterConfiguration(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver, string[] args)
    {
        var configuration = BuildConfiguration(args);

        RegisterLoggingConfiguration(services, configuration);
        RegisterInferenceConfiguration(services, configuration);
    }

    private static IConfiguration BuildConfiguration(string[] args)
    {

        var configuration = new ConfigurationBuilder();
#if DEBUG
        var env = "development";
#else
        var env = "release";
#endif

        var environmentVariable = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? env;

        // Only add secrets when in development
        var isDevelopment = string.IsNullOrEmpty(environmentVariable) || environmentVariable.ToLower().StartsWith("development");
        if (isDevelopment)
        {
            configuration.AddUserSecrets<Program>();
        }

        // Environment and command-line args can override settings in appsettings.json
        // Remember to configure variables as "DETECTOR_<section>__<key>" (and restart Visual Studio or Terminal, if open)
        configuration
            .AddEnvironmentVariables(prefix: "DETECTOR_")
            .AddCommandLine(args);

        return configuration
            //.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentVariable}.json", true, false)
            .Build();
    }

    private static void RegisterLoggingConfiguration(IMutableDependencyResolver services, IConfiguration configuration)
    {
        var config = new LoggingConfiguration();
        configuration.GetSection("Logging").Bind(config);
        services.RegisterConstant(config);
    }

    private static void RegisterInferenceConfiguration(IMutableDependencyResolver services, IConfiguration configuration)
    {
        var config = new InferenceConfiguration();
        configuration.GetSection("Inference").Bind(config);
        services.RegisterConstant(config);
    }
}