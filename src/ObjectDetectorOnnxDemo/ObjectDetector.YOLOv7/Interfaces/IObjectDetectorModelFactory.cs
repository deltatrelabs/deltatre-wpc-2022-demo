namespace Deltatre.ObjectDetector.YOLOv7.Interfaces;

public interface IObjectDetectorModelFactory
{
    IObjectDetectorModel GetModel(string model, int deviceId = -1);
}
