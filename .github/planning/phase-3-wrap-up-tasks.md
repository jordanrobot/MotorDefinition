## Phase 3 Wrap-up Subtasks: Explorer Polish + Unsaved Changes UX (Agent Execution Checklist)

### Purpose

- Provide a PR-sliceable task list for completing the remaining Phase 3 UX items listed in .github/planning/phase-3-wrap-up-plan.md.
- Make it easy to validate each acceptance criterion incrementally with a consistent Save/Don’t Save/Cancel UX.

### Execution Rules (Mandatory)

- Treat this file as the single source of truth for work tracking.
- Do not start a PR section until the prior PR section is complete.
- When a task is completed, mark it as `[x]` immediately.
- A PR section is not complete until:
  - All tasks are checked `[x]`, AND
  - The "Done when" criteria are satisfied.
- Do not add “nice-to-haves” that are not listed in this file or the phase plan.

### Assumptions and Constraints

- No new dialogs or UX surfaces beyond adjusting existing dialog copy/labels and wiring prompts into existing flows.
- The current unsaved-changes enum value `Ignore` remains as-is (we treat it as “Don’t Save” in the UI).
- We prefer testable ViewModel helpers for prompt decision logic; UI lifecycle wiring (window close) may need manual testing.
- Directory browser “dirty indicator” is only required for the currently active file in the editor.

### Scope Reminder (Phase 3 Wrap-up)

- Reduce indentation in the Directory Browser tree.
- Add a dirty indicator (`*`) in the directory list for the active dirty file.
- Ensure consistent Save / Don’t Save / Cancel prompts when the user is about to lose unsaved changes:
  - closing the app/window
  - opening another file (file picker and directory browser)
  - creating a new file

### Key Files (Expected touch points)

- Unsaved changes dialog:
  - [src/MotorEditor.Avalonia/Views/UnsavedChangesDialog.axaml](src/MotorEditor.Avalonia/Views/UnsavedChangesDialog.axaml)
  - [src/MotorEditor.Avalonia/Views/UnsavedChangesDialog.axaml.cs](src/MotorEditor.Avalonia/Views/UnsavedChangesDialog.axaml.cs)
- File/open/new/close logic:
  - [src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs](src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs)
- App/window close lifecycle:
  - [src/MotorEditor.Avalonia/Views/MainWindow.axaml.cs](src/MotorEditor.Avalonia/Views/MainWindow.axaml.cs)
  - (optional) [src/MotorEditor.Avalonia/App.axaml.cs](src/MotorEditor.Avalonia/App.axaml.cs)
- Directory browser (UI + state):
  - [src/MotorEditor.Avalonia/Views/DirectoryBrowserPanel.axaml](src/MotorEditor.Avalonia/Views/DirectoryBrowserPanel.axaml)
  - [src/MotorEditor.Avalonia/ViewModels/DirectoryBrowserViewModel.cs](src/MotorEditor.Avalonia/ViewModels/DirectoryBrowserViewModel.cs)
  - [src/MotorEditor.Avalonia/ViewModels/ExplorerNodeViewModel.cs](src/MotorEditor.Avalonia/ViewModels/ExplorerNodeViewModel.cs)
- Tests (existing patterns):
  - [tests/CurveEditor.Tests/ViewModels/MainWindowDirectoryBrowserDirtyOpenTests.cs](tests/CurveEditor.Tests/ViewModels/MainWindowDirectoryBrowserDirtyOpenTests.cs)
  - [tests/CurveEditor.Tests/ViewModels/MainWindowViewModelCloseFileTests.cs](tests/CurveEditor.Tests/ViewModels/MainWindowViewModelCloseFileTests.cs)
  - [tests/CurveEditor.Tests/ViewModels/MainWindowDirectoryBrowserSyncTests.cs](tests/CurveEditor.Tests/ViewModels/MainWindowDirectoryBrowserSyncTests.cs)

### Acceptance Criteria (Phase 3 Wrap-up)

- [x] AC 3W.1 — Unsaved changes prompt UI
  - [x] The prompt offers exactly: Save / Don’t Save / Cancel.
  - [x] Button text is centered within each button.
  - [x] “Don’t Save” maps to `UnsavedChangesChoice.Ignore`.
- [x] AC 3W.2 — Open file prompts
  - [x] When dirty, opening another file via directory browser prompts; Cancel aborts.
  - [x] When dirty, opening another file via the file picker prompts; Cancel aborts.
- [x] AC 3W.3 — New file prompt
  - [x] When dirty, creating a new file prompts; Cancel aborts.
- [ ] AC 3W.4 — App/window close prompt
  - [ ] When dirty, closing the app/window prompts; Cancel aborts app/window close.
  - [ ] If Save is chosen and save is cancelled/fails (still dirty), closing is aborted.
- [x] AC 3W.5 — Directory browser dirty indicator
  - [x] The directory list appends `*` to the active file’s displayed name when it is dirty.
  - [x] The `*` disappears after Save.
- [ ] AC 3W.6 — Directory browser indentation
  - [ ] Tree nesting indentation is reduced (target visual: ~2–3 monospace characters per level).
  - [ ] Chevrons remain aligned and readable.

### State Model Summary (Target)

- Runtime state
  - `MainWindowViewModel.IsDirty` and `MainWindowViewModel.CurrentFilePath` remain the single source of truth for document dirty state.
  - A single ViewModel helper decides whether an operation that would lose changes may proceed.
- Directory browser
  - Directory browser has enough state to render the active dirty indicator, driven by the main ViewModel.
- UI prompts
  - The `UnsavedChangesDialog` remains the single prompt surface for all “lose unsaved changes” decisions.

### Agent Notes (Migration Guidance)

- Current implementation already has centralized prompting via `UnsavedChangesPromptAsync` for:
  - “open another file” from directory browser
  - “close this file”
- This phase should migrate remaining paths to that same prompt:
  - `OpenFileAsync` (file picker)
  - `NewMotorAsync` (currently uses `MessageDialog`)
  - app/window close

### Implementation Notes (to avoid known pitfalls)

- Avoid awaiting async work directly inside `Window.Closing` unless the event args and Avalonia version support it cleanly; prefer a “cancel-first, then re-close after decision” approach if needed.
- Ensure “Save” flow aborts the initiating action if save is cancelled or still leaves `IsDirty=true`.
- Avoid prompt reentrancy loops on window close (guard flag).

---

## [ ] PR 0: Preparation (no behavior change)

### Tasks

- [ ] Decide and document the canonical action description strings (exact text values) to be used for the prompt, aligned to the plan:
  - [ ] "open another file"
  - [ ] "create a new file"
  - [ ] "close the app" (or "close the application")
- [ ] Identify the cleanest single helper point in `MainWindowViewModel` for “confirm lose unsaved changes” to avoid duplicating logic.
- [ ] Confirm current tests that already cover dirty prompt flows (directory browser open and close file) and list them in the PR description.

Required hygiene:

- [ ] No user-visible behavior changes in this PR.

### Done when

- The next PRs can reference a stable set of prompt action strings.

### Files

- (Docs only) [ .github/planning/phase-3-wrap-up-plan.md](.github/planning/phase-3-wrap-up-plan.md)
- (Tasks only) [ .github/planning/phase-3-wrap-up-tasks.md](.github/planning/phase-3-wrap-up-tasks.md)

---

## [x] PR 1: Standardize Unsaved Changes Dialog UI

### Goal

Match the required button labels and alignment for the existing prompt dialog.

### Tasks

- [x] Update the Unsaved Changes dialog buttons:
  - [x] Change button label from "Ignore" to "Don’t Save".
  - [x] Ensure button text is centered within each button.
- [x] Confirm the dialog still returns:
  - [x] Save → `UnsavedChangesChoice.Save`
  - [x] Don’t Save → `UnsavedChangesChoice.Ignore`
  - [x] Cancel → `UnsavedChangesChoice.Cancel`

Required hygiene:

- [x] Ensure no changes to enum values or public API surface; UI label change only.

### Done when

- AC 3W.1 passes.
- Existing unit tests remain green.

### Files

- [src/MotorEditor.Avalonia/Views/UnsavedChangesDialog.axaml](src/MotorEditor.Avalonia/Views/UnsavedChangesDialog.axaml)
- [src/MotorEditor.Avalonia/Views/UnsavedChangesDialog.axaml.cs](src/MotorEditor.Avalonia/Views/UnsavedChangesDialog.axaml.cs)

### Quick manual test

1. Open a file.
2. Make an edit to become dirty.
3. Trigger a flow that already prompts (directory browser open or Close File).
4. Verify the three buttons are Save / Don’t Save / Cancel and the text is centered.

---

## [x] PR 2: Unify Dirty Prompts for File Picker Open + New File

### Goal

All “lose changes” entry points use the same Save / Don’t Save / Cancel prompt and semantics.

### Tasks

- [x] Add a single ViewModel helper that encapsulates the dirty-check + prompt + save semantics:
  - [x] Inputs: action description string
  - [x] Output: proceed/cancel decision
  - [x] Behavior:
    - [x] If not dirty, proceeds without prompting
    - [x] If Cancel, aborts
    - [x] If Save, invokes `SaveAsync`; if still dirty, aborts
    - [x] If Don’t Save, proceeds
- [x] Update `MainWindowViewModel.OpenFileAsync`:
  - [x] If dirty, prompt *before* showing the file picker.
  - [x] Cancel aborts opening.
- [x] Update `MainWindowViewModel.NewMotorAsync`:
  - [x] Replace `MessageDialog` flow with `UnsavedChangesPromptAsync` via the helper.
  - [x] Cancel aborts new-file creation.

Required hygiene:

- [x] Keep status messages coherent (e.g., "Open cancelled.", "New file cancelled.") and consistent with existing patterns.

### Done when

- AC 3W.2 and AC 3W.3 pass.
- The directory browser dirty-open and close-file flows continue to work unchanged.

### Files

- [src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs](src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs)
- (tests) add/update one or more:
  - [tests/CurveEditor.Tests/ViewModels/MainWindowViewModelDirtyPromptTests.cs](tests/CurveEditor.Tests/ViewModels/MainWindowViewModelDirtyPromptTests.cs)
  - [tests/CurveEditor.Tests/ViewModels/MainWindowViewModelOpenAndNewDirtyPromptTests.cs](tests/CurveEditor.Tests/ViewModels/MainWindowViewModelOpenAndNewDirtyPromptTests.cs)

### Quick manual test

1. Open a file.
2. Make an edit.
3. Use File → Open; verify prompt appears before picker.
4. Choose Cancel; confirm picker does not open and file remains.
5. Use File → New; verify prompt and Cancel behavior.

---

## [x] PR 3: Prompt on App/Window Close

### Goal

Closing the window/app prompts to Save / Don’t Save / Cancel and Cancel blocks shutdown.

### Tasks

- [x] Implement a cancelable close flow that uses the same ViewModel helper as PR 2.
- [x] Choose the simplest lifecycle hook that supports cancellation reliably:
  - [x] Option A: handle `MainWindow.Closing` in code-behind and cancel the close when needed
  - [ ] Option B: handle `IClassicDesktopStyleApplicationLifetime.ShutdownRequested` and cancel shutdown
- [x] Ensure the close prompt behavior:
  - [x] If no motor is loaded, do not prompt.
  - [x] If dirty:
    - [x] Cancel aborts close
    - [x] Save attempts save; if still dirty, abort close
    - [x] Don’t Save proceeds with close
- [x] Add a guard to avoid re-entrancy loops when programmatically triggering close after user confirms.

Required hygiene:

- [x] Preserve existing panel layout persistence behavior; confirm it does not regress when close is cancelled.

### Done when

- AC 3W.4 passes in manual validation.
- No regressions in app close behavior (no infinite loops, no double prompts).

### Files

- [src/MotorEditor.Avalonia/Views/MainWindow.axaml.cs](src/MotorEditor.Avalonia/Views/MainWindow.axaml.cs)
- (optional) [src/MotorEditor.Avalonia/App.axaml.cs](src/MotorEditor.Avalonia/App.axaml.cs)
- (optional) [src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs](src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs) (if adding a dedicated close-confirm helper)

### Quick manual test

1. Open a file.
2. Make an edit.
3. Click the window close button.
4. Choose Cancel; confirm the app remains open.
5. Repeat and choose Save, then cancel the Save As dialog (if applicable); confirm app remains open.

---

## [x] PR 4: Dirty Indicator (`*`) in Directory Browser List

### Goal

Show `*` appended to the currently-open file name in the directory browser when dirty.

### Tasks

- [x] Decide the minimal data flow from the main editor state to the directory browser:
  - [x] Update directory browser state from `MainWindowViewModel` when `CurrentFilePath` or `IsDirty` changes.
- [x] Implement a non-XAML-converter-based computation for display text:
  - [x] Add a computed property (e.g., `DisplayNameWithDirtyIndicator`) on `ExplorerNodeViewModel`, or
  - [x] Add an `IsActiveFile`/`IsActiveFileDirty` flag on the node and compute display text from it.
- [x] Update [src/MotorEditor.Avalonia/Views/DirectoryBrowserPanel.axaml](src/MotorEditor.Avalonia/Views/DirectoryBrowserPanel.axaml) node template to bind to the dirty-aware display text.
- [x] Ensure updates propagate when:
  - [x] the active file changes
  - [x] dirty state toggles (save/undo back to clean)
  - [x] directory tree refresh occurs

Required hygiene:

- [x] Do not add per-node file parsing or disk I/O to compute the indicator.

### Done when

- AC 3W.5 passes.
- Directory browser selection sync test(s) remain green.

### Files

- [src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs](src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs)
- [src/MotorEditor.Avalonia/ViewModels/DirectoryBrowserViewModel.cs](src/MotorEditor.Avalonia/ViewModels/DirectoryBrowserViewModel.cs)
- [src/MotorEditor.Avalonia/ViewModels/ExplorerNodeViewModel.cs](src/MotorEditor.Avalonia/ViewModels/ExplorerNodeViewModel.cs)
- [src/MotorEditor.Avalonia/Views/DirectoryBrowserPanel.axaml](src/MotorEditor.Avalonia/Views/DirectoryBrowserPanel.axaml)
- Tests (choose smallest appropriate):
  - [tests/CurveEditor.Tests/ViewModels/MainWindowDirectoryBrowserSyncTests.cs](tests/CurveEditor.Tests/ViewModels/MainWindowDirectoryBrowserSyncTests.cs)
  - (optional) new focused unit test for dirty-indicator computation

### Quick manual test

1. Open a folder in the Directory Browser.
2. Open a file from the tree.
3. Make an edit; verify `*` appears next to that file’s name in the tree.
4. Save; verify `*` disappears.

---

## [ ] PR 6: Hardening and Final Validation

### Tasks

- [ ] Confirm all relevant unit tests pass: `dotnet test`.
- [ ] Do an AC-driven manual validation pass (script below).
- [ ] Confirm status messages are consistent and not misleading across cancel paths.
- [ ] Confirm no prompt regressions:
  - [ ] open from directory browser
  - [ ] open from file picker
  - [ ] new file
  - [ ] close file
  - [ ] close app

### Done when

- All acceptance criteria for Phase 3 Wrap-up pass in manual validation.

### Final manual validation script (AC-driven)

1. Create a dirty state (open file, edit) then:
   - attempt File → Open (Cancel must abort)
   - attempt File → New (Cancel must abort)
   - attempt Close File (Cancel must abort)
2. With the file still dirty, attempt app/window close:
   - Cancel must abort close
   - Save then cancel Save As (if applicable) must abort close
3. While dirty, confirm the directory browser shows `*` for the active file; after Save, it disappears.
4. Expand nested folders and confirm indentation is reduced and chevrons are readable.

### Sign-off checklist

- [ ] All tasks across all PR sections are checked `[x]`.
- [ ] All acceptance criteria listed above have a verification step (test or manual script).
- [ ] No out-of-scope features were implemented.
