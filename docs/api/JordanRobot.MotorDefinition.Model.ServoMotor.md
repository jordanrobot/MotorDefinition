#### [JordanRobot\.MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Model](JordanRobot.MotorDefinition.Model.md 'JordanRobot\.MotorDefinition\.Model')

## ServoMotor Class

Represents a complete motor definition including properties, drive configurations, and metadata\.

```csharp
public class ServoMotor
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; ServoMotor

### Remarks
Structure: [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor') → [Drive](JordanRobot.MotorDefinition.Model.Drive.md 'JordanRobot\.MotorDefinition\.Model\.Drive') → [Voltage](JordanRobot.MotorDefinition.Model.Voltage.md 'JordanRobot\.MotorDefinition\.Model\.Voltage') → [Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve')\.
### Constructors

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.ServoMotor()'></a>

## ServoMotor\(\) Constructor

Creates a new ServoMotor with default values\.

```csharp
public ServoMotor();
```

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.ServoMotor(string)'></a>

## ServoMotor\(string\) Constructor

Creates a new ServoMotor with the specified motor name\.

```csharp
public ServoMotor(string motorName);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.ServoMotor(string).motorName'></a>

`motorName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the motor\.
### Fields

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.CurrentSchemaVersion'></a>

## ServoMotor\.CurrentSchemaVersion Field

Specifies the current schema version for motor definition files\.

```csharp
public const string CurrentSchemaVersion = "1.0.0";
```

#### Field Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')
### Properties

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.BrakeAmperage'></a>

## ServoMotor\.BrakeAmperage Property

Gets or sets the current draw of the brake \(A\)\.

```csharp
public double BrakeAmperage { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

### Remarks
Only applicable when [HasBrake](JordanRobot.MotorDefinition.Model.ServoMotor.md#JordanRobot.MotorDefinition.Model.ServoMotor.HasBrake 'JordanRobot\.MotorDefinition\.Model\.ServoMotor\.HasBrake') is [true](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/builtin\-types/bool')\.

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.BrakeBacklash'></a>

## ServoMotor\.BrakeBacklash Property

Gets or sets the backlash of the brake mechanism\.

```csharp
public double BrakeBacklash { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.BrakeEngageTimeDiode'></a>

## ServoMotor\.BrakeEngageTimeDiode Property

Gets or sets the brake engage time when using a diode\.

```csharp
public double BrakeEngageTimeDiode { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.BrakeEngageTimeMov'></a>

## ServoMotor\.BrakeEngageTimeMov Property

Gets or sets the brake engage time when using an MOV\.

```csharp
public double BrakeEngageTimeMov { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.BrakeReleaseTime'></a>

## ServoMotor\.BrakeReleaseTime Property

Gets or sets the release time of the brake\.

```csharp
public double BrakeReleaseTime { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.BrakeTorque'></a>

## ServoMotor\.BrakeTorque Property

Gets or sets the holding torque of the integral brake\.

```csharp
public double BrakeTorque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

### Remarks
Only applicable when [HasBrake](JordanRobot.MotorDefinition.Model.ServoMotor.md#JordanRobot.MotorDefinition.Model.ServoMotor.HasBrake 'JordanRobot\.MotorDefinition\.Model\.ServoMotor\.HasBrake') is [true](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/builtin\-types/bool')\.

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.BrakeVoltage'></a>

## ServoMotor\.BrakeVoltage Property

Gets or sets the voltage requirement of the brake \(V\)\.

```csharp
public double BrakeVoltage { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

### Remarks
Only applicable when [HasBrake](JordanRobot.MotorDefinition.Model.ServoMotor.md#JordanRobot.MotorDefinition.Model.ServoMotor.HasBrake 'JordanRobot\.MotorDefinition\.Model\.ServoMotor\.HasBrake') is [true](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/builtin\-types/bool')\.

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.DriveNames'></a>

## ServoMotor\.DriveNames Property

Gets the drive names in this motor definition\.
Useful for populating UI lists and combo\-boxes\.

```csharp
public System.Collections.Generic.IEnumerable<string> DriveNames { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.Drives'></a>

## ServoMotor\.Drives Property

Gets or sets the drive configurations for this motor\.

```csharp
public System.Collections.Generic.List<JordanRobot.MotorDefinition.Model.Drive> Drives { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[Drive](JordanRobot.MotorDefinition.Model.Drive.md 'JordanRobot\.MotorDefinition\.Model\.Drive')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

### Remarks
Each drive can have multiple voltages with their own curves\.

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.FeedbackPpr'></a>

## ServoMotor\.FeedbackPpr Property

Gets or sets the feedback device pulses per revolution \(PPR\)\.

```csharp
public int FeedbackPpr { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

### Remarks
Used for encoder or resolver feedback resolution\.

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.HasBrake'></a>

## ServoMotor\.HasBrake Property

Gets or sets whether the motor includes an integral holding brake\.

```csharp
public bool HasBrake { get; set; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.Manufacturer'></a>

## ServoMotor\.Manufacturer Property

Gets or sets the company that manufactures the motor\.

```csharp
public string Manufacturer { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.MaxSpeed'></a>

## ServoMotor\.MaxSpeed Property

Gets or sets the theoretical maximum rotational speed of the motor \(RPM\)\.

```csharp
public double MaxSpeed { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.Metadata'></a>

## ServoMotor\.Metadata Property

Gets or sets metadata about the motor definition file\.

```csharp
public JordanRobot.MotorDefinition.Model.MotorMetadata Metadata { get; set; }
```

#### Property Value
[MotorMetadata](JordanRobot.MotorDefinition.Model.MotorMetadata.md 'JordanRobot\.MotorDefinition\.Model\.MotorMetadata')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.MotorName'></a>

## ServoMotor\.MotorName Property

Gets or sets the model name or identifier for the motor\.

```csharp
public string MotorName { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.MotorSignature'></a>

## ServoMotor\.MotorSignature Property

Gets or sets the validation signature for motor properties\.

```csharp
public JordanRobot.MotorDefinition.Model.ValidationSignature? MotorSignature { get; set; }
```

#### Property Value
[ValidationSignature](JordanRobot.MotorDefinition.Model.ValidationSignature.md 'JordanRobot\.MotorDefinition\.Model\.ValidationSignature')

### Remarks
This signature covers motor\-level properties only \(excludes drives and metadata\)\.
Null indicates the motor properties have not been signed\.

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.PartNumber'></a>

## ServoMotor\.PartNumber Property

Gets or sets the manufacturer's part number for the motor\.

```csharp
public string PartNumber { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.Power'></a>

## ServoMotor\.Power Property

Gets or sets the theoretical maximum power output of the motor\.

```csharp
public double Power { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

### Remarks
Expressed in the unit specified by [Units](JordanRobot.MotorDefinition.Model.ServoMotor.md#JordanRobot.MotorDefinition.Model.ServoMotor.Units 'JordanRobot\.MotorDefinition\.Model\.ServoMotor\.Units')\.[Power](JordanRobot.MotorDefinition.Model.UnitSettings.md#JordanRobot.MotorDefinition.Model.UnitSettings.Power 'JordanRobot\.MotorDefinition\.Model\.UnitSettings\.Power')\.

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.RatedContinuousTorque'></a>

## ServoMotor\.RatedContinuousTorque Property

Gets or sets the theoretical maximum continuous torque for the motor\.

```csharp
public double RatedContinuousTorque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.RatedPeakTorque'></a>

## ServoMotor\.RatedPeakTorque Property

Gets or sets the theoretical maximum peak torque for the motor\.

```csharp
public double RatedPeakTorque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.RatedSpeed'></a>

## ServoMotor\.RatedSpeed Property

Gets or sets the rated continuous operating speed of the motor \(RPM\)\.

```csharp
public double RatedSpeed { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.RotorInertia'></a>

## ServoMotor\.RotorInertia Property

Gets or sets the moment of inertia of the motor's rotor\.

```csharp
public double RotorInertia { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

### Remarks
This affects acceleration response\.

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.SchemaVersion'></a>

## ServoMotor\.SchemaVersion Property

Gets or sets the schema version for JSON compatibility\.

```csharp
public string SchemaVersion { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.Units'></a>

## ServoMotor\.Units Property

Gets or sets the unit settings for this motor definition\.

```csharp
public JordanRobot.MotorDefinition.Model.UnitSettings Units { get; set; }
```

#### Property Value
[UnitSettings](JordanRobot.MotorDefinition.Model.UnitSettings.md 'JordanRobot\.MotorDefinition\.Model\.UnitSettings')

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.Weight'></a>

## ServoMotor\.Weight Property

Gets or sets the mass of the motor\.

```csharp
public double Weight { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

### Remarks
Expressed in the unit specified by [Units](JordanRobot.MotorDefinition.Model.ServoMotor.md#JordanRobot.MotorDefinition.Model.ServoMotor.Units 'JordanRobot\.MotorDefinition\.Model\.ServoMotor\.Units')\.[Weight](JordanRobot.MotorDefinition.Model.UnitSettings.md#JordanRobot.MotorDefinition.Model.UnitSettings.Weight 'JordanRobot\.MotorDefinition\.Model\.UnitSettings\.Weight')\.
### Methods

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.AddDrive(string)'></a>

## ServoMotor\.AddDrive\(string\) Method

Adds a new drive configuration with the specified name\.

```csharp
public JordanRobot.MotorDefinition.Model.Drive AddDrive(string name);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.AddDrive(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name for the new drive\.

#### Returns
[Drive](JordanRobot.MotorDefinition.Model.Drive.md 'JordanRobot\.MotorDefinition\.Model\.Drive')  
The newly created drive configuration\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if a drive with the same name already exists\.

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.GetDriveByName(string)'></a>

## ServoMotor\.GetDriveByName\(string\) Method

Gets a drive configuration by name\.

```csharp
public JordanRobot.MotorDefinition.Model.Drive? GetDriveByName(string name);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.GetDriveByName(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the drive to find\.

#### Returns
[Drive](JordanRobot.MotorDefinition.Model.Drive.md 'JordanRobot\.MotorDefinition\.Model\.Drive')  
The matching drive configuration, or null if not found\.

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.HasValidConfiguration()'></a>

## ServoMotor\.HasValidConfiguration\(\) Method

Validates that the motor definition has at least one drive with a valid configuration\.

```csharp
public bool HasValidConfiguration();
```

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if valid; otherwise false\.

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.RemoveDrive(string)'></a>

## ServoMotor\.RemoveDrive\(string\) Method

Removes a drive configuration by name\.

```csharp
public bool RemoveDrive(string name);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.ServoMotor.RemoveDrive(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the drive to remove\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if removed; false if not found\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if attempting to remove the last drive\.