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

### Modified

- .copilot-tracking/plans/20260109-chart-underlay-plan.md - Marked panel infrastructure task complete.
- src/MotorEditor.Avalonia/Models/PanelRegistry.cs - Added Chart Format panel descriptor for the left zone and PanelBar.
- src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs - Exposed Chart Format panel expanded state derived from the left panel selection.
- src/MotorEditor.Avalonia/Views/MainWindow.axaml - Added Chart Format panel shell and wiring in the left zone.

### Removed

- None.

## Release Summary

**Total Files Affected**: 7

### Files Created (3)

- .copilot-tracking/plans/20260109-chart-underlay-plan.md - Plan document for Chart Format panel and underlay work.
- .copilot-tracking/details/20260109-chart-underlay-details.md - Detailed task breakdown for the new feature.
- .copilot-tracking/changes/20260109-chart-underlay-changes.md - Change log for this workstream.

### Files Modified (4)

- .copilot-tracking/plans/20260109-chart-underlay-plan.md - Updated status for T1 panel infrastructure.
- src/MotorEditor.Avalonia/Models/PanelRegistry.cs - Registered the Chart Format panel in the left zone registry.
- src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs - Added Chart Format expansion state tied to ActiveLeftPanelId.
- src/MotorEditor.Avalonia/Views/MainWindow.axaml - Added Chart Format panel container to the left zone layout.

### Files Removed (0)

- None.

### Dependencies & Infrastructure

- **New Dependencies**: None
- **Updated Dependencies**: None
- **Infrastructure Changes**: None
- **Configuration Updates**: None

### Deployment Notes

- None.
