## Curve Editor Phase 2 Plan

**Related ADRs**

- ADR-0003 Motor Property Undo Design (`.github/adr/adr-0003-motor-property-undo-design.md`)
- ADR-000X Voltage Property Undo Design (`.github/adr/adr-000X-voltage-property-undo-design.md`)

### 2. Undo/Redo Infrastructure (Phase 1.8 follow-through)
- **Goal**: Provide robust undo/redo for common editing actions and ensure it integrates with dirty-state tracking.
- **Context**: Roadmap section `1.8 Undo/Redo Infrastructure` outlines the building blocks. This plan refines them.

#### 2.1 Core Undoable Command Model
- [x] Define an `IUndoableCommand` interface with at least:
  - `void Execute()`
  - `void Undo()`
  - Optional: a `string Description` or similar for debugging/UI.
- [x] Decide on whether commands are:
  - Per-motor/document, or
  - Global for the entire app.
  (Recommended: one undo stack per open motor/file.)

#### 2.2 Undo Stack Service
- [x] Create an `UndoStack` (or `UndoService`) responsible for:
  - Holding a stack of executed commands and a separate stack for redo.
  - `PushAndExecute(IUndoableCommand command)` that runs `Execute()` and records the command.
  - `Undo()` that pops the last command, calls `Undo()`, and pushes it onto the redo stack.
  - `Redo()` that re-executes the last undone command and moves it back to the undo stack.
- [x] Expose simple properties/events:
  - `CanUndo`, `CanRedo` for button/shortcut enabling.
  - `UndoStackChanged` (optional) for debug/insights.
- [x] Associate the undo stack with the current document/motor definition so it resets appropriately when a new file is opened.

#### 2.3 Command Types for Common Operations
- [x] Implement concrete `IUndoableCommand` types for the main editing actions:
  - `EditPointCommand`: change torque (and/or RPM) for a single data point at a given index/series.
  - `EditSeriesCommand`: rename, visibility change, color change, or lock state for a `CurveSeries`.
  - `EditMotorPropertyCommand`: change a scalar property on `MotorDefinition` or its nested metadata (e.g., max RPM, max torque).
- [x] Ensure each command captures enough prior state to undo reliably (old/new value, series id, index, etc.).
- [x] Centralize domain mutations so that:
  - Direct property edits from the UI go through helper methods that create and push commands rather than mutating models directly.

See ADR-0003 (`.github/adr/adr-0003-motor-property-undo-design.md`) for the specific command-driven pattern adopted for motor-level text properties.

See ADR-000X (`.github/adr/adr-000X-voltage-property-undo-design.md`) for the extension of this pattern to drive and selected-voltage properties, including scalar values and series-related fields on `VoltageConfiguration`.

**Motor text properties implementation and lessons learned:**

Early attempts to bolt motor text property undo onto existing two-way `TextBox` bindings via attached behaviors proved fragile: bindings were mutating `MotorDefinition` before commands could reliably capture the old value, and Ctrl+Z handling oscillated between per-textbox and global behavior. We have now implemented the ADR-0003 pattern in production: motor text boxes bind to simple editor properties on the view model, `TextBox`-local undo is disabled, and explicit edit methods (e.g., `EditMotorName`) construct `EditMotorPropertyCommand` instances with old/new values that are pushed to the shared `UndoStack`. This yields predictable, whole-field undo/redo steps that align with chart and grid edits.

#### 2.4 Wiring Undo/Redo to UI and Dirty State
#### 2.4 Wiring Undo/Redo to UI and Dirty State
- [x] Wire Ctrl+Z / Ctrl+Y (or platform-appropriate equivalents) to call `Undo()` and `Redo()` on the active document's undo stack.
- [x] Add toolbar/menu items for Undo/Redo, bound to the same commands, with enable/disable driven by `CanUndo` / `CanRedo`.
- [x] Integrate undo/redo with dirty tracking:
  - Mark the document dirty whenever a command is executed.
  - Track the "clean" checkpoint (state after last save) so undoing back to that point clears the dirty flag.

#### 2.5 Tests and Safeguards
- [x] Add unit tests for `UndoStack` behavior:
  - Push/execute, undo, redo sequences.
  - Edge cases when stacks are empty.
- [x] Add unit tests for each concrete command type to verify:
  - `Execute()` applies the expected change.
  - `Undo()` fully restores prior state.
- [x] Add a small integration-style test around a view model (e.g., editing a point in the data grid) to ensure UI-driven edits go through commands and can be undone/redone.

### 3. Phase 2 Completion Criteria
- [x] All items under `1.7 Logging and Exception Handling` in `04-mvp-roadmap.md` are satisfied by the implemented logging/exception handling.
- [x] All items under `1.8 Undo/Redo Infrastructure` are satisfied by the implemented undo/redo system.
- [x] New logging and undo/redo behavior does not break existing tests; new tests are added where appropriate to cover the new infrastructure.

### 4. Motor Property Undo via Dedicated Command Path (completed)

- [x] Introduce explicit edit methods on the view model
  - [x] Add methods on `MainWindowViewModel` (or a dedicated `MotorPropertiesViewModel`) such as:
    - `EditMotorName(string newName)`
    - `EditMotorManufacturer(string newManufacturer)`
    - `EditMotorPartNumber(string newPartNumber)`
  - [x] For each method:
    - [x] Read the current value from `CurrentMotor`.
    - [x] Early-out if the value is unchanged.
    - [x] Construct an `IUndoableCommand` containing both old and new values.
    - [x] Push and execute the command through the existing `UndoStack`, ensuring dirty state is updated.

- [x] Refactor bindings for key motor text fields
  - [x] In `MainWindow.axaml`, replace direct bindings like `Text="{Binding CurrentMotor.MotorName}"` with bindings to simple VM properties (e.g., `MotorNameEditor`) and, on focus loss, use code-behind to call `EditMotorName(MotorNameEditor)`.
  - [x] Set `TextBox.IsUndoEnabled` to `False` for these fields so that Ctrl+Z / Ctrl+Y always route to the window-level undo/redo commands.
  - [x] Ensure the underlying `MotorDefinition` is mutated only via the command path, not by the bindings themselves.

- [x] Update or extend the motor property command type
  - [x] Refactor `EditMotorPropertyCommand` to accept both `oldValue` and `newValue` in its constructor.
  - [x] Store `oldValue` and `newValue` explicitly within the command.
  - [x] Set the property to `newValue` in `Execute()` and back to `oldValue` in `Undo()`.
  - [x] Avoid inferring `oldValue` from the model at execution time.

- [x] Keep keyboard shortcuts at the window level
  - [x] Leave the existing Ctrl+Z / Ctrl+Y key bindings on `MainWindow` bound to `UndoCommand` / `RedoCommand`.
  - [x] Ensure motor property edits participate in the global undo/redo history without additional per-control key handling.

- [x] Add focused tests
  - [x] Add tests that:
    - [x] Create a view model with a `MotorDefinition`.
    - [x] Invoke `EditMotorName("New Name")` and assert the model changes.
    - [x] Call `Undo()` and verify the original name is restored.
    - [x] Call `Redo()` and verify the new name is applied again.

This plan avoids the fragility observed with attached behaviors and tightly couples motor property edits to the undo/redo infrastructure in a clear, testable way.

This work is governed by ADR-0003 (`.github/adr/adr-0003-motor-property-undo-design.md`), which documents the rationale, decision, and migration steps.

The same principles now govern drive and voltage property edits as captured in ADR-000X (`.github/adr/adr-000X-voltage-property-undo-design.md`), which describes:

- The `EditDrivePropertyCommand` and `EditVoltagePropertyCommand` types;
- The use of editor buffers (e.g., `DriveNameEditor`, `VoltagePowerEditor`, `VoltagePeakTorqueEditor`);
- LostFocus-based commit methods (e.g., `EditDriveName`, `EditSelectedVoltagePower`) that push commands onto the shared `UndoStack`;
- Centralized editor refresh (`RefreshMotorEditorsFromCurrentMotor`) that keeps property textboxes, chart, and grid synchronized after undo/redo.

### 5. Follow-on Work and TODOs

- [ ] Implement the dedicated command-driven editing path for any remaining motor and drive metadata not yet covered by `EditMotorPropertyCommand` / `EditDrivePropertyCommand`, ensuring they participate in the same undo/redo history.
- [x] Apply the same command-driven pattern (per ADR-0003 and ADR-000X) to curve data table cells so that grid edits are always performed via undoable commands rather than direct model mutation.
- [ ] Add support for in-cell undo for data table edits, duplicating Avalonia's default per-TextBox undo behavior semantics (per-commit, not per-character) while routing all undo/redo operations through the shared global undo mechanism instead of control-local stacks.
- [ ] When adding new configuration panels or property groups, introduce editor buffers, an appropriate `Edit*PropertyCommand`, and LostFocus commit methods that push commands and refresh dependent views, rather than relying on direct two-way bindings to domain models.
 - [ ] When implementing any new undoable operation, follow ADR-000Y (Generalized Undo/Redo Command Pattern) as the primary checklist: add a single edit method on the view model, route all UI mutations through it, and ensure it constructs and pushes an `IUndoableCommand` onto the shared `UndoStack`.
