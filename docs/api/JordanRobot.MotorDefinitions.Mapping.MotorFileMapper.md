#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinitions\.Mapping](JordanRobot.MotorDefinitions.Mapping.md 'JordanRobot\.MotorDefinitions\.Mapping')

## MotorFileMapper Class

Converts between persisted motor definition DTOs and runtime models\.

```csharp
internal static class MotorFileMapper
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; MotorFileMapper
### Methods

<a name='JordanRobot.MotorDefinitions.Mapping.MotorFileMapper.ToFileDto(CurveEditor.Models.MotorDefinition)'></a>

## MotorFileMapper\.ToFileDto\(MotorDefinition\) Method

Maps a runtime [MotorDefinition](CurveEditor.Models.MotorDefinition.md 'CurveEditor\.Models\.MotorDefinition') into a persistence DTO\.

```csharp
public static JordanRobot.MotorDefinitions.Dtos.MotorDefinitionFileDto ToFileDto(CurveEditor.Models.MotorDefinition motor);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Mapping.MotorFileMapper.ToFileDto(CurveEditor.Models.MotorDefinition).motor'></a>

`motor` [MotorDefinition](CurveEditor.Models.MotorDefinition.md 'CurveEditor\.Models\.MotorDefinition')

The runtime motor definition\.

#### Returns
[MotorDefinitionFileDto](JordanRobot.MotorDefinitions.Dtos.MotorDefinitionFileDto.md 'JordanRobot\.MotorDefinitions\.Dtos\.MotorDefinitionFileDto')  
A DTO ready for serialization\.

<a name='JordanRobot.MotorDefinitions.Mapping.MotorFileMapper.ToRuntimeModel(JordanRobot.MotorDefinitions.Dtos.MotorDefinitionFileDto)'></a>

## MotorFileMapper\.ToRuntimeModel\(MotorDefinitionFileDto\) Method

Maps a persistence DTO into a runtime [MotorDefinition](CurveEditor.Models.MotorDefinition.md 'CurveEditor\.Models\.MotorDefinition')\.

```csharp
public static CurveEditor.Models.MotorDefinition ToRuntimeModel(JordanRobot.MotorDefinitions.Dtos.MotorDefinitionFileDto dto);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Mapping.MotorFileMapper.ToRuntimeModel(JordanRobot.MotorDefinitions.Dtos.MotorDefinitionFileDto).dto'></a>

`dto` [MotorDefinitionFileDto](JordanRobot.MotorDefinitions.Dtos.MotorDefinitionFileDto.md 'JordanRobot\.MotorDefinitions\.Dtos\.MotorDefinitionFileDto')

The deserialized DTO\.

#### Returns
[MotorDefinition](CurveEditor.Models.MotorDefinition.md 'CurveEditor\.Models\.MotorDefinition')  
A runtime motor definition model\.