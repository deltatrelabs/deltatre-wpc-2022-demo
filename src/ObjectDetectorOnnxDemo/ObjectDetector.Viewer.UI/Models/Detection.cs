namespace Deltatre.ObjectDetector.Viewer.UI.Models;

using System.Drawing;

public class Detection
{
    public int Id { get; set; }
    public string Label { get; set; }
    public float Score { get; set; }
    public Rectangle BoundingBox { get; set; }

    public Detection()
    {
        Id = -1;
        Label = "N/A";
        Score = -1.0f;
        BoundingBox = Rectangle.Empty;
    }
}
