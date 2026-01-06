<!-- markdownlint-disable-file -->
# Release Changes: Data Panel Navigation UX

**Related Plan**: .copilot-tracking/plans/20260106-data-panel-nav-plan.md
**Implementation Date**: 2026-01-06

## Summary

Track fixes for Data Panel navigation/selection so arrow keys and highlights stay consistent across all columns and data loads reliably.

## Changes

### Added

- .copilot-tracking/plans/20260106-data-panel-nav-plan.md - Plan for stabilizing Data Panel navigation and visuals.
- .copilot-tracking/details/20260106-data-panel-nav-details.md - Task-level requirements for diagnosing and fixing selection loss and adding tests.

### Modified

- .copilot-tracking/plans/20260106-data-panel-nav-plan.md - Marked all Data Panel navigation tasks complete.
- .copilot-tracking/changes/20260106-data-panel-nav-changes.md - Recorded Data Panel navigation progress and file updates.
- src/MotorEditor.Avalonia/ViewModels/CurveDataTableViewModel.cs - Suppressed coordinator-driven clears so selection persists across non-series columns.
- tests/CurveEditor.Tests/ViewModels/CurveDataTableViewModelTests.cs - Added coverage for RPM navigation with coordinator and coordinator-driven selection sync.

### Removed

- None.

## Release Summary

**Total Files Affected**: 6

### Files Created (2)

- .copilot-tracking/plans/20260106-data-panel-nav-plan.md - Navigation/selection fix plan.
- .copilot-tracking/details/20260106-data-panel-nav-details.md - Supporting task details.

### Files Modified (4)

- .copilot-tracking/plans/20260106-data-panel-nav-plan.md - Marked navigation tasks complete.
- .copilot-tracking/changes/20260106-data-panel-nav-changes.md - Logged navigation fixes and testing coverage.
- src/MotorEditor.Avalonia/ViewModels/CurveDataTableViewModel.cs - Prevented coordinator sync from clearing table selection when navigating non-series columns.
- tests/CurveEditor.Tests/ViewModels/CurveDataTableViewModelTests.cs - Added navigation and coordinator sync tests.

### Files Removed (0)

- None.

### Dependencies & Infrastructure

- **New Dependencies**: None
- **Updated Dependencies**: None
- **Infrastructure Changes**: None
- **Configuration Updates**: None

### Deployment Notes

- None.
