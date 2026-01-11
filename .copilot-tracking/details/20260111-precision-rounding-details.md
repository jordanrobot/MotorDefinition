<!-- markdownlint-disable-file -->
# Precision Rounding Details

## T1: Precision rounding utility
- Add a reusable helper in the data model layer to correct floating-point artifacts (e.g., 50.1300000000034 → 50.13).
- Accept a configurable threshold (default: 1e-10). Differences larger than the threshold should not be altered.
- Evaluate rounding against reasonable decimal places (0–15) and return the first rounded value that falls within the threshold.
- Provide a no-op path when the threshold is zero or negative.

## T2: User preference integration
- Add a `PrecisionErrorThreshold` setting to `UserPreferences` with default `1e-10`.
- Surface the setting in the Preferences UI with validation and persistence through `UserPreferencesService`.
- Update the preferences view model and save pipeline to honor the new setting.

## T3: Conversion hook & tests
- Apply precision correction inside unit conversion flows so all conversions benefit without UI involvement.
- Honor the user-configured threshold (including disabling when set to zero).
- Add focused tests covering:
  - Correction of typical precision artifacts during conversions.
  - Behavior when threshold is zero (no correction).
  - Persistence and retrieval of the preference.
