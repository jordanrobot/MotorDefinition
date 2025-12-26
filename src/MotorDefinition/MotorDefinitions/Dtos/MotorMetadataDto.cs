using System;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinitions.Persistence.Dtos;

/// <summary>
/// Persistence DTO for motor metadata.
/// </summary>
internal sealed class MotorMetadataDto
{
    [JsonPropertyName("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("modified")]
    public DateTime Modified { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
