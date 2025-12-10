# ADR-000X: Voltage Property Undo Design

## Status

Accepted

## Context

Phase 2 introduced command-driven undo/redo for motor properties per ADR-0003. As part of extending this behavior to drive and voltage properties, we needed a consistent pattern for selected-voltage fields (scalar values and series-related values) so that:

- All user edits to undoable voltage fields are represented as discrete commands on the shared `UndoStack`.
- Per-TextBox undo is disabled, and the global undo/redo commands (Ctrl+Z / Ctrl+Y) are the single source of truth.
- Voltage edits keep the chart and curve data table in sync, and undo/redo visibly updates the textboxes.

Voltage properties include both scalar fields and series-related fields on `VoltageConfiguration`:

- Scalars: `Voltage`, `Power`, `MaxSpeed`.
- Series-related: `RatedPeakTorque`, `RatedContinuousTorque`, `ContinuousAmperage`, `PeakAmperage`.

Additionally, the chart x-axis scale depends on the larger of the motor max speed and the drive/voltage max speed, so changes to voltage max speed must be reflected in both the model and the chart.

## Decision

We applied the same command-based pattern used for motor properties to selected-voltage properties, with the following concrete choices:

1. **Dedicated command type for voltage properties**

- Introduced `EditVoltagePropertyCommand : IUndoableCommand` in `Services/EditMotorPropertyCommand.cs`.
- The command captures:
  - Target `VoltageConfiguration` instance.
  - Reflected `PropertyInfo` for the named property.
  - Explicit `oldValue` and `newValue`.
- `Execute()` sets the underlying property to `newValue`; `Undo()` restores `oldValue`.
- `Description` is set to `"Edit voltage property '<PropertyName>'"` for logging.

This mirrors `EditMotorPropertyCommand` and `EditDrivePropertyCommand` and keeps reflection-based property changes in a single place.

2. **View-model editor buffers for selected-voltage fields**

- Added editor properties to `MainWindowViewModel`:
  - Scalars:
    - `VoltageValueEditor`
    - `VoltagePowerEditor`
    - `VoltageMaxSpeedEditor`
  - Series-related:
    - `VoltagePeakTorqueEditor`
    - `VoltageContinuousTorqueEditor`
    - `VoltageContinuousAmpsEditor`
    - `VoltagePeakAmpsEditor`
- These are simple `string` properties (`[ObservableProperty]`) that back the TextBoxes, decoupling the UI from the domain model in the same way as `MotorNameEditor`, etc.

3. **Centralized refresh of voltage editors from model state**

- Extended `RefreshMotorEditorsFromCurrentMotor()` to:
  - Clear all voltage editor buffers when `CurrentMotor` is `null`.
  - When `CurrentMotor` is not `null`, read from `SelectedVoltage` (if present):
    - `VoltageValueEditor`  `SelectedVoltage.Voltage`
    - `VoltagePowerEditor`  `SelectedVoltage.Power`
    - `VoltageMaxSpeedEditor`  `SelectedVoltage.MaxSpeed`
    - `VoltagePeakTorqueEditor`  `SelectedVoltage.RatedPeakTorque`
    - `VoltageContinuousTorqueEditor`  `SelectedVoltage.RatedContinuousTorque`
    - `VoltageContinuousAmpsEditor`  `SelectedVoltage.ContinuousAmperage`
    - `VoltagePeakAmpsEditor`  `SelectedVoltage.PeakAmperage`
- Ensured that `OnSelectedVoltageChanged` also initializes these editor buffers from the new `SelectedVoltage`.

This guarantees that after any undo/redo (which calls `RefreshMotorEditorsFromCurrentMotor()`), the textboxes reflect the model state.

4. **Explicit edit methods on the view model for each voltage property**

For each undoable voltage property we added a method on `MainWindowViewModel` that:

- Early-outs if `SelectedVoltage` is `null`.
- Reads the current value from the model (`oldValue`).
- Parses the editor buffer text (`Voltage*Editor`) into a `double` via a shared `TryParseDouble` utility, falling back to `oldValue` on invalid input.
- Early-outs if `oldValue` and `newValue` are effectively equal (tolerance check).
- Constructs `EditVoltagePropertyCommand(SelectedVoltage, propertyName, oldValue, newValue)` and calls `_undoStack.PushAndExecute(command)`.
- Updates the corresponding editor buffer from `newValue` and calls:
  - `ChartViewModel.RefreshChart()`;
  - `CurveDataTableViewModel.RefreshData()`;
  - Sets `IsDirty = true`.

Concrete methods:

- Scalars:
  - `EditSelectedVoltageValue()`
  - `EditSelectedVoltagePower()`
  - `EditSelectedVoltageMaxSpeed()`
- Series-related:
  - `EditSelectedVoltagePeakTorque()`
  - `EditSelectedVoltageContinuousTorque()`
  - `EditSelectedVoltageContinuousAmps()`
  - `EditSelectedVoltagePeakAmps()`

5. **LostFocus handlers commit edits via the command path**

In `MainWindow.axaml` and `MainWindow.axaml.cs`:

- Each selected-voltage TextBox is bound to its editor property and has `IsUndoEnabled="False"`.
- `UpdateSourceTrigger=LostFocus` is used, and code-behind LostFocus handlers call the corresponding view-model methods:
  - `OnVoltageValueLostFocus`  `EditSelectedVoltageValue()`
  - `OnVoltagePowerLostFocus`  `EditSelectedVoltagePower()`
  - `OnVoltagePeakTorqueLostFocus`  `EditSelectedVoltagePeakTorque()`
  - `OnVoltageContinuousTorqueLostFocus`  `EditSelectedVoltageContinuousTorque()`
  - `OnVoltageContinuousAmpsLostFocus`  `EditSelectedVoltageContinuousAmps()`
  - `OnVoltagePeakAmpsLostFocus`  `EditSelectedVoltagePeakAmps()`
- For **voltage max speed**, we aligned the behavior with other properties while preserving the confirmation dialog:
  - `OnMaxSpeedLostFocus` now first calls `EditSelectedVoltageMaxSpeed()` so the edit flows through the command/undo stack.
  - Then it compares `SelectedVoltage.MaxSpeed` against `_previousMaxSpeed` to optionally call `ConfirmMaxSpeedChangeAsync()` and refresh the chart.

This ensures that voltage max speed participates in undo/redo like the rest of the properties.

6. **Undo/Redo integration and logging**

- The `Undo` and `Redo` commands on `MainWindowViewModel` call `_undoStack.Undo()` / `_undoStack.Redo()`, then:
  - `RefreshMotorEditorsFromCurrentMotor()`;
  - `ChartViewModel.RefreshChart()`;
  - `CurveDataTableViewModel.RefreshData()`.
- We added Serilog debug logging in `UndoStack` and in the voltage edit methods to verify behavior in production:
  - `UndoStack: Executing and pushing command 'Edit voltage property 'Power''` etc.
  - `UndoStack: Undoing command 'Edit voltage property 'Power''` etc.
  - `EditSelectedVoltagePower: old=1500, new=1000` etc.

These logs confirmed that voltage commands are pushed and undone in the correct order, and that UI issues were due to missing editor refresh wiring, not the undo core.

## Consequences

Positive consequences:

- Voltage scalar and series properties now behave like motor and drive properties in undo/redo:
  - Every committed edit becomes a single undo step.
  - Ctrl+Z / Ctrl+Y operates across chart, table, and property textboxes consistently.
- The separation between editor buffers and domain model makes it straightforward to add additional validation, dialogs, or side effects around voltage edits without compromising undo behavior.
- The pattern scales to other sets of properties (e.g., future drive-level fields or per-series metadata) by:
  - Introducing a dedicated command type if needed.
  - Adding editor buffers.
  - Wiring LostFocus handlers to view-model edit methods that push commands.

Negative/neutral consequences:

- More boilerplate in `MainWindowViewModel` and `MainWindow` (editor properties and edit methods) compared to direct bindings, though this is aligned with ADR-0003 and considered acceptable for predictable undo behavior.
- The reflection-based commands rely on property name strings; refactor-safe navigation requires tests or tooling support. Centralizing the commands mitigates this risk.

## Applicability to Future Undo Scenarios

When implementing undo for other property groups (e.g., additional drive metadata, future configuration panels, or grid-driven scalar edits), follow this pattern:

1. Identify the owning domain type and properties that should be undoable.
2. If not already present, add a dedicated `Edit*PropertyCommand` class targeting that domain type, modeled after `EditVoltagePropertyCommand`.
3. In the relevant view model:
   - Add editor buffer properties for each undoable field.
   - Provide methods `Edit*` that:
     - Read `oldValue` from the model;
     - Parse the editor value;
     - Early-out on no-op;
     - Push a command with `oldValue`/`newValue` via `_undoStack.PushAndExecute`;
     - Update the editor property from the canonical model value;
     - Refresh any dependent views and mark the document dirty.
   - Ensure the global `Undo`/`Redo` commands call back into a central refresh method that updates all editors from the model.
4. In the view (XAML + code-behind):
   - Bind TextBoxes to editor properties.
   - Disable per-control undo.
   - Route LostFocus (or equivalent commit events) to the view-model edit methods.

By mirroring this voltage property undo design, future agents can implement undoable edits in new areas of the UI without re-discovering the interaction between editor buffers, reflection-based commands, and global undo/redo.
