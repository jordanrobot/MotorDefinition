# Motor Torque Curve Editor - MVP Roadmap

## Phase Overview

```
Phase 1: Foundation (Week 1-2)
    └── Project setup, basic UI shell, file operations

Phase 2: Core Features (Week 3-4)
    └── Chart visualization, data binding, basic editing

Phase 3: Advanced Editing (Week 5-6)
    └── Interactive chart editing, validation, polish

Phase 4: Units System (Future)
    └── Tare integration, unit conversion, preferences
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
- [ ] Implement basic layout (chart area + properties panel)
- [ ] Add File menu (New, Open, Save, Save As, Exit)
- [ ] Implement window title showing current file

### 1.3 Data Models
- [ ] Create TorqueCurve model class
- [ ] Create DataPoint model class
- [ ] Create CurveMetadata model class
- [ ] Add JSON serialization attributes
- [ ] Write model unit tests

### 1.4 File Service
- [ ] Implement JSON loading
- [ ] Implement JSON saving
- [ ] Handle file dialogs
- [ ] Error handling for invalid files
- [ ] Write service unit tests

**Deliverable:** Application that can open, display JSON content, and save files.

---

## Phase 2: Core Features

### 2.1 Chart Integration
- [ ] Add LiveCharts2 NuGet package
- [ ] Create ChartView component
- [ ] Bind chart to TorqueCurve data
- [ ] Configure axis labels (RPM, Torque)
- [ ] Style chart appearance

### 2.2 Properties Panel
- [ ] Create PropertiesPanel component
- [ ] Display curve metadata (name, manufacturer)
- [ ] Display data grid of points
- [ ] Bind grid to curve data
- [ ] Enable editing in grid cells

### 2.3 Two-Way Binding
- [ ] Chart updates when grid values change
- [ ] Grid updates when chart is modified (future)
- [ ] Implement INotifyPropertyChanged throughout
- [ ] Handle dirty state tracking (unsaved changes)

### 2.4 Basic Validation
- [ ] Validate RPM values (positive, ascending)
- [ ] Validate torque values (non-negative)
- [ ] Show validation errors in UI
- [ ] Prevent saving invalid data

**Deliverable:** Application with working chart and editable data grid.

---

## Phase 3: Advanced Editing

### 3.1 Interactive Chart Editing
- [ ] Enable point selection on chart
- [ ] Implement point dragging
- [ ] Sync dragged values back to data model
- [ ] Add/remove points via chart context menu

### 3.2 Add/Remove Data Points
- [ ] Add "Insert Point" button
- [ ] Add "Delete Point" button
- [ ] Handle insertion between existing points
- [ ] Update chart and grid automatically

### 3.3 Undo/Redo (Optional MVP)
- [ ] Implement command pattern for edits
- [ ] Track edit history
- [ ] Enable Ctrl+Z / Ctrl+Y shortcuts

### 3.4 UI Polish
- [ ] Add toolbar with common actions
- [ ] Implement keyboard shortcuts
- [ ] Add status bar
- [ ] Improve chart tooltips
- [ ] Add application icon

### 3.5 Unsaved Changes Handling
- [ ] Prompt to save on close
- [ ] Prompt to save on open new file
- [ ] Show asterisk (*) in title for unsaved changes

**Deliverable:** Fully functional MVP with interactive editing.

---

## Phase 4: Units System (Future)

### 4.1 Tare Integration
- [ ] Add Tare NuGet package
- [ ] Create UnitService using Tare
- [ ] Define supported units (Nm, lbf-in)

### 4.2 Unit Toggle UI
- [ ] Add unit selector in UI
- [ ] Store current unit preference
- [ ] Convert displayed values on toggle

### 4.3 Conversion Logic
- [ ] Convert on display (not stored data)
- [ ] Or: Convert stored data (with user confirmation)
- [ ] Handle precision/rounding

### 4.4 Persistence
- [ ] Save unit preference
- [ ] Remember last used unit
- [ ] Store unit in JSON file (optional)

**Deliverable:** Full unit system support.

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

Create `samples/example-curve.json` for testing:

```json
{
  "name": "Example Motor",
  "manufacturer": "Acme Motors",
  "unit": "Nm",
  "data": [
    { "rpm": 0, "torque": 50.0 },
    { "rpm": 500, "torque": 52.0 },
    { "rpm": 1000, "torque": 51.0 },
    { "rpm": 1500, "torque": 49.0 },
    { "rpm": 2000, "torque": 46.0 },
    { "rpm": 2500, "torque": 42.0 },
    { "rpm": 3000, "torque": 37.0 },
    { "rpm": 3500, "torque": 31.0 },
    { "rpm": 4000, "torque": 25.0 },
    { "rpm": 4500, "torque": 19.0 },
    { "rpm": 5000, "torque": 14.0 }
  ],
  "metadata": {
    "created": "2024-01-15T10:30:00Z",
    "modified": "2024-01-20T14:45:00Z",
    "notes": "Measured at 25°C ambient temperature, 12V supply"
  }
}
```

---

## Success Metrics

### MVP Complete When:
- [ ] Can create new curve file
- [ ] Can open existing JSON files
- [ ] Displays torque curve as line graph
- [ ] Can edit values in data grid
- [ ] Chart updates in real-time
- [ ] Can save to JSON file
- [ ] Runs as portable executable
- [ ] Works on Windows 11

### Nice to Have for MVP:
- [ ] Interactive chart point editing
- [ ] Undo/redo support
- [ ] Keyboard shortcuts
- [ ] Recent files list

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
3. **Charting Style**: Preference for line style, colors, grid appearance?
4. **Additional Metadata**: Any other properties needed in the curve model?
5. **Validation Rules**: Specific limits for RPM or torque values?
