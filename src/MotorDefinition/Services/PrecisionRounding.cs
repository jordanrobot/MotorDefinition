namespace JordanRobot.MotorDefinition.Services;

/// <summary>
/// Provides helpers to correct floating-point precision artifacts introduced during calculations.
/// </summary>
public static class PrecisionRounding
{
    private const int MaxDecimalPlaces = 15;

    /// <summary>
    /// Corrects likely floating-point precision errors when a value is within the specified threshold of a rounded representation.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <param name="threshold">The maximum delta tolerated before treating the value as a precision error.</param>
    /// <returns>The normalized value when a precision error is detected; otherwise, the original value.</returns>
    public static double CorrectPrecisionError(double value, double threshold = 1e-10)
    {
        if (threshold <= 0 || double.IsNaN(value) || double.IsInfinity(value))
        {
            return value;
        }

        for (var decimals = 0; decimals <= MaxDecimalPlaces; decimals++)
        {
            var rounded = Math.Round(value, decimals, MidpointRounding.ToEven);
            var delta = Math.Abs(value - rounded);

            if (delta <= threshold)
            {
                return rounded;
            }
        }

        return value;
    }
}
