<!-- markdownlint-disable-file -->
# Precision Rounding Correction Plan

## Scope

- Detect and correct floating-point precision artifacts near likely round values in the data model conversion pipeline.
- Provide a user preference to configure the precision error threshold (with an option to disable correction).
- Validate conversions and preference handling with targeted tests.

## Tasks

- [ ] T1: Analyze the unit conversion/data model flow to select where precision correction should occur and how the user preference will be applied.
- [ ] T2: Implement precision correction utility and integrate it into conversion services honoring the configurable threshold.
- [ ] T3: Add or update tests covering correction behavior, preference control, and round-trip conversions.
- [ ] T4: Update tracking changes file after each task with the files modified or added.

## References

- Details: `.copilot-tracking/details/20260106-precision-rounding-details.md`
- Code areas: `src/MotorDefinition/Services/UnitService.cs`, `src/MotorEditor.Avalonia/Models/UserPreferences.cs`, `src/MotorEditor.Avalonia/Services/UnitConversionService.cs`, `tests/CurveEditor.Tests/Services`
