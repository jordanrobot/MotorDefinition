using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Model;

/// <summary>
/// Represents a named series of motor torque/speed data points.
/// Each series represents a specific operating condition (e.g., "Peak" or "Continuous").
/// </summary>
public class Curve : INotifyPropertyChanged
{
    private const int MaxSupportedPointCount = 101;
    private string _name = string.Empty;
    private bool _locked;
    private bool _isVisible = true;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The name of this curve series (e.g., "Peak", "Continuous").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Curves name cannot be null or empty.", nameof(value));
            }
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Notes or comments about this curve series.
    /// </summary>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this curve series is locked for editing.
    /// When true, the curve data should not be modified.
    /// </summary>
    [JsonPropertyName("locked")]
    public bool Locked
    {
        get => _locked;
        set
        {
            if (_locked != value)
            {
                _locked = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Indicates whether this curve series is visible in the chart.
    /// This is a runtime-only property that is not persisted to JSON.
    /// </summary>
    [JsonIgnore]
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// The data points for this curve.
    /// Typically contains 101 points at 1% increments (0% through 100%), but may contain fewer points.
    /// Values above 100% may be present to represent overspeed ranges.
    /// </summary>
    [JsonPropertyName("data")]
    public List<DataPoint> Data { get; set; } = [];

    /// <summary>
    /// Gets the percentage axis (0..100) for this series.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<int> Percents => Data.Select(p => p.Percent);

    /// <summary>
    /// Gets the RPM values for this series.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<double> Rpms => Data.Select(p => p.Rpm);

    /// <summary>
    /// Gets the torque values for this series.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<double> Torques => Data.Select(p => p.Torque);

    /// <summary>
    /// Creates a new Curve with default values.
    /// </summary>
    public Curve()
    {
        _name = "Unnamed";
    }

    /// <summary>
    /// Creates a new Curve with the specified name.
    /// </summary>
    /// <param name="name">The name of the curve series.</param>
    public Curve(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes the data with the default 101 points (0% to 100%) at 1% increments.
    /// The file format can store 0..101 points per series; this helper always generates the standard 1% curve.
    /// </summary>
    /// <param name="maxRpm">The maximum RPM of the motor.</param>
    /// <param name="defaultTorque">The default torque value for all points.</param>
    public void InitializeData(double maxRpm, double defaultTorque)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxRpm);

        Data.Clear();
        for (var percent = 0; percent <= 100; percent++)
        {
            Data.Add(new DataPoint
            {
                Percent = percent,
                Rpm = percent / 100.0 * maxRpm,
                Torque = defaultTorque
            });
        }
    }

    /// <summary>
    /// Gets the number of data points in this series.
    /// </summary>
    [JsonIgnore]
    public int PointCount => Data.Count;

    /// <summary>
    /// Validates that the series has a supported shape.
    /// A valid series has 0..101 points, non-negative percent values, and a strictly increasing percent axis.
    /// </summary>
    /// <returns>True if the series has valid data structure; otherwise false.</returns>
    public bool ValidateDataIntegrity()
    {
        if (Data.Count > MaxSupportedPointCount)
        {
            return false;
        }

        var previousPercent = -1;
        for (var i = 0; i < Data.Count; i++)
        {
            var percent = Data[i].Percent;
            if (percent < 0)
            {
                return false;
            }

            if (percent <= previousPercent)
            {
                return false;
            }

            previousPercent = percent;
        }

        return true;
    }

    /// <summary>
    /// Gets the data point for a given percent.
    /// Prefer this for quick lookups when exporting or rendering tables.
    /// </summary>
    /// <param name="percent">The percent (non-negative).</param>
    /// <returns>The matching data point.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="percent"/> is negative.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no point exists for <paramref name="percent"/>.</exception>
    public DataPoint GetPointByPercent(int percent)
    {
        if (percent < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), percent, "Percent cannot be negative.");
        }

        if (Data.Count == 101 && percent >= 0 && percent <= 100 && percent < Data.Count && Data[percent].Percent == percent)
        {
            return Data[percent];
        }

        var point = Data.FirstOrDefault(p => p.Percent == percent);
        if (point is null)
        {
            throw new KeyNotFoundException($"No data point exists for {percent}%.");
        }

        return point;
    }

    /// <summary>
    /// Attempts to get the data point for a given percent.
    /// </summary>
    /// <param name="percent">The percent (non-negative).</param>
    /// <param name="point">When this method returns, contains the matching point if found; otherwise null.</param>
    /// <returns>True if found; otherwise false.</returns>
    public bool TryGetPointByPercent(int percent, out DataPoint? point)
    {
        point = null;

        if (percent < 0)
        {
            return false;
        }

        if (Data.Count == 101 && percent >= 0 && percent <= 100 && percent < Data.Count && Data[percent].Percent == percent)
        {
            point = Data[percent];
            return true;
        }

        point = Data.FirstOrDefault(p => p.Percent == percent);
        return point is not null;
    }

    /// <summary>
    /// Creates a lookup dictionary keyed by percent (0..100).
    /// This is useful for caching fast lookups in client applications.
    /// </summary>
    /// <returns>A dictionary mapping percent to data point.</returns>
    /// <exception cref="ArgumentException">Thrown if the series contains duplicate percent values.</exception>
    public IReadOnlyDictionary<int, DataPoint> ToPercentLookup()
    {
        return Data.ToDictionary(p => p.Percent);
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
