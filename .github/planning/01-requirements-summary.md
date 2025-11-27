# Motor Torque Curve Editor - Requirements Summary

## Overview

A dedicated desktop application for editing motor torque curves stored in JSON files.

## Core Requirements

### Functional Requirements

1. **File Operations**
   - Load JSON files containing motor torque curve data
   - Save edited curves back to JSON files
   - Standard file editor behavior (Open, Save, Save As, New)

2. **Visualization**
   - Display torque curves as line graphs
   - Interactive graph manipulation
   - Real-time updates between graph and numeric values

3. **Data Editing**
   - View and edit curve properties
   - Edit numeric values directly
   - Manipulate graph points interactively

4. **Units System (Future)**
   - Toggle between Nm (Newton-meters) and lbf-in (pound-force inches)
   - Automatic value conversion when switching units
   - Use the `Tare` NuGet package (by jordanrobot) for unit handling

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

## Data Model (Assumed)

Based on typical motor torque curve data:

```json
{
  "name": "Motor Model XYZ",
  "manufacturer": "Company Name",
  "unit": "Nm",
  "data": [
    { "rpm": 0, "torque": 50.0 },
    { "rpm": 1000, "torque": 48.5 },
    { "rpm": 2000, "torque": 45.0 },
    { "rpm": 3000, "torque": 40.0 },
    { "rpm": 4000, "torque": 33.0 },
    { "rpm": 5000, "torque": 25.0 }
  ],
  "metadata": {
    "created": "2024-01-15",
    "notes": "Test conditions: 25°C ambient"
  }
}
```

## User Workflow

1. Launch application (no installation)
2. Open existing JSON file or create new curve
3. View torque curve visualization
4. Edit curve via graph manipulation or numeric input
5. Toggle units (future feature)
6. Save changes to file

## Success Criteria

- [ ] Can load and parse JSON torque curve files
- [ ] Displays interactive line graph
- [ ] Supports direct numeric editing
- [ ] Supports graph-based editing
- [ ] Saves valid JSON output
- [ ] Runs without installation
- [ ] Works on Windows 11
