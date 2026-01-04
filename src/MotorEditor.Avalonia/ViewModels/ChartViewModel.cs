using CommunityToolkit.Mvvm.ComponentModel;
using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CurveEditor.ViewModels;

/// <summary>
/// Represents a legend item for the chart.
/// </summary>
public class LegendItem
{
    /// <summary>
    /// Display name of the legend item.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Color of the line/series.
    /// </summary>
    public required SKColor Color { get; init; }

    /// <summary>
    /// Whether the line is dashed.
    /// </summary>
    public bool IsDashed { get; init; }

    /// <summary>
    /// Whether the line is dotted.
    /// </summary>
    public bool IsDotted { get; init; }
}

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

    private Voltage? _currentVoltage;
    private readonly Dictionary<string, ObservableCollection<ObservablePoint>> _seriesDataCache = [];
    private readonly Dictionary<string, bool> _seriesVisibility = [];
    private UndoStack? _undoStack;

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

    [ObservableProperty]
    private string _powerUnit = "kW";

    [ObservableProperty]
    private ObservableCollection<LegendItem> _legendItems = [];

    [ObservableProperty]
    private bool _showMotorRatedSpeedLine = true;

    [ObservableProperty]
    private bool _showVoltageMaxSpeedLine = true;

    /// <summary>
    /// Called when TorqueUnit changes to update the chart.
    /// </summary>
    partial void OnTorqueUnitChanged(string value)
    {
        if (_currentVoltage is not null)
        {
            UpdateAxes();
            // Recalculate power curves since they depend on torque values
            if (ShowPowerCurves)
            {
                UpdateChart();
            }
        }
    }

    [ObservableProperty]
    private bool _showPowerCurves;

    [ObservableProperty]
    private double _motorMaxSpeed;

    [ObservableProperty]
    private double _motorRatedSpeed;

    [ObservableProperty]
    private bool _hasBrake;

    [ObservableProperty]
    private double _brakeTorque;

    /// <summary>
    /// Optional undo stack associated with the active document. When set,
    /// data mutations are routed through commands so they can be undone.
    /// </summary>
    public UndoStack? UndoStack
    {
        get => _undoStack;
        set => _undoStack = value;
    }

    /// <summary>
    /// Optional editing coordinator used to share selection state with other views.
    /// </summary>
    public EditingCoordinator? EditingCoordinator
    {
        get => _editingCoordinator;
        set
        {
            if (ReferenceEquals(_editingCoordinator, value))
            {
                return;
            }

            if (_editingCoordinator is not null)
            {
                _editingCoordinator.SelectionChanged -= OnCoordinatorSelectionChanged;
            }

            _editingCoordinator = value;

            if (_editingCoordinator is not null)
            {
                _editingCoordinator.SelectionChanged += OnCoordinatorSelectionChanged;
            }
        }
    }

    private EditingCoordinator? _editingCoordinator;

    /// <summary>
    /// Per-point highlighting state keyed by series name and data index.
    /// This is driven by the shared EditingCoordinator selection.
    /// </summary>
    private readonly Dictionary<string, HashSet<int>> _highlightedIndices = [];

    /// <summary>
    /// Suffix used to identify selection overlay series in the Curves
    /// collection.
    /// </summary>
    private const string SelectionOverlaySuffix = " (SelectionOverlay)";

    /// <summary>
    /// Called when MotorMaxSpeed changes to update the chart axes.
    /// </summary>
    partial void OnMotorMaxSpeedChanged(double value)
    {
        // Update chart axes when motor max speed changes
        if (_currentVoltage is not null)
        {
            UpdateAxes();
        }
    }

    /// <summary>
    /// Called when MotorRatedSpeed changes to update the chart axes.
    /// </summary>
    partial void OnMotorRatedSpeedChanged(double value)
    {
        // Update chart axes when motor rated speed changes
        if (_currentVoltage is not null)
        {
            UpdateAxes();
        }
    }

    /// <summary>
    /// Called when HasBrake changes to update the brake torque line.
    /// </summary>
    partial void OnHasBrakeChanged(bool value)
    {
        UpdateChart();
    }

    /// <summary>
    /// Called when BrakeTorque changes to update the brake torque line.
    /// </summary>
    partial void OnBrakeTorqueChanged(double value)
    {
        if (HasBrake)
        {
            UpdateChart();
        }
    }

    /// <summary>
    /// Called when ShowPowerCurves changes to update the chart.
    /// </summary>
    partial void OnShowPowerCurvesChanged(bool value)
    {
        UpdateChart();
    }

    /// <summary>
    /// Called when PowerUnit changes to update the chart axes and recalculate power curves.
    /// </summary>
    partial void OnPowerUnitChanged(string value)
    {
        if (_currentVoltage is not null)
        {
            UpdateAxes();
            // Recalculate power curves with new unit
            if (ShowPowerCurves)
            {
                UpdateChart();
            }
        }
    }

    /// <summary>
    /// Called when ShowMotorRatedSpeedLine changes to update the chart.
    /// </summary>
    partial void OnShowMotorRatedSpeedLineChanged(bool value)
    {
        UpdateChart();
    }

    /// <summary>
    /// Called when ShowVoltageMaxSpeedLine changes to update the chart.
    /// </summary>
    partial void OnShowVoltageMaxSpeedLineChanged(bool value)
    {
        UpdateChart();
    }

    /// <summary>
    /// Controls whether zoom/pan is enabled on the chart.
    /// When false, the graph is static and shows the full range of data.
    /// </summary>
    public static bool EnableZoomPan => false;

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
    public Voltage? CurrentVoltage
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
        if (_currentVoltage is null)
        {
            return;
        }

        var series = _currentVoltage.Curves.FirstOrDefault(s => s.Name == seriesName);
        if (series is null)
        {
            return;
        }

        if (index < 0 || index >= series.Data.Count)
        {
            return;
        }

        if (_undoStack is null)
        {
            // Fallback legacy behavior: update the cached points directly.
            if (_seriesDataCache.TryGetValue(seriesName, out var points) && index >= 0 && index < points.Count)
            {
                points[index].X = rpm;
                points[index].Y = torque;
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
            return;
        }

        var command = new EditPointCommand(series, index, rpm, torque);
        _undoStack.PushAndExecute(command);
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Refreshes the chart with current voltage configuration data.
    /// </summary>
    public void RefreshChart()
    {
        UpdateChart();
    }

    private void OnCoordinatorSelectionChanged(object? sender, EventArgs e)
    {
        if (_editingCoordinator is null)
        {
            return;
        }

        // Rebuild the highlighted index map from the coordinator's logical
        // point selection. This is kept separate from the underlying data
        // so we can control how the UI surfaces selection.
        _highlightedIndices.Clear();

        foreach (var point in _editingCoordinator.SelectedPoints)
        {
            var seriesName = point.Series.Name;
            if (!_highlightedIndices.TryGetValue(seriesName, out var indices))
            {
                indices = [];
                _highlightedIndices[seriesName] = indices;
            }

            if (point.Index >= 0)
            {
                indices.Add(point.Index);
            }
        }

        // Rebuild the lightweight selection overlay series so highlighted
        // points update immediately in response to table selection changes.
        UpdateSelectionOverlays();
    }

    /// <summary>
    /// Indicates whether a given series/index is currently highlighted
    /// according to the shared editing selection.
    /// </summary>
    public bool IsPointHighlighted(string seriesName, int index)
    {
        return _highlightedIndices.TryGetValue(seriesName, out var indices)
               && indices.Contains(index);
    }

    /// <summary>
    /// Handles a chart point click coming from the view. Uses modifier keys
    /// to decide whether to replace, extend, or toggle the shared selection
    /// via the EditingCoordinator.
    /// </summary>
    public void HandleChartPointClick(string seriesName, int index, Avalonia.Input.KeyModifiers modifiers)
    {
        if (_editingCoordinator is null || _currentVoltage is null)
        {
            return;
        }

        var series = _currentVoltage.Curves.FirstOrDefault(s => s.Name == seriesName);
        if (series is null || index < 0 || index >= series.Data.Count)
        {
            return;
        }

        var point = new EditingCoordinator.PointSelection(series, index);

        if (modifiers.HasFlag(Avalonia.Input.KeyModifiers.Control))
        {
            // Ctrl+click toggles the point in the selection.
            _editingCoordinator.ToggleSelection(point);
        }
        else if (modifiers.HasFlag(Avalonia.Input.KeyModifiers.Shift))
        {
            // Shift+click extends the selection by adding this point.
            _editingCoordinator.AddToSelection(new[] { point });
        }
        else
        {
            // No modifiers: replace selection with this single point.
            _editingCoordinator.SetSelection(new[] { point });
        }
    }

    private void UpdateChart()
    {
        Series.Clear();
        _seriesDataCache.Clear();

        if (_currentVoltage is null || _currentVoltage.Curves.Count == 0)
        {
            Title = "No Data";
            return;
        }

        Title = $"Torque Curve - {_currentVoltage.Value}V";

        // Calculate the maximum RPM from all relevant sources
        var maxRpmFromData = _currentVoltage.Curves
            .SelectMany(c => c.Data)
            .Select(dp => dp.Rpm)
            .DefaultIfEmpty(0)
            .Max();

        var maxRpm = new[]
        {
            MotorMaxSpeed,
            MotorRatedSpeed,
            _currentVoltage.MaxSpeed,
            _currentVoltage.RatedSpeed,
            maxRpmFromData
        }.Max();

        if (maxRpm <= 0)
        {
            maxRpm = 6000;
        }

        for (var i = 0; i < _currentVoltage.Curves.Count; i++)
        {
            var curve = _currentVoltage.Curves[i];
            var color = SeriesColors[i % SeriesColors.Length];
            var isVisible = IsSeriesVisible(curve.Name);

            // Create observable points for the series.
            // If no curve points exist, fall back to rated torque lines so the chart can still render.
            ObservableCollection<ObservablePoint> points;
            if (curve.Data.Count == 0)
            {
                var torque = 0d;
                if (curve.Name.Contains("peak", StringComparison.OrdinalIgnoreCase))
                {
                    torque = _currentVoltage.RatedPeakTorque;
                }
                else if (curve.Name.Contains("cont", StringComparison.OrdinalIgnoreCase))
                {
                    torque = _currentVoltage.RatedContinuousTorque;
                }
                else
                {
                    torque = Math.Max(_currentVoltage.RatedPeakTorque, _currentVoltage.RatedContinuousTorque);
                }

                points = new ObservableCollection<ObservablePoint>
                {
                    new(0, torque),
                    new(maxRpm, torque)
                };
            }
            else
            {
                points = new ObservableCollection<ObservablePoint>(
                    curve.Data.Select(dp => new ObservablePoint(dp.Rpm, dp.Torque))
                );
            }
            _seriesDataCache[curve.Name] = points;

            var lineSeries = new LineSeries<ObservablePoint>
            {
                Name = curve.Name,
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

            // Add power curve if enabled
            if (ShowPowerCurves)
            {
                AddPowerCurve(curve.Name, points, color, isVisible);
            }
        }

        // Rebuild selection overlays on top of the base series so that
        // any existing selection remains visible after a full chart refresh.
        UpdateSelectionOverlays();

        // Add brake torque line if motor has a brake
        if (HasBrake && BrakeTorque > 0)
        {
            AddBrakeTorqueLine();
        }

        // Add vertical lines for Motor Rated Speed and Voltage Max Speed
        AddVerticalReferenceLines(maxRpm);

        // Update axes based on data
        UpdateAxes();

        // Generate legend items
        UpdateLegend();
    }

    /// <summary>
    /// Adds a power curve series derived from a torque curve.
    /// </summary>
    /// <param name="curveName">Name of the source torque curve.</param>
    /// <param name="torquePoints">Torque data points (RPM, Torque).</param>
    /// <param name="color">Color to use for the power curve (matches torque curve).</param>
    /// <param name="isVisible">Whether the power curve should be visible.</param>
    private void AddPowerCurve(string curveName, ObservableCollection<ObservablePoint> torquePoints, SKColor color, bool isVisible)
    {
        // Calculate power from torque and speed: P = T × ω
        // Where ω = RPM × 2π / 60 (convert RPM to rad/s)
        var powerPoints = new ObservableCollection<ObservablePoint>();
        
        foreach (var point in torquePoints)
        {
            var rpm = point.X ?? 0;
            var torque = point.Y ?? 0;
            var power = CalculatePower(torque, rpm);
            powerPoints.Add(new ObservablePoint(rpm, power));
        }

        var powerSeries = new LineSeries<ObservablePoint>
        {
            Name = $"{curveName} (Power)",
            Values = powerPoints,
            Fill = null, // No fill for power curves
            GeometrySize = 0, // No points on power curves
                GeometryStroke = new SolidColorPaint(color) { StrokeThickness = 0 },
                GeometryFill = new SolidColorPaint(SKColors.Red),

            Stroke = new SolidColorPaint(color)
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect([3, 3]) // Dotted line
            },
            
            LineSmoothness = 0.3,
            IsVisible = isVisible,
            ScalesYAt = 1, // Use secondary Y-axis
            IsHoverable = false // Disable tooltips for power curves
        };

        Series.Add(powerSeries);
    }

    /// <summary>
    /// Calculates power from torque and RPM.
    /// </summary>
    /// <param name="torque">Torque in the current torque unit (TorqueUnit property).</param>
    /// <param name="rpm">Speed in RPM.</param>
    /// <returns>Power in the current power unit (PowerUnit property).</returns>
    private double CalculatePower(double torque, double rpm)
    {
        // Convert torque to Nm first if needed
        double torqueNm = torque;
        if (TorqueUnit == "lbf-in")
        {
            // 1 lbf-in = 0.112984829 Nm
            torqueNm = torque * 0.112984829;
        }
        else if (TorqueUnit == "lbf-ft")
        {
            // 1 lbf-ft = 1.355817948 Nm
            torqueNm = torque * 1.355817948;
        }
        else if (TorqueUnit == "oz-in")
        {
            // 1 oz-in = 0.00706155181 Nm
            torqueNm = torque * 0.00706155181;
        }
        // else assume already in Nm

        // P = T × ω, where ω = RPM × 2π / 60
        // P (Watts) = Torque (Nm) × RPM × 2π / 60
        var powerWatts = torqueNm * rpm * Math.PI * 2.0 / 60.0;

        // Convert to kW or HP based on current power unit
        if (PowerUnit == "hp")
        {
            // 1 HP = 745.7 W
            return powerWatts / 745.7;
        }
        else if (PowerUnit == "kW")
        {
            return powerWatts / 1000.0;
        }
        else
        {
            // Default to W
            return powerWatts;
        }
    }

    /// <summary>
    /// Rebuilds lightweight overlay series that render markers only for
    /// highlighted points. This gives per-point highlighting without
    /// disturbing the base line series.
    /// </summary>
    private void UpdateSelectionOverlays()
    {
        // Remove any existing selection overlay series.
        for (var i = Series.Count - 1; i >= 0; i--)
        {
            if (Series[i] is LineSeries<ObservablePoint> lineSeries &&
                lineSeries.Name is not null &&
                lineSeries.Name.EndsWith(SelectionOverlaySuffix, StringComparison.Ordinal))
            {
                Series.RemoveAt(i);
            }
        }

        var voltage = _currentVoltage;
        if (voltage is null || _highlightedIndices.Count == 0)
        {
            OnPropertyChanged(nameof(Series));
            return;
        }

        foreach (var series in voltage.Curves)
        {
            if (!_highlightedIndices.TryGetValue(series.Name, out var indices) || indices.Count == 0)
            {
                continue;
            }

            var color = SeriesColors[voltage.Curves.IndexOf(series) % SeriesColors.Length];

            // Build an overlay points collection containing only the
            // highlighted indices for this series.
            var overlayPoints = new ObservableCollection<ObservablePoint>();

            foreach (var index in indices.OrderBy(i => i))
            {
                if (index < 0 || index >= series.Data.Count)
                {
                    continue;
                }

                var dp = series.Data[index];
                overlayPoints.Add(new ObservablePoint(dp.Rpm, dp.Torque));
            }

            if (overlayPoints.Count == 0)
            {
                continue;
            }

            var overlaySeries = new LineSeries<ObservablePoint>
            {
                Name = series.Name + SelectionOverlaySuffix,
                Values = overlayPoints,
                // No filled area for overlays; just markers.
                Fill = null,
                Stroke = null,
                GeometrySize = 7,
                GeometryStroke = new SolidColorPaint(color) { StrokeThickness = 2 },
                GeometryFill = new SolidColorPaint(color.WithAlpha(220)),
                LineSmoothness = 0,
                IsVisible = IsSeriesVisible(series.Name)
            };

            Series.Add(overlaySeries);
        }

        OnPropertyChanged(nameof(Series));
    }

    /// <summary>
    /// Adds a horizontal line to the chart indicating the brake torque value.
    /// </summary>
    private void AddBrakeTorqueLine()
    {
        // Use the maximum of Motor Max Speed and Drive (voltage) Max Speed for line width
        var currentVoltageMaxSpeed = _currentVoltage is null ? 0 : _currentVoltage.MaxSpeed;
        var maxRpm = Math.Max(MotorMaxSpeed, currentVoltageMaxSpeed);
        if (maxRpm <= 0)
        {
            maxRpm = 6000; // Default fallback
        }

        // Create two points for a horizontal line from 0 to maxRpm at BrakeTorque
        var brakePoints = new ObservableCollection<ObservablePoint>
        {
            new(0, BrakeTorque),
            new(maxRpm, BrakeTorque)
        };

        var brakeLine = new LineSeries<ObservablePoint>
        {
            Name = "Brake Torque",
            Values = brakePoints,
            Fill = null, // No fill for the brake line
            GeometrySize = 0, // No points on the line
            Stroke = new SolidColorPaint(new SKColor(255, 165, 0)) // Orange color
            {
                StrokeThickness = 2,
                PathEffect = new DashEffect([5, 5]) // Dashed line
            },
            LineSmoothness = 0, // Straight line
            IsVisible = true
        };

        Series.Add(brakeLine);
    }

    /// <summary>
    /// Adds vertical reference lines for Motor Rated Speed and Voltage Max Speed.
    /// </summary>
    /// <param name="maxRpm">The maximum RPM value for determining line height.</param>
    private void AddVerticalReferenceLines(double maxRpm)
    {
        if (_currentVoltage is null)
        {
            return;
        }

        // Get max torque for determining line height
        var maxTorque = _currentVoltage.Curves
            .SelectMany(s => s.Data)
            .Select(dp => dp.Torque)
            .DefaultIfEmpty(0)
            .Max();

        if (maxTorque <= 0)
        {
            maxTorque = new[]
            {
                _currentVoltage.RatedPeakTorque,
                _currentVoltage.RatedContinuousTorque,
                HasBrake ? BrakeTorque : 0d
            }.Max();
        }

        var lineTorqueHeight = maxTorque * 1.1; // Extend slightly above max torque

        // Add Motor Rated Speed line if enabled
        if (ShowMotorRatedSpeedLine && MotorRatedSpeed > 0)
        {
            var motorRatedSpeedPoints = new ObservableCollection<ObservablePoint>
            {
                new(MotorRatedSpeed, 0),
                new(MotorRatedSpeed, lineTorqueHeight)
            };

            var motorRatedSpeedLine = new LineSeries<ObservablePoint>
            {
                Name = "Motor Rated Speed",
                Values = motorRatedSpeedPoints,
                Fill = null,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(100, 100, 255)) // Blue
                {
                    StrokeThickness = 2,
                    PathEffect = new DashEffect([10, 5]) // Dashed line
                },
                LineSmoothness = 0,
                IsVisible = true,
                IsHoverable = false
            };

            Series.Add(motorRatedSpeedLine);
        }

        // Add Voltage Max Speed line if enabled
        if (ShowVoltageMaxSpeedLine && _currentVoltage.MaxSpeed > 0)
        {
            var voltageMaxSpeedPoints = new ObservableCollection<ObservablePoint>
            {
                new(_currentVoltage.MaxSpeed, 0),
                new(_currentVoltage.MaxSpeed, lineTorqueHeight)
            };

            var voltageMaxSpeedLine = new LineSeries<ObservablePoint>
            {
                Name = "Voltage Max Speed",
                Values = voltageMaxSpeedPoints,
                Fill = null,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(new SKColor(255, 100, 100)) // Red
                {
                    StrokeThickness = 2,
                    PathEffect = new DashEffect([10, 5]) // Dashed line
                },
                LineSmoothness = 0,
                IsVisible = true,
                IsHoverable = false
            };

            Series.Add(voltageMaxSpeedLine);
        }
    }

    /// <summary>
    /// Updates the legend items based on the currently visible series.
    /// </summary>
    private void UpdateLegend()
    {
        LegendItems.Clear();

        if (_currentVoltage is null)
        {
            return;
        }

        // Add legend items for torque curve series
        for (var i = 0; i < _currentVoltage.Curves.Count; i++)
        {
            var curve = _currentVoltage.Curves[i];
            if (!IsSeriesVisible(curve.Name))
            {
                continue;
            }

            var color = SeriesColors[i % SeriesColors.Length];
            LegendItems.Add(new LegendItem
            {
                Name = curve.Name,
                Color = color,
                IsDashed = false,
                IsDotted = false
            });
        }

        // Add legend item for power curves if shown
        if (ShowPowerCurves)
        {
            for (var i = 0; i < _currentVoltage.Curves.Count; i++)
            {
                var curve = _currentVoltage.Curves[i];
                if (!IsSeriesVisible(curve.Name))
                {
                    continue;
                }

                var color = SeriesColors[i % SeriesColors.Length];
                LegendItems.Add(new LegendItem
                {
                    Name = $"{curve.Name} (Power)",
                    Color = color,
                    IsDashed = false,
                    IsDotted = true
                });
            }
        }

        // Add legend item for brake torque line if visible
        if (HasBrake && BrakeTorque > 0)
        {
            LegendItems.Add(new LegendItem
            {
                Name = "Brake Torque",
                Color = new SKColor(255, 165, 0), // Orange
                IsDashed = true,
                IsDotted = false
            });
        }

        // Add legend item for Motor Rated Speed if visible
        if (ShowMotorRatedSpeedLine && MotorRatedSpeed > 0)
        {
            LegendItems.Add(new LegendItem
            {
                Name = "Motor Rated Speed",
                Color = new SKColor(100, 100, 255), // Blue
                IsDashed = true,
                IsDotted = false
            });
        }

        // Add legend item for Voltage Max Speed if visible
        if (ShowVoltageMaxSpeedLine && _currentVoltage.MaxSpeed > 0)
        {
            LegendItems.Add(new LegendItem
            {
                Name = "Voltage Max Speed",
                Color = new SKColor(255, 100, 100), // Red
                IsDashed = true,
                IsDotted = false
            });
        }
    }

    private void UpdateAxes()
    {
        if (_currentVoltage is null) return;

        // Calculate the maximum RPM from all relevant sources
        var maxRpmFromData = _currentVoltage.Curves
            .SelectMany(c => c.Data)
            .Select(dp => dp.Rpm)
            .DefaultIfEmpty(0)
            .Max();

        var maxRpm = new[]
        {
            MotorMaxSpeed,
            MotorRatedSpeed,
            _currentVoltage.MaxSpeed,
            _currentVoltage.RatedSpeed,
            maxRpmFromData
        }.Max();

        if (maxRpm <= 0)
        {
            maxRpm = 6000; // Default fallback
        }

        var maxTorque = _currentVoltage.Curves
            .SelectMany(s => s.Data)
            .Select(dp => dp.Torque)
            .DefaultIfEmpty(0)
            .Max();

        if (maxTorque <= 0)
        {
            maxTorque = new[]
            {
                _currentVoltage.RatedPeakTorque,
                _currentVoltage.RatedContinuousTorque,
                HasBrake ? BrakeTorque : 0d
            }.Max();
        }

        // Use exact max RPM (no rounding), but round torque for nice Y-axis
        var yMax = RoundToNiceValue(maxTorque * 1.1, true); // Add 10% margin

        // Calculate max power if showing power curves
        double? maxPower = null;
        if (ShowPowerCurves)
        {
            maxPower = _currentVoltage.Curves
                .SelectMany(s => s.Data)
                .Select(dp => CalculatePower(dp.Torque, dp.Rpm))
                .DefaultIfEmpty(0)
                .Max();

            if (maxPower <= 0)
            {
                // Calculate from rated values
                maxPower = Math.Max(
                    CalculatePower(_currentVoltage.RatedPeakTorque, maxRpm),
                    CalculatePower(_currentVoltage.RatedContinuousTorque, maxRpm)
                );
            }

            maxPower = RoundToNiceValue(maxPower.Value * 1.1, true, isPowerValue: true); // Add 10% margin
        }

        XAxes = CreateXAxes(maxRpm);
        YAxes = CreateYAxes(yMax, maxPower);
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

    private Axis[] CreateYAxes(double? torqueMaxValue = null, double? powerMaxValue = null)
    {
        var axes = new List<Axis>
        {
            new Axis
            {
                Name = $"Torque ({TorqueUnit})",
                NamePaint = new SolidColorPaint(SKColors.Gray),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)) { StrokeThickness = 1, PathEffect = new DashEffect([3, 3]) },
                MinLimit = 0,
                MaxLimit = torqueMaxValue ?? 100,
                MinStep = CalculateTorqueStep(torqueMaxValue ?? 100),
                ForceStepToMin = true,
                Labeler = value => value.ToString("N0"),
                Position = LiveChartsCore.Measure.AxisPosition.Start
            }
        };

        // Add secondary Y-axis for power if power curves are shown
        if (ShowPowerCurves && powerMaxValue.HasValue)
        {
            // Use whole numbers for W (large values), decimals for kW/HP (small values)
            var labeler = PowerUnit == "W" 
                ? (Func<double, string>)(value => value.ToString("N0"))
                : (value => value.ToString("N1"));
            
            axes.Add(new Axis
            {
                Name = $"Power ({PowerUnit})",
                NamePaint = new SolidColorPaint(SKColors.DarkGray),
                LabelsPaint = new SolidColorPaint(SKColors.DarkGray),
                SeparatorsPaint = null, // Don't draw separators for secondary axis
                MinLimit = 0,
                MaxLimit = powerMaxValue.Value,
                MinStep = CalculatePowerStep(powerMaxValue.Value),
                ForceStepToMin = true,
                Labeler = labeler,
                Position = LiveChartsCore.Measure.AxisPosition.End,
                ShowSeparatorLines = false
            });
        }

        return [.. axes];
    }

    /// <summary>
    /// Calculates an appropriate step value for an axis based on its maximum value.
    /// Uses standardized increments to ensure readable labels that don't crowd the axis.
    /// </summary>
    /// <param name="maxValue">The maximum value on the axis.</param>
    /// <param name="isLargeUnit">True for units with large numeric values (W, oz-in), false for smaller units (kW, HP, Nm).</param>
    /// <returns>The step value to use for axis labels.</returns>
    private static double CalculateAxisStep(double maxValue, bool isLargeUnit)
    {
        if (isLargeUnit)
        {
            // For large units (W, oz-in, lbf-in), use larger steps to avoid crowding
            if (maxValue <= 5) return 0.5;
            if (maxValue <= 10) return 1;
            if (maxValue <= 20) return 2;
            if (maxValue <= 50) return 5;
            if (maxValue <= 100) return 10;
            if (maxValue <= 200) return 20;
            if (maxValue <= 250) return 25;
            if (maxValue <= 500) return 50;
            if (maxValue <= 1000) return 100;
            if (maxValue <= 1500) return 150;
            if (maxValue <= 2000) return 200;
            if (maxValue <= 5000) return 500;
            if (maxValue <= 10000) return 1000;
            return 2000;
        }
        
        // For smaller units (kW, HP, Nm, lbf-ft), use finer increments
        if (maxValue <= 1) return 0.1;
        if (maxValue <= 2.5) return 0.25;
        if (maxValue <= 5) return 0.5;
        if (maxValue <= 10) return 1;
        if (maxValue <= 20) return 2;
        if (maxValue <= 25) return 2.5;
        if (maxValue <= 50) return 5;
        if (maxValue <= 100) return 10;
        if (maxValue <= 200) return 20;
        if (maxValue <= 250) return 25;
        if (maxValue <= 500) return 50;
        if (maxValue <= 1000) return 100;
        return 200;
    }

    private double CalculatePowerStep(double maxValue)
    {
        // Calculate a nice step value based on max power and current unit
        // For W (larger values), we need larger steps to avoid crowding
        bool isLargeUnit = PowerUnit == "W";
        return CalculateAxisStep(maxValue, isLargeUnit);
    }

    private double CalculateTorqueStep(double maxValue)
    {
        // Calculate a nice step value based on max torque and current unit
        // For oz-in and lbf-in (larger numeric values), we need larger steps to avoid crowding
        bool isLargeUnit = TorqueUnit is "oz-in" or "lbf-in";
        return CalculateAxisStep(maxValue, isLargeUnit);
    }

    /// <summary>
    /// Rounds a value to a nice increment for axis display.
    /// Uses unit-specific rounding for power values to match standardized increments.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="roundUp">If true, rounds up to next nice value; otherwise rounds down.</param>
    /// <param name="isPowerValue">If true, applies power-specific rounding based on PowerUnit.</param>
    /// <returns>The rounded value.</returns>
    private double RoundToNiceValue(double value, bool roundUp, bool isPowerValue = false)
    {
        if (value <= 0) return 0;

        // For power values, use unit-specific standardized increments
        if (isPowerValue)
        {
            if (PowerUnit == "W")
            {
                // W: 10, 20, 50, 100, 250, 500, 1000, 1500, 2000, 5000, etc.
                double[] wIncrements = [10, 20, 50, 100, 250, 500, 1000, 1500, 2000, 5000, 10000, 20000];
                foreach (var increment in wIncrements)
                {
                    if (roundUp && value <= increment) return increment;
                    if (!roundUp && value < increment) return wIncrements[Array.IndexOf(wIncrements, increment) - 1];
                }
                return roundUp ? 20000 : 10000;
            }
            else if (PowerUnit == "kW" || PowerUnit == "hp")
            {
                // kW and HP: 1, 5, 10, 20, 50, 100, 200, 500, etc.
                double[] powerIncrements = [1, 5, 10, 20, 50, 100, 200, 500, 1000];
                foreach (var increment in powerIncrements)
                {
                    if (roundUp && value <= increment) return increment;
                    if (!roundUp && value < increment) return powerIncrements[Array.IndexOf(powerIncrements, increment) - 1];
                }
                return roundUp ? 1000 : 500;
            }
        }

        // Default rounding for non-power values (torque, etc.)
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
