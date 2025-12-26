# Motor Torque Curve Editor - Terms and Definitions

This document establishes consistent terminology for the motor torque curve editor project.

---

## Core Concepts

### Motor Definition
The root domain object for this application. Represents the entirety of information contained in a motor definition file: a single motor with all base properties, unit settings, drive configurations, voltage configurations, curve series, and metadata.

### Motor Definition File
The JSON file on disk that serializes a `MotorDefinition`. It contains motor properties, unit settings, one or more drive configurations, each with one or more voltage configurations, each of which contains multiple curve series and data points.

### Curve (or Curve Series)
A `CurveSeries` describes a single named series of motor torque/speed data points for a specific drive and voltage configuration. Each curve represents a specific operating condition (e.g., "Peak" or "Continuous") and is stored as 0–101 data points.

The standard curve is 101 points at 1% increments (0%..100%), but files may contain fewer points. Percent values above 100 may be used to represent overspeed (manual JSON editing; the editor may treat these as view-only).

### Data Point
A single point on a curve, consisting of:
- **Percent**: non-negative integer representing position along the speed range (may exceed 100% for overspeed)
- **RPM**: Rotational speed at that percentage point
- **Torque**: Torque value at that speed

### Drive Configuration
A `DriveConfiguration` groups curve data by servo drive. Each drive has a name, optional drive-specific properties, and a collection of voltage configurations.

### Voltage Configuration
A `VoltageConfiguration` represents all curve series for a specific operating voltage of a given drive. It includes the numeric voltage value and a collection of `CurveSeries` objects.

### Motor Metadata
`MotorMetadata` tracks file-related and descriptive metadata for a motor definition (for example, author, creation and modification timestamps, and notes). It is updated automatically when structure-changing operations occur.

---

## Motor Properties

### Motor Name
The model name or identifier for the motor (e.g., "M-1234" or "High Torque Servo Motor").

### Manufacturer
The company that produces the motor.

### Part Number
The manufacturer's part number for the motor.

### Drive Name
The name of the servo drive used with the motor.

### Drive Part Number
The manufacturer's part number for the servo drive.

### Voltage
The operating voltage for the motor/drive combination (e.g., 48V, 320V).

---

## Speed and Torque Properties

### Max RPM
The maximum rotational speed of the motor in revolutions per minute.

### Rated RPM
The rated (continuous) operating speed of the motor.

### Rated Continuous Torque
The torque the motor can produce continuously without overheating.

### Rated Peak Torque
The maximum torque the motor can produce for short periods.

---

## Electrical Properties

### Continuous Amperage
The current draw during continuous operation at rated torque.

### Peak Amperage
The maximum current draw during peak torque operation.

### Power
The power output of the motor (typically in Watts, kW, or HP).

### Feedback PPR (Pulses Per Revolution)
The resolution of the motor feedback device (encoder or resolver), expressed as pulses per revolution. Used for motion control tuning and position accuracy calculations.

---

## Mechanical Properties

### Weight
The mass of the motor (in kg, lbs, or g).

### Rotor Inertia
The moment of inertia of the motor's rotor, affecting acceleration response.

### Has Brake (Boolean)
Indicates whether the motor includes an integral holding brake.

### Brake Torque
The holding torque of the integral brake (if present).

### Brake Amperage
The current draw of the brake (if present).

### Brake Voltage
The operating voltage of the integral brake (if present).

---

## Curve Types

### Peak Curve
The torque/speed curve representing maximum instantaneous torque capability. Motor cannot sustain these torque levels continuously.

### Continuous Curve
The torque/speed curve representing continuous operating capability. Motor can sustain these torque levels indefinitely without overheating.

### Custom Curve
Any user-defined curve beyond the default Peak and Continuous curves.

---

## User Interface Terms

### Series List
The UI panel showing all curves in the current motor definition with visibility checkboxes and color indicators.

### Properties Panel
The UI panel showing motor properties and curve data in editable form.

### Chart View
The graphical display showing curves as line graphs with axes, grid lines, and labels.

### Directory Browser
The side pane showing the file system for navigating and selecting motor definition files.

### Editing Coordinator
An internal coordination object shared between the chart and data table views. It tracks the currently selected curve points and ensures that selections and highlights stay in sync between visual and tabular editing surfaces.

### Chart Zoom and Pan
The chart is currently configured as a static view without interactive zoom or pan. Axes ranges are computed from the current motor definition and voltage configuration.

---

## File Operations

### Dirty State
Indicates the motor definition has unsaved changes. Displayed as an asterisk (*) in the window title.

### Save
Write the current motor definition to the currently loaded file (overwrites).

### Save As
Write the current motor definition to a new file; the new file becomes the active file.

### Save Copy As
Write a copy of the current motor definition to a new file; the original file remains active.

### Validation Errors
Problems detected in the current motor definition by the `ValidationService` (for example, missing drives, voltages, or invalid numeric ranges). When validation errors are present, the UI indicates them in a status area and disables the primary Save command.

### Clean Checkpoint
The last saved state of the current motor definition as tracked by the undo/redo system. When the user undoes changes back to this checkpoint, the dirty state is cleared and the window title no longer shows the unsaved indicator.

---

## Editing Concepts

### Undoable Command
An individual editing operation (such as changing a data point, renaming a series, or editing a motor property) that can be executed and then later undone. Commands implement a simple interface with `Execute` and `Undo` methods and are recorded in the undo history.

### Undo Stack
An in-memory history of undoable commands associated with the active motor definition. The stack supports undo and redo operations, exposes `CanUndo` and `CanRedo` for UI enablement, and is reset when a new motor definition is created or opened.

### Q Value
A parameter (0.0 to 1.0) that controls how changes to one point affect adjacent points when editing curves:
- **Low Q (0.0-0.3)**: Sharp/abrupt changes, affects fewer neighbors
- **High Q (0.7-1.0)**: Gradual changes, affects more adjacent points

### EQ-Style Editing
Point-and-drag curve editing similar to audio equalizer interfaces.

### Background Image Overlay
A reference image displayed behind the chart for curve verification against manufacturer data.

--

## Data Cell Editing Concepts

### Override Mode
A mode where typing directly overwrites the value of the selected cell(s) in the data table. The user does not need to enter Edit Mode first.

### Edit Mode
A mode where the user can "enter" the cell to directly edit the value within the cell. Works on one cell at a time in a data table. Activated by double-clicking a cell or pressing F2. The user may exit Edit Mode in a variety of ways:
- by pressing Enter
- or clicking outside the cell,
- or pressing Escape (cancels changes).
- or pressing Tab (moves to next cell)

---

## Units

### Torque Units
- **Nm**: Newton-meters (SI unit)
- **lbf-in**: Pound-force inches (Imperial)
- **oz-in**: Ounce-force inches (Imperial, smaller scale)

### Speed Units
- **rpm**: Revolutions per minute
- **rev/s**: Revolutions per second

### Power Units
- **W**: Watts
- **kW**: Kilowatts
- **hp**: Horsepower

### Weight Units
- **kg**: Kilograms
- **lbs**: Pounds
- **g**: Grams

### Voltage Units
- **V**: Volts
- **kV**: Kilovolts

### Current Units
- **A**: Amperes
- **mA**: Milliamperes

### Inertia Units
- **kg-m^2**: Kilogram square meters (SI inertia unit)
- **g-cm^2**: Gram square centimeters (metric inertia unit)

### Torque Constant Units
- **Nm/A**: Newton-meters per ampere

### Backlash Units
- **arcmin**: Arcminutes
- **arcsec**: Arc seconds

---

## Calculated Values

### Calculated Power
Power derived from torque and speed using the formula:
```
Power (W) = Torque (Nm) × Speed (rad/s)
Power (W) = Torque (Nm) × RPM × (2π / 60)
```

### Power Curve
A derived curve showing power vs. speed, calculated from the torque curve.

---

## Data Format

### Standard 1% Increment Storage
Standard curves use data points at every 1% of max speed (101 points total: 0% through 100%), but the file format supports 0–101 points per series.

### Percentage
A non-negative integer representing position along the motor's speed range (may exceed 100% for overspeed):
- 0% = 0 RPM
- 100% = Max RPM

---

## Future Concepts

### Tabbed Interface
Multiple motor definitions open simultaneously, each in its own tab.

### Power Overlay
A second Y-axis displaying calculated power curves overlaid on torque curves.
