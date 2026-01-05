using System;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Persistence.Dtos;

/// <summary>
/// DTO for persisting validation signatures in JSON files.
/// </summary>
internal sealed class ValidationSignatureDto
{
    [JsonPropertyName("checksum")]
    public string Checksum { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("verifiedBy")]
    public string VerifiedBy { get; set; } = string.Empty;

    [JsonPropertyName("algorithm")]
    public string Algorithm { get; set; } = "SHA256";
}
