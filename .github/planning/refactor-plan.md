## Curve Editor Refactor Plan

### Goals:

Given the current state, functionality, and future #file:04-mvp-roadmap.md plans for this project, are there any refactorings you'd recommend to the current codebase to:

- simplify
- improve reliability
- improve maintainability
- remove duplication
- decrease risk

We don't want to loose functionality, but I'd like to tidy up if we can.

### 1. Centralize Editing Orchestration
- **Goal**: Reduce coupling between `MainWindowViewModel`, `CurveDataPanel`, `ChartViewModel`, and `CurveDataTableViewModel` by introducing a focused coordination layer.
- **Idea**: Introduce a lightweight "editing controller" (service or view model) responsible for:
  - Current selection context (series, rows, and graph/table linkage).
  - Coordinating torque edits, clipboard operations, and override-mode semantics.
  - Owning cross-cutting behaviors like dirty-state management and chart refresh triggers.
- **Benefits**:
  - Simplifies individual view models and views.
  - Makes it easier to add Phase 3+ features: graph selection drives table selection, EQ-style editing, and multi-point operations.

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

### 4. Strengthen Graph/Table Linking
- **Goal**: Lay groundwork for Phase 3+ features that tie graph point selection to table selection and vice versa.
- **Ideas**:
  - Represent selection in a model-level structure (e.g., selected points as `(series, index)` pairs) that both `ChartViewModel` and `CurveDataTableViewModel` understand.
  - Expose small, intention-revealing methods:
    - `SelectPoints(IEnumerable<PointSelection> points)`
    - `SelectCellsForPoints(IEnumerable<PointSelection> points)`
  - Ensure that selection changes raise a shared event or use a shared coordinator so chart ↔ table updates remain consistent.
- **Benefits**:
  - Easier implementation of:
    - Selecting a point on the graph highlights the cell.
    - Rubber-band select on graph updates table.
    - Dragging points updates torque values consistently.

### 5. Thin `MainWindowViewModel`
- **Goal**: Keep `MainWindowViewModel` focused on application shell responsibilities (file commands, high-level selections, status/validation).
- **Ideas**:
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
