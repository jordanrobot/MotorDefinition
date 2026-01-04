using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Command to create a new subdirectory in the selected directory.
/// </summary>
public class NewDirectoryCommand : IDirectoryBrowserCommand
{
    public string DisplayName => "New Directory";

    public bool CanExecute(string path, bool isDirectory)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        // Only enable for directories
        return isDirectory && Directory.Exists(path);
    }

    public Task ExecuteAsync(string path, bool isDirectory)
    {
        if (!CanExecute(path, isDirectory))
        {
            return Task.CompletedTask;
        }

        try
        {
            var newDirName = "NewFolder";
            var newDirPath = Path.Combine(path, newDirName);

            // If the directory already exists, add a number
            var counter = 1;
            while (Directory.Exists(newDirPath))
            {
                newDirPath = Path.Combine(path, $"{newDirName}{counter}");
                counter++;
            }

            Directory.CreateDirectory(newDirPath);
            Log.Information("Created new directory: {Path}", newDirPath);
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Failed to create new directory in: {Path}", path);
        }

        return Task.CompletedTask;
    }
}
