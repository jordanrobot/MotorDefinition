using JordanRobot.MotorDefinition;
using JordanRobot.MotorDefinition.Model;
using JordanRobot.MotorDefinition.Services;
using System.IO;

namespace CurveEditor.Tests.MotorDefinition;

/// <summary>
/// Tests for round-trip persistence of validation signatures.
/// </summary>
public class ValidationPersistenceTests : IDisposable
{
    private readonly string _tempFile;
    private readonly DataIntegrityService _service = new();

    public ValidationPersistenceTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"test-motor-{Guid.NewGuid()}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
        {
            File.Delete(_tempFile);
        }
    }

    [Fact]
    public void RoundTrip_MotorWithSignatures_PreservesSignatures()
    {
        // Arrange
        var motor = CreateTestMotor();
        motor.MotorSignature = _service.SignMotorProperties(motor, "user@example.com");
        motor.Drives[0].DriveSignature = _service.SignDrive(motor.Drives[0], "user@example.com");
        motor.Drives[0].Voltages[0].Curves[0].CurveSignature = _service.SignCurve(motor.Drives[0].Voltages[0].Curves[0], "user@example.com");

        // Act - Save and reload
        MotorFile.Save(motor, _tempFile);
        var reloaded = MotorFile.Load(_tempFile);

        // Assert - Verify signatures are preserved
        Assert.NotNull(reloaded.MotorSignature);
        Assert.Equal(motor.MotorSignature.Checksum, reloaded.MotorSignature.Checksum);
        Assert.Equal(motor.MotorSignature.VerifiedBy, reloaded.MotorSignature.VerifiedBy);
        Assert.Equal(motor.MotorSignature.Algorithm, reloaded.MotorSignature.Algorithm);

        Assert.NotNull(reloaded.Drives[0].DriveSignature);
        Assert.Equal(motor.Drives[0].DriveSignature.Checksum, reloaded.Drives[0].DriveSignature.Checksum);

        Assert.NotNull(reloaded.Drives[0].Voltages[0].Curves[0].CurveSignature);
        Assert.Equal(motor.Drives[0].Voltages[0].Curves[0].CurveSignature.Checksum, 
                     reloaded.Drives[0].Voltages[0].Curves[0].CurveSignature.Checksum);
    }

    [Fact]
    public void RoundTrip_SignaturesStillValid_AfterReload()
    {
        // Arrange
        var motor = CreateTestMotor();
        motor.MotorSignature = _service.SignMotorProperties(motor, "user@example.com");
        motor.Drives[0].DriveSignature = _service.SignDrive(motor.Drives[0], "user@example.com");
        motor.Drives[0].Voltages[0].Curves[0].CurveSignature = _service.SignCurve(motor.Drives[0].Voltages[0].Curves[0], "user@example.com");

        // Act - Save and reload
        MotorFile.Save(motor, _tempFile);
        var reloaded = MotorFile.Load(_tempFile);

        // Assert - Verify all signatures are still valid
        Assert.True(_service.VerifyMotorProperties(reloaded));
        Assert.True(_service.VerifyDrive(reloaded.Drives[0]));
        Assert.True(_service.VerifyCurve(reloaded.Drives[0].Voltages[0].Curves[0]));
    }

    [Fact]
    public void RoundTrip_MotorWithoutSignatures_WorksCorrectly()
    {
        // Arrange - Create motor without any signatures
        var motor = CreateTestMotor();

        // Act - Save and reload
        MotorFile.Save(motor, _tempFile);
        var reloaded = MotorFile.Load(_tempFile);

        // Assert - Verify no signatures present
        Assert.Null(reloaded.MotorSignature);
        Assert.Null(reloaded.Drives[0].DriveSignature);
        Assert.Null(reloaded.Drives[0].Voltages[0].Curves[0].CurveSignature);

        // Verify data is intact
        Assert.Equal(motor.MotorName, reloaded.MotorName);
        Assert.Equal(motor.Power, reloaded.Power);
        Assert.Equal(motor.Drives[0].Name, reloaded.Drives[0].Name);
    }

    [Fact]
    public void RoundTrip_PartialSignatures_PreservesOnlySigned()
    {
        // Arrange - Sign only motor properties, not drives or curves
        var motor = CreateTestMotor();
        motor.MotorSignature = _service.SignMotorProperties(motor, "user@example.com");

        // Act - Save and reload
        MotorFile.Save(motor, _tempFile);
        var reloaded = MotorFile.Load(_tempFile);

        // Assert - Only motor signature should be present
        Assert.NotNull(reloaded.MotorSignature);
        Assert.Null(reloaded.Drives[0].DriveSignature);
        Assert.Null(reloaded.Drives[0].Voltages[0].Curves[0].CurveSignature);

        // Motor signature should still be valid
        Assert.True(_service.VerifyMotorProperties(reloaded));
    }

    [Fact]
    public void LoadExistingFile_WithoutSignatures_DoesNotFail()
    {
        // Arrange - Create a simple motor definition file without signatures
        var motor = new ServoMotor("Legacy Motor")
        {
            Manufacturer = "Test",
            PartNumber = "LEG-001",
            Power = 1000,
            MaxSpeed = 3000,
            RatedSpeed = 2000,
            RatedContinuousTorque = 30,
            RatedPeakTorque = 40
        };

        var drive = new Drive("Legacy Drive");
        var voltage = new Voltage(208)
        {
            Power = 1000,
            MaxSpeed = 3000,
            RatedSpeed = 2000,
            RatedContinuousTorque = 30,
            RatedPeakTorque = 40,
            ContinuousAmperage = 5,
            PeakAmperage = 10
        };

        var curve = new Curve("Peak");
        curve.InitializeData(3000, 40);
        voltage.Curves.Add(curve);
        drive.Voltages.Add(voltage);
        motor.Drives.Add(drive);

        // Act
        MotorFile.Save(motor, _tempFile);
        var loaded = MotorFile.Load(_tempFile);

        // Assert - Should load successfully with null signatures
        Assert.NotNull(loaded);
        Assert.Null(loaded.MotorSignature);
        Assert.False(_service.VerifyMotorProperties(loaded)); // No signature means not verified
    }

    private static ServoMotor CreateTestMotor()
    {
        var motor = new ServoMotor("Test Motor")
        {
            Manufacturer = "Test Manufacturer",
            PartNumber = "TEST-001",
            Power = 1500,
            MaxSpeed = 5000,
            RatedSpeed = 3000,
            RatedContinuousTorque = 45,
            RatedPeakTorque = 55,
            Weight = 8.5m,
            RotorInertia = 0.0025m,
            FeedbackPpr = 131072
        };

        var drive = new Drive("Test Drive")
        {
            Manufacturer = "Drive Manufacturer",
            PartNumber = "DRV-001"
        };

        var voltage = new Voltage(208)
        {
            Power = 1400,
            MaxSpeed = 4800,
            RatedSpeed = 2900,
            RatedContinuousTorque = 42,
            RatedPeakTorque = 52,
            ContinuousAmperage = 9.5m,
            PeakAmperage = 22
        };

        var curve = new Curve("Peak");
        curve.InitializeData(4800, 52);
        voltage.Curves.Add(curve);

        drive.Voltages.Add(voltage);
        motor.Drives.Add(drive);

        return motor;
    }
}
