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
- Implement explorer text display requirements (monospace, persisted size, keyboard and Ctrl+wheel adjustments, ellipsis/no-wrap).
- Do not implement dirty indicators in the explorer list (explicitly out of scope for Phase 3.1).
- Do not implement save prompts or any new file-management dialogs beyond folder selection (Phase 3.3).
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

### Assumptions and Constraints
- The app continues using `PanelLayoutPersistence` (one-file-per-key JSON under `%AppData%/CurveEditor`) for Phase 3.1 settings to avoid introducing a second settings store.
- Phase 3.1 does not filter out “invalid” files in the explorer; it lists `*.json` and relies on the existing open-file flow to surface errors.
- Explorer tree is single-root (one opened folder) for Phase 3.1.
- Phase 3.1 does not require multi-tab or multi-root explorer.

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

## [ ] PR 0: Preparation (no user-visible behavior change)

### Tasks
- [ ] Add folder/file scaffolding for Phase 3.1 (types only):
  - [ ] `DirectoryBrowserViewModel` skeleton
  - [ ] `ExplorerNodeViewModel` skeleton
  - [ ] `IDirectoryBrowserService` skeleton
- [ ] Decide and lock down stable persistence keys listed above (do not rename after merging).
- [ ] Decide safe default and clamp range for `DirectoryBrowser.FontSize`.

Required hygiene
- [ ] Ensure no UI changes and no startup behavior changes in this PR.

### Done when
- Build passes.
- No user-visible behavior changes.

### Files
- New: `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs`
- New: `src/CurveEditor/ViewModels/ExplorerNodeViewModel.cs`
- New: `src/CurveEditor/Services/IDirectoryBrowserService.cs`

---

## [ ] PR 1: Persistence plumbing (Directory Browser state + last opened file)

### Tasks
- [ ] Implement minimal, narrow helpers for Phase 3.1 persistence needs (avoid a general settings system):
  - [ ] Encode bools and numbers via `LoadString/SaveString` consistently.
  - [ ] Encode JSON arrays as strings consistently.
- [ ] Add safe fallbacks and logging:
  - [ ] Invalid JSON in `ExpandedDirectoryPaths` -> treat as empty set; log once.
  - [ ] Invalid numeric font size -> use default; log once.
  - [ ] Invalid/unknown paths -> ignore.
- [ ] Add a small folder picker abstraction (e.g., `IFolderPicker`) so Open Folder can be unit-tested.
- [ ] Add debounced persistence for:
  - [ ] Expanded directory paths
  - [ ] Font size
- [ ] Add unit tests for persistence helpers if patterns exist (prefer adding to existing persistence tests folder; otherwise add a new focused test file).

Required hygiene
- [ ] Ensure persistence failures never crash the app.

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

## [ ] PR 2: Directory Browser UI shell (panel content + refresh header)

### Tasks
- [ ] Replace the Directory Browser placeholder content in the left zone with a new `DirectoryBrowserPanel` view.
- [ ] `DirectoryBrowserPanel` includes:
  - [ ] Header row with current root path (ellipsized, no-wrap)
  - [ ] "Refresh Explorer" unicode glyph button (disabled when no folder is open)
  - [ ] TreeView placeholder bound to `DirectoryBrowserViewModel` (can be empty data in this PR)
- [ ] Apply text display requirements at the control level:
  - [ ] Monospace font via a theme resource (do not hard-code a font family)
  - [ ] `NoWrap` + ellipsis trimming for node text

Required hygiene
- [ ] Do not change panel expand/collapse behavior; Directory Browser still toggles via Panel Bar as-is.

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

## [ ] PR 3: Directory scanning + tree behavior (core explorer)

### Tasks
- [ ] Implement `DirectoryBrowserService` to enumerate:
  - [ ] Directories
  - [ ] `*.json` candidate files
- [ ] Ensure scan implementation is UI-thread safe:
  - [ ] Service returns data snapshots (pure DTOs / immutable results)
  - [ ] VM applies changes to `ObservableCollection` on the UI thread
- [ ] Implement TreeView node model behavior in `DirectoryBrowserViewModel`:
  - [ ] Root node is always expanded and has no caret
  - [ ] Directory nodes show a caret and support expand/collapse
  - [ ] Directory name click toggles expansion (not selection)
  - [ ] Caret click toggles expansion
  - [ ] Sort: folders first, then files; alphabetical within each group
- [ ] Wire the header "Refresh Explorer" button to rescan.

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
- [ ] Add `DirectoryBrowserServiceTests`:
  - [ ] Sort order correctness
  - [ ] Cancellation does not throw and stops work

### Quick manual test
1. Open a folder with a mix of JSON and non-JSON.
2. Confirm `*.json` files appear.
3. Expand/collapse directories via caret and name.
4. Click Refresh and confirm the tree updates.

### Deferred follow-on (not Phase 3.1)
- Add schema-based and/or domain validation and (optionally) filter or badge invalid files.

---

## [ ] PR 4: Open folder / close folder + startup restore (AC-critical)

### Tasks
- [ ] Add `OpenFolderCommand`:
  - [ ] Uses the folder picker abstraction (backed by `IStorageProvider`)
  - [ ] Sets root folder, triggers scan
  - [ ] Expands tree to show opened directory (root)
- [ ] Add `CloseFolderCommand`:
  - [ ] Clears root folder
  - [ ] Sets `DirectoryBrowser.WasExplicitlyClosed=true`
  - [ ] Collapses the directory tree (clears nodes)
  - [ ] Does not collapse the entire left zone (left zone can still host other panels)
- [ ] Persist/restore:
  - [ ] `DirectoryBrowser.LastOpenedDirectory`
  - [ ] `DirectoryBrowser.WasExplicitlyClosed`
  - [ ] `DirectoryBrowser.ExpandedDirectoryPaths`
  - [ ] `DirectoryBrowser.FontSize`
  - [ ] `File.LastOpenedMotorFile`
- [ ] Implement startup restore flow:
  - [ ] If last directory exists and wasn’t explicitly closed: open it
  - [ ] If last directory missing: collapse Directory Browser panel and log once (AC 3.1.2)
  - [ ] If last opened motor file exists: open it
  - [ ] If file is under root: expand ancestors and select file node

Required hygiene
- [ ] Ensure startup restore runs once after the window is ready (avoid double-scans).

### Done when
- Meets AC 3.1.1 and AC 3.1.2.

### Files
- Update: [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)
- Update: [src/CurveEditor/Views/MainWindow.axaml.cs](src/CurveEditor/Views/MainWindow.axaml.cs)
- Update: `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs`

### Tests
- [ ] Add/extend tests to cover restore logic:
  - [ ] Missing last directory collapses browser (and does not crash)
  - [ ] Existing last directory restores expanded states

### Quick manual test
1. Open a folder; expand a few directories.
2. Close app; reopen; confirm expansions restored.
3. Rename/delete the folder on disk; reopen app; confirm Directory Browser starts collapsed and no user-facing errors.

---

## [ ] PR 5: Open-on-click + selection sync + keyboard/mouse font sizing

### Tasks
- [ ] Implement single-click file open behavior:
  - [ ] Clicking a file node opens it in the editor (call a dedicated open-by-path method in `MainWindowViewModel`)
  - [ ] Explorer selection reflects the active motor definition (AC 3.1.3)
- [ ] Add a stable `CurrentFilePath` (observable) surface on `MainWindowViewModel` for selection sync.
- [ ] Add File menu item:
  - [ ] File -> "Open Folder..." binds to `OpenFolderCommand`
- [ ] Add a minimal top toolbar:
  - [ ] Add "Close Folder" button bound to `CloseFolderCommand`
  - [ ] Do not add extra toolbar actions beyond requirements
- [ ] Add refresh shortcut:
  - [ ] `F5` triggers Refresh Explorer
- [ ] Implement explorer text size controls:
  - [ ] Persisted font size applied to tree
  - [ ] `Ctrl`+`+` and `Ctrl`+`-` adjust font size
  - [ ] `Ctrl` + mouse wheel adjusts font size
  - [ ] Clamp to min/max and persist after changes

Required hygiene
- [ ] Ensure keybindings remain centralized on `MainWindow` (shortcut policy).

### Done when
- Meets AC 3.1.3.
- All required UI inputs exist and work: File menu Open Folder, toolbar Close Folder, Refresh button, `F5`, font sizing controls.

### Files
- Update: [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml)
- Update: `src/CurveEditor/Views/DirectoryBrowserPanel.axaml`
- Update: `src/CurveEditor/ViewModels/DirectoryBrowserViewModel.cs`
- Update: [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)

### Tests
- [ ] Add/extend tests:
  - [ ] File click -> open-by-path is invoked
  - [ ] Selection sync updates when `CurrentFilePath` changes
  - [ ] Font size increments/decrements clamp and persist

### Quick manual test
1. Open folder.
2. Single-click a motor JSON file; confirm editor loads it.
3. Open a file using existing File->Open; confirm explorer selects it when it’s under the opened folder.
4. Press `F5`; confirm rescan.
5. Adjust font size via Ctrl+Plus/Ctrl+Minus and Ctrl+Wheel; confirm it persists after restart.

---

## [ ] PR 6: Hardening and final validation

### Tasks
- [ ] Handle common filesystem edge cases:
  - [ ] Access denied directories/files (skip with a log entry; do not crash)
  - [ ] Very deep trees (avoid recursion stack overflow; prefer iterative expansion)
  - [ ] Rapid refresh/open folder sequences (cancellation correctness)
- [ ] Ensure persisted expansion state does not "grow unbounded":
  - [ ] Remove expansion entries for directories that no longer exist under the current root
- [ ] Ensure Directory Browser default expanded/collapsed behavior remains as-is (no Phase 3.1 override).
- [ ] Run relevant automated tests and keep build clean.

### Done when
- All acceptance criteria for Phase 3.1 pass in a manual validation pass.

### Final manual validation script (AC-driven)
1. (AC 3.1.1) Open folder, expand nested directories, resize left zone, restart; verify width and expansions restore.
2. (AC 3.1.2) Delete/rename last opened folder, restart; verify Directory Browser panel is collapsed and app continues without user-facing errors.
3. (AC 3.1.3) With a folder open, single-click a motor JSON file; verify editor loads it and explorer selection matches the active file.

### Sign-off checklist
- [ ] All tasks across all PR sections are checked `[x]`.
- [ ] Each acceptance criterion listed above has a verification step (test or manual script).
- [ ] No out-of-scope features were implemented.
