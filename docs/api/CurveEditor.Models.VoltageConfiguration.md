#### [MotorDefinition](index.md 'index')
### [CurveEditor\.Models](CurveEditor.Models.md 'CurveEditor\.Models')

## VoltageConfiguration Class

Represents voltage\-specific configuration and performance data for a motor/drive combination\.
Contains the curve series for this specific voltage setting\.

```csharp
public class VoltageConfiguration : System.ComponentModel.INotifyPropertyChanged
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; VoltageConfiguration

Implements [System\.ComponentModel\.INotifyPropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged 'System\.ComponentModel\.INotifyPropertyChanged')
### Constructors

<a name='CurveEditor.Models.VoltageConfiguration.VoltageConfiguration()'></a>

## VoltageConfiguration\(\) Constructor

Creates a new VoltageConfiguration with default values\.

```csharp
public VoltageConfiguration();
```

<a name='CurveEditor.Models.VoltageConfiguration.VoltageConfiguration(double)'></a>

## VoltageConfiguration\(double\) Constructor

Creates a new VoltageConfiguration with the specified voltage\.

```csharp
public VoltageConfiguration(double voltage);
```
#### Parameters

<a name='CurveEditor.Models.VoltageConfiguration.VoltageConfiguration(double).voltage'></a>

`voltage` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The operating voltage\.
### Properties

<a name='CurveEditor.Models.VoltageConfiguration.ContinuousAmperage'></a>

## VoltageConfiguration\.ContinuousAmperage Property

The current draw during continuous operation at rated torque \(A\)\.

```csharp
public double ContinuousAmperage { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.VoltageConfiguration.CurveSeries'></a>

## VoltageConfiguration\.CurveSeries Property

Gets a LINQ\-friendly enumeration of Curve Series\.
This is a convenience alias for [Series](CurveEditor.Models.VoltageConfiguration.md#CurveEditor.Models.VoltageConfiguration.Series 'CurveEditor\.Models\.VoltageConfiguration\.Series')\.

```csharp
public System.Collections.Generic.IEnumerable<CurveEditor.Models.CurveSeries> CurveSeries { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[CurveSeries](CurveEditor.Models.CurveSeries.md 'CurveEditor\.Models\.CurveSeries')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

<a name='CurveEditor.Models.VoltageConfiguration.DisplayName'></a>

## VoltageConfiguration\.DisplayName Property

Gets a display\-friendly name for this voltage configuration \(e\.g\., "208 V"\)\.
Useful for populating UI lists and combo\-boxes\.

```csharp
public string DisplayName { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='CurveEditor.Models.VoltageConfiguration.MaxSpeed'></a>

## VoltageConfiguration\.MaxSpeed Property

The maximum rotational speed at this voltage \(RPM\)\.

```csharp
public double MaxSpeed { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.VoltageConfiguration.PeakAmperage'></a>

## VoltageConfiguration\.PeakAmperage Property

The maximum current draw during peak torque operation \(A\)\.

```csharp
public double PeakAmperage { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.VoltageConfiguration.Power'></a>

## VoltageConfiguration\.Power Property

The power output at this voltage \(in the unit specified by Units\.Power\)\.

```csharp
public double Power { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.VoltageConfiguration.RatedContinuousTorque'></a>

## VoltageConfiguration\.RatedContinuousTorque Property

The torque the motor can produce continuously at this voltage without overheating\.

```csharp
public double RatedContinuousTorque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.VoltageConfiguration.RatedPeakTorque'></a>

## VoltageConfiguration\.RatedPeakTorque Property

The maximum torque the motor can produce for short periods at this voltage\.

```csharp
public double RatedPeakTorque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.VoltageConfiguration.RatedSpeed'></a>

## VoltageConfiguration\.RatedSpeed Property

The rated continuous operating speed at this voltage \(RPM\)\.

```csharp
public double RatedSpeed { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.VoltageConfiguration.Series'></a>

## VoltageConfiguration\.Series Property

The collection of curve series for this voltage configuration \(e\.g\., "Peak", "Continuous"\)\.

```csharp
public System.Collections.Generic.List<CurveEditor.Models.CurveSeries> Series { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[CurveSeries](CurveEditor.Models.CurveSeries.md 'CurveEditor\.Models\.CurveSeries')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='CurveEditor.Models.VoltageConfiguration.Voltage'></a>

## VoltageConfiguration\.Voltage Property

The operating voltage \(V\)\.

```csharp
public double Voltage { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')
### Methods

<a name='CurveEditor.Models.VoltageConfiguration.AddSeries(string,double)'></a>

## VoltageConfiguration\.AddSeries\(string, double\) Method

Adds a new series with the specified name\.

```csharp
public CurveEditor.Models.CurveSeries AddSeries(string name, double initializeTorque=0.0);
```
#### Parameters

<a name='CurveEditor.Models.VoltageConfiguration.AddSeries(string,double).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name for the new series\.

<a name='CurveEditor.Models.VoltageConfiguration.AddSeries(string,double).initializeTorque'></a>

`initializeTorque` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The default torque value for all points\.

#### Returns
[CurveSeries](CurveEditor.Models.CurveSeries.md 'CurveEditor\.Models\.CurveSeries')  
The newly created series\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if a series with the same name already exists\.

<a name='CurveEditor.Models.VoltageConfiguration.GetSeriesByName(string)'></a>

## VoltageConfiguration\.GetSeriesByName\(string\) Method

Gets a curve series by name\.

```csharp
public CurveEditor.Models.CurveSeries? GetSeriesByName(string name);
```
#### Parameters

<a name='CurveEditor.Models.VoltageConfiguration.GetSeriesByName(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the series to find\.

#### Returns
[CurveSeries](CurveEditor.Models.CurveSeries.md 'CurveEditor\.Models\.CurveSeries')  
The matching series, or null if not found\.

<a name='CurveEditor.Models.VoltageConfiguration.OnPropertyChanged(string)'></a>

## VoltageConfiguration\.OnPropertyChanged\(string\) Method

Notifies listeners that a property value has changed\.

```csharp
protected virtual void OnPropertyChanged(string? propertyName=null);
```
#### Parameters

<a name='CurveEditor.Models.VoltageConfiguration.OnPropertyChanged(string).propertyName'></a>

`propertyName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the changed property\.
### Events

<a name='CurveEditor.Models.VoltageConfiguration.PropertyChanged'></a>

## VoltageConfiguration\.PropertyChanged Event

Raised when a property value changes\.

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
```

Implements [PropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged 'System\.ComponentModel\.INotifyPropertyChanged\.PropertyChanged')

#### Event Type
[System\.ComponentModel\.PropertyChangedEventHandler](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventhandler 'System\.ComponentModel\.PropertyChangedEventHandler')