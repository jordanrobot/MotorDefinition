<!-- markdownlint-disable-file -->
# Precision Rounding Correction Details

## T1: Analyze insertion points and preferences
- Review UnitService conversion paths (including hp/current handling) and UnitConversionService usage to determine where to apply precision correction without affecting UI-only formatting.
- Identify how user preferences are loaded/saved so a threshold setting can flow into conversions, with a sensible default and a disable option.

## T2: Implement correction and integrate preferences
- Add a data-model-level precision correction utility that rounds values only when the difference from a rounded candidate is within the user-defined threshold (default small epsilon, 0 disables).
- Integrate correction into the conversion pipeline so all conversions (including manual hp/current paths) honor the threshold, avoiding changes when values are already stable.
- Add a precision error threshold property to UserPreferences, ensure persistence, and surface it to conversion services.

## T3: Tests
- Add targeted unit tests for precision correction logic and conversion behavior near round numbers (e.g., 50.1300000000034 -> 50.13) plus values that must stay unchanged when outside the threshold.
- Add integration-style tests proving the preference controls rounding (including disabling correction) and that round-trip conversions eliminate typical floating-point artifacts.
