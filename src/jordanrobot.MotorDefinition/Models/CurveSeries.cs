using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CurveEditor.Models;

/// <summary>
/// Represents a named series of motor torque/speed data points.
/// Each series represents a specific operating condition (e.g., "Peak" or "Continuous").
/// </summary>
public class CurveSeries : INotifyPropertyChanged
{
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
                throw new ArgumentException("Series name cannot be null or empty.", nameof(value));
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
    /// The data points for this curve, stored at 1% increments.
    /// Should contain 101 points (0% through 100%).
    /// </summary>
    [JsonPropertyName("data")]
    public List<DataPoint> Data { get; set; } = [];

    /// <summary>
    /// Creates a new CurveSeries with default values.
    /// </summary>
    public CurveSeries()
    {
        _name = "Unnamed";
    }

    /// <summary>
    /// Creates a new CurveSeries with the specified name.
    /// </summary>
    /// <param name="name">The name of the curve series.</param>
    public CurveSeries(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes the data with 101 points (0% to 100%) at 1% increments.
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
    /// Validates that the series has the expected 101 data points at 1% increments.
    /// </summary>
    /// <returns>True if the series has valid data structure; otherwise false.</returns>
    public bool ValidateDataIntegrity()
    {
        if (Data.Count != 101)
        {
            return false;
        }

        for (var i = 0; i <= 100; i++)
        {
            if (Data[i].Percent != i)
            {
                return false;
            }
        }

        return true;
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
