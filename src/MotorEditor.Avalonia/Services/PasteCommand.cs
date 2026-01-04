using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Command to paste files or directories from the clipboard to a target directory.
/// </summary>
public class PasteCommand : IDirectoryBrowserCommand
{
    public string DisplayName => "Paste";

    public bool RequiresRefresh => true;

    public bool CanExecute(string path, bool isDirectory)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        // Paste can only be executed on directories
        return isDirectory && Directory.Exists(path);
    }

    public async Task ExecuteAsync(string path, bool isDirectory)
    {
        if (!CanExecute(path, isDirectory))
        {
            return;
        }

        try
        {
            await PasteFromClipboardAsync(path).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Failed to paste to directory: {Path}", path);
        }
    }

    private static async Task PasteFromClipboardAsync(string targetDirectory)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: Use PowerShell to paste from clipboard
            var psCommand = @"
                $files = Get-Clipboard -Format FileDropList
                if ($files) {
                    foreach ($file in $files) {
                        $dest = Join-Path '" + targetDirectory.Replace("'", "''") + @"' (Split-Path $file -Leaf)
                        if (Test-Path $file -PathType Container) {
                            Copy-Item -Path $file -Destination $dest -Recurse -Force
                        } else {
                            Copy-Item -Path $file -Destination $dest -Force
                        }
                    }
                }";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -Command \"{psCommand}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            });

            if (process is not null)
            {
                await process.WaitForExitAsync().ConfigureAwait(false);
                Log.Information("Pasted files to directory: {TargetDirectory}", targetDirectory);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS: Use osascript to paste from clipboard
            var script = $@"
                set targetDir to POSIX file ""{targetDirectory}""
                tell application ""Finder""
                    set clipboardItems to (the clipboard as list)
                    repeat with anItem in clipboardItems
                        try
                            duplicate anItem to targetDir
                        end try
                    end repeat
                end tell";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e '{script}'",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            if (process is not null)
            {
                await process.WaitForExitAsync().ConfigureAwait(false);
                Log.Information("Pasted files to directory: {TargetDirectory}", targetDirectory);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux: Read URI list from clipboard and copy files
            // This is more complex and requires reading from xclip and parsing URIs
            Log.Information("Paste operation not fully supported on Linux platform");
        }
        else
        {
            Log.Information("Unsupported platform for paste operations");
        }
    }
}
