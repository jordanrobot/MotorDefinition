using System;
using System.Text.Json.Serialization;

namespace CurveEditor.Models;

/// <summary>
/// Contains metadata about the motor definition file.
/// </summary>
public class MotorMetadata
{
    /// <summary>
    /// The date and time when the motor definition was created.
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The date and time when the motor definition was last modified.
    /// </summary>
    [JsonPropertyName("modified")]
    public DateTime Modified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional notes about the motor definition (e.g., test conditions).
    /// </summary>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Updates the modified timestamp to the current UTC time.
    /// </summary>
    public void UpdateModified()
    {
        Modified = DateTime.UtcNow;
    }
}
