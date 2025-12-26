## Phase 3.0 Functional Requirements: Generic Panel Expand/Collapse

### Scope (Phase 3.0)

- Introduce a generic, reusable expand/collapse mechanism for all major panels in the main CurveEditor window.
- Provide a VS Code–style vertical Panel Bar for quick toggling.
- Persist panel visibility and per-panel size settings across sessions via user settings.

### Non-goals (Phase 3.0)

- Do not redesign the internal content of individual panels (Directory Browser, Curve Data, etc.).
- Do not introduce per-document layout variants; panel expand/collapse is global per user/settings, not per file.
- Do not introduce undo/redo for layout changes (panel layout changes are not part of the command history).
- Do not require smooth expand/collapse animations in Phase 3.0.

### CurveEditor Panel Expand/Collapse Mechanism

- [x] The CurveEditor application should have a generic expand/collapse mechanism for window panels. Currently there is an existing expand/collapse mechanism, but it is limited and will be replaced with the following mechanism. This new mechanism should allow users to expand or collapse individual panels within the application. This mechanism should be implemented similar to the way Visual Studio Code handles its side panels. Please refer to the Visual Studio Code codebase for an example of the rough functionality being targeted: https://github.com/microsoft/vscode/tree/main

- [x] Panels that should use this mechanism include:
  - [x] Directory Browser panel
  - [x] Curve Data panel
    - [x] The Curve Graph panel should occupy the main content area.
    - [x] The Curve Graph panel should not participate in the "collapse any other expanded panels" behavior by default.
    - [x] The Curve Graph panel not be represented in the icon bar. Presumably this is by setting a property like `EnableIcon = false` on the panel descriptor.

- [x] All window panels within the CurveEditor application should have a header at the top of each panel. This panel header should contain the name of the panel, using terminology defined in 00-terms-and-definitions.

- [x] Collapsed panels should be fully hidden from view (including any per-panel header area), except for their representation in the Panel Bar.
- [x] Phase 3.0 uses text labels (not icons or glyphs) for Panel Bar items.
- [x] Panel Bar text labels must use the following text (exactly):
  - [x] Motor Properties = "Properties"
- [x] Panel Bar button backgrounds should be highlighted when the corresponding panel is expanded.
- [x] Panel Bar button backgrounds should not be highlighted when the corresponding panel is collapsed.
- [x] Panel Bar button backgrounds should be independent of each other (each Panel Bar Button background highlights based only on its own panel expanded/collapsed state).
- [x] When multiple panels are expanded across multiple zones (e.g., Left + Right), the Panel Bar must show highlighted backgrounds for all expanded panels at the same time.
- [x] Panel Bar text must be oriented sideways (rotated) so it fits without wrapping.
- [x] This vertical bar should be docked to the left side of the application window by default, but users should be able to change its position to the right side via user settings.
- [x] When the Panel Bar dock side is changed, the Panel Bar actually moves to that side in the layout (and does not merely persist the setting).
- [x] Changing the Panel Bar dock side should not change panel zones. Panel zones are independent of Panel Bar position.
- [x] When docked left or right, the separator/border is drawn on the edge between the Panel Bar and the main content.
- [x] Expanding a panel must not collapse panels in other zones.
- [x] If the clicked panel is already expanded, clicking its label should collapse it.
- [x] The size of the vertical bar should be fixed, and should not change when panels are expanded or collapsed.
  
### Overall Window Layout
- [x] The main window layout should consist of "zones" that correspond to the target areas for panels (e.g., left, right, bottom, center).
- [x] Collapsible panels should have a zone property that defines which zone of the window they dock to when expanded (e.g., left, right, bottom, center).
- [x] This zone property should persist across application restarts.
- [x] The application should apply the persisted zone at runtime by routing each panel's content to the appropriate zone host.
- [x] When a panel is expanded into a zone, it should collapse any other expanded panels in that zone.
- [x] Curve Data panel must be located in the left zone for Phase 3.0 (not the bottom zone).
- [x] When a panel is collapsed from a zone, the zone should adjust to minimize unused space.

- [x] The expand/collapse state of each panel should persist across application restarts. If a panel is expanded when the application is closed, it should be expanded when the application is reopened.
- [x] Each expand/collapse panel should have persisted size values that survive restarts:
  - [x] Persisted expanded width (when docked left/right).
- [x] Panels within the same zone share a single persisted zone size (e.g., Left zone width). Switching which panel is expanded within a zone must not move the zone edge.
- [x] Users should be able to resize expanded panels via splitters (right edge for left zone, left edge for right zone, top edge for bottom zone).
- [x] When a zone has no expanded panel, that zone's resize splitter must be disabled (it should not resize an empty zone).
- [x] When a zone has an expanded panel, that zone's resize splitter must be enabled and resize the zone normally.
- [x] Defaults (first run / no persisted state):
  - [x] Directory Browser defaults to expanded.
  - [x] Curve Data defaults to collapsed.
- [x] Phase 3.1 override: once the Directory Browser feature is implemented, the startup default for the Directory Browser panel is controlled by the Phase 3.1 requirements (including "starts collapsed" when no prior state is restored).
- [x] The expand/collapse mechanism should be implemented in a way that allows for easy addition of new panels in the future.
- [x] The expand/collapse mechanism should be responsive and should not cause any noticeable lag or delay when expanding or collapsing panels.
- [x] Phase 3.0 does not require smooth animations for expand/collapse.
- [x] Persistence load/parse failures should be logged (once per failure) and recovered with safe defaults.

### Acceptance Criteria (Phase 3.0)

- [x] AC 3.0.1: After restarting the application, all panel visibility and widths match the last session for the same user profile/settings file.
- [x] AC 3.0.2: Expanding or collapsing any supported panel via the Panel Bar completes within a reasonable time on a typical development machine.
- [x] AC 3.0.3: Adding a new panel type requires only minimal configuration (e.g., registering a name, icon, and content) without changes to core layout logic.
- [x] AC 3.0.4: Layout changes (panel expand/collapse, width changes) do not participate in the undo/redo history.

- [x] AC 3.0.12: When multiple panels share a zone (e.g., Directory Browser and Curve Data in the Left zone), the zone width is persisted per-zone and remains stable when switching which panel is expanded.



## Phase 3.0.5 Functional Requirements: VS Code UI Styling Pass

### Scope (Phase 3.0.5)

- Adjust CurveEditor UI colors and styling so it matches the screenshot: [.github/planning/vs-code-ui.png](.github/planning/vs-code-ui.png).
- Focus on main window chrome:
  - Panel Bar (VS Code Activity Bar equivalent)
  - Side panels (VS Code Side Bar equivalent)
  - Panel headers/section headers
  - Splitters/separators/borders

### Non-goals (Phase 3.0.5)

- Do not change panel expand/collapse behavior or persistence behavior.
- Do not redesign the content inside panels.
- Do not introduce new UI features, animations, or navigation.

### Requirements

- [ ] The application should centralize VS Code-like colors into a single resource dictionary (CurveEditor-specific theme tokens) and reference them via `DynamicResource`.
- [ ] The Panel Bar (Activity Bar) background and border should match the screenshot.
- [ ] Panel Bar button backgrounds:
  - [ ] Are highlighted when the corresponding panel is expanded.
  - [ ] Are not highlighted when the corresponding panel is collapsed.
  - [ ] Are independent per item.
  - [ ] Support multiple highlighted buttons simultaneously when multiple zones have expanded panels.
- [ ] Side panel backgrounds and borders should match the screenshot.
- [ ] Panel headers/section headers should match the screenshot.
- [ ] Splitters/separators should match the screenshot (subtle VS Code-like separators).
- [ ] Styling must work correctly when the Panel Bar is docked left or right.

### Acceptance Criteria (Phase 3.0.5)

- AC 3.0.5.1: Panel Bar visuals (background, hover, active background) match the screenshot closely.
- AC 3.0.5.1a: If multiple zones have expanded panels, all corresponding Panel Bar buttons show active background highlighting simultaneously.
- AC 3.0.5.2: Side panel visuals (background, header styling, borders) match the screenshot closely.
- AC 3.0.5.3: Styling changes do not alter panel behavior, persistence, or undo/redo.



## Phase 3.1 Functional Requirements: Directory Browser

### Directory Browser: General

### Directory Browser: file browser behavior

- [x] Only show folders and valid curve definition files in the directory listing.
  - [x] To make this efficient, make an initial file list containing only directories and JSON files, then validate each file in a background task to filter out invalid files.

- [x] Browser directory listing and navigation should work like VS Code's file browser.
  - [x] Show directories in a tree view on the left side, allowing navigation into subdirectories via expansion/collapse.
  - [x] Directories should be expandable/collapsible via caret icons to the left of the directory name.
  - [x] Clicking on a directory expansion icon will expand or collapse the directory.
  - [x] NEW: When a folder is collapsed, the icon should change to chevron right. 
    - [ ] Chevron Right (Fluent Icons): <svg width="24" height="24" fill="none" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path d="M8.293 4.293a1 1 0 0 0 0 1.414L14.586 12l-6.293 6.293a1 1 0 1 0 1.414 1.414l7-7a1 1 0 0 0 0-1.414l-7-7a1 1 0 0 0-1.414 0Z" fill="#ffffff"/></svg>
  - [x] New: When a folder is expanded, change the icon to chevron down.
    - [ ] Chevron Down (Fluent Icons): <svg width="24" height="24" fill="none" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path d="M4.293 8.293a1 1 0 0 1 1.414 0L12 14.586l6.293-6.293a1 1 0 1 1 1.414 1.414l-7 7a1 1 0 0 1-1.414 0l-7-7a1 1 0 0 1 0-1.414Z" fill="#ffffff"/></svg> 
  - [x] Directories should be sorted alphabetically, with folders listed before files.
  - [x] Show valid curve definition files in the selected directory in the tree (single unified tree), not in a separate pane.
  - [x] Single clicking on a file will open it in the curve editor.
  - [x] REGRESSION: If a user tries to open a file and the current file is dirty, ask the user if they want to save the existing file first?.

  - [x] Clicking on a directory name will expand/collapse it, rather than selecting it.

  - [x] Directories and files should be displayed in the same tree; remove the two-pane view. It should look roughly like VS Code's file explorer pane.

  - [X] Files and directories within a parent directory should be shown as children of that directory in the tree view. They should be indented to indicate they are children.
  
  - [x] BUG: The browser file/directory listing should look like this:

```text
top directory
 > directory 1
 > directory 2
 > directory 3
 V motor profiles
     motor profile 1.json
     motor profile 2.json
 > directory 4
   motor profile 3.json
   motor profile 4.json

```
Note: In this example, you'll notice that `motor profile 1.json` and `motor profile 2.json` are inside the `motor profiles` directory, while `motor profile 3.json` and `motor profile 4.json` are inside `top directory`. Note that `directory 4` is not expanded, so it's clear that those last two files are not in that directory. The tree structure allows for easy navigation through directories and files.

- [x] The top level directory in the directory browser should not participate in the expand/collapse mechanism. It should always be expanded and does not have a caret icon.

- [x] New: the top level directory in the directory browser should NOT show its full path; only show the directory name.

### Directory Browser: Behavior

- [x] When the program starts, automatically open the last opened file in the curve editor.
- [x]  When the user opens a directory, automatically expand the directory tree to show the opened directory.
- [x] When the user opens a directory and the directory browser panel is collapsed, the panel should automatically open.
- [x] Implement a "Close Directory" button to close the currently opened directory in the directory browser.
- [x] New: Keep the existing "Open Folder" menu item in the File menu (not the Directory Browser panel header).
- [x] When the command "Close Directory" is executed, the directory tree does not collapse; the opened root directory is cleared (tree becomes empty).
- [x] When the program starts, automatically open the last opened directory in the directory browser, unless the user had explicitly closed it before exiting the program.
- [x] When the program starts, remember the expanded/collapsed state of directories in the directory browser from the last session.
- [x] When the program starts, if the last opened directory no longer exists, collapse the directory browser.
- [x] Directory browser width should persist in user settings.

### Acceptance Criteria (Phase 3.1)

- AC 3.1a: On restart, if the last opened directory still exists, its expand/collapse state and the directory browser width are restored.
- AC 3.1b: If the last opened directory no longer exists, the directory browser starts collapsed with no errors shown to the user beyond any appropriate log entry.
- AC 3.1c: Clicking a file once in the directory browser always opens it in the CurveEditor, and the selection in the tree matches the active motor definition.
- AC 3.1d: The directory browser file/directory listing matches the specified tree structure and behavior described above.
- [x] AC 3.1e: If the user opens a directory while the directory browser panel is collapsed, the panel automatically expands to show the directory tree.
- [x] AC 3.1f: The "Close Folder" menu item is located in the File menu, not Directory Browser panel header.
- [x] AC 3.1g: The "Open Folder" menu item is located in the File Menu, not the Directory Browser panel header menu.
- [x] AC 3.1h: When the user executes "Close Directory", the directory tree does not collapse.
- [x] AC 3.1i: When the user opens a json file while the current file is dirty, the user is prompted to save the current file first.


### Directory Browser: UI
- [x] Ensure there is an Open Folder button added to the File menu.
- [x] Ensure there is an Close Folder button added to the File menu for easy access.
- [x] Add a "Refresh Explorer" icon button at the top of the file browser to re-scan the directory tree for files.
- [x] Implement a keyboard shortcut (F5) to trigger "Refresh Explorer".


### Directory Browser: Text display
- [x] The text within the directory browser should use a monospace font for better alignment.
- [x] The text size within the directory browser should persist in user settings.
- [x] The text size within the directory browser should be adjustable via keyboard shortcuts (e.g., Ctrl+Plus to increase, Ctrl+Minus to decrease).
- [x] The text size within the directory browser should be adjustable via mouse wheel while holding the Ctrl key.
  - [x] Ctrl+Mouse Wheel Up increases text size.
  - [x] Ctrl+Mouse Wheel Down decreases text size.
- [x] The text within the directory browser should not wrap; long file and directory names should be truncated with ellipses if they exceed the available width.


## Phase 3.1.5 Functional Requirements: Json Schema and Data Model Update

### Scope (Phase 3.1.5)

- Reduce JSON motor definition file size and improve manual edit ergonomics by introducing a table/map representation for curve series data.
- Preserve explicit `percent` and `rpm` values in the persisted file format.
- Keep the motor definition schema version at `1.0.0` (no schema version bump).
- Add new motor properties for brake release time, backlash, and some new units of measure.

### Non-goals (Phase 3.1.5)

- Do not change how curves are edited in the UI beyond what is required to load/save the updated format.
- Do not change curve interpolation or curve generation logic.
- Do not add compression (zip/gzip) as the primary solution.

### Json Format: General

- [ ] Update the schema, json samples, object models, and documentation to support the json file changes listed below.

### Json Format: Series Table/Map

- [ ] Each voltage configuration should support persisting curve data using a table/map representation (the “series table” format).
- [ ] The series table format should store:
  - [ ] A shared `percent` axis array (0–100) for all series.
  - [ ] A shared `rpm` axis array aligned 1:1 with `percent`.
  - [ ] A `series` map keyed by series name (e.g., `Peak`, `Continuous`), where each entry contains:
    - [ ] `locked` (boolean)
    - [ ] `notes` (string, optional)
    - [ ] `torque` array aligned by index with the shared axes.
- [ ] The series table format must support any number of series (including one) and must allow user-added series.

### Json Format: New Properties

- [ ] Add new properties to the motor properties:
  - [ ] `brakeReleaseTime` (number) indicating the motor brake release time unit of measure.
  - [ ] `brakeEngageTimeDiode` (number) indicating the motor brake engage time when using a diode.
  - [ ] `brakeEngageTimeMOV` (number) indicating the motor brake engage time when using an MOV.
  - [ ] `brakeBacklash` (number) indicating a angular displacement.

- [ ] Add new properties to the units section:
  - [ ] `responseTime` (string) indicating the motor brake release time unit of measure. Default is milliseconds.
  - [ ] `backlash` (string) indicating a angular displacement. Default is arcmin.
    - [ ] `percentage` (string) indicates a percentage number. Default is %.
    - [ ] `inertia` (string): default is "kg-m^2",
    - [ ] `temperature` (string): default is "C",
    - [ ] `torqueConstant` (string): "Nm/A",

### Json Format: Change Properties

- [ ] Change `drives` array property:
  - [ ] `name` (string) should be moved to the top of the drive object.
  - [ ] `manufacturer` should be moved to the second position in the drive object.
  - [ ] `partNumber` should be moved to the third position in the drive object.

### Schema Versioning

- [ ] The motor definition schema version remains `1.0.0`.
- [ ] The schema and file format should define a single representation for curve series data: the series table/map representation.
- [ ] Add the new properties to the schema with appropriate types and default values.
- [ ] When saving motor definition files, CurveEditor should write the series table/map representation.

### Validation Rules

- [ ] The shared `percent` axis must be strictly increasing and contain 0–101 entries.
  - [ ] Values must be integers and non-negative.
  - [ ] Values above 100 may be used to represent overspeed (manual JSON editing; editor may be view-only).
  - [ ] Standard curves use 101 points (0%..100% in 1% increments).
- [ ] The shared `rpm` axis must:
  - [ ] Contain the same number of entries as `percent` (0–101).
  - [ ] Be non-negative.
  - [ ] Be monotonically non-decreasing.
- [ ] Each series `torque` array must contain the same number of entries as the shared axes (0–101).
- [ ] All arrays must remain index-aligned (`percent[i]`, `rpm[i]`, and each series `torque[i]` refer to the same point).
- [ ] Load failures (schema mismatch, invalid lengths, invalid values) should be logged and handled per the project’s logging/error handling policy.

### Data Model Adjustments

- [ ] Update the in-memory data model so it can:
  - [ ] Deserialize the series table/map representation.
  - [ ] Deserialize the new motor properties.
  - [ ] Serialize the series table/map representation.
  - [ ] Serialize the new motor properties.
  - [ ] Normalize internally to a single representation suitable for the rest of the application (recommended: keep existing `CurveSeries` + `DataPoint` as the runtime model and convert on load/save).
- [ ] Implement conversion logic:
  - [ ] Series table/map  runtime `CurveSeries`/`DataPoint` (lossless conversion).

### Implementation Guidance (Phase 3.1.5)

- Keep persistence-facing DTOs/mapping/validation code isolated so it can be moved into the Phase 3.1.6 client library with minimal refactoring (recommended: `jordanrobot.MotorDefinitions.*` namespaces).

### Acceptance Criteria (Phase 3.1.5)

- [ ] AC 3.1.5a: CurveEditor saves motor definition files with `schemaVersion` set to `1.0.0` and persists curve data using the series table/map representation.
- [ ] AC 3.1.5b: CurveEditor can load and save the series table/map format, and reopening a saved file yields identical curve data (percent/rpm/torque) and series metadata.
- [ ] AC 3.1.5c: For a representative file with multiple voltages and at least two series per voltage, the saved JSON file is measurably smaller than the current object-per-point representation used before this change (track size reduction in a simple benchmark/test artifact).
- [ ] AC 3.1.5d: The new motor properties are correctly loaded and saved, and their default values are applied when loading files that omit them.

### Deferred (Phase 3.1.5)

- Duplicate series names and series-name uniqueness policy is deferred to a later phase. Phase 3.1.5 treats `series` keys as case-sensitive and does not define behavior for duplicates beyond what JSON objects inherently allow.


## Phase 3.1.6 Functional Requirements: .NET Client Library Skeleton Project

### Scope (Phase 3.1.6)

- Create a new SDK-style .NET class library project in this repo that is intended to become the reusable client library for the motor definition file format.
- Establish a clean separation boundary so the library has no dependency on Avalonia UI or CurveEditor application services.

### Non-goals (Phase 3.1.6)

- Do not publish a NuGet package in this phase.
- Do not require any breaking refactors of CurveEditor UI code in this phase.

### Requirements

- [ ] Add a new class library project `jordanrobot.MotorDefinition` to the solution.
- [ ] The library project must not reference Avalonia or MotorEditor UI assemblies.
- [ ] The library project should contain (initially):
  - [ ] Schema-aligned models (or rehomed shared models) that represent the motor definition file.
  - [ ] A single entrypoint for file IO (e.g., `MotorFile` helpers or `MotorFileSerializer`).
  - [ ] A copy of the Schema.
  - [ ] Rehome/move the Phase 3.1.5 persistence DTOs + mapper + validators into the library with minimal churn (ideally a file move and minor namespace/project-reference updates).
  - [ ] Unit test as needed to verify functionality.

- AFTER EACH OF THESE YOU MUST VERIFY BUILD BEFORE MOVING TO THE NEXT STEP:
- [ ] Rename the program CurveEditor to MotorEditor.
- [ ] Rename the project CurveEditor to MotorEditor.Avalonia.
- [ ] Move the curve editor application codebase to a `src/MotorEditor.Avalonia/` directory.
- [ ] Update the MotorEditor.Avalonia project to reference the new library project for motor definition file IO.
 
  
### Acceptance Criteria (Phase 3.1.6)

- AC 3.1.6a: `dotnet build` succeeds for the solution with the new library project added.
- AC 3.1.6b: The library project has zero dependencies on UI frameworks.


## Phase 3.1.7 Functional Requirements: NuGet Package Skeleton

### Scope (Phase 3.1.7)

- Prepare the library project for NuGet packaging without necessarily publishing it publicly.

### Non-goals (Phase 3.1.7)

- Do not automate publishing to nuget.org yet (unless separately requested).

### Requirements

- [ ] Configure package metadata in the library project (`PackageId`, `Version`, `Authors`, `Description`, `RepositoryUrl`).
- [ ] Enable XML documentation output.
- [ ] Add a minimal README with consumer usage examples for loading + validation.
- [ ] Add a minimal test project verifying basic round-trip save/load for the current format.

### Acceptance Criteria (Phase 3.1.7)

- AC 3.1.7a: `dotnet pack` produces a `.nupkg` locally.
- AC 3.1.7b: The package can be referenced by a sample app and successfully loads a motor definition file.


## Phase 3.1.8 Functional Requirements: .NET Client Library and NuGet Package (Consumer Validation)

### Scope (Phase 3.1.8)

- Provide a high-quality consumer experience for loading and validating motor definition JSON files.
- Consumers should not need to write custom schema validators or semantic validators.

### Non-goals (Phase 3.1.8)

- Do not couple the library to Avalonia UI types, CurveEditor ViewModels, or app-specific services.
- Do not require consumers to reference CurveEditor itself.

### Requirements

- [ ] The client library should provide:
  - [ ] Public POCO models aligned with the motor definition schema.
  - [ ] System.Text.Json serialization/deserialization helpers with consistent defaults.
  - [ ] Parsing for the current motor definition file format (schema `1.0.0` with series table/map curve data).
  - [ ] Built-in validation that checks schema compliance and semantic rules (axes length/alignment, monotonicity, required fields).
  - [ ] A non-throwing load API that returns a result object (success + errors) suitable for UI and tooling scenarios.
  - [ ] A throwing load API that throws a single exception containing a structured list of validation errors.
  - [ ] A structured validation error model that includes (at minimum):
    - [ ] A JSON path/pointer to the failing node (for easy user messaging and debugging).
    - [ ] A stable error code/category (e.g., `InvalidJson`, `SchemaViolation`, `SemanticViolation`).
    - [ ] A human-readable error message.
  - [ ] The schema should be embedded in the package (as an internal resource) so consumers do not need to ship a separate schema file.
- [ ] The NuGet package should:
  - [ ] Use semantic versioning aligned with the schema versioning strategy.
  - [ ] Target consumer-friendly frameworks (recommendation to validate: `netstandard2.0` + `net8.0`).
  - [ ] Include XML docs and a minimal README with usage examples.
  - [ ] Include unit tests in the repo verifying round-trip behavior and validation behavior.

### Acceptance Criteria (Phase 3.1.8)

- AC 3.1.8a: A small console app can reference the package and load/save a motor definition file without referencing CurveEditor UI assemblies.
- AC 3.1.8b: When given invalid JSON, the non-throwing load API returns a failure result with an `InvalidJson` error that includes location information.
- AC 3.1.8c: When given schema-invalid JSON, the non-throwing load API returns a failure result with one or more `SchemaViolation` errors that include JSON paths.
- AC 3.1.8d: When given schema-valid but semantically invalid JSON (e.g., wrong axis length), the non-throwing load API returns a failure result with one or more `SemanticViolation` errors.
- AC 3.1.8e: The throwing load API throws a single exception type containing the full error list.
- AC 3.1.8f: The package successfully round-trips the current format files without data loss.
- AC 3.1.8g: Package API surface is stable, documented, and does not expose internal CurveEditor-specific concepts.

## Phase 3.2 Functional Requirements: Curve Data Panel

These requirements will be added in a future update.

### Curve Data Panel: Layout

- [x] Curve Data Grid should occupy the entirety of the Curve Data Panel, and resize as the panel is resized.

### Open Questions (Phase 3.2)

- Q 3.2a: Which Curve Data Panel behaviors, if any, are planned for Phase 3.2 versus later phases (e.g., advanced editing vs. read-only enhancements)?