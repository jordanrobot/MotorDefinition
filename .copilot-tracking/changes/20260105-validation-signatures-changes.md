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

### Modified

- src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs - Captures validation signature lock state for motors, drives, and curves during load and tab changes.
- src/MotorEditor.Avalonia/ViewModels/CurveDataTableViewModel.cs - Tracks signature-locked series for downstream read-only handling.

### Removed

- None.

## Release Summary

**Total Files Affected**: 4

### Files Created (2)

- .copilot-tracking/plans/20260105-validation-signatures-plan.md - Plan for validation signature locking.
- .copilot-tracking/details/20260105-validation-signatures-details.md - Detailed steps for tasks in the plan.

### Files Modified (2)

- src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs - Computes validation signature lock state and exposes editability flags.
- src/MotorEditor.Avalonia/ViewModels/CurveDataTableViewModel.cs - Adds signature lock tracking for curve series.

### Files Removed (0)

- None.

### Dependencies & Infrastructure

- **New Dependencies**: None
- **Updated Dependencies**: None
- **Infrastructure Changes**: None
- **Configuration Updates**: None

### Deployment Notes

- None.
