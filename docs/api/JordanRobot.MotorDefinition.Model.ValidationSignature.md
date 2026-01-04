#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Model](JordanRobot.MotorDefinition.Model.md 'JordanRobot\.MotorDefinition\.Model')

## ValidationSignature Class

Represents a validation signature for data integrity verification\.

```csharp
public class ValidationSignature
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; ValidationSignature

### Remarks
Contains a checksum, timestamp, and user information to verify that
data has not been tampered with since it was signed\.
### Constructors

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.ValidationSignature()'></a>

## ValidationSignature\(\) Constructor

Creates a new ValidationSignature with default values\.

```csharp
public ValidationSignature();
```

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.ValidationSignature(string,string)'></a>

## ValidationSignature\(string, string\) Constructor

Creates a new ValidationSignature with the specified parameters\.

```csharp
public ValidationSignature(string checksum, string verifiedBy);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.ValidationSignature(string,string).checksum'></a>

`checksum` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The cryptographic hash of the signed data\.

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.ValidationSignature(string,string).verifiedBy'></a>

`verifiedBy` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier of the person who verified the data\.

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.ValidationSignature(string,string,System.DateTime)'></a>

## ValidationSignature\(string, string, DateTime\) Constructor

Creates a new ValidationSignature with the specified parameters and timestamp\.

```csharp
public ValidationSignature(string checksum, string verifiedBy, System.DateTime timestamp);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.ValidationSignature(string,string,System.DateTime).checksum'></a>

`checksum` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The cryptographic hash of the signed data\.

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.ValidationSignature(string,string,System.DateTime).verifiedBy'></a>

`verifiedBy` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier of the person who verified the data\.

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.ValidationSignature(string,string,System.DateTime).timestamp'></a>

`timestamp` [System\.DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime 'System\.DateTime')

The timestamp when the data was signed\.
### Properties

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.Algorithm'></a>

## ValidationSignature\.Algorithm Property

Gets or sets the hashing algorithm used to compute the checksum\.

```csharp
public string Algorithm { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

### Remarks
Default is "SHA256"\. This allows for future algorithm upgrades\.

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.Checksum'></a>

## ValidationSignature\.Checksum Property

Gets or sets the cryptographic hash of the signed data\.

```csharp
public string Checksum { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

### Remarks
The checksum is computed using the algorithm specified in [Algorithm](JordanRobot.MotorDefinition.Model.ValidationSignature.md#JordanRobot.MotorDefinition.Model.ValidationSignature.Algorithm 'JordanRobot\.MotorDefinition\.Model\.ValidationSignature\.Algorithm')\.
For SHA256, this will be a 64\-character hexadecimal string\.

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.Timestamp'></a>

## ValidationSignature\.Timestamp Property

Gets or sets the timestamp when the data was signed\.

```csharp
public System.DateTime Timestamp { get; set; }
```

#### Property Value
[System\.DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime 'System\.DateTime')

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.VerifiedBy'></a>

## ValidationSignature\.VerifiedBy Property

Gets or sets the identifier of the person who verified the data\.

```csharp
public string VerifiedBy { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

### Remarks
Typically an email address or username\.
### Methods

<a name='JordanRobot.MotorDefinition.Model.ValidationSignature.IsValid()'></a>

## ValidationSignature\.IsValid\(\) Method

Determines whether this signature has valid content\.

```csharp
public bool IsValid();
```

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the signature contains a checksum and verifier; otherwise false\.