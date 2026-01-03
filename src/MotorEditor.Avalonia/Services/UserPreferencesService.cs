using MotorEditor.Avalonia.Models;
using Serilog;
using System;
using System.IO;
using System.Text.Json;

namespace CurveEditor.Services;

/// <summary>
/// Service for managing user preferences and application state.
/// Persists settings to the AppData/MotorEditor folder.
/// </summary>
public sealed class UserPreferencesService : IUserPreferencesService
{
    private const string AppFolderName = "MotorEditor";
    private const string PreferencesFileName = "preferences.json";
    private const string StateFileName = "application-state.json";

    private UserPreferences _preferences;
    private ApplicationState _state;
    private readonly string? _customSettingsDirectory;

    /// <summary>
    /// Creates a new instance of the UserPreferencesService.
    /// </summary>
    /// <param name="customSettingsDirectory">Optional custom directory for testing purposes.</param>
    public UserPreferencesService(string? customSettingsDirectory = null)
    {
        _customSettingsDirectory = customSettingsDirectory;
        _preferences = LoadPreferences();
        _state = LoadState();
    }

    /// <inheritdoc />
    public UserPreferences Preferences => _preferences;

    /// <inheritdoc />
    public ApplicationState State => _state;

    /// <inheritdoc />
    public event EventHandler? PreferencesChanged;

    /// <inheritdoc />
    public event EventHandler? StateChanged;

    /// <inheritdoc />
    public void SavePreferences()
    {
        try
        {
            var directory = GetSettingsDirectory();
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, PreferencesFileName);
            var json = JsonSerializer.Serialize(_preferences, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
            Log.Debug("Saved user preferences to {Path}", path);

            PreferencesChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to save user preferences");
        }
    }

    /// <inheritdoc />
    public void SaveState()
    {
        try
        {
            var directory = GetSettingsDirectory();
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, StateFileName);
            var json = JsonSerializer.Serialize(_state, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
            Log.Debug("Saved application state to {Path}", path);

            StateChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to save application state");
        }
    }

    private UserPreferences LoadPreferences()
    {
        try
        {
            var path = Path.Combine(GetSettingsDirectory(), PreferencesFileName);
            if (!File.Exists(path))
            {
                Log.Debug("No preferences file found, using defaults");
                return new UserPreferences();
            }

            var json = File.ReadAllText(path);
            var preferences = JsonSerializer.Deserialize<UserPreferences>(json);
            if (preferences is null)
            {
                Log.Warning("Failed to deserialize preferences, using defaults");
                return new UserPreferences();
            }

            Log.Debug("Loaded user preferences from {Path}", path);
            return preferences;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load user preferences, using defaults");
            return new UserPreferences();
        }
    }

    private ApplicationState LoadState()
    {
        try
        {
            var path = Path.Combine(GetSettingsDirectory(), StateFileName);
            if (!File.Exists(path))
            {
                Log.Debug("No application state file found, using defaults");
                return new ApplicationState();
            }

            var json = File.ReadAllText(path);
            var state = JsonSerializer.Deserialize<ApplicationState>(json);
            if (state is null)
            {
                Log.Warning("Failed to deserialize application state, using defaults");
                return new ApplicationState();
            }

            Log.Debug("Loaded application state from {Path}", path);
            return state;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load application state, using defaults");
            return new ApplicationState();
        }
    }

    private string GetSettingsDirectory()
    {
        if (_customSettingsDirectory is not null)
        {
            return _customSettingsDirectory;
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, AppFolderName);
    }
}
