using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CurveEditor.Models;

/// <summary>
/// Represents voltage-specific configuration and performance data for a motor/drive combination.
/// Contains the curve series for this specific voltage setting.
/// </summary>
public class VoltageConfiguration : INotifyPropertyChanged
{
    private double _voltage;

    /// <summary>
    /// The operating voltage (V).
    /// </summary>
    [JsonPropertyName("voltage")]
    public double Voltage
    {
        get => _voltage;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Voltage must be positive.");
            }
            if (Math.Abs(_voltage - value) < double.Epsilon)
            {
                return;
            }

            _voltage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The power output at this voltage (in the unit specified by Units.Power).
    /// </summary>
    [JsonPropertyName("power")]
    public double Power { get; set; }

    /// <summary>
    /// The maximum rotational speed at this voltage (RPM).
    /// </summary>
    [JsonPropertyName("maxSpeed")]
    public double MaxSpeed { get; set; }

    /// <summary>
    /// The rated continuous operating speed at this voltage (RPM).
    /// </summary>
    [JsonPropertyName("ratedSpeed")]
    public double RatedSpeed { get; set; }

    /// <summary>
    /// The torque the motor can produce continuously at this voltage without overheating.
    /// </summary>
    [JsonPropertyName("ratedContinuousTorque")]
    public double RatedContinuousTorque { get; set; }

    /// <summary>
    /// The maximum torque the motor can produce for short periods at this voltage.
    /// </summary>
    [JsonPropertyName("ratedPeakTorque")]
    public double RatedPeakTorque { get; set; }

    /// <summary>
    /// The current draw during continuous operation at rated torque (A).
    /// </summary>
    [JsonPropertyName("continuousAmperage")]
    public double ContinuousAmperage { get; set; }

    /// <summary>
    /// The maximum current draw during peak torque operation (A).
    /// </summary>
    [JsonPropertyName("peakAmperage")]
    public double PeakAmperage { get; set; }

    /// <summary>
    /// The collection of curve series for this voltage configuration (e.g., "Peak", "Continuous").
    /// </summary>
    [JsonPropertyName("series")]
    public List<CurveSeries> Series { get; set; } = [];

    /// <summary>
    /// Creates a new VoltageConfiguration with default values.
    /// </summary>
    public VoltageConfiguration()
    {
    }

    /// <summary>
    /// Creates a new VoltageConfiguration with the specified voltage.
    /// </summary>
    /// <param name="voltage">The operating voltage.</param>
    public VoltageConfiguration(double voltage)
    {
        Voltage = voltage;
    }

    /// <summary>
    /// Gets a curve series by name.
    /// </summary>
    /// <param name="name">The name of the series to find.</param>
    /// <returns>The matching series, or null if not found.</returns>
    public CurveSeries? GetSeriesByName(string name)
    {
        return Series.Find(s => s.Name.Equals(name, StringComparison.Ordinal));
    }

    /// <summary>
    /// Adds a new series with the specified name.
    /// </summary>
    /// <param name="name">The name for the new series.</param>
    /// <param name="initializeTorque">The default torque value for all points.</param>
    /// <returns>The newly created series.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a series with the same name already exists.</exception>
    public CurveSeries AddSeries(string name, double initializeTorque = 0)
    {
        if (GetSeriesByName(name) is not null)
        {
            throw new InvalidOperationException($"A series with the name '{name}' already exists.");
        }

        var series = new CurveSeries(name);
        series.InitializeData(MaxSpeed, initializeTorque);
        Series.Add(series);
        return series;
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
