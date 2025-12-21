## Phase 3.1.5 Subtasks: JSON Schema + Data Model Update (Series Table/Map) (Agent Execution Checklist)

### Purpose
- Provide a PR-sliceable task list for implementing Phase 3.1.5 with minimal rework.
- Make it easy for agents to validate each acceptance criterion incrementally.

### Execution Rules (Mandatory)
- Treat this file as the single source of truth for work tracking.
- Do not start a PR section until the prior PR section is complete.
- When a task is completed, mark it as `[x]` immediately.
- A PR section is not complete until:
  - All tasks are checked `[x]`, AND
  - The "Done when" criteria are satisfied.
- Do not add “nice-to-haves” that are not listed in this file, the Phase 3.1.5 requirements, or the Phase 3.1.5 plan.

### Inputs
- Requirements: [.github/planning/phase-3-requirements.md](.github/planning/phase-3-requirements.md) (Phase 3.1.5 section)
- Plan: [.github/planning/phase-3.1.5-plan.md](.github/planning/phase-3.1.5-plan.md)
- ADRs:
  - [docs/adr/adr-0006-motor-file-schema-and-versioning.md](docs/adr/adr-0006-motor-file-schema-and-versioning.md)
  - [docs/adr/adr-0009-logging-and-error-handling-policy.md](docs/adr/adr-0009-logging-and-error-handling-policy.md)

### Scope Reminder (Phase 3.1.5)
- Replace per-point persisted curve data with a shared-axis series table/map representation:
  - voltage-level `percent[101]` and `rpm[101]`
  - per-series `torque[101]` under a `series` map keyed by series name
- Keep `schemaVersion` at `1.0.0`.
- Add new brake-related motor scalar properties and new unit labels with defaults.
- Update drive JSON shape: `name` -> `seriesName` and ensure emitted property ordering.
- Add validation rules for axes + alignment and ensure load failures are logged and handled per policy.
- Demonstrate measurable file size reduction via a simple benchmark/test artifact.

Out of scope reminders:
- Do not change curve interpolation/generation logic.
- Do not add UI redesign work.
- Do not add compression.

### Key Files (Expected touch points)
- Schema + examples:
  - [schema/motor-schema-v1.0.0.json](schema/motor-schema-v1.0.0.json)
  - [schema/example-motor.json](schema/example-motor.json)
  - [schema/index.json](schema/index.json) (only if needed by schema tooling; not required by Phase 3.1.5)
- Persistence:
  - [src/CurveEditor/Services/FileService.cs](src/CurveEditor/Services/FileService.cs)
- Runtime models (remain runtime-facing):
  - [src/CurveEditor/Models/MotorDefinition.cs](src/CurveEditor/Models/MotorDefinition.cs)
  - [src/CurveEditor/Models/DriveConfiguration.cs](src/CurveEditor/Models/DriveConfiguration.cs)
  - [src/CurveEditor/Models/VoltageConfiguration.cs](src/CurveEditor/Models/VoltageConfiguration.cs)
  - [src/CurveEditor/Models/CurveSeries.cs](src/CurveEditor/Models/CurveSeries.cs)
  - [src/CurveEditor/Models/DataPoint.cs](src/CurveEditor/Models/DataPoint.cs)
  - [src/CurveEditor/Models/UnitSettings.cs](src/CurveEditor/Models/UnitSettings.cs)
- Validation:
  - [src/CurveEditor/Services/ValidationService.cs](src/CurveEditor/Services/ValidationService.cs)
  - [src/CurveEditor/Services/IValidationService.cs](src/CurveEditor/Services/IValidationService.cs)
- Directory Browser lightweight file validation (must remain lightweight):
  - [src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs](src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs)
- Tests:
  - [tests/CurveEditor.Tests/Services/FileServiceTests.cs](tests/CurveEditor.Tests/Services/FileServiceTests.cs)
  - [tests/CurveEditor.Tests/Services/ValidationServiceTests.cs](tests/CurveEditor.Tests/Services/ValidationServiceTests.cs)

### Acceptance Criteria (Phase 3.1.5)
- AC 3.1.5a: CurveEditor saves motor definition files with `schemaVersion` set to `1.0.0` and persists curve data using the series table/map representation.
- AC 3.1.5b: CurveEditor can load and save the series table/map format, and reopening a saved file yields identical curve data (percent/rpm/torque) and series metadata.
- AC 3.1.5c: For a representative file with multiple voltages and at least two series per voltage, the saved JSON file is measurably smaller than the current object-per-point representation (track size reduction in a simple benchmark/test artifact).
- AC 3.1.5d: The new motor properties are correctly loaded and saved, and their default values are applied when loading files that omit them.

### Assumptions and Constraints
- Runtime model stays `CurveSeries` + `DataPoint`. The series table/map is a persistence-only representation.
- The new on-disk representation is the only representation written by the app.
- Directory Browser "is this a motor file" check must not call `IFileService.LoadAsync()` (it mutates global file state).
- Backward compatibility is explicitly out of scope for Phase 3.1.5 (project not released publicly yet). Do not implement loading of the legacy per-point `series[]/data[]` representation.

Series keys
- Treat `series` map keys as **case-sensitive**.
- Do not normalize casing of series names during load/save.

Future extraction (Phase 3.1.6+)
- Organize persistence DTOs + mapper so they can be moved into the planned client library with minimal churn.
- Avoid introducing UI/Avalonia dependencies into persistence code.

Extraction-friendly layout (recommended)
- Place persistence-facing code under `src/CurveEditor/MotorDefinitions/*` with namespaces matching the future library:
  - `jordanrobot.MotorDefinitions.Dtos`
  - `jordanrobot.MotorDefinitions.Mapping`
  - `jordanrobot.MotorDefinitions.Validation`
  - `jordanrobot.MotorDefinitions.Probing`
- In Phase 3.1.6, move `src/CurveEditor/MotorDefinitions/*` into a new class library project with minimal namespace churn.

### State Model Summary (Target)
- Persisted shape under each voltage configuration:
  - `percent`: array length 101, strictly increasing, 0..100
  - `rpm`: array length 101, non-negative, monotonic non-decreasing
  - `series`: object/map keyed by series name
    - entry has: `locked` (bool), `notes` (optional string), `torque` (array length 101)
- Runtime shape (unchanged):
  - `VoltageConfiguration.Series[]` of `CurveSeries`, each with `DataPoint{ Percent, Rpm, Torque }[101]`.
- Drive JSON rename:
  - On disk: drive uses `manufacturer`, `partNumber`, `seriesName` (third property), then `voltages`.
  - In runtime: keep `DriveConfiguration.Name` as the internal name.

### Agent Notes (Migration Guidance)
- Prefer a DTO + mapper boundary around `FileService` so UI/runtime code doesn’t need to know about the new persisted shape.
- Use `JsonPropertyOrder` (DTOs) to satisfy “manufacturer/partNumber/seriesName ordering” without relying on serializer implementation quirks.
- Ensure new fields have defaults so missing values deserialize safely.
- Update `ValidationService` to validate both:
  - the existing runtime invariants (101 points, percent ordering), and
  - the new shared-axis representability constraint.

---

## [x] PR 0: Preparation (DTOs + mapper scaffolding, no behavior change)

### Goal
Introduce persistence DTOs and a mapper layer without changing the app’s load/save behavior.

### Tasks
- [x] Add internal persistence DTOs representing the target on-disk shape (do not wire them into `FileService` yet):
  - [x] `MotorDefinitionFileDto`
  - [x] `DriveFileDto` (`seriesName`)
  - [x] `VoltageFileDto` (`percent`, `rpm`, `series` map)
  - [x] `SeriesEntryDto` (`locked`, `notes?`, `torque`)
- [x] Add a mapping layer (pure functions) between DTOs and runtime types:
  - [x] DTO -> runtime (lossless)
  - [x] runtime -> DTO (lossless if representable)
- [x] Add a lightweight shape probe helper for Directory Browser validation:
  - [x] `MotorFileProbe` (or similar) under `jordanrobot.MotorDefinitions.Probing` that inspects a `JsonDocument`.
- [x] Decide and document deterministic ordering for `series` map emission:
  - [x] Use `SortedDictionary<string, SeriesEntryDto>` or explicitly sort keys during serialization mapping.
- [x] Add unit tests for mapper round-trips using small in-memory objects:
  - [x] One voltage, two series
  - [x] Multiple voltages

Required hygiene:
- [x] No user-visible behavior change.

### Done when
- Solution builds.
- New mapper unit tests pass.
- No existing tests are modified yet.

### Files (expected)
- Add: `src/CurveEditor/MotorDefinitions/*` (internal folder for DTOs + mapper + probe)
- Add: tests under `tests/CurveEditor.Tests/Services/` (or a dedicated `MotorFiles` test folder)

### Quick manual test
1. Run existing test suite.
2. Launch the app and perform a basic open/save (should be unchanged).

---

## [x] PR 1: Switch Load + Save to the series table/map format (end-to-end persistence)

### Goal
Change persistence so saving and loading both use the new format, so the PR is merge-safe and the app can round-trip files immediately (AC 3.1.5a + AC 3.1.5b partial).

### Tasks
- [x] Update [src/CurveEditor/Services/FileService.cs](src/CurveEditor/Services/FileService.cs) to use DTOs for BOTH save and load:
  - [x] Save path: map runtime -> DTO and serialize DTO.
  - [x] Load path: deserialize DTO and map DTO -> runtime.
  - [x] Keep `schemaVersion` emitted as `1.0.0`.
- [x] Implement drive JSON rename end-to-end:
  - [x] On disk uses `seriesName` instead of `name`.
  - [x] On read, map `seriesName` -> runtime `DriveConfiguration.Name`.
  - [x] Ensure property ordering on write: `manufacturer`, `partNumber`, `seriesName`, then `voltages` (DTO `JsonPropertyOrder`).
- [x] Add new motor scalar properties end-to-end:
  - [x] `brakeResponseTime`
  - [x] `brakeEngageTimeDiode`
  - [x] `brakeEngageTimeMOV`
  - [x] `brakeBacklash`
- [x] Expand `UnitSettings` to include new unit labels (runtime model):
  - [x] Add `responseTime` (default `milliseconds`)
  - [x] Add `percentage` (default `%`)
  - [x] Add `temperature` (default `C`)
  - [x] Ensure `backlash` default is `arcmin` (not `arc-minutes`).
  - [x] Ensure `inertia` default is `kg-m^2`.
  - [x] Ensure `torqueConstant` default is `Nm/A`.
- [x] Update Directory Browser lightweight validator to recognize the new format without deserializing runtime models:
  - [x] Replace `JsonSerializer.Deserialize<MotorDefinition>` approach with a `JsonDocument` shape probe.
  - [x] Prefer calling the shared `jordanrobot.MotorDefinitions.Probing` helper so `DirectoryBrowserViewModel` does not embed schema details.
  - [x] Check for `schemaVersion == "1.0.0"`, `motorName`, and plausible `drives[].voltages[]` structure.
  - [x] Check that voltage entries contain `percent`/`rpm` arrays and a `series` object.
  - [x] Keep it lightweight (no full schema validation in Phase 3.1.5; migrate to library validator in Phase 3.1.6+).
- [x] Case-sensitive series key behavior:
  - [x] Ensure the mapper uses case-sensitive dictionary keys for `series` (no casing normalization).
  - [x] Update runtime series lookup helpers to be case-sensitive where applicable (e.g., `VoltageConfiguration.GetSeriesByName`).

Tests (must make this PR merge-safe)
- [x] Update/add unit tests to validate the new end-to-end behavior:
  - [x] Save writes `percent`/`rpm` and `series` map.
  - [x] Load reads the new shape and reconstructs the runtime hierarchy.
  - [x] Round-trip save -> load -> save preserves curve data and series metadata.
  - [x] Drive JSON uses `seriesName` and does not use `name`.

Required hygiene:
- [x] Keep `schemaVersion` emitted as `1.0.0`.
- [x] Do not change curve generation logic.

### Done when
- Saving produces the new persisted shape AND a saved file can be reopened.
- Updated save/load/round-trip unit tests pass.
- Directory Browser still shows valid motor definition files (new shape).

### Files (expected)
- Update: [src/CurveEditor/Services/FileService.cs](src/CurveEditor/Services/FileService.cs)
- Update: [src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs](src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs)
- Update: [src/CurveEditor/Models/UnitSettings.cs](src/CurveEditor/Models/UnitSettings.cs)
- Update (if needed): [src/CurveEditor/Models/VoltageConfiguration.cs](src/CurveEditor/Models/VoltageConfiguration.cs)
- Update/add: [tests/CurveEditor.Tests/Services/FileServiceTests.cs](tests/CurveEditor.Tests/Services/FileServiceTests.cs)

### Quick manual test
1. Launch app.
2. Create a new motor.
3. Save As.
4. Open the saved JSON and verify `percent`/`rpm` arrays and `series` map exist.

---

## [x] PR 2: Semantic validation on load + mapper shape validation (logging + safe failure)

### Goal
Enforce Phase 3.1.5 semantic rules during load (and fail fast with clear logging), keeping the mapper responsible for structural/shape validation.

### Tasks
- [x] Mapper shape validation (DTO -> runtime):
  - [x] Validate array lengths and required nodes during mapping:
    - [x] `percent` length 101
    - [x] `rpm` length 101
    - [x] each series `torque` length 101
  - [x] Validate required scalar properties are present or defaultable.
  - [x] Fail mapping with a clear exception that includes enough context to log.
- [x] Semantic validation on load:
  - [x] Validate `percent` axis semantics: strictly increasing, starts at 0, ends at 100.
  - [x] Validate `rpm` axis semantics: non-negative and monotonic non-decreasing.
  - [x] Validate torque values are non-negative (if required by existing domain rules).
- [x] Logging + safe failure behavior (per ADR-0009):
  - [x] In `LoadAsync`, catch mapping/semantic validation failures and log with `{FilePath}`.
  - [x] Surface a user-friendly exception message to the caller (no crash loop).
- [x] Unit tests:
  - [x] Invalid percent axis length fails to load.
  - [x] Percent not starting at 0 or not ending at 100 fails to load.
  - [x] RPM decreasing somewhere fails to load.

### Done when
- Load rejects semantically invalid files with clear errors and log entries.
- Tests cover at least one failing case per semantic rule.

### Files (expected)
- Update: [src/CurveEditor/Services/FileService.cs](src/CurveEditor/Services/FileService.cs)
- Update: [src/CurveEditor/Services/ValidationService.cs](src/CurveEditor/Services/ValidationService.cs)
- Update/add: [tests/CurveEditor.Tests/Services/ValidationServiceTests.cs](tests/CurveEditor.Tests/Services/ValidationServiceTests.cs)

### Quick manual test
1. Save a motor file (new format).
2. Close and relaunch.
3. Open the saved file from the directory browser.
4. Confirm chart/grid populate.

---

## [x] PR 3: Schema + example updates + size benchmark artifact

### Goal
Update schema and sample files to match the new representation, and add proof of file size reduction (AC 3.1.5c).

### Tasks
- [x] Update [schema/motor-schema-v1.0.0.json](schema/motor-schema-v1.0.0.json):
  - [x] Replace voltage `series` definition from array-of-series-with-data[] to series table/map format.
  - [x] Add new motor properties (`brakeResponseTime`, `brakeEngageTimeDiode`, `brakeEngageTimeMOV`, `brakeBacklash`).
  - [x] Expand `units` schema to include: `responseTime`, `backlash`, `percentage`, `inertia`, `temperature`, `torqueConstant`.
  - [x] Ensure `backlash` default is `arcmin`.
  - [x] Add array length constraints (101) where practical (`minItems`/`maxItems`).
  - [x] Ensure additionalProperties remains constrained where appropriate.
- [x] Update [schema/example-motor.json](schema/example-motor.json) to the new format.
- [x] Add a size benchmark/test artifact:
  - [x] Option A (preferred): xUnit test that serializes the same in-memory motor to:
    - legacy per-point shape (via a local legacy DTO in test only), and
    - new table/map shape,
    and asserts new JSON length is smaller.
  - [x] Option B: a small markdown file that records measured sizes from a representative sample.

### Done when
- `schema/example-motor.json` conforms to `schema/motor-schema-v1.0.0.json` (at least by inspection / external schema validation if available).
- The repo contains an automated or documented measurement demonstrating size reduction.

### Files (expected)
- Update: [schema/motor-schema-v1.0.0.json](schema/motor-schema-v1.0.0.json)
- Update: [schema/example-motor.json](schema/example-motor.json)
- Add: `tests/CurveEditor.Tests/Services/MotorFileSizeBenchmarkTests.cs` (or similar)

### Quick manual test
1. Open a saved JSON file.
2. Edit `rpm` to introduce a decreasing value.
3. Try to load/save and confirm it fails with a clear message and a log entry.

---

## [x] PR 4: Hardening + AC-driven validation pass

### Goal
Finish round-trip correctness, ensure logging is clean, and complete AC verification.

### Tasks
- [x] Ensure round-trip stability (save -> load -> save) in tests:
  - [x] No loss of `locked` or `notes`.
  - [x] Percent/rpm/torque values remain identical.
- [x] Ensure `DirectoryBrowserViewModel` background validation remains fast and doesn’t allocate huge structures unnecessarily.
- [x] Audit for accidental behavior changes outside persistence/validation.

Required hygiene:
- [x] Run unit tests.
- [x] Confirm no new noisy log spam on expected failures.

### Done when
- All acceptance criteria are satisfied.
- Manual validation script passes.

### Final manual validation script (AC-driven)
1. (AC 3.1.5a) Create a motor, save it, verify JSON uses series table/map and `schemaVersion` is `1.0.0`.
2. (AC 3.1.5b) Close and reopen the saved file; confirm chart and grid match prior state.
3. (AC 3.1.5c) Run the benchmark/test artifact and record/observe that new JSON is smaller.
4. (AC 3.1.5d) Remove new fields from JSON, reload, confirm defaults applied and save reintroduces fields.

### Sign-off checklist
- [x] All tasks across all PR sections are checked `[x]`.
- [x] Every AC has a verification step (test or manual script).
- [x] No out-of-scope features were implemented.

### Files (expected)
- Update: [schema/motor-schema-v1.0.0.json](schema/motor-schema-v1.0.0.json)
- Update: [schema/example-motor.json](schema/example-motor.json)
- Add: `tests/CurveEditor.Tests/Services/MotorFileSizeBenchmarkTests.cs` (or similar)

### Quick manual test
1. Open `schema/example-motor.json`.
2. Confirm it uses `percent`/`rpm` arrays and `series` map.
3. Run tests and confirm the benchmark test passes.
