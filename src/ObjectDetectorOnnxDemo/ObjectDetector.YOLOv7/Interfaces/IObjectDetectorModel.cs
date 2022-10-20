namespace Deltatre.ObjectDetector.YOLOv7.Interfaces;

using Deltatre.ObjectDetector.YOLOv7.Model;

public interface IObjectDetectorModel
{
    string Name { get; }

    IEnumerable<YoloPrediction> Score(byte[] imageData, int width, int height);
}
