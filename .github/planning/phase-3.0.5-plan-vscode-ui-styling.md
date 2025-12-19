## Curve Editor Phase 3.0.5 Plan: VS Code UI Styling Pass

### Goal
Adjust CurveEditor’s main-window styling (Panel Bar, panel headers/containers, splitters, and related chrome) so it visually matches the provided screenshot: [.github/planning/vs-code-ui.png](.github/planning/vs-code-ui.png).

This is a styling-only phase. It should not change layout behavior, persistence behavior, or panel expand/collapse logic from Phase 3.0.

### Non-goals
- No new UI features (no new panels, no drag/drop, no new layout behaviors).
- No new settings UI unless required for styling parity.
- No content redesign inside panels (Directory Browser placeholder, Curve Data grid, Properties fields).

### Prerequisites (must be true before Phase 3.0.5 is “done”)
- Phase 3.0 Panel Bar behavior is complete:
  - Panel Bar can be docked left or right and actually moves in the layout.
  - Panel Bar can show multiple highlighted buttons simultaneously when multiple zones have expanded panels (e.g., Browser + Properties).

### Current Baseline (observed)
- App uses `FluentTheme` and relies on `SystemControl*` dynamic resources ([src/CurveEditor/App.axaml](src/CurveEditor/App.axaml)).
- Panel Bar is a dedicated view ([src/CurveEditor/Views/PanelBar.axaml](src/CurveEditor/Views/PanelBar.axaml)) and already has pointerover/pressed/active style selectors.
- Several views still contain hard-coded colors (`Orange`, `Gray`, `#33FFFFFF`, `#FF5050`, chart colors) which can prevent a cohesive VS Code-like look.

### Design Approach

#### 1) Treat the screenshot as a “token source”
VS Code themes are effectively a set of named tokens (Activity Bar, Side Bar, Panel borders, etc.). We’ll replicate this in CurveEditor by introducing a small set of CurveEditor-specific theme resources and then wiring existing views to those resources.

Key idea: centralize all new colors into one resource dictionary and reference them everywhere via `DynamicResource`. This avoids scattered per-control color values and keeps future adjustments cheap.

#### 2) Keep FluentTheme, override selectively
We should continue using `FluentTheme` (it provides most control templates and accessibility behavior) and then layer CurveEditor-specific resources/styles on top.

### Resource Token Map (target)

Add a CurveEditor resource dictionary with resources that correspond to what the screenshot shows. Names below are suggested; actual names can be adjusted to match existing conventions.

- **Activity Bar (Panel Bar)**
  - `CurveEditor.ActivityBar.BackgroundBrush`
  - `CurveEditor.ActivityBar.BorderBrush`
  - `CurveEditor.ActivityBar.Item.ForegroundBrush`
  - `CurveEditor.ActivityBar.Item.HoverBackgroundBrush`
  - `CurveEditor.ActivityBar.Item.ActiveBackgroundBrush`
  - `CurveEditor.ActivityBar.Item.ActiveForegroundBrush`

- **Side Bar / Panels**
  - `CurveEditor.SideBar.BackgroundBrush`
  - `CurveEditor.SideBar.BorderBrush`
  - `CurveEditor.SideBar.SectionHeader.BackgroundBrush`
  - `CurveEditor.SideBar.SectionHeader.ForegroundBrush`

- **Main Editor (Center)**
  - `CurveEditor.Editor.BackgroundBrush`
  - `CurveEditor.Editor.BorderBrush`

- **Splitters / separators**
  - `CurveEditor.Splitter.Brush`

- **Status bar / Menu** (only if screenshot includes them and they visibly differ)
  - `CurveEditor.StatusBar.BackgroundBrush`
  - `CurveEditor.StatusBar.ForegroundBrush`

### How to get the actual colors
Because the screenshot is the source of truth, extract colors using a pipette tool (any image editor) and capture:
- Activity Bar background
- Activity Bar active item background
- Activity Bar hover background
- Side Bar background
- Side Bar section header background
- Border/separator color
- Default foreground text color in side areas

Record these values in the plan’s follow-up PR description (or in the resource dictionary as comments).

### Implementation Strategy (PR-sliceable)

#### PR A: Central theme resources (no layout changes)
1. Add a Styles include file, e.g.:
  - `src/CurveEditor/Styles/VsCodeTheme.axaml`
2. Implement it as a `<Styles>` file containing centralized brush resources (tokens) under `Styles.Resources`.
3. Support both Dark and Light modes using a theme dictionary pattern (e.g., `ThemeDictionaries` keyed by `Dark` and `Light`).
4. Include the Styles file from [src/CurveEditor/App.axaml](src/CurveEditor/App.axaml) after `FluentTheme` so overrides apply.

**Done when**
- App compiles.
- No visual regressions besides intended color changes (once wired in subsequent PRs).

#### PR B: Panel Bar styling parity
1. Update [src/CurveEditor/Views/PanelBar.axaml](src/CurveEditor/Views/PanelBar.axaml):
   - Replace `SystemControl*` brushes with CurveEditor Activity Bar brushes.
   - Ensure the “active” state is expressed through **button background** (per Phase 3.0 requirements), not only text.
   - Ensure hover/pressed backgrounds match screenshot.
   - Ensure default foreground and active foreground match screenshot.
2. Verify that the Panel Bar background matches the panel header background where the requirements call for it.

Docking detail:
- Ensure the Panel Bar separator/border is drawn on the edge between the Panel Bar and the main content for both left-docked and right-docked configurations.

**Done when**
- Panel Bar background/hover/active visuals match the screenshot.
- Active item is obvious and uses background highlight.

#### PR C: Side panels + headers styling parity
1. Update [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml):
   - Replace the `TextBlock.PanelHeader` background usage with a header style that matches VS Code section headers.
   - Ensure the **entire header area** uses the header background brush (often easiest via a `Border` wrapper + `TextBlock`).
   - Apply consistent side panel background and border/separator resources.
2. Replace hard-coded foregrounds that conflict with the theme where they are purely cosmetic (e.g. `Foreground="Gray"` placeholders) with a theme resource.

**Done when**
- Side panels and their headers visually match screenshot’s side bar + section header appearance.
- No panel content behavior changes.

#### PR D (optional but recommended): Chart + DataGrid theme alignment
Only do this if the screenshot includes the center “editor” look and the chart clashes.

1. Update [src/CurveEditor/Views/ChartView.axaml](src/CurveEditor/Views/ChartView.axaml) to avoid hard-coded axis/label colors; bind to CurveEditor editor/foreground resources.
2. Revisit [src/CurveEditor/Views/CurveDataPanel.axaml](src/CurveEditor/Views/CurveDataPanel.axaml) background `#33FFFFFF` and align DataGrid/rows to the side bar/editor palette.

**Done when**
- Chart and grid no longer look “off palette” compared to screenshot.

### Validation Checklist (manual)
- Panel Bar background and border match screenshot.
- Panel Bar items:
  - Hover background matches screenshot.
  - Active item uses background highlight.
  - Active highlight is independent per item.
- Side panel background matches screenshot.
- Panel headers match screenshot section header styling.
- Splitters and borders read like VS Code separators (subtle, not heavy).
- ThemeVariant switching:
  - If running in Dark mode, Dark resources apply.
  - If running in Light mode, Light resources apply.

### Expected Files to Touch
- [src/CurveEditor/App.axaml](src/CurveEditor/App.axaml)
- [src/CurveEditor/Views/PanelBar.axaml](src/CurveEditor/Views/PanelBar.axaml)
- [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml)
- (optional) [src/CurveEditor/Views/ChartView.axaml](src/CurveEditor/Views/ChartView.axaml)
- (optional) [src/CurveEditor/Views/CurveDataPanel.axaml](src/CurveEditor/Views/CurveDataPanel.axaml)
