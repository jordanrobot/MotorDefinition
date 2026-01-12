using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Model;

/// <summary>
/// Represents voltage-specific configuration and performance data for a motor/drive combination.
/// </summary>
/// <remarks>
/// Each <see cref="Voltage"/> contains one or more <see cref="Curve"/> definitions representing different
/// operating conditions (for example, peak vs. continuous).
/// </remarks>
public class Voltage : INotifyPropertyChanged
{
    private decimal _voltage;

    /// <summary>
    /// Gets or sets the operating voltage (V).
    /// </summary>
    [JsonPropertyName("voltage")]
    public decimal Value
    {
        get => _voltage;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Voltage must be positive.");
            }
            if (Math.Abs(_voltage - value) <= 0m)
            {
                return;
            }

            _voltage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a display-friendly name for this voltage configuration (for example, "208 V").
    /// </summary>
    /// <remarks>
    /// Useful for populating UI lists and combo-boxes.
    /// </remarks>
    [JsonIgnore]
    public string DisplayName => string.Create(CultureInfo.InvariantCulture, $"{Value:0.##} V");

    /// <summary>
    /// Gets or sets the power output at this voltage.
    /// </summary>
    /// <remarks>
    /// Expressed in the unit specified by the parent motor's <see cref="UnitSettings"/>.
    /// </remarks>
    [JsonPropertyName("power")]
    public decimal Power { get; set; }

    /// <summary>
    /// Gets or sets the maximum rotational speed at this voltage (RPM).
    /// </summary>
    [JsonPropertyName("maxSpeed")]
    public decimal MaxSpeed { get; set; }

    /// <summary>
    /// Gets or sets the rated continuous operating speed at this voltage (RPM).
    /// </summary>
    [JsonPropertyName("ratedSpeed")]
    public decimal RatedSpeed { get; set; }

    /// <summary>
    /// Gets or sets the rated continuous torque at this voltage.
    /// </summary>
    [JsonPropertyName("ratedContinuousTorque")]
    public decimal RatedContinuousTorque { get; set; }

    /// <summary>
    /// Gets or sets the rated peak torque at this voltage.
    /// </summary>
    [JsonPropertyName("ratedPeakTorque")]
    public decimal RatedPeakTorque { get; set; }

    /// <summary>
    /// Gets or sets the current draw during continuous operation at rated torque (A).
    /// </summary>
    [JsonPropertyName("continuousAmperage")]
    public decimal ContinuousAmperage { get; set; }

    /// <summary>
    /// Gets or sets the maximum current draw during peak torque operation (A).
    /// </summary>
    [JsonPropertyName("peakAmperage")]
    public decimal PeakAmperage { get; set; }

    /// <summary>
    /// Gets or sets the curves for this voltage configuration (for example, "Peak" and "Continuous").
    /// </summary>
    [JsonPropertyName("series")]
    public List<Curve> Curves { get; set; } = [];

    /// <summary>
    /// Creates a new Voltage with default values.
    /// </summary>
    public Voltage()
    {
    }

    /// <summary>
    /// Creates a new Voltage with the specified voltage.
    /// </summary>
    /// <param name="voltage">The operating voltage.</param>
    public Voltage(decimal voltage)
    {
        Value = voltage;
    }

    /// <summary>
    /// Gets a curve by name.
    /// </summary>
    /// <param name="name">The name of the curve to find.</param>
    /// <returns>The matching curve, or null if not found.</returns>
    public Curve? GetSeriesByName(string name)
    {
        return Curves.Find(s => s.Name.Equals(name, StringComparison.Ordinal));
    }

    /// <summary>
    /// Adds a new curve with the specified name.
    /// </summary>
    /// <param name="name">The name for the new curve.</param>
    /// <param name="initializeTorque">The default torque value for all points.</param>
    /// <returns>The newly created curve.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a curve with the same name already exists.</exception>
    public Curve AddSeries(string name, decimal initializeTorque = 0)
    {
        if (GetSeriesByName(name) is not null)
        {
            throw new InvalidOperationException($"A curve with the name '{name}' already exists.");
        }

        var curve = new Curve(name);
        curve.InitializeData(MaxSpeed, initializeTorque);
        Curves.Add(curve);
        return curve;
    }

    /// <summary>
    /// Raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifies listeners that a property value has changed.
    /// </summary>
    /// <param name="propertyName">The name of the changed property.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
