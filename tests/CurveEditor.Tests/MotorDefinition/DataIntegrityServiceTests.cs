using JordanRobot.MotorDefinition.Model;
using JordanRobot.MotorDefinition.Services;

namespace CurveEditor.Tests.MotorDefinition;

/// <summary>
/// Tests for the DataIntegrityService class.
/// </summary>
public class DataIntegrityServiceTests
{
    private readonly DataIntegrityService _service = new();

    #region Motor Signature Tests

    [Fact]
    public void SignMotorProperties_ValidMotor_ReturnsSignature()
    {
        // Arrange
        var motor = CreateTestMotor();
        var verifiedBy = "user@example.com";

        // Act
        var signature = _service.SignMotorProperties(motor, verifiedBy);

        // Assert
        Assert.NotNull(signature);
        Assert.NotEmpty(signature.Checksum);
        Assert.Equal(verifiedBy, signature.VerifiedBy);
        Assert.Equal("SHA256", signature.Algorithm);
        Assert.InRange(signature.Timestamp, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void SignMotorProperties_NullMotor_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.SignMotorProperties(null!, "user@example.com"));
    }

    [Fact]
    public void SignMotorProperties_EmptyVerifier_ThrowsArgumentException()
    {
        // Arrange
        var motor = CreateTestMotor();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.SignMotorProperties(motor, string.Empty));
    }

    [Fact]
    public void VerifyMotorProperties_ValidSignature_ReturnsTrue()
    {
        // Arrange
        var motor = CreateTestMotor();
        motor.MotorSignature = _service.SignMotorProperties(motor, "user@example.com");

        // Act
        var result = _service.VerifyMotorProperties(motor);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyMotorProperties_ModifiedData_ReturnsFalse()
    {
        // Arrange
        var motor = CreateTestMotor();
        motor.MotorSignature = _service.SignMotorProperties(motor, "user@example.com");
        motor.Power = 2000; // Tamper with data

        // Act
        var result = _service.VerifyMotorProperties(motor);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyMotorProperties_NoSignature_ReturnsFalse()
    {
        // Arrange
        var motor = CreateTestMotor();

        // Act
        var result = _service.VerifyMotorProperties(motor);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyMotorProperties_NullMotor_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.VerifyMotorProperties(null!));
    }

    [Fact]
    public void ComputeMotorChecksum_SameData_ProducesSameChecksum()
    {
        // Arrange
        var motor1 = CreateTestMotor();
        var motor2 = CreateTestMotor();

        // Act
        var checksum1 = _service.ComputeMotorChecksum(motor1);
        var checksum2 = _service.ComputeMotorChecksum(motor2);

        // Assert
        Assert.Equal(checksum1, checksum2);
    }

    [Fact]
    public void ComputeMotorChecksum_DifferentData_ProducesDifferentChecksum()
    {
        // Arrange
        var motor1 = CreateTestMotor();
        var motor2 = CreateTestMotor();
        motor2.Power = 2000;

        // Act
        var checksum1 = _service.ComputeMotorChecksum(motor1);
        var checksum2 = _service.ComputeMotorChecksum(motor2);

        // Assert
        Assert.NotEqual(checksum1, checksum2);
    }

    [Fact]
    public void ComputeMotorChecksum_IgnoresDrivesAndMetadata()
    {
        // Arrange
        var motor1 = CreateTestMotor();
        var motor2 = CreateTestMotor();
        
        // Add different drives and metadata
        motor1.Drives.Add(new Drive("Test Drive 1"));
        motor2.Drives.Add(new Drive("Test Drive 2"));
        motor1.Metadata.Notes = "Note 1";
        motor2.Metadata.Notes = "Note 2";

        // Act
        var checksum1 = _service.ComputeMotorChecksum(motor1);
        var checksum2 = _service.ComputeMotorChecksum(motor2);

        // Assert - Should be equal because motor properties are the same
        Assert.Equal(checksum1, checksum2);
    }

    #endregion

    #region Drive Signature Tests

    [Fact]
    public void SignDrive_ValidDrive_ReturnsSignature()
    {
        // Arrange
        var drive = CreateTestDrive();
        var verifiedBy = "user@example.com";

        // Act
        var signature = _service.SignDrive(drive, verifiedBy);

        // Assert
        Assert.NotNull(signature);
        Assert.NotEmpty(signature.Checksum);
        Assert.Equal(verifiedBy, signature.VerifiedBy);
        Assert.Equal("SHA256", signature.Algorithm);
    }

    [Fact]
    public void SignDrive_NullDrive_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.SignDrive(null!, "user@example.com"));
    }

    [Fact]
    public void VerifyDrive_ValidSignature_ReturnsTrue()
    {
        // Arrange
        var drive = CreateTestDrive();
        drive.DriveSignature = _service.SignDrive(drive, "user@example.com");

        // Act
        var result = _service.VerifyDrive(drive);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyDrive_ModifiedData_ReturnsFalse()
    {
        // Arrange
        var drive = CreateTestDrive();
        drive.DriveSignature = _service.SignDrive(drive, "user@example.com");
        drive.Manufacturer = "Different Manufacturer"; // Tamper with data

        // Act
        var result = _service.VerifyDrive(drive);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyDrive_ModifiedVoltage_ReturnsFalse()
    {
        // Arrange
        var drive = CreateTestDrive();
        drive.DriveSignature = _service.SignDrive(drive, "user@example.com");
        drive.Voltages[0].Power = 2000; // Tamper with voltage data

        // Act
        var result = _service.VerifyDrive(drive);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyDrive_NoSignature_ReturnsFalse()
    {
        // Arrange
        var drive = CreateTestDrive();

        // Act
        var result = _service.VerifyDrive(drive);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ComputeDriveChecksum_SameData_ProducesSameChecksum()
    {
        // Arrange
        var drive1 = CreateTestDrive();
        var drive2 = CreateTestDrive();

        // Act
        var checksum1 = _service.ComputeDriveChecksum(drive1);
        var checksum2 = _service.ComputeDriveChecksum(drive2);

        // Assert
        Assert.Equal(checksum1, checksum2);
    }

    [Fact]
    public void ComputeDriveChecksum_DifferentData_ProducesDifferentChecksum()
    {
        // Arrange
        var drive1 = CreateTestDrive();
        var drive2 = CreateTestDrive();
        drive2.PartNumber = "Different Part";

        // Act
        var checksum1 = _service.ComputeDriveChecksum(drive1);
        var checksum2 = _service.ComputeDriveChecksum(drive2);

        // Assert
        Assert.NotEqual(checksum1, checksum2);
    }

    #endregion

    #region Curve Signature Tests

    [Fact]
    public void SignCurve_ValidCurve_ReturnsSignature()
    {
        // Arrange
        var curve = CreateTestCurve();
        var verifiedBy = "user@example.com";

        // Act
        var signature = _service.SignCurve(curve, verifiedBy);

        // Assert
        Assert.NotNull(signature);
        Assert.NotEmpty(signature.Checksum);
        Assert.Equal(verifiedBy, signature.VerifiedBy);
        Assert.Equal("SHA256", signature.Algorithm);
    }

    [Fact]
    public void SignCurve_NullCurve_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.SignCurve(null!, "user@example.com"));
    }

    [Fact]
    public void VerifyCurve_ValidSignature_ReturnsTrue()
    {
        // Arrange
        var curve = CreateTestCurve();
        curve.CurveSignature = _service.SignCurve(curve, "user@example.com");

        // Act
        var result = _service.VerifyCurve(curve);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyCurve_ModifiedData_ReturnsFalse()
    {
        // Arrange
        var curve = CreateTestCurve();
        curve.CurveSignature = _service.SignCurve(curve, "user@example.com");
        curve.Data[0].Torque = 999; // Tamper with data

        // Act
        var result = _service.VerifyCurve(curve);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyCurve_ModifiedName_ReturnsFalse()
    {
        // Arrange
        var curve = CreateTestCurve();
        curve.CurveSignature = _service.SignCurve(curve, "user@example.com");
        curve.Notes = "Modified notes"; // Tamper with metadata

        // Act
        var result = _service.VerifyCurve(curve);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyCurve_NoSignature_ReturnsFalse()
    {
        // Arrange
        var curve = CreateTestCurve();

        // Act
        var result = _service.VerifyCurve(curve);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ComputeCurveChecksum_SameData_ProducesSameChecksum()
    {
        // Arrange
        var curve1 = CreateTestCurve();
        var curve2 = CreateTestCurve();

        // Act
        var checksum1 = _service.ComputeCurveChecksum(curve1);
        var checksum2 = _service.ComputeCurveChecksum(curve2);

        // Assert
        Assert.Equal(checksum1, checksum2);
    }

    [Fact]
    public void ComputeCurveChecksum_DifferentData_ProducesDifferentChecksum()
    {
        // Arrange
        var curve1 = CreateTestCurve();
        var curve2 = CreateTestCurve();
        curve2.Data[0].Torque = 999;

        // Act
        var checksum1 = _service.ComputeCurveChecksum(curve1);
        var checksum2 = _service.ComputeCurveChecksum(curve2);

        // Assert
        Assert.NotEqual(checksum1, checksum2);
    }

    #endregion

    #region Helper Methods

    private static ServoMotor CreateTestMotor()
    {
        return new ServoMotor("Test Motor")
        {
            Manufacturer = "Test Manufacturer",
            PartNumber = "TEST-001",
            Power = 1500,
            MaxSpeed = 5000,
            RatedSpeed = 3000,
            RatedContinuousTorque = 45,
            RatedPeakTorque = 55,
            Weight = 8.5,
            RotorInertia = 0.0025,
            FeedbackPpr = 131072,
            HasBrake = true,
            BrakeTorque = 44,
            BrakeAmperage = 0.5,
            BrakeVoltage = 24
        };
    }

    private static Drive CreateTestDrive()
    {
        var drive = new Drive("Test Drive")
        {
            Manufacturer = "Test Manufacturer",
            PartNumber = "DRV-001"
        };

        var voltage = new Voltage(208)
        {
            Power = 1400,
            MaxSpeed = 4800,
            RatedSpeed = 2900,
            RatedContinuousTorque = 42,
            RatedPeakTorque = 52,
            ContinuousAmperage = 9.5,
            PeakAmperage = 22
        };

        drive.Voltages.Add(voltage);
        return drive;
    }

    private static Curve CreateTestCurve()
    {
        var curve = new Curve("Test Curve")
        {
            Locked = false,
            Notes = "Test curve data"
        };

        for (var i = 0; i <= 100; i++)
        {
            curve.Data.Add(new DataPoint(i, i * 50.0, 50.0 - i * 0.3));
        }

        return curve;
    }

    #endregion
}
