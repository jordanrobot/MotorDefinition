## Phase 3.1.6 Subtasks: .NET Client Library Skeleton Project (Agent Execution Checklist)

### Purpose
- Provide a PR-sliceable task list for implementing Phase 3.1.6 with minimal rework.
- Make it easy to validate each Phase 3.1.6 acceptance criterion incrementally.

### Execution Rules (Mandatory)
- Treat this file as the single source of truth for work tracking.
- Do not start a PR section until the prior PR section is complete.
- When a task is completed, mark it as `[x]` immediately.
- A PR section is not complete until:
  - All tasks are checked `[x]`, AND
  - The "Done when" criteria are satisfied.
- Do not add “nice-to-haves” that are not listed in this file, the Phase 3.1.6 requirements, or the Phase 3.1.6 plan.

### Inputs
- Requirements: [.github/planning/phase-3-requirements.md](.github/planning/phase-3-requirements.md) (Phase 3.1.6 section)
- Plan: [.github/planning/phase-3.1.6-plan.md](.github/planning/phase-3.1.6-plan.md)
- ADRs:
  - [docs/adr/adr-0006-motor-file-schema-and-versioning.md](docs/adr/adr-0006-motor-file-schema-and-versioning.md)
  - [docs/adr/adr-0009-logging-and-error-handling-policy.md](docs/adr/adr-0009-logging-and-error-handling-policy.md)

### Scope Reminder (Phase 3.1.6)
- Add a new SDK-style class library project `jordanrobot.MotorDefinition` to the solution.
- The library must not reference Avalonia or the app assembly.
- Rehome/move the Phase 3.1.5 persistence layer (DTOs + mapper + validators + probe) into the library with minimal churn.
- Rehome schema-aligned runtime models into the library (so the library can own file IO end-to-end without referencing the app).
- Add a single library entrypoint for file IO (load/save).
- Copy the schema into the library.
- Perform required rename and move operations, verifying build after each:
  - Rename the program CurveEditor to MotorEditor.
  - Rename the project CurveEditor to MotorEditor.Avalonia.
  - Move app code to `src/MotorEditor.Avalonia/`.
  - Update the app to reference the new library for motor definition file IO.

Out of scope reminders:
- Do not publish a NuGet package (Phase 3.1.7).
- Do not implement consumer-facing non-throwing APIs / structured error model (Phase 3.1.8).

### Key Files (Expected touch points)
- Solution:
  - [CurveEditor.sln](CurveEditor.sln)
- Current app project:
  - [src/CurveEditor/CurveEditor.csproj](src/CurveEditor/CurveEditor.csproj)
  - [src/CurveEditor/Program.cs](src/CurveEditor/Program.cs)
  - [src/CurveEditor/app.manifest](src/CurveEditor/app.manifest)
  - [src/CurveEditor/AssemblyInfo.cs](src/CurveEditor/AssemblyInfo.cs) (internals visibility)
- Current persistence layer (to move):
  - [src/CurveEditor/MotorDefinitions/Dtos/MotorDefinitionFileDto.cs](src/CurveEditor/MotorDefinitions/Dtos/MotorDefinitionFileDto.cs)
  - [src/CurveEditor/MotorDefinitions/Mapping/MotorFileMapper.cs](src/CurveEditor/MotorDefinitions/Mapping/MotorFileMapper.cs)
  - [src/CurveEditor/MotorDefinitions/Probing/MotorFileProbe.cs](src/CurveEditor/MotorDefinitions/Probing/MotorFileProbe.cs)
  - [src/CurveEditor/MotorDefinitions/Validation/MotorFileShapeValidator.cs](src/CurveEditor/MotorDefinitions/Validation/MotorFileShapeValidator.cs)
- File IO integration:
  - [src/CurveEditor/Services/FileService.cs](src/CurveEditor/Services/FileService.cs)
  - [src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs](src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs)
- Runtime models (to move into the library):
  - [src/CurveEditor/Models/MotorDefinition.cs](src/CurveEditor/Models/MotorDefinition.cs)
  - [src/CurveEditor/Models/DriveConfiguration.cs](src/CurveEditor/Models/DriveConfiguration.cs)
  - [src/CurveEditor/Models/VoltageConfiguration.cs](src/CurveEditor/Models/VoltageConfiguration.cs)
  - `src/CurveEditor/Models/*.cs` (other schema-aligned model files)
- Schema:
  - [schema/motor-schema-v1.0.0.json](schema/motor-schema-v1.0.0.json)
- Publish script/profile:
  - [build-singlefile.ps1](build-singlefile.ps1)
  - [src/CurveEditor/Properties/PublishProfiles/WinSingleFile.pubxml](src/CurveEditor/Properties/PublishProfiles/WinSingleFile.pubxml)
- Tests:
  - [tests/CurveEditor.Tests/CurveEditor.Tests.csproj](tests/CurveEditor.Tests/CurveEditor.Tests.csproj)
  - [tests/CurveEditor.Tests/MotorDefinitions/MotorFileMapperTests.cs](tests/CurveEditor.Tests/MotorDefinitions/MotorFileMapperTests.cs)
  - [tests/CurveEditor.Tests/Services/MotorFileSizeBenchmarkTests.cs](tests/CurveEditor.Tests/Services/MotorFileSizeBenchmarkTests.cs)

### Acceptance Criteria (Phase 3.1.6)
- AC 3.1.6a: `dotnet build` succeeds for the solution with the new library project added.
- AC 3.1.6b: The library project has zero dependencies on UI frameworks.

### Assumptions and Constraints
- Keep namespaces stable as much as possible in this phase to reduce churn.
  - It is acceptable if the new library assembly contains types in the existing namespaces (e.g., `CurveEditor.Models`, `jordanrobot.MotorDefinitions.*`) temporarily.
- The app can continue to own app-level logging and user-facing exception behavior; the library should remain “pure” (no Serilog, no UI).
- Build verification uses `dotnet build` at solution level and `dotnet test` where existing tests cover refactored areas.

### State Model Summary (Target)
- Library project:
  - `src/jordanrobot.MotorDefinition/jordanrobot.MotorDefinition.csproj` (SDK-style class library, no UI dependencies)
  - Schema model types (moved from app) + persistence layer types (DTOs + mapper + validators + probe)
  - One entrypoint for file IO:
    - `MotorFile` or `MotorFileSerializer` with `Load/Save` APIs (path and optionally stream)
- App project:
  - References the library project.
  - Uses library entrypoint for load/save.
  - Uses library probe for directory browsing validation.
- Tests:
  - Continue running under `tests/CurveEditor.Tests` and validate load/save behavior through the library entrypoint (preferred).

### Agent Notes (Migration Guidance)
- The existing persistence layer is already isolated under `src/CurveEditor/MotorDefinitions/*` and uses library-like namespaces (`jordanrobot.MotorDefinitions.*`). Phase 3.1.6 should preserve this structure and move it with minimal refactoring.
- `MotorDefinitionFileDto` currently references `CurveEditor.Models.MotorDefinition.CurrentSchemaVersion`; after moving models into the library this should resolve without introducing an app reference.
- The test project currently accesses internal persistence code via `[assembly: InternalsVisibleTo("CurveEditor.Tests")]` in [src/CurveEditor/AssemblyInfo.cs](src/CurveEditor/AssemblyInfo.cs). After moving internals to the library, this attribute must move (or tests must switch to only using public library APIs).

### Implementation Notes (to avoid known pitfalls)
- Keep the library free of:
  - Avalonia packages
  - Serilog
  - app services (`IFileService`, `IUserSettingsStore`, etc.)
- Prefer moving tests away from internal DTOs/mapper and toward the public `MotorFile` API.
- Renames/moves must be performed in small PRs and must include explicit build verification tasks.

---

## [ ] PR 0: Lock down naming + build gates (no behavior change)

### Goal
Make the Phase 3.1.6 migration mechanically safe by locking down naming, paths, and build-verification rules before moving code.

### Tasks
- [ ] Decide and document (in this file) the library API type name: `MotorFile` vs `MotorFileSerializer` (pick one and do not rename later).
- [ ] Decide and document the library project folder path: `src/jordanrobot.MotorDefinition/` (keep as-is).
- [ ] Decide how tests will access persistence behavior:
  - [ ] Preferred: test only via the new public library entrypoint.
  - [ ] If needed: plan `InternalsVisibleTo` in the library assembly.
- [ ] Define build gate commands to use for every PR in this phase:
  - [ ] `dotnet build CurveEditor.sln`
  - [ ] `dotnet test CurveEditor.sln` (run when PR touches tests or IO paths)

Required hygiene:
- [ ] No production code behavior changes in this PR.

### Done when
- Decisions above are recorded in this tasks file.

### Files
- [.github/planning/phase-3.1.6-tasks.md](.github/planning/phase-3.1.6-tasks.md)

### Quick manual test
1. Run `dotnet build CurveEditor.sln`.

---

## [ ] PR 1: Add `jordanrobot.MotorDefinition` library project scaffold

### Goal
Create and add a new SDK-style class library project to the solution without impacting app behavior.

### Tasks
- [ ] Create `src/jordanrobot.MotorDefinition/jordanrobot.MotorDefinition.csproj`:
  - [ ] Target `net8.0`.
  - [ ] No references to Avalonia packages.
- [ ] Add minimal folder structure:
  - [ ] `Schema/` (for schema copy)
  - [ ] `MotorDefinitions/` (for DTOs/mapper/probe/validator)
  - [ ] `Models/` (for schema-aligned runtime models)
- [ ] Add placeholder entrypoint:
  - [ ] `MotorFile` (or chosen name) with stub `Load/Save` signatures (no wiring yet).
- [ ] Add the library project to [CurveEditor.sln](CurveEditor.sln) under the `src` solution folder.

Required hygiene:
- [ ] Do not change `FileService` or Directory Browser behavior in this PR.

### Done when
- `dotnet build CurveEditor.sln` succeeds.

### Files
- `src/jordanrobot.MotorDefinition/jordanrobot.MotorDefinition.csproj`
- `CurveEditor.sln`
- New library source files (placeholder entrypoint)

### Quick manual test
1. Run `dotnet build CurveEditor.sln`.

---

## [ ] PR 2: Move schema-aligned runtime models into the library

### Goal
Move runtime model types into the library so the library can own file IO end-to-end without referencing the app.

### Tasks
- [ ] Move schema-aligned model files from `src/CurveEditor/Models/` into `src/jordanrobot.MotorDefinition/Models/`.
  - [ ] Include `MotorDefinition`, `DriveConfiguration`, `VoltageConfiguration`, `CurveSeries`, `DataPoint`, `UnitSettings`, `MotorMetadata` and any supporting model files required by compilation.
- [ ] Ensure moved models compile without Avalonia dependencies.
- [ ] Update the app project to reference the library project:
  - [ ] Add a `ProjectReference` from `src/CurveEditor/CurveEditor.csproj` to `src/jordanrobot.MotorDefinition/jordanrobot.MotorDefinition.csproj`.
- [ ] Ensure the app no longer compiles the moved model files (remove them from the app project via file move).
- [ ] Ensure tests still compile:
  - [ ] If tests depended on app for models, they should resolve through the app’s transitive reference or reference the library directly.

Required hygiene:
- [ ] Do not change any user-visible UI behavior.

### Done when
- `dotnet build CurveEditor.sln` succeeds.
- Library has no UI framework dependencies.

### Files
- `src/CurveEditor/Models/*.cs` (moved)
- `src/jordanrobot.MotorDefinition/Models/*.cs` (new location)
- `src/CurveEditor/CurveEditor.csproj`
- Potentially `tests/CurveEditor.Tests/CurveEditor.Tests.csproj`

### Quick manual test
1. Run `dotnet build CurveEditor.sln`.
2. Run `dotnet test CurveEditor.sln`.

---

## [ ] PR 3: Move persistence layer into the library + introduce working library IO entrypoint

### Goal
Make the library own the DTO/mapper/probe/validation code and provide a functional load/save entrypoint, while keeping app logging and UX intact.

### Tasks
- [ ] Move persistence layer into the library (minimal churn):
  - [ ] Move `src/CurveEditor/MotorDefinitions/**` → `src/jordanrobot.MotorDefinition/MotorDefinitions/**`.
- [ ] Update persistence code so it does not reference app-only concerns:
  - [ ] Ensure schema version checks reference the model constant now located in the library.
  - [ ] Ensure no Serilog usage appears in the library.
- [ ] Implement the library IO entrypoint (`MotorFile` / chosen name):
  - [ ] `Load(string path)` returns `MotorDefinition`.
  - [ ] `Save(MotorDefinition motor, string path)` writes JSON.
  - [ ] Use System.Text.Json options consistent with current `FileService` where appropriate.
- [ ] Handle internals/test access:
  - [ ] Preferred: update tests to use `MotorFile` entrypoint.
  - [ ] If tests still need internal access: add `InternalsVisibleTo("CurveEditor.Tests")` in the library assembly.
  - [ ] Remove or adjust the existing [src/CurveEditor/AssemblyInfo.cs](src/CurveEditor/AssemblyInfo.cs) as appropriate.
- [ ] Copy schema into the library:
  - [ ] Copy `schema/motor-schema-v1.0.0.json` → `src/jordanrobot.MotorDefinition/Schema/motor-schema-v1.0.0.json`.
  - [ ] Include as an Embedded Resource.
- [ ] Update app integrations:
  - [ ] Update [src/CurveEditor/Services/FileService.cs](src/CurveEditor/Services/FileService.cs) to call `MotorFile.Load/Save`.
  - [ ] Keep app-level logging/error handling behavior in `FileService` (per ADR-0009).
  - [ ] Update [src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs](src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs) to use the library probe API.
- [ ] Update tests to reflect new assembly boundaries:
  - [ ] Update [tests/CurveEditor.Tests/MotorDefinitions/MotorFileMapperTests.cs](tests/CurveEditor.Tests/MotorDefinitions/MotorFileMapperTests.cs) to test via `MotorFile` entrypoint where possible.
  - [ ] Update [tests/CurveEditor.Tests/Services/MotorFileSizeBenchmarkTests.cs](tests/CurveEditor.Tests/Services/MotorFileSizeBenchmarkTests.cs) to reference the correct mapper/entrypoint location.

### Done when
- `dotnet build CurveEditor.sln` succeeds.
- `dotnet test CurveEditor.sln` succeeds.
- Library has zero UI framework dependencies.

### Files
- `src/jordanrobot.MotorDefinition/MotorDefinitions/**` (new location)
- `src/jordanrobot.MotorDefinition/MotorFile*.cs` (new)
- `src/jordanrobot.MotorDefinition/Schema/motor-schema-v1.0.0.json` (new)
- [src/CurveEditor/Services/FileService.cs](src/CurveEditor/Services/FileService.cs)
- [src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs](src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs)
- Tests under `tests/CurveEditor.Tests/**`

### Quick manual test
1. Run `dotnet test CurveEditor.sln`.
2. Run the app, open a motor JSON, save, re-open.

---

## [ ] PR 4: Rename the program CurveEditor → MotorEditor (VERIFY BUILD)

### Goal
Rename the product/program naming (without changing layout/persistence behavior) and verify build immediately afterward.

### Tasks
- [ ] Update program branding:
  - [ ] Update [src/CurveEditor/Program.cs](src/CurveEditor/Program.cs) log messages and log directory path (e.g., `%AppData%/MotorEditor/logs`).
  - [ ] Update [src/CurveEditor/CurveEditor.csproj](src/CurveEditor/CurveEditor.csproj): `AssemblyName`, `Product`, and description if needed.
  - [ ] Update [src/CurveEditor/app.manifest](src/CurveEditor/app.manifest) assembly identity name.
- [ ] Verify build immediately after the rename (required by Phase 3.1.6).

Required hygiene:
- [ ] Do not rename persisted settings keys in this phase.

### Done when
- `dotnet build CurveEditor.sln` succeeds.

### Files
- [src/CurveEditor/Program.cs](src/CurveEditor/Program.cs)
- [src/CurveEditor/CurveEditor.csproj](src/CurveEditor/CurveEditor.csproj)
- [src/CurveEditor/app.manifest](src/CurveEditor/app.manifest)

### Quick manual test
1. Run `dotnet build CurveEditor.sln`.
2. Launch app and confirm it starts.

---

## [ ] PR 5: Rename the project CurveEditor → MotorEditor.Avalonia (VERIFY BUILD)

### Goal
Rename the app project and update all references (solution + test references), verifying build immediately afterward.

### Tasks
- [ ] Rename project file:
  - [ ] `src/CurveEditor/CurveEditor.csproj` → `src/CurveEditor/MotorEditor.Avalonia.csproj`.
- [ ] Update solution to reference the new project path/name.
- [ ] Update test project reference:
  - [ ] Update [tests/CurveEditor.Tests/CurveEditor.Tests.csproj](tests/CurveEditor.Tests/CurveEditor.Tests.csproj) to point at the renamed csproj.
- [ ] Verify build immediately after the rename (required by Phase 3.1.6).

Required hygiene:
- [ ] Do not rename namespaces en masse in this phase unless required to compile.

### Done when
- `dotnet build CurveEditor.sln` succeeds.

### Files
- `src/CurveEditor/MotorEditor.Avalonia.csproj`
- [CurveEditor.sln](CurveEditor.sln)
- [tests/CurveEditor.Tests/CurveEditor.Tests.csproj](tests/CurveEditor.Tests/CurveEditor.Tests.csproj)

### Quick manual test
1. Run `dotnet build CurveEditor.sln`.

---

## [ ] PR 6: Move app folder to src/MotorEditor.Avalonia and update scripts (VERIFY BUILD)

### Goal
Move the app codebase directory and ensure the solution, tests, and publish script still work.

### Tasks
- [ ] Move folder:
  - [ ] `src/CurveEditor/` → `src/MotorEditor.Avalonia/`.
- [ ] Update solution project path to the moved csproj.
- [ ] Update tests project reference path to the moved csproj.
- [ ] Update publish script:
  - [ ] Update [build-singlefile.ps1](build-singlefile.ps1) to publish from `src/MotorEditor.Avalonia`.
  - [ ] Update the output path message in the script.
- [ ] Verify build immediately after the move (required by Phase 3.1.6).

### Done when
- `dotnet build CurveEditor.sln` succeeds.
- `dotnet test CurveEditor.sln` succeeds.

### Files
- Moved app files under `src/MotorEditor.Avalonia/**`
- [CurveEditor.sln](CurveEditor.sln)
- [tests/CurveEditor.Tests/CurveEditor.Tests.csproj](tests/CurveEditor.Tests/CurveEditor.Tests.csproj)
- [build-singlefile.ps1](build-singlefile.ps1)

### Quick manual test
1. Run `dotnet build CurveEditor.sln`.
2. Run `dotnet test CurveEditor.sln`.
3. Run `./build-singlefile.ps1` and confirm output exists.

---

## [ ] PR 7: Final AC validation + dependency audit

### Goal
Confirm Phase 3.1.6 acceptance criteria and ensure the library is cleanly separated from UI dependencies.

### Tasks
- [ ] Validate AC 3.1.6a:
  - [ ] Run `dotnet build CurveEditor.sln`.
- [ ] Validate AC 3.1.6b:
  - [ ] Confirm the library csproj has no UI package references.
  - [ ] Confirm no library source file references Avalonia namespaces.
- [ ] Confirm app references the library for file IO:
  - [ ] File load/save uses library entrypoint.
  - [ ] Directory Browser probe uses library code.
- [ ] Ensure tests still run and cover the moved IO boundary.

### Done when
- All Phase 3.1.6 acceptance criteria pass.

### Final manual validation script (AC-driven)
1. Run `dotnet build CurveEditor.sln`.
2. Run `dotnet test CurveEditor.sln`.
3. Launch the app, open a motor JSON, save it, and re-open it.
4. Use Directory Browser to confirm JSON filtering still works.
5. Run `./build-singlefile.ps1`.

### Sign-off checklist
- [ ] All tasks across all PR sections are checked `[x]`.
- [ ] All acceptance criteria listed above have a verification step (test or manual script).
- [ ] No out-of-scope features were implemented.
