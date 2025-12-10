using System;
using System.IO;
using System.Text.Json;
using Avalonia.Controls;

namespace CurveEditor.Behaviors;

/// <summary>
/// Persists and restores layout information (size and expanded state)
/// for panels such as sidebars and data tables.
/// </summary>
public static class PanelLayoutPersistence
{
    private const string AppFolderName = "CurveEditor";

    private sealed class PanelLayoutSettings
    {
        public double? Width { get; set; }
        public double? Height { get; set; }
        public bool? IsExpanded { get; set; }
    }

    /// <summary>
    /// Attach persistence for a grid column (typically a side panel).
    /// </summary>
    public static void AttachColumn(Window window, Grid grid, int columnIndex, string settingsKey)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(grid);

        if (columnIndex < 0 || columnIndex >= grid.ColumnDefinitions.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(columnIndex));
        }

        try
        {
            var settings = Load(settingsKey);
            if (settings?.Width is double width && width > 0)
            {
                grid.ColumnDefinitions[columnIndex].Width = new GridLength(width, GridUnitType.Pixel);
            }
        }
        catch
        {
            // Ignore restore issues and fall back to XAML defaults.
        }

        window.Closing += (_, _) =>
        {
            try
            {
                var column = grid.ColumnDefinitions[columnIndex];
                var actualWidth = column.ActualWidth > 0 ? column.ActualWidth : column.Width.Value;
                // If the column is fully collapsed (width 0), keep the last
                // non-zero width stored on disk so we can restore it next run.
                if (actualWidth > 0)
                {
                    Save(settingsKey, s => s.Width = actualWidth);
                }
            }
            catch
            {
                // Never let persistence failures crash the app.
            }
        };
    }

    /// <summary>
    /// Attach persistence for a grid row (typically a horizontal panel like the data table).
    /// If <paramref name="isExpandedPredicate"/> is provided, height is only
    /// persisted when it evaluates to <c>true</c> (for example, when a panel
    /// is expanded).
    /// </summary>
    public static void AttachRow(
        Window window,
        Grid grid,
        int rowIndex,
        string settingsKey,
        Func<bool>? isExpandedPredicate = null)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(grid);

        if (rowIndex < 0 || rowIndex >= grid.RowDefinitions.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(rowIndex));
        }

        try
        {
            var settings = Load(settingsKey);
            if (settings?.Height is double height && height > 0)
            {
                grid.RowDefinitions[rowIndex].Height = new GridLength(height, GridUnitType.Pixel);
            }
        }
        catch
        {
            // Ignore restore issues and fall back to XAML defaults.
        }

        window.Closing += (_, _) =>
        {
            try
            {
                if (isExpandedPredicate is not null && !isExpandedPredicate())
                {
                    return;
                }

                var row = grid.RowDefinitions[rowIndex];
                var actualHeight = row.ActualHeight > 0 ? row.ActualHeight : row.Height.Value;
                if (actualHeight > 0)
                {
                    Save(settingsKey, s => s.Height = actualHeight);
                }
            }
            catch
            {
                // Never let persistence failures crash the app.
            }
        };
    }

    /// <summary>
    /// Immediately update the stored width for a panel column.
    /// </summary>
    public static void UpdateColumnWidth(string settingsKey, double width)
    {
        if (width <= 0)
        {
            return;
        }

        Save(settingsKey, s => s.Width = width);
    }

    /// <summary>
    /// Immediately update the stored height for a panel row.
    /// </summary>
    public static void UpdateRowHeight(string settingsKey, double height)
    {
        if (height <= 0)
        {
            return;
        }

        Save(settingsKey, s => s.Height = height);
    }

    /// <summary>
    /// Attach persistence for a simple expanded/collapsed state.
    /// </summary>
    public static void AttachBoolean(Window window, Func<bool> getValue, Action<bool> setValue, string settingsKey)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(getValue);
        ArgumentNullException.ThrowIfNull(setValue);

        try
        {
            var settings = Load(settingsKey);
            if (settings?.IsExpanded is bool isExpanded)
            {
                setValue(isExpanded);
            }
        }
        catch
        {
            // Ignore restore issues and keep existing value.
        }

        window.Closing += (_, _) =>
        {
            try
            {
                var current = getValue();
                Save(settingsKey, s => s.IsExpanded = current);
            }
            catch
            {
                // Never let persistence failures crash the app.
            }
        };
    }

    private static void Save(string settingsKey, Action<PanelLayoutSettings> applyChanges)
    {
        var directory = GetSettingsDirectory();
        Directory.CreateDirectory(directory);

        var path = GetSettingsPath(settingsKey);

        PanelLayoutSettings settings;
        if (File.Exists(path))
        {
            var existingJson = File.ReadAllText(path);
            settings = JsonSerializer.Deserialize<PanelLayoutSettings>(existingJson) ?? new PanelLayoutSettings();
        }
        else
        {
            settings = new PanelLayoutSettings();
        }

        applyChanges(settings);

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }

    private static PanelLayoutSettings? Load(string settingsKey)
    {
        var path = GetSettingsPath(settingsKey);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<PanelLayoutSettings>(json);
    }

    private static string GetSettingsDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, AppFolderName);
    }

    private static string GetSettingsPath(string settingsKey)
    {
        var safeKey = settingsKey
            .Replace(Path.DirectorySeparatorChar, '_')
            .Replace(Path.AltDirectorySeparatorChar, '_');

        return Path.Combine(GetSettingsDirectory(), $"layout-{safeKey}.json");
    }
}
