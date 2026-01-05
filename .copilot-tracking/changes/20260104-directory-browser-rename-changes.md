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

- src/MotorEditor.Avalonia/ViewModels/DirectoryBrowserViewModel.cs - Avoids full refresh on rename, retaining expanded state while updating paths and clearing rename state.
- src/MotorEditor.Avalonia/Views/DirectoryBrowserPanel.axaml.cs - Focuses the rename textbox reliably, re-focuses when shown, and blocks tree shortcuts (Enter/Delete) during rename.
- .copilot-tracking/plans/20260104-directory-browser-rename-plan.md - Marked all rename tasks complete.
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
