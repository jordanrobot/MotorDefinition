using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Command to delete a file or directory.
/// </summary>
public class DeleteCommand : IDirectoryBrowserCommand
{
    public string DisplayName => "Delete";

    public bool CanExecute(string path, bool isDirectory)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return isDirectory ? Directory.Exists(path) : File.Exists(path);
    }

    public Task ExecuteAsync(string path, bool isDirectory)
    {
        if (!CanExecute(path, isDirectory))
        {
            return Task.CompletedTask;
        }

        try
        {
            if (isDirectory)
            {
                Directory.Delete(path, recursive: true);
                Log.Information("Deleted directory: {Path}", path);
            }
            else
            {
                File.Delete(path);
                Log.Information("Deleted file: {Path}", path);
            }
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Failed to delete {PathType}: {Path}",
                isDirectory ? "directory" : "file", path);
        }

        return Task.CompletedTask;
    }
}
