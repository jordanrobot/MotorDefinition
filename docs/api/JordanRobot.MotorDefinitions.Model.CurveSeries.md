#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinitions\.Model](JordanRobot.MotorDefinitions.Model.md 'JordanRobot\.MotorDefinitions\.Model')

## CurveSeries Class

Represents a named series of motor torque/speed data points\.
Each series represents a specific operating condition \(e\.g\., "Peak" or "Continuous"\)\.

```csharp
public class CurveSeries : System.ComponentModel.INotifyPropertyChanged
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; CurveSeries

Implements [System\.ComponentModel\.INotifyPropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged 'System\.ComponentModel\.INotifyPropertyChanged')
### Constructors

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.CurveSeries()'></a>

## CurveSeries\(\) Constructor

Creates a new CurveSeries with default values\.

```csharp
public CurveSeries();
```

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.CurveSeries(string)'></a>

## CurveSeries\(string\) Constructor

Creates a new CurveSeries with the specified name\.

```csharp
public CurveSeries(string name);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.CurveSeries(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the curve series\.
### Properties

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.Data'></a>

## CurveSeries\.Data Property

The data points for this curve\.
Typically contains 101 points at 1% increments \(0% through 100%\), but may contain fewer points\.
Values above 100% may be present to represent overspeed ranges\.

```csharp
public System.Collections.Generic.List<JordanRobot.MotorDefinitions.Model.DataPoint> Data { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[DataPoint](JordanRobot.MotorDefinitions.Model.DataPoint.md 'JordanRobot\.MotorDefinitions\.Model\.DataPoint')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.IsVisible'></a>

## CurveSeries\.IsVisible Property

Indicates whether this curve series is visible in the chart\.
This is a runtime\-only property that is not persisted to JSON\.

```csharp
public bool IsVisible { get; set; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.Locked'></a>

## CurveSeries\.Locked Property

Indicates whether this curve series is locked for editing\.
When true, the curve data should not be modified\.

```csharp
public bool Locked { get; set; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.Name'></a>

## CurveSeries\.Name Property

The name of this curve series \(e\.g\., "Peak", "Continuous"\)\.

```csharp
public string Name { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.Notes'></a>

## CurveSeries\.Notes Property

Notes or comments about this curve series\.

```csharp
public string Notes { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.Percents'></a>

## CurveSeries\.Percents Property

Gets the percentage axis \(0\.\.100\) for this series\.

```csharp
public System.Collections.Generic.IEnumerable<int> Percents { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.PointCount'></a>

## CurveSeries\.PointCount Property

Gets the number of data points in this series\.

```csharp
public int PointCount { get; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.Rpms'></a>

## CurveSeries\.Rpms Property

Gets the RPM values for this series\.

```csharp
public System.Collections.Generic.IEnumerable<double> Rpms { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.Torques'></a>

## CurveSeries\.Torques Property

Gets the torque values for this series\.

```csharp
public System.Collections.Generic.IEnumerable<double> Torques { get; }
```

#### Property Value
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')
### Methods

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.GetPointByPercent(int)'></a>

## CurveSeries\.GetPointByPercent\(int\) Method

Gets the data point for a given percent\.
Prefer this for quick lookups when exporting or rendering tables\.

```csharp
public JordanRobot.MotorDefinitions.Model.DataPoint GetPointByPercent(int percent);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.GetPointByPercent(int).percent'></a>

`percent` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The percent \(non\-negative\)\.

#### Returns
[DataPoint](JordanRobot.MotorDefinitions.Model.DataPoint.md 'JordanRobot\.MotorDefinitions\.Model\.DataPoint')  
The matching data point\.

#### Exceptions

[System\.ArgumentOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception 'System\.ArgumentOutOfRangeException')  
Thrown when [percent](JordanRobot.MotorDefinitions.Model.CurveSeries.md#JordanRobot.MotorDefinitions.Model.CurveSeries.GetPointByPercent(int).percent 'JordanRobot\.MotorDefinitions\.Model\.CurveSeries\.GetPointByPercent\(int\)\.percent') is negative\.

[System\.Collections\.Generic\.KeyNotFoundException](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.keynotfoundexception 'System\.Collections\.Generic\.KeyNotFoundException')  
Thrown when no point exists for [percent](JordanRobot.MotorDefinitions.Model.CurveSeries.md#JordanRobot.MotorDefinitions.Model.CurveSeries.GetPointByPercent(int).percent 'JordanRobot\.MotorDefinitions\.Model\.CurveSeries\.GetPointByPercent\(int\)\.percent')\.

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.InitializeData(double,double)'></a>

## CurveSeries\.InitializeData\(double, double\) Method

Initializes the data with the default 101 points \(0% to 100%\) at 1% increments\.
The file format can store 0\.\.101 points per series; this helper always generates the standard 1% curve\.

```csharp
public void InitializeData(double maxRpm, double defaultTorque);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.InitializeData(double,double).maxRpm'></a>

`maxRpm` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The maximum RPM of the motor\.

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.InitializeData(double,double).defaultTorque'></a>

`defaultTorque` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The default torque value for all points\.

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.OnPropertyChanged(string)'></a>

## CurveSeries\.OnPropertyChanged\(string\) Method

Raises the PropertyChanged event\.

```csharp
protected virtual void OnPropertyChanged(string? propertyName=null);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.OnPropertyChanged(string).propertyName'></a>

`propertyName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the property that changed\.

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.ToPercentLookup()'></a>

## CurveSeries\.ToPercentLookup\(\) Method

Creates a lookup dictionary keyed by percent \(0\.\.100\)\.
This is useful for caching fast lookups in client applications\.

```csharp
public System.Collections.Generic.IReadOnlyDictionary<int,JordanRobot.MotorDefinitions.Model.DataPoint> ToPercentLookup();
```

#### Returns
[System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[DataPoint](JordanRobot.MotorDefinitions.Model.DataPoint.md 'JordanRobot\.MotorDefinitions\.Model\.DataPoint')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')  
A dictionary mapping percent to data point\.

#### Exceptions

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
Thrown if the series contains duplicate percent values\.

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.TryGetPointByPercent(int,JordanRobot.MotorDefinitions.Model.DataPoint)'></a>

## CurveSeries\.TryGetPointByPercent\(int, DataPoint\) Method

Attempts to get the data point for a given percent\.

```csharp
public bool TryGetPointByPercent(int percent, out JordanRobot.MotorDefinitions.Model.DataPoint? point);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.TryGetPointByPercent(int,JordanRobot.MotorDefinitions.Model.DataPoint).percent'></a>

`percent` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The percent \(non\-negative\)\.

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.TryGetPointByPercent(int,JordanRobot.MotorDefinitions.Model.DataPoint).point'></a>

`point` [DataPoint](JordanRobot.MotorDefinitions.Model.DataPoint.md 'JordanRobot\.MotorDefinitions\.Model\.DataPoint')

When this method returns, contains the matching point if found; otherwise null\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if found; otherwise false\.

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.ValidateDataIntegrity()'></a>

## CurveSeries\.ValidateDataIntegrity\(\) Method

Validates that the series has a supported shape\.
A valid series has 0\.\.101 points, non\-negative percent values, and a strictly increasing percent axis\.

```csharp
public bool ValidateDataIntegrity();
```

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the series has valid data structure; otherwise false\.
### Events

<a name='JordanRobot.MotorDefinitions.Model.CurveSeries.PropertyChanged'></a>

## CurveSeries\.PropertyChanged Event

Occurs when a property value changes\.

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
```

Implements [PropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged 'System\.ComponentModel\.INotifyPropertyChanged\.PropertyChanged')

#### Event Type
[System\.ComponentModel\.PropertyChangedEventHandler](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventhandler 'System\.ComponentModel\.PropertyChangedEventHandler')