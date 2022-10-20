namespace Deltatre.ObjectDetector.Viewer.UI.ViewModels.Main;

using Avalonia;
using Deltatre.ObjectDetector.Viewer.UI.Models;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels;

public class DetectionViewModel : ViewModelBase, IDetectionViewModel
{
    #region Private fields
    private readonly Detection _detection;
    #endregion

    #region Properties
    public int Id => _detection.Id;
    public string Label => _detection.Label;
    public float Score => _detection.Score;
    public Rect BoundingBox { get; }
    #endregion

    #region Constructor
    public DetectionViewModel(Detection detection)
    {
        _detection = detection;
        BoundingBox = new Rect(detection.BoundingBox.X, detection.BoundingBox.Y, detection.BoundingBox.Width, detection.BoundingBox.Height);
    }
    #endregion

    #region Private methods
    #endregion
}
