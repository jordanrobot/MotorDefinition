using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Command to reveal a file or directory in the system file explorer.
/// </summary>
public class RevealInFileExplorerCommand : IDirectoryBrowserCommand
{
    public string DisplayName => "Reveal in File Explorer";

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
                OpenDirectory(path);
            }
            else
            {
                RevealFile(path);
            }
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Failed to reveal {PathType} in file explorer: {Path}", 
                isDirectory ? "directory" : "file", path);
        }

        return Task.CompletedTask;
    }

    private static void OpenDirectory(string directoryPath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{directoryPath}\"",
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", $"\"{directoryPath}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", $"\"{directoryPath}\"");
        }
        else
        {
            Log.Information("Unsupported platform for opening directory");
        }
    }

    private static void RevealFile(string filePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // /select shows the parent folder and highlights the file
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{filePath}\"",
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // -R reveals the file in Finder
            Process.Start("open", $"-R \"{filePath}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Open parent directory as fallback (no standard way to select file)
            var parentDirectory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(parentDirectory))
            {
                Process.Start("xdg-open", $"\"{parentDirectory}\"");
            }
        }
        else
        {
            Log.Information("Unsupported platform for revealing file");
        }
    }
}
