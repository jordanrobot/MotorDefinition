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
    /// Threshold for detecting and correcting floating-point precision errors in unit conversions.
    /// Values with fractional parts smaller than this threshold near round numbers will be rounded.
    /// Default is 1e-10 (0.0000000001).
    /// </summary>
    /// <remarks>
    /// This helps fix conversion rounding errors like 50.1300000000034 -> 50.13.
    /// Set to 0 to disable precision error correction.
    /// </remarks>
    public double PrecisionErrorThreshold { get; set; } = 1e-10;

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
            PrecisionErrorThreshold = PrecisionErrorThreshold
        };
    }
}
