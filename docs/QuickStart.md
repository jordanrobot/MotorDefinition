[User Guide](UserGuide.md) | [API documentation](api/index.md)

## Quick Start

This guide gets you from zero to reading and writing a motor definition JSON file using the `MotorDefinition` NuGet package.

### Install

Add the package to your project:

```bash
dotnet add package MotorDefinition
```

### Load a motor definition

`MotorFile` is the main entry point for persistence.

```csharp
using JordanRobot.MotorDefinitions;

var motor = MotorFile.Load("example-motor.json");

Console.WriteLine(motor.MotorName);
Console.WriteLine($"Drives: {motor.Drives.Count}");
```

### Modify and save

```csharp
using JordanRobot.MotorDefinitions;

var motor = MotorFile.Load("example-motor.json");

motor.Manufacturer = "Acme Motors";

// Example: tweak the first series' first point torque
var firstVoltage = motor.Drives[0].Voltages[0];
var firstSeries = firstVoltage.Series[0];
firstSeries.Data[0].Torque = 0;

MotorFile.Save(motor, "example-motor.updated.json");
```

### Create a new file (minimal)

The object model is:

- Motor → Drive(s) → Voltage(s) → Curve series → Data points

```csharp
using CurveEditor.Models;
using JordanRobot.MotorDefinitions;

var motor = new MotorDefinition("My Motor")
{
    Manufacturer = "Contoso",
    PartNumber = "M-0001",
    MaxSpeed = 5000,
    RatedContinuousTorque = 10,
    RatedPeakTorque = 15,
};

var drive = motor.AddDrive("My Drive");
var voltage = drive.AddVoltageConfiguration(230);
voltage.MaxSpeed = motor.MaxSpeed;

// Creates 101 points (0%..100%) and fills RPM axis from MaxSpeed.
var peak = voltage.AddSeries("Peak", initializeTorque: motor.RatedPeakTorque);

if (!peak.ValidateDataIntegrity())
{
    throw new InvalidOperationException("Series data must contain 101 points at 1% increments.");
}

MotorFile.Save(motor, "my-motor.json");
```

### Notes and gotchas

- **101 points at 1% increments**: Each series is expected to contain exactly 101 points from 0% to 100%.
- **Shared axes per voltage**: All series under a given voltage are expected to share the same percent and RPM axes.
- **Schema version**: `MotorDefinition.SchemaVersion` defaults to the current schema version (`1.0.0`).

Next steps: see the [User Guide](UserGuide.md) for deeper guidance, and the generated [API documentation](api/index.md).
