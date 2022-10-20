namespace Deltatre.ObjectDetector.Viewer.UI.Interfaces.Services;

using System.Threading.Tasks;

public interface ISystemDialogService
{
    Task<string?> GetDirectoryAsync(string? initialDirectory = null);

    Task<string?> GetFileAsync(string? initialFile = null);
}