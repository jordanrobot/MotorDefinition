<!-- markdownlint-disable-file -->
# Directory Browser Rename UX Details

## T1: Rename initiation focus and selection

- When rename begins (context menu or command), focus the inline rename TextBox automatically.
- Select the filename text for immediate editing (full name is acceptable; preserve extension handling if straightforward).
- Store the original display name so cancellation or failed rename can restore it.
- Avoid toggling expansion when interacting with the rename editor.

## T2: Rename completion and cancellation

- End rename when the user presses Enter, Tab, or clicks elsewhere in the UI, keeping the edited name and performing the file system rename.
- Escape cancels rename and restores the original name without changing the file system.
- Enter should not collapse the current directory node.
- After completion or cancellation, exit rename mode and refresh tree state as needed.

## T3: Tests

- Add or update unit/integration coverage to assert rename initiation sets renaming state and that completion/cancel paths invoke file rename or revert.
- Prefer ViewModel-level tests that do not require UI rendering where possible.
