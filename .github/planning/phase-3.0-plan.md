## Curve Editor Phase 3.0 Plan: Generic Panel Expand/Collapse

### Goal
- Replace the existing limited expand/collapse behavior with a VS Code–style panel system driven by a reusable panel descriptor model.
- Introduce a fixed-width, always-visible vertical icon bar (the "Panel Bar") for toggling panels.
- Implement a zone-based layout model (left/right/bottom/center) with persisted panel sizes and persisted panel zone assignment.
- Ensure layout changes do not participate in undo/redo.

### Requirements Snapshot (from Phase 3 requirements)
- Panels have headers with the panel name.
- Panel Bar:
  - Always visible, fixed size, docked left by default.
  - Dock side (left/right) is user-configurable and persisted.
  - Dock side changes do not change panel zones.
  - Clicking an icon expands/collapses the panel.
  - Clicking an icon collapses any other expanded panels represented in the Panel Bar.
- Overall layout uses zones; for Phase 3.0 each panel has a fixed zone, but the zone assignment is still persisted for forward compatibility.
- Curve Graph:
  - Occupies the main content area.
  - Never fully collapses (but can shrink as other panels expand).
  - Does not participate in "collapse any other expanded panels" by default.
  - Is not represented in the Panel Bar (via a descriptor property such as `EnableIcon = false`).

Clarifications for Phase 3.0:
- Phase 3.0 does not require expand/collapse animations.
- Phase 3.0 does not require true icons; Panel Bar items may use text labels and/or glyph font characters.

### Current Baseline
- [Main window layout](src/CurveEditor/Views/MainWindow.axaml) already maps well to zones: left column (browser), center (chart + curve data row), right column (properties).
- Existing persistence helpers exist (view-driven) per [ADR-0004](docs/adr/adr-0004-layout-and-panel-persistence.md).
- Existing global shortcuts exist per [ADR-0005](docs/adr/adr-0005-keyboard-shortcuts-and-input-routing.md).

### Proposed Design

### 1) Panel descriptors (core extensibility point)
Introduce a lightweight descriptor model so adding a panel is "register config + provide content":
- `PanelId` (stable string)
- `DisplayName`
- `EnableIcon` (false for Curve Graph)
- `EnableCollapse` (false for Curve Graph by default)
- `Zone` (left/right/bottom/center)
- `DefaultWidth` (used when the panel is in a left/right zone)
- `DefaultHeight` (used when the panel is in a bottom zone)
- `MinSize` (optional; mostly relevant for ensuring the center graph never hits 0)

Descriptor list (Phase 3.0 initial set):
- Directory Browser: `EnableIcon=true`, `EnableCollapse=true`, `Zone=Left`
- Motor Properties: `EnableIcon=true`, `EnableCollapse=true`, `Zone=Right`
- Curve Data: `EnableIcon=true`, `EnableCollapse=true`, `Zone=Bottom`
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
  - In Phase 3.0, the initial panel set maps one panel per collapsible zone (Directory Browser = left, Motor Properties = right, Curve Data = bottom), so this rule is mostly "future-proofing" for when multiple panels can share a zone.

### 3) Panel Bar behavior
Panel Bar renders icons for panels where `EnableIcon = true`.
- Clicking an icon toggles that panel.
- The Panel Bar enforces "only one expanded panel represented in the Panel Bar" by tracking a single `ActivePanelBarPanelId`.
  - Click inactive icon: set `ActivePanelBarPanelId` to that panel.
  - Click active icon: clear `ActivePanelBarPanelId`.

Panel Bar implementation note (Phase 3.0):
- Use a short text label and/or a glyph font character for each Panel Bar entry.

Notes:
- This satisfies the spec line "collapse any other expanded panels represented in the vertical bar".
- Curve Graph is not represented in the Panel Bar and therefore never participates in this toggle logic.

### 4) Persistence model (user settings)
Persist the following values across restarts:
- `MainWindow.PanelBarDockSide` (left/right)
- `MainWindow.ActivePanelBarPanelId` (nullable)
- Per-panel last expanded sizes (store both dimensions; apply based on current zone):
  - `MainWindow.DirectoryBrowser.Width`
  - `MainWindow.DirectoryBrowser.Height`
  - `MainWindow.MotorProperties.Width`
  - `MainWindow.MotorProperties.Height`
  - `MainWindow.CurveData.Width`
  - `MainWindow.CurveData.Height`
- Per-panel zone assignment:
  - `MainWindow.DirectoryBrowser.Zone`
  - `MainWindow.MotorProperties.Zone`
  - `MainWindow.CurveData.Zone`
  - `MainWindow.CurveGraph.Zone`

Implementation approach:
- Keep persistence view-driven and continue to use the existing persistence JSON mechanism (ADR-0004) so we don’t introduce a second settings store.
- Extend the persistence helper(s) only as needed to support string/enum values (dock side, active panel id, zone).
- Add logging for persistence load/parse failures, and recover with safe defaults.

### 5) Animation
Phase 3.0 does not require expand/collapse animations.

Note:
- We may revisit animations in a later phase once we have stable behavior with splitters and persistence.

### Implementation Steps (Incremental)

### Step 0: Confirm naming + IDs
- Confirm panel display names match [.github/planning/00-terms-and-definitions.md](.github/planning/00-terms-and-definitions.md).
- Lock down `PanelId` strings (must remain stable for persistence keys).

### Step 1: Add descriptor model + persisted state shape
- Implement the descriptor model and a small runtime registry (list of descriptors).
- Add persisted properties:
  - `PanelBarDockSide`
  - `ActivePanelBarPanelId`
  - `Zone` per panel (even though fixed in Phase 3.0)

Acceptance checkpoint:
- State can load/save and round-trip without UI changes.

### Step 2: Add Panel Bar UI (shell)
- Add the fixed-width Panel Bar to the window layout.
- Bind it to the descriptor list filtered by `EnableIcon = true`.
- Implement click handling to update `ActivePanelBarPanelId`.
- Implement Panel Bar dock side (left/right) without changing any zone assignments.

Acceptance checkpoint:
- Panel Bar appears and dock side can be swapped (via a setting toggle or temporary dev switch).

### Step 3: Convert panels one-at-a-time (per roadmap order)

1. Directory Browser
   - Convert to zone-based expand/collapse driven by `ActivePanelBarPanelId`.
   - Ensure default collapsed behavior remains possible (Phase 3.1 expects it collapsed by default).

2. Motor Properties
   - Convert to the new mechanism.
   - Confirm property editors and undo/redo behavior remain unchanged.

3. Curve Data
   - Convert to the new mechanism.
   - Persist/restore height and ensure toggling collapses other Panel Bar panels.

4. Curve Graph (center)
   - Ensure it remains in the center zone and never collapses.
   - Ensure it shrinks naturally as other zones expand, with sensible minimum constraints.

Acceptance checkpoint per conversion:
- Size persists across restart.
- Panel Bar exclusivity works.

### Step 4: Wire menus/shortcuts to Panel Bar toggles
- Update existing view menu items and keybindings so they set/clear `ActivePanelBarPanelId`.
- Ensure menu checkmarks reflect the new state.

### Step 5: Acceptance criteria validation
- AC 3.0.1: restart restores Panel Bar dock side, expanded panel, and sizes.
- AC 3.0.2: toggles feel instant; animation ~150ms or less.
- AC 3.0.3: adding a panel is descriptor + content (no core layout rewrite).
- AC 3.0.4: layout changes do not affect undo/redo.

Additional acceptance criteria (capturing the zone and Panel Bar requirements):
- AC 3.0.5: On restart, each panel’s persisted `Zone` value is restored (and if a persisted zone is invalid/unknown, the app falls back to the default zone without user-facing errors).
- AC 3.0.6: The Panel Bar is always visible, fixed-width, and never overlaps the main content (verified for both left-docked and right-docked configurations).
- AC 3.0.7: Panel Bar exclusivity is enforced: expanding any Panel Bar panel collapses any other currently expanded Panel Bar panel (including when the panels belong to different zones).
- AC 3.0.8: The Curve Graph panel is not represented in the Panel Bar (`EnableIcon = false`), and the Curve Graph remains visible in the center zone at all times.
- AC 3.0.9: Collapsing a panel shrinks its zone to minimize unused space (no persistent blank gutter/stripe beyond the Panel Bar itself).
- AC 3.0.10: Collapsing and re-expanding a panel restores the last non-zero size for that panel (collapse does not permanently “learn” a zero size).

### Testing Strategy (Phase 3.0)
- Manual UI pass after each panel conversion (per roadmap).
- Lightweight automated tests where practical:
  - ViewModel: toggle logic for `ActivePanelBarPanelId`.
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
