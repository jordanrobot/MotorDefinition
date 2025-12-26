#### [MotorDefinition](index.md 'index')
### [CurveEditor\.Models](CurveEditor.Models.md 'CurveEditor\.Models')

## MotorDefinition Class

Represents a complete motor definition including all properties, drive configurations, and metadata\.
Structure: Motor → Drive\(s\) → Voltage\(s\) → CurveSeries

```csharp
public class MotorDefinition
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; MotorDefinition
### Constructors

<a name='CurveEditor.Models.MotorDefinition.MotorDefinition()'></a>

## MotorDefinition\(\) Constructor

Creates a new MotorDefinition with default values\.

```csharp
public MotorDefinition();
```

<a name='CurveEditor.Models.MotorDefinition.MotorDefinition(string)'></a>

## MotorDefinition\(string\) Constructor

Creates a new MotorDefinition with the specified motor name\.

```csharp
public MotorDefinition(string motorName);
```
#### Parameters

<a name='CurveEditor.Models.MotorDefinition.MotorDefinition(string).motorName'></a>

`motorName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the motor\.
### Fields

<a name='CurveEditor.Models.MotorDefinition.CurrentSchemaVersion'></a>

## MotorDefinition\.CurrentSchemaVersion Field

The current schema version for motor definition files\.

```csharp
public const string CurrentSchemaVersion = "1.0.0";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')
### Properties

<a name='CurveEditor.Models.MotorDefinition.BrakeAmperage'></a>

## MotorDefinition\.BrakeAmperage Property

The current draw of the brake \(if present\) \(A\)\.

```csharp
public double BrakeAmperage { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.BrakeBacklash'></a>

## MotorDefinition\.BrakeBacklash Property

The backlash of the brake mechanism\.

```csharp
public double BrakeBacklash { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.BrakeEngageTimeDiode'></a>

## MotorDefinition\.BrakeEngageTimeDiode Property

The brake engage time when using a diode\.

```csharp
public double BrakeEngageTimeDiode { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.BrakeEngageTimeMov'></a>

## MotorDefinition\.BrakeEngageTimeMov Property

The brake engage time when using an MOV\.

```csharp
public double BrakeEngageTimeMov { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.BrakeReleaseTime'></a>

## MotorDefinition\.BrakeReleaseTime Property

The release time of the brake\.

```csharp
public double BrakeReleaseTime { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.BrakeTorque'></a>

## MotorDefinition\.BrakeTorque Property

The holding torque of the integral brake \(if present\)\.

```csharp
public double BrakeTorque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.BrakeVoltage'></a>

## MotorDefinition\.BrakeVoltage Property

The voltage requirement of the brake \(if present\) \(V\)\.

```csharp
public double BrakeVoltage { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.DriveConfigurations'></a>

## MotorDefinition\.DriveConfigurations Property

Gets a LINQ\-friendly enumeration of all drive configurations\.
This is a convenience alias for [Drives](CurveEditor.Models.MotorDefinition.md#CurveEditor.Models.MotorDefinition.Drives 'CurveEditor\.Models\.MotorDefinition\.Drives')\.

```csharp
public System.Collections.Generic.IEnumerable<CurveEditor.Models.DriveConfiguration> DriveConfigurations { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[DriveConfiguration](CurveEditor.Models.DriveConfiguration.md 'CurveEditor\.Models\.DriveConfiguration')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

<a name='CurveEditor.Models.MotorDefinition.DriveNames'></a>

## MotorDefinition\.DriveNames Property

Gets the drive names in this motor definition\.
Useful for populating UI lists and combo\-boxes\.

```csharp
public System.Collections.Generic.IEnumerable<string> DriveNames { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

<a name='CurveEditor.Models.MotorDefinition.Drives'></a>

## MotorDefinition\.Drives Property

The collection of drive configurations for this motor\.
Each drive can have multiple voltage configurations with their own curve series\.

```csharp
public System.Collections.Generic.List<CurveEditor.Models.DriveConfiguration> Drives { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DriveConfiguration](CurveEditor.Models.DriveConfiguration.md 'CurveEditor\.Models\.DriveConfiguration')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='CurveEditor.Models.MotorDefinition.FeedbackPpr'></a>

## MotorDefinition\.FeedbackPpr Property

The feedback device pulses per revolution \(PPR\)\.
Used for encoder or resolver feedback resolution\.

```csharp
public int FeedbackPpr { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='CurveEditor.Models.MotorDefinition.HasBrake'></a>

## MotorDefinition\.HasBrake Property

Indicates whether the motor includes an integral holding brake\.

```csharp
public bool HasBrake { get; set; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

<a name='CurveEditor.Models.MotorDefinition.Manufacturer'></a>

## MotorDefinition\.Manufacturer Property

The company that manufactures the motor\.

```csharp
public string Manufacturer { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='CurveEditor.Models.MotorDefinition.MaxSpeed'></a>

## MotorDefinition\.MaxSpeed Property

The theoretical maximum rotational speed of the motor \(RPM\)\.

```csharp
public double MaxSpeed { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.Metadata'></a>

## MotorDefinition\.Metadata Property

Metadata about the motor definition file\.

```csharp
public CurveEditor.Models.MotorMetadata Metadata { get; set; }
```

#### Property Value
[MotorMetadata](CurveEditor.Models.MotorMetadata.md 'CurveEditor\.Models\.MotorMetadata')

<a name='CurveEditor.Models.MotorDefinition.MotorName'></a>

## MotorDefinition\.MotorName Property

The model name or identifier for the motor\.

```csharp
public string MotorName { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='CurveEditor.Models.MotorDefinition.PartNumber'></a>

## MotorDefinition\.PartNumber Property

The manufacturer's part number for the motor\.

```csharp
public string PartNumber { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='CurveEditor.Models.MotorDefinition.Power'></a>

## MotorDefinition\.Power Property

The theoretical maximum power output of the motor \(in the unit specified by Units\.Power\)\.

```csharp
public double Power { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.RatedContinuousTorque'></a>

## MotorDefinition\.RatedContinuousTorque Property

The theoretical maximum torque the motor can produce continuously without overheating\.

```csharp
public double RatedContinuousTorque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.RatedPeakTorque'></a>

## MotorDefinition\.RatedPeakTorque Property

The theoretical maximum torque the motor can produce for short periods\.

```csharp
public double RatedPeakTorque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.RatedSpeed'></a>

## MotorDefinition\.RatedSpeed Property

The rated continuous operating speed of the motor \(RPM\)\.

```csharp
public double RatedSpeed { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.RotorInertia'></a>

## MotorDefinition\.RotorInertia Property

The moment of inertia of the motor's rotor, affecting acceleration response\.

```csharp
public double RotorInertia { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.MotorDefinition.SchemaVersion'></a>

## MotorDefinition\.SchemaVersion Property

Schema version for JSON compatibility\.

```csharp
public string SchemaVersion { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='CurveEditor.Models.MotorDefinition.Units'></a>

## MotorDefinition\.Units Property

The unit settings for this motor definition\.

```csharp
public CurveEditor.Models.UnitSettings Units { get; set; }
```

#### Property Value
[UnitSettings](CurveEditor.Models.UnitSettings.md 'CurveEditor\.Models\.UnitSettings')

<a name='CurveEditor.Models.MotorDefinition.Weight'></a>

## MotorDefinition\.Weight Property

The mass of the motor \(in the unit specified by Units\.Weight\)\.

```csharp
public double Weight { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')
### Methods

<a name='CurveEditor.Models.MotorDefinition.AddDrive(string)'></a>

## MotorDefinition\.AddDrive\(string\) Method

Adds a new drive configuration with the specified name\.

```csharp
public CurveEditor.Models.DriveConfiguration AddDrive(string name);
```
#### Parameters

<a name='CurveEditor.Models.MotorDefinition.AddDrive(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name for the new drive\.

#### Returns
[DriveConfiguration](CurveEditor.Models.DriveConfiguration.md 'CurveEditor\.Models\.DriveConfiguration')  
The newly created drive configuration\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if a drive with the same name already exists\.

<a name='CurveEditor.Models.MotorDefinition.GetDriveByName(string)'></a>

## MotorDefinition\.GetDriveByName\(string\) Method

Gets a drive configuration by name\.

```csharp
public CurveEditor.Models.DriveConfiguration? GetDriveByName(string name);
```
#### Parameters

<a name='CurveEditor.Models.MotorDefinition.GetDriveByName(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the drive to find\.

#### Returns
[DriveConfiguration](CurveEditor.Models.DriveConfiguration.md 'CurveEditor\.Models\.DriveConfiguration')  
The matching drive configuration, or null if not found\.

<a name='CurveEditor.Models.MotorDefinition.HasValidConfiguration()'></a>

## MotorDefinition\.HasValidConfiguration\(\) Method

Validates that the motor definition has at least one drive with a valid configuration\.

```csharp
public bool HasValidConfiguration();
```

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if valid; otherwise false\.

<a name='CurveEditor.Models.MotorDefinition.RemoveDrive(string)'></a>

## MotorDefinition\.RemoveDrive\(string\) Method

Removes a drive configuration by name\.

```csharp
public bool RemoveDrive(string name);
```
#### Parameters

<a name='CurveEditor.Models.MotorDefinition.RemoveDrive(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the drive to remove\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if removed; false if not found\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if attempting to remove the last drive\.