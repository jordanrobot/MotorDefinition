#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Model](JordanRobot.MotorDefinition.Model.md 'JordanRobot\.MotorDefinition\.Model')

## Drive Class

Represents a drive configuration for a motor\.

```csharp
public class Drive : System.ComponentModel.INotifyPropertyChanged
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; Drive

Implements [System\.ComponentModel\.INotifyPropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged 'System\.ComponentModel\.INotifyPropertyChanged')

### Remarks
A drive contains one or more [Voltage](JordanRobot.MotorDefinition.Model.Voltage.md 'JordanRobot\.MotorDefinition\.Model\.Voltage') configurations, each of which contains one or more
[Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve') definitions\.
### Constructors

<a name='JordanRobot.MotorDefinition.Model.Drive.Drive()'></a>

## Drive\(\) Constructor

Creates a new Drive with default values\.

```csharp
public Drive();
```

<a name='JordanRobot.MotorDefinition.Model.Drive.Drive(string)'></a>

## Drive\(string\) Constructor

Creates a new Drive with the specified name\.

```csharp
public Drive(string name);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.Drive.Drive(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the drive\.
### Fields

<a name='JordanRobot.MotorDefinition.Model.Drive.DefaultVoltageTolerance'></a>

## Drive\.DefaultVoltageTolerance Field

Default tolerance for matching voltage values \(in volts\)\.

```csharp
public const double DefaultVoltageTolerance = 0.1;
```

#### Field Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')
### Properties

<a name='JordanRobot.MotorDefinition.Model.Drive.DriveSignature'></a>

## Drive\.DriveSignature Property

Gets or sets the validation signature for this drive configuration\.

```csharp
public JordanRobot.MotorDefinition.Model.ValidationSignature? DriveSignature { get; set; }
```

#### Property Value
[ValidationSignature](JordanRobot.MotorDefinition.Model.ValidationSignature.md 'JordanRobot\.MotorDefinition\.Model\.ValidationSignature')

### Remarks
This signature covers the drive properties including all voltages\.
Null indicates the drive has not been signed\.

<a name='JordanRobot.MotorDefinition.Model.Drive.Manufacturer'></a>

## Drive\.Manufacturer Property

Gets or sets the manufacturer of the drive\.

```csharp
public string Manufacturer { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.Drive.Name'></a>

## Drive\.Name Property

Gets or sets the name or model identifier of the drive\.

```csharp
public string Name { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.Drive.PartNumber'></a>

## Drive\.PartNumber Property

Gets or sets the manufacturer's part number for the servo drive\.

```csharp
public string PartNumber { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.Drive.VoltageNames'></a>

## Drive\.VoltageNames Property

Gets display\-friendly voltage names \(for example, "208 V"\)\.

```csharp
public System.Collections.Generic.IEnumerable<string> VoltageNames { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

### Remarks
Useful for populating UI lists and combo\-boxes\.

<a name='JordanRobot.MotorDefinition.Model.Drive.Voltages'></a>

## Drive\.Voltages Property

Gets or sets the collection of voltage configurations for this drive\.

```csharp
public System.Collections.Generic.List<JordanRobot.MotorDefinition.Model.Voltage> Voltages { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[Voltage](JordanRobot.MotorDefinition.Model.Voltage.md 'JordanRobot\.MotorDefinition\.Model\.Voltage')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='JordanRobot.MotorDefinition.Model.Drive.VoltageValues'></a>

## Drive\.VoltageValues Property

Gets the numeric voltage values\.

```csharp
public System.Collections.Generic.IEnumerable<double> VoltageValues { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

### Remarks
Useful for populating UI lists and combo\-boxes\.
### Methods

<a name='JordanRobot.MotorDefinition.Model.Drive.AddVoltage(double)'></a>

## Drive\.AddVoltage\(double\) Method

Adds a new voltage configuration\.

```csharp
public JordanRobot.MotorDefinition.Model.Voltage AddVoltage(double voltage);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.Drive.AddVoltage(double).voltage'></a>

`voltage` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The voltage value\.

#### Returns
[Voltage](JordanRobot.MotorDefinition.Model.Voltage.md 'JordanRobot\.MotorDefinition\.Model\.Voltage')  
The newly created voltage configuration\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if a configuration with the same voltage already exists\.

<a name='JordanRobot.MotorDefinition.Model.Drive.GetVoltage(double,double)'></a>

## Drive\.GetVoltage\(double, double\) Method

Gets a voltage configuration by voltage value\.

```csharp
public JordanRobot.MotorDefinition.Model.Voltage? GetVoltage(double voltage, double tolerance=0.1);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.Drive.GetVoltage(double,double).voltage'></a>

`voltage` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The voltage to find\.

<a name='JordanRobot.MotorDefinition.Model.Drive.GetVoltage(double,double).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The tolerance for matching voltage values \(default 0\.1V\)\.

#### Returns
[Voltage](JordanRobot.MotorDefinition.Model.Voltage.md 'JordanRobot\.MotorDefinition\.Model\.Voltage')  
The matching voltage configuration, or null if not found\.

<a name='JordanRobot.MotorDefinition.Model.Drive.OnPropertyChanged(string)'></a>

## Drive\.OnPropertyChanged\(string\) Method

Raises the PropertyChanged event\.

```csharp
protected virtual void OnPropertyChanged(string? propertyName=null);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.Drive.OnPropertyChanged(string).propertyName'></a>

`propertyName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the property that changed\.
### Events

<a name='JordanRobot.MotorDefinition.Model.Drive.PropertyChanged'></a>

## Drive\.PropertyChanged Event

Occurs when a property value changes\.

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
```

Implements [PropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged 'System\.ComponentModel\.INotifyPropertyChanged\.PropertyChanged')

#### Event Type
[System\.ComponentModel\.PropertyChangedEventHandler](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventhandler 'System\.ComponentModel\.PropertyChangedEventHandler')