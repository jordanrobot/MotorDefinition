using Avalonia.Controls;
using CurveEditor.ViewModels;

namespace CurveEditor.Views;

/// <summary>
/// Panel for displaying and editing curve series data points.
/// </summary>
public partial class CurveDataPanel : UserControl
{
    /// <summary>
    /// Creates a new CurveDataPanel instance.
    /// </summary>
    public CurveDataPanel()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles when a cell edit is completed to trigger dirty state.
    /// </summary>
    private void DataGrid_CellEditEnded(object? sender, Avalonia.Controls.DataGridCellEditEndedEventArgs e)
    {
        // Mark data as dirty when edited
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.MarkDirty();
            viewModel.ChartViewModel.RefreshChart();
        }
    }
}
