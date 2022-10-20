namespace Deltatre.ObjectDetector.YOLOv7.Extensions
{
    using System.Drawing;

    public static class RectangleExtensions
    {
        public static float Area(this RectangleF source)
        {
            return source.Width * source.Height;
        }
    }
}
