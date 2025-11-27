# Motor Torque Curve Editor - Requirements Summary

## Overview

A dedicated desktop application for editing motor torque curves stored in JSON files.

## Core Requirements

### Functional Requirements

1. **File Operations**
   - Load JSON files containing motor torque curve data
   - Save edited curves back to JSON files
   - Standard file editor behavior (Open, Save, Save As, New)
   - **Save As**: Save current file as new file, new file becomes active
   - **Save Copy As**: Save copy to new file, original file stays active
   - File saves always overwrite (no append mode)
   - Explicit save only (changes not written until user saves)

2. **Directory Browser**
   - Side pane for browsing directories (VS Code-style)
   - Open a directory to view motor curve files
   - Click file in list to load into editor
   - Shows JSON files in selected directory

3. **Dirty State Tracking**
   - Track unsaved changes ("dirty" state) for each file
   - Mark file as dirty when any edit is made
   - Clear dirty state when file is saved
   - Show dirty indicator in UI (e.g., asterisk in title)

4. **Save Prompts**
   - Prompt to save when closing app with dirty file
   - Prompt to save when opening new file with dirty file active
   - Options: Save / Don't Save / Cancel

5. **Visualization**
   - Display torque curves as line graphs
   - Interactive graph manipulation
   - Real-time updates between graph and numeric values

3. **Data Editing**
   - View and edit curve properties
   - Edit numeric values directly
   - Manipulate graph points interactively

4. **EQ-Style Curve Editing**
   - Select points on the curve line and drag up/down to move them
   - Adjustable "Q" value (0.0-1.0) via slider to control curve sharpness
   - Q affects how gradual or sharp changes are to adjacent points
   - Similar interaction model to audio EQ applications

5. **Background Image Overlay**
   - Load an image behind the graph (for curve verification)
   - Scale image independently on X and Y axes
   - Purpose: overlay manufacturer motor curve images for verification

6. **Axis Scaling**
   - X and Y axes scalable via sliders
   - Smooth graph recalculation without jitter when scaling
   - Allows matching curve scale to background image

7. **Graph Grid & Labels**
   - Axis labels at rounded whole number increments
     - RPM: 500, 1000, 1500, 2000, etc.
     - Torque: 5, 10, 15, 20, etc.
   - Faded grid lines extending across graph at label positions
   - Auto-select label increments to avoid crowding

8. **Axis Configuration**
   - Default: RPM = X axis, Torque = Y axis
   - Future: Toggle to swap axes

9. **Hover Information**
   - Show RPM and torque values in popup when hovering near curve
   - Popup positioned to not obscure cursor
   - User preference to enable/disable hover popup

10. **Data Format**
    - Curve data saved at 1% increments (0% = 0 RPM, 100% = max RPM)
    - Each data point stores percentage, RPM, and torque values
    - RPM values displayed rounded to nearest whole number

11. **Multiple Curve Series**
    - Save multiple curve profiles within a single motor data file
    - Each series distinguished by a unique name
    - Default series: "Peak" and "Continuous"
    - User can add, edit, and delete additional curve series
    - Series visibility toggleable via checkboxes in series list

12. **Series Colors**
    - Each series distinguished by unique line color
    - User can edit colors for each series
    - Color selections per series name saved to persistent user options
    - Colors remain consistent across different files

13. **Units System (Future)**
   - Toggle between Nm (Newton-meters) and lbf-in (pound-force inches)
   - Automatic value conversion when switching units
   - Use the `Tare` NuGet package (by jordanrobot) for unit handling

14. **Tabbed Interface (Future)**
    - Load multiple motor files simultaneously
    - Each file in its own tab
    - Switch between tabs to edit different files
    - Independent dirty state per tab

### Non-Functional Requirements

1. **Deployment**
   - Portable application (no installation required)
   - Run from user space
   - No server infrastructure or cloud resources
   - Self-contained executable

2. **Platform**
   - Primary target: Windows 11
   - Nice to have: Cross-platform support

3. **Technology Preferences**
   - C# and .NET 8 preferred
   - Open to other solutions if justified

## Data Model

Motor data file with multiple curve series at 1% increments:

```json
{
  "name": "Motor Model XYZ",
  "manufacturer": "Company Name",
  "unit": "Nm",
  "maxRpm": 5000,
  "series": [
    {
      "name": "Peak",
      "data": [
        { "percent": 0, "rpm": 0, "torque": 55.0 },
        { "percent": 1, "rpm": 50, "torque": 55.2 },
        { "percent": 2, "rpm": 100, "torque": 55.4 },
        ...
        { "percent": 100, "rpm": 5000, "torque": 12.0 }
      ]
    },
    {
      "name": "Continuous",
      "data": [
        { "percent": 0, "rpm": 0, "torque": 45.0 },
        { "percent": 1, "rpm": 50, "torque": 45.1 },
        ...
        { "percent": 100, "rpm": 5000, "torque": 10.0 }
      ]
    }
  ],
  "metadata": {
    "created": "2024-01-15",
    "notes": "Test conditions: 25°C ambient"
  }
}
```

**Key data format notes:**
- Data saved at 1% increments (101 points per series: 0% to 100%)
- `percent`: 0-100, where 0% = 0 RPM, 100% = maxRpm
- `rpm`: Calculated from percent × maxRpm, displayed rounded to whole number
- `torque`: Torque value at that percentage point
- Multiple series per file (e.g., "Peak", "Continuous", custom names)

## User Workflow

1. Launch application (no installation)
2. Open a directory in the side pane to browse files
3. Click a motor curve file to load it into the editor
4. View multiple series (Peak, Continuous, custom) in chart
5. Toggle series visibility via checkboxes
6. Select series to edit from list
7. Optionally load background image for reference
8. Scale background image to match graph dimensions
9. Edit curve via:
   - Dragging points up/down (EQ-style)
   - Adjusting Q slider for curve sharpness
   - Direct numeric input in properties panel
10. File becomes "dirty" (unsaved changes indicator)
11. Hover over curve to see precise values (RPM rounded to whole number)
12. Scale axes as needed to match reference image
13. Customize series colors (persisted across sessions)
14. Add/rename/delete curve series as needed
15. Toggle units (future feature)
16. Save changes to file (explicit save, always overwrites)
    - **Save**: Overwrite current file
    - **Save As**: Save as new file, new file becomes active
    - **Save Copy As**: Save copy, original file stays active
17. If closing or opening new file with unsaved changes, prompted to save

## Success Criteria

- [ ] Can load and parse JSON motor data files
- [ ] Directory browser side pane (VS Code-style)
- [ ] Click file in directory list to load
- [ ] Dirty state tracked for unsaved changes
- [ ] Prompt to save on close if dirty
- [ ] Prompt to save when opening new file if dirty
- [ ] Save command overwrites file
- [ ] Save As creates new file (becomes active)
- [ ] Save Copy As creates copy (original stays active)
- [ ] Displays interactive line graph with multiple series
- [ ] Shows/hides individual series via checkboxes
- [ ] Series distinguished by color
- [ ] User can edit series colors (persisted per series name)
- [ ] Data saved at 1% increments (0-100%)
- [ ] RPM displayed rounded to nearest whole number
- [ ] Default series: "Peak" and "Continuous"
- [ ] Can add/rename/delete curve series
- [ ] Supports direct numeric editing
- [ ] Supports graph-based editing (EQ-style drag)
- [ ] Q slider affects curve sharpness (0.0-1.0)
- [ ] Can load background image for verification
- [ ] Background image scales independently on X/Y
- [ ] Axes scalable via sliders (smooth, no jitter)
- [ ] Grid lines at rounded value increments
- [ ] Hover popup shows RPM/torque values
- [ ] Hover popup can be disabled in settings
- [ ] RPM on X axis, Torque on Y axis by default
- [ ] Saves valid JSON output
- [ ] Runs without installation
- [ ] Works on Windows 11
