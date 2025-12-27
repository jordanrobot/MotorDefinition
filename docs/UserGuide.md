---
layout: default
title: User Guide
---

[Docs Home](index.md) | [Quick Start](QuickStart.md) | [Terms and Definitions](TermsAndDefinitions.md) | [API documentation](api/index.md)

## User Guide

This guide explains the concepts and common workflows for the `JordanRobot.MotorDefinition` NuGet package.

### What this library provides

- A runtime object model for motor definition data (`JordanRobot.MotorDefinition.Model`).
- Simple load/save entry points via `JordanRobot.MotorDefinition.MotorFile`.
- A JSON format designed for motor/drive/voltage torque curves, supporting a variable number of data points per performance curve.

The generated API reference lives under [docs/api](api/index.md).

### Core concepts

#### Data hierarchy

Motor definition files follow this structure:

- **Motor** (`ServoMotor`)
- **Drives** (`Drive`)
- **Voltages** (`Voltage`)
- **Curves** (`Curve`)
- **Points** (`DataPoint`)

This mirrors how a motor can be powered by different drives, and drives may be operated at different voltages. Each of these configurations may result in different motor performance torque curves.

#### Motor

The base object is `ServoMotor`. It contains metadata about the motor (name, manufacturer, part number, max speed, rated torques, weight, brake properties) and a collection of `Drive` entries. The motor metadata are inherent to the motor itself, and are not changed by the drive or voltage used. These are accessed via properties on the `ServoMotor` object itself.

#### Drive

Each `Drive` represents a specific motor drive that can power the motor. It contains metadata about the drive (name, manufacturer, part number) and a collection of `Voltage` entries. Drives may have different capabilities and characteristics that affect motor performance.

These drives are accessed via the `ServoMotor.Drives` collection.

#### Voltage

Each `Voltage` represents a specific voltage level at which the motor can be driven by the associated drive. It contains metadata about the voltage (nominal voltage, max speed at that voltage) and a collection of `Curve` entries. Different voltages can significantly affect motor performance, with higher voltages typically allowing for higher speeds and torques.

For example, running a 240V drive with 110V typically results in a lower output speed for the motor.

The voltages are accessed via the `Drive.Voltages` collection.

#### Performance Curves

Each `Curve` represents a specific performance curve for the motor at that voltage, such as "Continuous" or "Peak" torque curves. These curves define how the motor performs across its speed range at that voltage.

The curves are accessed via the `Voltage.Curves` collection.

#### Performance Data Points

Each `Curve` contains `DataPoint` entries. These points define the motor's performance characteristics at a specific speed. Each `DataPoint` if defined by three main attributes:
- `Percent`: The percentage of the motor's maximum speed.
- `Rpm`: The revolutions per minute corresponding to that percentage.
- `Torque`: The torque value at that speed.

It is important to note that:
- The smallest valid `Percent` value is `0`.
- Percentage values are saved as whole numbers, i.e. decimals cannot be used.
- A performance curve that fully defines 0% - 100% performance is composed of 101 data points (1% increments).
- Performance curves may use fewer data points to define a coarser performance curve. (E.g. 0%, 10%, 20%...).
- There is no maximum valid `Percent` value.
- Percent values above 100% are allowed in the file format, and represent motor performance at overspeed. Currently the Motor Editor Application does not provide a UI to author overspeed data, though it will show them in the performance graph if present.
- A performance curve may contain 0 data points. In this case clients may fall back to rated continuous/peak torque values for visualization purposes.

**Critical note**: Within a single `Voltage`, all curves are expected to share the same percent and RPM axes.


### Common workflows

#### Load a file

```csharp
using JordanRobot.MotorDefinition;

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
IEnumerable<string> voltages = drive.VoltageNames;
```

#### LINQ query access (drives and voltages)

For LINQ queries, use the LINQ-friendly enumerables:

```csharp
// All drives
var servoDrives = motor.Drives
    .Where(d => d.Manufacturer.Contains("Servo", StringComparison.OrdinalIgnoreCase));

// Find a specific curve
var drive = motor.Drives.FirstOrDefault(d => d.Name.Equals("Drive X", StringComparison.OrdinalIgnoreCase));
var voltage = drive?.GetVoltage(400);
var curve = voltage?.Curves.FirstOrDefault(c => c.Name.Equals("Continuous", StringComparison.OrdinalIgnoreCase));

```

If you’re dealing with “unknown JSON” and want to quickly detect whether it resembles a motor definition:

```csharp
using System.Text.Json;
using JordanRobot.MotorDefinition;

using var doc = JsonDocument.Parse(File.ReadAllText("input.json"));
var looksLikeMotorFile = MotorFile.IsLikelyMotorDefinition(doc);
```

#### Save a file

```csharp
using JordanRobot.MotorDefinition;

MotorFile.Save(motor, "motor.updated.json");
```

Async variants are available:

```csharp
using JordanRobot.MotorDefinition;

var motor = await MotorFile.LoadAsync("motor.json", cancellationToken);
await MotorFile.SaveAsync(motor, "motor.updated.json", cancellationToken);
```

#### Create drives, voltages, and series

```csharp
using JordanRobot.MotorDefinition.Model;

var motor = new ServoMotor("Example")
{
    MaxSpeed = 6000,
    RatedPeakTorque = 20,
};

var drive = motor.AddDrive("Drive A");
var voltage = drive.AddVoltage(208);

voltage.MaxSpeed = motor.MaxSpeed;

// Initializes RPM axis from voltage.MaxSpeed and sets all torque values.
var peak = voltage.AddSeries("Peak", initializeTorque: motor.RatedPeakTorque);
```

#### Update curve data safely

When you modify curve data, keep the shape consistent:

- Prefer editing the existing points in place.
- If you add/remove points, ensure the percent axis remains strictly increasing and that all curves under a `Voltage` share the same axis.
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

If you want quick lookups by percent (0..100), use percent-based accessors on `Curve`:

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
- **Empty or incomplete models**: `ServoMotor.HasValidConfiguration()` returns `false` when there is no drive/voltage/curve data.
- **Invalid curve shape**: `Curve.ValidateDataIntegrity()` returns `false` when the curve has more than 101 points, has negative percent values, or has a non-increasing percent axis.

For detailed type/member information, see the generated [API documentation](api/index.md).
