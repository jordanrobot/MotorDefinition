namespace MotorEditor.Avalonia.Models;

/// <summary>
/// Describes a panel in the application, including its behavior,
/// zone assignment, and default sizing.
/// </summary>
public class PanelDescriptor
{
    /// <summary>
    /// Stable identifier for the panel (must remain consistent for persistence).
    /// </summary>
    public required string PanelId { get; init; }

    /// <summary>
    /// Display name shown in panel headers.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Text label shown in the Panel Bar (Phase 3.0 uses text labels).
    /// </summary>
    public required string PanelBarLabel { get; init; }

    /// <summary>
    /// Whether this panel should appear as an icon in the Panel Bar.
    /// </summary>
    public bool EnableIcon { get; init; } = true;

    /// <summary>
    /// Whether this panel can be collapsed via the Panel Bar.
    /// </summary>
    public bool EnableCollapse { get; init; } = true;

    /// <summary>
    /// The zone where this panel is docked.
    /// </summary>
    public PanelZone Zone { get; init; } = PanelZone.Left;

    /// <summary>
    /// Default width in pixels (used when panel is in left/right zone).
    /// </summary>
    public double DefaultWidth { get; init; } = 200;

    /// <summary>
    /// Default height in pixels (used when panel is in bottom zone).
    /// </summary>
    public double DefaultHeight { get; init; } = 200;

    /// <summary>
    /// Minimum size in pixels (optional).
    /// </summary>
    public double? MinSize { get; init; }

    /// <summary>
    /// Icon glyph or text to display in the Panel Bar.
    /// </summary>
    public string IconGlyph { get; init; } = "■";
}
