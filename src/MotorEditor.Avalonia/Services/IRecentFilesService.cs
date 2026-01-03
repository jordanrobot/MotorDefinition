using System.Collections.Generic;

namespace CurveEditor.Services;

/// <summary>
/// Service interface for managing the recent files list.
/// </summary>
public interface IRecentFilesService
{
    /// <summary>
    /// Gets the list of recent file paths, ordered from most recent to oldest.
    /// </summary>
    IReadOnlyList<string> RecentFiles { get; }

    /// <summary>
    /// Adds a file to the recent files list.
    /// If the file already exists, it is moved to the top.
    /// The list is limited to 10 files maximum.
    /// </summary>
    /// <param name="filePath">The absolute path to the file.</param>
    void AddRecentFile(string filePath);

    /// <summary>
    /// Removes a file from the recent files list.
    /// </summary>
    /// <param name="filePath">The absolute path to the file.</param>
    void RemoveRecentFile(string filePath);

    /// <summary>
    /// Clears all recent files from the list.
    /// </summary>
    void ClearRecentFiles();
}
