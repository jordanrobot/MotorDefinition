#### [MotorDefinition](index.md 'index')
### [CurveEditor\.Models](CurveEditor.Models.md 'CurveEditor\.Models')

## DataPoint Class

Represents a single point on a motor torque curve\.

```csharp
public class DataPoint
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; DataPoint
### Constructors

<a name='CurveEditor.Models.DataPoint.DataPoint()'></a>

## DataPoint\(\) Constructor

Creates a new DataPoint with default values\.

```csharp
public DataPoint();
```

<a name='CurveEditor.Models.DataPoint.DataPoint(int,double,double)'></a>

## DataPoint\(int, double, double\) Constructor

Creates a new DataPoint with the specified values\.

```csharp
public DataPoint(int percent, double rpm, double torque);
```
#### Parameters

<a name='CurveEditor.Models.DataPoint.DataPoint(int,double,double).percent'></a>

`percent` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

Percentage along the speed range\. Must be non\-negative\.

<a name='CurveEditor.Models.DataPoint.DataPoint(int,double,double).rpm'></a>

`rpm` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

RPM value at this point\.

<a name='CurveEditor.Models.DataPoint.DataPoint(int,double,double).torque'></a>

`torque` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

Torque value at this point\.
### Properties

<a name='CurveEditor.Models.DataPoint.DisplayRpm'></a>

## DataPoint\.DisplayRpm Property

Gets the RPM value rounded to the nearest whole number for display\.

```csharp
public int DisplayRpm { get; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='CurveEditor.Models.DataPoint.Percent'></a>

## DataPoint\.Percent Property

Percentage representing position along the motor's speed range\.
Typically 0% = 0 RPM and 100% = MaxRpm, but values above 100 may be used for overspeed ranges\.

```csharp
public int Percent { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='CurveEditor.Models.DataPoint.Rpm'></a>

## DataPoint\.Rpm Property

Rotational speed at this percentage point in revolutions per minute\.

```csharp
public double Rpm { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='CurveEditor.Models.DataPoint.Torque'></a>

## DataPoint\.Torque Property

Torque value at this speed point\.
Can be negative for regenerative braking scenarios\.

```csharp
public double Torque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')