namespace Deltatre.ObjectDetector.Viewer.UI.Views.Main
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Media;
    using Avalonia.Media.Imaging;
    using Avalonia.Media.Immutable;
    using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels;
    using System;
    using System.Collections.ObjectModel;

    public partial class DetectionsView : UserControl
    {
        #region Properties
        public Size ViewPortSize => ViewPort.Bounds.Size;

        public static readonly StyledProperty<Bitmap?> ImageProperty = AvaloniaProperty.Register<DetectionsView, Bitmap?>(nameof(Image));

        /// <summary>
        /// Gets or sets the image to be displayed
        /// </summary>
        public Bitmap? Image
        {
            get => GetValue(ImageProperty);
            set
            {
                SetValue(ImageProperty, value);
                UpdateViewPort();
                InvalidateVisual();
            }
        }

        public static readonly DirectProperty<DetectionsView, ObservableCollection<IDetectionViewModel>> DetectionsProperty = AvaloniaProperty.RegisterDirect<DetectionsView, ObservableCollection<IDetectionViewModel>>(nameof(Detections), o => o.Detections, (o, value) => o.Detections = value);
        private ObservableCollection<IDetectionViewModel> _detections;
        public ObservableCollection<IDetectionViewModel> Detections
        {
            get => _detections;
            set
            {
                SetAndRaise(DetectionsProperty, ref _detections, value);
                UpdateViewPort();
                InvalidateVisual();
            }
        }
        #endregion

        #region Constructor
        public DetectionsView()
        {
            InitializeComponent();

            _detections = new ObservableCollection<IDetectionViewModel>();
        }
        #endregion

        #region Methods
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (Image is null) return;

            // Draw image and boxes to fit available viewport space
            ///////////////////////////////////////////////////////

            // Calculate scaled image size and coordinates
            double scaleFactor = Math.Min(ViewPortSize.Width / Image.Size.Width, ViewPortSize.Height / Image.Size.Height);
            double scaledImageWidth = Math.Floor(Image.Size.Width * scaleFactor);
            double scaledImageHeight = Math.Floor(Image.Size.Height * scaleFactor);
            double xOffset = (ViewPortSize.Width - scaledImageWidth) / 2;
            double yOffset = (ViewPortSize.Height - scaledImageHeight) / 2;

            // Draw image
            context.DrawImage(Image, new(xOffset, yOffset, scaledImageWidth, scaledImageHeight));

            // Draw detections (on the scaled image)

            var thickness = 1.0 * scaleFactor;

            foreach (var detection in Detections)
            {
                // Box/label color (may depend from detection label)
                var labelColor = Colors.Yellow.ToUint32();
                var textColor = Colors.Black.ToUint32();
                var boxPen = new ImmutablePen(labelColor, thickness);

                // Calculate box scaled size and coordinates
                var x = (int)Math.Max(detection.BoundingBox.X, 0) * scaleFactor;
                var y = (int)Math.Max(detection.BoundingBox.Y, 0) * scaleFactor;
                var width = (int)Math.Min(scaledImageWidth - x, detection.BoundingBox.Width * scaleFactor);
                var height = (int)Math.Min(scaledImageHeight - y, detection.BoundingBox.Height * scaleFactor);

                // Draw label text
                var text = new FormattedText($"{detection.Label} ({detection.Score * 100:0}%)", Typeface.Default, 12 * scaleFactor, TextAlignment.Left, TextWrapping.Wrap, Size.Infinity);
                var textSize = new Size(text.Bounds.Width, text.Bounds.Height);
                var labelPosY = yOffset + y - (int)textSize.Height - 1;
                if(labelPosY <= 0) labelPosY = yOffset + y + height;
                var atPoint = new Point(xOffset + x, labelPosY);
                var fontBrush = new ImmutableSolidColorBrush(textColor);
                var textBackgroundBrush = new ImmutableSolidColorBrush(labelColor);
                context.FillRectangle(textBackgroundBrush, new Rect(xOffset + x - 1, labelPosY, (int)Math.Max(textSize.Width + 1, width + 1), (int)textSize.Height + 1));
                context.DrawText(fontBrush, atPoint, text);

                // Draw box
                context.DrawRectangle(boxPen, new Rect(xOffset + x, yOffset + y, width, height));
            }
        }
        #endregion

        #region Private methods
        private bool UpdateViewPort()
        {
            if (Detections is null || Detections.Count == 0)
            {
                HorizontalScrollBar.Maximum = 0;
                VerticalScrollBar.Maximum = 0;
                return true;
            }

            var imageViewPort = GetImageViewPort();
            var scaledImageWidth = imageViewPort.Width;
            var scaledImageHeight = imageViewPort.Height;
            var width = scaledImageWidth - HorizontalScrollBar.ViewportSize;
            var height = scaledImageHeight - VerticalScrollBar.ViewportSize;

            bool changed = false;
            if (Math.Abs(HorizontalScrollBar.Maximum - width) > 0.01)
            {
                HorizontalScrollBar.Maximum = width;
                changed = true;
            }

            if (Math.Abs(VerticalScrollBar.Maximum - scaledImageHeight) > 0.01)
            {
                VerticalScrollBar.Maximum = height;
                changed = true;
            }

            return changed;
        }

        /// <summary>
        /// Gets the image view port (fit in the available space)
        /// </summary>
        /// <returns></returns>
        private Rect GetImageViewPort()
        {
            if (Image is null || Image.Size.Width == 0 || Image.Size.Height == 0 || (ViewPortSize.Width == 0 && ViewPortSize.Height == 0)) return Rect.Empty;

            double scaleFactor = Math.Min(ViewPortSize.Width / Image.Size.Width, ViewPortSize.Height / Image.Size.Height);

            double width = Math.Floor(Image.Size.Width * scaleFactor);
            double height = Math.Floor(Image.Size.Height * scaleFactor);

            double xOffset = (ViewPortSize.Width - width) / 2;
            double yOffset = (ViewPortSize.Height - height) / 2;

            return new(xOffset, yOffset, width, height);
        }

        ///// <summary>
        ///// Gets the zoom to fit level which shows all the image
        ///// </summary>
        //private int GetZoomLevelToFit()
        //{
        //    double zoom;
        //    double aspectRatio;

        //    if (ImageWidth > ImageHeight)
        //    {
        //        aspectRatio = ViewPortSize.Width / ImageWidth;
        //        zoom = aspectRatio * 100.0;

        //        if (ViewPortSize.Height < ImageHeight * zoom / 100.0)
        //        {
        //            aspectRatio = ViewPortSize.Height / ImageHeight;
        //            zoom = aspectRatio * 100.0;
        //        }
        //    }
        //    else
        //    {
        //        aspectRatio = ViewPortSize.Height / ImageHeight;
        //        zoom = aspectRatio * 100.0;

        //        if (ViewPortSize.Width < ImageWidth * zoom / 100.0)
        //        {
        //            aspectRatio = ViewPortSize.Width / ImageWidth;
        //            zoom = aspectRatio * 100.0;
        //        }
        //    }

        //    return (int)zoom;

        //}
        #endregion
    }
}
