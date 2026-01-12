<!-- markdownlint-disable-file -->
# Decimal Migration Plan

## Scope

- Migrate numeric types from double to decimal across MotorDefinition and MotorEditor to eliminate binary floating artifacts.
- Remove floating-point precision threshold preference now obsolete with decimal usage.
- Keep serialization/backward compatibility intact while favoring decimal-first APIs.

## Tasks

- [ ] T1: Core domain/DTO decimal types  
  - Convert core models (DataPoint, Curve, ServoMotor, Drive, Voltage, metadata as needed) and DTOs to decimal for numeric fields.
  - Update JSON converters/mappers to align with decimal storage.
- [ ] T2: Services and conversions  
  - Update UnitService/UnitConversionService and related helpers to operate on decimal; add shims only where external compatibility requires.  
  - Adjust CurveGeneratorService and other numeric helpers to decimal inputs/outputs.
- [ ] T3: UI/ViewModels & preferences  
  - Update viewmodels, views, formatting/validation to handle decimal values and remove precision-threshold preference/usage.  
  - Ensure Avalonia bindings/parsers use decimal-friendly paths.
- [ ] T4: Persistence & tests  
  - Update load/save flows, underlay metadata, and any persistence logic to use decimal while maintaining schema compatibility; bump schema only if necessary.  
  - Update focused tests to reflect decimal expectations.
- [ ] T5: Tracking  
  - Update this plan and the changes file after each completed task, marking checkboxes.

## References

- Issue: Change Numeric Types to Decimal
- Spike: `.github/planning/S001 - decimal-type-migration.md`
