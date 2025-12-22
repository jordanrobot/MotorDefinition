using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace CurveEditor.Services;

/// <summary>
/// Enumerates directories and <c>*.json</c> files for the Directory Browser explorer tree.
/// </summary>
public sealed class DirectoryBrowserService : IDirectoryBrowserService
{
    public Task<IReadOnlyList<DirectoryBrowserEntry>> GetChildrenAsync(string directoryPath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        return Task.Run<IReadOnlyList<DirectoryBrowserEntry>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var entries = new List<DirectoryBrowserEntry>();

                foreach (var dir in Directory.EnumerateDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    entries.Add(new DirectoryBrowserEntry(Name: Path.GetFileName(dir), FullPath: dir, IsDirectory: true));
                }

                foreach (var file in Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    entries.Add(new DirectoryBrowserEntry(Name: Path.GetFileName(file), FullPath: file, IsDirectory: false));
                }

                return entries
                    .OrderByDescending(e => e.IsDirectory)
                    .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information(ex, "Access denied enumerating directory browser children for {DirectoryPath}", directoryPath);
                return Array.Empty<DirectoryBrowserEntry>();
            }
            catch (IOException ex)
            {
                Log.Information(ex, "I/O error enumerating directory browser children for {DirectoryPath}", directoryPath);
                return Array.Empty<DirectoryBrowserEntry>();
            }
            catch (Exception ex)
            {
                Log.Information(ex, "Failed to enumerate directory browser children for {DirectoryPath}", directoryPath);
                return Array.Empty<DirectoryBrowserEntry>();
            }
        }, cancellationToken);
    }
}
