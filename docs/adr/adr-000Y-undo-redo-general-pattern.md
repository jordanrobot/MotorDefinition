---
post_title: Generalized Undo/Redo Command Pattern
author1: jordanrobot
post_slug: adr-000Y-undo-redo-general-pattern
microsoft_alias: jordanrobot
featured_image: /assets/images/curve-editor.png
categories:
  - architecture
  - undo-redo
tags:
  - undo
  - redo
  - commands
  - avalonia
  - mvvm
ai_note: Initial draft synthesized with AI assistance.
summary: Generalized, repeatable pattern for adding new undoable operations in the Curve Editor using command objects, editor buffers, and global undo/redo wiring.
post_date: 2025-12-09
---

## Status

Accepted

## Context

By Phase 2 we have several independent features that participate in a shared undo/redo history:

- Motor text properties using `EditMotorPropertyCommand`
- Drive and voltage properties using `EditDrivePropertyCommand` and `EditVoltagePropertyCommand`
- Curve series point edits using `EditPointCommand`
- Series metadata (name, lock state, etc.) using `EditSeriesCommand`

Each of these went through multiple iterations before converging on a stable pattern. The core ideas are now consistent but the implementation details are spread across several ADRs (ADR-0003, ADR-000X) and code comments. This makes it harder for a new contributor—or an AI agent—to add a new undoable operation correctly on the first try.

We want a **single, generalized pattern** for adding new undo/redo cases that:

- Centralizes where domain state is mutated;
- Uses command objects for every undoable change;
- Keeps keyboard handling and editor behavior consistent;
- Plays nicely with dirty-state tracking and view refresh.

## Decision

We adopt a **generalized undo/redo pattern** based on explicit command objects and view-model edit methods. Any new undoable operation must follow these rules:

1. **All domain mutations go through commands**
   - Do not mutate `MotorDefinition`, `DriveConfiguration`, `VoltageConfiguration`, `CurveSeries`, or `DataPoint` directly from the view or code-behind.
   - Instead, create an `IUndoableCommand` implementation (or reuse an existing one) and execute it through the shared `UndoStack`.

2. **A single edit method per logical operation**
   - Each logical edit operation has a single public method on the appropriate view model.
   - Examples: `EditMotorName`, `EditSelectedVoltagePower`, `ToggleSeriesLock`, `TryEditTorqueCell`.
   - That method is the **only** place that:
     - Reads the current value from the model;
     - Parses or normalizes user input;
     - Constructs and pushes an undoable command.

3. **Editor buffers for text-based input**
   - UI `TextBox` controls bind **only** to simple editor properties on the view model (`MotorNameEditor`, `VoltagePowerEditor`, etc.), not directly to domain-model properties.
   - On commit (usually `LostFocus` or an explicit "Apply" action), the view or code-behind calls the corresponding edit method with the current editor value.
   - The edit method then pushes an undoable command which updates the underlying model.

4. **Commands always capture old and new values explicitly**
   - Command constructors receive `oldValue` and `newValue` as arguments, or determine them in a controlled way at command creation time.
   - `Execute()` applies the `newValue`.
   - `Undo()` restores the `oldValue`.
   - Commands do **not** infer "old" state by re-reading models during undo; they already know what to restore.

5. **Global undo/redo is wired once, at the window level**
   - `MainWindow` defines keyboard bindings for `Ctrl+Z` / `Ctrl+Y` and menu items bound to `UndoCommand` / `RedoCommand` on `MainWindowViewModel`.
   - Individual controls have `IsUndoEnabled="False"` where needed so they do not maintain their own private undo stacks.
   - View-level key handlers that run inside editors (e.g., grid cell `TextBox`) forward `Ctrl+Z` / `Ctrl+Y` to the global commands instead of using per-control undo.

6. **Undo stack is per-document and shared across subsystems**
   - `MainWindowViewModel` owns a single `UndoStack` instance per open motor/file.
   - Sub-view-models (`ChartViewModel`, `CurveDataTableViewModel`, etc.) receive a reference to the same stack.
   - All commands that mutate the current document push into this shared stack so undo/redo is linear and global.

7. **Locking and guards are enforced at command-creation boundaries**
   - Guards like "series is locked", "no motor selected", or "no voltage selected" are enforced **before** commands are created and pushed.
   - Example: `CurveDataTableViewModel.TryEditTorqueCell` checks `IsSeriesLocked(seriesName)` and early-outs instead of creating an `EditPointCommand`.
   - This avoids having commands that try to operate on invalid or forbidden state.

8. **Dirty state and refresh are centralized**
   - After any command is successfully pushed and executed, the caller must:
     - Call `MarkDirty()` to mark the document as modified;
     - Trigger appropriate refresh helpers: e.g., `RefreshMotorEditorsFromCurrentMotor`, `ChartViewModel.RefreshChart()`, `CurveDataTableViewModel.RefreshData()`.
   - Global `UndoCommand` / `RedoCommand` in `MainWindowViewModel` always:
     - Call `_undoStack.Undo()` / `_undoStack.Redo()`;
     - Then call the same refresh helpers so views stay consistent with the model.

## Generalized Implementation Steps

When adding a new undoable case, follow this checklist:

1. **Identify the domain property or data you want to change.**
   - Example: a new scalar property on `MotorDefinition`, a field on `DriveConfiguration`, a flag on `CurveSeries`, or a new column in the curve data table.

2. **Add or reuse a command type.**
   - If the change is a simple scalar on an existing model type, prefer reusing `EditMotorPropertyCommand`, `EditDrivePropertyCommand`, `EditVoltagePropertyCommand`, or `EditSeriesCommand`.
   - For point edits, use or extend `EditPointCommand`.
   - For more complex operations, create a new `IUndoableCommand` implementation that:
     - Accepts all required identifiers and `old/new` values in its constructor;
     - Stores them in fields;
     - Implements `Execute()` and `Undo()` purely in terms of those fields.

3. **Add a dedicated edit method on the appropriate view model.**
   - Name it after the user-level action (`EditX`, `ToggleX`, `ApplyXToSelection`, etc.).
   - Inside that method:
     - Validate required context (current motor/drive/voltage/series not null, indexes in range, series not locked, etc.).
     - Read the current value from the model into `oldValue`.
     - Parse/normalize the new value from editor state.
     - If `oldValue` equals `newValue`, early-out.
     - If `_undoStack` is `null`, apply the change directly (for tests or special environments).
     - Otherwise, construct the appropriate command with `old/new` values and call `_undoStack.PushAndExecute(command)`.
     - Call `MarkDirty()` and any necessary refresh helpers.

4. **Update bindings and UI interaction.**
   - For text input, bind `TextBox.Text` to an editor property (e.g., `NewPropertyEditor`) instead of the domain property.
   - In code-behind, handle `LostFocus` or an explicit "Apply" button to call the edit method on the view model.
   - For toggle buttons, bind to the source-generated command (`[RelayCommand]`) that wraps the edit method, not directly to the model property.
   - For grids or other complex widgets, ensure all mutation paths (single-cell edit, multi-cell apply, paste, clear, ESC revert) go through a single edit helper (like `TryEditTorqueCell`).

5. **Wire undo/redo shortcuts only once.**
   - Confirm that `MainWindow` has keyboard shortcuts and menu items bound to `UndoCommand` and `RedoCommand`.
   - In per-control key handlers, intercept `Ctrl+Z` / `Ctrl+Y` and forward to the VM commands instead of performing local undo.

6. **Extend tests.**
   - Add unit tests for the new command type (if newly introduced) to cover `Execute()` and `Undo()`.
   - Add a small view-model level test that:
     - Creates the view model with a test motor/drive/voltage/series;
     - Calls the new edit method;
     - Asserts the model changed as expected;
     - Calls `Undo()` and asserts the old value is restored;
     - Calls `Redo()` and asserts the new value is applied again.

## Consequences

- **Pros**
  - Predictable, consistent undo/redo behavior across all features.
  - New undoable cases can be implemented quickly by following the checklist.
  - Tests are easier to write and reason about, since all mutations go through explicit commands and edit methods.
  - Global keyboard behavior (Ctrl+Z / Ctrl+Y) remains simple and centralized.

- **Cons**
  - Slightly more ceremony: each new editable property usually needs an editor buffer, an edit method, and (optionally) a new command type.
  - Requires discipline to avoid "just setting the property" directly from the view.

## How to Use This ADR

When adding or reviewing code that changes application state in a way the user might reasonably expect to undo:

- Start with this ADR and walk through the **Generalized Implementation Steps**.
- Cross-check with ADR-0003 and ADR-000X for concrete examples.
- Ensure that:
  - There is exactly one edit method for the operation;
  - That method uses a command and the shared undo stack;
  - All UI interactions route through that method.

This document is the primary reference for future undo/redo work in the Curve Editor.
