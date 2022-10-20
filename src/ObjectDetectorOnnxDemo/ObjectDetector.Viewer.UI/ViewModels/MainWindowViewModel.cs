namespace Deltatre.ObjectDetector.Viewer.UI.ViewModels;

using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels.Main;
using Deltatre.ObjectDetector.Viewer.UI.ViewModels.Main;
using ReactiveUI;

public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    public IMainViewModel MainViewModel { get; private set; }

    private bool _showOverlay;
    public bool ShowOverlay
    {
        get => _showOverlay;
        set => this.RaiseAndSetIfChanged(ref _showOverlay, value);
    }

    public MainWindowViewModel()
    {
        // Design-time support
        MainViewModel = new MainViewModel();
    }

    public MainWindowViewModel(IMainViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;
    }
}
