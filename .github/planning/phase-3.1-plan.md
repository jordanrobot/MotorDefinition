## Curve Editor Phase 3.1 Plan: Directory Browser (VS Code-style)

### Status

Planned

**Related ADRs**

- ADR-0004 Layout and Panel Persistence Strategy (`docs/adr/adr-0004-layout-and-panel-persistence.md`)
- ADR-0005 Keyboard Shortcuts and Input Routing Policy (`docs/adr/adr-0005-keyboard-shortcuts-and-input-routing.md`)
- ADR-0006 Motor File Schema and Versioning Strategy (`docs/adr/adr-0006-motor-file-schema-and-versioning.md`)
- ADR-0009 Logging and Error Handling Policy (`docs/adr/adr-0009-logging-and-error-handling-policy.md`)

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
- [ ] Show folders and JSON files in the directory listing.
- [ ] Future (deferred): validate candidate files using the JSON schema and/or domain validation, and optionally filter or badge invalid files.

Decision (scope adjustment)
- For Phase 3.1 implementation, we will **not** filter out "invalid" motor files. Instead, we will show all `*.json` files and rely on the existing file-open path to surface errors when the user attempts to open a file.
- This intentionally relaxes the "only show valid curve definition files" requirement for Phase 3.1 to reduce risk and avoid confusing hidden files.

Tree behavior (VS Code-like)
- [ ] Use a single unified tree (folders + files in the same tree).
- [ ] Folders expand/collapse via caret icon.
- [ ] Clicking the caret toggles expand/collapse.
- [ ] Clicking the folder name toggles expand/collapse (not selection).
- [ ] Single-clicking a file opens it in the editor.
- [ ] Sort alphabetically with folders before files.
- [ ] Top-level directory node is always expanded and has no caret.

Session behavior
- [ ] On startup, automatically open the last opened file.
- [ ] On startup, automatically open the last opened directory unless it was explicitly closed.
- [ ] Persist expanded/collapsed state of directories in the tree.
- [ ] If the last opened directory no longer exists, start with Directory Browser collapsed and only log a message.
- [ ] Directory browser width persists (already satisfied by left-zone width persistence).

UI + input
- [ ] Add "Open Folder" to the File menu.
- [ ] Add "Close Folder" button to the main toolbar.
- [ ] Add a "Refresh Explorer" **unicode glyph** button at the top of the Directory Browser panel.
- [ ] Add `F5` shortcut to trigger refresh.

Text display
- [ ] Use monospace font for explorer tree items.
- [ ] Persist explorer text size.
- [ ] Support `Ctrl` + `+` / `Ctrl` + `-` to change explorer text size.
- [ ] Support `Ctrl` + mouse wheel to change explorer text size.
- [ ] Prevent wrapping; long names truncate with ellipses.

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
  - [ ] Add "Open Folder..." -> `OpenFolderCommand`.

- Toolbar
  - [ ] Introduce a minimal top toolbar and add "Close Folder" -> `CloseFolderCommand`.

- Keyboard
  - [ ] Add `F5` -> `RefreshExplorerCommand`.
  - [ ] Add `Ctrl`+`+` and `Ctrl`+`-` -> increase/decrease explorer font size.
    - Implementation note: in practice, bind both OEM and numpad variants.

- Mouse
  - [ ] In the Directory Browser panel, handle `Ctrl` + mouse wheel to adjust font size.

### 6. Implementation steps (Phase 3.1)

#### 6.1 Directory browser foundation
- [ ] Create `DirectoryBrowserViewModel` with:
  - Root directory path
  - Node tree
  - Commands: open folder, close folder, refresh, open file
  - `FontSize` and increment/decrement helpers
- [ ] Create `DirectoryBrowserPanel` view and wire it into the Directory Browser panel content.
- [ ] Replace the placeholder UI in `MainWindow.axaml` with the new panel control.

#### 6.2 Directory scanning + validation
- [ ] Implement `IDirectoryBrowserService` for:
  - Enumerating folders + `*.json` candidates
  - Cancellation and bounded concurrency
- [ ] Ensure the service returns results in a UI-thread-safe way (pure data snapshots).

Future
- Add schema-based validation + optional file badges/filtering.

#### 6.3 Open file from explorer + selection sync
- [ ] Add an "open file by path" method on `MainWindowViewModel` that:
  - Calls `_fileService.LoadAsync(filePath)`
  - Resets selection/editing coordinator as needed
  - Resets undo stack / clean checkpoint appropriately (mirrors existing open-file behavior)
- [ ] When explorer opens a file, call this method.
- [ ] When `MainWindowViewModel` loads a motor file (via menu or explorer), update explorer selection:
  - If file is under the current directory root: expand ancestors and select.
  - Else: clear selection.

Required enabling change
- Expose a `CurrentFilePath` (observable) on `MainWindowViewModel` so selection sync does not depend on `WindowTitle` updates.

#### 6.4 Persistence + startup restore
- [ ] Persist on changes:
  - Last opened directory
  - Explicitly closed flag
  - Expanded directory paths
  - Last opened motor file
  - Explorer font size
- [ ] Implement startup restore logic (see Section 4) and ensure missing directory/file is handled gracefully with a log entry.
- Directory Browser default expanded/collapsed behavior remains as-is (Phase 3.0 default), except:
  - If the last opened directory no longer exists at startup, collapse the Directory Browser panel (AC 3.1.2).

#### 6.5 UI chrome + input
- [ ] Add File menu item for "Open Folder...".
- [ ] Add toolbar with "Close Folder" button (minimal toolbar; do not add extra actions).
- [ ] Add "Refresh Explorer" unicode glyph button to the Directory Browser header.
- [ ] Add keybindings: `F5`, `Ctrl`+`+`, `Ctrl`+`-`.
- [ ] Add Ctrl+mouse wheel handling in Directory Browser panel.

### 7. Testing strategy (Phase 3.1)

Unit tests (preferred)
- [ ] `DirectoryBrowserServiceTests`
  - Enumerates folders + JSON candidates.
  - Lists `*.json` files without filtering in Phase 3.1.
  - Sort order: folders before files, alphabetical.

Future
- Add schema/domain validation and (optionally) filter or badge invalid files.
- [ ] `DirectoryBrowserViewModelTests`
  - Expanding folder loads children.
  - Root is always expanded and non-collapsible.
  - Persist/restore of expanded directory state.
  - Font size changes clamp to reasonable min/max.

Integration-style tests (ViewModel-level)
- [ ] Update or add tests around `MainWindowViewModel` for:
  - Opening a file by path invoked from the explorer.
  - Persisting and restoring last opened file.

Manual verification checklist
- [ ] Large directory scan stays responsive.
- [ ] Single-click file opens consistently.
- [ ] Folder click toggles expansion, does not select.
- [ ] Restart restores directory, expanded nodes, and selected file.

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
