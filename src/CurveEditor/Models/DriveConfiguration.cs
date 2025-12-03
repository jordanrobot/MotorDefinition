using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CurveEditor.Models;

/// <summary>
/// Represents a servo drive configuration for a motor.
/// Contains voltage-specific configurations and their associated curve series.
/// </summary>
public class DriveConfiguration : INotifyPropertyChanged
{
    private string _name = string.Empty;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The name or model identifier of the drive.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Drive name cannot be null or empty.", nameof(value));
            }
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// The manufacturer's part number for the servo drive.
    /// </summary>
    [JsonPropertyName("partNumber")]
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// The manufacturer of the drive.
    /// </summary>
    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// The collection of voltage configurations for this drive.
    /// </summary>
    [JsonPropertyName("voltages")]
    public List<VoltageConfiguration> Voltages { get; set; } = [];

    /// <summary>
    /// Creates a new DriveConfiguration with default values.
    /// </summary>
    public DriveConfiguration()
    {
        _name = "Unnamed Drive";
    }

    /// <summary>
    /// Creates a new DriveConfiguration with the specified name.
    /// </summary>
    /// <param name="name">The name of the drive.</param>
    public DriveConfiguration(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Default tolerance for voltage matching in volts.
    /// </summary>
    public const double DefaultVoltageTolerance = 0.1;

    /// <summary>
    /// Gets a voltage configuration by voltage value.
    /// </summary>
    /// <param name="voltage">The voltage to find.</param>
    /// <param name="tolerance">The tolerance for matching voltage values (default 0.1V).</param>
    /// <returns>The matching voltage configuration, or null if not found.</returns>
    public VoltageConfiguration? GetVoltageConfiguration(double voltage, double tolerance = DefaultVoltageTolerance)
    {
        return Voltages.Find(v => Math.Abs(v.Voltage - voltage) < tolerance);
    }

    /// <summary>
    /// Adds a new voltage configuration.
    /// </summary>
    /// <param name="voltage">The voltage value.</param>
    /// <returns>The newly created voltage configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a configuration with the same voltage already exists.</exception>
    public VoltageConfiguration AddVoltageConfiguration(double voltage)
    {
        if (GetVoltageConfiguration(voltage) is not null)
        {
            throw new InvalidOperationException($"A voltage configuration for {voltage}V already exists.");
        }

        var config = new VoltageConfiguration(voltage);
        Voltages.Add(config);
        return config;
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
