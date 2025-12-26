#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinitions\.Model](JordanRobot.MotorDefinitions.Model.md 'JordanRobot\.MotorDefinitions\.Model')

## DriveConfiguration Class

Represents a servo drive configuration for a motor\.
Contains voltage\-specific configurations and their associated curve series\.

```csharp
public class DriveConfiguration : System.ComponentModel.INotifyPropertyChanged
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; DriveConfiguration

Implements [System\.ComponentModel\.INotifyPropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged 'System\.ComponentModel\.INotifyPropertyChanged')
### Constructors

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.DriveConfiguration()'></a>

## DriveConfiguration\(\) Constructor

Creates a new DriveConfiguration with default values\.

```csharp
public DriveConfiguration();
```

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.DriveConfiguration(string)'></a>

## DriveConfiguration\(string\) Constructor

Creates a new DriveConfiguration with the specified name\.

```csharp
public DriveConfiguration(string name);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.DriveConfiguration(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the drive\.
### Fields

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.DefaultVoltageTolerance'></a>

## DriveConfiguration\.DefaultVoltageTolerance Field

Default tolerance for voltage matching in volts\.

```csharp
public const double DefaultVoltageTolerance = 0.1;
```

#### Field Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')
### Properties

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.Manufacturer'></a>

## DriveConfiguration\.Manufacturer Property

The manufacturer of the drive\.

```csharp
public string Manufacturer { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.Name'></a>

## DriveConfiguration\.Name Property

The name or model identifier of the drive\.

```csharp
public string Name { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.PartNumber'></a>

## DriveConfiguration\.PartNumber Property

The manufacturer's part number for the servo drive\.

```csharp
public string PartNumber { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.VoltageConfigurations'></a>

## DriveConfiguration\.VoltageConfigurations Property

Gets a LINQ\-friendly enumeration of voltage configurations\.
This is a convenience alias for [Voltages](JordanRobot.MotorDefinitions.Model.DriveConfiguration.md#JordanRobot.MotorDefinitions.Model.DriveConfiguration.Voltages 'JordanRobot\.MotorDefinitions\.Model\.DriveConfiguration\.Voltages')\.

```csharp
public System.Collections.Generic.IEnumerable<JordanRobot.MotorDefinitions.Model.VoltageConfiguration> VoltageConfigurations { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[VoltageConfiguration](JordanRobot.MotorDefinitions.Model.VoltageConfiguration.md 'JordanRobot\.MotorDefinitions\.Model\.VoltageConfiguration')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.VoltageNames'></a>

## DriveConfiguration\.VoltageNames Property

Gets display\-friendly voltage names \(e\.g\., "208 V"\)\.
Useful for populating UI lists and combo\-boxes\.

```csharp
public System.Collections.Generic.IEnumerable<string> VoltageNames { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.Voltages'></a>

## DriveConfiguration\.Voltages Property

The collection of voltage configurations for this drive\.

```csharp
public System.Collections.Generic.List<JordanRobot.MotorDefinitions.Model.VoltageConfiguration> Voltages { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[VoltageConfiguration](JordanRobot.MotorDefinitions.Model.VoltageConfiguration.md 'JordanRobot\.MotorDefinitions\.Model\.VoltageConfiguration')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.VoltageValues'></a>

## DriveConfiguration\.VoltageValues Property

Gets the numeric voltage values\.
Useful for populating UI lists and combo\-boxes\.

```csharp
public System.Collections.Generic.IEnumerable<double> VoltageValues { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')
### Methods

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.AddVoltageConfiguration(double)'></a>

## DriveConfiguration\.AddVoltageConfiguration\(double\) Method

Adds a new voltage configuration\.

```csharp
public JordanRobot.MotorDefinitions.Model.VoltageConfiguration AddVoltageConfiguration(double voltage);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.AddVoltageConfiguration(double).voltage'></a>

`voltage` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The voltage value\.

#### Returns
[VoltageConfiguration](JordanRobot.MotorDefinitions.Model.VoltageConfiguration.md 'JordanRobot\.MotorDefinitions\.Model\.VoltageConfiguration')  
The newly created voltage configuration\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if a configuration with the same voltage already exists\.

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.GetVoltageConfiguration(double,double)'></a>

## DriveConfiguration\.GetVoltageConfiguration\(double, double\) Method

Gets a voltage configuration by voltage value\.

```csharp
public JordanRobot.MotorDefinitions.Model.VoltageConfiguration? GetVoltageConfiguration(double voltage, double tolerance=0.1);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.GetVoltageConfiguration(double,double).voltage'></a>

`voltage` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The voltage to find\.

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.GetVoltageConfiguration(double,double).tolerance'></a>

`tolerance` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The tolerance for matching voltage values \(default 0\.1V\)\.

#### Returns
[VoltageConfiguration](JordanRobot.MotorDefinitions.Model.VoltageConfiguration.md 'JordanRobot\.MotorDefinitions\.Model\.VoltageConfiguration')  
The matching voltage configuration, or null if not found\.

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.OnPropertyChanged(string)'></a>

## DriveConfiguration\.OnPropertyChanged\(string\) Method

Raises the PropertyChanged event\.

```csharp
protected virtual void OnPropertyChanged(string? propertyName=null);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.OnPropertyChanged(string).propertyName'></a>

`propertyName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the property that changed\.
### Events

<a name='JordanRobot.MotorDefinitions.Model.DriveConfiguration.PropertyChanged'></a>

## DriveConfiguration\.PropertyChanged Event

Occurs when a property value changes\.

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
```

Implements [PropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged 'System\.ComponentModel\.INotifyPropertyChanged\.PropertyChanged')

#### Event Type
[System\.ComponentModel\.PropertyChangedEventHandler](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventhandler 'System\.ComponentModel\.PropertyChangedEventHandler')