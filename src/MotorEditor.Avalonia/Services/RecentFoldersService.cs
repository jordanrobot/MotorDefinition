using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Serilog;

namespace CurveEditor.Services;

/// <summary>
/// Service for managing the recent folders list.
/// Persists the list using the user settings store.
/// </summary>
public class RecentFoldersService : IRecentFoldersService
{
    private const string SettingsKey = "RecentFolders";
    private const int MaxRecentFolders = 10;

    private readonly IUserSettingsStore _settingsStore;
    private readonly ObservableCollection<string> _recentFolders;
    private readonly ReadOnlyObservableCollection<string> _recentFoldersReadOnly;

    /// <summary>
    /// Creates a new RecentFoldersService instance.
    /// </summary>
    /// <param name="settingsStore">The settings store for persisting the recent folders list.</param>
    public RecentFoldersService(IUserSettingsStore settingsStore)
    {
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        _recentFolders = new ObservableCollection<string>();
        _recentFoldersReadOnly = new ReadOnlyObservableCollection<string>(_recentFolders);
        LoadRecentFolders();
    }

    /// <inheritdoc />
    public ReadOnlyObservableCollection<string> RecentFolders => _recentFoldersReadOnly;

    /// <inheritdoc />
    public void AddRecentFolder(string folderPath)
    {
        ArgumentNullException.ThrowIfNull(folderPath);

        try
        {
            // Normalize the path to avoid duplicates with different casing or separators
            var normalizedPath = Path.GetFullPath(folderPath);

            // Remove if already exists (to move it to top)
            var existingIndex = _recentFolders.IndexOf(normalizedPath);
            if (existingIndex >= 0)
            {
                _recentFolders.RemoveAt(existingIndex);
            }

            // Add to the top of the list
            _recentFolders.Insert(0, normalizedPath);

            // Trim to max size
            while (_recentFolders.Count > MaxRecentFolders)
            {
                _recentFolders.RemoveAt(_recentFolders.Count - 1);
            }

            SaveRecentFolders();
            Log.Debug("Added folder to recent folders: {FolderPath}", normalizedPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to add folder to recent folders: {FolderPath}", folderPath);
        }
    }

    /// <inheritdoc />
    public void RemoveRecentFolder(string folderPath)
    {
        ArgumentNullException.ThrowIfNull(folderPath);

        try
        {
            var normalizedPath = Path.GetFullPath(folderPath);
            if (_recentFolders.Remove(normalizedPath))
            {
                SaveRecentFolders();
                Log.Debug("Removed folder from recent folders: {FolderPath}", normalizedPath);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to remove folder from recent folders: {FolderPath}", folderPath);
        }
    }

    /// <inheritdoc />
    public void ClearRecentFolders()
    {
        try
        {
            _recentFolders.Clear();
            SaveRecentFolders();
            Log.Debug("Cleared recent folders list");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to clear recent folders");
        }
    }

    private void LoadRecentFolders()
    {
        try
        {
            var folders = _settingsStore.LoadStringArrayFromJson(SettingsKey);
            
            // Filter out folders that no longer exist and take only the first MaxRecentFolders
            var validFolders = folders
                .Where(Directory.Exists)
                .Take(MaxRecentFolders)
                .ToList();

            foreach (var folder in validFolders)
            {
                _recentFolders.Add(folder);
            }

            Log.Debug("Loaded {Count} recent folders", _recentFolders.Count);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load recent folders, starting with empty list");
        }
    }

    private void SaveRecentFolders()
    {
        try
        {
            _settingsStore.SaveStringArrayAsJson(SettingsKey, _recentFolders);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to save recent folders");
        }
    }
}
