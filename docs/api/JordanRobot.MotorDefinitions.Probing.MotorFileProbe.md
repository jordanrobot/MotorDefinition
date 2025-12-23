#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinitions\.Probing](JordanRobot.MotorDefinitions.Probing.md 'JordanRobot\.MotorDefinitions\.Probing')

## MotorFileProbe Class

Lightweight shape probe for motor definition files to avoid full deserialization\.

```csharp
internal static class MotorFileProbe
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; MotorFileProbe
### Methods

<a name='JordanRobot.MotorDefinitions.Probing.MotorFileProbe.IsLikelyMotorDefinition(System.Text.Json.JsonDocument)'></a>

## MotorFileProbe\.IsLikelyMotorDefinition\(JsonDocument\) Method

Determines whether a [System\.Text\.Json\.JsonDocument](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument 'System\.Text\.Json\.JsonDocument') resembles a motor definition file in the series table/map format\.

```csharp
public static bool IsLikelyMotorDefinition(System.Text.Json.JsonDocument document);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.Probing.MotorFileProbe.IsLikelyMotorDefinition(System.Text.Json.JsonDocument).document'></a>

`document` [System\.Text\.Json\.JsonDocument](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument 'System\.Text\.Json\.JsonDocument')

The parsed JSON document\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the document matches the expected shape; otherwise false\.