## Curve Editor Phase 3.1.6 Plan

### Status

Completed

**Related ADRs**

- ADR-0006 Motor File Schema and Versioning Strategy (`docs/adr/adr-0006-motor-file-schema-and-versioning.md`)
- ADR-0009 Logging and Error Handling Policy (`docs/adr/adr-0009-logging-and-error-handling-policy.md`)

### Goal

- Introduce a new SDK-style .NET class library project (`jordanrobot.MotorDefinition`) that owns the motor definition file format (models + load/save) with zero UI framework dependencies.
- Update the Avalonia app to consume this library for motor definition file IO.
- Perform required renames and folder moves (CurveEditor → MotorEditor, project rename, and `src/` path move) with a strict “verify build after each step” workflow.

### Scope

- In scope:
  - [x] Add new class library project `jordanrobot.MotorDefinition` to the solution.
  - [x] Move the existing persistence layer (DTOs + mapper + validators + probe) into the library with minimal churn.
  - [x] Rehome schema-aligned model types into the library so the library does not reference the Avalonia app.
  - [x] Add a single entrypoint for file IO in the library (e.g., `MotorFileSerializer` or `MotorFile`).
  - [x] Copy the schema into the library project.
  - [x] Perform the required product/project/folder renames and update references.
  - [x] Update unit tests as needed so `dotnet test` still runs.
- Out of scope (not planned for this phase):
  - Publishing a NuGet package (Phase 3.1.7).
  - Consumer-friendly public API surface / non-throwing result types (Phase 3.1.8).
  - Any UI behavior changes beyond what is necessary to keep builds and file IO working.

### Acceptance Criteria

- [x] AC 3.1.6a: `dotnet build` succeeds for the solution with the new library project added.
- [x] AC 3.1.6b: The library project has zero dependencies on UI frameworks (e.g., no Avalonia references).

### Recent Follow-up Fixes (Post-Phase)

- Variable curve point counts are supported when opening/viewing motor JSON files (no longer hard-requires 101 points).
  - Library probe no longer requires 101-length axes: `src/jordanrobot.MotorDefinition/MotorDefinitions/Probing/MotorFileProbe.cs`.
  - App validation no longer blocks loading non-101 series: `src/MotorEditor.Avalonia/Services/ValidationService.cs`.
  - Regression test added: `tests/CurveEditor.Tests/Services/MotorFileVariablePointsLoadTests.cs`.
  - Example schema file includes a 21-point series: `schema/example-motor.json`.

### Assumptions and Constraints

- The existing motor definition “runtime model” in `src/CurveEditor/Models/` is acceptable to rehome into the library (it is not Avalonia-specific today).
- To minimize churn in this phase, namespaces may temporarily remain as-is (e.g., `CurveEditor.Models` and `jordanrobot.MotorDefinitions.*`) even after moving types into the new library assembly. Namespace cleanup can be deferred until after the library is stabilized and before publishing.
- The schema version remains `1.0.0` for all files handled by the library.
- All build verification uses `dotnet build` at solution level (and `dotnet test` where relevant) on Windows.

### Current Baseline (What exists today)

- Solution/projects:
  - `CurveEditor.sln` contains:
    - `src/CurveEditor/CurveEditor.csproj` (Avalonia WinExe)
    - `tests/CurveEditor.Tests/CurveEditor.Tests.csproj`
- Current persistence layer (already isolated in its own namespace, but lives inside the Avalonia app project):
  - DTOs: `src/CurveEditor/MotorDefinitions/Dtos/*.cs`
  - Mapper: `src/CurveEditor/MotorDefinitions/Mapping/MotorFileMapper.cs`
  - Validation: `src/CurveEditor/MotorDefinitions/Validation/MotorFileShapeValidator.cs`
  - Probe: `src/CurveEditor/MotorDefinitions/Probing/MotorFileProbe.cs`
- Current file IO usage:
  - `src/CurveEditor/Services/FileService.cs` deserializes `MotorDefinitionFileDto`, maps via `MotorFileMapper`, and writes JSON.
  - `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs` uses `MotorFileProbe` to filter candidate JSON files.
- Current schema source:
  - `schema/motor-schema-v1.0.0.json` (plus schema-related files under `schema/`).
- Packaging script:
  - `build-singlefile.ps1` publishes `src/CurveEditor` with the WinSingleFile publish profile.

### Proposed Design

#### 1) Data / State Model

- Library-owned schema model types:
  - Rehome the current “file schema aligned” model classes from `src/CurveEditor/Models/` into the library project.
  - These include (at minimum): `MotorDefinition`, `MotorMetadata`, `DriveConfiguration`, `VoltageConfiguration`, `CurveSeries`, `DataPoint`, `UnitSettings`.
- Rationale:
  - The existing mapper (`MotorFileMapper`) currently depends on `CurveEditor.Models` types.
  - If those model types remain in the Avalonia app assembly, the new library would be forced to reference the app, violating the “no UI dependency” constraint.

#### 2) Library API Surface (Phase 3.1.6)

- Add a single entrypoint for file IO in the new library project, for example:
  - `public static class MotorFile` (or `MotorFileSerializer`)
    - `MotorDefinition Load(string path)`
    - `void Save(MotorDefinition motor, string path)`
    - Optional: overloads for `Stream`.
- Internals:
  - Keep DTOs + mapper + validators internal to the library by default.
  - If tests need direct access, add `InternalsVisibleTo` for the test assembly, or update tests to validate via the public entrypoint.

#### 3) Schema Copy / Embedding

- Copy `schema/motor-schema-v1.0.0.json` into the library project (e.g., `src/jordanrobot.MotorDefinition/Schema/motor-schema-v1.0.0.json`).
- Include it as an embedded resource so the library can expose it later without shipping a separate file (Phase 3.1.8 builds on this).

#### 4) App Integration

- Update the Avalonia app project to reference the new library:
  - The app should call the library’s file IO entrypoint for load/save.
  - The app should call the library probe for “is likely motor definition” checks (or move that logic behind a library API).

#### 5) Renames / Restructure

- Perform required renames and folder move:
  - Program name: CurveEditor → MotorEditor
  - Project name: CurveEditor → MotorEditor.Avalonia
  - Folder move: `src/CurveEditor/` → `src/MotorEditor.Avalonia/`
- Update scripts and solution references accordingly.

### Implementation Steps (Incremental)

#### Step 1: Add the library project scaffold (no app behavior change)

- [x] Create `src/jordanrobot.MotorDefinition/jordanrobot.MotorDefinition.csproj` as an SDK-style class library.
  - Target: `net8.0` (match solution); keep dependencies minimal (System.Text.Json only).
  - Confirm no Avalonia packages are referenced.
- [x] Add the project to `CurveEditor.sln` under the `src` solution folder.
- [x] Add an initial `MotorFile` (or `MotorFileSerializer`) entrypoint stub.

**Done when**

- `dotnet build CurveEditor.sln` succeeds.

#### Step 2: Rehome schema-aligned models into the library

- [x] Move model types from `src/CurveEditor/Models/` into the library project.
  - Keep namespaces unchanged initially to avoid large app-wide edits.
  - Ensure model code compiles without referencing Avalonia.
- [x] Update the Avalonia app project to reference the library and remove the moved model source files from the app project.
- [x] Update the test project to reference the library (directly or via the app project) so model types resolve.

**Done when**

- `dotnet build CurveEditor.sln` succeeds.
- Library has zero UI framework references.

#### Step 3: Move persistence DTOs + mapper + validators + probe into the library

- [x] Move `src/CurveEditor/MotorDefinitions/**` into the library.
- [x] Remove app-specific dependencies inside persistence code:
  - Replace any references like `CurveEditor.Models.MotorDefinition.CurrentSchemaVersion` with a library-owned constant source (or keep `MotorDefinition.CurrentSchemaVersion` if `MotorDefinition` moved to the library).
  - Ensure probe/validator/mapper do not use Serilog or app services.
- [x] Decide visibility:
  - Preferred: keep DTOs/mapper internal and test through `MotorFile` APIs.
  - If needed: add `InternalsVisibleTo("CurveEditor.Tests")` (or updated test assembly name after rename).

**Done when**

- `dotnet build CurveEditor.sln` succeeds.

#### Step 4: Route app file IO through the library entrypoint

- [x] Update `src/CurveEditor/Services/FileService.cs`:
  - Replace direct `JsonSerializer.DeserializeAsync<MotorDefinitionFileDto>` + `MotorFileMapper` calls with `MotorFile.Load` / `MotorFile.Save`.
  - Keep app-level logging and error dialogs per ADR-0009.
- [x] Update `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs` to call the library probe API (directly or via `MotorFile` helpers).
- [x] Update tests:
  - Update `MotorFileMapperTests` (and any other tests referencing internal persistence types) to validate behavior via the new public library entrypoint where possible.

**Done when**

- `dotnet build CurveEditor.sln` succeeds.
- `dotnet test CurveEditor.sln` succeeds.

#### Step 5: Required rename 1 — Rename the program CurveEditor to MotorEditor (VERIFY BUILD)

- [x] Update product/program naming artifacts:
  - `src/CurveEditor/Program.cs`: log messages and log directory naming (e.g., `%AppData%/MotorEditor/logs`).
  - `src/CurveEditor/CurveEditor.csproj`: `AssemblyName`, `Product`, and any other branding values.
  - `src/CurveEditor/app.manifest`: assembly identity name.
- [x] Search/replace user-facing strings “CurveEditor” → “MotorEditor” only where appropriate (avoid changing persisted keys unless explicitly intended).

**Done when**

- `dotnet build CurveEditor.sln` succeeds.

#### Step 6: Required rename 2 — Rename the project CurveEditor to MotorEditor.Avalonia (VERIFY BUILD)

- [x] Rename project file and project entry:
  - `src/CurveEditor/CurveEditor.csproj` → `src/CurveEditor/MotorEditor.Avalonia.csproj`.
  - Update `CurveEditor.sln` project name + path.
  - Update `tests/CurveEditor.Tests/CurveEditor.Tests.csproj` project reference to the renamed csproj.
- [x] Decide whether to rename the default root namespace now or defer (defer preferred for minimal churn).

**Done when**

- `dotnet build CurveEditor.sln` succeeds.

#### Step 7: Required move — Move app codebase folder to src/MotorEditor.Avalonia (VERIFY BUILD)

- [x] Move folder:
  - `src/CurveEditor/` → `src/MotorEditor.Avalonia/`.
- [x] Update solution paths:
  - `CurveEditor.sln` project path.
  - `tests/CurveEditor.Tests/CurveEditor.Tests.csproj` project reference path.
- [x] Update scripts:
  - `build-singlefile.ps1` publish path and output message.

**Done when**

- `dotnet build CurveEditor.sln` succeeds.

#### Step 8: Required wiring — Ensure MotorEditor.Avalonia references the new library for file IO (VERIFY BUILD)

- [x] Ensure `MotorEditor.Avalonia` has a `ProjectReference` to `jordanrobot.MotorDefinition`.
- [x] Ensure all app-side persistence usages compile against the library entrypoint.
- [x] Update tests if assembly names changed (e.g., InternalsVisibleTo target).

**Done when**

- `dotnet build CurveEditor.sln` succeeds.
- `dotnet test CurveEditor.sln` succeeds.

### Risks, Edge Cases, and Mitigations

- **Namespace/assembly naming drift**: Keeping old namespaces while renaming projects can be confusing.
  - Mitigation: document the decision in this phase; plan a namespace cleanup step before any NuGet packaging work (Phase 3.1.7/3.1.8).
- **Hidden UI dependencies sneaking into the library**: easy to accidentally reference Avalonia types when moving files.
  - Mitigation: keep library csproj free of UI packages; rely on compiler errors; optionally add a simple test or CI check that inspects package references.
- **Breaking changes to persistence behavior** during refactor.
  - Mitigation: keep existing serialization semantics (System.Text.Json options, null-handling) and validate via round-trip tests.
- **Renames impact persistence keys / settings** (especially `%AppData%` paths).
  - Mitigation: do not rename user-settings keys in this phase unless required; if log folder name changes, accept that it creates a new folder.
- **Path-based scripts** (publish script) break after folder rename.
  - Mitigation: update `build-singlefile.ps1` as part of the move step and include it in build verification.

### Testing Strategy

- Unit tests (existing patterns under `tests/CurveEditor.Tests`):
  - [ ] Update/add tests to cover `MotorFile.Load`/`MotorFile.Save` round-trip for a representative motor with:
    - multiple voltages,
    - multiple series,
    - series metadata (`locked`, `notes`),
    - and the new brake/unit fields already present in DTOs.
  - [ ] Keep a focused test verifying the probe accepts a valid file shape and rejects obvious non-matching JSON.
- Manual validation script:
  - [ ] Launch the app, open an existing motor JSON, verify it loads.
  - [ ] Save and re-open, verify the curves and metadata persist.
  - [ ] Use Directory Browser to verify JSON filtering still works.
  - [ ] Run `build-singlefile.ps1` to confirm publish still succeeds after renames.

### Follow-on Work and TODOs

- [ ] Decide final public namespaces and visibility for consumer usage (Phase 3.1.8).
- [ ] Add non-throwing load API and structured error model (Phase 3.1.8).
- [ ] Add packaging metadata + README for NuGet (Phase 3.1.7).
