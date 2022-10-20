namespace Deltatre.ObjectDetector.Viewer.UI.Services;

using Avalonia.Controls;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.Services;
using Deltatre.ObjectDetector.Viewer.UI.Interfaces.ViewModels;
using System;
using System.Threading.Tasks;

public class SystemDialogService : ISystemDialogService
{
    private readonly IMainWindowProvider _mainWindowProvider;

    public SystemDialogService(IMainWindowProvider mainWindowProvider)
    {
        _mainWindowProvider = mainWindowProvider;
    }

    public async Task<string?> GetDirectoryAsync(string? initialDirectory = null)
    {
        var dialog = new OpenFolderDialog { Directory = initialDirectory };
        var window = _mainWindowProvider.GetMainWindow();
        var mainWindowDataContext = (IMainWindowViewModel?)window?.DataContext;

        if (window is null)
        {
            throw new NullReferenceException(nameof(window));
        }

        if (mainWindowDataContext is null)
        {
            throw new NullReferenceException(nameof(mainWindowDataContext));
        }

        mainWindowDataContext.ShowOverlay = true;
        var res = await dialog.ShowAsync(window);
        mainWindowDataContext.ShowOverlay = false;
        return res;
    }

    public async Task<string?> GetFileAsync(string? initialFile = null)
    {
        var dialog = new SaveFileDialog { InitialFileName = initialFile };
        var window = _mainWindowProvider.GetMainWindow();
        var mainWindowDataContext = (IMainWindowViewModel?)window?.DataContext;

        if (window is null)
        {
            throw new NullReferenceException(nameof(window));
        }

        if (mainWindowDataContext is null)
        {
            throw new NullReferenceException(nameof(mainWindowDataContext));
        }

        mainWindowDataContext.ShowOverlay = true;
        var res = await dialog.ShowAsync(window);
        mainWindowDataContext.ShowOverlay = false;
        return res;
    }
}