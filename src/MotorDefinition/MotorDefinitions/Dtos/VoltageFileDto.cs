using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Persistence.Dtos;

/// <summary>
/// Represents voltage-specific data using shared axes and a series map.
/// </summary>
internal sealed class VoltageFileDto
{
    [JsonPropertyName("voltage")]
    public decimal Voltage { get; set; }

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

    [JsonPropertyName("continuousAmperage")]
    public decimal ContinuousAmperage { get; set; }

    [JsonPropertyName("peakAmperage")]
    public decimal PeakAmperage { get; set; }

    [JsonPropertyName("percent")]
    public int[] Percent { get; set; } = [];

    [JsonPropertyName("rpm")]
    public decimal[] Rpm { get; set; } = [];

    [JsonPropertyName("series")]
    public IDictionary<string, SeriesEntryDto> Series { get; set; } = new SortedDictionary<string, SeriesEntryDto>();
}
