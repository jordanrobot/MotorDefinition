using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace jordanrobot.MotorDefinitions.Dtos;

/// <summary>
/// Root DTO representing the persisted motor definition file.
/// </summary>
internal sealed class MotorDefinitionFileDto
{
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = CurveEditor.Models.MotorDefinition.CurrentSchemaVersion;

    [JsonPropertyName("motorName")]
    public string MotorName { get; set; } = string.Empty;

    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    [JsonPropertyName("partNumber")]
    public string PartNumber { get; set; } = string.Empty;

    [JsonPropertyName("power")]
    public double Power { get; set; }

    [JsonPropertyName("maxSpeed")]
    public double MaxSpeed { get; set; }

    [JsonPropertyName("ratedSpeed")]
    public double RatedSpeed { get; set; }

    [JsonPropertyName("ratedContinuousTorque")]
    public double RatedContinuousTorque { get; set; }

    [JsonPropertyName("ratedPeakTorque")]
    public double RatedPeakTorque { get; set; }

    [JsonPropertyName("weight")]
    public double Weight { get; set; }

    [JsonPropertyName("rotorInertia")]
    public double RotorInertia { get; set; }

    [JsonPropertyName("feedbackPpr")]
    public int FeedbackPpr { get; set; }

    [JsonPropertyName("hasBrake")]
    public bool HasBrake { get; set; }

    [JsonPropertyName("brakeTorque")]
    public double BrakeTorque { get; set; }

    [JsonPropertyName("brakeAmperage")]
    public double BrakeAmperage { get; set; }

    [JsonPropertyName("brakeVoltage")]
    public double BrakeVoltage { get; set; }

    [JsonPropertyName("brakeResponseTime")]
    public double BrakeResponseTime { get; set; }

    [JsonPropertyName("brakeEngageTimeDiode")]
    public double BrakeEngageTimeDiode { get; set; }

    [JsonPropertyName("brakeEngageTimeMOV")]
    public double BrakeEngageTimeMov { get; set; }

    [JsonPropertyName("brakeBacklash")]
    public double BrakeBacklash { get; set; }

    [JsonPropertyName("units")]
    public UnitSettingsDto Units { get; set; } = new();

    [JsonPropertyName("drives")]
    public List<DriveFileDto> Drives { get; set; } = [];

    [JsonPropertyName("metadata")]
    public MotorMetadataDto Metadata { get; set; } = new();
}
