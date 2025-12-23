#### [MotorDefinition](index.md 'index')
### [CurveEditor\.Models](CurveEditor.Models.md 'CurveEditor\.Models')

## CurveSeries Class

Represents a named series of motor torque/speed data points\.
Each series represents a specific operating condition \(e\.g\., "Peak" or "Continuous"\)\.

```csharp
public class CurveSeries : System.ComponentModel.INotifyPropertyChanged
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; CurveSeries

Implements [System\.ComponentModel\.INotifyPropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged 'System\.ComponentModel\.INotifyPropertyChanged')
### Constructors

<a name='CurveEditor.Models.CurveSeries.CurveSeries()'></a>

## CurveSeries\(\) Constructor

Creates a new CurveSeries with default values\.

```csharp
public CurveSeries();
```

<a name='CurveEditor.Models.CurveSeries.CurveSeries(string)'></a>

## CurveSeries\(string\) Constructor

Creates a new CurveSeries with the specified name\.

```csharp
public CurveSeries(string name);
```
#### Parameters

<a name='CurveEditor.Models.CurveSeries.CurveSeries(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the curve series\.
### Properties

<a name='CurveEditor.Models.CurveSeries.Data'></a>

## CurveSeries\.Data Property

The data points for this curve, stored at 1% increments\.
Should contain 101 points \(0% through 100%\)\.

```csharp
public System.Collections.Generic.List<CurveEditor.Models.DataPoint> Data { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DataPoint](CurveEditor.Models.DataPoint.md 'CurveEditor\.Models\.DataPoint')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='CurveEditor.Models.CurveSeries.IsVisible'></a>

## CurveSeries\.IsVisible Property

Indicates whether this curve series is visible in the chart\.
This is a runtime\-only property that is not persisted to JSON\.

```csharp
public bool IsVisible { get; set; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

<a name='CurveEditor.Models.CurveSeries.Locked'></a>

## CurveSeries\.Locked Property

Indicates whether this curve series is locked for editing\.
When true, the curve data should not be modified\.

```csharp
public bool Locked { get; set; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

<a name='CurveEditor.Models.CurveSeries.Name'></a>

## CurveSeries\.Name Property

The name of this curve series \(e\.g\., "Peak", "Continuous"\)\.

```csharp
public string Name { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='CurveEditor.Models.CurveSeries.Notes'></a>

## CurveSeries\.Notes Property

Notes or comments about this curve series\.

```csharp
public string Notes { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='CurveEditor.Models.CurveSeries.PointCount'></a>

## CurveSeries\.PointCount Property

Gets the number of data points in this series\.

```csharp
public int PointCount { get; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')
### Methods

<a name='CurveEditor.Models.CurveSeries.InitializeData(double,double)'></a>

## CurveSeries\.InitializeData\(double, double\) Method

Initializes the data with 101 points \(0% to 100%\) at 1% increments\.

```csharp
public void InitializeData(double maxRpm, double defaultTorque);
```
#### Parameters

<a name='CurveEditor.Models.CurveSeries.InitializeData(double,double).maxRpm'></a>

`maxRpm` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The maximum RPM of the motor\.

<a name='CurveEditor.Models.CurveSeries.InitializeData(double,double).defaultTorque'></a>

`defaultTorque` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The default torque value for all points\.

<a name='CurveEditor.Models.CurveSeries.OnPropertyChanged(string)'></a>

## CurveSeries\.OnPropertyChanged\(string\) Method

Raises the PropertyChanged event\.

```csharp
protected virtual void OnPropertyChanged(string? propertyName=null);
```
#### Parameters

<a name='CurveEditor.Models.CurveSeries.OnPropertyChanged(string).propertyName'></a>

`propertyName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the property that changed\.

<a name='CurveEditor.Models.CurveSeries.ValidateDataIntegrity()'></a>

## CurveSeries\.ValidateDataIntegrity\(\) Method

Validates that the series has the expected 101 data points at 1% increments\.

```csharp
public bool ValidateDataIntegrity();
```

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the series has valid data structure; otherwise false\.
### Events

<a name='CurveEditor.Models.CurveSeries.PropertyChanged'></a>

## CurveSeries\.PropertyChanged Event

Occurs when a property value changes\.

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
```

Implements [PropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged 'System\.ComponentModel\.INotifyPropertyChanged\.PropertyChanged')

#### Event Type
[System\.ComponentModel\.PropertyChangedEventHandler](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventhandler 'System\.ComponentModel\.PropertyChangedEventHandler')