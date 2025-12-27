#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Model](JordanRobot.MotorDefinition.Model.md 'JordanRobot\.MotorDefinition\.Model')

## UnitSettings Class

Specifies the units used for various motor properties\.

```csharp
public class UnitSettings : System.ComponentModel.INotifyPropertyChanged
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; UnitSettings

Implements [System\.ComponentModel\.INotifyPropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged 'System\.ComponentModel\.INotifyPropertyChanged')
### Properties

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.Backlash'></a>

## UnitSettings\.Backlash Property

Backlash unit: "arcmin" or "arcsec"\.

```csharp
public string Backlash { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.Current'></a>

## UnitSettings\.Current Property

Current unit: "A" or "mA"\.

```csharp
public string Current { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.Inertia'></a>

## UnitSettings\.Inertia Property

Inertia unit: "kg\-m^2" or "g\-cm^2"\.

```csharp
public string Inertia { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.Percentage'></a>

## UnitSettings\.Percentage Property

Percentage unit label\.

```csharp
public string Percentage { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.Power'></a>

## UnitSettings\.Power Property

Power unit: "kW", "W", or "hp"\.

```csharp
public string Power { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.ResponseTime'></a>

## UnitSettings\.ResponseTime Property

Response time unit for brake measurements\.

```csharp
public string ResponseTime { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.Speed'></a>

## UnitSettings\.Speed Property

Speed unit: "rpm"\.

```csharp
public string Speed { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedBacklashUnits'></a>

## UnitSettings\.SupportedBacklashUnits Property

Gets the supported backlash units\.

```csharp
public static string[] SupportedBacklashUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedCurrentUnits'></a>

## UnitSettings\.SupportedCurrentUnits Property

Gets the supported current units\.

```csharp
public static string[] SupportedCurrentUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedInertiaUnits'></a>

## UnitSettings\.SupportedInertiaUnits Property

Gets the supported inertia units\.

```csharp
public static string[] SupportedInertiaUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedPercentageUnits'></a>

## UnitSettings\.SupportedPercentageUnits Property

Gets the supported percentage units\.

```csharp
public static string[] SupportedPercentageUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedPowerUnits'></a>

## UnitSettings\.SupportedPowerUnits Property

Gets the supported power units\.

```csharp
public static string[] SupportedPowerUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedResponseTimeUnits'></a>

## UnitSettings\.SupportedResponseTimeUnits Property

Gets the supported response time units\.

```csharp
public static string[] SupportedResponseTimeUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedSpeedUnits'></a>

## UnitSettings\.SupportedSpeedUnits Property

Gets the supported speed units\.

```csharp
public static string[] SupportedSpeedUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedTemperatureUnits'></a>

## UnitSettings\.SupportedTemperatureUnits Property

Gets the supported temperature units\.

```csharp
public static string[] SupportedTemperatureUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedTorqueConstantUnits'></a>

## UnitSettings\.SupportedTorqueConstantUnits Property

Gets the supported torque constant units\.

```csharp
public static string[] SupportedTorqueConstantUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedTorqueUnits'></a>

## UnitSettings\.SupportedTorqueUnits Property

Gets the supported torque units\.

```csharp
public static string[] SupportedTorqueUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedVoltageUnits'></a>

## UnitSettings\.SupportedVoltageUnits Property

Gets the supported voltage units\.

```csharp
public static string[] SupportedVoltageUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SupportedWeightUnits'></a>

## UnitSettings\.SupportedWeightUnits Property

Gets the supported weight units\.

```csharp
public static string[] SupportedWeightUnits { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.Temperature'></a>

## UnitSettings\.Temperature Property

Temperature unit label\.

```csharp
public string Temperature { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.Torque'></a>

## UnitSettings\.Torque Property

Torque unit: "Nm", "lbf\-ft", "lbf\-in", or "oz\-in"\.

```csharp
public string Torque { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.TorqueConstant'></a>

## UnitSettings\.TorqueConstant Property

Torque constant unit: "Nm/A"\.

```csharp
public string TorqueConstant { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.Voltage'></a>

## UnitSettings\.Voltage Property

Value unit: "V" or "kV"\.

```csharp
public string Voltage { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.Weight'></a>

## UnitSettings\.Weight Property

Weight unit: "kg", "g", "lbs", or "oz"\.

```csharp
public string Weight { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')
### Methods

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.OnPropertyChanged(string)'></a>

## UnitSettings\.OnPropertyChanged\(string\) Method

Raises the PropertyChanged event\.

```csharp
protected virtual void OnPropertyChanged(string? propertyName=null);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.OnPropertyChanged(string).propertyName'></a>

`propertyName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SetProperty_T_(T,T,string)'></a>

## UnitSettings\.SetProperty\<T\>\(T, T, string\) Method

Sets the property value and raises PropertyChanged if the value changed\.

```csharp
protected bool SetProperty<T>(ref T field, T value, string? propertyName=null);
```
#### Type parameters

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SetProperty_T_(T,T,string).T'></a>

`T`
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SetProperty_T_(T,T,string).field'></a>

`field` [T](JordanRobot.MotorDefinition.Model.UnitSettings.md#JordanRobot.MotorDefinition.Model.UnitSettings.SetProperty_T_(T,T,string).T 'JordanRobot\.MotorDefinition\.Model\.UnitSettings\.SetProperty\<T\>\(T, T, string\)\.T')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SetProperty_T_(T,T,string).value'></a>

`value` [T](JordanRobot.MotorDefinition.Model.UnitSettings.md#JordanRobot.MotorDefinition.Model.UnitSettings.SetProperty_T_(T,T,string).T 'JordanRobot\.MotorDefinition\.Model\.UnitSettings\.SetProperty\<T\>\(T, T, string\)\.T')

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.SetProperty_T_(T,T,string).propertyName'></a>

`propertyName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')
### Events

<a name='JordanRobot.MotorDefinition.Model.UnitSettings.PropertyChanged'></a>

## UnitSettings\.PropertyChanged Event

Occurs when a property value changes\.

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
```

Implements [PropertyChanged](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged 'System\.ComponentModel\.INotifyPropertyChanged\.PropertyChanged')

#### Event Type
[System\.ComponentModel\.PropertyChangedEventHandler](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.propertychangedeventhandler 'System\.ComponentModel\.PropertyChangedEventHandler')