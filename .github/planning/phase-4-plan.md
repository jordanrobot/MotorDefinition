## Curve Editor Phase 4 Plan

### 1. EQ-Style Curve Editing
- **Goal**: Enable rich, EQ-style editing of torque curves directly on the chart while keeping the data grid and underlying models in sync.
- **Steps**:
  - [ ] Enable point selection on the chart (single-click on a point) and ensure it updates selection via `EditingCoordinator`.
  - [ ] Implement rubber-band selection on the chart (click-drag rectangle to select multiple points) using `EditingCoordinator` and `SelectionOrigin.Chart`.
  - [ ] Ensure chart selections stay synchronized with table selection via `EditingCoordinator` (table selections push with `SelectionOrigin.Table`).
  - [ ] Implement point dragging (drag selected points up/down to adjust torque) using the existing torque mutation APIs on `CurveDataTableViewModel`.
  - [ ] Provide clear visual feedback when points are selected and while they are being dragged (e.g., highlighted points, live-updating line).
  - [ ] Sync dragged torque changes back to the data model in real-time so the data grid and chart remain consistent.

### 2. Q Value Control
- **Goal**: Let users control how broadly edits propagate along the curve using a Q-style control.
- **Steps**:
  - [ ] Add a Q slider control in the UI (range 0.0 to 1.0).
  - [ ] Define how Q influences the edit falloff (low Q = sharp/affects fewer neighbors, high Q = gradual/affects more neighbors).
  - [ ] Integrate Q into the drag-edit logic so neighboring points are adjusted according to the chosen Q value.
  - [ ] Provide a visual indication of the affected zone while dragging (e.g., highlighted region on the chart).
  - [ ] Ensure the selected Q value persists for the duration of the editing session.

### 3. Background Image Overlay
- **Goal**: Allow users to load and align a reference image behind the torque chart.
- **Steps**:
  - [ ] Add a "Load Background Image" menu item or button.
  - [ ] Support at least PNG, JPG, and BMP image formats.
  - [ ] Render the background image behind the chart (z-order below all curve series).
  - [ ] Add X-axis and Y-axis scale sliders for the image so users can align image axes with chart axes independently.
  - [ ] Provide position offset controls for fine alignment (optional if not needed initially).
  - [ ] Add a toggle to show/hide the background image without losing its configuration.

### 4. Axis Scaling
- **Goal**: Give users control over the visible X/Y ranges while keeping labels and grid lines readable.
- **Steps**:
  - [ ] Add X-axis range controls (e.g., min/max RPM sliders or fields).
  - [ ] Add Y-axis range controls (e.g., min/max torque sliders or fields).
  - [ ] Ensure that changing axis ranges results in smooth graph recalculation without visible jitter.
  - [ ] Update grid lines and axis labels dynamically when ranges change while preserving "nice" rounded label increments.

### 5. Add/Remove Data Points
- **Goal**: Let users explicitly insert or delete data points while preserving curve integrity.
- **Steps**:
  - [ ] Add an "Insert Point" action that inserts a new point between existing points, maintaining sorted order by percent/RPM.
  - [ ] Add a "Delete Point" action that removes the currently selected point(s), respecting any constraints (e.g., minimum number of points).
  - [ ] Ensure both chart and data grid update automatically when points are inserted or deleted.
  - [ ] Route all point insert/delete operations through a centralized API (and later, undoable commands) so they integrate cleanly with dirty-state tracking and future undo/redo.

### 6. Selection Coordination & Feedback Loops
- **Goal**: Keep chart and table selections consistent and avoid selection feedback loops between views.
- **Steps**:
  - [ ] Model all point selections through a shared `EditingCoordinator` and a `SelectionOrigin` enum (e.g., Table vs Chart vs Programmatic).
  - [ ] Ensure table-originated selection changes use `SelectionOrigin.Table`, and chart-originated changes use `SelectionOrigin.Chart`.
  - [ ] Use origin information in listeners to avoid reacting to selections they initiated themselves (preventing feedback loops).
  - [ ] Keep replace/extend/toggle semantics consistent between chart and table, and document these behaviors.

### 7. Testing & Documentation
- **Goal**: Validate advanced editing behavior and document how to work with it.
- **Steps**:
  - [ ] Add unit and view-model tests for selection coordination, drag-to-edit behavior, and Q-influenced edits.
  - [ ] Add tests to confirm that background images, axis scaling, and point insert/delete operations keep chart and grid in sync.
  - [ ] Update the architecture or README documentation with a "Graph Editing" section covering EQ-style editing, Q control, selection coordination, and background image/axis scaling behavior.
