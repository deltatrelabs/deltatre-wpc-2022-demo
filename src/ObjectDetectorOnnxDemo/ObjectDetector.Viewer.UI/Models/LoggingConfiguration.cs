namespace Deltatre.ObjectDetector.Viewer.UI.Models;

public class LoggingConfiguration
{
    public string LogFileName { get; set; }

    public long LimitBytes { get; set; }

    public string DefaultLogLevel { get; set; }

    public string MicrosoftLogLevel { get; set; }

    public LoggingConfiguration()
    {
        LogFileName = string.Empty;
        LimitBytes = -1;
        DefaultLogLevel = string.Empty;
        MicrosoftLogLevel = string.Empty;
    }
}
