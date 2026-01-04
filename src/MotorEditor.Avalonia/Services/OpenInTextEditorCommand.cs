using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Command to open a file or directory in the system's default text editor.
/// </summary>
public class OpenInTextEditorCommand : IDirectoryBrowserCommand
{
    public string DisplayName => "Open in Text Editor";

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
            OpenInDefaultEditor(path);
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Failed to open {PathType} in text editor: {Path}",
                isDirectory ? "directory" : "file", path);
        }

        return Task.CompletedTask;
    }

    private static void OpenInDefaultEditor(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // notepad works for both files and directories (opens parent folder for directories)
            Process.Start(new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // open -t opens in default text editor
            Process.Start("open", $"-t \"{path}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // xdg-open uses the default text editor
            Process.Start("xdg-open", $"\"{path}\"");
        }
        else
        {
            Log.Information("Unsupported platform for opening in text editor");
        }
    }
}
