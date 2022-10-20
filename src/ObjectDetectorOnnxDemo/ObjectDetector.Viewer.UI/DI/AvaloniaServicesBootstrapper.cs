namespace Deltatre.ObjectDetector.Viewer.UI.DI;

using Deltatre.ObjectDetector.Viewer.UI.Interfaces.Services;
using Deltatre.ObjectDetector.Viewer.UI.Services;
using Splat;

public static class AvaloniaServicesBootstrapper
{
    public static void RegisterAvaloniaServices(IMutableDependencyResolver services)
    {
        services.RegisterLazySingleton<IMainWindowProvider>(() => new MainWindowProvider());
    }
}