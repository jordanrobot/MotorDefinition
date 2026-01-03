using System;
using System.Globalization;

namespace CurveEditor.Services;

/// <summary>
/// Service for formatting numeric values according to user preferences.
/// Provides consistent number display formatting across the application.
/// </summary>
public class NumericFormattingService
{
    private readonly IUserPreferencesService _preferencesService;

    /// <summary>
    /// Creates a new instance of the NumericFormattingService.
    /// </summary>
    /// <param name="preferencesService">The user preferences service.</param>
    public NumericFormattingService(IUserPreferencesService preferencesService)
    {
        _preferencesService = preferencesService ?? throw new ArgumentNullException(nameof(preferencesService));
    }

    /// <summary>
    /// Gets the current decimal precision from user preferences.
    /// </summary>
    public int DecimalPrecision => _preferencesService.Preferences.DecimalPrecision;

    /// <summary>
    /// Formats a double value using the user's precision preference.
    /// Rounds to maximum decimal places but removes trailing zeros.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="excludeFromRounding">If true, formats with full precision (for values like Rotor Inertia).</param>
    /// <returns>A formatted string representation of the value.</returns>
    public string FormatNumber(double value, bool excludeFromRounding = false)
    {
        if (excludeFromRounding)
        {
            // Use invariant culture for full precision values
            return value.ToString(CultureInfo.InvariantCulture);
        }

        // Round to the specified precision
        var rounded = Math.Round(value, DecimalPrecision);
        
        // Format with the precision, then remove trailing zeros and decimal point if not needed
        var formatted = rounded.ToString($"F{DecimalPrecision}", CultureInfo.CurrentCulture);
        
        // Remove trailing zeros after decimal point
        if (formatted.Contains('.') || formatted.Contains(','))
        {
            formatted = formatted.TrimEnd('0').TrimEnd('.', ',');
        }
        
        return formatted;
    }

    /// <summary>
    /// Formats a double value using the "N" (number) format with thousand separators.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="excludeFromRounding">If true, formats with full precision (for values like Rotor Inertia).</param>
    /// <returns>A formatted string representation of the value with thousand separators.</returns>
    public string FormatNumberWithSeparators(double value, bool excludeFromRounding = false)
    {
        if (excludeFromRounding)
        {
            return value.ToString("N", CultureInfo.CurrentCulture);
        }

        var precision = DecimalPrecision;
        return value.ToString($"N{precision}", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Formats a double value using the "F" (fixed-point) format.
    /// Always shows the specified number of decimal places.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="excludeFromRounding">If true, formats with full precision (for values like Rotor Inertia).</param>
    /// <returns>A formatted string representation of the value.</returns>
    public string FormatFixedPoint(double value, bool excludeFromRounding = false)
    {
        if (excludeFromRounding)
        {
            return value.ToString("F", CultureInfo.CurrentCulture);
        }

        var precision = DecimalPrecision;
        return value.ToString($"F{precision}", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Gets a format string for the current decimal precision.
    /// </summary>
    /// <param name="formatType">The type of format: "N" (number with separators), "F" (fixed-point), or "G" (general).</param>
    /// <returns>A format string like "N2", "F2", or "G3".</returns>
    public string GetFormatString(string formatType = "N")
    {
        var precision = DecimalPrecision;
        
        if (formatType.Equals("G", StringComparison.OrdinalIgnoreCase))
        {
            // For "G" format, use significant figures
            return $"G{Math.Max(precision + 1, 1)}";
        }
        
        return $"{formatType}{precision}";
    }
}
