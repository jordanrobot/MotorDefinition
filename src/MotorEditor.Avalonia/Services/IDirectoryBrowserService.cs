using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Enumerates filesystem entries for the Directory Browser explorer tree.
/// </summary>
public interface IDirectoryBrowserService
{
    /// <summary>
    /// Lists the immediate child directories and <c>*.json</c> files of <paramref name="directoryPath"/>.
    /// </summary>
    Task<IReadOnlyList<DirectoryBrowserEntry>> GetChildrenAsync(string directoryPath, CancellationToken cancellationToken);
}

/// <summary>
/// A directory browser entry (directory or file) returned by <see cref="IDirectoryBrowserService"/>.
/// </summary>
public sealed record DirectoryBrowserEntry(string Name, string FullPath, bool IsDirectory);
