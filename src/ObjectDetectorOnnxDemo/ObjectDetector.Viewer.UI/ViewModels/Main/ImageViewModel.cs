namespace Deltatre.ObjectDetector.Viewer.UI.ViewModels.Main;

using Avalonia.Media.Imaging;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels;
using ReactiveUI;
using System;
using System.IO;
using System.Threading.Tasks;

public class ImageViewModel : ViewModelBase, IImageViewModel
{
    #region Properties
    private byte[] _pixelData;
    public byte[] PixelData
    {
        get => _pixelData;
        private set => this.RaiseAndSetIfChanged(ref _pixelData, value);
    }

    private string _path;
    public string Path
    {
        get => _path;
        private set => this.RaiseAndSetIfChanged(ref _path, value);
    }

    private bool _isPreview;
    public bool IsPreview
    {
        get => _isPreview;
        private set => this.RaiseAndSetIfChanged(ref _isPreview, value);
    }

    private string _fileName;
    public string FileName
    {
        get => _fileName;
        private set => this.RaiseAndSetIfChanged(ref _fileName, value);
    }

    //private readonly ObservableAsPropertyHelper<Bitmap?> _image;
    //public Bitmap? Image => _image?.Value;
    private Bitmap? _image;
    public Bitmap? Image
    {
        get => _image;
        private set => this.RaiseAndSetIfChanged(ref _image, value);
    }
    #endregion

    #region Constructor
    public ImageViewModel(string filePath, bool isPreview)
    {
        _pixelData = Array.Empty<byte>();
        _path = filePath;
        _isPreview = isPreview;
        _fileName = System.IO.Path.GetFileName(Path);
    }
    #endregion

    #region Private methods
    public async Task LoadImageAsync()
    {
        Image?.Dispose();
        Image = null;
        _pixelData = Array.Empty<byte>();

        if (File.Exists(Path))
        {
            using (var imageStream = File.OpenRead(Path))
            {
                if (IsPreview)
                {
                    Image = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
                }
                else
                {
                    Image = await Task.Run(() =>
                    {
                        // See: https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata.scan0?view=dotnet-plat-ext-6.0

                        var bitmap = new System.Drawing.Bitmap(imageStream);

                        // Copy the RGB values into the array
                        var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        int bytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
                        _pixelData = new byte[bytes];
                        System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, _pixelData, 0, bytes);
                        bitmap.UnlockBits(bitmapData);

                        // Create the Avalonia bitmap
                        bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        var avaloniaBitmap = new Bitmap(Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Premul,
                                bitmapData.Scan0,
                                new Avalonia.PixelSize(bitmapData.Width, bitmapData.Height),
                                new Avalonia.Vector(96, 96),
                                bitmapData.Stride);
                        bitmap.UnlockBits(bitmapData);
                        bitmap.Dispose();
                        return avaloniaBitmap;
                    });
                }
            }
        }
    }

    //private static Bitmap? ConvertToAvaloniaBitmap(System.Drawing.Image bitmap)
    //{
    //    if (bitmap == null)
    //        return null;

    //    System.Drawing.Bitmap bitmapTmp = new System.Drawing.Bitmap(bitmap);
    //    var bitmapdata = bitmapTmp.LockBits(new System.Drawing.Rectangle(0, 0, bitmapTmp.Width, bitmapTmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
    //    Bitmap bitmap1 = new Bitmap(Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Premul,
    //        bitmapdata.Scan0,
    //        new Avalonia.PixelSize(bitmapdata.Width, bitmapdata.Height),
    //        new Avalonia.Vector(96, 96),
    //        bitmapdata.Stride);
    //    bitmapTmp.UnlockBits(bitmapdata);
    //    bitmapTmp.Dispose();
    //    return bitmap1;
    //} 
    #endregion
}
