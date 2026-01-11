<!-- markdownlint-disable-file -->
# Release Changes: Chart Format Panel & Image Underlay

**Related Plan**: .copilot-tracking/plans/20260109-chart-underlay-plan.md
**Implementation Date**: 2026-01-09

## Summary

Tracking work to add a Chart Format panel with image underlay controls and persistence.

## Changes

### Added

- .copilot-tracking/plans/20260109-chart-underlay-plan.md - Plan for Chart Format panel and image underlay.
- .copilot-tracking/details/20260109-chart-underlay-details.md - Task-level requirements for the Chart Format panel and underlay feature set.
- .copilot-tracking/changes/20260109-chart-underlay-changes.md - Tracking log for Chart Format and image underlay implementation.
- src/MotorEditor.Avalonia/Models/UnderlayMetadata.cs - Value model for per-drive/voltage underlay persistence.
- src/MotorEditor.Avalonia/Services/UnderlayMetadataService.cs - JSON persistence service for underlay metadata in .motorEditor.
- tests/CurveEditor.Tests/ViewModels/UnderlayPersistenceTests.cs - Coverage for underlay metadata save/load and view model application (including lock).

### Modified

 - .copilot-tracking/plans/20260109-chart-underlay-plan.md - Marked panel infrastructure, controls, underlay interaction, persistence, and T5 coordinate synchronization complete.
 - .copilot-tracking/details/20260109-chart-underlay-details.md - Documented T5 requirements for anchor/scale origin synchronization with chart origin.
 - src/MotorEditor.Avalonia/Models/PanelRegistry.cs - Added Chart Format panel descriptor for the left zone and PanelBar.
 - src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs - Exposed Chart Format panel expanded state derived from the left panel selection.
  - src/MotorEditor.Avalonia/ViewModels/ChartViewModel.cs - Added underlay state, commands, and persistence hooks; fixed HasUnderlayImage property to use generated backing; lock toggle no longer resets offsets; anchor-aware scaling keeps chart origin aligned when changing scale or offsets; added persisted opacity with live binding; clear operation now detaches the bitmap before disposal and defers disposal on UI thread to prevent crashes.
  - src/MotorEditor.Avalonia/Views/ChartView.axaml - Inserted underlay canvas and LiveCharts chart layout; temporarily overlaid underlay canvas above chart for validation; bound underlay opacity.
  - src/MotorEditor.Avalonia/Views/ChartView.axaml.cs - Added underlay rendering, drag-to-pan, and layout logic; refreshes anchor when chart layout changes; re-layouts on chart layout changes.
  - src/MotorEditor.Avalonia/Views/ChartView.axaml.cs - Queued underlay layout updates on the dispatcher to pick up chart draw margin changes after layout/visibility adjustments while keeping drag updates immediate; subscribed to LiveCharts UpdateFinished to refresh whenever the chart recalculates (e.g., series visibility, panel toggles).
  - src/MotorEditor.Avalonia/Views/MainWindow.axaml - Added Chart Format panel UI, underlay controls, numeric scale inputs, and corrected Load Underlay command binding.
  - src/MotorEditor.Avalonia/Models/UnderlayMetadata.cs - Added persisted opacity for underlay images.
  - tests/CurveEditor.Tests/ViewModels/UnderlayPersistenceTests.cs - Added coverage for scaling around positive and negative underlay anchors to keep chart origin synchronized and for persisted opacity; added regression coverage to ensure clearing underlays detaches the image and clears metadata without crashing.
  - tests/CurveEditor.Tests/ViewModels/UnderlayPersistenceTests.cs - Added coverage for scaling around positive and negative underlay anchors to keep chart origin synchronized.
  - .gitignore - Ignored generated docs/api output to avoid committing regenerated documentation artifacts during testing.

### Removed

- docs/api/* - Reverted accidentally added generated API reference files.

## Release Summary

**Total Files Affected**: 13

### Files Created (6)

- .copilot-tracking/plans/20260109-chart-underlay-plan.md - Plan document for Chart Format panel and underlay work.
- .copilot-tracking/details/20260109-chart-underlay-details.md - Detailed task breakdown for the new feature.
- .copilot-tracking/changes/20260109-chart-underlay-changes.md - Change log for this workstream.
- src/MotorEditor.Avalonia/Models/UnderlayMetadata.cs - Underlay persistence model.
- src/MotorEditor.Avalonia/Services/UnderlayMetadataService.cs - Underlay metadata persistence service.
- tests/CurveEditor.Tests/ViewModels/UnderlayPersistenceTests.cs - Tests for underlay metadata persistence and application.

### Files Modified (7)

- .copilot-tracking/plans/20260109-chart-underlay-plan.md - Updated status for T1 panel infrastructure.
- src/MotorEditor.Avalonia/Models/PanelRegistry.cs - Registered the Chart Format panel in the left zone registry.
- src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs - Added Chart Format expansion state tied to ActiveLeftPanelId and underlay command handlers.
- src/MotorEditor.Avalonia/Views/MainWindow.axaml - Added Chart Format panel container and underlay control bindings.
- src/MotorEditor.Avalonia/ViewModels/ChartViewModel.cs - Added underlay state, commands, and persistence hooks; fixed HasUnderlayImage property to use generated backing.
- src/MotorEditor.Avalonia/Views/ChartView.axaml - Inserted underlay canvas and LiveCharts chart layout; temporarily overlaid underlay canvas above chart for validation.
- src/MotorEditor.Avalonia/Views/ChartView.axaml.cs - Added underlay rendering, drag-to-pan, and layout logic.

### Files Removed (0)

- None.

### Dependencies & Infrastructure

- **New Dependencies**: None
- **Updated Dependencies**: None
- **Infrastructure Changes**: None
- **Configuration Updates**: None

### Deployment Notes

- None.
