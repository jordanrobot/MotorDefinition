#### [JordanRobot\.MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Model](JordanRobot.MotorDefinition.Model.md 'JordanRobot\.MotorDefinition\.Model')

## DataPoint Class

Represents a single point on a motor torque curve\.

```csharp
public class DataPoint
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; DataPoint
### Constructors

<a name='JordanRobot.MotorDefinition.Model.DataPoint.DataPoint()'></a>

## DataPoint\(\) Constructor

Creates a new DataPoint with default values\.

```csharp
public DataPoint();
```

<a name='JordanRobot.MotorDefinition.Model.DataPoint.DataPoint(int,double,double)'></a>

## DataPoint\(int, double, double\) Constructor

Creates a new DataPoint with the specified values\.

```csharp
public DataPoint(int percent, double rpm, double torque);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.DataPoint.DataPoint(int,double,double).percent'></a>

`percent` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

Percentage along the speed range\. Must be non\-negative\.

<a name='JordanRobot.MotorDefinition.Model.DataPoint.DataPoint(int,double,double).rpm'></a>

`rpm` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

RPM value at this point\.

<a name='JordanRobot.MotorDefinition.Model.DataPoint.DataPoint(int,double,double).torque'></a>

`torque` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

Torque value at this point\.
### Properties

<a name='JordanRobot.MotorDefinition.Model.DataPoint.DisplayRpm'></a>

## DataPoint\.DisplayRpm Property

Gets the RPM value rounded to the nearest whole number for display\.

```csharp
public int DisplayRpm { get; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

<a name='JordanRobot.MotorDefinition.Model.DataPoint.Percent'></a>

## DataPoint\.Percent Property

Gets or sets the percent position along the motor's speed range\.

```csharp
public int Percent { get; set; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

### Remarks
Typically 0% corresponds to 0 RPM and 100% corresponds to max speed, but values above 100% may be used
to represent overspeed ranges\.

<a name='JordanRobot.MotorDefinition.Model.DataPoint.Rpm'></a>

## DataPoint\.Rpm Property

Gets or sets the rotational speed at this point \(RPM\)\.

```csharp
public double Rpm { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Model.DataPoint.Torque'></a>

## DataPoint\.Torque Property

Gets or sets the torque value at this point\.

```csharp
public double Torque { get; set; }
```

#### Property Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

### Remarks
Torque may be negative for regenerative braking scenarios\.