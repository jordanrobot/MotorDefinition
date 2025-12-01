using System.Text.Json.Serialization;

namespace CurveEditor.Models;

/// <summary>
/// Specifies the units used for various motor properties.
/// </summary>
public class UnitSettings
{
    /// <summary>
    /// Torque unit: "Nm" (Newton-meters), "lbf-in" (pound-force inches), or "oz-in" (ounce-force inches).
    /// </summary>
    [JsonPropertyName("torque")]
    public string Torque { get; set; } = "Nm";

    /// <summary>
    /// Speed unit: "rpm" (revolutions per minute) or "rev/s" (revolutions per second).
    /// </summary>
    [JsonPropertyName("speed")]
    public string Speed { get; set; } = "rpm";

    /// <summary>
    /// Power unit: "W" (Watts), "kW" (kilowatts), or "hp" (horsepower).
    /// </summary>
    [JsonPropertyName("power")]
    public string Power { get; set; } = "W";

    /// <summary>
    /// Weight unit: "kg" (kilograms), "lbs" (pounds), or "g" (grams).
    /// </summary>
    [JsonPropertyName("weight")]
    public string Weight { get; set; } = "kg";

    /// <summary>
    /// Gets the supported torque units.
    /// </summary>
    public static string[] SupportedTorqueUnits => ["Nm", "lbf-in", "oz-in"];

    /// <summary>
    /// Gets the supported speed units.
    /// </summary>
    public static string[] SupportedSpeedUnits => ["rpm", "rev/s"];

    /// <summary>
    /// Gets the supported power units.
    /// </summary>
    public static string[] SupportedPowerUnits => ["W", "kW", "hp"];

    /// <summary>
    /// Gets the supported weight units.
    /// </summary>
    public static string[] SupportedWeightUnits => ["kg", "lbs", "g"];
}
