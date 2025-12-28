# Power Curve Overlay Feature

## Overview
The Power Curve Overlay feature allows users to visualize motor power curves alongside torque curves on the same chart. Power is automatically calculated from torque and speed data and displayed on a secondary Y-axis.

## Features

### 1. Power Calculation
- **Formula**: P = T × ω (Power = Torque × Angular Velocity)
  - Where ω = RPM × 2π / 60 (converts RPM to rad/s)
- **Units**: Supports both kW (kilowatts) and HP (horsepower)
  - kW: Power (Watts) / 1000
  - HP: Power (Watts) / 745.7

### 2. Dual Y-Axis Chart
- **Primary Y-Axis (Left)**: Torque in configured unit (Nm, lbf-in, etc.)
- **Secondary Y-Axis (Right)**: Power in configured unit (kW or HP)
- Both axes have independent scaling with appropriate step values
- Power axis only appears when power curves are enabled

### 3. Visual Design
- **Line Style**: Dotted lines (5px dash, 5px gap)
- **Colors**: Match the corresponding torque curve colors
- **Geometry**: No data points displayed (GeometrySize = 0)
- **Smoothness**: 0.3 (matches torque curves)
- **Naming**: Power curves are labeled as "{CurveName} (Power)"

### 4. Toggle Functionality
- **Menu Location**: View > Show Power Curves
- **Shortcut**: None (can be added if desired)
- **Persistence**: User preference is saved and restored between sessions
- **Setting Key**: "ShowPowerCurves" (boolean)

### 5. Tooltips
- Automatic tooltips display when hovering over power curves
- Shows: RPM value and calculated power value
- Format: Series name, X value (RPM), Y value (Power in current unit)

## Usage

### Enabling Power Curves
1. Open a motor definition file
2. Go to **View** menu
3. Click **Show Power Curves** (checkmark indicates enabled)
4. Power curves will appear as dotted lines on the chart

### Disabling Power Curves
1. Go to **View** menu
2. Click **Show Power Curves** again (checkmark removed)
3. Power curves and secondary Y-axis will be hidden

### Changing Power Units
The power unit (kW or HP) can be changed programmatically through the `ChartViewModel.PowerUnit` property. Currently defaults to "kW".

## Technical Implementation

### Modified Files
1. **ChartViewModel.cs**
   - Added `ShowPowerCurves` property
   - Added `PowerUnit` property (default: "kW")
   - Added `CalculatePower()` method
   - Added `AddPowerCurve()` method
   - Modified `UpdateAxes()` to include secondary Y-axis
   - Modified `CreateYAxes()` to support dual axes
   - Added `CalculatePowerStep()` method for axis scaling

2. **MainWindowViewModel.cs**
   - Added `ToggleShowPowerCurves` command
   - Added preference loading/saving logic

3. **MainWindow.axaml**
   - Added menu item under View tab

### Code Structure

```csharp
// Power calculation formula
private double CalculatePower(double torque, double rpm)
{
    // P = T × ω, where ω = RPM × 2π / 60
    var powerWatts = torque * rpm * Math.PI * 2.0 / 60.0;
    
    // Convert to selected unit
    if (PowerUnit == "HP")
        return powerWatts / 745.7;
    else
        return powerWatts / 1000.0;
}
```

### Series Configuration

```csharp
var powerSeries = new LineSeries<ObservablePoint>
{
    Name = $"{curveName} (Power)",
    Values = powerPoints,
    Fill = null,
    GeometrySize = 0,
    Stroke = new SolidColorPaint(color)
    {
        StrokeThickness = 1,
        PathEffect = new DashEffect([5, 5])
    },
    LineSmoothness = 0.3,
    IsVisible = isVisible,
    ScalesYAt = 1 // Use secondary Y-axis
};
```

## Testing

### Unit Tests
The following test cases verify power curve functionality:
- `ShowPowerCurves_DefaultsToFalse()` - Verifies default state
- `ShowPowerCurves_WhenEnabled_AddsPowerSeriesToChart()` - Verifies power series creation
- `ShowPowerCurves_WhenDisabled_RemovesPowerSeriesFromChart()` - Verifies cleanup
- `PowerUnit_DefaultsToKw()` - Verifies default unit
- `ShowPowerCurves_AddsSecondaryYAxis()` - Verifies dual Y-axis
- `ShowPowerCurves_WhenDisabled_HasOnlyOneYAxis()` - Verifies single Y-axis when disabled

All tests pass successfully (297/297 tests pass as of implementation).

## Future Enhancements

Potential improvements for future versions:
1. Add keyboard shortcut for toggling power curves
2. Add UI control for selecting power unit (kW/HP)
3. Add power curve export functionality
4. Add custom tooltip templates with more detailed information
5. Add option to toggle individual power curves per series
6. Add power curve data to data table view
7. Support additional power units (W, mW, BTU/hr, etc.)

## Dependencies

- LiveChartsCore.SkiaSharpView.Avalonia v2.0.0-rc6.1
- SkiaSharp (via LiveCharts)
- CommunityToolkit.Mvvm (for commands and properties)

## See Also

- [ChartViewModel.cs](../src/MotorEditor.Avalonia/ViewModels/ChartViewModel.cs)
- [MainWindowViewModel.cs](../src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs)
- [ChartViewModelTests.cs](../tests/CurveEditor.Tests/ViewModels/ChartViewModelTests.cs)
