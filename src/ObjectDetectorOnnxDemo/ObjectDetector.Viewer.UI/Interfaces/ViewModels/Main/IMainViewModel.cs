namespace Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels.Main
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    public interface IMainViewModel
    {
        string StatusMessage { get; }
        bool IsBusy { get; }
        ICommand OpenFolderCommand { get; }

        bool ImageCollectionIsEmpty { get; }

        ObservableCollection<IImageViewModel> Images { get; }

        bool DetectionCollectionIsEmpty { get; }
        ObservableCollection<IDetectionViewModel> Detections { get; }

        IImageViewModel? SelectedPreview { get; set; }
        IImageViewModel? CurrentImage { get; set; }
    }
}
