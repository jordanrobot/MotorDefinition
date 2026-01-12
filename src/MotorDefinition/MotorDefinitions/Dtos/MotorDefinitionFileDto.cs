using JordanRobot.MotorDefinition.Model;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Persistence.Dtos;

/// <summary>
/// Root DTO representing the persisted motor definition file.
/// </summary>
internal sealed class MotorDefinitionFileDto
{
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = Model.ServoMotor.CurrentSchemaVersion;

    [JsonPropertyName("motorName")]
    public string MotorName { get; set; } = string.Empty;

    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    [JsonPropertyName("partNumber")]
    public string PartNumber { get; set; } = string.Empty;

    [JsonPropertyName("power")]
    public decimal Power { get; set; }

    [JsonPropertyName("maxSpeed")]
    public decimal MaxSpeed { get; set; }

    [JsonPropertyName("ratedSpeed")]
    public decimal RatedSpeed { get; set; }

    [JsonPropertyName("ratedContinuousTorque")]
    public decimal RatedContinuousTorque { get; set; }

    [JsonPropertyName("ratedPeakTorque")]
    public decimal RatedPeakTorque { get; set; }

    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    [JsonPropertyName("rotorInertia")]
    public decimal RotorInertia { get; set; }

    [JsonPropertyName("feedbackPpr")]
    public int FeedbackPpr { get; set; }

    [JsonPropertyName("hasBrake")]
    public bool HasBrake { get; set; }

    [JsonPropertyName("brakeTorque")]
    public decimal BrakeTorque { get; set; }

    [JsonPropertyName("brakeAmperage")]
    public decimal BrakeAmperage { get; set; }

    [JsonPropertyName("brakeVoltage")]
    public decimal BrakeVoltage { get; set; }

    [JsonPropertyName("brakeReleaseTime")]
    public decimal BrakeReleaseTime { get; set; }

    [JsonPropertyName("brakeEngageTimeDiode")]
    public decimal BrakeEngageTimeDiode { get; set; }

    [JsonPropertyName("brakeEngageTimeMOV")]
    public decimal BrakeEngageTimeMov { get; set; }

    [JsonPropertyName("brakeBacklash")]
    public decimal BrakeBacklash { get; set; }

    [JsonPropertyName("units")]
    public UnitSettingsDto Units { get; set; } = new();

    [JsonPropertyName("drives")]
    public List<DriveFileDto> Drives { get; set; } = [];

    [JsonPropertyName("metadata")]
    public MotorMetadataDto Metadata { get; set; } = new();

    [JsonPropertyName("motorSignature")]
    public ValidationSignatureDto? MotorSignature { get; set; }
}
