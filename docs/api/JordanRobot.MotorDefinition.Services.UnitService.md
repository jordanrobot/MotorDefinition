#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Services](JordanRobot.MotorDefinition.Services.md 'JordanRobot\.MotorDefinition\.Services')

## UnitService Class

Provides unit conversion and management services using the Tare library\.
Maps between the application's unit string labels and Tare's Quantity\-based unit system\.

```csharp
public class UnitService
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; UnitService
### Properties

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedBacklashUnits'></a>

## UnitService\.SupportedBacklashUnits Property

Gets the supported backlash units\.

```csharp
public static string[] SupportedBacklashUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedCurrentUnits'></a>

## UnitService\.SupportedCurrentUnits Property

Gets the supported current units\.

```csharp
public static string[] SupportedCurrentUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedInertiaUnits'></a>

## UnitService\.SupportedInertiaUnits Property

Gets the supported inertia units\.

```csharp
public static string[] SupportedInertiaUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedPercentageUnits'></a>

## UnitService\.SupportedPercentageUnits Property

Gets the supported percentage units\.

```csharp
public static string[] SupportedPercentageUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedPowerUnits'></a>

## UnitService\.SupportedPowerUnits Property

Gets the supported power units\.

```csharp
public static string[] SupportedPowerUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedResponseTimeUnits'></a>

## UnitService\.SupportedResponseTimeUnits Property

Gets the supported response time units\.

```csharp
public static string[] SupportedResponseTimeUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedSpeedUnits'></a>

## UnitService\.SupportedSpeedUnits Property

Gets the supported speed units\.

```csharp
public static string[] SupportedSpeedUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedTemperatureUnits'></a>

## UnitService\.SupportedTemperatureUnits Property

Gets the supported temperature units\.

```csharp
public static string[] SupportedTemperatureUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedTorqueConstantUnits'></a>

## UnitService\.SupportedTorqueConstantUnits Property

Gets the supported torque constant units\.

```csharp
public static string[] SupportedTorqueConstantUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedTorqueUnits'></a>

## UnitService\.SupportedTorqueUnits Property

Gets the supported torque units\.

```csharp
public static string[] SupportedTorqueUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedVoltageUnits'></a>

## UnitService\.SupportedVoltageUnits Property

Gets the supported voltage units\.

```csharp
public static string[] SupportedVoltageUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Services.UnitService.SupportedWeightUnits'></a>

## UnitService\.SupportedWeightUnits Property

Gets the supported weight \(mass\) units\.

```csharp
public static string[] SupportedWeightUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')
### Methods

<a name='JordanRobot.MotorDefinition.Services.UnitService.Convert(double,string,string)'></a>

## UnitService\.Convert\(double, string, string\) Method

Converts a value from one unit to another\.

```csharp
public double Convert(double value, string fromUnit, string toUnit);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.UnitService.Convert(double,string,string).value'></a>

`value` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The value to convert\.

<a name='JordanRobot.MotorDefinition.Services.UnitService.Convert(double,string,string).fromUnit'></a>

`fromUnit` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The source unit string \(e\.g\., "Nm", "lbf\-in"\)\.

<a name='JordanRobot.MotorDefinition.Services.UnitService.Convert(double,string,string).toUnit'></a>

`toUnit` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The target unit string \(e\.g\., "Nm", "lbf\-in"\)\.

#### Returns
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')  
The converted value\.

#### Exceptions

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
Thrown when unit conversion is not supported or units are incompatible\.

<a name='JordanRobot.MotorDefinition.Services.UnitService.ConvertWithCurrent(double,string,string)'></a>

## UnitService\.ConvertWithCurrent\(double, string, string\) Method

Handles conversions involving electrical current units\.
Tare library doesn't support electrical current units, so we handle them manually\.

```csharp
private double ConvertWithCurrent(double value, string fromUnit, string toUnit);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.UnitService.ConvertWithCurrent(double,string,string).value'></a>

`value` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Services.UnitService.ConvertWithCurrent(double,string,string).fromUnit'></a>

`fromUnit` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Services.UnitService.ConvertWithCurrent(double,string,string).toUnit'></a>

`toUnit` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

#### Returns
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Services.UnitService.ConvertWithHorsepower(double,string,string)'></a>

## UnitService\.ConvertWithHorsepower\(double, string, string\) Method

Handles conversions involving horsepower\.

```csharp
private double ConvertWithHorsepower(double value, string fromUnit, string toUnit);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.UnitService.ConvertWithHorsepower(double,string,string).value'></a>

`value` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Services.UnitService.ConvertWithHorsepower(double,string,string).fromUnit'></a>

`fromUnit` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Services.UnitService.ConvertWithHorsepower(double,string,string).toUnit'></a>

`toUnit` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

#### Returns
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

<a name='JordanRobot.MotorDefinition.Services.UnitService.Format(double,string,int)'></a>

## UnitService\.Format\(double, string, int\) Method

Formats a value with its unit for display\.

```csharp
public string Format(double value, string unit, int decimalPlaces=2);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.UnitService.Format(double,string,int).value'></a>

`value` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The numeric value\.

<a name='JordanRobot.MotorDefinition.Services.UnitService.Format(double,string,int).unit'></a>

`unit` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The unit string\.

<a name='JordanRobot.MotorDefinition.Services.UnitService.Format(double,string,int).decimalPlaces'></a>

`decimalPlaces` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The number of decimal places to display\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
A formatted string with value and unit\.

<a name='JordanRobot.MotorDefinition.Services.UnitService.IsUnitSupported(string)'></a>

## UnitService\.IsUnitSupported\(string\) Method

Validates whether a unit string is supported\.

```csharp
public bool IsUnitSupported(string unit);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.UnitService.IsUnitSupported(string).unit'></a>

`unit` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The unit string to validate\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the unit is supported; otherwise, false\.

<a name='JordanRobot.MotorDefinition.Services.UnitService.MapToTareUnit(string)'></a>

## UnitService\.MapToTareUnit\(string\) Method

Maps application unit strings to Tare unit strings\.

```csharp
private static string MapToTareUnit(string appUnit);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.UnitService.MapToTareUnit(string).appUnit'></a>

`appUnit` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The application unit string \(e\.g\., "Nm", "lbf\-in"\)\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The Tare\-compatible unit string \(e\.g\., "N\*m", "lbf\*in"\)\.

#### Exceptions

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
Thrown when the unit is not recognized\.

<a name='JordanRobot.MotorDefinition.Services.UnitService.TryConvert(double,string,string,double)'></a>

## UnitService\.TryConvert\(double, string, string, double\) Method

Tries to convert a value from one unit to another\.

```csharp
public bool TryConvert(double value, string fromUnit, string toUnit, out double result);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.UnitService.TryConvert(double,string,string,double).value'></a>

`value` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The value to convert\.

<a name='JordanRobot.MotorDefinition.Services.UnitService.TryConvert(double,string,string,double).fromUnit'></a>

`fromUnit` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The source unit string\.

<a name='JordanRobot.MotorDefinition.Services.UnitService.TryConvert(double,string,string,double).toUnit'></a>

`toUnit` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The target unit string\.

<a name='JordanRobot.MotorDefinition.Services.UnitService.TryConvert(double,string,string,double).result'></a>

`result` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

When this method returns, contains the converted value if successful; otherwise, the original value\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if conversion was successful; otherwise, false\.