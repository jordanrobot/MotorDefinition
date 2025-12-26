using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinitions.Persistence.Dtos;

/// <summary>
/// Represents a series entry in the persisted series map.
/// </summary>
internal sealed class SeriesEntryDto
{
    [JsonPropertyName("locked")]
    public bool Locked { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("torque")]
    public double[] Torque { get; set; } = [];
}
