## Phase 3.1.7 Subtasks: NuGet Package Skeleton (Agent Execution Checklist)

### Purpose

- Provide a PR-sliceable task list for implementing Phase 3.1.7 with minimal churn.
- Make it easy to validate each requirement and acceptance criterion incrementally.

### Execution Rules (Mandatory)

- Treat this file as the single source of truth for work tracking.
- Do not start a PR section until the prior PR section is complete.
- When a task is completed, mark it as `[x]` immediately.
- A PR section is not complete until:
  - All tasks are checked `[x]`, AND
  - The "Done when" criteria are satisfied.
- Do not add “nice-to-haves” that are not listed in this file or Phase 3.1.7 requirements.

### Inputs

- Requirements: `.github/planning/phase-3-requirements.md` (Phase 3.1.7 section)
- Plan: `.github/planning/phase-3.1.7-plan.md`
- ADR-0006: `docs/adr/adr-0006-motor-file-schema-and-versioning.md`

### Scope Reminder (Phase 3.1.7)

- Prepare the library project for NuGet packaging (local pack).
- Configure required package metadata.
- Ensure XML docs output is enabled.
- Add minimal package README with usage examples.
- Add minimal automated round-trip test project.
- Add a minimal sample app that references the packed package.

Out of scope reminders

- No publishing automation (nuget.org).
- No non-throwing load API / structured validation errors (Phase 3.1.8).

### Key Files (Expected touch points)

- Library project:
  - `src/MotorDefinition/MotorDefinition.csproj`
  - `src/MotorDefinition/README.md`
- Sample input file:
  - `schema/example-motor.json`
- Solution:
  - `CurveEditor.slnx`
- New:
  - `tests/MotorDefinition.Tests/MotorDefinition.Tests.csproj`
  - `samples/MotorDefinition.Sample/MotorDefinition.Sample.csproj`

### Acceptance Criteria (Phase 3.1.7)

- AC 3.1.7a: `dotnet pack` produces a `.nupkg` locally.
- AC 3.1.7b: The package can be referenced by a sample app and successfully loads a motor definition file.

---

## [ ] PR 0: Lock down packaging decisions (no behavior changes)

### Tasks

- [ ] Confirm the intended NuGet identity values:
  - [x] `PackageId` = `JordanRobot.MotorDefinitions`
  - [x] `Version` = `1.0.0-alpha.1`
  - [x] `Authors` = `mjordan`
  - [x] `RepositoryUrl` = `https://github.com/jordanrobot/curve`
  - [x] `Description` = "Motor definition JSON load/save library (Motor → Drive(s) → Voltage(s) → Curve series)."
- [ ] Confirm the README location to pack (`src/MotorDefinition/README.md`).
- [ ] Confirm we will add a **new library-only test project** instead of extending the existing app-level tests.
- [ ] Record the exact build/pack verification commands for this phase:
  - [ ] `dotnet build CurveEditor.slnx -c Release`
  - [ ] `dotnet test CurveEditor.slnx -c Release`
  - [ ] `dotnet pack src/MotorDefinition/MotorDefinition.csproj -c Release`

### Done when

- Decisions are recorded in this PR section.

---

## [ ] PR 1: Add required NuGet package metadata + pack correctness

### Tasks

- [ ] Update `src/MotorDefinition/MotorDefinition.csproj`:
  - [ ] Set required NuGet properties:
    - [ ] `PackageId`
    - [ ] `Version`
    - [ ] `Authors`
    - [ ] `Description`
    - [ ] `RepositoryUrl`
  - [ ] Ensure XML docs output remains enabled (should already be `GenerateDocumentationFile=true`).
  - [ ] Set `PackageReadmeFile=README.md`.
  - [ ] Ensure README is packed exactly once (avoid duplicate `<None Include="README.md" />` entries).

### Done when

- `dotnet pack src/MotorDefinition/MotorDefinition.csproj -c Release` produces a `.nupkg`.

---

## [ ] PR 2: Minimal package README with consumer examples

### Tasks

- [ ] Expand `src/MotorDefinition/README.md` to include:
  - [ ] One-paragraph overview (“what is this library?”).
  - [ ] Load example (path-based).
  - [ ] Save example.
  - [ ] Lightweight validation/probing example using `MotorFile.IsLikelyMotorDefinition`.
  - [ ] Note about schema version being `MotorDefinition.CurrentSchemaVersion`.
- [ ] Verify README renders as the package README (via `PackageReadmeFile`).

### Done when

- README includes minimal runnable examples and is packed into the `.nupkg`.

---

## [ ] PR 3: Add library-only round-trip test project

### Tasks

- [ ] Create new test project `tests/MotorDefinition.Tests`:
  - [ ] xUnit test project targeting `net8.0`.
  - [ ] References `src/MotorDefinition/MotorDefinition.csproj` (not the app).
- [ ] Add a round-trip test:
  - [ ] Load `schema/example-motor.json` via `MotorFile.Load(...)`.
  - [ ] Save to a temp file.
  - [ ] Load again.
  - [ ] Assert core fields are preserved (at least `MotorName`, plus one nested structure assertion).

### Done when

- Tests pass via `dotnet test CurveEditor.slnx -c Release`.

---

## [ ] PR 4: Add minimal sample app that references the packed package

### Tasks

- [ ] Create `samples/MotorDefinition.Sample` console app.
- [ ] Document and validate local package restore:
  - [ ] Pack the library to a known output folder (e.g., `artifacts/packages`).
  - [ ] Configure the sample to restore from that local folder source.
  - [ ] Reference the package by `PackageId` + `Version`.
- [ ] In the sample app:
  - [ ] Load `schema/example-motor.json`.
  - [ ] Print the motor name.
- [ ] Add a short “How to run” snippet in the sample README or in the root README (minimal).

### Done when

- Sample app builds and runs using the locally packed `.nupkg`.
- Meets AC 3.1.7b.

---

## [ ] PR 5: Final validation pass (Phase 3.1.7)

### Tasks

- [ ] Run the full Phase 3.1.7 gates:
  - [ ] `dotnet build CurveEditor.slnx -c Release`
  - [ ] `dotnet test CurveEditor.slnx -c Release`
  - [ ] `dotnet pack src/MotorDefinition/MotorDefinition.csproj -c Release`
- [ ] Run the sample app using only the local package reference.

### Done when

- AC 3.1.7a and AC 3.1.7b are satisfied.
