## Phase 3.0.5 Subtasks: VS Code UI Styling Pass (Agent Execution Checklist)

### Purpose
- Provide a PR-sliceable task list for implementing Phase 3.0.5 with minimal churn.
- Centralize UI styling work so colors/brushes are consistent and future adjustments are easy.

### Scope Reminder (Phase 3.0.5)
- Adjust UI colors and styling so CurveEditor matches the screenshot: [.github/planning/vs-code-ui.png](.github/planning/vs-code-ui.png).
- Styling-only phase: do not change panel behavior, persistence behavior, or expand/collapse logic.
- Keep the existing `FluentTheme`; layer CurveEditor-specific resources/styles on top.

### Key Files (Expected touch points)
- Theme and resources: [src/CurveEditor/App.axaml](src/CurveEditor/App.axaml)
- Panel Bar styling: [src/CurveEditor/Views/PanelBar.axaml](src/CurveEditor/Views/PanelBar.axaml)
- Panel containers/headers/splitters: [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml)
- Optional alignment:
  - Chart colors: [src/CurveEditor/Views/ChartView.axaml](src/CurveEditor/Views/ChartView.axaml)
  - Curve data styling: [src/CurveEditor/Views/CurveDataPanel.axaml](src/CurveEditor/Views/CurveDataPanel.axaml)

### Inputs
- Screenshot reference: [.github/planning/vs-code-ui.png](.github/planning/vs-code-ui.png)
- Plan reference: [.github/planning/phase-3.0.5-plan-vscode-ui-styling.md](.github/planning/phase-3.0.5-plan-vscode-ui-styling.md)

---

## [ ] PR 0: Prepare styling token map (no UI changes)

### Tasks
- [ ] Identify which VS Code chrome elements the screenshot represents (Activity Bar, Side Bar, separators, section headers).
- [ ] Decide and lock down the CurveEditor resource key names for these tokens.
- [ ] Capture screenshot color values (via pipette tool) for:
  - [ ] Activity Bar background
  - [ ] Activity Bar hover background
  - [ ] Activity Bar active background
  - [ ] Activity Bar foreground
  - [ ] Side Bar background
  - [ ] Side Bar section header background
  - [ ] Separator/splitter/border color
  - [ ] Main editor background (if visible)

### Done when
- The token names + color values are ready to be added to a resource dictionary.

---

## [ ] PR 1: Add CurveEditor VS Code-style resource dictionary

### Tasks
- [ ] Add a resource dictionary file, e.g. `src/CurveEditor/Styles/VsCodeTheme.axaml`.
- [ ] Define CurveEditor brush resources for:
  - [ ] Activity Bar (Panel Bar) backgrounds/foregrounds (normal/hover/active)
  - [ ] Side Bar backgrounds/foregrounds
  - [ ] Section header background/foreground
  - [ ] Splitter/separator brush
- [ ] Add Light and Dark variants (via `ThemeVariant`) if the screenshot implies a dark theme but the app can run in light mode.
- [ ] Include the dictionary from [src/CurveEditor/App.axaml](src/CurveEditor/App.axaml) after `FluentTheme`.

### Done when
- App compiles and runs.
- No direct hard-coded colors are introduced into view files for the new look.

---

## [ ] PR 2: Panel Bar styling parity

### Tasks
- [ ] Update [src/CurveEditor/Views/PanelBar.axaml](src/CurveEditor/Views/PanelBar.axaml) to use CurveEditor resources instead of `SystemControl*` resources for:
  - [ ] Panel Bar background and border
  - [ ] Button hover background
  - [ ] Button pressed background
  - [ ] Button active background + active foreground
- [ ] Ensure “active” state is expressed through **button background highlighting** (not only text styling).
- [ ] Ensure hover/pressed/active visuals match the screenshot.

### Done when
- Panel Bar resembles the screenshot’s Activity Bar.
- Active background highlight is visible and independent per item.

---

## [ ] PR 3: Side panels, headers, and separators styling parity

### Tasks
- [ ] Update [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml):
  - [ ] Apply side panel background/border tokens to panel containers.
  - [ ] Update panel headers to match VS Code section header styling.
  - [ ] Apply separator/splitter token to splitters and borders.
- [ ] Replace purely cosmetic hard-coded foregrounds (e.g., placeholder `Gray`) with theme resources where appropriate.

### Done when
- Side panel chrome resembles the screenshot’s Side Bar.
- Section headers look like VS Code section headers.

---

## [ ] PR 4 (optional): Chart + DataGrid theme alignment

### Tasks
- [ ] If chart appearance conflicts with the screenshot theme, update [src/CurveEditor/Views/ChartView.axaml](src/CurveEditor/Views/ChartView.axaml) to use theme resources for axis labels/grid/foreground rather than hard-coded colors.
- [ ] If Curve Data visuals clash, update [src/CurveEditor/Views/CurveDataPanel.axaml](src/CurveEditor/Views/CurveDataPanel.axaml) background and borders to use theme resources.

### Done when
- The chart/grid no longer look “off palette” compared to the screenshot.

---

## [ ] PR 5: Clean up remaining hard-coded colors (chrome-only)

### Tasks
- [ ] Remove/replace remaining hard-coded colors in the main window chrome and panel chrome:
  - [ ] `Foreground="Orange"` / `Foreground="Gray"` in main window UI chrome.
  - [ ] `#33FFFFFF` background usage if it conflicts with the screenshot palette.
- [ ] Do not change intentional domain colors (e.g., series colors) unless they are also part of the screenshot target.

### Done when
- Main window chrome uses theme resources consistently.

---

## [ ] PR 6: Validation and screenshot parity

### Tasks
- [ ] Manual pass with the screenshot open side-by-side:
  - [ ] Activity Bar (Panel Bar) background/hover/active match.
  - [ ] Side Bar background/section headers match.
  - [ ] Separators/splitters match.
  - [ ] Text foregrounds are readable and consistent.
- [ ] Verify both Panel Bar dock sides (left and right) still look correct.
- [ ] Verify no behavior regressions (expand/collapse, persistence, commands).

### Done when
- Styling matches [.github/planning/vs-code-ui.png](.github/planning/vs-code-ui.png) closely.
- No functional regressions.
