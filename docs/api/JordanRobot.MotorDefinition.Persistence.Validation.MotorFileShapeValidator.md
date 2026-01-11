#### [JordanRobot\.MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Persistence\.Validation](JordanRobot.MotorDefinition.Persistence.Validation.md 'JordanRobot\.MotorDefinition\.Persistence\.Validation')

## MotorFileShapeValidator Class

Provides shape validation helpers for motor definition persistence DTOs\.

```csharp
internal static class MotorFileShapeValidator
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; MotorFileShapeValidator
### Methods

<a name='JordanRobot.MotorDefinition.Persistence.Validation.MotorFileShapeValidator.ValidateRuntimeVoltage(JordanRobot.MotorDefinition.Model.Voltage)'></a>

## MotorFileShapeValidator\.ValidateRuntimeVoltage\(Voltage\) Method

Validates that a runtime [Voltage](JordanRobot.MotorDefinition.Model.Voltage.md 'JordanRobot\.MotorDefinition\.Model\.Voltage') can be serialized into the persisted series\-map format\.

```csharp
public static void ValidateRuntimeVoltage(JordanRobot.MotorDefinition.Model.Voltage voltage);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Persistence.Validation.MotorFileShapeValidator.ValidateRuntimeVoltage(JordanRobot.MotorDefinition.Model.Voltage).voltage'></a>

`voltage` [Voltage](JordanRobot.MotorDefinition.Model.Voltage.md 'JordanRobot\.MotorDefinition\.Model\.Voltage')

The runtime voltage configuration to validate\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown when the runtime model is missing curves or has inconsistent axes\.

<a name='JordanRobot.MotorDefinition.Persistence.Validation.MotorFileShapeValidator.ValidateVoltageDto(JordanRobot.MotorDefinition.Persistence.Dtos.VoltageFileDto,string)'></a>

## MotorFileShapeValidator\.ValidateVoltageDto\(VoltageFileDto, string\) Method

Validates that a deserialized [VoltageFileDto](JordanRobot.MotorDefinition.Persistence.Dtos.VoltageFileDto.md 'JordanRobot\.MotorDefinition\.Persistence\.Dtos\.VoltageFileDto') matches the expected persisted shape\.

```csharp
public static void ValidateVoltageDto(JordanRobot.MotorDefinition.Persistence.Dtos.VoltageFileDto voltage, string driveLabel);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Persistence.Validation.MotorFileShapeValidator.ValidateVoltageDto(JordanRobot.MotorDefinition.Persistence.Dtos.VoltageFileDto,string).voltage'></a>

`voltage` [VoltageFileDto](JordanRobot.MotorDefinition.Persistence.Dtos.VoltageFileDto.md 'JordanRobot\.MotorDefinition\.Persistence\.Dtos\.VoltageFileDto')

The voltage DTO to validate\.

<a name='JordanRobot.MotorDefinition.Persistence.Validation.MotorFileShapeValidator.ValidateVoltageDto(JordanRobot.MotorDefinition.Persistence.Dtos.VoltageFileDto,string).driveLabel'></a>

`driveLabel` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

A human\-readable label used for exception messages\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown when required nodes are missing or arrays are inconsistent\.