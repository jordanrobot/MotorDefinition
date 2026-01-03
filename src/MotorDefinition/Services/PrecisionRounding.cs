using System;

namespace JordanRobot.MotorDefinition.Services;

/// <summary>
/// Provides utility methods for detecting and correcting floating-point precision errors
/// that occur during unit conversions and mathematical operations.
/// </summary>
public static class PrecisionRounding
{
    /// <summary>
    /// Default threshold for detecting precision errors (1e-10).
    /// </summary>
    public const double DefaultThreshold = 1e-10;

    /// <summary>
    /// Corrects floating-point precision errors by rounding values that are very close to
    /// round numbers based on the specified threshold.
    /// </summary>
    /// <param name="value">The value to potentially round.</param>
    /// <param name="threshold">
    /// The threshold for detecting precision errors. If the fractional part of the value
    /// is smaller than this threshold from a round number, it will be rounded.
    /// Default is 1e-10. Set to 0 or negative to disable rounding.
    /// </param>
    /// <returns>
    /// The corrected value with precision errors removed, or the original value if no
    /// correction is needed or threshold is disabled.
    /// </returns>
    /// <remarks>
    /// This method helps fix conversion rounding errors like:
    /// - 50.1300000000034 -> 50.13
    /// - 1.4999999999998 -> 1.5
    /// - 6.2000000000001 -> 6.2
    /// 
    /// The method works by determining a reasonable number of decimal places for the value,
    /// then checking if the difference between the original and rounded value is within
    /// the threshold. This approach preserves legitimate precision while fixing errors.
    /// </remarks>
    public static double CorrectPrecisionError(double value, double threshold = DefaultThreshold)
    {
        // If threshold is disabled or value is special, return as-is
        if (threshold <= 0 || double.IsNaN(value) || double.IsInfinity(value) || value == 0)
        {
            return value;
        }

        // Determine a reasonable number of decimal places to check
        // We'll check multiple decimal place values to find precision errors
        for (int decimalPlaces = 0; decimalPlaces <= 15; decimalPlaces++)
        {
            double rounded = Math.Round(value, decimalPlaces);
            double difference = Math.Abs(value - rounded);

            // If the difference is within the threshold, this is likely a precision error
            if (difference > 0 && difference < threshold)
            {
                return rounded;
            }

            // If we've already found an exact match, no need to continue
            if (difference == 0)
            {
                break;
            }
        }

        // No precision error detected, return original value
        return value;
    }

    /// <summary>
    /// Corrects floating-point precision errors in an array of values.
    /// </summary>
    /// <param name="values">The array of values to correct.</param>
    /// <param name="threshold">The threshold for detecting precision errors.</param>
    /// <returns>A new array with precision errors corrected.</returns>
    public static double[] CorrectPrecisionErrors(double[] values, double threshold = DefaultThreshold)
    {
        ArgumentNullException.ThrowIfNull(values);

        var result = new double[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = CorrectPrecisionError(values[i], threshold);
        }
        return result;
    }
}
