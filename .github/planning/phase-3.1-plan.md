## Curve Editor Phase 3.1 Plan: Directory Browser (VS Code-style)

### Status

Complete

**Related ADRs**

- ADR-0004 Layout and Panel Persistence Strategy (`docs/adr/adr-0004-layout-and-panel-persistence.md`)
- ADR-0005 Keyboard Shortcuts and Input Routing Policy (`docs/adr/adr-0005-keyboard-shortcuts-and-input-routing.md`)
- ADR-0006 Motor File Schema and Versioning Strategy (`docs/adr/adr-0006-motor-file-schema-and-versioning.md`)
- ADR-0009 Logging and Error Handling Policy (`docs/adr/adr-0009-logging-and-error-handling-policy.md`)

### Goal

- Replace the Directory Browser placeholder with a VS Code–style explorer tree that opens motor JSON files by single-click, persists its state, and restores the last session.

### Scope

In scope
- [x] VS Code–style explorer tree (folders + `*.json` files) with folders-first sorting, lazy expansion, and single-click open.
- [x] Persistence of last opened directory, expanded state, selected file, and explorer font size.
- [x] Restore last session on startup (with safe behavior when the directory is missing).
- [x] Follow-up fixes from updated requirements (Dec 2025): chevron icons, root display name (directory name only), dirty prompt on open, auto-expand Browser panel on Open Folder when collapsed, file-menu placement tweaks.
- [x] Filter explorer listing to folders + valid motor definition JSON files (background validation removes invalid candidates).

Out of scope (Phase 3.1)
- [ ] Explorer dirty indicators.
- [ ] Exit/open save prompts outside of the explorer-initiated open prompt.
- [ ] Multi-root workspaces, tabs, or “open in new tab”.

### Assumptions and Constraints

- **AC precedence**: when bullets conflict with acceptance criteria, acceptance criteria win.
- **No startup prompts**: do not show modal prompts during startup restore; prompts only occur on explicit user actions.
- **No new UX beyond requirements**: do not add extra menus, views, or navigation.
- **Windows path handling**: root display name logic must handle standard directories and drive-root paths.

### Current Baseline (What exists today)

Relevant files/components
- `src/CurveEditor/Views/DirectoryBrowserPanel.axaml` and `src/CurveEditor/Views/DirectoryBrowserPanel.axaml.cs`
- `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs`
- `src/CurveEditor/ViewModels/ExplorerNodeViewModel.cs`
- `src/CurveEditor/Services/IDirectoryBrowserService.cs` and `src/CurveEditor/Services/DirectoryBrowserService.cs`
- `src/CurveEditor/ViewModels/MainWindowViewModel.cs`
- `src/CurveEditor/Views/MainWindow.axaml` and `src/CurveEditor/Views/MainWindow.axaml.cs`
- Persistence via `src/CurveEditor/Behaviors/PanelLayoutPersistence.cs` (wrapped for testability)

### 1. Directory Browser (Phase 3.1)

- **Goal**: Replace the current "Coming Soon" Directory Browser placeholder with a VS Code–style explorer tree that can open motor JSON files by single-click, persists its state, and restores the last session.
- **Context**: Phase 3 in `04-mvp-roadmap.md` calls out file management (Directory browser, dirty indicators, save prompts). The panel expand/collapse system (Phase 3.0) is already implemented and the Directory Browser panel is registered (left zone), but its content is still stubbed.

#### 1.1 Current baseline (codebase)

- Panel system is implemented via `PanelRegistry` + `PanelBar`, and left-zone width persistence already exists (zone-based) in `MainWindow.axaml.cs`.
- Directory Browser panel is present in the left zone, but its content is a placeholder in `MainWindow.axaml`.
- File open/save is implemented via `MainWindowViewModel.OpenFileAsync()` and `IFileService.LoadAsync()`.
- Domain validation rules exist in `IValidationService.ValidateMotorDefinition()` and the JSON schema exists under `schema/`; these will be used in a future phase to validate/badge/filter explorer files.

#### 1.2 Functional requirements coverage (Phase 3.1)

File filtering + validation

- [x] Show folders and JSON files in the directory listing.
- [x] Validate candidate files in the background and filter out invalid motor definition files.

Decision (implementation)
- For Phase 3.1 implementation, we filter out obviously non-motor JSON files after initial listing.
- Validation is intentionally lightweight and runs in the background: if a JSON file does not deserialize into a plausible motor definition shape (non-empty motor name and at least one drive/voltage/series), it is removed from the tree.

Tree behavior (VS Code-like)
- [x] Use a single unified tree (folders + files in the same tree).
- [x] Folders expand/collapse via caret icon.
- [x] Clicking the caret toggles expand/collapse.
- [x] Clicking the folder name toggles expand/collapse (not selection).
- [x] Single-clicking a file opens it in the editor.
- [x] Sort alphabetically with folders before files.
- [x] Top-level directory node is always expanded and has no caret.
- [ ] Show Fluent chevron icons for folders:
  - [ ] Collapsed folder shows chevron right.
  - [ ] Expanded folder shows chevron down.
- [ ] Top-level directory node display name shows directory name only (not full path).

Session behavior
- [x] On startup, automatically open the last opened file.
- [x] On startup, automatically open the last opened directory unless it was explicitly closed.
- [x] Persist expanded/collapsed state of directories in the tree.
- [x] If the last opened directory no longer exists, start with Directory Browser collapsed and only log a message.
- [x] Directory browser width persists (already satisfied by left-zone width persistence).

Dirty/open behavior
- [ ] If a user opens a file while the current file is dirty, prompt to save before replacing it (Phase 3.1 regression item).

UI + input
- [x] Add "Open Folder" to the File menu.
- [x] Add "Close Folder" button to the main toolbar.
- [x] Add a "Refresh Explorer" **unicode glyph** button at the top of the Directory Browser panel.
- [x] Add `F5` shortcut to trigger refresh.
- [ ] Add "Close Folder" to the File menu (AC 3.1.6).
- [ ] Ensure "Open Folder" remains in the File menu (AC 3.1.7).
- [ ] If opening a folder while Directory Browser is collapsed, automatically expand the Directory Browser panel (AC 3.1.5).

Text display
- [x] Use monospace font for explorer tree items.
- [x] Persist explorer text size.
- [x] Support `Ctrl` + `+` / `Ctrl` + `-` to change explorer text size.
- [x] Support `Ctrl` + mouse wheel to change explorer text size.
- [x] Prevent wrapping; long names truncate with ellipses.

#### 1.3 New/changed requirements (Dec 2025)

The Phase 3.1 requirements file has been updated with new items, regressions, and additional acceptance criteria.
This plan treats the following as remaining work for Phase 3.1 follow-up.

New acceptance criteria
- AC 3.1.4: Explorer tree structure and behavior matches the specified example (including icon behavior).
- AC 3.1.5: Opening a directory auto-expands the Directory Browser panel if it was collapsed.
- AC 3.1.6: "Close Folder" menu item is located in the File menu (in addition to any toolbar button).
- AC 3.1.7: "Open Folder" menu item remains in the File menu (not in a panel header menu).
- AC 3.1.9: Opening a JSON file while the current file is dirty prompts the user to save first.

Known requirement conflicts to resolve
- Close Directory behavior conflict:
  - Requirement bullet: "When the command 'Close Directory' is executed, collapse the directory tree to hide the closed directory."
  - AC 3.1.8: "When the user executes 'Close Directory', the directory tree does not collapse."
  - Proposed interpretation for implementation planning: keep the Directory Browser panel expanded/collapsed state unchanged, but clear the opened root directory (tree becomes empty). This matches the prior intent of "close directory" while avoiding collapsing the entire left zone.
  - If AC 3.1.8 is intended literally (tree must remain visible), we will need a different UX (e.g., keep a non-collapsible root placeholder that shows the last folder name but with no children).

- Open Folder placement conflict:
  - Requirement bullet: "Move the existing 'Open Folder' menu item ... to the Directory Browser panel header."
  - AC 3.1.7: "Open Folder" must be in the File menu, not the panel header menu.
  - Plan decision: follow AC 3.1.7 and keep Open Folder in the File menu.

### 2. Proposed architecture

#### 2.1 New/updated components

- `DirectoryBrowserPanel` (new View): user control that hosts the explorer header (path + buttons) and the tree.
- `DirectoryBrowserViewModel` (new VM): owns the root directory state, node tree, selection, and commands (open folder, close folder, refresh, change font size, open file).
- `IDirectoryBrowserService` (new Service) + implementation:
  - Enumerate directory contents (folders + `*.json` candidates).
  - Provide cancellation support so rapid refresh or folder switching doesn’t leak work.
  - (Future) Validation pipeline (schema + domain) can be added later without changing the UI tree model.

Folder picker abstraction (testability)
- Introduce a small abstraction for choosing a folder (e.g., `IFolderPicker`), so we can unit test the Open/Close/Restore logic without depending on Avalonia `IStorageProvider`.

Integration points
- `MainWindowViewModel`:
  - Owns the application-level file open behavior.
  - Exposes methods the explorer can call to open a file path (reusing existing load/dirty/undo reset patterns).
  - Exposes an observable `CurrentFilePath` surface (or equivalent event) so the explorer can keep selection synchronized with the active file.
- `MainWindow.axaml`:
  - Replace the placeholder Directory Browser panel content with `DirectoryBrowserPanel`.
  - Add menu/toolbar items and keybindings.

#### 2.2 Explorer node model (ViewModel-side)

Represent explorer nodes as view models (so the tree is driven by observable state and can be tested without UI):

- `ExplorerNodeViewModel`
  - `string DisplayName`
  - `string FullPath`
  - `bool IsDirectory`
  - `bool IsRoot`
  - `bool IsExpanded`
  - `ObservableCollection<ExplorerNodeViewModel> Children`
  - `bool IsLoadingChildren`
  - `bool IsValidMotorFile` (only meaningful for files)

Loading strategy
- Directories are loaded lazily (children populated when expanding) to keep large trees responsive.
- Files are shown as `*.json` candidates (no filtering in Phase 3.1).
- The root node is a special case:
  - Always expanded.
  - No caret.
  - Should not be collapsible by user input.

UI-thread safety
- Background directory enumeration must not mutate `ObservableCollection<T>` from background threads.
- Services should return immutable snapshots/DTOs; the view model should apply them on the UI thread (dispatcher).

#### 2.3 File validation strategy

Deferred validation (future phase)
- Phase 3.1 will not filter or badge invalid files.
- Future implementation should prefer **JSON schema validation** (per `schema/motor-schema-v1.0.0.json`) as the first pass, and only then optionally run domain validation.

Addendum (post-Phase 3.1)
- The current implementation in `DirectoryBrowserViewModel` performs a lightweight validation by deserializing `MotorDefinition` and checking `HasValidConfiguration()`.
- Phase 3.1.5 changes the on-disk JSON shape (series table/map) while keeping the runtime model unchanged, so this deserialization-based validator will become incorrect and must be updated.
- Short-term (Phase 3.1.5): update the explorer filter to a `JsonDocument`-based shape probe that is cheap and does not depend on runtime model serialization.
- Long-term (Phase 3.1.6+): replace the shape probe with schema-based validation provided by the planned client library (schema + validator), so the explorer validator automatically stays aligned with schema evolution.
- Do not use `IFileService.LoadAsync()` as a validator because it mutates `CurrentFilePath` and dirty state.

### 3. Persistence plan (user settings)

Phase 3.0 uses `PanelLayoutPersistence` for user settings. Phase 3.1 should continue using it to avoid introducing a second settings store.

Proposed settings keys (all persisted via `PanelLayoutPersistence.SaveString/AttachBoolean/LoadWidth` etc.)

- Directory session state
  - `DirectoryBrowser.LastOpenedDirectory` (string)
  - `DirectoryBrowser.WasExplicitlyClosed` (bool)
  - `DirectoryBrowser.ExpandedDirectoryPaths` (string containing JSON array of relative paths)

- File session state
  - `File.LastOpenedMotorFile` (string)

- Explorer text settings
  - `DirectoryBrowser.FontSize` (string containing a number)

Notes
- Avoid turning `PanelLayoutPersistence` into a general settings system.
  - Prefer encoding Phase 3.1 values into the existing `StringValue` field (e.g., `"true"/"false"`, numeric strings, and JSON arrays as strings) with narrow helper methods.
- Debounce high-frequency writes (font size changes and expand/collapse toggles) to avoid write amplification.

### 4. Startup + restore flow

Phase 3.1 startup should run after the window is ready (e.g., in `MainWindow.OnOpened`).

Restore order (recommended)
1. Load persisted explorer state.
2. If `DirectoryBrowser.WasExplicitlyClosed == true`, do not auto-open a directory.
3. Else if `DirectoryBrowser.LastOpenedDirectory` exists on disk, open it in explorer.
4. Else (missing directory):
  - Ensure the Directory Browser is not the active left panel (do not collapse the entire left zone).
    - If the active left panel is Directory Browser, switch to another left-zone panel (e.g., Curve Data) or leave the prior selection intact if available.
   - Log once (info or warning) and continue.
5. If `File.LastOpenedMotorFile` exists on disk:
   - Load it into the editor.
   - If the file is under the opened directory root, expand the directory chain and select the file node.

### 5. UI plan

#### 5.1 Directory Browser panel content

Replace the placeholder in the Directory Browser panel with:
- Top row: current directory path (non-wrapping, ellipsized) + "Refresh Explorer" unicode glyph button.
- Main area: TreeView with folders and `*.json` files.

Note
- In Phase 3.1, the TreeView shows folders + `*.json` files (no validity filtering). The header copy should not imply filtering.

Tree rendering requirements
- Monospace font via a theme resource (e.g., `CurveEditor.Fonts.Monospace`) so we don’t hard-code font families.
- `TextWrapping="NoWrap"` and `TextTrimming="CharacterEllipsis"` for node text.
- Folder caret behavior:
  - Root node: no caret.
  - Folder nodes: caret present; toggles `IsExpanded`.
- Input routing:
  - Folder name click toggles expansion and prevents selection.
  - File click selects the node and triggers open.

#### 5.2 Commands + menus + shortcuts

- File menu
  - [x] Add "Open Folder..." -> `OpenFolderCommand`.

- Toolbar
  - [x] Introduce a minimal top toolbar and add "Close Folder" -> `CloseFolderCommand`.

- Keyboard
  - [x] Add `F5` -> `RefreshExplorerCommand`.
  - [x] Add `Ctrl`+`+` and `Ctrl`+`-` -> increase/decrease explorer font size.
    - Implementation note: in practice, bind both OEM and numpad variants.

- Mouse
  - [x] In the Directory Browser panel, handle `Ctrl` + mouse wheel to adjust font size.

### 6. Implementation steps (Phase 3.1)

#### 6.1 Directory browser foundation
- [x] Create `DirectoryBrowserViewModel` with:
  - Root directory path
  - Node tree
  - Commands: open folder, close folder, refresh, open file
  - `FontSize` and increment/decrement helpers
- [x] Create `DirectoryBrowserPanel` view and wire it into the Directory Browser panel content.
- [x] Replace the placeholder UI in `MainWindow.axaml` with the new panel control.

Completed via [src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs](src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs) and [src/CurveEditor/Views/DirectoryBrowserPanel.axaml](src/CurveEditor/Views/DirectoryBrowserPanel.axaml).

#### 6.2 Directory scanning + validation
- [x] Implement `IDirectoryBrowserService` for:
  - Enumerating folders + `*.json` candidates
  - Cancellation and bounded concurrency
- [x] Ensure the service returns results in a UI-thread-safe way (pure data snapshots).

Future
- Add schema-based validation + optional file badges/filtering.

#### 6.3 Open file from explorer + selection sync
- [x] Add an "open file by path" method on `MainWindowViewModel` that:
  - Calls `_fileService.LoadAsync(filePath)`
  - Resets selection/editing coordinator as needed
  - Resets undo stack / clean checkpoint appropriately (mirrors existing open-file behavior)
- [x] When explorer opens a file, call this method.
- [x] When `MainWindowViewModel` loads a motor file (via menu or explorer), update explorer selection:
  - If file is under the current directory root: expand ancestors and select.
  - Else: clear selection.

Required enabling change
- Expose a `CurrentFilePath` (observable) on `MainWindowViewModel` so selection sync does not depend on `WindowTitle` updates.

#### 6.4 Persistence + startup restore
- [x] Persist on changes:
  - Last opened directory
  - Explicitly closed flag
  - Expanded directory paths
  - Last opened motor file
  - Explorer font size
- [x] Implement startup restore logic (see Section 4) and ensure missing directory/file is handled gracefully with a log entry.
- Directory Browser default expanded/collapsed behavior remains as-is (Phase 3.0 default), except:
  - If the last opened directory no longer exists at startup, collapse the Directory Browser panel (AC 3.1.2).

#### 6.5 UI chrome + input
- [x] Add File menu item for "Open Folder...".
- [x] Add toolbar with "Close Folder" button (minimal toolbar; do not add extra actions).
- [x] Add "Refresh Explorer" unicode glyph button to the Directory Browser header.
- [x] Add keybindings: `F5`, `Ctrl`+`+`, `Ctrl`+`-`.
- [x] Add Ctrl+mouse wheel handling in Directory Browser panel.

#### 6.6 Follow-up: Explorer visuals + menu placement + dirty prompts

Chevron visuals
- [ ] Replace the default caret toggle visuals with Fluent chevron icons:
  - [ ] Collapsed: chevron right
  - [ ] Expanded: chevron down
- [ ] Ensure the root node continues to hide the expander icon.

Root display name
- [ ] Ensure the root node shows directory name only (no full path) across:
  - [ ] Standard paths (e.g., `C:\Users\name\folder`)
  - [ ] Drive root paths (e.g., `C:\` should display as `C:`)

Open folder behavior (collapsed panel)
- [ ] If the user invokes Open Folder while Directory Browser panel is collapsed:
  - [ ] Set the active left panel to Directory Browser before scanning so the user sees the tree.

File menu placement
- [ ] Add File menu -> "Close Folder" command (do not remove toolbar button).
- [ ] Ensure there is no separate "Open Folder" menu inside the Directory Browser header.

Dirty file prompt regression
- [ ] On explorer-initiated file open (and optionally File->Open too), if the current document is dirty:
  - [ ] Prompt: Save / Ignore / Cancel
  - [ ] Save: run existing Save flow, then continue opening
  - [ ] Ignore: proceed opening without saving
  - [ ] Cancel: do not open the new file

Implementation notes
- Keep shortcut policy: keybindings remain centralized on `MainWindow.axaml` (ADR-0005).
- Avoid showing modal prompts during startup restore (only prompt on explicit user action).

### Acceptance Criteria

- [x] AC 3.1.1: On restart, if the last opened directory still exists, its expand/collapse state and the directory browser width are restored.
- [x] AC 3.1.2: If the last opened directory no longer exists, the directory browser starts collapsed with no errors shown to the user beyond any appropriate log entry.
- [x] AC 3.1.3: Clicking a file once in the directory browser always opens it in the CurveEditor, and the selection in the tree matches the active motor definition.
- [ ] AC 3.1.4: The directory browser file/directory listing matches the specified tree structure and behavior described in the requirements.
- [ ] AC 3.1.5: If the user opens a directory while the directory browser panel is collapsed, the panel automatically expands to show the directory tree.
- [ ] AC 3.1.6: The "Close Folder" menu item is located in the File menu, not Directory Browser panel header.
- [x] AC 3.1.7: The "Open Folder" menu item is located in the File menu, not the Directory Browser panel header menu.
- [ ] AC 3.1.8: When the user executes "Close Directory", the directory tree does not collapse.
- [ ] AC 3.1.9: When the user opens a json file while the current file is dirty, the user is prompted to save the current file first.

### 7. Testing strategy (Phase 3.1)

Unit tests (preferred)
- [x] `DirectoryBrowserServiceTests`
  - Enumerates folders + JSON candidates.
  - Lists `*.json` files without filtering in Phase 3.1.
  - Sort order: folders before files, alphabetical.

Future
- Add schema/domain validation and (optionally) filter or badge invalid files.
- [x] `DirectoryBrowserViewModelTests`
  - Expanding folder loads children.
  - Root is always expanded and non-collapsible.
  - Persist/restore of expanded directory state.
  - Font size changes clamp to reasonable min/max.

Integration-style tests (ViewModel-level)
- [x] Update or add tests around `MainWindowViewModel` for:
  - Opening a file by path invoked from the explorer.
  - Persisting and restoring last opened file.

Follow-up tests (recommended)
- [ ] Dirty prompt decision logic:
  - [ ] Cancel prevents open-by-path
  - [ ] Ignore allows open-by-path
  - [ ] Save attempts save then opens if successful
  - Note: this likely needs a small abstraction for prompting (e.g., `IUnsavedChangesPrompt`) to keep tests non-UI.

Manual verification checklist
- [ ] Large directory scan stays responsive.
- [ ] Explorer tree matches the example structure (AC 3.1.4), including chevron visuals.
- [ ] Single-click file opens consistently.
- [ ] If current file is dirty, opening a different file prompts to save/ignore/cancel.
- [ ] Folder click toggles expansion, does not select.
- [ ] Restart restores directory, expanded nodes, and selected file.
- [ ] Open Folder while Browser collapsed auto-expands Browser panel (AC 3.1.5).

### 8. Logging and error handling

- Log persistence load/parse failures once per failure and fall back to safe defaults (ADR-0009).
- Log directory scan failures (e.g., access denied) at info/warn and continue rendering the rest of the tree when possible.
- Avoid spamming logs for routine operations (expand/collapse, hover, selection).

### 9. Out of scope / follow-ons

- Dirty indicators in the explorer list (Roadmap Phase 3.2 item).
- Save prompts when opening a new file or exiting (Roadmap Phase 3.3).
- Multi-root workspaces, tabs, or "Open in new tab" behavior (future phases).

Future (explicit follow-on)
- JSON schema validation for file listing (filtering or badging invalid files).
