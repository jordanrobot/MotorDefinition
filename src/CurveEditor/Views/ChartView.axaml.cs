using Avalonia.Controls;

namespace CurveEditor.Views;

/// <summary>
/// User control for displaying motor torque curves using LiveCharts2.
/// </summary>
public partial class ChartView : UserControl
{
    /// <summary>
    /// Creates a new ChartView instance.
    /// </summary>
    public ChartView()
    {
        InitializeComponent();
    }
}
