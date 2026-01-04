# Data Integrity Validation

The MotorDefinition library includes data integrity validation features to ensure that motor definition files have not been tampered with. This is critical for engineering calculations where data accuracy is essential.

## Overview

The validation system uses SHA-256 cryptographic checksums to verify data integrity. Each validatable section (motor properties, drives, and curves) can be independently signed and verified.

## Key Features

- **Independent Validation**: Sign motor properties, drives, and curves separately
- **Tamper Detection**: Detect any modifications to signed data
- **Backward Compatible**: Files without signatures remain valid
- **Metadata Tracking**: Records timestamp and verifier information

## Quick Start

### 1. Create a Data Integrity Service

```csharp
using JordanRobot.MotorDefinition.Services;

var integrityService = new DataIntegrityService();
```

### 2. Load a Motor Definition

```csharp
using JordanRobot.MotorDefinition;

var motor = MotorFile.Load("path/to/motor.json");
```

### 3. Sign Motor Properties

```csharp
// Sign the motor-level properties
motor.MotorSignature = integrityService.SignMotorProperties(motor, "user@example.com");

// Verify the signature
bool isValid = integrityService.VerifyMotorProperties(motor);
Console.WriteLine($"Motor properties verified: {isValid}");
```

### 4. Sign Drive Configurations

```csharp
// Sign each drive independently
foreach (var drive in motor.Drives)
{
    drive.DriveSignature = integrityService.SignDrive(drive, "user@example.com");
}

// Verify a drive
bool isDriveValid = integrityService.VerifyDrive(motor.Drives[0]);
Console.WriteLine($"Drive verified: {isDriveValid}");
```

### 5. Sign Curves

```csharp
// Sign individual curves
foreach (var drive in motor.Drives)
{
    foreach (var voltage in drive.Voltages)
    {
        foreach (var curve in voltage.Curves)
        {
            curve.CurveSignature = integrityService.SignCurve(curve, "user@example.com");
        }
    }
}

// Verify a curve
var firstCurve = motor.Drives[0].Voltages[0].Curves[0];
bool isCurveValid = integrityService.VerifyCurve(firstCurve);
Console.WriteLine($"Curve verified: {isCurveValid}");
```

### 6. Save Signed Motor Definition

```csharp
// Save the motor with all signatures
MotorFile.Save(motor, "path/to/signed-motor.json");
```

## Complete Example

```csharp
using JordanRobot.MotorDefinition;
using JordanRobot.MotorDefinition.Services;

// Load motor definition
var motor = MotorFile.Load("motor.json");

// Create integrity service
var integrityService = new DataIntegrityService();
var verifiedBy = "engineer@company.com";

// Sign all validatable sections
motor.MotorSignature = integrityService.SignMotorProperties(motor, verifiedBy);

foreach (var drive in motor.Drives)
{
    drive.DriveSignature = integrityService.SignDrive(drive, verifiedBy);
    
    foreach (var voltage in drive.Voltages)
    {
        foreach (var curve in voltage.Curves)
        {
            curve.CurveSignature = integrityService.SignCurve(curve, verifiedBy);
        }
    }
}

// Save with signatures
MotorFile.Save(motor, "signed-motor.json");

// Later, verify data integrity
var loadedMotor = MotorFile.Load("signed-motor.json");

if (integrityService.VerifyMotorProperties(loadedMotor))
{
    Console.WriteLine("✓ Motor properties verified");
}
else
{
    Console.WriteLine("✗ WARNING: Motor properties have been modified or are not signed!");
}

// Check each drive
foreach (var drive in loadedMotor.Drives)
{
    if (integrityService.VerifyDrive(drive))
    {
        Console.WriteLine($"✓ Drive '{drive.Name}' verified");
    }
    else
    {
        Console.WriteLine($"✗ WARNING: Drive '{drive.Name}' has been modified or is not signed!");
    }
}
```

## Validation Signature Format

Each signature contains:

- **Checksum**: SHA-256 hash of the normalized data
- **Timestamp**: When the data was verified (UTC)
- **VerifiedBy**: Identifier of the person who verified (email/username)
- **Algorithm**: Hashing algorithm used (default: "SHA256")

Example JSON:

```json
{
  "motorSignature": {
    "checksum": "a3f5d8c2b1e4f6a7c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2",
    "timestamp": "2024-01-15T10:30:00Z",
    "verifiedBy": "engineer@company.com",
    "algorithm": "SHA256"
  }
}
```

## Detecting Tampering

If data is modified after signing, verification will fail:

```csharp
var motor = MotorFile.Load("signed-motor.json");

// This should return true
bool beforeTamper = integrityService.VerifyMotorProperties(motor);

// Modify some data
motor.Power = 2000;

// This will now return false
bool afterTamper = integrityService.VerifyMotorProperties(motor);
```

## Working with Unsigned Files

Files without signatures are still valid and can be loaded normally:

```csharp
var unsignedMotor = MotorFile.Load("unsigned-motor.json");

// Returns false (not an error, just means no signature)
bool isVerified = integrityService.VerifyMotorProperties(unsignedMotor);

// You can check if a signature exists
if (unsignedMotor.MotorSignature == null)
{
    Console.WriteLine("Motor has no signature - consider signing for data integrity");
}
```

## Computing Checksums Without Signing

You can compute checksums without creating full signatures:

```csharp
// Just get the checksum
string checksum = integrityService.ComputeMotorChecksum(motor);
Console.WriteLine($"Motor checksum: {checksum}");

// Compare checksums later
string newChecksum = integrityService.ComputeMotorChecksum(motor);
if (checksum != newChecksum)
{
    Console.WriteLine("Motor data has changed!");
}
```

## Best Practices

1. **Sign After Data Entry**: Sign sections after data has been entered and validated
2. **Verify Before Use**: Always verify signatures before using data for critical calculations
3. **Track Verifiers**: Use meaningful identifiers (email addresses) for accountability
4. **Independent Signing**: Sign motor, drives, and curves separately for granular control
5. **Re-sign After Changes**: If you need to modify data, you must re-sign it afterward

## API Reference

### IDataIntegrityService Interface

- `SignMotorProperties(motor, verifiedBy)` - Sign motor-level properties
- `VerifyMotorProperties(motor)` - Verify motor properties signature
- `SignDrive(drive, verifiedBy)` - Sign a drive configuration
- `VerifyDrive(drive)` - Verify drive signature
- `SignCurve(curve, verifiedBy)` - Sign a curve
- `VerifyCurve(curve)` - Verify curve signature
- `ComputeMotorChecksum(motor)` - Compute checksum without signing
- `ComputeDriveChecksum(drive)` - Compute drive checksum
- `ComputeCurveChecksum(curve)` - Compute curve checksum

### ValidationSignature Class

Properties:
- `Checksum` - SHA-256 hash (hex string)
- `Timestamp` - When verified (DateTime UTC)
- `VerifiedBy` - Verifier identifier (string)
- `Algorithm` - Hash algorithm (default "SHA256")

Methods:
- `IsValid()` - Returns true if checksum and verifiedBy are present

## Future Enhancements

The validation system is designed to be extensible:

- Support for digital signatures with private/public key pairs
- Integration with certificate authorities
- Audit trail logging
- Batch validation operations
- Custom hashing algorithms
