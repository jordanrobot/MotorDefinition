using System;

namespace JordanRobot.MotorDefinition.Services;

/// <summary>
/// Provides utilities to correct minor floating-point precision artifacts by rounding
/// values that are within a configurable threshold of a rounded candidate.
/// </summary>
public static class PrecisionRounding
{
    /// <summary>
    /// Corrects minor floating-point precision errors when a value is sufficiently
    /// close to a rounded representation.
    /// </summary>
    /// <param name="value">The value to inspect for precision artifacts.</param>
    /// <param name="precisionErrorThreshold">The maximum delta allowed to treat the value as a rounding artifact. Use 0 to disable.</param>
    /// <returns>The corrected value when within threshold; otherwise, the original value.</returns>
    public static double CorrectPrecisionError(double value, double precisionErrorThreshold)
    {
        if (precisionErrorThreshold <= 0)
        {
            return value;
        }

        for (var decimals = 0; decimals <= 15; decimals++)
        {
            var rounded = Math.Round(value, decimals, MidpointRounding.AwayFromZero);
            var delta = Math.Abs(rounded - value);

            if (delta > 0 && delta <= precisionErrorThreshold)
            {
                return rounded;
            }
        }

        return value;
    }
}
