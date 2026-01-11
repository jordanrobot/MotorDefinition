<!-- markdownlint-disable-file -->
# Precision Rounding Plan

## Scope

- Detect and correct floating-point precision artifacts in unit conversions at the data model layer.
- Introduce a user preference that configures the precision error threshold.
- Add test coverage to verify corrected conversions and preference-controlled behavior.

## Tasks

- [ ] T1: Precision rounding utility
  - Add a data-layer helper that normalizes values within a configurable threshold of a rounded representation.
- [ ] T2: User preference integration
  - Add a precision error threshold preference with a sensible default and surface it in the preferences UI.
- [ ] T3: Conversion hook & tests
  - Apply precision correction in unit conversion flows and add targeted tests validating corrections and threshold handling.

## References

- Details: `.copilot-tracking/details/20260111-precision-rounding-details.md`
- Code areas: `src/MotorDefinition/Services/UnitService.cs`, `src/MotorEditor.Avalonia/Models/UserPreferences.cs`, `src/MotorEditor.Avalonia/Services/*`, `src/MotorEditor.Avalonia/ViewModels/PreferencesViewModel.cs`, `src/MotorEditor.Avalonia/Views/PreferencesWindow.axaml`, `tests/CurveEditor.Tests/**`
