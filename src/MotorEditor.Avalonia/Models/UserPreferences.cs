using System.Collections.Generic;

namespace MotorEditor.Avalonia.Models;

/// <summary>
/// Represents explicit user preferences that the user intentionally sets.
/// These are controlled via the Preferences UI.
/// </summary>
public sealed class UserPreferences
{
    /// <summary>
    /// Decimal precision for displaying numeric values (e.g., 2 for "0.00").
    /// </summary>
    public int DecimalPrecision { get; set; } = 2;

    /// <summary>
    /// Theme preference: "Light" or "Dark".
    /// </summary>
    public string Theme { get; set; } = "Light";

    /// <summary>
    /// List of color hex codes for curve colors.
    /// </summary>
    public List<string> CurveColors { get; set; } = new()
    {
        "#FF0000", // Red
        "#00FF00", // Green
        "#0000FF", // Blue
        "#FFFF00", // Yellow
        "#FF00FF", // Magenta
        "#00FFFF", // Cyan
        "#FFA500", // Orange
        "#800080"  // Purple
    };

    /// <summary>
    /// Whether to show the Motor Rated Speed reference line on the chart.
    /// </summary>
    public bool ShowMotorRatedSpeedLine { get; set; } = true;

    /// <summary>
    /// Whether to show the Voltage Max Speed reference line on the chart.
    /// </summary>
    public bool ShowVoltageMaxSpeedLine { get; set; } = true;

    /// <summary>
    /// Creates a copy of the current preferences.
    /// </summary>
    public UserPreferences Clone()
    {
        return new UserPreferences
        {
            DecimalPrecision = DecimalPrecision,
            Theme = Theme,
            CurveColors = new List<string>(CurveColors),
            ShowMotorRatedSpeedLine = ShowMotorRatedSpeedLine,
            ShowVoltageMaxSpeedLine = ShowVoltageMaxSpeedLine
        };
    }
}
