# Motor Torque Curve Editor - MVP Roadmap

## Phase Overview

```
Phase 1: Foundation (Week 1-2)
    └── Project setup, basic UI shell, file operations

Phase 2: Core Features (Week 3-4)
    └── Chart visualization, data binding, grid lines, hover tooltip

Phase 3: File Management (Week 5)
    └── Directory browser, dirty state, save prompts, Save As/Copy As

Phase 4: Advanced Editing (Week 6-8)
    └── EQ-style editing, Q slider, background images, axis scaling

Phase 5: Polish & User Preferences (Week 9)
    └── User settings, keyboard shortcuts, undo/redo

Phase 6: Units System (Future)
    └── Tare integration, unit conversion, preferences

Phase 7: Tabbed Interface (Future)
    └── Multiple files open simultaneously in tabs
```

---

## Phase 1: Foundation

### 1.1 Project Setup
- [ ] Create solution with Avalonia template
- [ ] Configure project for .NET 8
- [ ] Add NuGet packages (Avalonia, CommunityToolkit.Mvvm)
- [ ] Set up project structure (Models, Views, ViewModels, Services)
- [ ] Configure single-file publishing

### 1.2 Basic Window Shell
- [ ] Create MainWindow with menu bar
- [ ] Implement basic layout (directory pane + chart area + properties panel)
- [ ] Add File menu (New, Open, Save, Save As, Save Copy As, Exit)
- [ ] Implement window title showing current file
- [ ] Add asterisk (*) to title when file is dirty

### 1.3 Data Models
- [ ] Create MotorData model class (container for file)
- [ ] Create CurveSeries model class (named curve)
- [ ] Create DataPoint model class (percent, rpm, torque)
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

**Deliverable:** Application that can open, display JSON content, and save files.

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

### 2.6 Properties Panel
- [ ] Create PropertiesPanel component
- [ ] Display curve metadata (name, manufacturer)
- [ ] Display data grid of points
- [ ] Bind grid to curve data
- [ ] Enable editing in grid cells

### 2.7 Two-Way Binding
- [ ] Chart updates when grid values change
- [ ] Grid updates when chart is modified (future)
- [ ] Implement INotifyPropertyChanged throughout
- [ ] Handle dirty state tracking (unsaved changes)

### 2.8 Basic Validation
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

### 4.6 Undo/Redo (Optional MVP)
- [ ] Implement command pattern for edits
- [ ] Track edit history
- [ ] Enable Ctrl+Z / Ctrl+Y shortcuts

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
  "name": "Example Motor",
  "manufacturer": "Acme Motors",
  "unit": "Nm",
  "maxRpm": 5000,
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
    "notes": "Measured at 25°C ambient temperature, 12V supply"
  }
}
```

**Note:** Full files will have 101 data points per series (0% to 100% in 1% increments).
The sample above shows 10% increments for brevity.

---

## Success Metrics

### MVP Complete When:
- [ ] Can create new motor data file
- [ ] Can open existing JSON files with multiple series
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
- [ ] Can save to JSON file
- [ ] Runs as portable executable
- [ ] Works on Windows 11

### Nice to Have for MVP:
- [ ] Undo/redo support
- [ ] Keyboard shortcuts
- [ ] Recent files list
- [ ] Axis swap toggle (RPM ↔ Torque)

### Future Features:
- [ ] Tabbed interface for multiple files
- [ ] Units system (Nm ↔ lbf-in via Tare)

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

1. Review this roadmap and architecture documents
2. Decide on Avalonia vs WPF (or other option)
3. Set up development environment
4. Create initial project scaffold
5. Begin Phase 1 implementation

---

## Questions to Resolve

1. **JSON Schema**: Is the proposed schema suitable, or do you have an existing format?
2. **Unit Default**: Should Nm or lbf-in be the default unit?
3. ~~**Charting Style**: Preference for line style, colors, grid appearance?~~ ✅ Faded grid lines, rounded label increments
4. **Additional Metadata**: Any other properties needed in the curve model?
5. **Validation Rules**: Specific limits for RPM or torque values?
6. **Q Algorithm**: Preferred algorithm for Q-influenced point editing? (Gaussian, linear falloff, etc.)
7. **Image Formats**: Any specific image formats beyond PNG/JPG/BMP needed?
