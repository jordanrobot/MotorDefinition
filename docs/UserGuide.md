[Quick Start](QuickStart.md) | [API documentation](api/index.md)

## User Guide

This guide explains the concepts and common workflows for the `MotorDefinition` NuGet package.

### What this library provides

- A runtime object model for motor definition data (in the `CurveEditor.Models` namespace).
- Simple load/save entry points via `JordanRobot.MotorDefinitions.MotorFile`.
- A JSON format designed for motor/drive/voltage torque curves, using 1% increments (101 points).

The generated API reference lives under [docs/api](api/index.md).

### Core concepts

#### Data hierarchy

Motor definition files follow this structure:

- **Motor** (`MotorDefinition`)
- **Drive configurations** (`DriveConfiguration`)
- **Voltage configurations** (`VoltageConfiguration`)
- **Series** (`CurveSeries`)
- **Points** (`DataPoint`)

This mirrors how a motor can be paired with different drives and operated at different voltages.

#### Axes and data shape

Each `CurveSeries` contains `DataPoint` entries at 1% increments:

- `Percent`: integer 0..100
- `Rpm`: typically derived from max RPM (e.g., `percent / 100.0 * maxRpm`)
- `Torque`: torque value at that point (can be negative)

Within a single `VoltageConfiguration`, all series are expected to share the same percent and RPM axes.

### Common workflows

#### Load a file

```csharp
using JordanRobot.MotorDefinitions;

var motor = MotorFile.Load("motor.json");

// Example: find a drive by name
var drive = motor.GetDriveByName("Servo Drive Pro X-203");
```

If you’re dealing with “unknown JSON” and want to quickly detect whether it resembles a motor definition:

```csharp
using System.Text.Json;
using JordanRobot.MotorDefinitions;

using var doc = JsonDocument.Parse(File.ReadAllText("input.json"));
var looksLikeMotorFile = MotorFile.IsLikelyMotorDefinition(doc);
```

#### Save a file

```csharp
using JordanRobot.MotorDefinitions;

MotorFile.Save(motor, "motor.updated.json");
```

Async variants are available:

```csharp
using JordanRobot.MotorDefinitions;

var motor = await MotorFile.LoadAsync("motor.json", cancellationToken);
await MotorFile.SaveAsync(motor, "motor.updated.json", cancellationToken);
```

#### Create drives, voltages, and series

```csharp
using CurveEditor.Models;

var motor = new MotorDefinition("Example")
{
    MaxSpeed = 6000,
    RatedPeakTorque = 20,
};

var drive = motor.AddDrive("Drive A");
var voltage = drive.AddVoltageConfiguration(208);

voltage.MaxSpeed = motor.MaxSpeed;

// Initializes RPM axis from voltage.MaxSpeed and sets all torque values.
var peak = voltage.AddSeries("Peak", initializeTorque: motor.RatedPeakTorque);
```

#### Update curve data safely

When you modify curve data, keep the shape consistent:

- Do not add/remove points from `CurveSeries.Data` unless you also re-establish the full 0..100 axis.
- Prefer editing the existing points in place.

Example: apply a simple torque scale to the entire series:

```csharp
foreach (var point in peak.Data)
{
    point.Torque *= 0.95;
}
```

Verify the data integrity:

```csharp
if (!peak.ValidateDataIntegrity())
{
    throw new InvalidOperationException("Invalid series data. Expected 101 points (0%..100%).");
}
```

### JSON schema and examples

The repository includes:

- A sample file: [schema/example-motor.json](../schema/example-motor.json)
- A JSON schema: [schema/motor-schema-v1.0.0.json](../schema/motor-schema-v1.0.0.json)

If you are integrating with other tools or generating files externally, use the schema and sample to validate the expected shape.

### Troubleshooting

- **Deserialization fails**: `MotorFile.Load` throws an `InvalidOperationException` when it cannot deserialize a motor definition.
- **Empty or incomplete models**: `MotorDefinition.HasValidConfiguration()` returns `false` when there is no drive/voltage/series data.
- **Wrong number of points**: `CurveSeries.ValidateDataIntegrity()` returns `false` when the series is not 101 points.

For detailed type/member information, see the generated [API documentation](api/index.md).
