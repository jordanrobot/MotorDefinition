using System;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Model;

/// <summary>
/// Represents a single point on a motor torque curve.
/// </summary>
public class DataPoint
{
    private int _percent;
    private decimal _rpm;

    /// <summary>
    /// Gets or sets the percent position along the motor's speed range.
    /// </summary>
    /// <remarks>
    /// Typically 0% corresponds to 0 RPM and 100% corresponds to max speed, but values above 100% may be used
    /// to represent overspeed ranges.
    /// </remarks>
    [JsonPropertyName("percent")]
    public int Percent
    {
        get => _percent;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Percent cannot be negative.");
            }
            _percent = value;
        }
    }

    /// <summary>
    /// Gets or sets the rotational speed at this point (RPM).
    /// </summary>
    [JsonPropertyName("rpm")]
    public decimal Rpm
    {
        get => _rpm;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "RPM cannot be negative.");
            }
            _rpm = value;
        }
    }

    /// <summary>
    /// Gets or sets the torque value at this point.
    /// </summary>
    /// <remarks>
    /// Torque may be negative for regenerative braking scenarios.
    /// </remarks>
    [JsonPropertyName("torque")]
    public decimal Torque { get; set; }

    /// <summary>
    /// Gets the RPM value rounded to the nearest whole number for display.
    /// </summary>
    [JsonIgnore]
    public int DisplayRpm => (int)Math.Round(Rpm);

    /// <summary>
    /// Creates a new DataPoint with default values.
    /// </summary>
    public DataPoint()
    {
    }

    /// <summary>
    /// Creates a new DataPoint with the specified values.
    /// </summary>
    /// <param name="percent">Percentage along the speed range. Must be non-negative.</param>
    /// <param name="rpm">RPM value at this point.</param>
    /// <param name="torque">Torque value at this point.</param>
    public DataPoint(int percent, decimal rpm, decimal torque)
    {
        Percent = percent;
        Rpm = rpm;
        Torque = torque;
    }
}
