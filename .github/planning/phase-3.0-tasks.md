## Phase 3.0 Subtasks: Generic Panel Expand/Collapse (Agent Execution Checklist)

### Purpose
- Provide a PR-sliceable task list for implementing Phase 3.0 with minimal rework.
- Make it easy for agents to validate each acceptance criterion incrementally.

### Scope Reminder (Phase 3.0)
- Implement Panel Bar + zone-based panel system with persisted sizes and persisted zone assignment.
- Convert existing panels one at a time (Directory Browser, Motor Properties, Curve Data, Curve Graph behavior).
- Do not implement Directory Browser content (Phase 3.1).
- Do not add undo/redo for layout changes.

### Key Files (Expected touch points)
- UI layout: [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml)
- UI code-behind: [src/CurveEditor/Views/MainWindow.axaml.cs](src/CurveEditor/Views/MainWindow.axaml.cs)
- Persistence helpers: [src/CurveEditor/Behaviors/PanelLayoutPersistence.cs](src/CurveEditor/Behaviors/PanelLayoutPersistence.cs)
- View model commands/state: [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)

### State Model Summary (Target)
- Panel descriptors:
  - `PanelId` (stable)
  - `DisplayName`
  - `PanelBarLabel` (text shown in the Panel Bar)
  - `EnableIcon`
  - `EnableCollapse`
  - `Zone` (Left/Right/Bottom/Center)
  - `DefaultWidth` (double)
  - `DefaultHeight` (double)
  - `MinSize` (double?)
- Runtime state:
  - `PanelBarDockSide` (Left/Right)
  - per-zone active panel ids:
    - `LeftZoneActivePanelId` (nullable)
    - `RightZoneActivePanelId` (nullable)
    - `BottomZoneActivePanelId` (nullable)
- Persistence:
  - per-panel `Zone`
  - per-panel last non-zero expanded width
  - per-panel last non-zero expanded height

Notes:
- Panel Bar dock side is independent of panel zones (zones do not "follow" the Panel Bar).
- Phase 3.0 does not require expand/collapse animations.
- Phase 3.0 uses rotated text labels (no icons/glyphs).

Notes:
- Curve Graph: `EnableIcon=false`, `EnableCollapse=false`, `Zone=Center`.
- Zone exclusivity applies only within a zone.
- Zone non-overlap rule must exist even if Phase 3.0 has one panel per zone.

Defaults (first run / no persisted state):
- Directory Browser expanded.
- Motor Properties expanded.
- Curve Data collapsed.

### Agent Notes (Migration Guidance)
- Current implementation already has per-panel booleans and persistence wiring (e.g., `IsBrowserPanelExpanded`, `IsPropertiesPanelExpanded`, `IsCurveDataExpanded`) and related commands/keybindings.
- Phase 3.0 should migrate behavior to a single source of truth: per-zone active panel ids.
  - Do not attempt to keep both systems “authoritative” at the same time.
  - During migration, it’s OK for the old booleans to temporarily remain in the view model for menu checkmarks and backwards compatibility, but they should be derived from the per-zone active panel ids (or removed once all panels are converted).
- Command migration pattern (recommended):
  - Keep existing commands (`ToggleBrowserPanelCommand`, `TogglePropertiesPanelCommand`, `ToggleCurveDataPanelCommand`) so shortcuts and menus don’t churn.
  - Re-implement their handlers to set/clear the appropriate zone's active panel id rather than flipping booleans.
  - Keep the keyboard shortcut policy in [docs/adr/adr-0005-keyboard-shortcuts-and-input-routing.md](docs/adr/adr-0005-keyboard-shortcuts-and-input-routing.md): shortcuts remain defined on `MainWindow`.
- Persistence migration pattern (recommended):
  - Preserve the view-driven persistence approach from [docs/adr/adr-0004-layout-and-panel-persistence.md](docs/adr/adr-0004-layout-and-panel-persistence.md).
  - Prefer adding new keys (Panel Bar dock side, active panel id, per-panel zone) instead of repurposing old keys in-place.
  - If legacy keys exist for widths/heights, either:
    - continue honoring them as defaults on first run after upgrade, then write the new keys, or
    - migrate them once (read old -> write new -> keep fallback read for one release).
- Animation migration note:
  - If the current code relies on setting `GridLength` to 0 to “collapse”, switch to animating container `Width`/`Height` to satisfy the smooth animation requirement.
  - Avoid simultaneous animation + splitter updates fighting each other; prefer temporarily disabling splitters during animation if needed.

---

## [x] PR 0: Preparation (no behavior change)

### Tasks
- [x] Add/update any shared types needed for Phase 3.0 (enums for `Zone` and `DockSide`).
- [x] Decide the stable `PanelId` strings (do not rename after merging).

### Done when
- Build passes.
- No user-visible behavior changes.

### Files
- Likely adds: `src/CurveEditor/Models` or `ViewModels` area for enums (follow existing conventions).

---

## [ ] PR 1: Descriptor registry + persisted state plumbing

### Tasks
- [x] Add a panel descriptor model and an initial registry list (4 panels).
- [x] Add persisted settings fields:
  - [x] `MainWindow.PanelBarDockSide`
  - [x] `MainWindow.LeftZone.ActivePanelId`
  - [x] `MainWindow.RightZone.ActivePanelId`
  - [x] `MainWindow.BottomZone.ActivePanelId`
  - [x] `MainWindow.<PanelId>.Zone`
  - [ ] `MainWindow.<PanelId>.Width` (last non-zero expanded width)
  - [ ] `MainWindow.<PanelId>.Height` (last non-zero expanded height)
- [x] Extend `PanelLayoutPersistence` as needed to store/load string/enum values.
- [x] Add logging for persistence load/parse failures (recover with defaults; log once per failure).
- [x] Add safe fallbacks:
  - [x] Unknown `PanelId` in persisted state -> treat as null.
  - [x] Unknown zone -> fall back to descriptor default.

### Done when
- App can start, persist values, and restore them on restart (even if not yet wired to UI).
- Meets AC 3.0.5 fallback behavior.

### Files
- [src/CurveEditor/Behaviors/PanelLayoutPersistence.cs](src/CurveEditor/Behaviors/PanelLayoutPersistence.cs)
- [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)
- Optional new files: panel descriptor model (choose consistent folder).

### Quick manual test
1. Launch app.
2. Close app.
3. Confirm no exceptions and logs are clean.
4. (If you expose a temporary debug toggle) change dock side/state and restart to verify round-trip.

---

## [ ] PR 2: Panel Bar UI shell (no panel conversions yet)

### Tasks
- [x] Add the fixed-width Panel Bar to `MainWindow`.
- [x] Bind Panel Bar label buttons to descriptors where `EnableIcon=true`.
- [x] Ensure Panel Bar uses text labels (not icons/glyphs) with exact strings:
  - [x] Motor Properties = "Properties"
  - [x] Curve Data = "Data"
  - [x] Directory Browser = "Browser"
- [x] Ensure Panel Bar text is oriented sideways (rotated) and does not wrap.
- [x] Ensure Panel Bar background color matches panel header background.
- [ ] Ensure Panel Bar button backgrounds are highlighted when the corresponding panel is expanded.
- [ ] Ensure Panel Bar button backgrounds are not highlighted when the corresponding panel is collapsed.
- [ ] Ensure highlight state is independent per label (each Panel Bar button background highlights based only on its own panel expanded/collapsed state).
- [x] Implement click -> toggle the appropriate zone active panel id.
- [x] Implement left/right docking based on `PanelBarDockSide`.
- [x] Ensure Panel Bar dock side changes do not change panel zones.

### Done when
- Panel Bar is always visible and fixed width.
- Panel Bar button backgrounds highlight only when their panel is expanded (and never when collapsed).
- Each Panel Bar button background highlight is independent of other items.
- Dock side can be switched (via an existing settings mechanism, or a temporary debug mechanism if settings UI is not ready).
- Meets AC 3.0.6.

### Files
- [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml)
- [src/CurveEditor/Views/MainWindow.axaml.cs](src/CurveEditor/Views/MainWindow.axaml.cs)

### Quick manual test
1. Launch app.
2. Verify Panel Bar appears and does not overlap content.
3. Toggle Browser/Properties/Data and verify only the active panel's button background is highlighted.
4. Toggle dock side (if available) and verify Panel Bar moves.

---

## [x] PR 3: Implement zone non-overlap rule (framework behavior)

### Tasks
- [x] Implement zone-level single-expanded-panel rule in the panel system:
  - [x] Expanding a panel into a zone collapses any currently expanded panel in that same zone first.
- [x] Ensure expanding a panel does not collapse panels in other zones.
- [x] Ensure the zone collapse shrinks space (0 width/height).
- [x] When a zone has no expanded panel, disable that zone's splitter.
- [x] When a zone has an expanded panel, enable that zone's splitter.
- [x] Apply persisted per-panel `Zone` at runtime by routing each panel into the correct zone host (AC 3.0.5).

### Done when
- Meets “Panel Behavior in Overall Window Layout” requirements.
- Meets AC 3.0.9.

### Files
- Likely: [src/CurveEditor/ViewModels/MainWindowViewModel.cs](src/CurveEditor/ViewModels/MainWindowViewModel.cs)
- Possibly: `MainWindow.axaml.cs` if the rule is best enforced in view-level layout application.

### Quick manual test
1. If you temporarily configure two panels to share a zone (developer-only), verify expanding one collapses the other.
2. Verify no blank gutter remains when collapsed.

---

## [x] PR 4: Convert Directory Browser panel (placeholder)

### Tasks
- [x] Convert left zone behavior to be driven by the new panel system (not `IsBrowserPanelExpanded`).
- [x] Persist/restore last non-zero width for the Directory Browser panel.
- [x] Ensure it defaults to expanded on first run (no persisted state) for Phase 3.0 placeholder behavior.
- [x] Ensure Phase 3.1 can override startup behavior to start collapsed by default once directory browsing is implemented.
- [x] Ensure header exists with correct panel name.

### Done when
- Clicking the Directory Browser icon toggles the panel.
- Resizing persists across restart (AC 3.0.1, AC 3.0.10).
- Expanding it collapses any other panel already expanded in the left zone (AC 3.0.7).

### Files
- [src/CurveEditor/Views/MainWindow.axaml](src/CurveEditor/Views/MainWindow.axaml)
- [src/CurveEditor/Views/MainWindow.axaml.cs](src/CurveEditor/Views/MainWindow.axaml.cs)

### Quick manual test
1. Launch.
2. Click Directory Browser icon -> expands.
3. Resize width via splitter.
4. Click icon again -> collapses.
5. Restart -> width restores when expanded.

---

## [x] PR 5: Convert Motor Properties panel

### Tasks
- [x] Convert right zone behavior to be driven by the new panel system (not `IsPropertiesPanelExpanded`).
- [x] Persist/restore last non-zero width.
- [x] Add panel header consistent with Phase 3.0 header rule.
- [x] Ensure no interaction with undo/redo stack.

### Done when
- Properties toggles via Panel Bar.
- Resizing persists across restart.
- Editing motor fields still behaves exactly as before.

### Quick manual test
1. Load a motor.
2. Expand/collapse properties.
3. Edit a property; verify Ctrl+Z / Ctrl+Y still undo document edits, not layout.

---

## [x] PR 6: Convert Curve Data panel

### Tasks
- [x] Locate Curve Data in the left zone (not bottom).
- [x] Convert left zone behavior to be driven by the new panel system (not `IsCurveDataExpanded`).
- [x] Persist/restore last non-zero width.
- [x] Add panel header consistent with the new header rule.
- [x] Ensure the Curve Data Grid occupies the entirety of the Curve Data Panel and resizes with it.

### Done when
- Curve Data toggles via Panel Bar.
- Expanding Curve Data collapses Directory Browser if it was expanded (same zone), and does not collapse Motor Properties (other zone) (AC 3.0.7).
- Width persists across restart (AC 3.0.1, AC 3.0.10).

### Quick manual test
1. Load a motor.
2. Toggle Curve Data panel.
3. Resize its width.
4. Restart -> verify width restores when expanded.

---

## [x] PR 7: Curve Graph (center zone) invariants

### Tasks
- [x] Ensure Curve Graph is always present in the center zone and never fully collapses.
- [x] Ensure Curve Graph is not represented in the Panel Bar (`EnableIcon=false`).
- [x] Add sensible minimum constraints so the chart remains usable when other panels expand.

### Done when
- Meets AC 3.0.8.

### Quick manual test
1. Expand each side/bottom panel.
2. Verify chart remains visible.
3. Verify there is no Curve Graph icon in the Panel Bar.

---

## [x] PR 8: Wire menus and shortcuts to the new panel system

### Tasks
- [x] Update View menu toggles to reflect and control the per-zone active panel ids.
- [x] Update existing keybindings (Ctrl+B, Ctrl+R, Ctrl+G) to toggle the appropriate zone active panel id.
- [x] Ensure menu checkmarks accurately reflect expanded state.

### Done when
- Keyboard shortcuts behave consistently with the Panel Bar.
- No regressions in the existing shortcut policy.

---

## [ ] PR 9: Hardening and final validation

### Tasks
- [ ] Ensure persistence never “learns” zero sizes.
- [x] Ensure invalid persisted values fail safely (no user-facing errors; log once).
- [x] Ensure expand/collapse responsiveness feels instant.

### Cross-cutting implementation constraint
- Keep `MainLayoutGrid` stable by nesting it inside a parent layout that hosts the Panel Bar docked left/right.

### Done when
- All AC 3.0.1–3.0.10 pass in a manual validation pass.

### Final manual validation script (AC-driven)
1. Dock side: verify Panel Bar left/right and persistence (AC 3.0.1, AC 3.0.6).
2. Zone behavior: expand Browser and Properties; verify both can stay expanded (different zones) (AC 3.0.7).
3. Zone behavior: expand Browser then Data; verify only one is open in the left zone (AC 3.0.7).
3. Zone shrink: collapse an open panel and ensure no blank gutter remains (AC 3.0.9).
4. Size persistence: set widths/heights, restart, verify restore (AC 3.0.1, AC 3.0.10).
5. Zone persistence: (dev-only) alter persisted zone values, restart, verify restore/fallback (AC 3.0.5).
6. Curve Graph: confirm always visible and not in Panel Bar (AC 3.0.8).
7. Undo/redo: perform document edits, toggle panels, confirm undo stack only affects document edits (AC 3.0.4).
8. Performance: toggle panels repeatedly; confirm the UI remains responsive (AC 3.0.2).
9. Panel Bar highlight: verify each label highlights only when its panel is expanded.
