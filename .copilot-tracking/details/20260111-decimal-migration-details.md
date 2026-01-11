<!-- markdownlint-disable-file -->
# Decimal Migration Details

## T1: Core domain/DTO decimal types
- Replace `double` numeric fields with `decimal` in domain models (DataPoint, Curve, Drive, Voltage, ServoMotor, metadata torque/power/rpm/inertia/weight fields).
- Update constructors and property backing fields accordingly.
- Adjust DTOs and mapping (MotorDefinitionFileDto, VoltageFileDto, SeriesEntryDto, MotorFileMapper) to use decimal and ensure JSON converters work with decimal.
- Confirm `CompactNumberArrayJsonConverters` handle decimal arrays.

## T2: Services and conversions
- Update `UnitService` and `UnitConversionService` methods to operate on decimal input/output; maintain precision and avoid intermediate double conversions.
- Update `CurveGeneratorService`, `DriveVoltageSeriesService`, and any numeric helpers to decimal.
- Only keep double shims if needed for external clients in MotorDefinition; avoid in MotorEditor.

## T3: UI/ViewModels & preferences
- Update viewmodels and views to use decimal properties, bindings, and formatting for torque/rpm/power/inertia/weight, curve data, and underlay metadata.
- Ensure parsers/formatters use decimal-aware culture handling.
- Remove the floating point error precision-threshold preference and any usage.

## T4: Persistence & tests
- Ensure file load/save paths read/write decimal without loss; schema bump to 1.0.1 only if required.
- Update tests covering unit conversion, curve generation, persistence, underlay metadata, and viewmodels to assert decimal values.

## T5: Tracking
- After completing each task, mark it `[x]` in the plan and append a summary entry to the changes file under Added/Modified/Removed.
