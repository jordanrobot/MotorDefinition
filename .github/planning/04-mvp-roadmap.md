# Motor Torque Curve Editor - MVP Roadmap

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
- [ ] Create solution with Avalonia template
- [ ] Configure project for .NET 8
- [ ] Add NuGet packages (Avalonia, CommunityToolkit.Mvvm, Serilog, Serilog.Sinks.File, Serilog.Sinks.Console)
- [ ] Set up project structure (Models, Views, ViewModels, Services)
- [ ] Configure single-file publishing
- [ ] Set up Serilog logging infrastructure
- [ ] Configure global exception handling

### 1.2 Basic Window Shell
- [ ] Create MainWindow with menu bar
- [ ] Implement basic layout (directory pane + chart area + properties panel)
- [ ] Add File menu (New, Open, Save, Save As, Save Copy As, Exit)
- [ ] Implement window title showing current file
- [ ] Add asterisk (*) to title when file is dirty

### 1.3 Data Models
- [ ] Create MotorDefinition model class (all motor properties)
- [ ] Create CurveSeries model class (named curve)
- [ ] Create DataPoint model class (percent, rpm, torque)
- [ ] Create UnitSettings model class (torque, speed, power, weight units)
- [ ] Create MotorMetadata model class
- [ ] Implement 1% increment data structure (101 points per series)
- [ ] Add JSON serialization attributes
- [ ] Write model unit tests

### 1.4 File Service
- [ ] Implement JSON loading
- [ ] Implement JSON saving (always overwrites)
- [ ] Implement Save As (save to new file, becomes active)
- [ ] Implement Save Copy As (save copy, original stays active)
- [ ] Handle file dialogs
- [ ] Error handling for invalid files
- [ ] Write service unit tests

### 1.5 Curve Generator Service
- [ ] Create ICurveGeneratorService interface
- [ ] Implement curve interpolation from max parameters
- [ ] Generate curves at 1% increments
- [ ] Calculate corner speed (constant torque to constant power transition)
- [ ] Power calculation from torque and speed
- [ ] Write generator unit tests

### 1.6 New Motor Definition Wizard
- [ ] "New Motor" command in File menu
- [ ] Dialog to enter basic motor parameters:
  - Motor name
  - Max RPM
  - Max torque
  - Max power
- [ ] Generate initial Peak and Continuous curves
- [ ] Create new file with generated data

### 1.7 Logging and Exception Handling
- [ ] Configure Serilog with file and console sinks
- [ ] Add structured logging throughout services
- [ ] Implement global unhandled exception handler
- [ ] Create user-friendly error dialogs for exceptions
- [ ] Log file location in user's AppData folder
- [ ] Include context (file path, operation) in log entries

### 1.8 Undo/Redo Infrastructure
- [ ] Implement IUndoableCommand interface
- [ ] Create UndoStack service for managing edit history
- [ ] Define command classes for common operations:
  - EditPointCommand
  - EditSeriesCommand
  - EditMotorPropertyCommand
- [ ] Wire Ctrl+Z / Ctrl+Y keyboard shortcuts
- [ ] Integrate undo/redo with dirty state tracking

**Deliverable:** Application that can create, open, and save motor files with JSON content display. Includes full undo/redo support and structured logging.

---

## Phase 2: Core Features

### 2.1 Chart Integration
- [ ] Add LiveCharts2 NuGet package
- [ ] Create ChartView component
- [ ] Bind chart to MotorData with multiple series
- [ ] Configure default axes (RPM = X, Torque = Y)
- [ ] Display RPM values rounded to nearest whole number
- [ ] Style chart appearance

### 2.2 Multiple Series Display
- [ ] Load and display multiple curve series from file
- [ ] Create default series ("Peak", "Continuous") for new files
- [ ] Each series rendered as separate line on chart
- [ ] Distinguish series by unique line colors
- [ ] Series legend with names and colors

### 2.3 Grid Lines and Axis Labels
- [ ] Configure axis labels at rounded increments
  - RPM: 500, 1000, 1500, 2000, etc.
  - Torque: 5, 10, 15, 20, etc.
- [ ] Add faded grid lines extending across graph
- [ ] Implement auto-calculation for label spacing (avoid crowding)
- [ ] Labels update smoothly when axis range changes

### 2.4 Hover Tooltip
- [ ] Show RPM and torque values on hover near curve
- [ ] Position tooltip to not obscure cursor
- [ ] Style tooltip for readability

### 2.5 Series Management Panel
- [ ] Create series list panel in UI
- [ ] Checkbox for each series to show/hide visibility
- [ ] Display series color swatch next to name
- [ ] Editable series name field
- [ ] Add Series button
- [ ] Delete Series button (with confirmation)

### 2.6 Motor Properties Panel
- [ ] Create MotorPropertiesPanel component
- [ ] Display all motor properties (editable):
  - Motor name, manufacturer, part number
  - Drive name, drive part number
  - Voltage, amperage (continuous/peak)
  - Max/rated RPM
  - Continuous/peak torque
  - Power, weight, rotor inertia
  - Brake properties (hasBrake, torque, amperage)
- [ ] Unit selector for each property type
- [ ] Bind properties to MotorDefinition model
- [ ] Enable editing of all fields

### 2.7 Curve Data Panel
- [ ] Create CurveDataPanel component
- [ ] Display data grid of points for selected series
- [ ] Bind grid to curve data
- [ ] Enable editing in grid cells
- [ ] RPM values displayed rounded to whole numbers

### 2.8 Two-Way Binding
- [ ] Chart updates when grid values change
- [ ] Grid updates when chart is modified (future)
- [ ] Implement INotifyPropertyChanged throughout
- [ ] Handle dirty state tracking (unsaved changes)

### 2.9 Basic Validation
- [ ] Validate RPM values (positive, ascending)
- [ ] Validate torque values (non-negative)
- [ ] Show validation errors in UI
- [ ] Prevent saving invalid data

**Deliverable:** Application with working multi-series chart, series visibility toggles, and editable data grid.

---

## Phase 3: File Management

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
- [ ] Enable point selection on chart (click to select)
- [ ] Implement point dragging (drag up/down to adjust torque)
- [ ] Visual feedback when point is selected
- [ ] Sync dragged values back to data model in real-time

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
