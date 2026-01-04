using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Command to copy a file or directory to the clipboard.
/// The file/directory can then be pasted in File Explorer.
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
            CopyToClipboard(path);
            Log.Information("Copied {ItemType} to clipboard: {Path}", 
                isDirectory ? "directory" : "file", path);
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Failed to copy {ItemType} to clipboard: {Path}",
                isDirectory ? "directory" : "file", path);
        }

        return Task.CompletedTask;
    }

    private static void CopyToClipboard(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: Use PowerShell to copy file/directory to clipboard
            var escapedPath = path.Replace("'", "''");
            var psCommand = $"Set-Clipboard -Path '{escapedPath}'";
            
            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -Command \"{psCommand}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            })?.WaitForExit();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS: Use osascript to copy file/directory
            var escapedPath = path.Replace("\"", "\\\"");
            var script = $"set the clipboard to POSIX file \"{escapedPath}\"";
            
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e '{script}'",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            process?.WaitForExit();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux: Copy URI to clipboard for file managers that support it
            var uri = $"file://{path}";
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = "-selection clipboard -t text/uri-list",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            
            if (process is not null)
            {
                process.StandardInput.WriteLine(uri);
                process.StandardInput.Close();
                process.WaitForExit();
            }
        }
        else
        {
            Log.Information("Unsupported platform for clipboard file operations");
        }
    }
}
