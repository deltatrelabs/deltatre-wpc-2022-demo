namespace Deltatre.ObjectDetector.Viewer.UI;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels;
using Deltatre.ObjectDetector.Viewer.UI.Views;
using Splat;
using System;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DataContext = GetRequiredService<IMainWindowViewModel>(Locator.Current);
            desktop.MainWindow = new MainWindow
            {
                DataContext = DataContext
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static TService GetRequiredService<TService>(IReadonlyDependencyResolver resolver)
    {
        var service = resolver.GetService<TService>();
        if (service is null) // Splat is not able to resolve type for us
        {
            throw new InvalidOperationException($"Failed to resolve object of type {typeof(TService)}"); // throw error with detailed description
        }

        return service; // return instance if not null
    }
}
