## Curve Editor Phase 3 Wrap-up Plan: Explorer Polish + Unsaved Changes UX

### Status

Draft (planning)

### Goal

Finish the remaining Phase 3 UX items:

- [ ] Reduce indentation in the Directory Browser tree.
- [x] Show a dirty indicator (`*`) in the directory list for the active dirty file.
- [ ] Ensure consistent Save/Don’t Save/Cancel prompts when the user is about to lose unsaved changes:
  - [ ] closing the app/window
  - [x] opening another file (both via directory browser and via file picker)

### Progress Checklist (PR-slice)

- [x] PR 1 — Standardize the Unsaved Changes Dialog
- [x] PR 2 — Add Dirty Prompt to File Picker Open + New File
- [x] PR 3 — Prompt on App/Window Close
- [x] PR 4 — Dirty Indicator in Directory Browser

### Current Baseline (What’s Already There)

Unsaved changes prompt plumbing

- There is a dedicated dialog and model for Save/Ignore/Cancel:
  - `src/MotorEditor.Avalonia/Views/UnsavedChangesDialog.axaml`
  - `src/MotorEditor.Avalonia/Views/UnsavedChangesDialog.axaml.cs`
  - `MainWindowViewModel.UnsavedChangesChoice` and `UnsavedChangesPromptAsync`
- “Open another file from the directory browser” already prompts when dirty:
  - `MainWindowViewModel.OpenMotorFileFromDirectoryBrowserAsync`
- “Close current file” already prompts when dirty:
  - `MainWindowViewModel.CloseFileAsync`
- There are existing unit tests covering these flows:
  - `tests/CurveEditor.Tests/ViewModels/MainWindowDirectoryBrowserDirtyOpenTests.cs`
  - `tests/CurveEditor.Tests/ViewModels/MainWindowViewModelCloseFileTests.cs`

Gaps / inconsistencies

- Opening a file via the file picker now prompts when dirty:
  - `MainWindowViewModel.OpenFileAsync`
- Creating a new file now uses the same Save/Don’t Save/Cancel prompt:
  - `MainWindowViewModel.NewMotorAsync`
- Window/application close currently persists layout state, but does not appear to block closing on unsaved changes:
  - `src/MotorEditor.Avalonia/Views/MainWindow.axaml.cs` registers `Closing += ...` for persistence
- The Unsaved Changes dialog button label is “Don’t Save” (enum value remains `Ignore`).

Directory Browser UI

- The tree is implemented as a `TreeView` with a custom node template:
  - `src/MotorEditor.Avalonia/Views/DirectoryBrowserPanel.axaml`
- Nodes are `ExplorerNodeViewModel` with `DisplayName`, `FullPath`, etc:
  - `src/MotorEditor.Avalonia/ViewModels/ExplorerNodeViewModel.cs`
- No “dirty file” indicator exists in the explorer node model/template today.

### Scope

In scope

- Adjust Directory Browser indentation to be visually closer to ~2–3 monospace characters per nesting level.
- Append `*` to the displayed filename in the Directory Browser list for the currently-open file when it is dirty.
- Normalize unsaved-changes prompts across:
  - “Open file” (file picker)
  - “Open file from directory browser” (already prompts)
  - “New file”
  - “Close app/window”

Out of scope

- Adding new UI surfaces or additional prompt types.
- Adding autosave.
- Any redesign of the explorer beyond indentation and the dirty `*` indicator.

### Design / UX Requirements

Save prompts

- [ ] The prompt must offer exactly: Save / Don’t Save / Cancel.
- [ ] Text must be centered within each button.
- [ ] Cancel aborts the initiating action:
  - [ ] abort opening another file
  - [ ] abort closing the app/window
- [ ] If “Save” is chosen and the save is cancelled/fails (still dirty afterwards), abort the initiating action.

Dirty indicator

- [ ] The explorer item for the currently-open file should show `*` appended when the editor has unsaved changes.
- [ ] Only the active file needs the indicator (not every changed sub-entity).

Indentation

- [ ] Reduce indentation so nested folders/files are closer to the parent level.
- [ ] Keep chevrons aligned and readable.

### Proposed Implementation Approach

#### PR 1 — Standardize the Unsaved Changes Dialog

Goal: Ensure the dialog wording matches the spec and can be reused everywhere.

- Update `UnsavedChangesDialog.axaml`:
  - Rename “Ignore” button text to “Don’t Save”.
  - Keep the enum value as `Ignore` (no API churn needed) but treat it as “don’t save”.
- Confirm the message copy still reads well for different action descriptions (e.g., “open another file”, “close the app”).

Validation

- Manual: trigger the prompt from the directory browser open flow and confirm the button text + semantics.
- Automated: existing tests should still pass.

#### PR 2 — Add Dirty Prompt to File Picker Open + New File

Goal: Any “open” path should behave consistently.

- Update `MainWindowViewModel.OpenFileAsync` to check `IsDirty` before opening:
  - Use `UnsavedChangesPromptAsync("open another file")` (or a more specific action string).
  - Respect Cancel and Save semantics.
- Update `MainWindowViewModel.NewMotorAsync`:
  - Replace the `MessageDialog` usage with `UnsavedChangesPromptAsync("create a new file")`.
  - Keep behavior consistent with other flows.

Validation

- Add unit tests for the new logic by extracting the “confirm unsaved changes” branch into a testable helper (recommended), or by adding tests around a new internal method that performs the dirty-check flow.
- Manual: verify Open (file picker) prompts and honors Cancel.

#### PR 3 — Prompt on App/Window Close

Goal: Closing the window (app exit) should offer Save/Don’t Save/Cancel.

Implementation options (choose the simplest that fits Avalonia’s lifecycle):

1. Handle `MainWindow.Closing` in `MainWindow.axaml.cs` and cancel the close when needed.
   - This is likely simplest if `Closing` provides cancelable args in the current Avalonia version.
   - Use the ViewModel’s `UnsavedChangesPromptAsync("close the app")`.

2. Handle `IClassicDesktopStyleApplicationLifetime.ShutdownRequested` in `App.axaml.cs` and cancel shutdown.
   - Prefer this if the window-level Closing event is not reliably cancelable.

Details

- If no file is open, do not prompt.
- If dirty:
  - Cancel → abort close
  - Save → call `SaveAsync`; if still dirty, abort close
  - Don’t Save → proceed with close

Validation

- Manual: modify a file, attempt to close the window, verify Cancel blocks exit.
- Optional: add a ViewModel-level helper method (e.g., `ConfirmLoseUnsavedChangesAsync("close the app")`) to enable unit testing without UI.

#### PR 4 — Dirty Indicator in Directory Browser

Goal: Show `*` in the directory list for the active dirty file.

Recommended approach

- Add two properties on `DirectoryBrowserViewModel`:
  - `ActiveFilePath` (string?)
  - `IsActiveFileDirty` (bool)
- Update them from `MainWindowViewModel` whenever `CurrentFilePath` or `IsDirty` changes.
- Add an `IsActiveFile` and/or `DisplayNameWithDirtyIndicator` capability on `ExplorerNodeViewModel`.
  - Implementation detail: compute “dirty star” in ViewModel rather than XAML converters.
- Update the node template in `DirectoryBrowserPanel.axaml` to bind to the “dirty-aware” display text.

Validation

- Manual: open a file, edit it, confirm the selected file’s name shows `*` in the explorer; save it, confirm `*` disappears.
- Automated: add a small unit test that sets ActiveFilePath + IsActiveFileDirty and asserts the computed display text for the active node.

### Roll-up Testing

- `dotnet test` (ensure existing tests remain green).
- Manual smoke:
  - Open file from directory browser while dirty (prompt + Cancel)
  - Open file from Open dialog while dirty (prompt + Cancel)
  - New file while dirty (prompt + Cancel)
  - Close app while dirty (prompt + Cancel)
  - Dirty indicator appears/disappears as expected

### Risks / Notes

- App-close prompting is the most lifecycle-sensitive item; if cancellation is tricky at the window layer, use application lifetime shutdown handling.
- Keep the prompt implementation centralized to avoid divergence between the different “lose changes” entry points.
