namespace Deltatre.ObjectDetector.Viewer.UI.DI;

using Deltatre.ObjectDetector.Viewer.UI.Interfaces.Services;
using Deltatre.ObjectDetector.Viewer.UI.Services;
using Deltatre.ObjectDetector.YOLOv7;
using Deltatre.ObjectDetector.YOLOv7.Interfaces;
using Splat;

public static class ServicesBootstrapper
{
    public static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        RegisterCommonServices(services, resolver);
    }

    private static void RegisterCommonServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<ISystemDialogService>(() => new SystemDialogService(resolver.GetRequiredService<IMainWindowProvider>()));
        services.RegisterLazySingleton<IObjectDetectorModelFactory>(() => new ObjectDetectorYOLOv7ModelFactory());
    }
}
