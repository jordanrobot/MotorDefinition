<!-- markdownlint-disable-file -->
# Release Changes: Directory Browser Rename UX

**Related Plan**: .copilot-tracking/plans/20260104-directory-browser-rename-plan.md
**Implementation Date**: 2026-01-04

## Summary

Tracking implementation of Directory Browser rename UX improvements and associated tests.

## Changes

### Added

- .copilot-tracking/plans/20260104-directory-browser-rename-plan.md - Plan capturing rename UX scope and tasks.
- .copilot-tracking/details/20260104-directory-browser-rename-details.md - Task-level requirements for rename initiation and completion behavior.
- tests/CurveEditor.Tests/ViewModels/DirectoryBrowserRenameTests.cs - ViewModel coverage for rename start, cancel, and successful completion.

### Modified

- src/MotorEditor.Avalonia/ViewModels/DirectoryBrowserViewModel.cs - Trimmed rename inputs and ensured rename state clears with originals preserved on cancel.
- src/MotorEditor.Avalonia/Views/DirectoryBrowserPanel.axaml.cs - Guarded rename completion to avoid duplicate commits when no rename is active.
- .copilot-tracking/plans/20260104-directory-browser-rename-plan.md - Marked progress through T1-T3 with T4 still pending.
- .copilot-tracking/changes/20260104-directory-browser-rename-changes.md - Recorded rename UX implementation progress.

### Removed

- None.

## Release Summary

**Total Files Affected**: 6

### Files Created (3)

- .copilot-tracking/plans/20260104-directory-browser-rename-plan.md - Rename UX plan.
- .copilot-tracking/details/20260104-directory-browser-rename-details.md - Rename task requirements.
- tests/CurveEditor.Tests/ViewModels/DirectoryBrowserRenameTests.cs - Unit tests for rename lifecycle.

### Files Modified (3)

- src/MotorEditor.Avalonia/ViewModels/DirectoryBrowserViewModel.cs - Adjusted rename completion handling and state reset.
- src/MotorEditor.Avalonia/Views/DirectoryBrowserPanel.axaml.cs - Prevented duplicate rename commits when not in rename mode.
- .copilot-tracking/changes/20260104-directory-browser-rename-changes.md - Logged rename UX implementation progress.

### Files Removed (0)

- None.

### Dependencies & Infrastructure

- **New Dependencies**: None
- **Updated Dependencies**: None
- **Infrastructure Changes**: None
- **Configuration Updates**: None

### Deployment Notes

- None.
