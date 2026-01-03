using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;

namespace CurveEditor.Behaviors;

/// <summary>
/// Persists and restores a window's size, position, and state across application runs.
/// </summary>
public static class WindowBoundsPersistence
{
    private const string AppFolderName = "MotorEditor";

    private sealed class WindowSettings
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public WindowState State { get; set; }
    }

    /// <summary>
    /// Attach persistence behavior to the specified window.
    /// </summary>
    /// <param name="window">The window to track.</param>
    /// <param name="settingsKey">Logical key for the window, used in the settings file name.</param>
    public static void Attach(Window window, string settingsKey)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(settingsKey);

        try
        {
            var settings = Load(settingsKey);
            if (settings is not null)
            {
                if (settings.Width > 0 && settings.Height > 0)
                {
                    window.Width = settings.Width;
                    window.Height = settings.Height;
                }

                if (settings.X >= 0 && settings.Y >= 0)
                {
                    window.Position = new PixelPoint(settings.X, settings.Y);
                }

                window.WindowState = settings.State;
            }
        }
        catch
        {
            // Ignore any issues restoring settings; fall back to defaults.
        }

        window.Closing += (_, _) => SaveSafe(window, settingsKey);
    }

    private static void SaveSafe(Window window, string settingsKey)
    {
        try
        {
            Save(window, settingsKey);
        }
        catch
        {
            // Never let persistence failures crash the app.
        }
    }

    private static void Save(Window window, string settingsKey)
    {
        var rect = window.Bounds;
        var position = window.Position;

        var settings = new WindowSettings
        {
            Width = rect.Width,
            Height = rect.Height,
            X = position.X,
            Y = position.Y,
            State = window.WindowState
        };

        var directory = GetSettingsDirectory();
        Directory.CreateDirectory(directory);

        var path = GetSettingsPath(settingsKey);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }

    private static WindowSettings? Load(string settingsKey)
    {
        var path = GetSettingsPath(settingsKey);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<WindowSettings>(json);
    }

    private static string GetSettingsDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, AppFolderName);
    }

    private static string GetSettingsPath(string settingsKey)
    {
        var directory = GetSettingsDirectory();
        var fileName = $"window-{settingsKey}.json";
        return Path.Combine(directory, fileName);
    }
}
