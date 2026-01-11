#### [JordanRobot\.MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition](JordanRobot.MotorDefinition.md 'JordanRobot\.MotorDefinition')

## MotorFile Class

Provides entrypoints for loading and saving motor definition files\.

```csharp
public static class MotorFile
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; MotorFile
### Methods

<a name='JordanRobot.MotorDefinition.MotorFile.IsLikelyMotorDefinition(System.Text.Json.JsonDocument)'></a>

## MotorFile\.IsLikelyMotorDefinition\(JsonDocument\) Method

Determines whether a JSON document resembles a motor definition file\.

```csharp
public static bool IsLikelyMotorDefinition(System.Text.Json.JsonDocument document);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.MotorFile.IsLikelyMotorDefinition(System.Text.Json.JsonDocument).document'></a>

`document` [System\.Text\.Json\.JsonDocument](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument 'System\.Text\.Json\.JsonDocument')

The JSON document to probe\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True when the document shape matches the motor definition format\.

<a name='JordanRobot.MotorDefinition.MotorFile.Load(string)'></a>

## MotorFile\.Load\(string\) Method

Loads a motor definition from the specified path\.

```csharp
public static JordanRobot.MotorDefinition.Model.ServoMotor Load(string path);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.MotorFile.Load(string).path'></a>

`path` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The file path to read\.

#### Returns
[ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')  
The parsed motor definition\.

<a name='JordanRobot.MotorDefinition.MotorFile.LoadAsync(string,System.Threading.CancellationToken)'></a>

## MotorFile\.LoadAsync\(string, CancellationToken\) Method

Loads a motor definition from the specified path asynchronously\.

```csharp
public static System.Threading.Tasks.Task<JordanRobot.MotorDefinition.Model.ServoMotor> LoadAsync(string path, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='JordanRobot.MotorDefinition.MotorFile.LoadAsync(string,System.Threading.CancellationToken).path'></a>

`path` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The file path to read\.

<a name='JordanRobot.MotorDefinition.MotorFile.LoadAsync(string,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
The parsed motor definition\.

<a name='JordanRobot.MotorDefinition.MotorFile.Save(JordanRobot.MotorDefinition.Model.ServoMotor,string)'></a>

## MotorFile\.Save\(ServoMotor, string\) Method

Saves a motor definition to the specified path\.

```csharp
public static void Save(JordanRobot.MotorDefinition.Model.ServoMotor motor, string path);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.MotorFile.Save(JordanRobot.MotorDefinition.Model.ServoMotor,string).motor'></a>

`motor` [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')

The motor definition to persist\.

<a name='JordanRobot.MotorDefinition.MotorFile.Save(JordanRobot.MotorDefinition.Model.ServoMotor,string).path'></a>

`path` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The destination file path\.

<a name='JordanRobot.MotorDefinition.MotorFile.SaveAsync(JordanRobot.MotorDefinition.Model.ServoMotor,string,System.Threading.CancellationToken)'></a>

## MotorFile\.SaveAsync\(ServoMotor, string, CancellationToken\) Method

Saves a motor definition to the specified path asynchronously\.

```csharp
public static System.Threading.Tasks.Task SaveAsync(JordanRobot.MotorDefinition.Model.ServoMotor motor, string path, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='JordanRobot.MotorDefinition.MotorFile.SaveAsync(JordanRobot.MotorDefinition.Model.ServoMotor,string,System.Threading.CancellationToken).motor'></a>

`motor` [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')

The motor definition to persist\.

<a name='JordanRobot.MotorDefinition.MotorFile.SaveAsync(JordanRobot.MotorDefinition.Model.ServoMotor,string,System.Threading.CancellationToken).path'></a>

`path` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The destination file path\.

<a name='JordanRobot.MotorDefinition.MotorFile.SaveAsync(JordanRobot.MotorDefinition.Model.ServoMotor,string,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')