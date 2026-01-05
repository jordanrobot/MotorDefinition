<!-- markdownlint-disable-file -->
# Release Changes: Validation Signature Editing Locks

**Related Plan**: .copilot-tracking/plans/20260105-validation-signatures-plan.md
**Implementation Date**: 2026-01-05

## Summary

Tracking implementation of validation signature checks and edit locking in MotorEditor.

## Changes

### Added

- .copilot-tracking/plans/20260105-validation-signatures-plan.md - Plan covering validation signature lock scope and tasks.
- .copilot-tracking/details/20260105-validation-signatures-details.md - Task-level requirements for signature verification and edit locking.
- samples/motor-with-signatures.json - Sample motor file with valid motor/drive/curve signatures for UI validation.
- samples/motor-without-signatures.json - Sample motor file without signatures for editing scenarios.

### Modified

- src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs - Captures validation signature lock state for motors, drives, and curves during load and tab changes.
- src/MotorEditor.Avalonia/ViewModels/CurveDataTableViewModel.cs - Tracks signature-locked series for downstream read-only handling.
- src/MotorEditor.Avalonia/Views/MotorPropertiesPanel.axaml - Disables motor/drive/voltage editors when corresponding signatures are valid.
- src/MotorEditor.Avalonia/Views/CurveDataPanel.axaml.cs - Blocks rename/lock/delete and column lock toggles for signature-locked curve series.

### Removed

- None.

## Release Summary

**Total Files Affected**: 8

### Files Created (4)

- .copilot-tracking/plans/20260105-validation-signatures-plan.md - Plan for validation signature locking.
- .copilot-tracking/details/20260105-validation-signatures-details.md - Detailed steps for tasks in the plan.
- samples/motor-with-signatures.json - Signed sample motor file.
- samples/motor-without-signatures.json - Unsigned sample motor file.

### Files Modified (4)

- src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs - Computes validation signature lock state, exposes editability flags, and blocks edits when signed.
- src/MotorEditor.Avalonia/ViewModels/CurveDataTableViewModel.cs - Adds signature lock tracking for curve series.
- src/MotorEditor.Avalonia/Views/MotorPropertiesPanel.axaml - Disables motor/drive/voltage editors based on signature lock state.
- src/MotorEditor.Avalonia/Views/CurveDataPanel.axaml.cs - Prevents edits and lock toggles for signature-locked curve series.

### Files Removed (0)

- None.

### Dependencies & Infrastructure

- **New Dependencies**: None
- **Updated Dependencies**: None
- **Infrastructure Changes**: None
- **Configuration Updates**: None

### Deployment Notes

- None.
