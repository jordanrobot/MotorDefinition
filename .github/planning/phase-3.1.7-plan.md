## Curve Editor Phase 3.1.7 Plan: NuGet Package Skeleton

### Status

Draft

**Related ADRs**

- ADR-0006 Motor File Schema and Versioning Strategy (`docs/adr/adr-0006-motor-file-schema-and-versioning.md`)

### Goal

- Prepare the motor definition library project for NuGet packaging.
- Produce a local `.nupkg` via `dotnet pack`.
- Validate the package can be referenced by a minimal sample app and can load a motor definition JSON file.

### Scope

In scope (Phase 3.1.7)

- Configure package metadata in the library project:
  - `PackageId`, `Version`, `Authors`, `Description`, `RepositoryUrl`.
- Ensure XML documentation output is enabled for the library.
- Add a minimal package README with consumer usage examples:
  - Load
  - Save
  - “shape probe” (`IsLikelyMotorDefinition`) as the current lightweight validation surface.
- Add a minimal automated test project that validates basic round-trip save/load for the current format.
- Add a minimal sample console app that references the **packed** NuGet package from a local source and loads a file.

Out of scope (Phase 3.1.7)

- Public non-throwing load APIs and structured validation error models (Phase 3.1.8).
- Publishing automation (e.g., nuget.org push) or CI pipelines.
- Adding additional NuGet “polish” fields beyond what is required (icons, release notes, etc.).

### Current Baseline

- Library project:
  - `src/MotorDefinition/MotorDefinition.csproj`
  - Public entrypoint: `JordanRobot.MotorDefinitions.MotorFile`
  - Public runtime model types: `JordanRobot.MotorDefinitions.Model.*`
  - Persistence internals: `JordanRobot.MotorDefinitions.Persistence.*` (internal)
- Existing tests:
  - `tests/CurveEditor.Tests/MotorEditor.Tests.csproj` references the app project.
- The library already enables XML docs generation (`GenerateDocumentationFile=true`).
- The library currently includes `README.md` as a packed file, but does not declare `PackageReadmeFile`.

### Decisions (Proposed)

Package identity

- `PackageId`: `JordanRobot.MotorDefinitions` (confirmed)
  - Rationale: matches the public API namespace root.

Versioning for the package

- Use a prerelease version for Phase 3.1.7 (package skeleton work):
  - `Version = 1.0.0-alpha.1` (confirmed)
  - Rationale: schema is currently `1.0.0` (see `MotorDefinition.CurrentSchemaVersion`), but Phase 3.1.7 is explicitly “prepare for packaging”, not “declare a stable client library”.

Repository metadata

- `RepositoryUrl`: `https://github.com/jordanrobot/curve`
  - Source: schema `$id` values currently point under this repo URL.
- Use SourceLink (`Microsoft.SourceLink.GitHub`) and `PublishRepositoryUrl=true` (already present).

Authors

- `Authors`: `mjordan`
  - Source: most ADRs are authored by `mjordan`.

Description

- `Description`: "Motor definition JSON load/save library (Motor → Drive(s) → Voltage(s) → Curve series)."
  - Source: summary of the library section in the root README.

README packaging

- Keep the README in `src/MotorDefinition/README.md`.
- Add `PackageReadmeFile=README.md` so NuGet tooling displays it.

Tests

- Add a new, library-only test project (recommended name: `tests/MotorDefinition.Tests`).
  - Rationale: keeps packaging/round-trip tests independent of UI/app references.
  - Tests should use only the public surface (`MotorFile` + model types), not internals.

Sample app

- Add `samples/MotorDefinition.Sample` (console app).
- The sample should restore from the locally packed `.nupkg` (local folder source).
- The sample should load `schema/example-motor.json` and print at least the motor name.

### Implementation Outline

1. Update `MotorDefinition.csproj` packaging metadata
  - Add the required NuGet properties.
  - Ensure `PackageReadmeFile` is set and the README is included exactly once.

2. Expand the library README
  - Add a short “Quick usage” section with code examples.
  - Keep examples aligned to the current public API.

3. Add a library-only round-trip test project
  - Round-trip a representative in-repo motor JSON file:
    - Load via `MotorFile.Load(...)`
    - Save to a temp file via `MotorFile.Save(...)`
    - Load again and assert key fields.

4. Add a minimal sample app referencing the packed package
  - Demonstrate `dotnet pack` -> local source -> `dotnet add package` -> run.
  - This validates that the `.nupkg` is usable as a consumer artifact.

5. Validation gates
  - `dotnet build CurveEditor.slnx -c Release`
  - `dotnet test CurveEditor.slnx -c Release`
  - `dotnet pack src/MotorDefinition/MotorDefinition.csproj -c Release`
  - Run the sample app against the local package.

### Risks and Mitigations

- README packing confusion (duplicate items, missing `PackageReadmeFile`).
  - Mitigation: declare `PackageReadmeFile` and include README only once with explicit `Pack=true`.

- Package version strategy ambiguity (schema version vs package version).
  - Mitigation: start with an explicit prerelease version in Phase 3.1.7; revisit alignment rules in Phase 3.1.8 where it is a requirement.

- Test project accidentally pulling in UI dependencies.
  - Mitigation: ensure the new test project references `src/MotorDefinition/MotorDefinition.csproj` directly, not the app project.
