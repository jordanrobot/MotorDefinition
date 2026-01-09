using System.Collections.Generic;
using System.Linq;

namespace MotorEditor.Avalonia.Models;

/// <summary>
/// Registry of all panel descriptors in the application.
/// Provides a central location for panel configuration.
/// </summary>
public static class PanelRegistry
{
    /// <summary>
    /// Stable panel IDs (must not change after release for persistence compatibility).
    /// </summary>
    public static class PanelIds
    {
        public const string DirectoryBrowser = "DirectoryBrowser";
        public const string MotorProperties = "MotorProperties";
        public const string CurveData = "CurveData";
        public const string ChartFormat = "ChartFormat";
        public const string CurveGraph = "CurveGraph";
        // NOTE: This ID is persisted in user settings. Do not change its string value.
        public const string ValidationErrors = "ValidationErrors";

        // Alias to reflect current UX naming. Keep the underlying ID stable.
        public const string BottomPanel = ValidationErrors;
    }

    /// <summary>
    /// All panel descriptors in the application.
    /// </summary>
    public static IReadOnlyList<PanelDescriptor> AllPanels { get; } = new List<PanelDescriptor>
    {
        new PanelDescriptor
        {
            PanelId = PanelIds.DirectoryBrowser,
            DisplayName = "Directory Browser",
            PanelBarLabel = "Browser",
            EnableIcon = true,
            EnableCollapse = true,
            Zone = PanelZone.Left,
            DefaultWidth = 200,
            IconGlyph = "Browser"
        },
        new PanelDescriptor
        {
            PanelId = PanelIds.CurveData,
            DisplayName = "Curve Data",
            PanelBarLabel = "Data",
            EnableIcon = true,
            EnableCollapse = true,
            Zone = PanelZone.Left,
            DefaultWidth = 200,
            IconGlyph = "Data"
        },
        new PanelDescriptor
        {
            PanelId = PanelIds.ChartFormat,
            DisplayName = "Chart Format",
            PanelBarLabel = "Format",
            EnableIcon = true,
            EnableCollapse = true,
            Zone = PanelZone.Left,
            DefaultWidth = 220,
            IconGlyph = "Format"
        },
        new PanelDescriptor
        {
            PanelId = PanelIds.MotorProperties,
            DisplayName = "Motor Properties",
            PanelBarLabel = "Properties",
            EnableIcon = true,
            EnableCollapse = true,
            Zone = PanelZone.Right,
            DefaultWidth = 280,
            IconGlyph = "Properties"
        },
        new PanelDescriptor
        {
            PanelId = PanelIds.CurveGraph,
            DisplayName = "Curve Graph",
            PanelBarLabel = "",
            EnableIcon = false,
            EnableCollapse = false,
            Zone = PanelZone.Center,
            MinSize = 400
        },
        new PanelDescriptor
        {
            PanelId = PanelIds.BottomPanel,
            DisplayName = "Bottom Panel",
            PanelBarLabel = "Bottom",
            EnableIcon = true,
            EnableCollapse = true,
            Zone = PanelZone.Bottom,
            DefaultHeight = 150,
            IconGlyph = "Bottom"
        }
    };

    /// <summary>
    /// Gets panels that should appear in the Panel Bar.
    /// </summary>
    public static IEnumerable<PanelDescriptor> PanelBarPanels =>
        AllPanels.Where(p => p.EnableIcon);

    /// <summary>
    /// Gets a panel descriptor by its ID.
    /// </summary>
    public static PanelDescriptor? GetById(string panelId) =>
        AllPanels.FirstOrDefault(p => p.PanelId == panelId);
}
