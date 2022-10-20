namespace Deltatre.ObjectDetector.Viewer.UI.Interfaces.Services;

using Avalonia.Controls;

public interface IMainWindowProvider
{
    Window? GetMainWindow();
}