namespace Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels;

using Avalonia.Media.Imaging;
using System.Threading.Tasks;

public interface IImageViewModel
{
    string Path { get; }
    Bitmap? Image { get; }
    byte[] PixelData { get; }
    bool IsPreview { get; }

    Task LoadImageAsync();
}
