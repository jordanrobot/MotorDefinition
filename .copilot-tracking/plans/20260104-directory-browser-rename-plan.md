<!-- markdownlint-disable-file -->
# Directory Browser Rename UX Plan

## Scope

- Align the Directory Browser rename interaction with Windows Explorer-style behavior described in the issue.
- Maintain existing commands and selection behavior while enhancing rename UX.

## Tasks

- [x] T1: Improve rename initiation to focus the inline editor, pre-select the name, and retain the original value for cancel/revert.
- [x] T2: Handle rename completion and cancellation inputs (Enter/Tab/click outside commit; Escape reverts) without collapsing nodes.
- [x] T3: Add/update targeted tests covering rename initiation and completion flows.
- [x] T4: Update tracking changes file after each task and capture final artifacts (tests/screenshots).

## References

- Details: `.copilot-tracking/details/20260104-directory-browser-rename-details.md`
- Code areas: `src/MotorEditor.Avalonia/ViewModels/DirectoryBrowserViewModel.cs`, `src/MotorEditor.Avalonia/Views/DirectoryBrowserPanel.axaml*`, `src/MotorEditor.Avalonia/Services/RenameCommand.cs`.
