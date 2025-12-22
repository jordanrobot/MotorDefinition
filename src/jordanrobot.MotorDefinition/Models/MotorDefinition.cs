using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace CurveEditor.Models;

/// <summary>
/// Represents a complete motor definition including all properties, drive configurations, and metadata.
/// Structure: Motor → Drive(s) → Voltage(s) → CurveSeries
/// </summary>
public class MotorDefinition
{
    /// <summary>
    /// The current schema version for motor definition files.
    /// </summary>
    public const string CurrentSchemaVersion = "1.0.0";

    /// <summary>
    /// Schema version for JSON compatibility.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = CurrentSchemaVersion;

    // Motor Identification
    /// <summary>
    /// The model name or identifier for the motor.
    /// </summary>
    [JsonPropertyName("motorName")]
    public string MotorName { get; set; } = string.Empty;

    /// <summary>
    /// The company that manufactures the motor.
    /// </summary>
    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// The manufacturer's part number for the motor.
    /// </summary>
    [JsonPropertyName("partNumber")]
    public string PartNumber { get; set; } = string.Empty;

    // Motor Base Properties (theoretical maximums from motor cut sheet)
    /// <summary>
    /// The theoretical maximum power output of the motor (in the unit specified by Units.Power).
    /// </summary>
    [JsonPropertyName("power")]
    public double Power { get; set; }

    /// <summary>
    /// The theoretical maximum rotational speed of the motor (RPM).
    /// </summary>
    [JsonPropertyName("maxSpeed")]
    public double MaxSpeed { get; set; }

    /// <summary>
    /// The rated continuous operating speed of the motor (RPM).
    /// </summary>
    [JsonPropertyName("ratedSpeed")]
    public double RatedSpeed { get; set; }

    /// <summary>
    /// The theoretical maximum torque the motor can produce continuously without overheating.
    /// </summary>
    [JsonPropertyName("ratedContinuousTorque")]
    public double RatedContinuousTorque { get; set; }

    /// <summary>
    /// The theoretical maximum torque the motor can produce for short periods.
    /// </summary>
    [JsonPropertyName("ratedPeakTorque")]
    public double RatedPeakTorque { get; set; }

    // Mechanical Properties
    /// <summary>
    /// The mass of the motor (in the unit specified by Units.Weight).
    /// </summary>
    [JsonPropertyName("weight")]
    public double Weight { get; set; }

    /// <summary>
    /// The moment of inertia of the motor's rotor, affecting acceleration response.
    /// </summary>
    [JsonPropertyName("rotorInertia")]
    public double RotorInertia { get; set; }

    /// <summary>
    /// The feedback device pulses per revolution (PPR).
    /// Used for encoder or resolver feedback resolution.
    /// </summary>
    [JsonPropertyName("feedbackPpr")]
    public int FeedbackPpr { get; set; }

    // Brake Properties
    /// <summary>
    /// Indicates whether the motor includes an integral holding brake.
    /// </summary>
    [JsonPropertyName("hasBrake")]
    public bool HasBrake { get; set; }

    /// <summary>
    /// The holding torque of the integral brake (if present).
    /// </summary>
    [JsonPropertyName("brakeTorque")]
    public double BrakeTorque { get; set; }

    /// <summary>
    /// The current draw of the brake (if present) (A).
    /// </summary>
    [JsonPropertyName("brakeAmperage")]
    public double BrakeAmperage { get; set; }

    /// <summary>
    /// The voltage requirement of the brake (if present) (V).
    /// </summary>
    [JsonPropertyName("brakeVoltage")]
    public double BrakeVoltage { get; set; }

    /// <summary>
    /// The release time of the brake.
    /// </summary>
    [JsonPropertyName("brakeReleaseTime")]
    public double BrakeReleaseTime { get; set; }

    /// <summary>
    /// The brake engage time when using a diode.
    /// </summary>
    [JsonPropertyName("brakeEngageTimeDiode")]
    public double BrakeEngageTimeDiode { get; set; }

    /// <summary>
    /// The brake engage time when using an MOV.
    /// </summary>
    [JsonPropertyName("brakeEngageTimeMOV")]
    public double BrakeEngageTimeMov { get; set; }

    /// <summary>
    /// The backlash of the brake mechanism.
    /// </summary>
    [JsonPropertyName("brakeBacklash")]
    public double BrakeBacklash { get; set; }

    // Units Configuration
    /// <summary>
    /// The unit settings for this motor definition.
    /// </summary>
    [JsonPropertyName("units")]
    public UnitSettings Units { get; set; } = new();

    // Drive Configurations
    /// <summary>
    /// The collection of drive configurations for this motor.
    /// Each drive can have multiple voltage configurations with their own curve series.
    /// </summary>
    [JsonPropertyName("drives")]
    public List<DriveConfiguration> Drives { get; set; } = [];

    // Metadata
    /// <summary>
    /// Metadata about the motor definition file.
    /// </summary>
    [JsonPropertyName("metadata")]
    public MotorMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Creates a new MotorDefinition with default values.
    /// </summary>
    public MotorDefinition()
    {
    }

    /// <summary>
    /// Creates a new MotorDefinition with the specified motor name.
    /// </summary>
    /// <param name="motorName">The name of the motor.</param>
    public MotorDefinition(string motorName)
    {
        MotorName = motorName;
    }

    /// <summary>
    /// Validates that the motor definition has at least one drive with a valid configuration.
    /// </summary>
    /// <returns>True if valid; otherwise false.</returns>
    public bool HasValidConfiguration()
    {
        return Drives.Count > 0 && 
               Drives.Any(d => d.Voltages.Count > 0 && 
                              d.Voltages.Any(v => v.Series.Count > 0));
    }

    /// <summary>
    /// Gets a drive configuration by name.
    /// </summary>
    /// <param name="name">The name of the drive to find.</param>
    /// <returns>The matching drive configuration, or null if not found.</returns>
    public DriveConfiguration? GetDriveByName(string name)
    {
        return Drives.Find(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Adds a new drive configuration with the specified name.
    /// </summary>
    /// <param name="name">The name for the new drive.</param>
    /// <returns>The newly created drive configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a drive with the same name already exists.</exception>
    public DriveConfiguration AddDrive(string name)
    {
        if (GetDriveByName(name) is not null)
        {
            throw new InvalidOperationException($"A drive with the name '{name}' already exists.");
        }

        var drive = new DriveConfiguration(name);
        Drives.Add(drive);
        Metadata.UpdateModified();
        return drive;
    }

    /// <summary>
    /// Removes a drive configuration by name.
    /// </summary>
    /// <param name="name">The name of the drive to remove.</param>
    /// <returns>True if removed; false if not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if attempting to remove the last drive.</exception>
    public bool RemoveDrive(string name)
    {
        if (Drives.Count <= 1)
        {
            throw new InvalidOperationException("Cannot remove the last drive. At least one drive must exist.");
        }

        var drive = GetDriveByName(name);
        if (drive is null)
        {
            return false;
        }

        Drives.Remove(drive);
        Metadata.UpdateModified();
        return true;
    }

    /// <summary>
    /// Gets all curve series across all drives and voltages.
    /// Useful for getting a flat list of all curves in the motor definition.
    /// </summary>
    /// <returns>All curve series in the motor definition.</returns>
    public IEnumerable<CurveSeries> GetAllSeries()
    {
        return Drives.SelectMany(d => d.Voltages.SelectMany(v => v.Series));
    }
}
