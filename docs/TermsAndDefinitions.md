---
layout: default
title: Terms and Definitions
---

[Docs Home](index.md) | [Quick Start](QuickStart.md) | [User Guide](UserGuide.md) | [Terms and Definitions](TermsAndDefinitions.md) | [API documentation](api/index.md)

## Terms and Definitions

This page defines the terminology used across MotorDefinition (the JSON file format, the `JordanRobot.MotorDefinition` library, and the editor).

### Core objects (the data hierarchy)

Motor definition files follow this structure:

- **Motor** (`ServoMotor`)
- **Drive(s)** (`Drive`)
- **Voltage(s)** (`Voltage`)
- **Curve(s)** (`Curve`)
- **Data point(s)** (`DataPoint`)

This mirrors real-world usage: a motor may be paired with different drives, and a given drive may be operated at different voltages, each producing different performance curves.

### Motor

The **Motor** is the root domain object (`ServoMotor`). It represents everything contained in a motor definition file:

- Base motor metadata (name, manufacturer, part number)
- Ratings and limits (max speed, rated torques, etc.)
- Unit settings
- A collection of drives, voltages, curves, and curve data
- File-related metadata (author/notes/timestamps)

Related terms:

- **Motor definition file**: the JSON file on disk that serializes a `ServoMotor`.

### Drive

A **Drive** groups curve data by servo drive. Drives are accessed via `ServoMotor.Drives`.

Typical fields include a drive name and optional drive-specific metadata (manufacturer, part number).

### Voltage

A **Voltage** represents all curves for a specific operating voltage of a given drive.

- The voltage value is typically a numeric nominal voltage (for example, 48V, 208V, 320V).
- A voltage often has a max speed at that voltage.

Voltages are accessed via `Drive.Voltages`.

### Curve (performance curve / series)

A **Curve** is a named series of torque/speed data for a specific drive and voltage (for example, “Peak” or “Continuous”). Curves are accessed via `Voltage.Curves`.

Notes:

- The standard curve shape is **101 points** at **1% increments** (0%..100%).
- The file format allows **0 to 101 points per curve**.
- Percent values above 100% are allowed (overspeed) and may be treated as view-only in the editor.

### Data point

A **Data point** is a single row within a curve:

- **Percent**: non-negative integer position along the speed range (may exceed 100% for overspeed)
- **RPM**: rotational speed corresponding to that percent
- **Torque**: torque value at that RPM

Critical rule:

- Within a single `Voltage`, all curves are expected to share the same percent and RPM axes.

### Motor metadata

**Motor metadata** (`MotorMetadata`) tracks file-related and descriptive metadata for a motor definition (for example author, creation/modification timestamps, and notes).

## Motor and drive properties (common fields)

### Identification

- **Motor name**: model name or identifier for the motor.
- **Manufacturer**: company that produces the motor.
- **Part number**: manufacturer’s part number.
- **Drive name**: name/identifier for the servo drive.
- **Drive part number**: manufacturer’s part number for the drive.

### Speed and torque

- **Max RPM**: maximum rotational speed of the motor.
- **Rated continuous torque**: torque the motor can produce continuously.
- **Rated peak torque**: torque the motor can produce for short periods.

## UI terms (editor)

These terms apply to the editor application.

### Series list

The panel showing all curves in the current motor definition, typically with visibility toggles.

### Properties panel

The panel showing motor properties and curve data in editable form.

### Chart view

The graph display that renders curves as line graphs.

### Directory browser and root folder

- **Directory browser**: the file system pane for selecting motor definition files.
- **Root folder**: the top-level folder used to scope what the directory browser shows.

### Dirty state

Indicates there are unsaved changes. This is typically shown as an asterisk (*) in the window title.

### Save / Save As / Save Copy As

- **Save**: write the current motor definition to the currently loaded file (overwrite).
- **Save As**: write the current motor definition to a new file; that file becomes the active file.
- **Save Copy As**: write a copy to a new file; the original file remains active.

### Validation errors

Problems detected in the current motor definition by validation logic (for example missing drives/voltages, invalid ranges, or invalid curve shape). When validation errors are present, the editor may disable Save.

## Editing and data table terms (editor)

### Undoable command and undo stack

- **Undoable command**: a single edit operation that can be executed and later undone.
- **Undo stack**: the in-memory history that enables undo/redo, typically reset when opening/creating a new file.

### Q value

A parameter ($0.0$ to $1.0$) that controls how edits to one point affect neighboring points:

- **Low Q (0.0–0.3)**: sharper changes; affects fewer neighbors
- **High Q (0.7–1.0)**: smoother changes; affects more neighbors

### Override mode and edit mode

- **Override mode**: typing overwrites the selected cell(s) directly.
- **Edit mode**: the user enters a cell for in-place editing (commonly via double-click or F2), then commits or cancels.

## Units (common)

Typical units seen in the format and editor:

- **Torque**: Nm, lbf-in, oz-in
- **Speed**: rpm, rev/s
- **Power**: W, kW, hp
- **Weight**: kg, lbs, g
- **Voltage**: V, kV
- **Current**: A, mA
- **Inertia**: kg-m^2, g-cm^2

## Derived values

### Calculated power

Power can be derived from torque and speed as:

```text
Power (W) = Torque (Nm) × Speed (rad/s)
Power (W) = Torque (Nm) × RPM × (2π / 60)
```
