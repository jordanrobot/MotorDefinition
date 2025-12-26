[Quick Start](docs/QuickStart.md) | [User Guide](docs/UserGuide.md) | [API documentation](docs/api/index.md)

# MotorDefinition Library

## Getting Started

Use these pages to learn the `MotorDefinition` library:

- [Quick Start](docs/QuickStart.md)
- [User Guide](docs/UserGuide.md)
- [API documentation](docs/api/index.md "MotorDefinition API")
Related resources:

- [Example motor definition JSON](../schema/example-motor.json)
- [JSON schema](../schema/motor-schema-v1.0.0.json)


# Motor Torque Curve Editor

A desktop application for creating and editing motor torque curves stored in JSON files.

## Features

### Phase 1 (Foundation) - Complete
- ✅ Create new motor definition files
- ✅ Load and save motor curve data in JSON format
- ✅ Hierarchical data structure: Motor → Drive(s) → Voltage(s) → Curve Series
- ✅ Data models for motor definitions, drive configurations, voltage configurations, curve series, and data points
- ✅ Curve generation from motor parameters (max speed, torque, power)
- ✅ Curve data storage supports 0–101 points per curve (default: 101 points at 1% increments)
- ✅ Basic UI shell with menu bar
- ✅ Structured logging with Serilog

### Phase 2 (Current - Core Features)
- ✅ Interactive chart with LiveCharts2
- ✅ Multiple series display with unique colors
- ✅ Series visibility toggles
- ✅ Grid lines at rounded value increments
- ✅ Hover tooltips showing RPM/torque values
- ✅ Curve data panel with editable DataGrid
- ✅ Two-way binding between chart and data grid
- ✅ Data validation with error indicators
- ✅ Comprehensive unit tests (172 tests)

### Future Phases
- Phase 3: Directory browser and save prompts
- Phase 4: EQ-style curve editing with Q slider
- Phase 5: User preferences and settings
- Phase 6: Units system (Nm ↔ lbf-in)
- Phase 7: Tabbed interface for multiple files
- Phase 8: Power curve overlay

## Getting Started

### Prerequisites
- .NET 8 SDK

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project src/MotorEditor.Avalonia
```

### Test

```bash
dotnet test
```

### Publish (Windows Single-File Executable)

This project supports creating a self-contained, single-file executable for Windows.

- **Target runtime**: `win-x64`
- **Publish profile**: `WinSingleFile`
- **Output folder**: `src/MotorEditor.Avalonia/bin/Release/net8.0/win-x64/publish`
- **Artifact to distribute**: `MotorEditor.exe` in the `publish` folder

#### Option 1: Using the publish profile (recommended)

```bash
dotnet publish src/MotorEditor.Avalonia -c Release -p:PublishProfile=WinSingleFile
```

#### Option 2: Explicit single-file command

```bash
dotnet publish src/MotorEditor.Avalonia -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

#### Option 3: Convenience PowerShell script (Windows)

From the repository root:

```pwsh
./build-singlefile.ps1
```

The resulting EXE is self-contained and should run on a Windows 10/11 x64 machine without installing the .NET runtime.

## Project Structure

```
CurveEditor/
├── src/
│   ├── MotorEditor.Avalonia/   # Avalonia application
│   └── jordanrobot.MotorDefinition/ # Shared library owning models + IO
├── tests/
│   └── CurveEditor.Tests/      # Unit tests
└── samples/
    └── example-motor.json      # Sample motor definition file
```

## Data Format

Motor definitions are stored as JSON files with a hierarchical structure:

**Motor → Drive(s) → Voltage(s) → Curve Series**

This structure reflects the real-world relationship where:
- A motor can be paired with multiple drives
- Each drive can operate at multiple voltages
- Each voltage configuration has its own performance characteristics and curve series
- Motor base properties (theoretical maximums from cut sheet) are stored at the motor level to avoid data duplication

```json
{
  "schemaVersion": "1.0.0",
  "motorName": "High Torque Servo Motor",
  "manufacturer": "Acme Motors",
  "partNumber": "M-1234-HT",
  "power": 1500,
  "maxSpeed": 5000,
  "ratedSpeed": 3000,
  "ratedContinuousTorque": 45.0,
  "ratedPeakTorque": 55.0,
  "weight": 8.5,
  "rotorInertia": 0.0025,
  "hasBrake": true,
  "brakeTorque": 12.0,
  "brakeAmperage": 0.5,
  "brakeVoltage": 24,
  "units": {
    "torque": "Nm",
    "speed": "rpm",
    "power": "W",
    "weight": "kg"
  },
  "drives": [
    {
      "name": "Servo Drive Pro X-203",
      "partNumber": "SD-X203",
      "manufacturer": "Acme Drives",
      "voltages": [
        {
          "voltage": 208,
          "power": 1400,
          "maxSpeed": 4800,
          "ratedContinuousTorque": 42.0,
          "ratedPeakTorque": 52.0,
          "continuousAmperage": 9.5,
          "peakAmperage": 22.0,
          "series": [
            {
              "name": "Peak",
              "data": [
                { "percent": 0, "rpm": 0, "torque": 52.0 },
                { "percent": 1, "rpm": 48, "torque": 52.0 },
                ...
                { "percent": 100, "rpm": 4800, "torque": 29.0 }
              ]
            },
            {
              "name": "Continuous",
              "data": [...]
            }
          ]
        },
        {
          "voltage": 220,
          ...
        }
      ]
    },
    {
      "name": "Servo Drive Max X-220",
      ...
    }
  ],
  "metadata": {
    "created": "2024-01-15T10:30:00Z",
    "modified": "2024-01-20T14:45:00Z",
    "notes": "Motor base properties are theoretical maximums from cut sheet."
  }
}
```

Each curve series can contain 0 to 101 data points.

- 101 points corresponds to the standard 1% increment curve (0% through 100%).
- Fewer points are allowed for coarse datasets.
- Percent values above 100% may be present for overspeed ranges (JSON authoring only in the editor for now).

## Technology Stack

- **Framework**: .NET 8
- **UI**: Avalonia UI 11.x
- **Charting**: LiveCharts2 (future phases)
- **MVVM**: CommunityToolkit.Mvvm
- **Logging**: Serilog

## Development & Planning

- Overall roadmap: `.github/planning/04-mvp-roadmap.md`.
- Phase-specific functional requirements: `.github/planning/phase-N-requirements.md`.
- Phase plans, tasks checklists, and bug/notes docs: `.github/planning/phase-N-plan.md`, `.github/planning/phase-N-tasks.md`, `.github/planning/phase-N-bugs-and-notes.md`.
- Architecture Decision Records (ADRs): `.github/adr/adr-XXXX-*.md`.
- Copilot development workflow (how agents should use these docs): `.github/planning/copilot-development-workflow.md`.

## License

MIT License - see [LICENSE](LICENSE) for details.
