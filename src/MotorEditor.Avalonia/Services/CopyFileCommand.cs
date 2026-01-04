using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Command to copy the file path to the clipboard.
/// Note: This command copies the file path as text, not the file contents.
/// Clipboard access in Avalonia requires TopLevel context which is not available in commands.
/// This implementation uses platform-specific clipboard APIs.
/// </summary>
public class CopyFileCommand : IDirectoryBrowserCommand
{
    public string DisplayName => "Copy File";

    public bool CanExecute(string path, bool isDirectory)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        // Only enable for files, not directories
        return !isDirectory && File.Exists(path);
    }

    public Task ExecuteAsync(string path, bool isDirectory)
    {
        if (!CanExecute(path, isDirectory))
        {
            return Task.CompletedTask;
        }

        try
        {
            CopyToClipboard(path);
            Log.Information("Copied file path to clipboard: {Path}", path);
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Failed to copy file path to clipboard: {Path}", path);
        }

        return Task.CompletedTask;
    }

    private static void CopyToClipboard(string text)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Use clip.exe on Windows
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c echo {text} | clip",
                CreateNoWindow = true,
                UseShellExecute = false
            })?.WaitForExit();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Use pbcopy on macOS
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "pbcopy",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            if (process is not null)
            {
                process.StandardInput.Write(text);
                process.StandardInput.Close();
                process.WaitForExit();
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Use xclip on Linux (if available)
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = "-selection clipboard",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            if (process is not null)
            {
                process.StandardInput.Write(text);
                process.StandardInput.Close();
                process.WaitForExit();
            }
        }
        else
        {
            Log.Information("Unsupported platform for clipboard operations");
        }
    }
}
