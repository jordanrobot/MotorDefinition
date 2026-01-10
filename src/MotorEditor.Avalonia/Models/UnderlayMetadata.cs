namespace MotorEditor.Avalonia.Models;

/// <summary>
/// Persisted image underlay state for a specific motor/drive/voltage combination.
/// Stored alongside motor files in the .motorEditor folder.
/// </summary>
public sealed class UnderlayMetadata
{
    /// <summary>
    /// Absolute path to the selected image. Null indicates no image is assigned.
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Whether the underlay should be rendered.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// When true, the image stays anchored to the chart origin and offsets are suppressed.
    /// </summary>
    public bool LockZero { get; set; }

    /// <summary>
    /// Horizontal scale multiplier, relative to the chart draw margin width.
    /// </summary>
    public double XScale { get; set; } = 1d;

    /// <summary>
    /// Vertical scale multiplier, relative to the chart draw margin height.
    /// </summary>
    public double YScale { get; set; } = 1d;

    /// <summary>
    /// Horizontal offset expressed as a fraction of the chart draw margin width.
    /// Positive values move the image right.
    /// </summary>
    public double OffsetX { get; set; }

    /// <summary>
    /// Vertical offset expressed as a fraction of the chart draw margin height.
    /// Positive values move the image upward.
    /// </summary>
    public double OffsetY { get; set; }

    /// <summary>
    /// Opacity applied to the underlay image (0 = fully transparent, 1 = fully opaque).
    /// </summary>
    public double Opacity { get; set; } = 0.45;
}
