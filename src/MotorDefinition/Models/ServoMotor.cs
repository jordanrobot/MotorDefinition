using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Model;

/// <summary>
/// Represents a complete motor definition including properties, drive configurations, and metadata.
/// </summary>
/// <remarks>
/// Structure: <see cref="ServoMotor"/> → <see cref="Drive"/> → <see cref="Voltage"/> → <see cref="Curve"/>.
/// </remarks>
public class ServoMotor
{
    /// <summary>
    /// Specifies the current schema version for motor definition files.
    /// </summary>
    public const string CurrentSchemaVersion = "1.0.0";

    /// <summary>
    /// Gets or sets the schema version for JSON compatibility.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = CurrentSchemaVersion;

    // Motor Identification
    /// <summary>
    /// Gets or sets the model name or identifier for the motor.
    /// </summary>
    [JsonPropertyName("motorName")]
    public string MotorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the company that manufactures the motor.
    /// </summary>
    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the manufacturer's part number for the motor.
    /// </summary>
    [JsonPropertyName("partNumber")]
    public string PartNumber { get; set; } = string.Empty;

    // Motor Base Properties (theoretical maximums from motor cut sheet)
    /// <summary>
    /// Gets or sets the theoretical maximum power output of the motor.
    /// </summary>
    /// <remarks>
    /// Expressed in the unit specified by <see cref="Units"/>.<see cref="UnitSettings.Power"/>.
    /// </remarks>
    [JsonPropertyName("power")]
    public decimal Power { get; set; }

    /// <summary>
    /// Gets or sets the theoretical maximum rotational speed of the motor (RPM).
    /// </summary>
    [JsonPropertyName("maxSpeed")]
    public decimal MaxSpeed { get; set; }

    /// <summary>
    /// Gets or sets the rated continuous operating speed of the motor (RPM).
    /// </summary>
    [JsonPropertyName("ratedSpeed")]
    public decimal RatedSpeed { get; set; }

    /// <summary>
    /// Gets or sets the theoretical maximum continuous torque for the motor.
    /// </summary>
    [JsonPropertyName("ratedContinuousTorque")]
    public decimal RatedContinuousTorque { get; set; }

    /// <summary>
    /// Gets or sets the theoretical maximum peak torque for the motor.
    /// </summary>
    [JsonPropertyName("ratedPeakTorque")]
    public decimal RatedPeakTorque { get; set; }

    // Mechanical Properties
    /// <summary>
    /// Gets or sets the mass of the motor.
    /// </summary>
    /// <remarks>
    /// Expressed in the unit specified by <see cref="Units"/>.<see cref="UnitSettings.Weight"/>.
    /// </remarks>
    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    /// <summary>
    /// Gets or sets the moment of inertia of the motor's rotor.
    /// </summary>
    /// <remarks>
    /// This affects acceleration response.
    /// </remarks>
    [JsonPropertyName("rotorInertia")]
    public decimal RotorInertia { get; set; }

    /// <summary>
    /// Gets or sets the feedback device pulses per revolution (PPR).
    /// </summary>
    /// <remarks>
    /// Used for encoder or resolver feedback resolution.
    /// </remarks>
    [JsonPropertyName("feedbackPpr")]
    public int FeedbackPpr { get; set; }

    // Brake Properties
    /// <summary>
    /// Gets or sets whether the motor includes an integral holding brake.
    /// </summary>
    [JsonPropertyName("hasBrake")]
    public bool HasBrake { get; set; }

    /// <summary>
    /// Gets or sets the holding torque of the integral brake.
    /// </summary>
    /// <remarks>
    /// Only applicable when <see cref="HasBrake"/> is <see langword="true"/>.
    /// </remarks>
    [JsonPropertyName("brakeTorque")]
    public decimal BrakeTorque { get; set; }

    /// <summary>
    /// Gets or sets the current draw of the brake (A).
    /// </summary>
    /// <remarks>
    /// Only applicable when <see cref="HasBrake"/> is <see langword="true"/>.
    /// </remarks>
    [JsonPropertyName("brakeAmperage")]
    public decimal BrakeAmperage { get; set; }

    /// <summary>
    /// Gets or sets the voltage requirement of the brake (V).
    /// </summary>
    /// <remarks>
    /// Only applicable when <see cref="HasBrake"/> is <see langword="true"/>.
    /// </remarks>
    [JsonPropertyName("brakeVoltage")]
    public decimal BrakeVoltage { get; set; }

    /// <summary>
    /// Gets or sets the release time of the brake.
    /// </summary>
    [JsonPropertyName("brakeReleaseTime")]
    public decimal BrakeReleaseTime { get; set; }

    /// <summary>
    /// Gets or sets the brake engage time when using a diode.
    /// </summary>
    [JsonPropertyName("brakeEngageTimeDiode")]
    public decimal BrakeEngageTimeDiode { get; set; }

    /// <summary>
    /// Gets or sets the brake engage time when using an MOV.
    /// </summary>
    [JsonPropertyName("brakeEngageTimeMOV")]
    public decimal BrakeEngageTimeMov { get; set; }

    /// <summary>
    /// Gets or sets the backlash of the brake mechanism.
    /// </summary>
    [JsonPropertyName("brakeBacklash")]
    public decimal BrakeBacklash { get; set; }

    // Units Configuration
    /// <summary>
    /// Gets or sets the unit settings for this motor definition.
    /// </summary>
    [JsonPropertyName("units")]
    public UnitSettings Units { get; set; } = new();

    // Drive Configurations
    /// <summary>
    /// Gets or sets the drive configurations for this motor.
    /// </summary>
    /// <remarks>
    /// Each drive can have multiple voltages with their own curves.
    /// </remarks>
    [JsonPropertyName("drives")]
    public List<Drive> Drives { get; set; } = [];

    /// <summary>
    /// Gets the drive names in this motor definition.
    /// Useful for populating UI lists and combo-boxes.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<string> DriveNames => Drives.Select(d => d.Name);

    // Metadata
    /// <summary>
    /// Gets or sets metadata about the motor definition file.
    /// </summary>
    [JsonPropertyName("metadata")]
    public MotorMetadata Metadata { get; set; } = new();

    // Data Integrity
    /// <summary>
    /// Gets or sets the validation signature for motor properties.
    /// </summary>
    /// <remarks>
    /// This signature covers motor-level properties only (excludes drives and metadata).
    /// Null indicates the motor properties have not been signed.
    /// </remarks>
    [JsonPropertyName("motorSignature")]
    public ValidationSignature? MotorSignature { get; set; }

    /// <summary>
    /// Creates a new ServoMotor with default values.
    /// </summary>
    public ServoMotor()
    {
    }

    /// <summary>
    /// Creates a new ServoMotor with the specified motor name.
    /// </summary>
    /// <param name="motorName">The name of the motor.</param>
    public ServoMotor(string motorName)
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
                              d.Voltages.Any(v => v.Curves.Count > 0));
    }

    /// <summary>
    /// Gets a drive configuration by name.
    /// </summary>
    /// <param name="name">The name of the drive to find.</param>
    /// <returns>The matching drive configuration, or null if not found.</returns>
    public Drive? GetDriveByName(string name)
    {
        return Drives.Find(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Adds a new drive configuration with the specified name.
    /// </summary>
    /// <param name="name">The name for the new drive.</param>
    /// <returns>The newly created drive configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a drive with the same name already exists.</exception>
    public Drive AddDrive(string name)
    {
        if (GetDriveByName(name) is not null)
        {
            throw new InvalidOperationException($"A drive with the name '{name}' already exists.");
        }

        var drive = new Drive(name);
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
}
