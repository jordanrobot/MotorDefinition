---
post_title: Migrate numeric types to decimal across MotorDefinition and MotorEditor
author1: copilot
post_slug: migrate-to-decimal-numerics
microsoft_alias: copilot
featured_image: ""
categories:
  - architecture
tags:
  - precision
  - performance
  - migration
ai_note: This document was assisted by GitHub Copilot.
summary: Issue template for a future effort to replace double with decimal in MotorDefinition and MotorEditor, including requirements, decisions, and testing guidance.
post_date: 2026-01-11
---

## Goal
Replace `double` with `decimal` for numeric fields and calculations in MotorDefinition (nuget) and MotorEditor to eliminate binary floating artifacts and improve serialized precision.

## Context (from spike)
- Decimal avoids most binary FP artifacts for typical engineering values (28.3, 50.13, 0.1) and yields cleaner JSON.
- Costs: slower arithmetic (often 4–10x), larger struct size (16B), more cache pressure; UI impact likely acceptable but needs measurement.
- Tare library currently uses `double`; a decimal path or adapter will be required to get full benefit.

## Architectural decisions to make
- **Interop strategy:** Provide decimal-first APIs and optional double adapters, or switch wholly to decimal and break double callers.
- **Tare integration:** Update Tare to expose decimal conversions or wrap its output with decimal-rounded results.
- **Mixed precision policy:** Decide if any hot paths remain `double` (e.g., perf-critical) with localized decimal normalization.
- **Serialization format:** Confirm JSON continues to emit standard numbers; consider schema/version note if changing persisted expectations.
- **UI binding:** Ensure Avalonia numeric controls parse/format decimal cleanly; align formatting settings with decimal precision.

## Functional requirements (to-do)
- [ ] Convert core model fields (torque, speed, power, inertia, curve data points, motor properties) to `decimal`.
- [ ] Update UnitService/UnitConversionService to operate on decimal inputs/outputs; provide compatibility shims if needed.
- [ ] Adjust user preferences, formatting, and validation to use decimal-aware parsing/formatting.
- [ ] Ensure curve series data remains artifact-free after conversions and saves without post-hoc rounding thresholds.
- [ ] Keep preference-driven precision/threshold features functional or retire them if no longer needed (decision required).
- [ ] Update file load/save to handle decimal types and preserve backward compatibility where feasible.

## Testing requirements
- **Data correctness:** Round-trip motor files (load → convert units → save) and diff numeric fields to ensure no artifact growth; cover curves and motor properties.
- **Conversion fidelity:** Verify common conversions (Nm↔lbf-in, rpm↔rad/s, W↔hp) match expected decimal results within tolerance.
- **Performance sanity:** Benchmark bulk conversions and large curve processing before/after to detect regressions; acceptable thresholds to be defined.
- **UI behavior:** Validate preferences dialog and numeric editors accept/format decimal values; ensure dirty-tracking and undo/redo remain consistent.
- **Compatibility:** If double shims remain, test both decimal and double entry points to ensure consistent outcomes.

## Clarifications / decisions requested
- Should we maintain dual APIs (decimal primary, double compatibility) or fully break from double?
- Acceptable performance envelope: what slowdown is tolerable for conversions and bulk curve operations?
- Backward compatibility: is schema/versioning needed for existing motor JSON files, or is in-place decimal acceptable?
- Should the existing precision-threshold preference be kept as a guardrail even after decimal migration?

## Notes
- Related spike: `.github/planning/decimal-evaluation-spike.md`.
