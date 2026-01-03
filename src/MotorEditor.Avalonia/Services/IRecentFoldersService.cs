using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CurveEditor.Services;

/// <summary>
/// Service interface for managing the recent folders list.
/// </summary>
public interface IRecentFoldersService
{
    /// <summary>
    /// Gets the observable list of recent folder paths, ordered from most recent to oldest.
    /// </summary>
    ReadOnlyObservableCollection<string> RecentFolders { get; }

    /// <summary>
    /// Adds a folder to the recent folders list.
    /// If the folder already exists, it is moved to the top.
    /// The list is limited to 10 folders maximum.
    /// </summary>
    /// <param name="folderPath">The absolute path to the folder.</param>
    void AddRecentFolder(string folderPath);

    /// <summary>
    /// Removes a folder from the recent folders list.
    /// </summary>
    /// <param name="folderPath">The absolute path to the folder.</param>
    void RemoveRecentFolder(string folderPath);

    /// <summary>
    /// Clears all recent folders from the list.
    /// </summary>
    void ClearRecentFolders();
}
