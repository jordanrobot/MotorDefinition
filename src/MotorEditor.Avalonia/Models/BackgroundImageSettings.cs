using System.Collections.Generic;

namespace MotorEditor.Avalonia.Models;

/// <summary>
/// Settings for a background image associated with a specific drive and voltage.
/// </summary>
public class BackgroundImageSettings
{
    /// <summary>
    /// Path to the background image file.
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    /// Name of the drive this image is associated with.
    /// </summary>
    public string DriveName { get; set; } = string.Empty;

    /// <summary>
    /// Voltage value this image is associated with.
    /// </summary>
    public double VoltageValue { get; set; }

    /// <summary>
    /// Whether the image is currently visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Whether the image is locked to the graph zero point.
    /// </summary>
    public bool IsLockedToZero { get; set; } = true;

    /// <summary>
    /// X-axis offset from the graph zero point (in data units).
    /// </summary>
    public double OffsetX { get; set; }

    /// <summary>
    /// Y-axis offset from the graph zero point (in data units).
    /// </summary>
    public double OffsetY { get; set; }

    /// <summary>
    /// X-axis scale factor (1.0 = 100%).
    /// </summary>
    public double ScaleX { get; set; } = 1.0;

    /// <summary>
    /// Y-axis scale factor (1.0 = 100%).
    /// </summary>
    public double ScaleY { get; set; } = 1.0;
}

/// <summary>
/// Container for all background image settings for a motor file.
/// </summary>
public class MotorBackgroundImageSettings
{
    /// <summary>
    /// List of background image settings for each drive/voltage combination.
    /// </summary>
    public List<BackgroundImageSettings> Images { get; set; } = new();
}
