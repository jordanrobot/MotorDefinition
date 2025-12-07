## Curve Editor Phase 1 Plan

### 1. Single-File Publishing

- **Goal**: Configure publishing so the Windows build produces a single-file executable that contains the entire application (runtime + dependencies), suitable for distribution and basic testing on machines without a full .NET SDK.

#### 1.1 Decide Target Runtime and Layout
- [ ] Confirm primary target runtime for distribution:
  - Likely `win-x64` (Windows 64-bit) as per roadmap quick-start example.
- [ ] Decide whether you want:
  - A single EXE with no companion files (fully self-contained), or
  - A single-file EXE plus a small set of satellite files (e.g., PDBs, config, optional trimming).
- [ ] Document these decisions in the planning docs so future phases use the same conventions.

#### 1.2 MSBuild Publish Settings (Project-Level)
- [ ] Update `src/CurveEditor/CurveEditor.csproj` with publish properties appropriate for single-file:
  - `RuntimeIdentifier` (for local publish defaults, e.g., `win-x64`).
  - `PublishSingleFile=true` to produce a single-file app.
  - `SelfContained=true` so the .NET runtime is bundled with the app.
- [ ] Consider additional properties:
  - `IncludeNativeLibrariesForSelfExtract=true` (as needed for Avalonia/Skia native libraries).
  - `PublishTrimmed=false` initially (avoid trimming until you’ve validated you won’t break Avalonia/Serilog/etc.).
- [ ] Ensure these settings are configured either:
  - Under a `Release` PropertyGroup, or
  - Behind a dedicated publish profile (.pubxml) to keep Debug builds fast and developer-friendly.

#### 1.3 Publish Profiles and Commands
- [ ] Create a publish profile (e.g., `Properties/PublishProfiles/WinSingleFile.pubxml`) that encodes:
  - `RuntimeIdentifier=win-x64`
  - `PublishSingleFile=true`
  - `SelfContained=true`
  - `Configuration=Release`
- [ ] Verify the existing quick-start command in `04-mvp-roadmap.md`:
  ```bash
  dotnet publish src/CurveEditor -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
  ```
  - [ ] Align the publish profile with this CLI command so both produce equivalent artifacts.
- [ ] Decide on a standard output folder for the single-file executable (e.g., `src/CurveEditor/bin/Release/net8.0/win-x64/publish`), and document it.

#### 1.4 Packaging with Avalonia and Native Dependencies
- [ ] Verify that Avalonia and Skia native libraries are correctly included in the single-file build:
  - Run a publish and inspect the output for expected native assemblies.
- [ ] Confirm whether any content files (e.g., icons, default JSON sample) must remain as external files:
  - If so, treat them as content and document that the app still ships with those sidecar files.
- [ ] If necessary, adjust project properties or `ItemGroup` entries to ensure resources are embedded or copied as intended for single-file deployment.

#### 1.5 Smoke Testing the Single-File Build
- [ ] Publish using the new profile/command on a dev machine.
- [ ] Run the resulting EXE on:
  - The development machine (with SDK installed).
  - Preferably on a clean Windows install or VM without the .NET SDK, to confirm true self-contained behavior.
- [ ] Validate basic scenarios:
  - Application starts and shows the main window.
  - Can open the sample JSON file.
  - Chart and grid render correctly.
- [ ] Capture any startup errors related to missing native libraries or resources and feed back into MSBuild/publish configuration.

### 2. Documentation and Developer Workflow

- **Goal**: Make it easy for future you (and others) to reproduce the single-file build reliably.

#### 2.1 Update Roadmap / README
- [ ] Update `04-mvp-roadmap.md` Phase 1.1 "Configure single-file publishing" as complete once:
  - The publish profile is in place, and
  - A single-file build has been successfully smoke-tested.
- [ ] Add a short section to the README (or a `docs/build-and-deploy.md`) describing:
  - The publish profile name.
  - The exact `dotnet publish` command to run for a single-file build.
  - The location of the resulting EXE.

#### 2.2 Optional Convenience Scripts
- [ ] Optionally add a small PowerShell script or `.cmd` helper (e.g., `build-singlefile.cmd`) that invokes the correct `dotnet publish` command so non-.NET experts can build the single-file EXE with one step.
- [ ] Document that script’s usage alongside the publish instructions.

### 3. Phase 1 Completion Criteria

- [ ] A single-file, self-contained Windows EXE can be produced for the CurveEditor using a documented command or publish profile.
- [ ] The EXE runs successfully on a clean Windows 11 machine (or VM) without installing the .NET SDK.
- [ ] `04-mvp-roadmap.md` item "Configure single-file publishing" is marked complete, with a pointer to the build instructions/doc.
