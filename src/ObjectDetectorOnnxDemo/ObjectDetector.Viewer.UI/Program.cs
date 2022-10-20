namespace Deltatre.ObjectDetector.Viewer.UI;

using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Deltatre.ObjectDetector.Viewer.UI.DI;
using Microsoft.Extensions.Logging;
using Splat;
using System;
using System.Globalization;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        // Add the event handler for handling non-UI thread exceptions to the event
        //AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => HandleUnhandledException("Non-UI", (Exception)e.ExceptionObject);
        TaskScheduler.UnobservedTaskException += (sender, e) => HandleUnhandledException("Task", e.Exception);


        try
        {
            RegisterDependencies(args);

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        }
        catch (Exception e)
        {
            HandleUnhandledException("Application", e);
        }
    }

    private static void HandleUnhandledException(string category, Exception ex)
    {
        var logger = Locator.Current.GetRequiredService<ILogger>();

        logger.LogCritical($"Unhandled [{category}] error: {ex}");

        Environment.Exit(-1);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            //.With(new Win32PlatformOptions { AllowEglInitialization = true/*, UseWgl = true*/})
            //.With(new X11PlatformOptions { UseGpu = true/*, UseEGL = true*/ })
            //.With(new MacOSPlatformOptions { ShowInDock = true })
            .With(new AvaloniaNativePlatformOptions { UseGpu = true })
            .LogToTrace()
            .UseReactiveUI();
    }

    private static void RegisterDependencies(string[] args)
    {
        Bootstrapper.Register(Locator.CurrentMutable, Locator.Current, args);
    }
}
