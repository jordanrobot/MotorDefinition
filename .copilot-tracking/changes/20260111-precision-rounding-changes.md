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
- src/MotorEditor.Avalonia/Models/UserPreferences.cs - Added precision error threshold preference with default value and cloning support.
- src/MotorEditor.Avalonia/ViewModels/PreferencesViewModel.cs - Surfaced precision threshold in the preferences workflow and enforced non-negative saving.
- src/MotorEditor.Avalonia/Views/PreferencesWindow.axaml - Added UI controls for configuring the precision error threshold.
- tests/CurveEditor.Tests/Services/UserPreferencesServiceTests.cs - Covered default, persistence, and clone behavior for the new threshold preference.
- src/MotorDefinition/Services/UnitService.cs - Applied precision correction after conversions with a configurable threshold.
- src/MotorEditor.Avalonia/Services/UnitConversionService.cs - Exposed precision threshold configuration to the UI layer.
- src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs - Propagated user-configured precision thresholds to conversion services and reacted to preference changes.
- tests/CurveEditor.Tests/Services/UnitServiceTests.cs - Validated precision correction and disablement in unit conversions.
- tests/CurveEditor.Tests/Services/UnitConversionServiceTests.cs - Ensured conversion service applies and respects precision correction thresholds.

### Removed

- None.

## Release Summary

**Total Files Affected**: 16

### Files Created (5)

- .copilot-tracking/plans/20260111-precision-rounding-plan.md - Precision rounding implementation plan.
- .copilot-tracking/details/20260111-precision-rounding-details.md - Supporting task details.
- .copilot-tracking/changes/20260111-precision-rounding-changes.md - Ongoing change tracking for this work.
- src/MotorDefinition/Services/PrecisionRounding.cs - Helper for correcting floating-point precision artifacts with a configurable threshold.
- tests/CurveEditor.Tests/Services/PrecisionRoundingTests.cs - Unit tests validating precision correction behaviors.

### Files Modified (11)

- src/MotorEditor.Avalonia/Models/UserPreferences.cs - Added precision error threshold preference with default value and cloning support.
- src/MotorEditor.Avalonia/ViewModels/PreferencesViewModel.cs - Surfaced precision threshold in the preferences workflow and enforced non-negative saving.
- src/MotorEditor.Avalonia/Views/PreferencesWindow.axaml - Added UI controls for configuring the precision error threshold.
- tests/CurveEditor.Tests/Services/UserPreferencesServiceTests.cs - Covered default, persistence, and clone behavior for the new threshold preference.
- src/MotorDefinition/Services/UnitService.cs - Applied precision correction after conversions with a configurable threshold.
- src/MotorEditor.Avalonia/Services/UnitConversionService.cs - Exposed precision threshold configuration to the UI layer.
- src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs - Propagated user-configured precision thresholds to conversion services and reacted to preference changes.
- tests/CurveEditor.Tests/Services/UnitServiceTests.cs - Validated precision correction and disablement in unit conversions.
- tests/CurveEditor.Tests/Services/UnitConversionServiceTests.cs - Ensured conversion service applies and respects precision correction thresholds.
- .copilot-tracking/plans/20260111-precision-rounding-plan.md - Updated task completion status.
- .copilot-tracking/changes/20260111-precision-rounding-changes.md - Documented ongoing changes and release summary.

### Files Removed (0)

- None.

### Dependencies & Infrastructure

- **New Dependencies**: None
- **Updated Dependencies**: None
- **Infrastructure Changes**: None
- **Configuration Updates**: None

### Deployment Notes

- None.
