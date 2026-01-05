using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Model;

/// <summary>
/// Represents a drive configuration for a motor.
/// </summary>
/// <remarks>
/// A drive contains one or more <see cref="Voltage"/> configurations, each of which contains one or more
/// <see cref="Curve"/> definitions.
/// </remarks>
public class Drive : INotifyPropertyChanged
{
    private string _name = string.Empty;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the name or model identifier of the drive.
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
    /// Gets or sets the manufacturer's part number for the servo drive.
    /// </summary>
    [JsonPropertyName("partNumber")]
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the manufacturer of the drive.
    /// </summary>
    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of voltage configurations for this drive.
    /// </summary>
    [JsonPropertyName("voltages")]
    public List<Voltage> Voltages { get; set; } = [];

    /// <summary>
    /// Gets display-friendly voltage names (for example, "208 V").
    /// </summary>
    /// <remarks>
    /// Useful for populating UI lists and combo-boxes.
    /// </remarks>
    [JsonIgnore]
    public IEnumerable<string> VoltageNames => Voltages.Select(v => v.DisplayName);

    /// <summary>
    /// Gets the numeric voltage values.
    /// </summary>
    /// <remarks>
    /// Useful for populating UI lists and combo-boxes.
    /// </remarks>
    [JsonIgnore]
    public IEnumerable<double> VoltageValues => Voltages.Select(v => v.Value);

    /// <summary>
    /// Gets or sets the validation signature for this drive configuration.
    /// </summary>
    /// <remarks>
    /// This signature covers the drive properties including all voltages.
    /// Null indicates the drive has not been signed.
    /// </remarks>
    [JsonPropertyName("driveSignature")]
    public ValidationSignature? DriveSignature { get; set; }

    /// <summary>
    /// Creates a new Drive with default values.
    /// </summary>
    public Drive()
    {
        _name = "Unnamed Drive";
    }

    /// <summary>
    /// Creates a new Drive with the specified name.
    /// </summary>
    /// <param name="name">The name of the drive.</param>
    public Drive(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Default tolerance for matching voltage values (in volts).
    /// </summary>
    public const double DefaultVoltageTolerance = 0.1;

    /// <summary>
    /// Gets a voltage configuration by voltage value.
    /// </summary>
    /// <param name="voltage">The voltage to find.</param>
    /// <param name="tolerance">The tolerance for matching voltage values (default 0.1V).</param>
    /// <returns>The matching voltage configuration, or null if not found.</returns>
    public Voltage? GetVoltage(double voltage, double tolerance = DefaultVoltageTolerance)
    {
        return Voltages.Find(v => Math.Abs(v.Value - voltage) < tolerance);
    }

    /// <summary>
    /// Adds a new voltage configuration.
    /// </summary>
    /// <param name="voltage">The voltage value.</param>
    /// <returns>The newly created voltage configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a configuration with the same voltage already exists.</exception>
    public Voltage AddVoltage(double voltage)
    {
        if (GetVoltage(voltage) is not null)
        {
            throw new InvalidOperationException($"A voltage configuration for {voltage}V already exists.");
        }

        var config = new Voltage(voltage);
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
