## Phase 3.0 Functional Requirements: Generic Panel Expand/Collapse

### Scope (Phase 3.0)

- Introduce a generic, reusable expand/collapse mechanism for all major panels in the main CurveEditor window.
- Provide a VS Code–style vertical bar for panel icons and quick toggling.
- Persist panel visibility and width settings across sessions via user settings.

### Non-goals (Phase 3.0)

- Do not redesign the internal content of individual panels (Directory Browser, Curve Data, etc.).
- Do not introduce per-document layout variants; panel expand/collapse is global per user/settings, not per file.
- Do not introduce undo/redo for layout changes (panel layout changes are not part of the command history).

### CurveEditor Panel Expand/Collapse Mechanism

- [ ] The CurveEditor application should have a generic expand/collapse mechanism for window panels. Currently there is an existing expand/collapse mechanism, but it is limited and will be replaced with the following mechanism. This new mechanism should allow users to expand or collapse individual panels within the application. This mechanism should be implemented similar to the way Visual Studio Code handles its side panels. Please refer to the Visual Studio Code codebase for an example of the rough functionality being targeted: https://github.com/microsoft/vscode/tree/main

- [ ] Panels that should use this mechanism include:
  - [ ] Directory Browser panel
  - [ ] Curve Data panel
  - [ ] Any future panels that may be added to the application.
  - [ ] Motor Properties panel
  - [ ] Curve Graph panel
    - [ ] The Curve Graph panel should never be fully collapsed, but can be resized to a minimal width.
    - [ ] The Curve Graph panel should occupy the main content area.
    - [ ] The Curve Graph panel should not participate in the "collapse any other expanded panels" behavior by default.
    - [ ] The Curve Graph panel not be represented in the icon bar. Presumably this is by setting a property like `EnableIcon = false` on the panel descriptor.

### Panel Headers and Layout

- [ ] All window panels within the CurveEditor application should have a header at the top of each panel. This panel header should contain the name of the panel, using terminology defined in 00-terms-and-definitions.

- [ ] The CurveEditor application should have a vertical bar that shows an icon for each collapsible panel. Call this the Panel Bar.
- [ ] This vertical bar with icons should be docked to one side of the main window.
- [ ] The vertical bar should not overlap with the main content area of the application.
- [ ] This vertical bar should be docked to the left side of the application window by default, but users should be able to change its position to the right side via user settings.
- [ ] This vertical bar should always be visible.
- [ ] Clicking on a panel icon in the vertical bar should expand that panel, and collapse any other expanded panels represented in the vertical bar.
  - [ ] This "collapse any other expanded panels" behavior applies only to panels with `EnableCollapse = true`.
  - [ ] Initially, `EnableCollapse = true` for Directory Browser, Motor Properties, and Curve Data.
  - [ ] Initially, `EnableCollapse = false` for Curve Graph.
  - [ ] See ### Panel Behavior in Overall Window Layout below for more details related to zones and collapse/expand behavior.
- [ ] If the clicked panel is already expanded, clicking its icon should collapse it.
- [ ] The size of the vertical bar should be fixed, and should not change when panels are expanded or collapsed.
- [ ] Any collapsed panel should be hidden from view, except for its icon in the vertical bar.
  
### Overall Window Layout
- [ ] The main window layout should consist of "zones" that correspond to the target areas for panels (e.g., left, right, bottom, center).
- [ ] Panels should dock to their designated zones when expanded.
- [ ] Panels may be moved between zones (by the user) in future phases, but for Phase 3.0, each panel should have a fixed zone.

### Panel Behavior in Overall Window Layout
- [ ] Collapsible panels should have a zone property that defines which zone of the window they dock to when expanded (e.g., left, right, bottom, center).
- [ ] This zone property should persist across application restarts.
- [ ] When a panel is expanded, it should occupy its designated zone in the window layout.
- [ ] When a panel is expanded into a zone, it should not overlap with other expanded panels in that zone.
- [ ] When a panel is expanded into a zone, it should collapse any other expanded panels in that zone.
- [ ] When a panel is collapsed from a zone, the zone should adjust to minimize unused space.

### Persistence and Responsiveness

- [ ] The expand/collapse state of each panel should persist across application restarts. If a panel is expanded when the application is closed, it should be expanded when the application is reopened.
- [ ] Each expand/collapse panel should have a unique width that persists across application restarts. Users should be able to resize the width of each expanded panel by dragging its right edge.
- [ ] The expand/collapse mechanism should be implemented in a way that allows for easy addition of new panels in the future.
- [ ] The expand/collapse mechanism should be responsive and should not cause any noticeable lag or delay when expanding or collapsing panels.
- [ ] The expand/collapse mechanism should be visually appealing and should use smooth animations when expanding or collapsing panels.

### Acceptance Criteria (Phase 3.0)

- AC 3.0.1: After restarting the application, all panel visibility and widths match the last session for the same user profile/settings file.
- AC 3.0.2: Expanding or collapsing any supported panel via the icon bar completes within a reasonable time on a typical development machine (exact performance target to be confirmed; e.g., under 150 ms for animation plus layout update).
- AC 3.0.3: Adding a new panel type requires only minimal configuration (e.g., registering a name, icon, and content) without changes to core layout logic.
- AC 3.0.4: Layout changes (panel expand/collapse, width changes) do not participate in the undo/redo history.

- AC 3.0.5: After restarting the application, each panel's persisted `Zone` value is restored (and if a persisted zone is invalid/unknown, the app falls back to the default zone without user-facing errors).
- AC 3.0.6: The Panel Bar is always visible, fixed-width, and never overlaps the main content (verified for both left-docked and right-docked configurations).
- AC 3.0.7: Panel Bar exclusivity is enforced: expanding any Panel Bar panel collapses any other currently expanded Panel Bar panel (including when the panels belong to different zones).
- AC 3.0.8: The Curve Graph panel is not represented in the Panel Bar (`EnableIcon = false`), and the Curve Graph remains visible in the center zone at all times.
- AC 3.0.9: Collapsing a panel shrinks its zone to minimize unused space (no persistent blank gutter/stripe beyond the Panel Bar itself).
- AC 3.0.10: Collapsing and re-expanding a panel restores the last non-zero size for that panel (collapse does not permanently "learn" a zero size).



## Phase 3.1 Functional Requirements: Directory Browser

### Directory Browser: General

### Directory Browser: file browser behavior

- [ ] Only show folders and valid curve definition files in the directory listing.
  - [ ] To make this efficient, make an initial file list containing only directories and JSON files, then validate each file in a background task to filter out invalid files.

- [ ] Browser directory listing and navigation should work like VS Code's file browser.
  - [ ] Show directories in a tree view on the left side, allowing navigation into subdirectories via expansion/collapse.
  - [ ] Directories should be expandable/collapsible via caret icons to the left of the directory name.
  - [ ] Clicking on a directory expansion icon will expand or collapse the directory.
  - [ ] Directories should be sorted alphabetically, with folders listed before files.
  - [ ] Show valid curve definition files in the selected directory in the tree (single unified tree), not in a separate pane.
  - [ ] Single clicking on a file will open it in the curve editor.

  - [ ] Clicking on a directory name will expand/collapse it, rather than selecting it.

  - [ ] Directories and files should be displayed in the same tree; remove the two-pane view. It should look roughly like VS Code's file explorer pane.

  - [ ] Files and directories within a parent directory should be shown as children of that directory in the tree view. They should be indented to indicate they are children. Please see the example below for reference:

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

- [ ] The top level directory in the directory browser should not participate in the expand/collapse mechanism. It should always be expanded and does not have a caret icon.

### Directory Browser: Behavior

- [ ] When the program starts, automatically open the last opened file in the curve editor.
- [ ] By default, the directory browser should be collapsed when the program starts.
- [ ] When the user opens a directory, automatically expand the directory tree to show the opened directory.
- [ ] Implement a "Close Directory" button to close the currently opened directory in the directory browser.
- [ ] When the command "Close Directory" is executed, collapse the directory tree to hide the closed directory.
- [ ] When the program starts, automatically open the last opened directory in the directory browser, unless the user had explicitly closed it before exiting the program.
- [ ] When the program starts, remember the expanded/collapsed state of directories in the directory browser from the last session.
- [ ] When the program starts, if the last opened directory no longer exists, collapse the directory browser.
- [ ] Directory browser width should persist in user settings.

### Acceptance Criteria (Phase 3.1)

- AC 3.1.1: On restart, if the last opened directory still exists, its expand/collapse state and the directory browser width are restored.
- AC 3.1.2: If the last opened directory no longer exists, the directory browser starts collapsed with no errors shown to the user beyond any appropriate log entry.
- AC 3.1.3: Clicking a file once in the directory browser always opens it in the CurveEditor, and the selection in the tree matches the active motor definition.


### Directory Browser: UI
- [ ] Ensure there is an Open Folder button added to the File menu.
- [ ] Ensure there is an Close Folder button added to the main toolbar for easy access.
- [ ] Add a "Refresh Explorer" icon button at the top of the file browser to re-scan the directory tree for files.
- [ ] Implement a keyboard shortcut (F5) to trigger "Refresh Explorer".


### Directory Browser: Text display
- [ ] The text within the directory browser should use a monospace font for better alignment.
- [ ] The text size within the directory browser should persist in user settings.
- [ ] The text size within the directory browser should be adjustable via keyboard shortcuts (e.g., Ctrl+Plus to increase, Ctrl+Minus to decrease).
- [ ] The text size within the directory browser should be adjustable via mouse wheel while holding the Ctrl key.
  - [ ] Ctrl+Mouse Wheel Up increases text size.
  - [ ] Ctrl+Mouse Wheel Down decreases text size.
- [ ] The text within the directory browser should not wrap; long file and directory names should be truncated with ellipses if they exceed the available width.


## Phase 3.2 Functional Requirements: Curve Data Panel

These requirements will be added in a future update.

### Open Questions (Phase 3.2)

- Q 3.2.1: Which Curve Data Panel behaviors, if any, are planned for Phase 3.2 versus later phases (e.g., advanced editing vs. read-only enhancements)?