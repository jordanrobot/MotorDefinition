## Curve Editor Phase 3 Plan

### 1. Graph → Table Selection
- **Goal**: Selecting points on the graph updates the table selection using the existing `EditingCoordinator`.
- **Steps**:
  - [ ] Add hit-testing support in `ChartView` to detect clicks on (or near) chart points.
  - [ ] Map clicked points to `(CurveSeries series, int index)` using the existing `_seriesDataCache`/`CurrentVoltage` in `ChartViewModel`.
  - [ ] On click, update the coordinator using `SetSelection`, `AddToSelection`, or `ToggleSelection` based on modifier keys.
  - [x] Extend `CurveDataTableViewModel` to listen to `EditingCoordinator.SelectionChanged` and project `PointSelection`s back into `SelectedCells`.
  - [ ] Ensure selection sync is two-way but avoids feedback loops (e.g., ignore coordinator events when the change originated from the table itself).

### 2. Rubber-Band Selection on Graph
- **Goal**: Click-and-drag on the graph selects all enclosed curve points and updates the table selection.
- **Steps**:
  - [ ] In `ChartView`, capture mouse-down, move, and up to define a selection rectangle in chart coordinates.
  - [ ] Convert screen coordinates to data coordinates using LiveCharts APIs.
  - [ ] For each visible series, find all points within the rectangle and map them to `PointSelection` entries.
  - [ ] Update `EditingCoordinator.SelectedPoints` with the full set (replacing or extending selection depending on modifier keys).
  - [ ] Verify that the existing table-side listener responds by updating `SelectedCells` correctly for multi-series, multi-row ranges.

### 3. Drag-to-Edit Points on Graph
- **Goal**: Dragging selected graph points up/down adjusts torque values via the centralized torque mutation APIs and keeps the table in sync.
- **Steps**:
  - [ ] Introduce a "point drag" interaction in `ChartView`:
    - Mouse-down on a selected point enters drag mode.
    - Vertical mouse movement determines the new torque value(s).
  - [ ] Convert drag delta from screen space to torque deltas using the Y-axis scale from `ChartViewModel`.
  - [ ] For each affected `PointSelection`, compute the new torque and route it through `CurveDataTableViewModel.TrySetTorqueAtCell` / `ApplyTorqueToCells` (never mutate `CurveSeries` directly).
  - [ ] Ensure `CurveDataTableViewModel.DataChanged` and `ChartViewModel.UpdateDataPoint` keep both views in sync without redundant refreshes.
  - [ ] Respect existing rules: locked series, read-only columns, bounds, and no-op suppression.

### 4. Coordinated Selection Semantics
- **Goal**: Provide consistent keyboard/mouse semantics across table and graph for multi-selection.
- **Steps**:
  - [ ] Define a small set of selection behaviors (replace, extend, toggle) based on modifier keys (none, Shift, Ctrl) and document them.
  - [ ] Apply the same semantics to graph clicks and rubber-band selection as already used in the table (e.g., Ctrl-click toggles, Shift-click extends from last anchor).
  - [x] Expose helper methods on `EditingCoordinator` such as `SetSelection`, `AddToSelection`, `ToggleSelection` to keep this logic centralized.
  - [ ] Add unit tests for `EditingCoordinator` to validate selection operations independently from UI.

### 5. Performance & UX Considerations
- **Goal**: Keep interactions smooth even with many points and series.
- **Steps**:
  - [ ] Measure selection and drag performance with realistic data sizes.
  - [ ] Optimize selection rectangle hit-testing (e.g., avoid repeated LINQ allocations in tight mouse-move loops).
  - [ ] Ensure overlay series updates are incremental where possible (e.g., only rebuild overlays for series whose selection changed).
  - [ ] Provide clear visual feedback for drag operations (e.g., temporary ghost line or tooltip showing torque value while dragging).

### 6. Testing Strategy
- **Goal**: Cover new behavior with a mix of unit and integration tests.
- **Steps**:
  - [ ] Add unit tests around `EditingCoordinator` selection operations and any new helpers.
  - [ ] Add view-model level tests for graph-driven edits that verify torque changes go through `CurveDataTableViewModel` helpers.
  - [ ] Extend existing Avalonia UI tests (or add new ones) to validate basic graph ↔ table selection wiring (e.g., a simulated graph click results in correct `SelectedCells`).

### 7. Documentation Updates
- **Goal**: Make Phase 3 behavior and extension points easy to understand.
- **Steps**:
  - [ ] Document the selection flow: table ⇄ `EditingCoordinator` ⇄ chart.
  - [ ] Add a short "Graph Editing" section to the README or a dedicated architecture doc describing drag-to-edit behavior and how to extend it.
  - [ ] Note that all torque edits (including graph interactions) must go through `CurveDataTableViewModel` mutation APIs.
