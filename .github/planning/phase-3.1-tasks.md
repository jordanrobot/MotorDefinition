## Phase 3.1 Subtasks: Directory Browser (VS Code-style) (Agent Execution Checklist)

### Purpose
- Provide a PR-sliceable task list for implementing Phase 3.1 with minimal rework.
- Make it easy for agents to validate each acceptance criterion incrementally.

### Execution Rules (Mandatory)
- Treat this file as the single source of truth for work tracking.
- Do not start a PR section until the prior PR section is complete.
- When a task is completed, mark it as `[x]` immediately.
- A PR section is not complete until:
  - All tasks are checked `[x]`, AND
  - The "Done when" criteria are satisfied.
- Do not add “nice-to-haves” that are not listed in this file or the Phase 3.1 requirements.

### Inputs
- Requirements: [.github/planning/phase-3-requirements.md](.github/planning/phase-3-requirements.md) (Phase 3.1 section)
- Plan: [.github/planning/phase-3.1-plan.md](.github/planning/phase-3.1-plan.md)

### Scope Reminder (Phase 3.1)
- Implement the VS Code-style Directory Browser explorer tree (folders + `*.json` files) with persistence and startup restore.
- Add the required UI affordances: File menu "Open Folder", toolbar "Close Folder", panel header "Refresh Explorer" button, `F5` refresh.
- Implement the updated Phase 3.1 follow-up requirements (AC 3.1.4–3.1.9): chevron icons, root display name, dirty prompt on open, auto-expand browser panel when opening a folder while collapsed, and File menu placement requirements.
- Implement explorer text display requirements (monospace, persisted size, keyboard and Ctrl+wheel adjustments, ellipsis/no-wrap).
- Do not implement dirty indicators in the explorer list (explicitly out of scope for Phase 3.1).
- Do not implement exit prompts; only implement the explicit explorer-initiated dirty prompt required by AC 3.1.9.
- Do not change panel expand/collapse behavior (Phase 3.0 already implemented).

### Key Files (Expected touch points)
- UI layout: [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml)
- UI code-behind (startup/restore hook): [src/CurveEditor/Views/MainWindow.axaml.cs](src/CurveEditor/Views/MainWindow.axaml.cs)
- View model (file open integration): [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)
- Persistence helper (settings storage): [src/CurveEditor/Behaviors/PanelLayoutPersistence.cs](src/CurveEditor/Behaviors/PanelLayoutPersistence.cs)
- File load/save baseline: [src/CurveEditor/Services/FileService.cs](src/CurveEditor/Services/FileService.cs)
- Validation baseline: [src/CurveEditor/Services/ValidationService.cs](src/CurveEditor/Services/ValidationService.cs)

New expected files
- Explorer UI: `src/CurveEditor/Views/DirectoryBrowserPanel.axaml` (+ `.cs` if needed)
- Explorer VM + node model: `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs`, `ExplorerNodeViewModel.cs`
- Explorer scanning service: `src/CurveEditor/Services/DirectoryBrowserService.cs` (+ interface)
- Tests: `tests/CurveEditor.Tests/...`

### Acceptance Criteria (Phase 3.1)
- AC 3.1.1: On restart, if the last opened directory still exists, its expand/collapse state and the directory browser width are restored.
- AC 3.1.2: If the last opened directory no longer exists, the directory browser starts collapsed with no errors shown to the user beyond any appropriate log entry.
- AC 3.1.3: Clicking a file once in the directory browser always opens it in the CurveEditor, and the selection in the tree matches the active motor definition.
- AC 3.1.4: The directory browser file/directory listing matches the specified tree structure and behavior described in the requirements.
- AC 3.1.5: If the user opens a directory while the directory browser panel is collapsed, the panel automatically expands to show the directory tree.
- AC 3.1.6: The "Close Folder" menu item is located in the File menu, not Directory Browser panel header.
- AC 3.1.7: The "Open Folder" menu item is located in the File menu, not the Directory Browser panel header menu.
- AC 3.1.8: When the user executes "Close Directory", the directory tree does not collapse.
- AC 3.1.9: When the user opens a json file while the current file is dirty, the user is prompted to save the current file first.

### Assumptions and Constraints
- The app continues using `PanelLayoutPersistence` (one-file-per-key JSON under `%AppData%/CurveEditor`) for Phase 3.1 settings to avoid introducing a second settings store.
- Phase 3.1 does not filter out “invalid” files in the explorer; it lists `*.json` and relies on the existing open-file flow to surface errors.
- Explorer tree is single-root (one opened folder) for Phase 3.1.
- Phase 3.1 does not require multi-tab or multi-root explorer.
- **AC precedence**: when requirements bullets conflict with acceptance criteria, acceptance criteria win.
- **No startup prompts**: do not show modal prompts during startup restore; prompts only occur on explicit user actions.
- **Close Directory conflict**: requirements contain both "collapse the directory tree" and AC 3.1.8 "does not collapse". For this tasks list, interpret AC 3.1.8 as: do not collapse the Directory Browser panel/zone; closing clears the opened root directory but keeps the panel’s expanded/collapsed state unchanged.

### State Model Summary (Target)
- Runtime state (Directory Browser)
  - `RootDirectoryPath` (string?)
  - `RootNode` (always expanded, non-collapsible)
  - Per-directory expansion state (set of directory paths)
  - `SelectedNodePath` (string?)
  - `ExplorerFontSize` (double)
  - Last scan snapshot (per-scan lifetime)
- Persistence keys (Phase 3.1)
  - `DirectoryBrowser.LastOpenedDirectory` (string)
  - `DirectoryBrowser.WasExplicitlyClosed` (bool)
  - `DirectoryBrowser.ExpandedDirectoryPaths` (string: JSON array)
  - `DirectoryBrowser.SelectedPath` (string, optional but recommended to satisfy AC 3.1.3 after restart)
  - `DirectoryBrowser.FontSize` (double)
  - `File.LastOpenedMotorFile` (string)

Defaults (first run / no persisted state)
- Directory Browser default expanded/collapsed behavior remains as-is (Phase 3.0 default).
- Explorer font size uses a conservative default (e.g., 12) and is clamped to a safe range.

### Implementation Notes (to avoid known pitfalls)
- Do not use `IFileService.LoadAsync()` for background scanning; it mutates `CurrentFilePath`.
- Any background work (scanning) must not mutate `ObservableCollection<T>` off the UI thread; apply snapshots on the UI thread.
- Clicking folder names must toggle expansion without selecting (per requirements); TreeView’s selection model will fight you if not handled explicitly.
- Root folder must not show a caret and must not be user-collapsible.
- Persist expanded directories using stable path semantics:
  - Prefer paths relative to the opened root to keep the persisted set portable when the root path changes.
  - Normalize directory separators and casing consistently per OS.
- Persist frequently-changing values (expanded paths + font size) with debouncing to avoid write amplification.

---

## [x] PR 0: Preparation (no user-visible behavior change)

### Tasks
- [x] Add folder/file scaffolding for Phase 3.1 (types only):
  - [x] `DirectoryBrowserViewModel` skeleton
  - [x] `ExplorerNodeViewModel` skeleton
  - [x] `IDirectoryBrowserService` skeleton
- [x] Decide and lock down stable persistence keys listed above (do not rename after merging).
- [x] Decide safe default and clamp range for `DirectoryBrowser.FontSize`.

Required hygiene
- [x] Ensure no UI changes and no startup behavior changes in this PR.

### Done when
- Build passes.
- No user-visible behavior changes.

### Files
- New: `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs`
- New: `src/CurveEditor/ViewModels/ExplorerNodeViewModel.cs`
- New: `src/CurveEditor/Services/IDirectoryBrowserService.cs`

---

## [x] PR 1: Persistence plumbing (Directory Browser state + last opened file)

### Tasks
- [x] Implement minimal, narrow helpers for Phase 3.1 persistence needs (avoid a general settings system):
  - [x] Encode bools and numbers via `LoadString/SaveString` consistently.
  - [x] Encode JSON arrays as strings consistently.
- [x] Add safe fallbacks and logging:
  - [x] Invalid JSON in `ExpandedDirectoryPaths` -> treat as empty set; log once.
  - [x] Invalid numeric font size -> use default; log once.
  - [x] Invalid/unknown paths -> ignore.
- [x] Add a small folder picker abstraction (e.g., `IFolderPicker`) so Open Folder can be unit-tested.
- [x] Add debounced persistence for:
  - [x] Expanded directory paths
  - [x] Font size
- [x] Add unit tests for persistence helpers if patterns exist (prefer adding to existing persistence tests folder; otherwise add a new focused test file).

Required hygiene
- [x] Ensure persistence failures never crash the app.

### Done when
- Settings values can be saved and loaded reliably (even if not yet wired to UI).
- High-frequency explorer interactions do not trigger a file write per event (debounced/coalesced).

### Files
- Update: [src/CurveEditor/Behaviors/PanelLayoutPersistence.cs](src/CurveEditor/Behaviors/PanelLayoutPersistence.cs)
- Add/Update tests under `tests/CurveEditor.Tests` (new file if needed)

### Quick manual test
1. Run the app.
2. Close it.
3. Verify no crashes and no noisy logs.

---

## [x] PR 2: Directory Browser UI shell (panel content + refresh header)

### Tasks
- [x] Replace the Directory Browser placeholder content in the left zone with a new `DirectoryBrowserPanel` view.
- [x] `DirectoryBrowserPanel` includes:
  - [x] Header row with current root path (ellipsized, no-wrap)
  - [x] "Refresh Explorer" unicode glyph button (disabled when no folder is open)
  - [x] TreeView placeholder bound to `DirectoryBrowserViewModel` (can be empty data in this PR)
- [x] Apply text display requirements at the control level:
  - [x] Monospace font via a theme resource (do not hard-code a font family)
  - [x] `NoWrap` + ellipsis trimming for node text

Required hygiene
- [x] Do not change panel expand/collapse behavior; Directory Browser still toggles via Panel Bar as-is.

### Done when
- Directory Browser panel content renders (no longer shows "Coming Soon").
- Refresh button appears (may be non-functional until PR 3).

### Files
- Update: [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml)
- Add: `src/CurveEditor/Views/DirectoryBrowserPanel.axaml`
- Add (optional): `src/CurveEditor/Views/DirectoryBrowserPanel.axaml.cs`
- Add: `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs` (wired into `MainWindowViewModel`)

### Quick manual test
1. Launch.
2. Toggle Directory Browser via Panel Bar.
3. Confirm the panel renders and no exceptions occur.

---

## [x] PR 3: Directory scanning + tree behavior (core explorer)

### Tasks
- [x] Implement `DirectoryBrowserService` to enumerate:
  - [x] Directories
  - [x] `*.json` candidate files
- [x] Ensure scan implementation is UI-thread safe:
  - [x] Service returns data snapshots (pure DTOs / immutable results)
  - [x] VM applies changes to `ObservableCollection` on the UI thread
- [x] Implement TreeView node model behavior in `DirectoryBrowserViewModel`:
  - [x] Root node is always expanded and has no caret
  - [x] Directory nodes show a caret and support expand/collapse
  - [x] Directory name click toggles expansion (not selection)
  - [x] Caret click toggles expansion
  - [x] Sort: folders first, then files; alphabetical within each group
- [x] Wire the header "Refresh Explorer" button to rescan.

Required hygiene
- [ ] Ensure large directories remain responsive:
  - [ ] Use lazy loading for directory children (load on expand) OR otherwise clearly justify an eager approach with cancellation.

### Done when
- Explorer shows folders and `*.json` files.
- Refresh triggers a rescan.

### Files
- Add: `src/CurveEditor/Services/DirectoryBrowserService.cs`
- Update: `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs`
- Update: `src/CurveEditor/ViewModels/ExplorerNodeViewModel.cs`
- Update: `src/CurveEditor/Views/DirectoryBrowserPanel.axaml`

### Tests
- [x] Add `DirectoryBrowserServiceTests`:
  - [x] Sort order correctness
  - [x] Cancellation does not throw and stops work

### Quick manual test
1. Open a folder with a mix of JSON and non-JSON.
2. Confirm `*.json` files appear.
3. Expand/collapse directories via caret and name.
4. Click Refresh and confirm the tree updates.

### Deferred follow-on (not Phase 3.1)
- Add schema-based and/or domain validation and (optionally) filter or badge invalid files.

---

## [x] PR 4: Open folder / close folder + startup restore (AC-critical)

### Tasks
- [x] Add `OpenFolderCommand`:
  - [x] Uses the folder picker abstraction (backed by `IStorageProvider`)
  - [x] Sets root folder, triggers scan
  - [x] Expands tree to show opened directory (root)
- [x] Add `CloseFolderCommand`:
  - [x] Clears root folder
  - [x] Sets `DirectoryBrowser.WasExplicitlyClosed=true`
  - [x] Collapses the directory tree (clears nodes)
  - [x] Does not collapse the entire left zone (left zone can still host other panels)
- [x] Persist/restore:
  - [x] `DirectoryBrowser.LastOpenedDirectory`
  - [x] `DirectoryBrowser.WasExplicitlyClosed`
  - [x] `DirectoryBrowser.ExpandedDirectoryPaths`
  - [x] `DirectoryBrowser.FontSize`
  - [x] `File.LastOpenedMotorFile`
- [x] Implement startup restore flow:
  - [x] If last directory exists and wasn’t explicitly closed: open it
  - [x] If last directory missing: collapse Directory Browser panel and log once (AC 3.1.2)
  - [x] If last opened motor file exists: open it
  - [x] If file is under root: expand ancestors and select file node


Required hygiene
- [x] Ensure startup restore runs once after the window is ready (avoid double-scans).

### Done when
- Meets AC 3.1.1 and AC 3.1.2.

### Files
- Update: [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)
- Update: [src/CurveEditor/Views/MainWindow.axaml.cs](src/CurveEditor/Views/MainWindow.axaml.cs)
- Update: `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs`

### Tests
- [x] Add/extend tests to cover restore logic:
  - [x] Missing last directory collapses browser (and does not crash)
  - [x] Existing last directory restores expanded states

### Quick manual test
1. Open a folder; expand a few directories.
2. Close app; reopen; confirm expansions restored.
3. Rename/delete the folder on disk; reopen app; confirm Directory Browser starts collapsed and no user-facing errors.

---

## [x] PR 5: Open-on-click + selection sync + keyboard/mouse font sizing

### Tasks
- [x] Implement single-click file open behavior:
  - [x] Clicking a file node opens it in the editor (call a dedicated open-by-path method in `MainWindowViewModel`)
  - [x] Explorer selection reflects the active motor definition (AC 3.1.3)
- [x] Add a stable `CurrentFilePath` (observable) surface on `MainWindowViewModel` for selection sync.
- [x] Add File menu item:
  - [x] File -> "Open Folder..." binds to `OpenFolderCommand`
- [x] Add a minimal top toolbar:
  - [x] Add "Close Folder" button bound to `CloseFolderCommand`
  - [x] Do not add extra toolbar actions beyond requirements
- [x] Add refresh shortcut:
  - [x] `F5` triggers Refresh Explorer
- [x] Implement explorer text size controls:
  - [x] Persisted font size applied to tree
  - [x] `Ctrl`+`+` and `Ctrl`+`-` adjust font size
  - [x] `Ctrl` + mouse wheel adjusts font size
  - [x] Clamp to min/max and persist after changes

Required hygiene
- [x] Ensure keybindings remain centralized on `MainWindow` (shortcut policy).

### Done when
- Meets AC 3.1.3.
- All required UI inputs exist and work: File menu Open Folder, toolbar Close Folder, Refresh button, `F5`, font sizing controls.

### Files
- Update: [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml)
- Update: `src/CurveEditor/Views/DirectoryBrowserPanel.axaml`
- Update: `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs`
- Update: [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)

### Tests
- [x] Add/extend tests:
  - [x] File click -> open-by-path is invoked
  - [x] Selection sync updates when `CurrentFilePath` changes
  - [x] Font size increments/decrements clamp and persist

### Quick manual test
1. Open folder.
2. Single-click a motor JSON file; confirm editor loads it.
3. Open a file using existing File->Open; confirm explorer selects it when it’s under the opened folder.
4. Press `F5`; confirm rescan.
5. Adjust font size via Ctrl+Plus/Ctrl+Minus and Ctrl+Wheel; confirm it persists after restart.

---

## [x] PR 6: Hardening and final validation

### Tasks
- [x] Handle common filesystem edge cases:
  - [x] Access denied directories/files (skip with a log entry; do not crash)
  - [x] Very deep trees (avoid recursion stack overflow; prefer iterative expansion)
  - [x] Rapid refresh/open folder sequences (cancellation correctness)
- [x] Ensure persisted expansion state does not "grow unbounded":
  - [x] Remove expansion entries for directories that no longer exist under the current root
- [x] Ensure Directory Browser default expanded/collapsed behavior remains as-is (no Phase 3.1 override).
- [x] Run relevant automated tests and keep build clean.

### Done when
- AC 3.1.1–3.1.3 pass in a manual validation pass (original Phase 3.1 scope).

### Final manual validation script (AC-driven, original scope)
1. (AC 3.1.1) Open folder, expand nested directories, resize left zone, restart; verify width and expansions restore.
2. (AC 3.1.2) Delete/rename last opened folder, restart; verify Directory Browser panel is collapsed and app continues without user-facing errors.
3. (AC 3.1.3) With a folder open, single-click a motor JSON file; verify editor loads it and explorer selection matches the active file.

### Sign-off checklist
- [x] All tasks across PR 0–PR 6 sections are checked `[x]`.
- [x] Each acceptance criterion listed above (AC 3.1.1–3.1.3) has a verification step (test or manual script).

---

## [ ] PR 7: Phase 3.1 follow-up - AC 3.1.4–3.1.7 wiring (panel auto-expand + menu placement + root display name)

### Goal
Address newly added Phase 3.1 acceptance criteria around explorer layout fidelity, menu placement, and panel auto-expand behavior.

### Tasks

AC 3.1.5: Auto-expand browser panel when opening a folder
- [ ] If the Directory Browser panel is currently collapsed, invoking Open Folder must set the active left panel to Directory Browser before scanning.
- [ ] Ensure this only occurs for explicit user actions (not during startup restore).

AC 3.1.6 / AC 3.1.7: File menu placement
- [ ] Add File menu item "Close Folder" bound to the existing close command.
- [ ] Ensure "Open Folder" remains in the File menu.
- [ ] Ensure there is no separate Open Folder action in the Directory Browser panel header/menu.

Root display name requirement
- [ ] Change the root node display name to show directory name only (not full path).
- [ ] Handle drive-root cases (e.g., `C:\` displays as `C:`).

AC 3.1.4 (partial): verify tree structure fidelity
- [ ] Add a focused test against the view model tree shape that verifies:
  - Files appear under the correct parent directory.
  - Files do not appear under collapsed directories.
  - A file at root level does not visually imply it is under a collapsed sibling directory.

### Done when
- AC 3.1.5, AC 3.1.6, and AC 3.1.7 are satisfied by manual verification.
- Root node shows directory name only.
- Added/updated tests pass.

### Files
- Likely: [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml)
- Likely: [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)
- Likely: [src/CurveEditor/ViewModels/ExplorerNodeViewModel.cs](src/CurveEditor/ViewModels/ExplorerNodeViewModel.cs)
- Likely: [src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs](src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs)
- Tests: `tests/CurveEditor.Tests/ViewModels/...`

### Quick manual test
1. Collapse the Directory Browser panel via Panel Bar.
2. Use File -> Open Folder.
3. Confirm the Directory Browser panel expands and shows the tree.
4. Confirm root node displays only directory name.
5. Confirm File menu contains Open Folder + Close Folder.

---

## [ ] PR 8: Phase 3.1 follow-up - chevron icons (collapsed/expanded)

### Goal
Match the updated VS Code-style visuals: collapsed folder shows chevron right; expanded shows chevron down.

### Tasks
- [ ] Replace the folder expander/caret visuals with Fluent chevrons:
  - [ ] Collapsed: chevron right
  - [ ] Expanded: chevron down
- [ ] Ensure the root node shows no expander icon.
- [ ] Ensure clicking the chevron toggles expand/collapse.
- [ ] Keep the existing behavior: clicking the folder name toggles expand/collapse without selecting.

Required hygiene
- [ ] Do not introduce new colors or styling tokens; reuse existing theme resources.

### Done when
- Chevron visuals match the requirement and don’t regress folder click behavior.

### Files
- Likely: `src/CurveEditor/Views/DirectoryBrowserPanel.axaml`
- Likely: `src/CurveEditor/Views/DirectoryBrowserPanel.axaml.cs`
- Likely: `src/CurveEditor/ViewModels/ExplorerNodeViewModel.cs`

### Quick manual test
1. Open a folder and verify chevron right/down changes with expand/collapse.
2. Verify root has no chevron.
3. Click folder name: expands/collapses without selecting.

---

## [ ] PR 9: Phase 3.1 follow-up - dirty prompt on explorer open (AC 3.1.9)

### Goal
Fix the regression: when opening a file from the explorer while the current file is dirty, prompt the user to Save / Ignore / Cancel.

### Tasks

Prompt plumbing (testability)
- [ ] Introduce a small abstraction for prompting (e.g., `IUnsavedChangesPrompt`) so ViewModel tests can cover the decision logic without UI.
- [ ] Add a production implementation that uses the existing UI framework to show the prompt.

Explorer open behavior
- [ ] When an explorer-initiated open is requested:
  - [ ] If current document is not dirty: open immediately.
  - [ ] If dirty: prompt Save / Ignore / Cancel.
    - [ ] Save: call existing save flow; if save succeeds, proceed opening.
    - [ ] Ignore: proceed opening without saving.
    - [ ] Cancel: do not open; keep current file.

Startup restore safety
- [ ] Ensure startup restore never shows this prompt.

Tests
- [ ] Add unit tests verifying:
  - [ ] Cancel prevents open-by-path.
  - [ ] Ignore allows open-by-path.
  - [ ] Save attempts save then opens (if save succeeded).

### Done when
- AC 3.1.9 is satisfied.
- All tests pass.

### Files
- Likely: [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)
- Likely: `src/CurveEditor/Services/...` (prompt abstraction + implementation)
- Tests: `tests/CurveEditor.Tests/ViewModels/...`

### Quick manual test
1. Open a motor file, make an edit to mark it dirty.
2. Single-click another motor JSON file in the explorer.
3. Verify prompt Save/Ignore/Cancel.
4. Verify each option behaves as specified.

---

## [ ] PR 10: Phase 3.1 follow-up - Close Directory behavior (AC 3.1.8) + final validation refresh

### Goal
Resolve the Close Directory behavior conflict and align implementation with AC 3.1.8.

### Tasks
- [ ] Update Close Directory behavior to satisfy AC 3.1.8 (directory tree does not collapse):
  - [ ] Keep the Directory Browser panel expanded/collapsed state unchanged.
  - [ ] Clear the opened root directory state so the tree is empty (no root directory shown).
- [ ] Update persistence flags so this does not re-open the directory on restart.
- [ ] Update manual validation script for AC 3.1.4–3.1.9.

### Done when
- AC 3.1.8 passes manual verification.
- All Phase 3.1 acceptance criteria (AC 3.1.1–3.1.9) have a verification step.

### Files
- Likely: [src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs](src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs)
- Likely: [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)

### Quick manual test
1. Open a folder.
2. Click Close Folder.
3. Confirm the left zone does not collapse, and the explorer shows no open directory.

Verification pointers
- AC 3.1.1/3.1.2: [tests/CurveEditor.Tests/ViewModels/DirectoryBrowserRestoreTests.cs](tests/CurveEditor.Tests/ViewModels/DirectoryBrowserRestoreTests.cs), [tests/CurveEditor.Tests/ViewModels/MainWindowRestoreTests.cs](tests/CurveEditor.Tests/ViewModels/MainWindowRestoreTests.cs)
- AC 3.1.3: [tests/CurveEditor.Tests/ViewModels/DirectoryBrowserInteractionTests.cs](tests/CurveEditor.Tests/ViewModels/DirectoryBrowserInteractionTests.cs), [tests/CurveEditor.Tests/ViewModels/MainWindowDirectoryBrowserSyncTests.cs](tests/CurveEditor.Tests/ViewModels/MainWindowDirectoryBrowserSyncTests.cs)

---

## Updated final manual validation script (AC-driven, updated scope)

1. (AC 3.1.1) Open folder, expand nested directories, resize left zone, restart; verify width and expansions restore.
2. (AC 3.1.2) Delete/rename last opened folder, restart; verify Directory Browser panel is collapsed and app continues without user-facing errors.
3. (AC 3.1.3) With a folder open, single-click a motor JSON file; verify editor loads it and explorer selection matches the active file.
4. (AC 3.1.4) Verify the tree structure matches the example:
  - Root-level files visually belong to the root.
  - Collapsed directories do not visually contain root-level files.
  - Expanded directories show their immediate children correctly.
5. (AC 3.1.5) Collapse the Directory Browser panel; use File -> Open Folder; verify the panel expands and shows the tree.
6. (AC 3.1.6) Verify File menu contains Close Folder.
7. (AC 3.1.7) Verify File menu contains Open Folder (and explorer header does not add a conflicting Open Folder menu).
8. (AC 3.1.8) Execute Close Directory; verify the Directory Browser panel does not collapse (panel state unchanged), and no directory is shown as open.
9. (AC 3.1.9) Make current file dirty; single-click a different JSON file in explorer; verify Save/Ignore/Cancel and each option behaves as specified.

## Updated sign-off checklist (for revised Phase 3.1 requirements)

- [ ] PR 7–PR 10 tasks are complete.
- [ ] All acceptance criteria (AC 3.1.1–3.1.9) have a verification step (test or manual script).
- [ ] No out-of-scope features were implemented.
