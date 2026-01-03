using System.Collections.Generic;

namespace MotorEditor.Avalonia.Models;

/// <summary>
/// Represents implicit user preferences - application state that is captured
/// and restored to provide continuity between sessions.
/// </summary>
public sealed class ApplicationState
{
    /// <summary>
    /// Currently open file path.
    /// </summary>
    public string? CurrentFilePath { get; set; }

    /// <summary>
    /// Currently open folder path in the directory browser.
    /// </summary>
    public string? CurrentFolderPath { get; set; }

    /// <summary>
    /// List of currently open file tabs.
    /// </summary>
    public List<string> OpenTabs { get; set; } = new();

    /// <summary>
    /// Visibility state for power curves in the chart.
    /// </summary>
    public bool ShowPowerCurves { get; set; } = true;

    /// <summary>
    /// Visibility of the directory browser panel.
    /// </summary>
    public bool IsBrowserPanelVisible { get; set; } = true;

    /// <summary>
    /// Visibility of the properties panel.
    /// </summary>
    public bool IsPropertiesPanelVisible { get; set; } = true;

    /// <summary>
    /// Visibility of the curve data panel.
    /// </summary>
    public bool IsCurveDataPanelVisible { get; set; } = true;

    /// <summary>
    /// Visibility of the bottom panel.
    /// </summary>
    public bool IsBottomPanelVisible { get; set; } = true;

    /// <summary>
    /// Width of the directory browser panel.
    /// </summary>
    public double? BrowserPanelWidth { get; set; }

    /// <summary>
    /// Width of the properties panel.
    /// </summary>
    public double? PropertiesPanelWidth { get; set; }

    /// <summary>
    /// Width of the curve data panel.
    /// </summary>
    public double? CurveDataPanelWidth { get; set; }

    /// <summary>
    /// Height of the bottom panel.
    /// </summary>
    public double? BottomPanelHeight { get; set; }

    /// <summary>
    /// Main window width.
    /// </summary>
    public double? WindowWidth { get; set; }

    /// <summary>
    /// Main window height.
    /// </summary>
    public double? WindowHeight { get; set; }

    /// <summary>
    /// Main window X position.
    /// </summary>
    public int? WindowX { get; set; }

    /// <summary>
    /// Main window Y position.
    /// </summary>
    public int? WindowY { get; set; }

    /// <summary>
    /// Main window state (Normal, Maximized, Minimized).
    /// </summary>
    public string? WindowState { get; set; }

    /// <summary>
    /// Creates a copy of the current application state.
    /// </summary>
    public ApplicationState Clone()
    {
        return new ApplicationState
        {
            CurrentFilePath = CurrentFilePath,
            CurrentFolderPath = CurrentFolderPath,
            OpenTabs = new List<string>(OpenTabs),
            ShowPowerCurves = ShowPowerCurves,
            IsBrowserPanelVisible = IsBrowserPanelVisible,
            IsPropertiesPanelVisible = IsPropertiesPanelVisible,
            IsCurveDataPanelVisible = IsCurveDataPanelVisible,
            IsBottomPanelVisible = IsBottomPanelVisible,
            BrowserPanelWidth = BrowserPanelWidth,
            PropertiesPanelWidth = PropertiesPanelWidth,
            CurveDataPanelWidth = CurveDataPanelWidth,
            BottomPanelHeight = BottomPanelHeight,
            WindowWidth = WindowWidth,
            WindowHeight = WindowHeight,
            WindowX = WindowX,
            WindowY = WindowY,
            WindowState = WindowState
        };
    }
}
