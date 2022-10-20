namespace Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels;

using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels.Main;

public interface IMainWindowViewModel
{
    IMainViewModel MainViewModel { get; }
    bool ShowOverlay { get; set; }
}
