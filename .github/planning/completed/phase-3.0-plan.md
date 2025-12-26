## Curve Editor Phase 3.0 Plan: Generic Panel Expand/Collapse

### Goal
- Replace the existing limited expand/collapse behavior with a VS Code–style panel system driven by a reusable panel descriptor model.
- Introduce a fixed-width, always-visible vertical Panel Bar for toggling panels.
- Implement a zone-based layout model (left/right/bottom/center) with persisted panel sizes and persisted panel zone assignment.
- Ensure layout changes do not participate in undo/redo.

### Requirements Snapshot (from Phase 3 requirements)
- Panels have headers with the panel name.
- Panel Bar:
  - Always visible, fixed size, docked left by default.
  - Dock side (left/right) is user-configurable and persisted.
  - Dock side changes do not change panel zones.
  - Uses rotated text labels (not icons/glyphs): "Browser", "Properties", "Data".
  - Clicking a label expands/collapses the corresponding panel.
  - Each Panel Bar button background highlights when its panel is expanded (and is not highlighted when collapsed).
  - Panel Bar button background highlighting is independent per item.
  - Expanding a panel collapses other panels only within the same zone.
- Overall layout uses zones; for Phase 3.0 each panel has a fixed zone, but the zone assignment is still persisted for forward compatibility.
- Curve Graph:
  - Occupies the main content area.
  - Never fully collapses (but can shrink as other panels expand).
  - Does not participate in "collapse any other expanded panels" by default.
  - Is not represented in the Panel Bar (via a descriptor property such as `EnableIcon = false`).

Clarifications for Phase 3.0:
- Phase 3.0 does not require expand/collapse animations.
- Phase 3.0 uses text labels (no icons/glyph characters).

### Current Baseline
- [Main window layout](src/CurveEditor/Views/MainWindow.axaml) already maps well to zones: left column (browser), center (chart + curve data row), right column (properties).
- Existing persistence helpers exist (view-driven) per [ADR-0004](docs/adr/adr-0004-layout-and-panel-persistence.md).
- Existing global shortcuts exist per [ADR-0005](docs/adr/adr-0005-keyboard-shortcuts-and-input-routing.md).

### Proposed Design

### 1) Panel descriptors (core extensibility point)
Introduce a lightweight descriptor model so adding a panel is "register config + provide content":
- `PanelId` (stable string)
- `DisplayName`
- `PanelBarLabel` (text shown in the Panel Bar; Phase 3.0 uses labels only)
- `EnableIcon` (false for Curve Graph)
- `EnableCollapse` (false for Curve Graph by default)
- `Zone` (left/right/bottom/center)
- `DefaultWidth` (used when the panel is in a left/right zone)
- `DefaultHeight` (used when the panel is in a bottom zone)
- `MinSize` (optional; mostly relevant for ensuring the center graph never hits 0)

Descriptor list (Phase 3.0 initial set):
- Directory Browser: `EnableIcon=true`, `EnableCollapse=true`, `Zone=Left`, `PanelBarLabel="Browser"`
- Motor Properties: `EnableIcon=true`, `EnableCollapse=true`, `Zone=Right`, `PanelBarLabel="Properties"`
- Curve Data: `EnableIcon=true`, `EnableCollapse=true`, `Zone=Left`, `PanelBarLabel="Data"`
- Curve Graph: `EnableIcon=false`, `EnableCollapse=false`, `Zone=Center`

### 2) Zone-based layout model
Represent the window as four zones:
- Left zone (collapsible)
- Right zone (collapsible)
- Bottom zone (collapsible)
- Center zone (always present; Curve Graph)

Phase 3.0 constraint:
- Zones are fixed per panel (no drag/drop or UI to reassign), but the descriptor still has `Zone` and it is persisted.

AC 3.0.5 implementation intent (keep it simple, but future-ready):
- Persist `Zone` per panel.
- Apply persisted `Zone` at runtime by routing each panel's content to a zone host (Left/Right/Bottom/Center).
- Do not add a user-facing UI to move panels between zones in Phase 3.0.

Zone behavior:
- When a panel in a zone is collapsed, the zone should shrink to minimize unused space (typically 0 width/height).
- When a panel in a zone is expanded, it should occupy the zone and be resizable.
- Zone non-overlap / single-expanded-panel rule:
  - At most one panel may be expanded in a given zone at a time.
  - If a request would expand a second panel into an already-occupied zone, the currently expanded panel in that zone is collapsed first, then the requested panel is expanded.
  - In Phase 3.0, the initial panel set intentionally has multiple panels in the left zone (Directory Browser + Curve Data), so this rule is a primary behavior, not just future-proofing.

Splitter behavior:
- When a zone has no expanded panel, that zone's resize splitter is disabled.
- When a zone has an expanded panel, that zone's resize splitter is enabled and resizes the zone normally.

### 3) Panel Bar behavior
Panel Bar renders label buttons for panels where `EnableIcon = true`.
- Clicking a label toggles that panel.
- The Panel Bar must not collapse panels in other zones when a panel is expanded.
- Expand/collapse state is tracked per zone (for Phase 3.0: Left and Right; Bottom may be unused).
- Each Panel Bar item has an "active" (highlighted) visual state driven only by whether its corresponding panel is expanded.
  - The active visual state is represented via the Panel Bar button background (not only text styling).

Important clarification (Phase 3.0):
- Multiple zones may be expanded at the same time (e.g., Left + Right). The Panel Bar must be able to show multiple highlighted buttons simultaneously when multiple panels are expanded across zones.

Panel Bar implementation note (Phase 3.0):
- Use rotated text labels for each Panel Bar entry.

Notes:
- Curve Graph is not represented in the Panel Bar and therefore never participates in the toggle logic.

### 4) Persistence model (user settings)
Persist the following values across restarts:
- `MainWindow.PanelBarDockSide` (left/right)
- Per-zone expanded panel ids:
  - `MainWindow.LeftZone.ActivePanelId` (nullable)
  - `MainWindow.RightZone.ActivePanelId` (nullable)
  - `MainWindow.BottomZone.ActivePanelId` (nullable)
- Per-zone last expanded sizes (apply to any panel shown in that zone):
  - `MainWindow.LeftZone.Width`
  - `MainWindow.RightZone.Width`
  - `MainWindow.BottomZone.Height`
- Per-panel zone assignment:
  - `MainWindow.DirectoryBrowser.Zone`
  - `MainWindow.MotorProperties.Zone`
  - `MainWindow.CurveData.Zone`
  - `MainWindow.CurveGraph.Zone`

Note:
- The Left zone intentionally contains multiple panels in Phase 3.0 (Directory Browser + Curve Data). Persisting a single per-zone width prevents the zone edge from "jumping" when switching which panel is expanded.

Implementation approach:
- Keep persistence view-driven and continue to use the existing persistence JSON mechanism (ADR-0004) so we don’t introduce a second settings store.
- Extend the persistence helper(s) only as needed to support string/enum values (dock side, per-zone active ids, zone).
- Add logging for persistence load/parse failures, and recover with safe defaults.

Default state (first run / no persisted state):
- Directory Browser expanded.
- Motor Properties expanded.
- Curve Data collapsed.

Phase 3.1 note:
- Once the Directory Browser feature is implemented, Phase 3.1 startup behavior controls whether the Directory Browser panel starts collapsed or is restored from last session.

### 5) Animation
Phase 3.0 does not require expand/collapse animations.

Note:
- We may revisit animations in a later phase once we have stable behavior with splitters and persistence.

### Implementation Steps (Incremental)

### [x] Step 0: Confirm naming + IDs
- [x] Confirm panel display names match [.github/planning/00-terms-and-definitions.md](.github/planning/00-terms-and-definitions.md).
- [x] Lock down `PanelId` strings (must remain stable for persistence keys).

### [x] Step 1: Add descriptor model + persisted state shape
- [x] Implement the descriptor model and a small runtime registry (list of descriptors).
- [x] Add persisted properties:
  - [x] `PanelBarDockSide`
  - [x] `LeftZone.ActivePanelId`, `RightZone.ActivePanelId`, `BottomZone.ActivePanelId`
  - [x] `Zone` per panel (even though fixed in Phase 3.0)

Acceptance checkpoint:
- State can load/save and round-trip without UI changes.

### [x] Step 2: Add Panel Bar UI (shell)
- [x] Add the fixed-width Panel Bar to the window layout.
- [x] Bind it to the descriptor list filtered by `EnableIcon = true`.
- [x] Implement click handling to toggle the active panel within the clicked panel's zone (and only that zone).
- [x] Implement Panel Bar dock side (left/right) without changing any zone assignments.
- [x] Match Panel Bar background to panel header background.
- [x] Highlight each Panel Bar button background only when its panel is expanded.
- [x] Ensure Panel Bar button background highlighting is independent per item.
- [x] Ensure Panel Bar supports multiple highlighted buttons simultaneously when multiple zones have expanded panels.
- [x] Ensure right-docked Panel Bar uses the correct border edge (separator appears between Panel Bar and content).
- [x] Render labels as rotated text (no wrapping).

Acceptance checkpoint:
- Panel Bar appears, dock side can be swapped (via a setting toggle or temporary dev switch), and Panel Bar button backgrounds correctly reflect expanded/collapsed state.

Additional checkpoint detail:
- If Left and Right zones both have expanded panels, both corresponding Panel Bar buttons are highlighted.

### [x] Step 3: Convert panels one-at-a-time (per roadmap order)

1. Directory Browser
  - Convert to zone-based expand/collapse driven by the left zone's active panel id.
  - Ensure default expanded behavior on first run.

2. Motor Properties
   - Convert to the new mechanism.
   - Confirm property editors and undo/redo behavior remain unchanged.

3. Curve Data
   - Convert to the new mechanism.
  - Locate Curve Data in the left zone (not bottom).
  - Persist/restore width (left zone behavior).
  - Ensure expanding Curve Data collapses Directory Browser (same zone), but does not collapse Motor Properties (other zone).
  - Ensure the Curve Data Grid occupies the entirety of the Curve Data Panel and resizes with it.

4. Curve Graph (center)
   - Ensure it remains in the center zone and never collapses.
   - Ensure it shrinks naturally as other zones expand, with sensible minimum constraints.

Acceptance checkpoint per conversion:
- Size persists across restart.
- Zone exclusivity works.
- Panel Bar button background highlight state matches expanded state.

### [x] Step 4: Wire menus/shortcuts to Panel Bar toggles
- [x] Update existing view menu items and keybindings so they toggle the appropriate zone's active panel id.
- [x] Ensure menu checkmarks reflect the new state.

### [x] Step 5: Acceptance criteria validation
- [x] AC 3.0.1: restart restores Panel Bar dock side, expanded panel, and sizes.
- [x] AC 3.0.2: toggles feel instant; no animation required.
- [x] AC 3.0.3: adding a panel is descriptor + content (no core layout rewrite).
- [x] AC 3.0.4: layout changes do not affect undo/redo.

Additional acceptance criteria (capturing the zone and Panel Bar requirements):
- [x] AC 3.0.5: On restart, each panel’s persisted `Zone` value is restored (and if a persisted zone is invalid/unknown, the app falls back to the default zone without user-facing errors).
- [x] AC 3.0.6: The Panel Bar is always visible, fixed-width, and never overlaps the main content (verified for both left-docked and right-docked configurations).
- [x] AC 3.0.7: Zone exclusivity is enforced: expanding a panel collapses any other expanded panel in the same zone, and does not collapse panels in other zones.
- [x] AC 3.0.8: The Curve Graph panel is not represented in the Panel Bar (`EnableIcon = false`), and the Curve Graph remains visible in the center zone at all times.
- [x] AC 3.0.9: Collapsing a panel shrinks its zone to minimize unused space (no persistent blank gutter/stripe beyond the Panel Bar itself).
- [x] AC 3.0.10: Collapsing and re-expanding a panel restores the last non-zero size for that panel (collapse does not permanently “learn” a zero size).

### Testing Strategy (Phase 3.0)
- Manual UI pass after each panel conversion (per roadmap).
- Lightweight automated tests where practical:
  - ViewModel: toggle logic for per-zone active panel ids.
  - Persistence helper: ensure string/enum fields round-trip.

### Logging and Error Handling
- Log only persistence load/parse failures (and recover with defaults) per [ADR-0009](docs/adr/adr-0009-logging-and-error-handling-policy.md).
- Avoid logging every toggle action.

### ADR impacts
- This plan supersedes the "Curve data panel uses an Auto when collapsed pattern" guidance in [ADR-0004](docs/adr/adr-0004-layout-and-panel-persistence.md) for Phase 3.0, because collapsed panels must be fully hidden except for the Panel Bar.
- This plan supersedes any existing UI behavior that keeps collapsed panel headers visible.

### Layout stability (implementation constraint)
- Keep the existing `MainLayoutGrid` stable by nesting it inside a parent layout that hosts the Panel Bar docked left/right. This minimizes churn and reduces risk to current sizing/persistence behavior.

### Out of Scope
- Phase 3.1 directory tree behavior and file validation.
- User-driven moving panels between zones (future phase).
