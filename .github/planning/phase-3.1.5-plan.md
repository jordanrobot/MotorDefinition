## Curve Editor Phase 3.1.5 Plan

### Status

Draft

**Related ADRs**

- ADR-0006 Motor File Schema and Versioning Strategy (`docs/adr/adr-0006-motor-file-schema-and-versioning.md`)
- ADR-0009 Logging and Error Handling Policy (`docs/adr/adr-0009-logging-and-error-handling-policy.md`)

### Goal

- Reduce motor definition JSON file size and improve hand-edit ergonomics by switching curve data persistence to a shared-axis “series table/map” representation.
- Keep `schemaVersion` at `1.0.0`.
- Add new motor brake-related scalar properties and additional unit labels, with safe defaults when missing.

### Scope

- In scope:
  - [ ] Update the JSON Schema (`schema/motor-schema-v1.0.0.json`) and example file (`schema/example-motor.json`) to the new series table/map representation.
  - [ ] Update persistence code so CurveEditor **writes** the series table/map representation.
  - [ ] Update load logic so CurveEditor can **read** the series table/map representation.
  - [ ] Add new motor scalar properties:
    - [ ] `brakeResponseTime`
    - [ ] `brakeEngageTimeDiode`
    - [ ] `brakeEngageTimeMOV`
    - [ ] `brakeBacklash`
  - [ ] Add / adjust unit labels in the units section (at minimum: `responseTime`, `backlash`, `percentage`, `inertia`, `temperature`, `torqueConstant`) with defaults.
  - [ ] Update drive JSON shape:
    - [ ] Rename drive `name` -> `seriesName`.
    - [ ] Ensure property ordering in emitted JSON: `manufacturer`, `partNumber`, `seriesName`, then remaining properties.
  - [ ] Add validation rules for shared axes and series alignment.
  - [ ] Add a simple benchmark/test artifact to demonstrate file size reduction.

- Out of scope:
  - [ ] UI changes beyond what is strictly required to load/save the new format.
  - [ ] Changes to curve interpolation / generation logic.
  - [ ] Undo/redo support for any file-format-related changes.
  - [ ] Compression (zip/gzip) as the primary solution.

### Assumptions and Constraints

- The runtime model remains `CurveSeries` + `DataPoint` (as recommended in the requirements), and conversion happens at load/save boundaries.
- `schemaVersion` remains `1.0.0` even though the on-disk shape changes; treat this as acceptable because the repo currently treats `1.0.0` as the canonical version (`MotorDefinition.CurrentSchemaVersion`).
- System.Text.Json is the persistence mechanism.
- Logging must follow ADR-0009: log parse/validation failures with context, and recover with safe defaults where possible.
- Backward compatibility is explicitly out of scope for Phase 3.1.5 (project not released publicly yet). The app does not need to load the legacy per-point `series[]/data[]` JSON representation once the new format is implemented.

Series keys
- Treat `series` map keys as case-sensitive.
- Do not normalize casing of series names during load/save.

Units
- Use unit strings as specified in the requirements; ensure backlash default is `arcmin`.

### Current Baseline (What exists today)

- Persistence:
  - `FileService` loads/saves by serializing/deserializing the runtime model directly with System.Text.Json (`src/CurveEditor/Services/FileService.cs`).
  - The persisted curve format is object-per-point: `voltages[].series[]` with `data[]` entries containing `{ percent, rpm, torque }`.
- Runtime model:
  - `MotorDefinition`, `DriveConfiguration`, `VoltageConfiguration`, `CurveSeries`, `DataPoint`, `UnitSettings` (`src/CurveEditor/Models/*`).
- Validation:
  - `ValidationService` validates series count, 101 points per series, percent order, and per-series ascending RPM (`src/CurveEditor/Services/ValidationService.cs`).
  - The Directory Browser uses a lightweight JSON parse + `HasValidConfiguration()` (`src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs`).
- Schema:
  - `schema/motor-schema-v1.0.0.json` still describes the object-per-point representation and a minimal `units` object.

### Proposed Design

#### Extraction-Friendly Namespace/Layout (for Phase 3.1.6)

- In Phase 3.1.5, place persistence-facing code under the future library namespace even though it lives inside the app project.
- Target folder + namespaces (inside `src/CurveEditor/`):
  - `MotorDefinitions/`
    - `Dtos/` (`jordanrobot.MotorDefinitions.Dtos`)
    - `Mapping/` (`jordanrobot.MotorDefinitions.Mapping`)
    - `Validation/` (`jordanrobot.MotorDefinitions.Validation`)
    - `Probing/` (`jordanrobot.MotorDefinitions.Probing`) (lightweight shape probe used by Directory Browser)
- Design intent:
  - Phase 3.1.6 becomes a mechanical move of `src/CurveEditor/MotorDefinitions/*` into a new `jordanrobot.MotorDefinitions` class library project with minimal namespace churn.
  - No UI/Avalonia dependencies in any `jordanrobot.MotorDefinitions.*` code.

#### 1) Data / State Model

- Keep the existing runtime domain types as-is for editing:
  - `MotorDefinition`, `DriveConfiguration`, `VoltageConfiguration`, `CurveSeries`, `DataPoint`.

- Introduce **persistence DTOs** (internal to CurveEditor) that match the new on-disk format:
  - `MotorDefinitionFileDto`
  - `DriveFileDto` (with `seriesName`)
  - `VoltageFileDto` containing:
    - `percent`: int[101]
    - `rpm`: double[101]
    - `series`: map/dictionary keyed by series name
  - `SeriesEntryDto` containing:
    - `locked`: bool
    - `notes`: string? (optional)
    - `torque`: double[101]

- Add a mapper/converter layer:
  - `MotorFileMapper` (or similar) to convert between DTOs and runtime models.
  - Lossless mapping rules:
    - `percent[i]` and `rpm[i]` become `DataPoint.Percent` and `DataPoint.Rpm`.
    - Each `SeriesEntryDto.torque[i]` becomes `DataPoint.Torque` for the corresponding series.

#### 2) Persistence (Load/Save)

- Update `FileService` to:
  - Deserialize `MotorDefinitionFileDto` and map to runtime models.
  - Serialize by mapping runtime models to `MotorDefinitionFileDto` and writing the new series table/map format.

- Drive rename (`name` -> `seriesName`):
  - Prefer DTO-based persistence so the runtime property can remain `DriveConfiguration.Name`, while the JSON uses `seriesName`.
  - Emit JSON in stable order using `JsonPropertyOrder` on DTOs.

  - Backward compatibility:
    - Explicitly not required for Phase 3.1.5.

- Directory Browser motor-file validation:
  - The Directory Browser "is this a motor file" check must remain lightweight and must not deserialize the runtime `MotorDefinition`.
  - Short-term approach: a `JsonDocument` shape probe for `schemaVersion`, `motorName`, and `drives[].voltages[]` with `percent`/`rpm` arrays and `series` object.

#### 3) Validation

- Mapper responsibilities (shape validation):
  - Validate required nodes exist and array lengths are correct during DTO -> runtime mapping:
    - `percent` length 101
    - `rpm` length 101
    - each series `torque` length 101
  - Fail mapping with a clear exception suitable for logging with `{FilePath}`.

- ValidationService responsibilities (domain/semantic validation):
  - Enforce axis semantics during load:
    - `percent` strictly increasing, starts at 0, ends at 100
    - `rpm` non-negative and monotonic non-decreasing
  - Keep validation domain-focused; structural schema/shape checks remain in the mapper.

#### 4) Error Handling and Logging

- On load:
  - Invalid JSON: `FileService.LoadAsync` already logs at Error and throws a user-friendly exception; keep this behavior.
  - Schema/semantic violations (wrong array lengths, invalid axes):
    - Log at Warning or Error with `{FilePath}` and enough context to debug.
    - Fail the load (recommended) rather than silently producing malformed runtime state.

- On save:
  - If the runtime model cannot be represented in the new format (axis mismatch), return validation errors rather than writing a corrupt file.

### Implementation Steps (Incremental)

#### PR 0: DTOs + mapping scaffolding (no behavior change)

- [ ] Add internal DTO types for the new persisted shape.
- [ ] Add a mapping layer between DTOs and runtime models.
- [ ] Add focused unit tests for mapping round-trips (DTO -> runtime -> DTO).

**Done when**

- Solution builds.
- New mapping tests pass.
- File I/O behavior is unchanged.

#### PR 1: Switch Load + Save to the series table/map format (merge-safe)

- [ ] Update `FileService` to use DTOs for BOTH save and load.
- [ ] Implement drive JSON rename end-to-end (`name` -> `seriesName`) and property ordering.
- [ ] Add new motor properties and unit labels with defaults.
- [ ] Update Directory Browser lightweight validation to recognize the new shape (via `JsonDocument` shape probe; ideally shared helper under `jordanrobot.MotorDefinitions.Probing`).
- [ ] Add round-trip tests so a saved file can be reopened.

**Done when**

- Saving produces the new persisted shape AND a saved file can be reopened.
- Updated save/load/round-trip unit tests pass.

#### PR 2: Semantic validation on load + mapper shape validation

- [ ] Mapper validates required nodes + array lengths during mapping.
- [ ] `ValidationService` enforces axis semantics during load.
- [ ] `FileService.LoadAsync` logs failures with `{FilePath}` and returns a user-friendly exception message.

**Done when**

- Load rejects invalid files with clear errors and log entries.
- Tests cover at least one failing case per semantic rule.

#### PR 3: Schema, samples, docs, and size benchmark

- [ ] Update `schema/motor-schema-v1.0.0.json` to:
  - [ ] define the series table/map structure under `voltages[]`
  - [ ] add new motor properties and units
  - [ ] enforce array length constraints (101) where practical in JSON Schema
- [ ] Update `schema/example-motor.json` to the new format.
- [ ] Add a size benchmark/test artifact demonstrating reduction.

**Done when**

- Schema and sample validate against each other.
- AC 3.1.5c evidence exists in-repo.

#### PR 4: Hardening + AC-driven validation pass

- [ ] Ensure round-trip stability (save -> load -> save) and no loss of series metadata.
- [ ] Confirm Directory Browser background validation remains lightweight.
- [ ] Audit for accidental behavior changes outside persistence/validation.

### Acceptance Criteria

- [ ] AC 3.1.5a: CurveEditor saves motor definition files with `schemaVersion` set to `1.0.0` and persists curve data using the series table/map representation.
- [ ] AC 3.1.5b: CurveEditor can load and save the series table/map format, and reopening a saved file yields identical curve data (percent/rpm/torque) and series metadata.
- [ ] AC 3.1.5c: For a representative file with multiple voltages and at least two series per voltage, the saved JSON file is measurably smaller than the prior object-per-point representation (tracked via a benchmark/test artifact).
- [ ] AC 3.1.5d: The new motor properties are correctly loaded and saved, and their default values are applied when loading files that omit them.

### Testing Strategy

- Unit tests (existing pattern in `tests/CurveEditor.Tests`):
  - [ ] Mapper tests (lossless conversion, axis validation)
  - [ ] `FileService` tests updated for the new persisted shape
  - [ ] Validation tests for new semantic rules

- Manual validation script:
  - [ ] Create/open a motor file, save it, confirm the JSON uses the new series table format.
  - [ ] Reopen the saved file and confirm the chart/grid content matches pre-save.
  - [ ] Introduce an invalid axis length in JSON and confirm load fails with a clear error and log entry.

### Risks / Edge Cases / Mitigations

- **Dictionary ordering / stable diffs** for `series` map
  - Mitigation: use a deterministic key ordering when serializing (e.g., `SortedDictionary<string, SeriesEntryDto>`).

- **Schema vs runtime drift** (schema currently under-specifies units and will need expansion)
  - Mitigation: treat schema and DTO as the source of truth for persisted shape; keep runtime model separate and mapped.


- **Legacy file support is out of scope**
  - Mitigation: none in Phase 3.1.5; old object-per-point files are not required to load.

### Follow-on Work and TODOs

- [ ] Consider updating/adding an ADR documenting the new series table/map persisted representation (if we want a formal design record).
- [ ] Revisit schema/index.json correctness and tool expectations if schema-based tooling is introduced.
- [ ] Phase 3.1.6+ extraction: the DTO + mapper layer is a natural seam for moving persistence into a UI-independent library.
