namespace Deltatre.ObjectDetector.Viewer.UI.ViewModels.Main;

using Deltatre.ObjectDetector.Viewer.UI.Models;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.Services;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels.Main;
using Deltatre.ObjectDetector.YOLOv7.Interfaces;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class MainViewModel : ViewModelBase, IMainViewModel
{
    #region Private fields
    private readonly ISystemDialogService _systemDialogService;
    private readonly IObjectDetectorModelFactory _objectDetectorModelFactory;
    private readonly InferenceConfiguration _config;
    private readonly IObjectDetectorModel _objectDetectorModel;
    private readonly Stopwatch _stopwatch;
    #endregion

    #region Properties
    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    private bool _imageCollectionEmpty;
    public bool ImageCollectionIsEmpty
    {
        get => _imageCollectionEmpty;
        private set => this.RaiseAndSetIfChanged(ref _imageCollectionEmpty, value);
    }

    private bool _detectionCollectionEmpty;
    public bool DetectionCollectionIsEmpty
    {
        get => _detectionCollectionEmpty;
        private set => this.RaiseAndSetIfChanged(ref _detectionCollectionEmpty, value);
    }

    public ObservableCollection<IImageViewModel> Images { get; private set; }
    public ObservableCollection<IDetectionViewModel> Detections { get; private set; }

    public ICommand OpenFolderCommand { get; private set; }

    private IImageViewModel? _selectedPreview;
    public IImageViewModel? SelectedPreview
    {
        get => _selectedPreview;
        set => this.RaiseAndSetIfChanged(ref _selectedPreview, value);
    }

    private IImageViewModel? _currentImage;
    public IImageViewModel? CurrentImage
    {
        get => _currentImage;
        set => this.RaiseAndSetIfChanged(ref _currentImage, value);
    }
    #endregion

    #region Constructor
    public MainViewModel()
    {
        // DESIGN TIME //

        IsBusy = true;
        StatusMessage = "Ready";
        Images = new ObservableCollection<IImageViewModel>();
        Detections = new ObservableCollection<IDetectionViewModel>
        {
            new DetectionViewModel(new Detection { Id = 0, Label = "TEST_LABEL1", BoundingBox = new System.Drawing.Rectangle(0, 0, 100, 100), Score = 80.0f }),
            new DetectionViewModel(new Detection { Id = 1, Label = "TEST_LABEL2", BoundingBox = new System.Drawing.Rectangle(500, 500, 100, 100), Score = 99.1f })
        };
        ImageCollectionIsEmpty = true;
        DetectionCollectionIsEmpty = true;
    }

    public MainViewModel(ISystemDialogService systemDialogService, IObjectDetectorModelFactory objectDetectorModelFactory, InferenceConfiguration config)
    {
        _systemDialogService = systemDialogService;
        _objectDetectorModelFactory = objectDetectorModelFactory;
        _config = config;
        _objectDetectorModel = _objectDetectorModelFactory.GetModel(_config.ObjectDetectionModel, _config.DeviceId);
        _stopwatch = new Stopwatch();

        IsBusy = false;
        StatusMessage = "Ready";
        Images = new ObservableCollection<IImageViewModel>();
        Detections = new ObservableCollection<IDetectionViewModel>();

        // See: https://www.reactiveui.net/docs/getting-started/compelling-example

        OpenFolderCommand = ReactiveCommand.Create(OpenFolderCommandHandler);

        this.WhenAnyValue(x => x.Images.Count).Subscribe(x => ImageCollectionIsEmpty = x == 0);
        this.WhenAnyValue(x => x.SelectedPreview).Subscribe(async x =>
        {
            if (x is null)
            {
                return;
            }

            IsBusy = true;
            CurrentImage?.Image?.Dispose();
            CurrentImage = new ImageViewModel(x.Path, false);
            await CurrentImage.LoadImageAsync();
            await AnalyzeImageAsync();
            IsBusy = false;
        });

        // Dummy Model Score (to pre-load ONNX engine)
        Task.Run(() => _objectDetectorModel.Score(new byte[100], 100, 100));
    }
    #endregion

    #region Private methods
    private async void OpenFolderCommandHandler()
    {
        var folderPath = await _systemDialogService.GetDirectoryAsync();
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return;
        }

        IsBusy = true;
        Images.Clear();

        var files = Directory.GetFiles(folderPath, "*.jpg");

        foreach (var file in files)
        {
            var imageViewModel = new ImageViewModel(file, true);
            await imageViewModel.LoadImageAsync();
            Images.Add(imageViewModel);
        }

        if (files.Length > 0)
        {
            StatusMessage = $"Loaded {Images.Count} JPG file(s) from the selected folder";
        }
        else
        {
            StatusMessage = $"No images found from the selected folder";
        }
        IsBusy = false;
    }

    private async Task AnalyzeImageAsync()
    {
        if (_currentImage is null || _currentImage.Image is null)
        {
            return;
        }

        await Task.Run(() =>
        {
            _stopwatch.Restart();
            var predictions = _objectDetectorModel.Score(_currentImage.PixelData, _currentImage.Image.PixelSize.Width, _currentImage.Image.PixelSize.Height);
            Detections.Clear();
            foreach (var pred in predictions)
            {
                Detections.Add(new DetectionViewModel(new Detection
                {
                    Id = 0,
                    Label = pred.Label.Name,
                    Score = pred.Score,
                    BoundingBox = new System.Drawing.Rectangle
                    {
                        X = (int)Math.Round(pred.Rectangle.X),
                        Y = (int)Math.Round(pred.Rectangle.Y),
                        Width = (int)Math.Round(pred.Rectangle.Width),
                        Height = (int)Math.Round(pred.Rectangle.Height)
                    }
                }));
            }
            _stopwatch.Stop();
            StatusMessage = $"Found: {predictions.Count()} objects in {_stopwatch.ElapsedMilliseconds}ms";
        });
    }
    #endregion
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
