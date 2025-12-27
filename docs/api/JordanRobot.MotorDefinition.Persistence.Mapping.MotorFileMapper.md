#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Persistence\.Mapping](JordanRobot.MotorDefinition.Persistence.Mapping.md 'JordanRobot\.MotorDefinition\.Persistence\.Mapping')

## MotorFileMapper Class

Converts between persisted motor definition DTOs and runtime models\.

```csharp
internal static class MotorFileMapper
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; MotorFileMapper
### Methods

<a name='JordanRobot.MotorDefinition.Persistence.Mapping.MotorFileMapper.ToFileDto(JordanRobot.MotorDefinition.Model.ServoMotor)'></a>

## MotorFileMapper\.ToFileDto\(ServoMotor\) Method

Maps a runtime [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor') into a persistence DTO\.

```csharp
public static JordanRobot.MotorDefinition.Persistence.Dtos.MotorDefinitionFileDto ToFileDto(JordanRobot.MotorDefinition.Model.ServoMotor motor);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Persistence.Mapping.MotorFileMapper.ToFileDto(JordanRobot.MotorDefinition.Model.ServoMotor).motor'></a>

`motor` [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')

The runtime motor definition\.

#### Returns
[MotorDefinitionFileDto](JordanRobot.MotorDefinition.Persistence.Dtos.MotorDefinitionFileDto.md 'JordanRobot\.MotorDefinition\.Persistence\.Dtos\.MotorDefinitionFileDto')  
A DTO ready for serialization\.

<a name='JordanRobot.MotorDefinition.Persistence.Mapping.MotorFileMapper.ToRuntimeModel(JordanRobot.MotorDefinition.Persistence.Dtos.MotorDefinitionFileDto)'></a>

## MotorFileMapper\.ToRuntimeModel\(MotorDefinitionFileDto\) Method

Maps a persistence DTO into a runtime [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')\.

```csharp
public static JordanRobot.MotorDefinition.Model.ServoMotor ToRuntimeModel(JordanRobot.MotorDefinition.Persistence.Dtos.MotorDefinitionFileDto dto);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Persistence.Mapping.MotorFileMapper.ToRuntimeModel(JordanRobot.MotorDefinition.Persistence.Dtos.MotorDefinitionFileDto).dto'></a>

`dto` [MotorDefinitionFileDto](JordanRobot.MotorDefinition.Persistence.Dtos.MotorDefinitionFileDto.md 'JordanRobot\.MotorDefinition\.Persistence\.Dtos\.MotorDefinitionFileDto')

The deserialized DTO\.

#### Returns
[ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')  
A runtime motor definition model\.