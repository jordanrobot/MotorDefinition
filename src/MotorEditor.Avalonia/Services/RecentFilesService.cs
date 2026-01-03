using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Serilog;

namespace CurveEditor.Services;

/// <summary>
/// Service for managing the recent files list.
/// Persists the list using the user settings store.
/// </summary>
public class RecentFilesService : IRecentFilesService
{
    private const string SettingsKey = "RecentFiles";
    private const int MaxRecentFiles = 10;

    private readonly IUserSettingsStore _settingsStore;
    private readonly ObservableCollection<string> _recentFiles;

    /// <summary>
    /// Creates a new RecentFilesService instance.
    /// </summary>
    /// <param name="settingsStore">The settings store for persisting the recent files list.</param>
    public RecentFilesService(IUserSettingsStore settingsStore)
    {
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        _recentFiles = new ObservableCollection<string>();
        LoadRecentFiles();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> RecentFiles => _recentFiles;

    /// <inheritdoc />
    public void AddRecentFile(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        try
        {
            // Normalize the path to avoid duplicates with different casing or separators
            var normalizedPath = Path.GetFullPath(filePath);

            // Remove if already exists (to move it to top)
            var existingIndex = _recentFiles.IndexOf(normalizedPath);
            if (existingIndex >= 0)
            {
                _recentFiles.RemoveAt(existingIndex);
            }

            // Add to the top of the list
            _recentFiles.Insert(0, normalizedPath);

            // Trim to max size
            while (_recentFiles.Count > MaxRecentFiles)
            {
                _recentFiles.RemoveAt(_recentFiles.Count - 1);
            }

            SaveRecentFiles();
            Log.Debug("Added file to recent files: {FilePath}", normalizedPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to add file to recent files: {FilePath}", filePath);
        }
    }

    /// <inheritdoc />
    public void RemoveRecentFile(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        try
        {
            var normalizedPath = Path.GetFullPath(filePath);
            if (_recentFiles.Remove(normalizedPath))
            {
                SaveRecentFiles();
                Log.Debug("Removed file from recent files: {FilePath}", normalizedPath);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to remove file from recent files: {FilePath}", filePath);
        }
    }

    /// <inheritdoc />
    public void ClearRecentFiles()
    {
        try
        {
            _recentFiles.Clear();
            SaveRecentFiles();
            Log.Debug("Cleared recent files list");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to clear recent files");
        }
    }

    private void LoadRecentFiles()
    {
        try
        {
            var files = _settingsStore.LoadStringArrayFromJson(SettingsKey);
            
            // Filter out files that no longer exist and take only the first MaxRecentFiles
            var validFiles = files
                .Where(File.Exists)
                .Take(MaxRecentFiles)
                .ToList();

            foreach (var file in validFiles)
            {
                _recentFiles.Add(file);
            }

            Log.Debug("Loaded {Count} recent files", _recentFiles.Count);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load recent files, starting with empty list");
        }
    }

    private void SaveRecentFiles()
    {
        try
        {
            _settingsStore.SaveStringArrayAsJson(SettingsKey, _recentFiles);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to save recent files");
        }
    }
}
