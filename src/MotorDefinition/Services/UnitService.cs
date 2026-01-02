using Tare;

namespace JordanRobot.MotorDefinition.Services;

/// <summary>
/// Provides unit conversion and management services using the Tare library.
/// Maps between the application's unit string labels and Tare's Quantity-based unit system.
/// </summary>
public class UnitService
{
    /// <summary>
    /// Gets the supported torque units.
    /// </summary>
    public static string[] SupportedTorqueUnits => ["Nm", "lbf-ft", "lbf-in", "oz-in"];

    /// <summary>
    /// Gets the supported speed units.
    /// </summary>
    public static string[] SupportedSpeedUnits => ["rpm"];

    /// <summary>
    /// Gets the supported power units.
    /// </summary>
    public static string[] SupportedPowerUnits => ["kW", "W", "hp"];

    /// <summary>
    /// Gets the supported weight (mass) units.
    /// </summary>
    public static string[] SupportedWeightUnits => ["kg", "g", "lbs", "oz"];

    /// <summary>
    /// Gets the supported voltage units.
    /// </summary>
    public static string[] SupportedVoltageUnits => ["V", "kV"];

    /// <summary>
    /// Gets the supported current units.
    /// </summary>
    public static string[] SupportedCurrentUnits => ["A", "mA"];

    /// <summary>
    /// Gets the supported inertia units.
    /// </summary>
    public static string[] SupportedInertiaUnits => ["kg-m^2", "g-cm^2"];

    /// <summary>
    /// Gets the supported torque constant units.
    /// </summary>
    public static string[] SupportedTorqueConstantUnits => ["Nm/A"];

    /// <summary>
    /// Gets the supported backlash units.
    /// </summary>
    public static string[] SupportedBacklashUnits => ["arcmin", "arcsec"];

    /// <summary>
    /// Gets the supported response time units.
    /// </summary>
    public static string[] SupportedResponseTimeUnits => ["ms", "s"];

    /// <summary>
    /// Gets the supported percentage units.
    /// </summary>
    public static string[] SupportedPercentageUnits => ["%"];

    /// <summary>
    /// Gets the supported temperature units.
    /// </summary>
    public static string[] SupportedTemperatureUnits => ["C", "F", "K"];

    /// <summary>
    /// Converts a value from one unit to another.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="fromUnit">The source unit string (e.g., "Nm", "lbf-in").</param>
    /// <param name="toUnit">The target unit string (e.g., "Nm", "lbf-in").</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="ArgumentException">Thrown when unit conversion is not supported or units are incompatible.</exception>
    public double Convert(double value, string fromUnit, string toUnit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fromUnit);
        ArgumentException.ThrowIfNullOrWhiteSpace(toUnit);

        if (fromUnit == toUnit)
        {
            return value;
        }

        // Handle hp specially as it's not natively supported by Tare
        if (fromUnit == "hp" || toUnit == "hp")
        {
            return ConvertWithHorsepower(value, fromUnit, toUnit);
        }

        var tareFromUnit = MapToTareUnit(fromUnit);
        var tareToUnit = MapToTareUnit(toUnit);

        var quantity = Quantity.Parse($"{value} {tareFromUnit}");
        var converted = quantity.As(tareToUnit);

        return (double)converted.Value;
    }

    /// <summary>
    /// Handles conversions involving horsepower.
    /// </summary>
    private double ConvertWithHorsepower(double value, string fromUnit, string toUnit)
    {
        const double HpToWatts = 745.699872; // Mechanical horsepower (hp) to watts

        // Convert from hp to another unit
        if (fromUnit == "hp")
        {
            var watts = value * HpToWatts;
            if (toUnit == "W")
            {
                return watts;
            }
            else if (toUnit == "kW")
            {
                return watts / 1000.0;
            }
            else if (toUnit == "hp")
            {
                return value;
            }
            else
            {
                throw new ArgumentException($"Cannot convert from hp to {toUnit}", nameof(toUnit));
            }
        }

        // Convert to hp from another unit
        if (toUnit == "hp")
        {
            double watts;
            if (fromUnit == "W")
            {
                watts = value;
            }
            else if (fromUnit == "kW")
            {
                watts = value * 1000.0;
            }
            else
            {
                throw new ArgumentException($"Cannot convert from {fromUnit} to hp", nameof(fromUnit));
            }
            return watts / HpToWatts;
        }

        throw new ArgumentException($"Invalid hp conversion: {fromUnit} to {toUnit}");
    }

    /// <summary>
    /// Tries to convert a value from one unit to another.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="fromUnit">The source unit string.</param>
    /// <param name="toUnit">The target unit string.</param>
    /// <param name="result">When this method returns, contains the converted value if successful; otherwise, the original value.</param>
    /// <returns>True if conversion was successful; otherwise, false.</returns>
    public bool TryConvert(double value, string fromUnit, string toUnit, out double result)
    {
        result = value;

        if (string.IsNullOrWhiteSpace(fromUnit) || string.IsNullOrWhiteSpace(toUnit))
        {
            return false;
        }

        if (fromUnit == toUnit)
        {
            return true;
        }

        try
        {
            result = Convert(value, fromUnit, toUnit);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Maps application unit strings to Tare unit strings.
    /// </summary>
    /// <param name="appUnit">The application unit string (e.g., "Nm", "lbf-in").</param>
    /// <returns>The Tare-compatible unit string (e.g., "N*m", "lbf*in").</returns>
    /// <exception cref="ArgumentException">Thrown when the unit is not recognized.</exception>
    private static string MapToTareUnit(string appUnit)
    {
        return appUnit switch
        {
            // Torque units
            "Nm" => "N*m",
            "lbf-ft" => "lbf*ft",
            "lbf-in" => "lbf*in",
            "oz-in" => "ozf*in",

            // Speed units (RPM = revolutions per minute)
            "rpm" => "rev/min",

            // Power units (W = J/s = N*m/s)
            "kW" => "kJ/s",
            "W" => "J/s",
            "hp" => "J/s", // Handled specially in Convert method

            // Mass units
            "kg" => "kg",
            "g" => "g",
            "lbs" => "lb",
            "oz" => "oz",

            // Voltage units
            "V" => "V",
            "kV" => "kV",

            // Current units
            "A" => "ampere",
            "mA" => "m*ampere",

            // Inertia units (moment of inertia)
            "kg-m^2" => "kg*m^2",
            "g-cm^2" => "g*cm^2",

            // Torque constant units
            "Nm/A" => "N*m/ampere",

            // Backlash units
            "arcmin" => "arcminute",
            "arcsec" => "arcsecond",

            // Time units
            "ms" => "ms",
            "s" => "s",

            // Dimensionless/percentage
            "%" => "",

            // Temperature units
            "C" => "°C",
            "F" => "°F",
            "K" => "K",

            _ => throw new ArgumentException($"Unsupported unit: {appUnit}", nameof(appUnit))
        };
    }

    /// <summary>
    /// Formats a value with its unit for display.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <param name="unit">The unit string.</param>
    /// <param name="decimalPlaces">The number of decimal places to display.</param>
    /// <returns>A formatted string with value and unit.</returns>
    public string Format(double value, string unit, int decimalPlaces = 2)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);

        if (decimalPlaces < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places cannot be negative.");
        }

        var formattedValue = Math.Round(value, decimalPlaces).ToString($"F{decimalPlaces}");
        return $"{formattedValue} {unit}";
    }

    /// <summary>
    /// Validates whether a unit string is supported.
    /// </summary>
    /// <param name="unit">The unit string to validate.</param>
    /// <returns>True if the unit is supported; otherwise, false.</returns>
    public bool IsUnitSupported(string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
        {
            return false;
        }

        try
        {
            _ = MapToTareUnit(unit);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
