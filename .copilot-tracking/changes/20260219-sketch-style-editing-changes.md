<!-- markdownlint-disable-file -->
# Release Changes: Phase 4.0 Sketch-Style Curve Editing

**Related Plan**: .github/planning/phase-4-plan.md (Section 0)
**Implementation Date**: 2026-02-19

## Summary

Implements sketch-style curve editing on the chart, allowing users to click and drag to draw torque values directly onto a curve series. The mouse position is snapped to the nearest speed data point (X axis) and rounded to a configurable torque increment (Y axis, default 0.2).

## Changes

### Added

- src/MotorEditor.Avalonia/ViewModels/ChartViewModel.cs - Added `SketchEditSeriesName`, `IsSketchEditActive`, `TorqueSnapIncrement` properties and `SetSketchEditSeries`, `ClearSketchEditSeries`, `ApplySketchPoint`, `FindNearestSpeedIndex`, `SnapTorque` methods for sketch-edit mode
- src/MotorEditor.Avalonia/Views/ChartView.axaml.cs - Added `_isSketchDragging` state, `TryApplySketchAtPixel` helper, and sketch-edit handling in pointer event handlers
- tests/CurveEditor.Tests/ViewModels/ChartViewModelTests.cs - Added 26 unit tests covering sketch-edit activation/deactivation, torque snapping, nearest speed index, and ApplySketchPoint behavior

### Modified

- src/MotorEditor.Avalonia/Models/UserPreferences.cs - Added `TorqueSnapIncrement` property (default 0.2) with clone support
- src/MotorEditor.Avalonia/Views/ChartView.axaml - Added sketch-edit mode indicator overlay showing active series name
- src/MotorEditor.Avalonia/Views/ChartView.axaml.cs - Extended pointer pressed/moved/released/capture-lost handlers to support sketch-edit drag interactions
- src/MotorEditor.Avalonia/Views/CurveDataPanel.axaml - Added per-series ✏ sketch-edit toggle button in the column header
- src/MotorEditor.Avalonia/Views/CurveDataPanel.axaml.cs - Added `OnSketchEditToggleClick` and `RefreshSketchEditToggles` handlers for sketch-edit toggle

### Removed

(none)

## Release Summary

**Total Files Affected**: 6

### Files Created (0)

(none)

### Files Modified (6)

- src/MotorEditor.Avalonia/Models/UserPreferences.cs - Added TorqueSnapIncrement preference
- src/MotorEditor.Avalonia/ViewModels/ChartViewModel.cs - Added sketch-edit state management and point application logic
- src/MotorEditor.Avalonia/Views/ChartView.axaml - Added sketch-edit mode indicator overlay
- src/MotorEditor.Avalonia/Views/ChartView.axaml.cs - Added sketch-edit mouse interaction handling
- src/MotorEditor.Avalonia/Views/CurveDataPanel.axaml - Added per-series sketch-edit toggle button in column header
- src/MotorEditor.Avalonia/Views/CurveDataPanel.axaml.cs - Added sketch-edit toggle click handler and sibling refresh logic
- tests/CurveEditor.Tests/ViewModels/ChartViewModelTests.cs - Added 26 unit tests for sketch-edit functionality

### Files Removed (0)

(none)

### Dependencies & Infrastructure

- **New Dependencies**: None
- **Updated Dependencies**: None
- **Infrastructure Changes**: None
- **Configuration Updates**: None

### Deployment Notes

No special deployment considerations. The sketch-edit feature is purely additive and does not change existing behavior.
