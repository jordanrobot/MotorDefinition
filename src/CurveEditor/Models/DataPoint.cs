using System;
using System.Text.Json.Serialization;

namespace CurveEditor.Models;

/// <summary>
/// Represents a single point on a motor torque curve.
/// </summary>
public class DataPoint
{
    private int _percent;
    private double _rpm;

    /// <summary>
    /// Percentage (0-100) representing position along the motor's speed range.
    /// 0% = 0 RPM, 100% = MaxRpm.
    /// </summary>
    [JsonPropertyName("percent")]
    public int Percent
    {
        get => _percent;
        set
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Percent must be between 0 and 100.");
            }
            _percent = value;
        }
    }

    /// <summary>
    /// Rotational speed at this percentage point in revolutions per minute.
    /// </summary>
    [JsonPropertyName("rpm")]
    public double Rpm
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
    /// Torque value at this speed point.
    /// Can be negative for regenerative braking scenarios.
    /// </summary>
    [JsonPropertyName("torque")]
    public double Torque { get; set; }

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
    /// <param name="percent">Percentage (0-100) along the speed range.</param>
    /// <param name="rpm">RPM value at this point.</param>
    /// <param name="torque">Torque value at this point.</param>
    public DataPoint(int percent, double rpm, double torque)
    {
        Percent = percent;
        Rpm = rpm;
        Torque = torque;
    }
}
