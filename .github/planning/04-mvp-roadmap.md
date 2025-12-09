# Motor Torque Curve Editor - MVP Roadmap

**Related ADRs**

- ADR-0003 Motor Property Undo Design (`.github/adr/adr-0003-motor-property-undo-design.md`)
- ADR-000X Voltage Property Undo Design (`.github/adr/adr-000X-voltage-property-undo-design.md`)

## Technology Stack Decision

**Confirmed**: The Motor Torque Curve Editor will be built using:

- **Framework**: Avalonia UI 11.x
- **Runtime**: .NET 8 (LTS)
- **Charting**: LiveCharts2
- **MVVM**: CommunityToolkit.Mvvm
- **Logging**: Serilog (structured logging)

This provides a cross-platform capable, modern .NET desktop application with excellent charting support.

---

## Phase Overview

```
Phase 1: Foundation (Week 1-2)
    └── Project setup, basic UI shell, file operations, curve generator
    └── Logging infrastructure (Serilog), exception handling
    └── Undo/redo infrastructure (command pattern)

Phase 2: Core Features (Week 3-4)
    └── Chart visualization, motor properties, data binding, grid lines

Phase 3: File Management (Week 5)
    └── Directory browser, dirty state, save prompts, Save As/Copy As

Phase 4: Advanced Editing (Week 6-8)
    └── EQ-style editing, Q slider, background images, axis scaling

Phase 5: Polish & User Preferences (Week 9)
    └── User settings, keyboard shortcuts

Phase 6: Units System (Future)
    └── Tare integration, unit conversion, preferences

Phase 7: Tabbed Interface (Future)
    └── Multiple files open simultaneously in tabs

Phase 8: Power Curve Overlay (Future)
    └── Calculated power curves, dual Y-axis display
```

---

## Phase 1: Foundation

### 1.1 Project Setup
- [X] Create solution with Avalonia template
- [X] Configure project for .NET 8
- [X] Add NuGet packages (Avalonia, CommunityToolkit.Mvvm, Serilog, Serilog.Sinks.File, Serilog.Sinks.Console)
- [X] Set up project structure (Models, Views, ViewModels, Services)
- [X] Configure single-file publishing
- [X] Set up Serilog logging infrastructure
- [X] Configure global exception handling

### 1.2 Basic Window Shell
- [X] Create MainWindow with menu bar
- [X] Implement basic layout (directory pane + chart area + properties panel)
- [X] Add File menu (New, Open, Save, Save As, Save Copy As, Exit)
- [X] Implement window title showing current file
- [X] Add asterisk (*) to title when file is dirty

### 1.3 Data Models
- [X] Create MotorDefinition model class (all motor properties)
- [X] Create CurveSeries model class (named curve)
- [X] Create DataPoint model class (percent, rpm, torque)
- [X] Create UnitSettings model class (torque, speed, power, weight units)
- [X] Create MotorMetadata model class
- [X] Implement 1% increment data structure (101 points per series)
- [X] Add JSON serialization attributes
- [X] Write model unit tests

### 1.4 File Service
- [X] Implement JSON loading
- [X] Implement JSON saving (always overwrites)
- [X] Implement Save As (save to new file, becomes active)
- [X] Implement Save Copy As (save copy, original stays active)
- [X] Handle file dialogs
- [X] Error handling for invalid files
- [X] Write service unit tests

### 1.5 Curve Generator Service
- [X] Create ICurveGeneratorService interface
- [X] Implement curve interpolation from max parameters
- [X] Generate curves at 1% increments
- [X] Calculate corner speed (constant torque to constant power transition)
- [X] Power calculation from torque and speed
- [X] Write generator unit tests

### 1.6 New Motor Definition Wizard
- [X] "New Motor" command in File menu
- [X] Dialog to enter basic motor parameters:
  - Motor name
  - Max RPM
  - Max torque
  - Max power
- [X] Generate initial Peak and Continuous curves
- [X] Create new file with generated data

### 1.7 Logging and Exception Handling
- [x] Configure Serilog with file and console sinks
- [x] Add structured logging throughout services
- [x] Implement global unhandled exception handler
- [x] Create user-friendly error dialogs for exceptions
- [x] Log file location in user's AppData folder
- [x] Include context (file path, operation) in log entries

### 1.8 Undo/Redo Infrastructure
- [x] Implement IUndoableCommand interface
- [x] Create UndoStack service for managing per-document edit history and exposing CanUndo/CanRedo and UndoDepth
- [x] Define command classes for common operations with reliable Execute/Undo semantics and logging on failure:
  - EditPointCommand
  - EditSeriesCommand
  - EditMotorPropertyCommand
- [x] Wire Ctrl+Z / Ctrl+Y keyboard shortcuts
- [x] Integrate undo/redo with dirty state tracking, including a clean checkpoint tied to saves so undoing back to the saved state clears the dirty flag

See ADR-0003 (`.github/adr/adr-0003-motor-property-undo-design.md`) for the architectural decision and migration plan for bringing motor-level text properties under the undo/redo system via a dedicated command-driven editing path.

See ADR-000X (`.github/adr/adr-000X-voltage-property-undo-design.md`) for the analogous design applied to drive/voltage properties, including the use of editor buffers, `EditVoltagePropertyCommand`, and LostFocus-based commit methods so that selected-voltage fields (scalars and series-related) participate in the same global undo/redo history.

**Note on Motor, Drive, and Voltage Properties:**

Motor-level text properties (e.g., Motor Name, Manufacturer, Part Number) are wired through explicit view-model edit methods and `EditMotorPropertyCommand` instances that store old and new values up front. The motor text boxes bind to simple editor properties (e.g., `MotorNameEditor`) and commit changes via these methods on focus loss, with `TextBox`-local undo disabled. Ctrl+Z / Ctrl+Y are handled at the window level and operate on the shared per-document `UndoStack`, so motor property edits participate in the same undo/redo history as chart and grid edits. See ADR-0003 (`.github/adr/adr-0003-motor-property-undo-design.md`) for the finalized design and rationale.

Drive and selected-voltage properties (scalars and series-related) follow the same pattern using `EditDrivePropertyCommand` and `EditVoltagePropertyCommand`, plus editor buffers such as `DriveNameEditor`, `VoltagePowerEditor`, and `VoltagePeakTorqueEditor`. TextBoxes bind to these editor properties with `IsUndoEnabled = false` and commit on LostFocus via view-model methods (e.g., `EditDriveName`, `EditSelectedVoltagePower`) that push commands onto the shared `UndoStack`. See ADR-000X (`.github/adr/adr-000X-voltage-property-undo-design.md`) for details.

On undo/redo, the main view model calls back into a central refresh method (e.g., `RefreshMotorEditorsFromCurrentMotor`) and then refreshes the chart and data table so that all property textboxes, the chart axes, and the grid stay synchronized with the current undo state.

**Future Refinements Using ADR-0003 and ADR-000X Patterns:**

To keep the codebase cohesive and efficient, a future agent should consider how the command-driven edit patterns from ADR-0003 and ADR-000X can be applied more broadly:

- **Motor and drive scalar properties:** Route all edits (name, manufacturer, part number, numeric specs) through explicit view-model edit methods that create undoable commands, instead of direct two-way bindings to domain models.
- **Additional configuration panels:** When adding new groups of scalar properties (e.g., future drive metadata, per-series configuration), add editor buffers, an `Edit*PropertyCommand`, and LostFocus commit methods that push commands and refresh dependent views.
- **Curve data table and chart edits:** Ensure grid cell edits and (future) chart drag edits both go through shared edit methods (e.g., `EditPointTorque`) that push `EditPointCommand`s, rather than letting the UI mutate `DataPoint` instances directly.
- **Selection and coordination logic:** Centralize selection changes (from chart and table) through coordinator/view-model APIs that record origin and avoid feedback loops, making it easier to reason about and test selection behavior.

These refinements should be evaluated against the current codebase when planning advanced editing work (Phase 4) so that undo/redo behavior, selection coordination, and mutation paths remain consistent and maintainable.

**Deliverable:** Application that can create, open, and save motor files with JSON content display. Includes full undo/redo support and structured logging.

---

## Phase 2: Core Features

### 2.1 Chart Integration
- [X] Add LiveCharts2 NuGet package
- [X] Create ChartView component
- [X] Bind chart to MotorData with multiple series
- [X] Configure default axes (RPM = X, Torque = Y)
- [X] Display RPM values rounded to nearest whole number
- [X] Style chart appearance

### 2.2 Multiple Series Display
- [X] Load and display multiple curve series from file
- [X] Create default series ("Peak", "Continuous") for new files
- [X] Each series rendered as separate line on chart
- [X] Distinguish series by unique line colors
- [X] Series legend with names and colors

### 2.3 Grid Lines and Axis Labels
- [X] Configure axis labels at rounded increments
  - RPM: 500, 1000, 1500, 2000, etc.
  - Torque: 5, 10, 15, 20, etc.
- [X] Add faded grid lines extending across graph
- [X] Implement auto-calculation for label spacing (avoid crowding)
- [X] Labels update smoothly when axis range changes

### 2.4 Hover Tooltip
- [X] Show RPM and torque values on hover near curve
- [X] Position tooltip to not obscure cursor
- [X] Style tooltip for readability

### 2.5 Series Management Panel
- [X] Create series list panel in UI
- [X] Checkbox for each series to show/hide visibility
- [X] Display series color swatch next to name
- [X] Editable series name field
- [X] Add Series button
- [X] Delete Series button (with confirmation)

### 2.6 Motor Properties Panel
- [X] Create MotorPropertiesPanel component
- [X] Display all motor properties (editable):
  - Motor name, manufacturer, part number
  - Drive name, drive part number
  - Voltage, amperage (continuous/peak)
  - Max/rated RPM
  - Continuous/peak torque
  - Power, weight, rotor inertia
  - Brake properties (hasBrake, torque, amperage)
- [X] Unit selector for each property type
- [X] Bind properties to MotorDefinition model
- [X] Enable editing of all fields

Note: Motor text property edits (e.g., name, manufacturer, part number) are routed through explicit view-model edit methods and undoable commands as described in ADR-0003 (`.github/adr/adr-0003-motor-property-undo-design.md`).

### 2.7 Curve Data Panel
- [X] Create CurveDataPanel component
- [X] Display data grid of points for selected series
- [X] Bind grid to curve data
- [X] Enable editing in grid cells
- [X] RPM values displayed rounded to whole numbers

### 2.8 Two-Way Binding
- [X] Chart updates when grid values change
- [X] Grid updates when chart is modified (future)
- [X] Implement INotifyPropertyChanged throughout
- [X] Handle dirty state tracking (unsaved changes)

### 2.9 Basic Validation
- [X] Validate RPM values (positive, ascending)
- [X] Validate torque values (non-negative)
- [X] Show validation errors in UI
- [X] Prevent saving invalid data

**Deliverable:** Application with working multi-series chart, series visibility toggles, and editable data grid.

---

## Phase 3: File Management

### 3.0 Update JSON schema
- [ ] Use new schema file provied by user for JSON schema for motor files
- [ ] Refactor existing JSON serialization/deserialization to match new schema
- [ ] present options for consolidating series data within drive&voltage sections. This is to group series torque values together so that veiwing and editing raw json files is much easier. 
- [ ] If the user chooses to adjust series data format within the json schema, implement this change.

### 3.1 Directory Browser (VS Code-style)
- [ ] Create side pane for directory browsing
- [ ] "Open Folder" command to select directory
- [ ] List JSON files in selected directory
- [ ] Click file in list to load into editor
- [ ] Refresh button to reload file list
- [ ] Show current directory path

### 3.2 Dirty State Tracking
- [ ] Track unsaved changes per file
- [ ] Mark file dirty when any edit is made
- [ ] Clear dirty state when file is saved
- [ ] Show asterisk (*) in window title when dirty
- [ ] Visual indicator in directory list for dirty files

### 3.3 Save Prompts
- [ ] Prompt to save when closing app with dirty file
- [ ] Prompt to save when opening new file with dirty file active
- [ ] Dialog with Save / Don't Save / Cancel options
- [ ] Handle Cancel to abort the close/open operation

### 3.4 Save Commands
- [ ] Save command overwrites current file
- [ ] Save As: save to new file, new file becomes active
- [ ] Save Copy As: save copy to new file, original stays active
- [ ] All saves overwrite (no append mode)

**Deliverable:** Full file management with directory browser and save prompts.

---

## Phase 4: Advanced Editing

### 4.1 EQ-Style Curve Editing
- [ ] Enable point selection on chart (single-click on point)
- [ ] Implement rubber-band selection on chart (click-drag rectangle to select multiple points)
- [ ] Ensure chart selections stay synchronized with table selection via `EditingCoordinator`
- [ ] Implement point dragging (drag selected points up/down to adjust torque)
- [ ] Provide clear visual feedback for selection and drag operations
- [ ] Sync dragged torque changes back to the data model in real-time

### 4.2 Q Value Control
- [ ] Add Q slider (range 0.0 to 1.0)
- [ ] Q affects curve sharpness when editing
  - Low Q = sharp/abrupt changes (affects fewer neighbors)
  - High Q = gradual changes (affects more adjacent points)
- [ ] Visual indication of affected zone when dragging
- [ ] Q value persists during editing session

### 4.3 Background Image Overlay
- [ ] Add "Load Background Image" menu/button
- [ ] Support PNG, JPG, BMP image formats
- [ ] Render image behind chart (z-order below curve)
- [ ] X-axis scale slider for image
- [ ] Y-axis scale slider for image (independent)
- [ ] Position offset controls (optional)
- [ ] Toggle image visibility on/off

### 4.4 Axis Scaling
- [ ] X-axis range slider (min/max RPM)
- [ ] Y-axis range slider (min/max Torque)
- [ ] Smooth graph recalculation (no jitter)
- [ ] Grid lines and labels update with scaling
- [ ] Maintain nice rounded label increments

### 4.5 Add/Remove Data Points
- [ ] Add "Insert Point" button
- [ ] Add "Delete Point" button
- [ ] Handle insertion between existing points
- [ ] Update chart and grid automatically

### 4.6 Selection Coordination & Feedback Loops
- [ ] Model selection changes through a shared `EditingCoordinator`
- [ ] Introduce an explicit selection origin flag (e.g., Table vs Chart) on coordinator APIs/events
- [ ] Use origin information to avoid selection feedback loops between chart and table
- [ ] Keep selection semantics (replace, extend, toggle) consistent between chart and table

**Deliverable:** Fully functional MVP with EQ-style editing and image overlay.

---

## Phase 5: Polish & User Preferences

### 5.1 User Settings
- [ ] Toggle hover tooltip on/off
- [ ] Series color editor (color picker per series)
- [ ] Save series colors per series name (persistent)
- [ ] Colors consistent across different files
- [ ] Persist settings between sessions
- [ ] Settings accessible via menu

### 5.2 UI Polish
- [ ] Add toolbar with common actions
- [ ] Implement keyboard shortcuts
- [ ] Add status bar
- [ ] Add application icon

### 5.3 Unsaved Changes Handling
- [ ] Prompt to save on close
- [ ] Prompt to save on open new file
- [ ] Show asterisk (*) in title for unsaved changes

**Deliverable:** Polished application with user preferences.

---

## Phase 6: Units System (Future)

### 6.1 Tare Integration
- [ ] Add Tare NuGet package
- [ ] Create UnitService using Tare
- [ ] Define supported units (Nm, lbf-in)

### 6.2 Unit Toggle UI
- [ ] Add unit selector in UI
- [ ] Store current unit preference
- [ ] Convert displayed values on toggle

### 6.3 Conversion Logic
- [ ] Convert on display (not stored data)
- [ ] Or: Convert stored data (with user confirmation)
- [ ] Handle precision/rounding

### 6.4 Persistence
- [ ] Save unit preference
- [ ] Remember last used unit
- [ ] Store unit in JSON file (optional)

**Deliverable:** Full unit system support.

---

## Phase 7: Tabbed Interface (Future)

### 7.1 Tab Management
- [ ] Tab bar for multiple open files
- [ ] Each file in its own tab
- [ ] Click tab to switch between files
- [ ] Close button on each tab

### 7.2 Independent State
- [ ] Each tab has independent dirty state
- [ ] Each tab has independent undo/redo history
- [ ] Prompt to save when closing dirty tab

### 7.3 Tab Behavior
- [ ] New file opens in new tab
- [ ] Double-click file in directory browser opens new tab
- [ ] Option to open in current tab vs new tab
- [ ] Drag tabs to reorder

**Deliverable:** Multi-file editing with tabbed interface.

---

## Phase 8: Power Curve Overlay (Future)

### 8.1 Power Calculation
- [ ] Calculate power from torque and speed: P = T × ω
- [ ] Generate power curve from each torque curve
- [ ] Support kW and HP display units

### 8.2 Dual Y-Axis Chart
- [ ] Add secondary Y-axis for power (right side)
- [ ] Primary Y-axis: Torque (left side)
- [ ] Label axes appropriately
- [ ] Power curves distinguished by color

### 8.3 Power Overlay Toggle
- [ ] Menu option to show/hide power curves
- [ ] Toggle per-series power visibility
- [ ] User preference to remember setting

**Deliverable:** Power curves overlaid on torque chart.

---

## Quick Start Commands

### Create New Project

```bash
# Install Avalonia templates
dotnet new install Avalonia.Templates

# Create new Avalonia MVVM project
dotnet new avalonia.mvvm -n CurveEditor -o src/CurveEditor

# Add packages
cd src/CurveEditor
dotnet add package LiveChartsCore.SkiaSharpView.Avalonia
dotnet add package CommunityToolkit.Mvvm
dotnet add package Serilog
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Console
```

### Build and Run

```bash
# Development
dotnet run --project src/CurveEditor

# Publish portable executable
dotnet publish src/CurveEditor -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## Sample JSON File

Create `samples/example-motor.json` for testing:

```json
{
  "motorName": "High Torque Servo Motor",
  "manufacturer": "Acme Motors",
  "partNumber": "M-1234-HT",
  "driveName": "Servo Drive Pro",
  "drivePartNumber": "SD-5000",
  "voltage": 48,
  "maxRpm": 5000,
  "ratedRpm": 3000,
  "ratedContinuousTorque": 45.0,
  "ratedPeakTorque": 55.0,
  "continuousAmperage": 10.5,
  "peakAmperage": 25.0,
  "power": 1500,
  "weight": 8.5,
  "hasBrake": true,
  "brakeTorque": 12.0,
  "brakeAmperage": 0.5,
  "rotorInertia": 0.0025,
  "units": {
    "torque": "Nm",
    "speed": "rpm",
    "power": "W",
    "weight": "kg"
  },
  "series": [
    {
      "name": "Peak",
      "data": [
        { "percent": 0, "rpm": 0, "torque": 55.0 },
        { "percent": 10, "rpm": 500, "torque": 57.0 },
        { "percent": 20, "rpm": 1000, "torque": 56.0 },
        { "percent": 30, "rpm": 1500, "torque": 54.0 },
        { "percent": 40, "rpm": 2000, "torque": 51.0 },
        { "percent": 50, "rpm": 2500, "torque": 47.0 },
        { "percent": 60, "rpm": 3000, "torque": 42.0 },
        { "percent": 70, "rpm": 3500, "torque": 36.0 },
        { "percent": 80, "rpm": 4000, "torque": 29.0 },
        { "percent": 90, "rpm": 4500, "torque": 21.0 },
        { "percent": 100, "rpm": 5000, "torque": 14.0 }
      ]
    },
    {
      "name": "Continuous",
      "data": [
        { "percent": 0, "rpm": 0, "torque": 45.0 },
        { "percent": 10, "rpm": 500, "torque": 46.0 },
        { "percent": 20, "rpm": 1000, "torque": 45.5 },
        { "percent": 30, "rpm": 1500, "torque": 44.0 },
        { "percent": 40, "rpm": 2000, "torque": 41.5 },
        { "percent": 50, "rpm": 2500, "torque": 38.0 },
        { "percent": 60, "rpm": 3000, "torque": 33.5 },
        { "percent": 70, "rpm": 3500, "torque": 28.0 },
        { "percent": 80, "rpm": 4000, "torque": 22.0 },
        { "percent": 90, "rpm": 4500, "torque": 15.5 },
        { "percent": 100, "rpm": 5000, "torque": 10.0 }
      ]
    }
  ],
  "metadata": {
    "created": "2024-01-15T10:30:00Z",
    "modified": "2024-01-20T14:45:00Z",
    "notes": "Measured at 25°C ambient temperature"
  }
}
```

**Note:** Full files will have 101 data points per series (0% to 100% in 1% increments).
The sample above shows 10% increments for brevity.

---

## Success Metrics

### MVP Complete When:
- [ ] Can create new motor definition file with wizard
- [ ] New curve generation from max speed, torque, power parameters
- [ ] Can open existing JSON files with multiple series
- [ ] All motor properties editable (name, manufacturer, specs, etc.)
- [ ] Directory browser side pane (VS Code-style)
- [ ] Click file in directory list to load
- [ ] Dirty state tracked for unsaved changes
- [ ] Prompt to save on close if dirty
- [ ] Prompt to save when opening new file if dirty
- [ ] Save command overwrites file
- [ ] Save As creates new file (becomes active)
- [ ] Save Copy As creates copy (original stays active)
- [ ] Displays torque curve as line graph with multiple series
- [ ] Default series: "Peak" and "Continuous"
- [ ] Can add/rename/delete curve series
- [ ] Series visibility toggleable via checkboxes
- [ ] Series distinguished by unique colors
- [ ] User can edit series colors (persisted per series name)
- [ ] Data saved at 1% increments (0-100%)
- [ ] RPM displayed rounded to nearest whole number
- [ ] RPM on X axis, Torque on Y axis (default)
- [ ] Grid lines at rounded value increments
- [ ] Hover tooltip shows RPM/torque values
- [ ] Can edit values in data grid
- [ ] Chart updates in real-time
- [ ] EQ-style point dragging works
- [ ] Q slider affects curve sharpness
- [ ] Can load background image
- [ ] Background image scales independently (X/Y)
- [ ] Axis scaling via sliders (smooth, no jitter)
- [ ] Can save to JSON file with all motor properties
- [ ] Runs as portable executable
- [ ] Works on Windows 11
- [ ] Undo/redo support (Ctrl+Z / Ctrl+Y)
- [ ] Structured logging with Serilog
- [ ] Global exception handling with user-friendly error dialogs

### Nice to Have for MVP:
- [ ] Keyboard shortcuts
- [ ] Recent files list
- [ ] Axis swap toggle (RPM ↔ Torque)

### Future Features:
- [ ] Tabbed interface for multiple files
- [ ] Units system (Nm ↔ lbf-in via Tare)
- [ ] Power curve overlay (dual Y-axis)

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| LiveCharts2 learning curve | Start with static charts, add interactivity later |
| File format changes | Design flexible JSON schema |
| Cross-platform issues | Focus on Windows first, test others later |
| Bundle size concerns | Use trimming options, evaluate necessity |

---

## Next Steps

1. ~~Review this roadmap and architecture documents~~ ✅ Complete
2. ~~Decide on Avalonia vs WPF (or other option)~~ ✅ Avalonia + .NET 8 confirmed
3. Set up development environment
4. Create initial project scaffold with Serilog logging
5. Implement undo/redo infrastructure
6. Begin Phase 1 implementation

---

## Questions to Resolve

1. **JSON Schema**: Is the proposed schema suitable, or do you have an existing format?
2. **Unit Default**: Should Nm or lbf-in be the default unit?
3. ~~**Charting Style**: Preference for line style, colors, grid appearance?~~ ✅ Faded grid lines, rounded label increments
4. **Additional Metadata**: Any other properties needed in the curve model?
5. **Validation Rules**: Specific limits for RPM or torque values?
6. **Q Algorithm**: Preferred algorithm for Q-influenced point editing? (Gaussian, linear falloff, etc.)
7. **Image Formats**: Any specific image formats beyond PNG/JPG/BMP needed?
