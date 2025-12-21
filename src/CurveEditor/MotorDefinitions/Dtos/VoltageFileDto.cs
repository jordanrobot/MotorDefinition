using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace jordanrobot.MotorDefinitions.Dtos;

/// <summary>
/// Represents voltage-specific data using shared axes and a series map.
/// </summary>
internal sealed class VoltageFileDto
{
    [JsonPropertyName("voltage")]
    public double Voltage { get; set; }

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

    [JsonPropertyName("continuousAmperage")]
    public double ContinuousAmperage { get; set; }

    [JsonPropertyName("peakAmperage")]
    public double PeakAmperage { get; set; }

    [JsonPropertyName("percent")]
    public int[] Percent { get; set; } = [];

    [JsonPropertyName("rpm")]
    public double[] Rpm { get; set; } = [];

    [JsonPropertyName("series")]
    public IDictionary<string, SeriesEntryDto> Series { get; set; } = new SortedDictionary<string, SeriesEntryDto>();
}
