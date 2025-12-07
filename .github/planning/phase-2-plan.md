## Curve Editor Phase 2 Plan

### 2. Undo/Redo Infrastructure (Phase 1.8 follow-through)
- **Goal**: Provide robust undo/redo for common editing actions and ensure it integrates with dirty-state tracking.
- **Context**: Roadmap section `1.8 Undo/Redo Infrastructure` outlines the building blocks. This plan refines them.

#### 2.1 Core Undoable Command Model
- [ ] Define an `IUndoableCommand` interface with at least:
  - `void Execute()`
  - `void Undo()`
  - Optional: a `string Description` or similar for debugging/UI.
- [ ] Decide on whether commands are:
  - Per-motor/document, or
  - Global for the entire app.
  (Recommended: one undo stack per open motor/file.)

#### 2.2 Undo Stack Service
- [ ] Create an `UndoStack` (or `UndoService`) responsible for:
  - Holding a stack of executed commands and a separate stack for redo.
  - `PushAndExecute(IUndoableCommand command)` that runs `Execute()` and records the command.
  - `Undo()` that pops the last command, calls `Undo()`, and pushes it onto the redo stack.
  - `Redo()` that re-executes the last undone command and moves it back to the undo stack.
- [ ] Expose simple properties/events:
  - `CanUndo`, `CanRedo` for button/shortcut enabling.
  - `UndoStackChanged` (optional) for debug/insights.
- [ ] Associate the undo stack with the current document/motor definition so it resets appropriately when a new file is opened.

#### 2.3 Command Types for Common Operations
- [ ] Implement concrete `IUndoableCommand` types for the main editing actions:
  - `EditPointCommand`: change torque (and/or RPM) for a single data point at a given index/series.
  - `EditSeriesCommand`: rename, visibility change, color change, or lock state for a `CurveSeries`.
  - `EditMotorPropertyCommand`: change a scalar property on `MotorDefinition` or its nested metadata (e.g., max RPM, max torque).
- [ ] Ensure each command captures enough prior state to undo reliably (old/new value, series id, index, etc.).
- [ ] Centralize domain mutations so that:
  - Direct property edits from the UI go through helper methods that create and push commands rather than mutating models directly.

#### 2.4 Wiring Undo/Redo to UI and Dirty State
- [ ] Wire Ctrl+Z / Ctrl+Y (or platform-appropriate equivalents) to call `Undo()` and `Redo()` on the active document's undo stack.
- [ ] Add toolbar/menu items for Undo/Redo, bound to the same commands, with enable/disable driven by `CanUndo` / `CanRedo`.
- [ ] Integrate undo/redo with dirty tracking:
  - Mark the document dirty whenever a command is executed.
  - Consider tracking the "clean" checkpoint (state after last save) so undoing back to that point clears the dirty flag.

#### 2.5 Tests and Safeguards
- [ ] Add unit tests for `UndoStack` behavior:
  - Push/execute, undo, redo sequences.
  - Edge cases when stacks are empty.
- [ ] Add unit tests for each concrete command type to verify:
  - `Execute()` applies the expected change.
  - `Undo()` fully restores prior state.
- [ ] Add a small integration-style test around a view model (e.g., editing a point in the data grid) to ensure UI-driven edits go through commands and can be undone/redone.

### 3. Phase 2 Completion Criteria
- [ ] All items under `1.7 Logging and Exception Handling` in `04-mvp-roadmap.md` are satisfied by the implemented logging/exception handling.
- [ ] All items under `1.8 Undo/Redo Infrastructure` are satisfied by the implemented undo/redo system.
- [ ] New logging and undo/redo behavior does not break existing tests; new tests are added where appropriate to cover the new infrastructure.
