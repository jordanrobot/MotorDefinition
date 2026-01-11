#### [JordanRobot\.MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Model](JordanRobot.MotorDefinition.Model.md 'JordanRobot\.MotorDefinition\.Model')

## MotorMetadata Class

Contains metadata about the motor definition file\.

```csharp
public class MotorMetadata
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; MotorMetadata
### Properties

<a name='JordanRobot.MotorDefinition.Model.MotorMetadata.Created'></a>

## MotorMetadata\.Created Property

Gets or sets the date and time when the motor definition was created\.

```csharp
public System.DateTime Created { get; set; }
```

#### Property Value
[System\.DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime 'System\.DateTime')

<a name='JordanRobot.MotorDefinition.Model.MotorMetadata.Modified'></a>

## MotorMetadata\.Modified Property

Gets or sets the date and time when the motor definition was last modified\.

```csharp
public System.DateTime Modified { get; set; }
```

#### Property Value
[System\.DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime 'System\.DateTime')

<a name='JordanRobot.MotorDefinition.Model.MotorMetadata.Notes'></a>

## MotorMetadata\.Notes Property

Gets or sets notes about the motor definition\.

```csharp
public string Notes { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

### Remarks
This can include test conditions or other free\-form information\.
### Methods

<a name='JordanRobot.MotorDefinition.Model.MotorMetadata.UpdateModified()'></a>

## MotorMetadata\.UpdateModified\(\) Method

Updates the modified timestamp to the current UTC time\.

```csharp
public void UpdateModified();
```