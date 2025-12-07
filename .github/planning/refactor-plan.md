## Curve Editor Refactor Plan

### Goals:

Given the current state, functionality, and future #file:04-mvp-roadmap.md plans for this project, are there any refactorings you'd recommend to the current codebase to:

- simplify
- improve reliability
- improve maintainability
- remove duplication
- decrease risk

We don't want to loose functionality, but I'd like to tidy up if we can.

### 1. Centralize Editing Orchestration (in progress)
- **Goal**: Reduce coupling between `MainWindowViewModel`, `CurveDataPanel`, `ChartViewModel`, and `CurveDataTableViewModel` by introducing a focused coordination layer.
- **What we implemented so far**:
  - Added an `EditingCoordinator` class that owns selection as a model-level structure of `(CurveSeries Series, int Index)` pairs.
  - `MainWindowViewModel` creates and wires a single `EditingCoordinator` instance into `ChartViewModel` and `CurveDataTableViewModel`.
  - `CurveDataTableViewModel` now pushes all selection changes (click, drag, Shift+arrow, Ctrl+Shift+arrow, etc.) into the coordinator via a single `PushSelectionToCoordinator` helper.
  - `ChartViewModel` listens to coordinator `SelectionChanged` and renders per-point selection overlays (extra marker-only series) so selected table cells immediately highlight corresponding curve points.
- **Still to consider**:
  - Whether to move additional cross-cutting behaviors (dirty-state decisions, some chart refresh triggers, future graph-drag editing) into this coordinator or a sibling orchestration service.
- **Benefits**:
  - Simplifies individual view models and views by centralizing shared selection logic.
  - Provides a clean foundation for Phase 3+ features: graph selection drives table selection, EQ-style editing, and multi-point operations.

### 2. Unify Torque Mutation Paths ✅ (Completed)
- **Current State**: All per-cell torque mutations now go through `CurveDataTableViewModel.TrySetTorqueAtCell`, and batch updates use `ApplyTorqueToCells`.
- **Next Steps**:
  - Keep future features (graph drag, Q-slider, bulk operations) routed through these helpers.
  - Avoid introducing new code paths that call into `CurveSeries` or `VoltageConfiguration` directly.
- **Benefits**:
  - Single enforcement point for: bounds, read-only columns, locked series, and no-op suppression.
  - Strong test coverage already in `CurveDataTableViewModelTests`.

### 3. Slim `CurveDataPanel` Further ✅ (Completed)
- **Goal**: Reduce the amount of business logic embedded in `CurveDataPanel.axaml.cs`.
- **What we implemented**:
  - Introduced `CurveDataTableViewModel` helpers for common editing operations:
    - `ApplyTorqueToSelectedCells(double value)`
    - `ClearSelectedTorqueCells()`
    - `TryPasteClipboard(string clipboardText)` (scalar and rectangular paste via centralized rules)
    - `ApplyOverrideValue(double value)` for override-mode updates.
  - Refactored `CurveDataPanel` clipboard handlers to delegate all data-shape and validation logic to the view model; the panel now only:
    - Reads/writes clipboard text.
    - Calls the corresponding view-model methods.
    - Updates visible cell `TextBlock`s and forces `DataGrid` refresh for virtualized rows.
  - Refactored override-mode behavior so value application flows through `ApplyOverrideValue`, while the panel remains responsible only for capturing keystrokes, managing `_overrideText`, and updating immediate visuals.
- **Resulting responsibilities**:
  - `CurveDataTableViewModel` owns torque mutation rules for override mode, clear, and paste (respecting locked series, fixed columns, bounds, and no-op suppression).
  - `CurveDataPanel` focuses on input events, selection visuals, and immediate UI feedback, with minimal business logic.
- **Benefits**:
  - Core editing semantics are now testable at the view-model level without Avalonia.
  - Reduced duplication of torque mutation rules between view and view model.
  - Lower risk when adding new clipboard or override-mode behaviors, since they plug into centralized APIs.

### 4. Strengthen Graph/Table Linking (substantially completed)
- **Goal**: Lay groundwork for Phase 3+ features that tie graph point selection to table selection and vice versa.
- **Current state**:
  - Selection is now represented in a model-level structure (`EditingCoordinator.PointSelection` as `(series, index)` pairs) understood by both `ChartViewModel` and `CurveDataTableViewModel`.
  - Table  Graph is implemented:
    - `CurveDataTableViewModel` translates its `SelectedCells` into `PointSelection`s and updates the coordinator (including drag and Ctrl/Shift/Ctrl+Shift arrow selection).
    - `ChartViewModel` listens to the coordinator and builds lightweight overlay series that highlight only the selected points.
  - Graph  Table (click selection) is implemented:
    - `ChartView` uses LiveCharts hit-testing to find the nearest point under the cursor.
    - `ChartViewModel.HandleChartPointClick` updates the coordinator using `SetSelection`, `AddToSelection`, or `ToggleSelection` based on modifier keys.
    - `CurveDataTableViewModel` listens to `EditingCoordinator.SelectionChanged` and mirrors `SelectedPoints` back into `SelectedCells`, so graph clicks update the table selection.
- **Planned (Phase 3)**:
  - Rubber-band graph selection and drag-to-edit behaviors that build on the same coordinator and view-model APIs.
  - Use the existing centralized torque mutation APIs (`TrySetTorqueAtCell` / `ApplyTorqueToCells`) as the single path for graph-drag multi-point torque edits.
- **Benefits**:
  - Clear, testable selection model shared between graph and table.
  - Easier implementation of:
    - Selecting a point on the graph highlights the cell.
    - Rubber-band select on graph updates table.
    - Dragging points updates torque values consistently.

### 5. Thin `MainWindowViewModel` (partially addressed)
- **Goal**: Keep `MainWindowViewModel` focused on application shell responsibilities (file commands, high-level selections, status/validation).
- **Ideas / next steps**:
  - Move detailed drive/voltage/series creation logic (including dialogs) into dedicated services or smaller view models.
  - Introduce small domain services for:
    - Drive/voltage creation from dialog results.
    - Initial default selections (first drive, preferred 208 V, first series).
  - Keep `MainWindowViewModel` mostly as a composition root and coordinator.
- **Benefits**:
  - Improves readability and testability of file/drive/voltage workflows.
  - Reduces risk of regressions when adding more commands or dialogs.

### 6. Improve Testability and Surface Areas
- **Goal**: Make more behavior reachable through public, non-UI types rather than private event handlers in views.
- **Ideas**:
  - Promote behaviors currently reachable only via `CurveDataPanel` private handlers into `CurveDataTableViewModel` or small services.
  - Keep UI tests like `CurveDataPanelOverrideModeTests` for wiring and integration, but rely on view-model-level tests for most logic.
  - Consider introducing a small abstraction for message dialogs / prompts so tests can verify decisions without invoking actual UI.
- **Benefits**:
  - Faster, more reliable tests for core logic.
  - Clear boundaries between UI and domain behavior.

### 7. Documentation and Conventions
- **Goal**: Keep refactors coherent and discoverable for future contributors.
- **Ideas**:
  - Document the intended responsibilities of:
    - `MainWindowViewModel`
    - `ChartViewModel`
    - `CurveDataTableViewModel`
    - Any new coordinator/controller services
  - Capture patterns for:
    - How to add new curve editing features without bypassing the centralized mutation APIs.
    - How selection is modeled and propagated between graph and table.
- **Benefits**:
  - Reduces onboarding time and helps maintain the architecture as new features are added.
