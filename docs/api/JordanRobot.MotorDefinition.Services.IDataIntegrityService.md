#### [JordanRobot\.MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Services](JordanRobot.MotorDefinition.Services.md 'JordanRobot\.MotorDefinition\.Services')

## IDataIntegrityService Interface

Provides services for data integrity verification using cryptographic checksums\.

```csharp
public interface IDataIntegrityService
```

Derived  
&#8627; [DataIntegrityService](JordanRobot.MotorDefinition.Services.DataIntegrityService.md 'JordanRobot\.MotorDefinition\.Services\.DataIntegrityService')
### Methods

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.ComputeCurveChecksum(JordanRobot.MotorDefinition.Model.Curve)'></a>

## IDataIntegrityService\.ComputeCurveChecksum\(Curve\) Method

Computes a checksum for curve data without creating a signature\.

```csharp
string ComputeCurveChecksum(JordanRobot.MotorDefinition.Model.Curve curve);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.ComputeCurveChecksum(JordanRobot.MotorDefinition.Model.Curve).curve'></a>

`curve` [Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve')

The curve whose data should be hashed\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
A hexadecimal string representing the SHA\-256 hash\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when curve is null\.

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.ComputeDriveChecksum(JordanRobot.MotorDefinition.Model.Drive)'></a>

## IDataIntegrityService\.ComputeDriveChecksum\(Drive\) Method

Computes a checksum for drive properties without creating a signature\.

```csharp
string ComputeDriveChecksum(JordanRobot.MotorDefinition.Model.Drive drive);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.ComputeDriveChecksum(JordanRobot.MotorDefinition.Model.Drive).drive'></a>

`drive` [Drive](JordanRobot.MotorDefinition.Model.Drive.md 'JordanRobot\.MotorDefinition\.Model\.Drive')

The drive whose properties should be hashed\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
A hexadecimal string representing the SHA\-256 hash\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when drive is null\.

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.ComputeMotorChecksum(JordanRobot.MotorDefinition.Model.ServoMotor)'></a>

## IDataIntegrityService\.ComputeMotorChecksum\(ServoMotor\) Method

Computes a checksum for motor properties without creating a signature\.

```csharp
string ComputeMotorChecksum(JordanRobot.MotorDefinition.Model.ServoMotor motor);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.ComputeMotorChecksum(JordanRobot.MotorDefinition.Model.ServoMotor).motor'></a>

`motor` [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')

The motor whose properties should be hashed\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
A hexadecimal string representing the SHA\-256 hash\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when motor is null\.

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignCurve(JordanRobot.MotorDefinition.Model.Curve,string)'></a>

## IDataIntegrityService\.SignCurve\(Curve, string\) Method

Computes a validation signature for a curve\.

```csharp
JordanRobot.MotorDefinition.Model.ValidationSignature SignCurve(JordanRobot.MotorDefinition.Model.Curve curve, string verifiedBy);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignCurve(JordanRobot.MotorDefinition.Model.Curve,string).curve'></a>

`curve` [Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve')

The curve whose data should be signed\.

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignCurve(JordanRobot.MotorDefinition.Model.Curve,string).verifiedBy'></a>

`verifiedBy` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier of the person verifying the data \(typically email or username\)\.

#### Returns
[ValidationSignature](JordanRobot.MotorDefinition.Model.ValidationSignature.md 'JordanRobot\.MotorDefinition\.Model\.ValidationSignature')  
A validation signature containing the checksum and metadata\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when curve is null\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
Thrown when verifiedBy is null or whitespace\.

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignDrive(JordanRobot.MotorDefinition.Model.Drive,string)'></a>

## IDataIntegrityService\.SignDrive\(Drive, string\) Method

Computes a validation signature for a drive configuration\.

```csharp
JordanRobot.MotorDefinition.Model.ValidationSignature SignDrive(JordanRobot.MotorDefinition.Model.Drive drive, string verifiedBy);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignDrive(JordanRobot.MotorDefinition.Model.Drive,string).drive'></a>

`drive` [Drive](JordanRobot.MotorDefinition.Model.Drive.md 'JordanRobot\.MotorDefinition\.Model\.Drive')

The drive whose properties should be signed\.

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignDrive(JordanRobot.MotorDefinition.Model.Drive,string).verifiedBy'></a>

`verifiedBy` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier of the person verifying the data \(typically email or username\)\.

#### Returns
[ValidationSignature](JordanRobot.MotorDefinition.Model.ValidationSignature.md 'JordanRobot\.MotorDefinition\.Model\.ValidationSignature')  
A validation signature containing the checksum and metadata\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when drive is null\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
Thrown when verifiedBy is null or whitespace\.

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor,string)'></a>

## IDataIntegrityService\.SignMotorProperties\(ServoMotor, string\) Method

Computes a validation signature for motor properties\.

```csharp
JordanRobot.MotorDefinition.Model.ValidationSignature SignMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor motor, string verifiedBy);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor,string).motor'></a>

`motor` [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')

The motor whose properties should be signed\.

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.SignMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor,string).verifiedBy'></a>

`verifiedBy` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier of the person verifying the data \(typically email or username\)\.

#### Returns
[ValidationSignature](JordanRobot.MotorDefinition.Model.ValidationSignature.md 'JordanRobot\.MotorDefinition\.Model\.ValidationSignature')  
A validation signature containing the checksum and metadata\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when motor is null\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
Thrown when verifiedBy is null or whitespace\.

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.VerifyCurve(JordanRobot.MotorDefinition.Model.Curve)'></a>

## IDataIntegrityService\.VerifyCurve\(Curve\) Method

Verifies that curve data has not been tampered with since signing\.

```csharp
bool VerifyCurve(JordanRobot.MotorDefinition.Model.Curve curve);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.VerifyCurve(JordanRobot.MotorDefinition.Model.Curve).curve'></a>

`curve` [Curve](JordanRobot.MotorDefinition.Model.Curve.md 'JordanRobot\.MotorDefinition\.Model\.Curve')

The curve whose data should be verified\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the signature is valid and matches the current data; false otherwise\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when curve is null\.

### Remarks
Returns false if the curve has no signature\.

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.VerifyDrive(JordanRobot.MotorDefinition.Model.Drive)'></a>

## IDataIntegrityService\.VerifyDrive\(Drive\) Method

Verifies that drive properties have not been tampered with since signing\.

```csharp
bool VerifyDrive(JordanRobot.MotorDefinition.Model.Drive drive);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.VerifyDrive(JordanRobot.MotorDefinition.Model.Drive).drive'></a>

`drive` [Drive](JordanRobot.MotorDefinition.Model.Drive.md 'JordanRobot\.MotorDefinition\.Model\.Drive')

The drive whose properties should be verified\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the signature is valid and matches the current data; false otherwise\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when drive is null\.

### Remarks
Returns false if the drive has no signature\.

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.VerifyMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor)'></a>

## IDataIntegrityService\.VerifyMotorProperties\(ServoMotor\) Method

Verifies that motor properties have not been tampered with since signing\.

```csharp
bool VerifyMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor motor);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.IDataIntegrityService.VerifyMotorProperties(JordanRobot.MotorDefinition.Model.ServoMotor).motor'></a>

`motor` [ServoMotor](JordanRobot.MotorDefinition.Model.ServoMotor.md 'JordanRobot\.MotorDefinition\.Model\.ServoMotor')

The motor whose properties should be verified\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the signature is valid and matches the current data; false otherwise\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
Thrown when motor is null\.

### Remarks
Returns false if the motor has no signature\.