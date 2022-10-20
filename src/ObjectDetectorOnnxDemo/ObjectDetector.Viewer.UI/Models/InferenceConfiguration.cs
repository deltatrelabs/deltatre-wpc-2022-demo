namespace Deltatre.ObjectDetector.Viewer.UI.Models;

public class InferenceConfiguration
{
    public string ObjectDetectionModel { get; set; }
    public int DeviceId { get; set; }

    public InferenceConfiguration()
	{
		ObjectDetectionModel = string.Empty;
        DeviceId = -1;
	}
}
