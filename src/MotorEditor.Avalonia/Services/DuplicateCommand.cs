using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Command to duplicate a file or directory with a "_copy" suffix.
/// </summary>
public class DuplicateCommand : IDirectoryBrowserCommand
{
    public string DisplayName => "Duplicate";

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
                DuplicateDirectory(path);
            }
            else
            {
                DuplicateFile(path);
            }
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Failed to duplicate {PathType}: {Path}",
                isDirectory ? "directory" : "file", path);
        }

        return Task.CompletedTask;
    }

    private static void DuplicateFile(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        var newPath = Path.Combine(directory, $"{fileName}_copy{extension}");

        // If the copy already exists, add a number
        var counter = 1;
        while (File.Exists(newPath))
        {
            newPath = Path.Combine(directory, $"{fileName}_copy{counter}{extension}");
            counter++;
        }

        File.Copy(filePath, newPath);
        Log.Information("Duplicated file: {OriginalPath} -> {NewPath}", filePath, newPath);
    }

    private static void DuplicateDirectory(string directoryPath)
    {
        var parentDirectory = Path.GetDirectoryName(directoryPath);
        var directoryName = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        if (string.IsNullOrWhiteSpace(parentDirectory) || string.IsNullOrWhiteSpace(directoryName))
        {
            return;
        }

        var newPath = Path.Combine(parentDirectory, $"{directoryName}_copy");

        // If the copy already exists, add a number
        var counter = 1;
        while (Directory.Exists(newPath))
        {
            newPath = Path.Combine(parentDirectory, $"{directoryName}_copy{counter}");
            counter++;
        }

        CopyDirectory(directoryPath, newPath);
        Log.Information("Duplicated directory: {OriginalPath} -> {NewPath}", directoryPath, newPath);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
        }

        Directory.CreateDirectory(destinationDir);

        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        foreach (var subDir in dir.GetDirectories())
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }
}
