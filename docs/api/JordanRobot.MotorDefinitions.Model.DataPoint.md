#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinitions\.Model](JordanRobot.MotorDefinitions.Model.md 'JordanRobot\.MotorDefinitions\.Model')

## DataPoint Class

Represents a single point on a motor torque curve\.

```csharp
public class DataPoint
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; DataPoint
### Constructors

<a name='JordanRobot.MotorDefinitions.Model.DataPoint.DataPoint()'></a>

## DataPoint\(\) Constructor

Creates a new DataPoint with default values\.

```csharp
public DataPoint();
```

<a name='JordanRobot.MotorDefinitions.Model.DataPoint.DataPoint(int,double,double)'></a>

## DataPoint\(int, double, double\) Constructor

Creates a new DataPoint with the specified values\.

```csharp
public DataPoint(int percent, double rpm, double torque);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Model.DataPoint.DataPoint(int,double,double).percent'></a>

`percent` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

Percentage along the speed range\. Must be non\-negative\.

<a name='JordanRobot.MotorDefinitions.Model.DataPoint.DataPoint(int,double,double).rpm'></a>

`rpm` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

RPM value at this point\.

<a name='JordanRobot.MotorDefinitions.Model.DataPoint.DataPoint(int,double,double).torque'></a>

`torque` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

Torque value at this point\.
### Properties

<a name='JordanRobot.MotorDefinitions.Model.DataPoint.DisplayRpm'></a>

## DataPoint\.DisplayRpm Property

Gets the RPM value rounded to the nearest whole number for display\.

```csharp
public int DisplayRpm { get; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='JordanRobot.MotorDefinitions.Model.DataPoint.Percent'></a>

## DataPoint\.Percent Property

Percentage representing position along the motor's speed range\.
Typically 0% = 0 RPM and 100% = MaxRpm, but values above 100 may be used for overspeed ranges\.

```csharp
public int Percent { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='JordanRobot.MotorDefinitions.Model.DataPoint.Rpm'></a>

## DataPoint\.Rpm Property

Rotational speed at this percentage point in revolutions per minute\.

```csharp
public double Rpm { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinitions.Model.DataPoint.Torque'></a>

## DataPoint\.Torque Property

Torque value at this speed point\.
Can be negative for regenerative braking scenarios\.

```csharp
public double Torque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')