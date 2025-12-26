#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinitions\.Model](JordanRobot.MotorDefinitions.Model.md 'JordanRobot\.MotorDefinitions\.Model')

## MotorMetadata Class

Contains metadata about the motor definition file\.

```csharp
public class MotorMetadata
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; MotorMetadata
### Properties

<a name='JordanRobot.MotorDefinitions.Model.MotorMetadata.Created'></a>

## MotorMetadata\.Created Property

The date and time when the motor definition was created\.

```csharp
public System.DateTime Created { get; set; }
```

#### Property Value
[System\.DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime 'System\.DateTime')

<a name='JordanRobot.MotorDefinitions.Model.MotorMetadata.Modified'></a>

## MotorMetadata\.Modified Property

The date and time when the motor definition was last modified\.

```csharp
public System.DateTime Modified { get; set; }
```

#### Property Value
[System\.DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime 'System\.DateTime')

<a name='JordanRobot.MotorDefinitions.Model.MotorMetadata.Notes'></a>

## MotorMetadata\.Notes Property

Optional notes about the motor definition \(e\.g\., test conditions\)\.

```csharp
public string Notes { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')
### Methods

<a name='JordanRobot.MotorDefinitions.Model.MotorMetadata.UpdateModified()'></a>

## MotorMetadata\.UpdateModified\(\) Method

Updates the modified timestamp to the current UTC time\.

```csharp
public void UpdateModified();
```