using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace CurveEditor.Services;

/// <summary>
/// Folder picker backed by Avalonia <see cref="IStorageProvider"/>.
/// </summary>
public sealed class StorageProviderFolderPicker : IFolderPicker
{
    public async Task<string?> PickFolderAsync(CancellationToken cancellationToken)
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider is null)
        {
            return null;
        }

        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open Folder",
            AllowMultiple = false
        });

        var folder = folders.FirstOrDefault();
        return folder?.Path.LocalPath;
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.StorageProvider;
        }

        return null;
    }
}
