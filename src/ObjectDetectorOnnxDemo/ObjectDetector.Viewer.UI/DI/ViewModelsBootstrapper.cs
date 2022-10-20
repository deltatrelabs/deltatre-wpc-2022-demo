namespace Deltatre.ObjectDetector.Viewer.UI.DI;

using Deltatre.ObjectDetector.Viewer.UI.Models;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.Services;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels.Main;
using Deltatre.ObjectDetector.Viewer.UI.ViewModels;
using Deltatre.ObjectDetector.Viewer.UI.ViewModels.Main;
using Deltatre.ObjectDetector.YOLOv7.Interfaces;
using Splat;

public static class ViewModelsBootstrapper
{
    public static void RegisterViewModels(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        RegisterCommonViewModels(services, resolver);
    }

    private static void RegisterCommonViewModels(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
            resolver.GetRequiredService<IMainViewModel>()
        ));

        services.RegisterLazySingleton<IMainViewModel>(() => new MainViewModel(
            resolver.GetRequiredService<ISystemDialogService>(),
            resolver.GetRequiredService<IObjectDetectorModelFactory>(),
            resolver.GetRequiredService<InferenceConfiguration>()
        ));
    }
}
