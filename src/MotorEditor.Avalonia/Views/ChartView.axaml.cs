using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
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
    private ChartViewModel? _chartViewModel;
    private bool _isUnderlayDragging;
    private Point _dragStart;
    private double _startOffsetX;
    private double _startOffsetY;
    private Rect _underlayBounds = new();
    private bool _underlayLayoutQueued;

    /// <summary>
    /// Creates a new ChartView instance.
    /// </summary>
    public ChartView()
    {
        InitializeComponent();

        TorqueChart.PointerMoved += OnChartPointerMoved;
        TorqueChart.PointerReleased += OnChartPointerReleased;
        TorqueChart.PointerCaptureLost += OnChartPointerCaptureLost;
        TorqueChart.SizeChanged += (_, _) => QueueUnderlayLayout();
        TorqueChart.LayoutUpdated += (_, _) => QueueUnderlayLayout();
        TorqueChart.UpdateFinished += _ => QueueUnderlayLayout();

        // Handle mouse clicks on the chart to support basic point
        // selection. This wiring keeps the interaction logic in the
        // view while delegating selection state to the EditingCoordinator
        // via the ChartViewModel.
        TorqueChart.PointerPressed += OnChartPointerPressed;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_chartViewModel is not null)
        {
            _chartViewModel.PropertyChanged -= OnChartViewModelPropertyChanged;
        }

        _chartViewModel = DataContext as ChartViewModel;
        if (_chartViewModel is not null)
        {
            _chartViewModel.PropertyChanged += OnChartViewModelPropertyChanged;
        }

        base.OnDataContextChanged(e);
        QueueUnderlayLayout();
    }

    private void OnChartPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (TryBeginUnderlayDrag(e))
        {
            e.Handled = true;
            return;
        }

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

    private void OnChartPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isUnderlayDragging || _chartViewModel is null)
        {
            return;
        }

        var chart = TorqueChart.CoreChart;
        if (chart is null)
        {
            return;
        }

        var drawSize = chart.DrawMarginSize;
        if (drawSize.Width <= 0 || drawSize.Height <= 0)
        {
            return;
        }

        var position = e.GetPosition(TorqueChart);
        var delta = position - _dragStart;
        var newOffsetX = _startOffsetX + delta.X / drawSize.Width;
        var newOffsetY = _startOffsetY + (-delta.Y / drawSize.Height);

        _chartViewModel.UnderlayOffsetX = newOffsetX;
        _chartViewModel.UnderlayOffsetY = newOffsetY;

        // Drag feedback should be immediate, so refresh without queuing.
        UpdateUnderlayLayoutCore();
        e.Handled = true;
    }

    private void OnChartPointerReleased(object? sender, PointerEventArgs e)
    {
        if (!_isUnderlayDragging)
        {
            return;
        }

        _isUnderlayDragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void OnChartPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _isUnderlayDragging = false;
    }

    private void OnChartViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ChartViewModel.UnderlayImage)
            or nameof(ChartViewModel.UnderlayVisible)
            or nameof(ChartViewModel.UnderlayLockZero)
            or nameof(ChartViewModel.UnderlayXScale)
            or nameof(ChartViewModel.UnderlayYScale)
            or nameof(ChartViewModel.UnderlayOffsetX)
            or nameof(ChartViewModel.UnderlayOffsetY)
            or nameof(ChartViewModel.XAxes)
            or nameof(ChartViewModel.YAxes)
            or nameof(ChartViewModel.Series))
        {
            QueueUnderlayLayout();
        }
    }

    private bool TryBeginUnderlayDrag(PointerPressedEventArgs e)
    {
        if (_chartViewModel is null ||
            !_chartViewModel.UnderlayVisible ||
            _chartViewModel.UnderlayLockZero ||
            _chartViewModel.UnderlayImage is null ||
            _underlayBounds.Width <= 0 ||
            _underlayBounds.Height <= 0)
        {
            return false;
        }

        var position = e.GetPosition(TorqueChart);
        if (!_underlayBounds.Contains(position))
        {
            return false;
        }

        var chart = TorqueChart.CoreChart;
        if (chart is null)
        {
            return false;
        }

        var drawSize = chart.DrawMarginSize;
        if (drawSize.Width <= 0 || drawSize.Height <= 0)
        {
            return false;
        }

        _isUnderlayDragging = true;
        _dragStart = position;
        _startOffsetX = _chartViewModel.UnderlayOffsetX;
        _startOffsetY = _chartViewModel.UnderlayOffsetY;
        e.Pointer.Capture(TorqueChart);
        return true;
    }

    private void QueueUnderlayLayout()
    {
        if (_underlayLayoutQueued)
        {
            return;
        }

        _underlayLayoutQueued = true;
        Dispatcher.UIThread.Post(ProcessQueuedUnderlayLayout, DispatcherPriority.Render);
    }

    private void ProcessQueuedUnderlayLayout()
    {
        _underlayLayoutQueued = false;
        UpdateUnderlayLayoutCore();
    }

    private void UpdateUnderlayLayoutCore()
    {
        if (_chartViewModel is null)
        {
            return;
        }

        _chartViewModel.RefreshUnderlayAnchors();

        if (_chartViewModel.UnderlayImage is null || !_chartViewModel.UnderlayVisible)
        {
            UnderlayImage.IsVisible = false;
            _underlayBounds = new Rect();
            return;
        }

        var chart = TorqueChart.CoreChart;
        if (chart is null)
        {
            return;
        }

        var drawLocation = chart.DrawMarginLocation;
        var drawSize = chart.DrawMarginSize;
        if (drawSize.Width <= 0 || drawSize.Height <= 0)
        {
            return;
        }

        var imageWidth = drawSize.Width * _chartViewModel.UnderlayXScale;
        var imageHeight = drawSize.Height * _chartViewModel.UnderlayYScale;

        var x = drawLocation.X + (_chartViewModel.UnderlayOffsetX * drawSize.Width);
        var y = drawLocation.Y + drawSize.Height - imageHeight - (_chartViewModel.UnderlayOffsetY * drawSize.Height);

        Canvas.SetLeft(UnderlayImage, x);
        Canvas.SetTop(UnderlayImage, y);
        UnderlayImage.Width = imageWidth;
        UnderlayImage.Height = imageHeight;
        UnderlayImage.Source = _chartViewModel.UnderlayImage;
        UnderlayImage.IsVisible = _chartViewModel.UnderlayVisible && _chartViewModel.UnderlayImage is not null;
        _underlayBounds = new Rect(x, y, imageWidth, imageHeight);
    }
}
