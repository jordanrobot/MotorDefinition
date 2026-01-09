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

### Modified

- .copilot-tracking/plans/20260109-chart-underlay-plan.md - Marked panel infrastructure, controls, and underlay interaction tasks complete.
- src/MotorEditor.Avalonia/Models/PanelRegistry.cs - Added Chart Format panel descriptor for the left zone and PanelBar.
- src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs - Exposed Chart Format panel expanded state derived from the left panel selection.
- src/MotorEditor.Avalonia/ViewModels/ChartViewModel.cs - Added underlay state, commands, and persistence hooks; fixed HasUnderlayImage property to use generated backing; lock toggle no longer resets offsets.
- src/MotorEditor.Avalonia/Views/ChartView.axaml - Inserted underlay canvas and LiveCharts chart layout; temporarily overlaid underlay canvas above chart for validation.
- src/MotorEditor.Avalonia/Views/ChartView.axaml.cs - Added underlay rendering, drag-to-pan, and layout logic.
- src/MotorEditor.Avalonia/Views/MainWindow.axaml - Added Chart Format panel UI, underlay controls, numeric scale inputs, and corrected Load Underlay command binding.

### Removed

- None.

## Release Summary

**Total Files Affected**: 12

### Files Created (5)

- .copilot-tracking/plans/20260109-chart-underlay-plan.md - Plan document for Chart Format panel and underlay work.
- .copilot-tracking/details/20260109-chart-underlay-details.md - Detailed task breakdown for the new feature.
- .copilot-tracking/changes/20260109-chart-underlay-changes.md - Change log for this workstream.
- src/MotorEditor.Avalonia/Models/UnderlayMetadata.cs - Underlay persistence model.
- src/MotorEditor.Avalonia/Services/UnderlayMetadataService.cs - Underlay metadata persistence service.

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
