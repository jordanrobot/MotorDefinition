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

- [ ] Panels that should use this mechanism include:
  - [x] Directory Browser panel
  - [x] Curve Data panel
  - [x] Any future panels that may be added to the application.
  - [x] Motor Properties panel
  - [x] Curve Graph panel
    - [x] The Curve Graph panel should never be fully collapsed, but can be resized to a minimal width.
    - [x] The Curve Graph panel should occupy the main content area.
    - [x] The Curve Graph panel should not participate in the "collapse any other expanded panels" behavior by default.
    - [x] The Curve Graph panel not be represented in the icon bar. Presumably this is by setting a property like `EnableIcon = false` on the panel descriptor.

### Panel Headers and Layout

- [x] All window panels within the CurveEditor application should have a header at the top of each panel. This panel header should contain the name of the panel, using terminology defined in 00-terms-and-definitions.

- [x] Collapsed panels should be fully hidden from view (including any per-panel header area), except for their representation in the Panel Bar.

- [x] The CurveEditor application should have a vertical bar that shows a text label button for each collapsible panel. Call this the Panel Bar.
- [x] Phase 3.0 uses text labels (not icons or glyphs) for Panel Bar items.
- [x] Panel Bar text labels must use the following text (exactly):
  - [x] Motor Properties = "Properties"
  - [x] Curve Data = "Data"
  - [x] Directory Browser = "Browser"
- [x] Clicking a Panel Bar text label expands/collapses the corresponding panel.
- [ ] Panel Bar button backgrounds should be highlighted when the corresponding panel is expanded.
- [ ] Panel Bar button backgrounds should not be highlighted when the corresponding panel is collapsed.
- [ ] Panel Bar button backgrounds should be independent of each other (each Panel Bar Button background highlights based only on its own panel expanded/collapsed state).
- [ ] When multiple panels are expanded across multiple zones (e.g., Left + Right), the Panel Bar must show highlighted backgrounds for all expanded panels at the same time.
- [x] Panel Bar text must be oriented sideways (rotated) so it fits without wrapping.
- [x] This vertical bar with icons should be docked to one side of the main window.
- [x] The vertical bar should not overlap with the main content area of the application.
- [x] This vertical bar should be docked to the left side of the application window by default, but users should be able to change its position to the right side via user settings.
- [x] When the Panel Bar dock side is changed, the Panel Bar actually moves to that side in the layout (and does not merely persist the setting).
- [x] Changing the Panel Bar dock side should not change panel zones. Panel zones are independent of Panel Bar position.
- [x] When docked left or right, the separator/border is drawn on the edge between the Panel Bar and the main content.
- [x] This vertical bar should always be visible.
- [x] Expanding a panel must not collapse panels in other zones.
- [x] If the clicked panel is already expanded, clicking its label should collapse it.
- [x] The size of the vertical bar should be fixed, and should not change when panels are expanded or collapsed.
- [x] Any collapsed panel should be hidden from view, except for its icon in the vertical bar.
  
### Overall Window Layout
- [x] The main window layout should consist of "zones" that correspond to the target areas for panels (e.g., left, right, bottom, center).
- [x] Panels should dock to their designated zones when expanded.
- [ ] Panels may be moved between zones (by the user) in future phases, but for Phase 3.0, each panel should have a fixed zone.

### Panel Behavior in Overall Window Layout
- [x] Collapsible panels should have a zone property that defines which zone of the window they dock to when expanded (e.g., left, right, bottom, center).
- [x] This zone property should persist across application restarts.
- [x] The application should apply the persisted zone at runtime by routing each panel's content to the appropriate zone host.
- [x] When a panel is expanded, it should occupy its designated zone in the window layout.
- [x] When a panel is expanded into a zone, it should not overlap with other expanded panels in that zone.
- [x] When a panel is expanded into a zone, it should collapse any other expanded panels in that zone.
- [x] Curve Data panel must be located in the left zone for Phase 3.0 (not the bottom zone).
- [x] When a panel is collapsed from a zone, the zone should adjust to minimize unused space.

### Persistence and Responsiveness

- [x] The expand/collapse state of each panel should persist across application restarts. If a panel is expanded when the application is closed, it should be expanded when the application is reopened.
- [x] Each expand/collapse panel should have persisted size values that survive restarts:
  - [x] Persisted expanded width (when docked left/right).
  - [x] Persisted expanded height (when docked bottom).
  - [x] Persisted sizes must never "learn" a zero size when a panel is collapsed.
- [x] Users should be able to resize expanded panels via splitters (right edge for left zone, left edge for right zone, top edge for bottom zone).
- [x] When a zone has no expanded panel, that zone's resize splitter must be disabled (it should not resize an empty zone).
- [x] When a zone has an expanded panel, that zone's resize splitter must be enabled and resize the zone normally.
- [x] Defaults (first run / no persisted state):
  - [x] Directory Browser defaults to expanded.
  - [x] Motor Properties defaults to expanded.
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

- [x] AC 3.0.5: After restarting the application, each panel's persisted `Zone` value is restored (and if a persisted zone is invalid/unknown, the app falls back to the default zone without user-facing errors).
- [x] AC 3.0.6: The Panel Bar is always visible, fixed-width, and never overlaps the main content (verified for both left-docked and right-docked configurations).
- [x] AC 3.0.7: Zone exclusivity is enforced: expanding a panel collapses any other expanded panel in the same zone, and does not collapse panels in other zones.
- [x] AC 3.0.8: The Curve Graph panel is not represented in the Panel Bar (`EnableIcon = false`), and the Curve Graph remains visible in the center zone at all times.
- [x] AC 3.0.9: Collapsing a panel shrinks its zone to minimize unused space (no persistent blank gutter/stripe beyond the Panel Bar itself).
- [x] AC 3.0.10: Collapsing and re-expanding a panel restores the last non-zero size for that panel (collapse does not permanently "learn" a zero size).
- [x] AC 3.0.11: When a zone has no expanded panel, its splitter is disabled; when a zone has an expanded panel, its splitter is enabled and resizes the zone.



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

### Curve Data Panel: Layout

- [x] Curve Data Grid should occupy the entirety of the Curve Data Panel, and resize as the panel is resized.

### Open Questions (Phase 3.2)

- Q 3.2.1: Which Curve Data Panel behaviors, if any, are planned for Phase 3.2 versus later phases (e.g., advanced editing vs. read-only enhancements)?