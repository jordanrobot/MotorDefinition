using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinitions.Persistence.Dtos;

/// <summary>
/// Represents a drive configuration in the persisted motor file.
/// </summary>
internal sealed class DriveFileDto
{
    [JsonPropertyOrder(1)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyOrder(2)]
    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    [JsonPropertyOrder(3)]
    [JsonPropertyName("partNumber")]
    public string PartNumber { get; set; } = string.Empty;

    [JsonPropertyOrder(4)]
    [JsonPropertyName("voltages")]
    public List<VoltageFileDto> Voltages { get; set; } = [];
}
