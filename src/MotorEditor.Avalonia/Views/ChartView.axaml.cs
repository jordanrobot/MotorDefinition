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
    private bool _isSketchDragging;
    private bool _isPanning;
    private Point _panStart;
    private Point _dragStart;
    private double _startOffsetX;
    private double _startOffsetY;
    private Rect _underlayBounds = new();
    private bool _underlayLayoutQueued;

    /// <summary>
    /// Tracks the last known pointer position over the chart in
    /// data-space coordinates, used as the focus point for keyboard
    /// zoom (+/- keys).
    /// </summary>
    private Point _lastPointerDataPosition;

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

        // Use tunneling (handledEventsToo: true) for wheel events so we
        // intercept them before LiveCharts processes them internally.
        TorqueChart.AddHandler(
            PointerWheelChangedEvent,
            OnChartPointerWheelChanged,
            Avalonia.Interactivity.RoutingStrategies.Tunnel,
            handledEventsToo: true);

        // Handle mouse clicks on the chart to support basic point
        // selection. This wiring keeps the interaction logic in the
        // view while delegating selection state to the EditingCoordinator
        // via the ChartViewModel.
        TorqueChart.PointerPressed += OnChartPointerPressed;

        // Wire keyboard zoom to the chart control itself and ensure
        // it can receive focus when the pointer enters the chart area.
        TorqueChart.Focusable = true;
        TorqueChart.KeyDown += OnChartKeyDown;
        TorqueChart.PointerEntered += OnChartPointerEntered;
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

    /// <summary>
    /// Gives the chart control keyboard focus when the pointer enters,
    /// so that +/- zoom keys work without requiring a click first.
    /// </summary>
    private void OnChartPointerEntered(object? sender, PointerEventArgs e)
    {
        TorqueChart.Focus();
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

        // Middle-button double-click resets zoom to unzoomed view.
        if (pointerPoint.Properties.IsMiddleButtonPressed && e.ClickCount == 2)
        {
            vm.ResetZoom();
            QueueUnderlayLayout();
            e.Handled = true;
            return;
        }

        // Middle-button single press begins a pan drag.
        if (pointerPoint.Properties.IsMiddleButtonPressed && e.ClickCount <= 1)
        {
            _isPanning = true;
            _panStart = e.GetPosition(TorqueChart);
            e.Pointer.Capture(TorqueChart);
            e.Handled = true;
            return;
        }

        if (!pointerPoint.Properties.IsLeftButtonPressed)
        {
            return;
        }

        // When sketch-edit mode is active, apply the sketch point
        // and begin tracking the drag.
        if (vm.IsSketchEditActive)
        {
            if (TryApplySketchAtPixel(vm, e.GetPosition(TorqueChart)))
            {
                _isSketchDragging = true;
                e.Pointer.Capture(TorqueChart);
                e.Handled = true;
            }

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
        if (_chartViewModel is null)
        {
            return;
        }

        // Track the data-space position for keyboard zoom focus.
        var pixelPos = e.GetPosition(TorqueChart);
        if (TryPixelToDataSpace(pixelPos, out var dataX, out var dataY))
        {
            _lastPointerDataPosition = new Point(dataX, dataY);
        }

        // Middle-mouse pan: convert pixel delta to data-space delta.
        if (_isPanning)
        {
            var currentPos = e.GetPosition(TorqueChart);
            var pixelDelta = currentPos - _panStart;
            _panStart = currentPos;

            if (TryPixelDeltaToDataDelta(pixelDelta, out var dataDeltaX, out var dataDeltaY))
            {
                _chartViewModel.PanBy(dataDeltaX, dataDeltaY);
                QueueUnderlayLayout();
            }

            e.Handled = true;
            return;
        }

        // Continue sketch-edit drag: apply sketch point as the mouse moves.
        if (_isSketchDragging && _chartViewModel.IsSketchEditActive)
        {
            TryApplySketchAtPixel(_chartViewModel, pixelPos);
            e.Handled = true;
            return;
        }

        if (!_isUnderlayDragging)
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
        if (_isPanning)
        {
            _isPanning = false;
            e.Pointer.Capture(null);
            e.Handled = true;
            return;
        }

        if (_isSketchDragging)
        {
            _isSketchDragging = false;
            e.Pointer.Capture(null);
            e.Handled = true;
            return;
        }

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
        _isSketchDragging = false;
        _isPanning = false;
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

    /// <summary>
    /// Converts a pixel position on the chart to data-space coordinates
    /// and forwards it to the view model's sketch-edit logic.
    /// </summary>
    private bool TryApplySketchAtPixel(ChartViewModel vm, Point pixelPosition)
    {
        var chart = TorqueChart.CoreChart;
        if (chart is null)
        {
            return false;
        }

        var drawLocation = chart.DrawMarginLocation;
        var drawSize = chart.DrawMarginSize;
        if (drawSize.Width <= 0 || drawSize.Height <= 0)
        {
            return false;
        }

        // Only apply when the pointer is within the draw margin area.
        if (pixelPosition.X < drawLocation.X ||
            pixelPosition.X > drawLocation.X + drawSize.Width ||
            pixelPosition.Y < drawLocation.Y ||
            pixelPosition.Y > drawLocation.Y + drawSize.Height)
        {
            return false;
        }

        // Use the primary X and Y axes to convert pixel to data coordinates.
        var xAxes = vm.XAxes;
        var yAxes = vm.YAxes;
        if (xAxes.Length == 0 || yAxes.Length == 0)
        {
            return false;
        }

        var xAxis = xAxes[0];
        var yAxis = yAxes[0];

        var xMin = xAxis.MinLimit ?? 0;
        var xMax = xAxis.MaxLimit ?? 1;
        var yMin = yAxis.MinLimit ?? 0;
        var yMax = yAxis.MaxLimit ?? 1;

        if (xMax - xMin <= 0 || yMax - yMin <= 0)
        {
            return false;
        }

        // Linear interpolation from pixel space to data space.
        var chartX = xMin + (pixelPosition.X - drawLocation.X) / drawSize.Width * (xMax - xMin);
        var chartY = yMax - (pixelPosition.Y - drawLocation.Y) / drawSize.Height * (yMax - yMin);

        return vm.ApplySketchPoint(chartX, chartY);
    }

    /// <summary>
    /// Converts a pixel position on the chart to data-space coordinates
    /// using the current axis limits.
    /// </summary>
    private bool TryPixelToDataSpace(Point pixelPosition, out double dataX, out double dataY)
    {
        dataX = 0;
        dataY = 0;

        if (_chartViewModel is null)
        {
            return false;
        }

        var chart = TorqueChart.CoreChart;
        if (chart is null)
        {
            return false;
        }

        var drawLocation = chart.DrawMarginLocation;
        var drawSize = chart.DrawMarginSize;
        if (drawSize.Width <= 0 || drawSize.Height <= 0)
        {
            return false;
        }

        var xAxes = _chartViewModel.XAxes;
        var yAxes = _chartViewModel.YAxes;
        if (xAxes.Length == 0 || yAxes.Length == 0)
        {
            return false;
        }

        var xMin = xAxes[0].MinLimit ?? 0;
        var xMax = xAxes[0].MaxLimit ?? 1;
        var yMin = yAxes[0].MinLimit ?? 0;
        var yMax = yAxes[0].MaxLimit ?? 1;

        if (xMax - xMin <= 0 || yMax - yMin <= 0)
        {
            return false;
        }

        dataX = xMin + (pixelPosition.X - drawLocation.X) / drawSize.Width * (xMax - xMin);
        dataY = yMax - (pixelPosition.Y - drawLocation.Y) / drawSize.Height * (yMax - yMin);
        return true;
    }

    /// <summary>
    /// Converts a pixel delta (movement) to data-space delta for panning.
    /// </summary>
    private bool TryPixelDeltaToDataDelta(Point pixelDelta, out double deltaX, out double deltaY)
    {
        deltaX = 0;
        deltaY = 0;

        if (_chartViewModel is null)
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

        var xAxes = _chartViewModel.XAxes;
        var yAxes = _chartViewModel.YAxes;
        if (xAxes.Length == 0 || yAxes.Length == 0)
        {
            return false;
        }

        var xRange = (xAxes[0].MaxLimit ?? 1) - (xAxes[0].MinLimit ?? 0);
        var yRange = (yAxes[0].MaxLimit ?? 1) - (yAxes[0].MinLimit ?? 0);

        if (xRange <= 0 || yRange <= 0)
        {
            return false;
        }

        deltaX = pixelDelta.X / drawSize.Width * xRange;
        // Y axis is inverted: positive pixel delta = downward = negative data Y.
        deltaY = -(pixelDelta.Y / drawSize.Height * yRange);
        return true;
    }

    /// <summary>
    /// Handles Ctrl+mouse wheel / Ctrl+touchpad scroll to zoom the chart.
    /// Also handles Ctrl+touchpad pinch-to-zoom (reported as wheel events
    /// with Ctrl modifier on most platforms).
    /// </summary>
    private void OnChartPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_chartViewModel is null)
        {
            return;
        }

        // Only zoom when the Ctrl key is held.
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            return;
        }

        var pixelPos = e.GetPosition(TorqueChart);
        if (!TryPixelToDataSpace(pixelPos, out var focusX, out var focusY))
        {
            return;
        }

        // Delta.Y is positive for scroll-up (zoom in) and negative for scroll-down (zoom out).
        _chartViewModel.ApplyZoom(focusX, focusY, e.Delta.Y);
        QueueUnderlayLayout();
        e.Handled = true;
    }

    /// <summary>
    /// Handles keyboard zoom: <c>+</c> to zoom in, <c>-</c> to zoom out,
    /// <c>=</c> to reset zoom to the unzoomed view.
    /// </summary>
    private void OnChartKeyDown(object? sender, KeyEventArgs e)
    {
        if (_chartViewModel is null)
        {
            return;
        }

        // + key (Shift+= on most keyboards) or numpad + — zoom in.
        if ((e.Key is Key.OemPlus && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            || e.Key is Key.Add)
        {
            _chartViewModel.ApplyZoom(
                _lastPointerDataPosition.X,
                _lastPointerDataPosition.Y,
                1.0);
            QueueUnderlayLayout();
            e.Handled = true;
            return;
        }

        // - key or numpad - — zoom out.
        if (e.Key is Key.OemMinus or Key.Subtract)
        {
            _chartViewModel.ApplyZoom(
                _lastPointerDataPosition.X,
                _lastPointerDataPosition.Y,
                -1.0);
            QueueUnderlayLayout();
            e.Handled = true;
            return;
        }

        // = key (OemPlus without Shift) — reset zoom.
        if (e.Key is Key.OemPlus && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            _chartViewModel.ResetZoom();
            QueueUnderlayLayout();
            e.Handled = true;
        }
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

        // Base image size and position in the unzoomed coordinate system.
        var baseImageWidth = drawSize.Width * _chartViewModel.UnderlayXScale;
        var baseImageHeight = drawSize.Height * _chartViewModel.UnderlayYScale;

        var baseX = drawLocation.X + (_chartViewModel.UnderlayOffsetX * drawSize.Width);
        var baseY = drawLocation.Y + drawSize.Height - baseImageHeight
                    - (_chartViewModel.UnderlayOffsetY * drawSize.Height);

        double imageWidth;
        double imageHeight;
        double x;
        double y;

        // When zoom is active and base limits have been captured, scale
        // the underlay image to keep it aligned with the chart data.
        // The draw-margin pixel area stays the same but represents a
        // narrower data range, so the image must grow proportionally
        // and shift so that the visible slice matches.
        if (_chartViewModel.BaseLimitsCaptured && _chartViewModel.ZoomLevel > 1.001)
        {
            var xAxes = _chartViewModel.XAxes;
            var yAxes = _chartViewModel.YAxes;
            if (xAxes.Length > 0 && yAxes.Length > 0)
            {
                var baseXRange = _chartViewModel.BaseXMax - _chartViewModel.BaseXMin;
                var baseYRange = _chartViewModel.BaseYMax - _chartViewModel.BaseYMin;
                var curXMin = xAxes[0].MinLimit ?? _chartViewModel.BaseXMin;
                var curXMax = xAxes[0].MaxLimit ?? _chartViewModel.BaseXMax;
                var curYMin = yAxes[0].MinLimit ?? _chartViewModel.BaseYMin;
                var curYMax = yAxes[0].MaxLimit ?? _chartViewModel.BaseYMax;
                var curXRange = curXMax - curXMin;
                var curYRange = curYMax - curYMin;

                if (curXRange > 0 && curYRange > 0 && baseXRange > 0 && baseYRange > 0)
                {
                    var xZoom = baseXRange / curXRange;
                    var yZoom = baseYRange / curYRange;

                    imageWidth = baseImageWidth * xZoom;
                    imageHeight = baseImageHeight * yZoom;

                    // Shift so the image data alignment is preserved.
                    // The base image left edge corresponds to a data-space X
                    // offset. We need to map that into the zoomed pixel space.
                    x = drawLocation.X + (baseX - drawLocation.X) * xZoom
                        - (curXMin - _chartViewModel.BaseXMin) / baseXRange * drawSize.Width * xZoom;
                    // Y axis is inverted (top of draw area = max data value).
                    y = drawLocation.Y + (baseY - drawLocation.Y) * yZoom
                        - (_chartViewModel.BaseYMax - curYMax) / baseYRange * drawSize.Height * yZoom;
                }
                else
                {
                    imageWidth = baseImageWidth;
                    imageHeight = baseImageHeight;
                    x = baseX;
                    y = baseY;
                }
            }
            else
            {
                imageWidth = baseImageWidth;
                imageHeight = baseImageHeight;
                x = baseX;
                y = baseY;
            }
        }
        else
        {
            imageWidth = baseImageWidth;
            imageHeight = baseImageHeight;
            x = baseX;
            y = baseY;
        }

        Canvas.SetLeft(UnderlayImage, x);
        Canvas.SetTop(UnderlayImage, y);
        UnderlayImage.Width = imageWidth;
        UnderlayImage.Height = imageHeight;
        UnderlayImage.Source = _chartViewModel.UnderlayImage;
        UnderlayImage.IsVisible = _chartViewModel.UnderlayVisible && _chartViewModel.UnderlayImage is not null;
        _underlayBounds = new Rect(x, y, imageWidth, imageHeight);
    }
}
