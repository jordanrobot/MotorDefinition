<!-- markdownlint-disable-file -->
# Release Changes: Precision Rounding

**Related Plan**: .copilot-tracking/plans/20260111-precision-rounding-plan.md
**Implementation Date**: 2026-01-11

## Summary

Track precision rounding improvements for unit conversions, including a configurable user threshold and supporting tests.

## Changes

### Added

- .copilot-tracking/plans/20260111-precision-rounding-plan.md - Plan outlining precision rounding tasks.
- .copilot-tracking/details/20260111-precision-rounding-details.md - Task-level requirements for rounding utility, preference, and tests.
- .copilot-tracking/changes/20260111-precision-rounding-changes.md - Change log for precision rounding work.
- src/MotorDefinition/Services/PrecisionRounding.cs - Helper to normalize floating-point precision artifacts with a configurable threshold.
- tests/CurveEditor.Tests/Services/PrecisionRoundingTests.cs - Unit tests covering precision rounding helper behavior and threshold handling.

### Modified

- .copilot-tracking/plans/20260111-precision-rounding-plan.md - Marked precision rounding utility task as complete.

### Removed

- None.

## Release Summary

**Total Files Affected**: 3

### Files Created (3)

- .copilot-tracking/plans/20260111-precision-rounding-plan.md - Precision rounding implementation plan.
- .copilot-tracking/details/20260111-precision-rounding-details.md - Supporting task details.
- .copilot-tracking/changes/20260111-precision-rounding-changes.md - Ongoing change tracking for this work.

### Files Modified (0)

- None.

### Files Removed (0)

- None.

### Dependencies & Infrastructure

- **New Dependencies**: None
- **Updated Dependencies**: None
- **Infrastructure Changes**: None
- **Configuration Updates**: None

### Deployment Notes

- None.
