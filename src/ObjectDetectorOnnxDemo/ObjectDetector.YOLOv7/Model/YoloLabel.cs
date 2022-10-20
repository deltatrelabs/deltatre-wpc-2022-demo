// Based on: https://github.com/mentalstack/yolov5-net

namespace Deltatre.ObjectDetector.YOLOv7.Model
{
    using System.Drawing;

    /// <summary>
    /// Label of detected object.
    /// </summary>
    public class YoloLabel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public YoloLabelKind Kind { get; set; }
        public Color Color { get; set; }

        public YoloLabel()
        {
            Name = string.Empty;
            Color = Color.Yellow;
        }
    }
}
