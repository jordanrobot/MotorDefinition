---
post_title: Decimal evaluation spike
author1: copilot
post_slug: decimal-evaluation-spike
microsoft_alias: copilot
featured_image: ""
categories:
  - architecture
tags:
  - precision
  - performance
  - technical-spike
ai_note: This document was assisted by GitHub Copilot.
summary: Assessment of switching MotorDefinition and MotorEditor numeric handling from double to decimal, including precision, performance, and implementation considerations.
post_date: 2026-01-11
---

## Scope
- Evaluate pros/cons of migrating numeric types (MotorDefinition library and MotorEditor app) from `double` to `decimal`.
- Focus on torque/speed/power/curve data and user-editable numeric properties.

## Current state
- Uses `double` for torque, speed, power, inertia, curve points; downstream conversions rely on Tare library (double-based).
- Precision rounding helper mitigates binary floating artifacts but adds post-processing and thresholds.

## Option: switch to decimal

### Benefits
- Base-10 representation eliminates most binary floating artifacts for typical engineering inputs (e.g., 28.3, 50.13) without post-rounding.
- Reduces need for precision-threshold tuning and post-save normalization.
- Improves determinism for serialized JSON: fewer trailing digits after conversions.

### Costs / risks
- Tare currently uses `double`; full benefit requires Tare to expose decimal-based APIs or adapters. Otherwise conversions would still round-trip through double.
- Surface-area change: models (`ServoMotor`, `Voltage`, `Curve`, `DataPoint`), services (conversion, formatting), validation, and tests would need type updates.
- Interop: external consumers or plugins expecting `double` would need adjustment or dual-path APIs.
- Serialization: JSON decimal handling is supported, but schemas/tests need updates; stored files might mix number kinds during transition.
- UI bindings and numeric controls must ensure decimal-friendly parsing/formatting.

### Precision characteristics
- Decimal has 28–29 significant digits; eliminates binary representation issues for common engineering magnitudes (0.000001–1e6) and exact decimals (0.1, 28.3, 50.13).
- Still subject to rounding when exceeding precision (very large magnitudes or extreme fractional precision), but artifacts are far smaller and decimal-aligned.
- Conversions that rely on transcendental or non-decimal-friendly ops (e.g., sqrt, trig) would still incur rounding; Tare coverage to be confirmed.

### Performance considerations
- Decimal arithmetic is slower and allocates more than double:
  - Basic arithmetic often 4–10x slower than double in tight loops.
  - Larger struct size (16 bytes vs 8) increases memory bandwidth and cache pressure for large curve datasets.
- Expected UI impact: likely acceptable for typical curve sizes, but bulk operations (e.g., mass conversions on large data sets or frequent formatting) may show measurable slowdown; needs profiling with real workloads.

## Recommended spike steps
1. Prototype a decimal-backed branch in MotorDefinition models and UnitService:
   - Add decimal overloads or switch core fields to decimal; keep double adapters for compatibility during transition.
   - Measure serialization size and precision improvements on representative motor files.
2. Update a small slice of the UI (e.g., torque fields and one curve series) to decimal and benchmark:
   - Measure conversion time and UI responsiveness on bulk unit conversions.
3. Coordinate with Tare:
   - Add decimal-based conversion APIs or a high-precision path; otherwise conversions will still be constrained by double precision.
4. Risk mitigation:
   - Provide migration script for existing JSON to ensure consistent number formatting.
   - Maintain optional precision rounding as a fallback for any remaining double paths.

## Decision guidance
- If eliminating post-rounding and improving serialized cleanliness are higher priority than peak performance, decimal is attractive.
- If conversion performance and minimal churn are priorities, keep double and enhance targeted rounding or mixed-type adapters only where artifacts matter most.
