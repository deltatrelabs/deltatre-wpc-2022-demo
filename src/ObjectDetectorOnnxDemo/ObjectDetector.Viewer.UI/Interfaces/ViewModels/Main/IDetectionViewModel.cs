namespace Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels;

using Avalonia;

public interface IDetectionViewModel
{
    public int Id { get; }
    public string Label { get; }
    public float Score { get; }
    public Rect BoundingBox { get; }
}
