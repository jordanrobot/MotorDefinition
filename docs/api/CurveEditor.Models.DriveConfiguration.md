#### [MotorDefinition](index.md 'index')
### [CurveEditor\.Models](CurveEditor.Models.md 'CurveEditor\.Models')

## DriveConfiguration Class

Represents a servo drive configuration for a motor\.
Contains voltage\-specific configurations and their associated curve series\.

```csharp
public class DriveConfiguration : System.ComponentModel.INotifyPropertyChanged
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; DriveConfiguration

Implements [System\.ComponentModel\.INotifyPropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged 'System\.ComponentModel\.INotifyPropertyChanged')
### Constructors

<a name='CurveEditor.Models.DriveConfiguration.DriveConfiguration()'></a>

## DriveConfiguration\(\) Constructor

Creates a new DriveConfiguration with default values\.

```csharp
public DriveConfiguration();
```

<a name='CurveEditor.Models.DriveConfiguration.DriveConfiguration(string)'></a>

## DriveConfiguration\(string\) Constructor

Creates a new DriveConfiguration with the specified name\.

```csharp
public DriveConfiguration(string name);
```
#### Parameters

<a name='CurveEditor.Models.DriveConfiguration.DriveConfiguration(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the drive\.
### Fields

<a name='CurveEditor.Models.DriveConfiguration.DefaultVoltageTolerance'></a>

## DriveConfiguration\.DefaultVoltageTolerance Field

Default tolerance for voltage matching in volts\.

```csharp
public const double DefaultVoltageTolerance = 0.1;
```

#### Field Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')
### Properties

<a name='CurveEditor.Models.DriveConfiguration.Manufacturer'></a>

## DriveConfiguration\.Manufacturer Property

The manufacturer of the drive\.

```csharp
public string Manufacturer { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='CurveEditor.Models.DriveConfiguration.Name'></a>

## DriveConfiguration\.Name Property

The name or model identifier of the drive\.

```csharp
public string Name { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='CurveEditor.Models.DriveConfiguration.PartNumber'></a>

## DriveConfiguration\.PartNumber Property

The manufacturer's part number for the servo drive\.

```csharp
public string PartNumber { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='CurveEditor.Models.DriveConfiguration.Voltages'></a>

## DriveConfiguration\.Voltages Property

The collection of voltage configurations for this drive\.

```csharp
public System.Collections.Generic.List<CurveEditor.Models.VoltageConfiguration> Voltages { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[VoltageConfiguration](CurveEditor.Models.VoltageConfiguration.md 'CurveEditor\.Models\.VoltageConfiguration')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')
### Methods

<a name='CurveEditor.Models.DriveConfiguration.AddVoltageConfiguration(double)'></a>

## DriveConfiguration\.AddVoltageConfiguration\(double\) Method

Adds a new voltage configuration\.

```csharp
public CurveEditor.Models.VoltageConfiguration AddVoltageConfiguration(double voltage);
```
#### Parameters

<a name='CurveEditor.Models.DriveConfiguration.AddVoltageConfiguration(double).voltage'></a>

`voltage` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The voltage value\.

#### Returns
[VoltageConfiguration](CurveEditor.Models.VoltageConfiguration.md 'CurveEditor\.Models\.VoltageConfiguration')  
The newly created voltage configuration\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if a configuration with the same voltage already exists\.

<a name='CurveEditor.Models.DriveConfiguration.GetVoltageConfiguration(double,double)'></a>

## DriveConfiguration\.GetVoltageConfiguration\(double, double\) Method

Gets a voltage configuration by voltage value\.

```csharp
public CurveEditor.Models.VoltageConfiguration? GetVoltageConfiguration(double voltage, double tolerance=0.1);
```
#### Parameters

<a name='CurveEditor.Models.DriveConfiguration.GetVoltageConfiguration(double,double).voltage'></a>

`voltage` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The voltage to find\.

<a name='CurveEditor.Models.DriveConfiguration.GetVoltageConfiguration(double,double).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The tolerance for matching voltage values \(default 0\.1V\)\.

#### Returns
[VoltageConfiguration](CurveEditor.Models.VoltageConfiguration.md 'CurveEditor\.Models\.VoltageConfiguration')  
The matching voltage configuration, or null if not found\.

<a name='CurveEditor.Models.DriveConfiguration.OnPropertyChanged(string)'></a>

## DriveConfiguration\.OnPropertyChanged\(string\) Method

Raises the PropertyChanged event\.

```csharp
protected virtual void OnPropertyChanged(string? propertyName=null);
```
#### Parameters

<a name='CurveEditor.Models.DriveConfiguration.OnPropertyChanged(string).propertyName'></a>

`propertyName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the property that changed\.
### Events

<a name='CurveEditor.Models.DriveConfiguration.PropertyChanged'></a>

## DriveConfiguration\.PropertyChanged Event

Occurs when a property value changes\.

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
```

Implements [PropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged 'System\.ComponentModel\.INotifyPropertyChanged\.PropertyChanged')

#### Event Type
[System\.ComponentModel\.PropertyChangedEventHandler](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventhandler 'System\.ComponentModel\.PropertyChangedEventHandler')