[Docs Home](index.md) | [Quick Start](QuickStart.md) | [API documentation](api/index.md)

## User Guide

This guide explains the concepts and common workflows for the `JordanRobot.MotorDefinition` NuGet package.

### What this library provides

- A runtime object model for motor definition data (in the `CurveEditor.Models` namespace).
- Simple load/save entry points via `JordanRobot.MotorDefinitions.MotorFile`.
- A JSON format designed for motor/drive/voltage torque curves, supporting 0–101 points per curve.
    - 101 points corresponds to the standard 1% increment curve (0%..100%).
    - Fewer points are allowed for coarse datasets.
    - Percent values above 100% may be used to represent overspeed ranges.

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

Each `CurveSeries` contains `DataPoint` entries.

- Point count is supported from 0 to 101.
- In the standard case, a series uses 101 points at 1% increments (0%..100%).

- `Percent`: integer >= 0 (may exceed 100% for overspeed ranges)
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

#### Populate UI lists (drive names and voltages)

If you're populating combo-boxes or lists, you can use the convenience name enumerables:

```csharp
// Drive names for a combo-box
var driveNames = motor.DriveNames;

var drive = motor.GetDriveByName("Drive A");
// Or
IEnumerable<string> voltage = drive.VoltageNames;
```

#### LINQ query access (drives and voltages)

For LINQ queries, use the LINQ-friendly enumerables:

```csharp
// All drives
var servoDrives = motor.DriveConfigurations
    .Where(d => d.Manufacturer.Contains("Servo", StringComparison.OrdinalIgnoreCase));

// Find a specific curve series
var curve = this.Drives.Where(d => d.Name.Equals("Drive X", StringComparison.OrdinalIgnoreCase))
                       .Where(d => d.VoltageNames.Equals("400 V"))
                       .Where(v => v.CurveSeries.Equals("Continuous"));

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

- Prefer editing the existing points in place.
- If you add/remove points, ensure the percent axis remains strictly increasing and that all series under a `VoltageConfiguration` share the same axis.
- A point count of 0 is allowed; clients may fall back to rated continuous/peak torque for visualization.

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
    throw new InvalidOperationException("Invalid series data. Expected 0..101 points with a strictly increasing, non-negative percent axis.");
}
```

#### Fast curve lookups and exporting rows

If you want quick lookups by percent (0..100), use percent-based accessors on `CurveSeries`:

```csharp
// Get RPM + torque for a specific percent
var point50 = peak.GetPointByPercent(50);
var rpmAt50 = point50.Rpm;
var torqueAt50 = point50.Torque;

// Build a lookup dictionary keyed by percent (cache this in your client if needed)
var byPercent = peak.ToPercentLookup();
var point75 = byPercent[75];
```

For exporting rows (e.g., to Excel), the `Data` list is already row-friendly, and these helper sequences are also available:

```csharp
foreach (var p in peak.Data)
{
    // Percent, RPM, Torque
    // Write a row to your output here
}

// Or, if you want separate columns
var percents = peak.Percents;
var rpms = peak.Rpms;
var torques = peak.Torques;
```

### JSON schema and examples

The repository includes:

- A sample file: [schema/example-motor.json](../schema/example-motor.json)
- A JSON schema: [schema/motor-schema-v1.0.0.json](../schema/motor-schema-v1.0.0.json)

If you are integrating with other tools or generating files externally, use the schema and sample to validate the expected shape.

### Troubleshooting

- **Deserialization fails**: `MotorFile.Load` throws an `InvalidOperationException` when it cannot deserialize a motor definition.
- **Empty or incomplete models**: `MotorDefinition.HasValidConfiguration()` returns `false` when there is no drive/voltage/series data.
- **Invalid curve shape**: `CurveSeries.ValidateDataIntegrity()` returns `false` when the series has more than 101 points, has negative percent values, or has a non-increasing percent axis.

For detailed type/member information, see the generated [API documentation](api/index.md).
