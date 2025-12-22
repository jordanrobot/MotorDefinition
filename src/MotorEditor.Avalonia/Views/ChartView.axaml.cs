using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CurveEditor.ViewModels;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;

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

        // Handle mouse clicks on the chart to support basic point
        // selection. This wiring keeps the interaction logic in the
        // view while delegating selection state to the EditingCoordinator
        // via the ChartViewModel.
        TorqueChart.PointerPressed += OnChartPointerPressed;
    }

    private void OnChartPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not ChartViewModel vm)
        {
            return;
        }

        var pointerPoint = e.GetCurrentPoint(TorqueChart);
        if (!pointerPoint.Properties.IsLeftButtonPressed)
        {
            return;
        }

        // Use the underlying chart to find the nearest point under the
        // cursor within a reasonable radius.
        var chart = TorqueChart.CoreChart;
        if (chart is null)
        {
            return;
        }

        var position = e.GetPosition(TorqueChart);
        var location = new LiveChartsCore.Drawing.LvcPointD(position.X, position.Y);
        var foundPoint = chart.GetPointsAt(location).FirstOrDefault();
        if (foundPoint is null)
        {
            return;
        }

        if (foundPoint.Context.Series is not LineSeries<ObservablePoint> lineSeries)
        {
            return;
        }

        var seriesName = lineSeries.Name;
        if (string.IsNullOrWhiteSpace(seriesName))
        {
            return;
        }

        // Resolve the index of the clicked point within the series by
        // matching the underlying ObservablePoint instance. This keeps
        // the logic aligned with how the series values are constructed
        // in the ChartViewModel.
        if (foundPoint.Context.DataSource is not ObservablePoint observablePoint)
        {
            return;
        }

        if (lineSeries.Values is null)
        {
            return;
        }

        var values = lineSeries.Values as IEnumerable<ObservablePoint> ??
                     lineSeries.Values.OfType<ObservablePoint>();
        var index = values
            .Select((p, i) => new { Point = p, Index = i })
            .FirstOrDefault(x => ReferenceEquals(x.Point, observablePoint))?.Index ?? -1;

        if (index < 0)
        {
            return;
        }

        vm.HandleChartPointClick(seriesName, index, e.KeyModifiers);
    }
}
