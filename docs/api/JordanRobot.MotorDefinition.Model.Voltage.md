#### [JordanRobot\.MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Model](JordanRobot.MotorDefinition.Model.md 'JordanRobot\.MotorDefinition\.Model')

## Voltage Class

Represents voltage\-specific configuration and performance data for a motor/drive combination\.

```csharp
public class Voltage : System.ComponentModel.INotifyPropertyChanged
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; Voltage

Implements [System\.ComponentModel\.INotifyPropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged 'System\.ComponentModel\.INotifyPropertyChanged')

### Remarks
Each [Voltage](JordanRobot.MotorDefinition.Model.Voltage.md 'JordanRobot\.MotorDefinition\.Model\.Voltage') contains one or more [Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve') definitions representing different
operating conditions \(for example, peak vs\. continuous\)\.
### Constructors

<a name='JordanRobot.MotorDefinition.Model.Voltage.Voltage()'></a>

## Voltage\(\) Constructor

Creates a new Voltage with default values\.

```csharp
public Voltage();
```

<a name='JordanRobot.MotorDefinition.Model.Voltage.Voltage(double)'></a>

## Voltage\(double\) Constructor

Creates a new Voltage with the specified voltage\.

```csharp
public Voltage(double voltage);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.Voltage.Voltage(double).voltage'></a>

`voltage` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The operating voltage\.
### Properties

<a name='JordanRobot.MotorDefinition.Model.Voltage.ContinuousAmperage'></a>

## Voltage\.ContinuousAmperage Property

Gets or sets the current draw during continuous operation at rated torque \(A\)\.

```csharp
public double ContinuousAmperage { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.Voltage.Curves'></a>

## Voltage\.Curves Property

Gets or sets the curves for this voltage configuration \(for example, "Peak" and "Continuous"\)\.

```csharp
public System.Collections.Generic.List<JordanRobot.MotorDefinition.Model.Curve> Curves { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='JordanRobot.MotorDefinition.Model.Voltage.DisplayName'></a>

## Voltage\.DisplayName Property

Gets a display\-friendly name for this voltage configuration \(for example, "208 V"\)\.

```csharp
public string DisplayName { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

### Remarks
Useful for populating UI lists and combo\-boxes\.

<a name='JordanRobot.MotorDefinition.Model.Voltage.MaxSpeed'></a>

## Voltage\.MaxSpeed Property

Gets or sets the maximum rotational speed at this voltage \(RPM\)\.

```csharp
public double MaxSpeed { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.Voltage.PeakAmperage'></a>

## Voltage\.PeakAmperage Property

Gets or sets the maximum current draw during peak torque operation \(A\)\.

```csharp
public double PeakAmperage { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.Voltage.Power'></a>

## Voltage\.Power Property

Gets or sets the power output at this voltage\.

```csharp
public double Power { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

### Remarks
Expressed in the unit specified by the parent motor's [UnitSettings](JordanRobot.MotorDefinition.Model.UnitSettings.md 'JordanRobot\.MotorDefinition\.Model\.UnitSettings')\.

<a name='JordanRobot.MotorDefinition.Model.Voltage.RatedContinuousTorque'></a>

## Voltage\.RatedContinuousTorque Property

Gets or sets the rated continuous torque at this voltage\.

```csharp
public double RatedContinuousTorque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.Voltage.RatedPeakTorque'></a>

## Voltage\.RatedPeakTorque Property

Gets or sets the rated peak torque at this voltage\.

```csharp
public double RatedPeakTorque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.Voltage.RatedSpeed'></a>

## Voltage\.RatedSpeed Property

Gets or sets the rated continuous operating speed at this voltage \(RPM\)\.

```csharp
public double RatedSpeed { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.Voltage.Value'></a>

## Voltage\.Value Property

Gets or sets the operating voltage \(V\)\.

```csharp
public double Value { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')
### Methods

<a name='JordanRobot.MotorDefinition.Model.Voltage.AddSeries(string,double)'></a>

## Voltage\.AddSeries\(string, double\) Method

Adds a new curve with the specified name\.

```csharp
public JordanRobot.MotorDefinition.Model.Curve AddSeries(string name, double initializeTorque=0.0);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.Voltage.AddSeries(string,double).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name for the new curve\.

<a name='JordanRobot.MotorDefinition.Model.Voltage.AddSeries(string,double).initializeTorque'></a>

`initializeTorque` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The default torque value for all points\.

#### Returns
[Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve')  
The newly created curve\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if a curve with the same name already exists\.

<a name='JordanRobot.MotorDefinition.Model.Voltage.GetSeriesByName(string)'></a>

## Voltage\.GetSeriesByName\(string\) Method

Gets a curve by name\.

```csharp
public JordanRobot.MotorDefinition.Model.Curve? GetSeriesByName(string name);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.Voltage.GetSeriesByName(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the curve to find\.

#### Returns
[Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve')  
The matching curve, or null if not found\.

<a name='JordanRobot.MotorDefinition.Model.Voltage.OnPropertyChanged(string)'></a>

## Voltage\.OnPropertyChanged\(string\) Method

Notifies listeners that a property value has changed\.

```csharp
protected virtual void OnPropertyChanged(string? propertyName=null);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.Voltage.OnPropertyChanged(string).propertyName'></a>

`propertyName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the changed property\.
### Events

<a name='JordanRobot.MotorDefinition.Model.Voltage.PropertyChanged'></a>

## Voltage\.PropertyChanged Event

Raised when a property value changes\.

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
```

Implements [PropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged 'System\.ComponentModel\.INotifyPropertyChanged\.PropertyChanged')

#### Event Type
[System\.ComponentModel\.PropertyChangedEventHandler](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventhandler 'System\.ComponentModel\.PropertyChangedEventHandler')