#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Services](JordanRobot.MotorDefinition.Services.md 'JordanRobot\.MotorDefinition\.Services')

## DataIntegrityService Class

Provides data integrity verification using SHA\-256 cryptographic checksums\.

```csharp
public class DataIntegrityService : JordanRobot.MotorDefinition.Services.IDataIntegrityService
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; DataIntegrityService

Implements [IDataIntegrityService](JordanRobot.MotorDefinition.Services.IDataIntegrityService.md 'JordanRobot\.MotorDefinition\.Services\.IDataIntegrityService')

### Remarks

This service uses a normalized JSON representation with specific serialization options
to compute checksums. The serialization format is fixed to ensure checksum stability:
- Compact JSON (no indentation)
- camelCase property names
- SHA-256 hash algorithm
- Lowercase hexadecimal output

IMPORTANT: Changes to the serialization format will invalidate all existing signatures.
The format is considered stable and should not be changed without a major version bump.

Checksum Coverage:
- Motor checksum: Motor-level properties only (excludes drives and metadata)
- Drive checksum: Drive properties and all voltages (excludes curve data)
- Curve checksum: Curve name, locked flag, notes, and all data points
### Methods

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.ComputeCurveChecksum(JordanRobot.MotorDefinition.Model.Curve)'></a>

## DataIntegrityService\.ComputeCurveChecksum\(Curve\) Method

Computes a checksum for curve data without creating a signature\.

```csharp
public string ComputeCurveChecksum(JordanRobot.MotorDefinition.Model.Curve curve);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.ComputeCurveChecksum(JordanRobot.MotorDefinition.Model.Curve).curve'></a>

`curve` [Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve')

The curve whose data should be hashed\.

Implements [ComputeCurveChecksum\(Curve\)](JordanRobot.MotorDefinition.Services.IDataIntegrityService.md#JordanRobot.MotorDefinition.Services.IDataIntegrityService.ComputeCurveChecksum(JordanRobot.MotorDefinition.Model.Curve) 'JordanRobot\.MotorDefinition\.Services\.IDataIntegrityService\.ComputeCurveChecksum\(JordanRobot\.MotorDefinition\.Model\.Curve\)')

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
A hexadecimal string representing the SHA\-256 hash\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when curve is null\.

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.ComputeDriveChecksum(JordanRobot.MotorDefinition.Model.Drive)'></a>

## DataIntegrityService\.ComputeDriveChecksum\(Drive\) Method

Computes a checksum for drive properties without creating a signature\.

```csharp
public string ComputeDriveChecksum(JordanRobot.MotorDefinition.Model.Drive drive);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.ComputeDriveChecksum(JordanRobot.MotorDefinition.Model.Drive).drive'></a>

`drive` [Drive](JordanRobot.MotorDefinition.Model.Drive.md 'JordanRobot\.MotorDefinition\.Model\.Drive')

The drive whose properties should be hashed\.

Implements [ComputeDriveChecksum\(Drive\)](JordanRobot.MotorDefinition.Services.IDataIntegrityService.md#JordanRobot.MotorDefinition.Services.IDataIntegrityService.ComputeDriveChecksum(JordanRobot.MotorDefinition.Model.Drive) 'JordanRobot\.MotorDefinition\.Services\.IDataIntegrityService\.ComputeDriveChecksum\(JordanRobot\.MotorDefinition\.Model\.Drive\)')

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
A hexadecimal string representing the SHA\-256 hash\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when drive is null\.

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.ComputeMotorChecksum(JordanRobot.MotorDefinition.Model.ServoMotor)'></a>

## DataIntegrityService\.ComputeMotorChecksum\(ServoMotor\) Method

Computes a checksum for motor properties without creating a signature\.

```csharp
public string ComputeMotorChecksum(JordanRobot.MotorDefinition.Model.ServoMotor motor);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.ComputeMotorChecksum(JordanRobot.MotorDefinition.Model.ServoMotor).motor'></a>

`motor` [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')

The motor whose properties should be hashed\.

Implements [ComputeMotorChecksum\(ServoMotor\)](JordanRobot.MotorDefinition.Services.IDataIntegrityService.md#JordanRobot.MotorDefinition.Services.IDataIntegrityService.ComputeMotorChecksum(JordanRobot.MotorDefinition.Model.ServoMotor) 'JordanRobot\.MotorDefinition\.Services\.IDataIntegrityService\.ComputeMotorChecksum\(JordanRobot\.MotorDefinition\.Model\.ServoMotor\)')

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
A hexadecimal string representing the SHA\-256 hash\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when motor is null\.

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.SignCurve(JordanRobot.MotorDefinition.Model.Curve,string)'></a>

## DataIntegrityService\.SignCurve\(Curve, string\) Method

Computes a validation signature for a curve\.

```csharp
public JordanRobot.MotorDefinition.Model.ValidationSignature SignCurve(JordanRobot.MotorDefinition.Model.Curve curve, string verifiedBy);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.SignCurve(JordanRobot.MotorDefinition.Model.Curve,string).curve'></a>

`curve` [Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve')

The curve whose data should be signed\.

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.SignCurve(JordanRobot.MotorDefinition.Model.Curve,string).verifiedBy'></a>

`verifiedBy` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier of the person verifying the data \(typically email or username\)\.

Implements [SignCurve\(Curve, string\)](JordanRobot.MotorDefinition.Services.IDataIntegrityService.md#JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignCurve(JordanRobot.MotorDefinition.Model.Curve,string) 'JordanRobot\.MotorDefinition\.Services\.IDataIntegrityService\.SignCurve\(JordanRobot\.MotorDefinition\.Model\.Curve, string\)')

#### Returns
[ValidationSignature](JordanRobot.MotorDefinition.Model.ValidationSignature.md 'JordanRobot\.MotorDefinition\.Model\.ValidationSignature')  
A validation signature containing the checksum and metadata\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when curve is null\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
Thrown when verifiedBy is null or whitespace\.

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.SignDrive(JordanRobot.MotorDefinition.Model.Drive,string)'></a>

## DataIntegrityService\.SignDrive\(Drive, string\) Method

Computes a validation signature for a drive configuration\.

```csharp
public JordanRobot.MotorDefinition.Model.ValidationSignature SignDrive(JordanRobot.MotorDefinition.Model.Drive drive, string verifiedBy);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.SignDrive(JordanRobot.MotorDefinition.Model.Drive,string).drive'></a>

`drive` [Drive](JordanRobot.MotorDefinition.Model.Drive.md 'JordanRobot\.MotorDefinition\.Model\.Drive')

The drive whose properties should be signed\.

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.SignDrive(JordanRobot.MotorDefinition.Model.Drive,string).verifiedBy'></a>

`verifiedBy` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier of the person verifying the data \(typically email or username\)\.

Implements [SignDrive\(Drive, string\)](JordanRobot.MotorDefinition.Services.IDataIntegrityService.md#JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignDrive(JordanRobot.MotorDefinition.Model.Drive,string) 'JordanRobot\.MotorDefinition\.Services\.IDataIntegrityService\.SignDrive\(JordanRobot\.MotorDefinition\.Model\.Drive, string\)')

#### Returns
[ValidationSignature](JordanRobot.MotorDefinition.Model.ValidationSignature.md 'JordanRobot\.MotorDefinition\.Model\.ValidationSignature')  
A validation signature containing the checksum and metadata\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when drive is null\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
Thrown when verifiedBy is null or whitespace\.

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.SignMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor,string)'></a>

## DataIntegrityService\.SignMotorProperties\(ServoMotor, string\) Method

Computes a validation signature for motor properties\.

```csharp
public JordanRobot.MotorDefinition.Model.ValidationSignature SignMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor motor, string verifiedBy);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.SignMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor,string).motor'></a>

`motor` [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')

The motor whose properties should be signed\.

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.SignMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor,string).verifiedBy'></a>

`verifiedBy` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier of the person verifying the data \(typically email or username\)\.

Implements [SignMotorProperties\(ServoMotor, string\)](JordanRobot.MotorDefinition.Services.IDataIntegrityService.md#JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor,string) 'JordanRobot\.MotorDefinition\.Services\.IDataIntegrityService\.SignMotorProperties\(JordanRobot\.MotorDefinition\.Model\.ServoMotor, string\)')

#### Returns
[ValidationSignature](JordanRobot.MotorDefinition.Model.ValidationSignature.md 'JordanRobot\.MotorDefinition\.Model\.ValidationSignature')  
A validation signature containing the checksum and metadata\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when motor is null\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
Thrown when verifiedBy is null or whitespace\.

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.VerifyCurve(JordanRobot.MotorDefinition.Model.Curve)'></a>

## DataIntegrityService\.VerifyCurve\(Curve\) Method

Verifies that curve data has not been tampered with since signing\.

```csharp
public bool VerifyCurve(JordanRobot.MotorDefinition.Model.Curve curve);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.VerifyCurve(JordanRobot.MotorDefinition.Model.Curve).curve'></a>

`curve` [Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve')

The curve whose data should be verified\.

Implements [VerifyCurve\(Curve\)](JordanRobot.MotorDefinition.Services.IDataIntegrityService.md#JordanRobot.MotorDefinition.Services.IDataIntegrityService.VerifyCurve(JordanRobot.MotorDefinition.Model.Curve) 'JordanRobot\.MotorDefinition\.Services\.IDataIntegrityService\.VerifyCurve\(JordanRobot\.MotorDefinition\.Model\.Curve\)')

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the signature is valid and matches the current data; false otherwise\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when curve is null\.

### Remarks
Returns false if the curve has no signature\.

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.VerifyDrive(JordanRobot.MotorDefinition.Model.Drive)'></a>

## DataIntegrityService\.VerifyDrive\(Drive\) Method

Verifies that drive properties have not been tampered with since signing\.

```csharp
public bool VerifyDrive(JordanRobot.MotorDefinition.Model.Drive drive);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.VerifyDrive(JordanRobot.MotorDefinition.Model.Drive).drive'></a>

`drive` [Drive](JordanRobot.MotorDefinition.Model.Drive.md 'JordanRobot\.MotorDefinition\.Model\.Drive')

The drive whose properties should be verified\.

Implements [VerifyDrive\(Drive\)](JordanRobot.MotorDefinition.Services.IDataIntegrityService.md#JordanRobot.MotorDefinition.Services.IDataIntegrityService.VerifyDrive(JordanRobot.MotorDefinition.Model.Drive) 'JordanRobot\.MotorDefinition\.Services\.IDataIntegrityService\.VerifyDrive\(JordanRobot\.MotorDefinition\.Model\.Drive\)')

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the signature is valid and matches the current data; false otherwise\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when drive is null\.

### Remarks
Returns false if the drive has no signature\.

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.VerifyMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor)'></a>

## DataIntegrityService\.VerifyMotorProperties\(ServoMotor\) Method

Verifies that motor properties have not been tampered with since signing\.

```csharp
public bool VerifyMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor motor);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.DataIntegrityService.VerifyMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor).motor'></a>

`motor` [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')

The motor whose properties should be verified\.

Implements [VerifyMotorProperties\(ServoMotor\)](JordanRobot.MotorDefinition.Services.IDataIntegrityService.md#JordanRobot.MotorDefinition.Services.IDataIntegrityService.VerifyMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor) 'JordanRobot\.MotorDefinition\.Services\.IDataIntegrityService\.VerifyMotorProperties\(JordanRobot\.MotorDefinition\.Model\.ServoMotor\)')

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the signature is valid and matches the current data; false otherwise\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when motor is null\.

### Remarks
Returns false if the motor has no signature\.