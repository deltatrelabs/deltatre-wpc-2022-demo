namespace Deltatre.ObjectDetector.Viewer.UI.Services;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.Services;

public class MainWindowProvider : IMainWindowProvider
{
    public Window? GetMainWindow()
    {
        var lifetime = (IClassicDesktopStyleApplicationLifetime?)Application.Current?.ApplicationLifetime;

        return lifetime?.MainWindow;
    }
}