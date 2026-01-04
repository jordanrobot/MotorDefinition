using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Command to rename a file or directory.
/// </summary>
public class RenameCommand : IDirectoryBrowserCommand
{
    public string DisplayName => "Rename";

    public bool RequiresRefresh => true;

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
        // The actual rename logic is handled by the ViewModel through the IsRenaming state
        // This command just signals that rename mode should be entered
        return Task.CompletedTask;
    }

    /// <summary>
    /// Performs the actual rename operation.
    /// </summary>
    public static Task<bool> PerformRenameAsync(string oldPath, string newName, bool isDirectory)
    {
        if (string.IsNullOrWhiteSpace(oldPath) || string.IsNullOrWhiteSpace(newName))
        {
            return Task.FromResult(false);
        }

        try
        {
            var directory = Path.GetDirectoryName(oldPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return Task.FromResult(false);
            }

            var newPath = Path.Combine(directory, newName);

            // Check if the new name is the same as the old name
            if (string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(true);
            }

            // Check if target already exists
            if (isDirectory)
            {
                if (Directory.Exists(newPath))
                {
                    Log.Information("Cannot rename directory: target already exists {NewPath}", newPath);
                    return Task.FromResult(false);
                }
                Directory.Move(oldPath, newPath);
            }
            else
            {
                if (File.Exists(newPath))
                {
                    Log.Information("Cannot rename file: target already exists {NewPath}", newPath);
                    return Task.FromResult(false);
                }
                File.Move(oldPath, newPath);
            }

            Log.Information("Renamed {ItemType} from {OldPath} to {NewPath}",
                isDirectory ? "directory" : "file", oldPath, newPath);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Failed to rename {ItemType}: {Path}",
                isDirectory ? "directory" : "file", oldPath);
            return Task.FromResult(false);
        }
    }
}
