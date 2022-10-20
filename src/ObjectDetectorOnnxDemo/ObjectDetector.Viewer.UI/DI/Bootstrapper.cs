namespace Deltatre.ObjectDetector.Viewer.UI.DI;

using Splat;

public static class Bootstrapper
{
    // See: https://dev.to/ingvarx/avaloniaui-dependency-injection-4aka

    public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver, string[] args)
    {
        ConfigurationBootstrapper.RegisterConfiguration(services, resolver, args);
        LoggingBootstrapper.RegisterLogging(services, resolver);
        AvaloniaServicesBootstrapper.RegisterAvaloniaServices(services);
        ServicesBootstrapper.RegisterServices(services, resolver);
        ViewModelsBootstrapper.RegisterViewModels(services, resolver);
    }
}