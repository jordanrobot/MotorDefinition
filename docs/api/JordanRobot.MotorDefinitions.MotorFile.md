#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinitions](JordanRobot.MotorDefinitions.md 'JordanRobot\.MotorDefinitions')

## MotorFile Class

Provides entrypoints for loading and saving motor definition files\.

```csharp
public static class MotorFile
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; MotorFile
### Methods

<a name='JordanRobot.MotorDefinitions.MotorFile.IsLikelyMotorDefinition(System.Text.Json.JsonDocument)'></a>

## MotorFile\.IsLikelyMotorDefinition\(JsonDocument\) Method

Determines whether a JSON document resembles a motor definition file\.

```csharp
public static bool IsLikelyMotorDefinition(System.Text.Json.JsonDocument document);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.MotorFile.IsLikelyMotorDefinition(System.Text.Json.JsonDocument).document'></a>

`document` [System\.Text\.Json\.JsonDocument](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument 'System\.Text\.Json\.JsonDocument')

The JSON document to probe\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True when the document shape matches the motor definition format\.

<a name='JordanRobot.MotorDefinitions.MotorFile.Load(string)'></a>

## MotorFile\.Load\(string\) Method

Loads a motor definition from the specified path\.

```csharp
public static JordanRobot.MotorDefinitions.Model.MotorDefinition Load(string path);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.MotorFile.Load(string).path'></a>

`path` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The file path to read\.

#### Returns
[MotorDefinition](JordanRobot.MotorDefinitions.Model.MotorDefinition.md 'JordanRobot\.MotorDefinitions\.Model\.MotorDefinition')  
The parsed motor definition\.

<a name='JordanRobot.MotorDefinitions.MotorFile.LoadAsync(string,System.Threading.CancellationToken)'></a>

## MotorFile\.LoadAsync\(string, CancellationToken\) Method

Loads a motor definition from the specified path asynchronously\.

```csharp
public static System.Threading.Tasks.Task<JordanRobot.MotorDefinitions.Model.MotorDefinition> LoadAsync(string path, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.MotorFile.LoadAsync(string,System.Threading.CancellationToken).path'></a>

`path` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The file path to read\.

<a name='JordanRobot.MotorDefinitions.MotorFile.LoadAsync(string,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[MotorDefinition](JordanRobot.MotorDefinitions.Model.MotorDefinition.md 'JordanRobot\.MotorDefinitions\.Model\.MotorDefinition')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
The parsed motor definition\.

<a name='JordanRobot.MotorDefinitions.MotorFile.Save(JordanRobot.MotorDefinitions.Model.MotorDefinition,string)'></a>

## MotorFile\.Save\(MotorDefinition, string\) Method

Saves a motor definition to the specified path\.

```csharp
public static void Save(JordanRobot.MotorDefinitions.Model.MotorDefinition motor, string path);
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.MotorFile.Save(JordanRobot.MotorDefinitions.Model.MotorDefinition,string).motor'></a>

`motor` [MotorDefinition](JordanRobot.MotorDefinitions.Model.MotorDefinition.md 'JordanRobot\.MotorDefinitions\.Model\.MotorDefinition')

The motor definition to persist\.

<a name='JordanRobot.MotorDefinitions.MotorFile.Save(JordanRobot.MotorDefinitions.Model.MotorDefinition,string).path'></a>

`path` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The destination file path\.

<a name='JordanRobot.MotorDefinitions.MotorFile.SaveAsync(JordanRobot.MotorDefinitions.Model.MotorDefinition,string,System.Threading.CancellationToken)'></a>

## MotorFile\.SaveAsync\(MotorDefinition, string, CancellationToken\) Method

Saves a motor definition to the specified path asynchronously\.

```csharp
public static System.Threading.Tasks.Task SaveAsync(JordanRobot.MotorDefinitions.Model.MotorDefinition motor, string path, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='JordanRobot.MotorDefinitions.MotorFile.SaveAsync(JordanRobot.MotorDefinitions.Model.MotorDefinition,string,System.Threading.CancellationToken).motor'></a>

`motor` [MotorDefinition](JordanRobot.MotorDefinitions.Model.MotorDefinition.md 'JordanRobot\.MotorDefinitions\.Model\.MotorDefinition')

The motor definition to persist\.

<a name='JordanRobot.MotorDefinitions.MotorFile.SaveAsync(JordanRobot.MotorDefinitions.Model.MotorDefinition,string,System.Threading.CancellationToken).path'></a>

`path` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The destination file path\.

<a name='JordanRobot.MotorDefinitions.MotorFile.SaveAsync(JordanRobot.MotorDefinitions.Model.MotorDefinition,string,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')