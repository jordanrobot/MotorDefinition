using CommunityToolkit.Mvvm.ComponentModel;
using CurveEditor.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CurveEditor.ViewModels;

/// <summary>
/// ViewModel for the torque curve chart visualization.
/// Manages series data, axes configuration, and chart styling.
/// </summary>
public partial class ChartViewModel : ViewModelBase
{
    /// <summary>
    /// Predefined colors for curve series.
    /// </summary>
    private static readonly SKColor[] SeriesColors =
    [
        new SKColor(220, 50, 50),    // Red (Peak)
        new SKColor(50, 150, 220),   // Blue (Continuous)
        new SKColor(50, 180, 100),   // Green
        new SKColor(200, 130, 50),   // Orange
        new SKColor(150, 80, 200),   // Purple
        new SKColor(50, 200, 180),   // Teal
        new SKColor(200, 50, 150),   // Pink
        new SKColor(100, 100, 100),  // Gray
    ];

    private VoltageConfiguration? _currentVoltage;
    private readonly Dictionary<string, ObservableCollection<ObservablePoint>> _seriesDataCache = [];
    private readonly Dictionary<string, bool> _seriesVisibility = [];

    [ObservableProperty]
    private ObservableCollection<ISeries> _series = [];

    [ObservableProperty]
    private Axis[] _xAxes;

    [ObservableProperty]
    private Axis[] _yAxes;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _torqueUnit = "Nm";

    /// <summary>
    /// Event raised when any series data point changes.
    /// </summary>
    public event EventHandler? DataChanged;

    /// <summary>
    /// Creates a new ChartViewModel with default configuration.
    /// </summary>
    public ChartViewModel()
    {
        _xAxes = CreateXAxes();
        _yAxes = CreateYAxes();
    }

    /// <summary>
    /// Gets or sets the current voltage configuration whose series are displayed.
    /// </summary>
    public VoltageConfiguration? CurrentVoltage
    {
        get => _currentVoltage;
        set
        {
            if (_currentVoltage == value) return;
            _currentVoltage = value;
            OnPropertyChanged();
            UpdateChart();
        }
    }

    /// <summary>
    /// Sets the visibility of a series by name.
    /// </summary>
    /// <param name="seriesName">Name of the series.</param>
    /// <param name="isVisible">Whether the series should be visible.</param>
    public void SetSeriesVisibility(string seriesName, bool isVisible)
    {
        _seriesVisibility[seriesName] = isVisible;

        // Find the series and update its visibility
        var series = Series.FirstOrDefault(s => s.Name == seriesName);
        if (series is not null)
        {
            series.IsVisible = isVisible;
        }
    }

    /// <summary>
    /// Gets whether a series is visible.
    /// </summary>
    /// <param name="seriesName">Name of the series.</param>
    /// <returns>True if visible; otherwise false.</returns>
    public bool IsSeriesVisible(string seriesName)
    {
        return !_seriesVisibility.TryGetValue(seriesName, out var visible) || visible;
    }

    /// <summary>
    /// Updates a data point in a series.
    /// </summary>
    /// <param name="seriesName">Name of the series.</param>
    /// <param name="index">Index of the data point.</param>
    /// <param name="rpm">New RPM value.</param>
    /// <param name="torque">New torque value.</param>
    public void UpdateDataPoint(string seriesName, int index, double rpm, double torque)
    {
        if (_seriesDataCache.TryGetValue(seriesName, out var points) && index >= 0 && index < points.Count)
        {
            points[index].X = rpm;
            points[index].Y = torque;
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Refreshes the chart with current voltage configuration data.
    /// </summary>
    public void RefreshChart()
    {
        UpdateChart();
    }

    private void UpdateChart()
    {
        Series.Clear();
        _seriesDataCache.Clear();

        if (_currentVoltage is null || _currentVoltage.Series.Count == 0)
        {
            Title = "No Data";
            return;
        }

        Log.Debug("Updating chart with {SeriesCount} series from voltage {Voltage}V",
            _currentVoltage.Series.Count, _currentVoltage.Voltage);

        Title = $"Torque Curve - {_currentVoltage.Voltage}V";

        for (var i = 0; i < _currentVoltage.Series.Count; i++)
        {
            var curveSeries = _currentVoltage.Series[i];
            var color = SeriesColors[i % SeriesColors.Length];
            var isVisible = IsSeriesVisible(curveSeries.Name);

            // Create observable points for the series
            var points = new ObservableCollection<ObservablePoint>(
                curveSeries.Data.Select(dp => new ObservablePoint(dp.Rpm, dp.Torque))
            );
            _seriesDataCache[curveSeries.Name] = points;

            var lineSeries = new LineSeries<ObservablePoint>
            {
                Name = curveSeries.Name,
                Values = points,
                Fill = new SolidColorPaint(color.WithAlpha(40)),
                GeometrySize = 3,
                GeometryStroke = new SolidColorPaint(color) { StrokeThickness = 1 },
                GeometryFill = new SolidColorPaint(SKColors.White),
                Stroke = new SolidColorPaint(color) { StrokeThickness = 1 },
                LineSmoothness = 0.3,
                IsVisible = isVisible
            };

            Series.Add(lineSeries);
        }

        // Update axes based on data
        UpdateAxes();
    }

    private void UpdateAxes()
    {
        if (_currentVoltage is null) return;

        var maxRpm = _currentVoltage.MaxSpeed;
        var maxTorque = _currentVoltage.Series
            .SelectMany(s => s.Data)
            .Select(dp => dp.Torque)
            .DefaultIfEmpty(0)
            .Max();

        // Round up to nice values
        var xMax = RoundToNiceValue(maxRpm, true);
        var yMax = RoundToNiceValue(maxTorque * 1.1, true); // Add 10% margin

        XAxes = CreateXAxes(xMax);
        YAxes = CreateYAxes(yMax);
    }

    private static Axis[] CreateXAxes(double? maxValue = null)
    {
        return
        [
            new Axis
            {
                Name = "Speed (RPM)",
                NamePaint = new SolidColorPaint(SKColors.Gray),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)) { StrokeThickness = 1, PathEffect = new DashEffect([3, 3]) },
                MinLimit = 0,
                MaxLimit = maxValue ?? 6000,
                MinStep = 500,
                ForceStepToMin = true,
                Labeler = value => Math.Round(value).ToString("N0")
            }
        ];
    }

    private Axis[] CreateYAxes(double? maxValue = null)
    {
        return
        [
            new Axis
            {
                Name = $"Torque ({TorqueUnit})",
                NamePaint = new SolidColorPaint(SKColors.Gray),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)) { StrokeThickness = 1, PathEffect = new DashEffect([3, 3]) },
                MinLimit = 0,
                MaxLimit = maxValue ?? 100,
                MinStep = CalculateTorqueStep(maxValue ?? 100),
                ForceStepToMin = true,
                Labeler = value => value.ToString("N0")
            }
        ];
    }

    private static double CalculateTorqueStep(double maxValue)
    {
        // Calculate a nice step value based on max torque
        if (maxValue <= 10) return 1;
        if (maxValue <= 25) return 2.5;
        if (maxValue <= 50) return 5;
        if (maxValue <= 100) return 10;
        if (maxValue <= 250) return 25;
        if (maxValue <= 500) return 50;
        return 100;
    }

    private static double RoundToNiceValue(double value, bool roundUp)
    {
        if (value <= 0) return 0;

        // Find the order of magnitude
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
        var normalized = value / magnitude;

        // Round to a nice value (1, 2, 2.5, 5, 10)
        double[] niceValues = [1, 2, 2.5, 5, 10];
        double result;

        if (roundUp)
        {
            // Always use FirstOrDefault with fallback to handle edge cases
            result = (niceValues.FirstOrDefault(n => n >= normalized, 10) * magnitude);
        }
        else
        {
            // Always use LastOrDefault with fallback to handle edge cases
            result = (niceValues.LastOrDefault(n => n <= normalized, 1) * magnitude);
        }

        return result;
    }
}
