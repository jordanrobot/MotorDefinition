#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinitions\.Persistence\.Mapping](JordanRobot.MotorDefinitions.Persistence.Mapping.md 'JordanRobot\.MotorDefinitions\.Persistence\.Mapping')

## MotorFileMapper Class

Converts between persisted motor definition DTOs and runtime models\.

```csharp
internal static class MotorFileMapper
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; MotorFileMapper
### Methods

<a name='JordanRobot.MotorDefinitions.Persistence.Mapping.MotorFileMapper.ToFileDto(JordanRobot.MotorDefinitions.Model.MotorDefinition)'></a>

## MotorFileMapper\.ToFileDto\(MotorDefinition\) Method

Maps a runtime [MotorDefinition](JordanRobot.MotorDefinitions.Model.MotorDefinition.md 'JordanRobot\.MotorDefinitions\.Model\.MotorDefinition') into a persistence DTO\.

```csharp
public static JordanRobot.MotorDefinitions.Persistence.Dtos.MotorDefinitionFileDto ToFileDto(JordanRobot.MotorDefinitions.Model.MotorDefinition motor);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Persistence.Mapping.MotorFileMapper.ToFileDto(JordanRobot.MotorDefinitions.Model.MotorDefinition).motor'></a>

`motor` [MotorDefinition](JordanRobot.MotorDefinitions.Model.MotorDefinition.md 'JordanRobot\.MotorDefinitions\.Model\.MotorDefinition')

The runtime motor definition\.

#### Returns
[MotorDefinitionFileDto](JordanRobot.MotorDefinitions.Persistence.Dtos.MotorDefinitionFileDto.md 'JordanRobot\.MotorDefinitions\.Persistence\.Dtos\.MotorDefinitionFileDto')  
A DTO ready for serialization\.

<a name='JordanRobot.MotorDefinitions.Persistence.Mapping.MotorFileMapper.ToRuntimeModel(JordanRobot.MotorDefinitions.Persistence.Dtos.MotorDefinitionFileDto)'></a>

## MotorFileMapper\.ToRuntimeModel\(MotorDefinitionFileDto\) Method

Maps a persistence DTO into a runtime [MotorDefinition](JordanRobot.MotorDefinitions.Model.MotorDefinition.md 'JordanRobot\.MotorDefinitions\.Model\.MotorDefinition')\.

```csharp
public static JordanRobot.MotorDefinitions.Model.MotorDefinition ToRuntimeModel(JordanRobot.MotorDefinitions.Persistence.Dtos.MotorDefinitionFileDto dto);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Persistence.Mapping.MotorFileMapper.ToRuntimeModel(JordanRobot.MotorDefinitions.Persistence.Dtos.MotorDefinitionFileDto).dto'></a>

`dto` [MotorDefinitionFileDto](JordanRobot.MotorDefinitions.Persistence.Dtos.MotorDefinitionFileDto.md 'JordanRobot\.MotorDefinitions\.Persistence\.Dtos\.MotorDefinitionFileDto')

The deserialized DTO\.

#### Returns
[MotorDefinition](JordanRobot.MotorDefinitions.Model.MotorDefinition.md 'JordanRobot\.MotorDefinitions\.Model\.MotorDefinition')  
A runtime motor definition model\.