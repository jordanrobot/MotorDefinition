using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Model;

/// <summary>
/// Represents a named motor torque/speed curve.
/// </summary>
/// <remarks>
/// A curve represents a specific operating condition (for example, "Peak" or "Continuous") and contains a set of
/// torque/speed <see cref="DataPoint"/> values.
/// </remarks>
public class Curve : INotifyPropertyChanged
{
    private const int MaxSupportedPointCount = 101;
    private string _name = string.Empty;
    private bool _locked;
    private bool _lockedPercent;
    private bool _lockedRpm;
    private bool _isVisible = true;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the name of this curve (for example, "Peak" or "Continuous").
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
    /// Gets or sets notes about this curve.
    /// </summary>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this curve is locked for editing.
    /// </summary>
    /// <remarks>
    /// When <see langword="true"/>, the curve data should not be modified.
    /// </remarks>
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
    /// Gets or sets whether the speed percentage column is locked for editing.
    /// </summary>
    /// <remarks>
    /// When <see langword="true"/>, the percent values in the curve data should not be modified.
    /// This allows locking percent editing independently of torque or RPM editing.
    /// </remarks>
    [JsonPropertyName("lockedPercent")]
    public bool LockedPercent
    {
        get => _lockedPercent;
        set
        {
            if (_lockedPercent != value)
            {
                _lockedPercent = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the speed RPM column is locked for editing.
    /// </summary>
    /// <remarks>
    /// When <see langword="true"/>, the RPM values in the curve data should not be modified.
    /// This allows locking RPM editing independently of torque or percent editing.
    /// </remarks>
    [JsonPropertyName("lockedRpm")]
    public bool LockedRpm
    {
        get => _lockedRpm;
        set
        {
            if (_lockedRpm != value)
            {
                _lockedRpm = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether this curve is visible in the chart.
    /// </summary>
    /// <remarks>
    /// This is a runtime-only property that is not persisted to JSON.
    /// </remarks>
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
    /// Gets or sets the data points for this curve.
    /// </summary>
    /// <remarks>
    /// Typically contains 101 points at 1% increments (0% through 100%), but may contain fewer points.
    /// Values above 100% may be present to represent overspeed ranges.
    /// </remarks>
    [JsonPropertyName("data")]
    public List<DataPoint> Data { get; set; } = [];

    /// <summary>
    /// Gets the percentage axis (0..100) for this curve.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<int> Percents => Data.Select(p => p.Percent);

    /// <summary>
    /// Gets the RPM values for this curve.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<double> Rpms => Data.Select(p => p.Rpm);

    /// <summary>
    /// Gets the torque values for this curve.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<double> Torques => Data.Select(p => p.Torque);

    /// <summary>
    /// Gets or sets the validation signature for this curve's data.
    /// </summary>
    /// <remarks>
    /// This signature covers the curve data points only.
    /// Null indicates the curve has not been signed.
    /// </remarks>
    [JsonPropertyName("curveSignature")]
    public ValidationSignature? CurveSignature { get; set; }

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
    /// <param name="name">The name of the curve.</param>
    public Curve(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes the data with the default 101 points (0% to 100%) at 1% increments.
    /// </summary>
    /// <remarks>
    /// The file format can store 0..101 points per curve; this helper always generates the standard 1% curve.
    /// </remarks>
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
    /// Gets the number of data points in this curve.
    /// </summary>
    [JsonIgnore]
    public int PointCount => Data.Count;

    /// <summary>
    /// Validates that this curve has a supported shape.
    /// </summary>
    /// <remarks>
    /// A valid curve has 0..101 points, non-negative percent values, and a strictly increasing percent axis.
    /// </remarks>
    /// <returns><see langword="true"/> if the curve has a valid data structure; otherwise <see langword="false"/>.</returns>
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
    /// </summary>
    /// <remarks>
    /// Prefer this for quick lookups when exporting or rendering tables.
    /// </remarks>
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
    /// <exception cref="ArgumentException">Thrown if the curve contains duplicate percent values.</exception>
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
