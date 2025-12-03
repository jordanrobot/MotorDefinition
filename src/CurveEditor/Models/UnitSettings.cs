using System.Text.Json.Serialization;

namespace CurveEditor.Models;

/// <summary>
/// Specifies the units used for various motor properties.
/// </summary>
public class UnitSettings
{
    /// <summary>
    /// Torque unit: "Nm", "lbf-ft", "lbf-in", or "oz-in".
    /// </summary>
    [JsonPropertyName("torque")]
    public string Torque { get; set; } = "Nm";

    /// <summary>
    /// Speed unit: "rpm".
    /// </summary>
    [JsonPropertyName("speed")]
    public string Speed { get; set; } = "rpm";

    /// <summary>
    /// Power unit: "kW", "W", or "hp".
    /// </summary>
    [JsonPropertyName("power")]
    public string Power { get; set; } = "W";

    /// <summary>
    /// Weight unit: "kg", "g", "lbs", or "oz".
    /// </summary>
    [JsonPropertyName("weight")]
    public string Weight { get; set; } = "kg";

    /// <summary>
    /// Voltage unit: "V" or "kV".
    /// </summary>
    [JsonPropertyName("voltage")]
    public string Voltage { get; set; } = "V";

    /// <summary>
    /// Current unit: "A" or "mA".
    /// </summary>
    [JsonPropertyName("current")]
    public string Current { get; set; } = "A";

    /// <summary>
    /// Inertia unit: "kg-m^2" or "g-cm^2".
    /// </summary>
    [JsonPropertyName("inertia")]
    public string Inertia { get; set; } = "kg-m^2";

    /// <summary>
    /// Gets the supported speed units.
    /// </summary>
    public static string[] SupportedSpeedUnits => ["rpm"];

    /// <summary>
    /// Gets the supported weight units.
    /// </summary>
    public static string[] SupportedWeightUnits => ["kg", "g", "lbs", "oz"];

    /// <summary>
    /// Gets the supported torque units.
    /// </summary>
    public static string[] SupportedTorqueUnits => ["Nm", "lbf-ft", "lbf-in", "oz-in"];

    /// <summary>
    /// Gets the supported power units.
    /// </summary>
    public static string[] SupportedPowerUnits => ["kW", "W", "hp"];

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
}
