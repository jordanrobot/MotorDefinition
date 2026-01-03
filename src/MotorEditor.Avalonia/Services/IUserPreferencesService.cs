using MotorEditor.Avalonia.Models;
using System;

namespace CurveEditor.Services;

/// <summary>
/// Service for managing user preferences and application state.
/// </summary>
public interface IUserPreferencesService
{
    /// <summary>
    /// Gets the current user preferences.
    /// </summary>
    UserPreferences Preferences { get; }

    /// <summary>
    /// Gets the current application state.
    /// </summary>
    ApplicationState State { get; }

    /// <summary>
    /// Saves the current user preferences.
    /// </summary>
    void SavePreferences();

    /// <summary>
    /// Saves the current application state.
    /// </summary>
    void SaveState();

    /// <summary>
    /// Event raised when preferences change.
    /// </summary>
    event EventHandler? PreferencesChanged;

    /// <summary>
    /// Event raised when application state changes.
    /// </summary>
    event EventHandler? StateChanged;
}
