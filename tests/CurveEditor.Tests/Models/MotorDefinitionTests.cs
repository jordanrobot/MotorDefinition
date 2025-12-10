using System;
using System.Linq;
using CurveEditor.Models;
using Xunit;

namespace CurveEditor.Tests.Models;

public class MotorDefinitionTests
{
    [Fact]
    public void Constructor_Default_CreatesEmptyDefinition()
    {
        var motor = new MotorDefinition();

        Assert.Equal(string.Empty, motor.MotorName);
        Assert.Empty(motor.Drives);
        Assert.NotNull(motor.Units);
        Assert.NotNull(motor.Metadata);
    }

    [Fact]
    public void Constructor_WithName_CreatesDefinition()
    {
        var motor = new MotorDefinition("Test Motor");

        Assert.Equal("Test Motor", motor.MotorName);
    }

    [Fact]
    public void SchemaVersion_DefaultsTo1_0_0()
    {
        var motor = new MotorDefinition();

        Assert.Equal(MotorDefinition.CurrentSchemaVersion, motor.SchemaVersion);
        Assert.Equal("1.0.0", motor.SchemaVersion);
    }

    [Fact]
    public void HasValidConfiguration_NoDrives_ReturnsFalse()
    {
        var motor = new MotorDefinition();

        Assert.False(motor.HasValidConfiguration());
    }

    [Fact]
    public void HasValidConfiguration_WithValidConfiguration_ReturnsTrue()
    {
        var motor = new MotorDefinition { MaxSpeed = 5000 };
        var drive = motor.AddDrive("Test Drive");
        var voltage = drive.AddVoltageConfiguration(220);
        voltage.Series.Add(new CurveSeries("Peak"));

        Assert.True(motor.HasValidConfiguration());
    }

    [Fact]
    public void HasValidConfiguration_DriveWithNoVoltages_ReturnsFalse()
    {
        var motor = new MotorDefinition { MaxSpeed = 5000 };
        motor.AddDrive("Test Drive");

        Assert.False(motor.HasValidConfiguration());
    }

    [Fact]
    public void HasValidConfiguration_VoltageWithNoSeries_ReturnsFalse()
    {
        var motor = new MotorDefinition { MaxSpeed = 5000 };
        var drive = motor.AddDrive("Test Drive");
        drive.AddVoltageConfiguration(220);

        Assert.False(motor.HasValidConfiguration());
    }

    [Fact]
    public void GetDriveByName_ExistingDrive_ReturnsDrive()
    {
        var motor = new MotorDefinition();
        var drive = motor.AddDrive("Test Drive");

        var result = motor.GetDriveByName("Test Drive");

        Assert.Same(drive, result);
    }

    [Fact]
    public void GetDriveByName_CaseInsensitive_ReturnsDrive()
    {
        var motor = new MotorDefinition();
        motor.AddDrive("Test Drive");

        var result = motor.GetDriveByName("TEST DRIVE");

        Assert.NotNull(result);
        Assert.Equal("Test Drive", result.Name);
    }

    [Fact]
    public void GetDriveByName_NonExistentDrive_ReturnsNull()
    {
        var motor = new MotorDefinition();
        motor.AddDrive("Drive A");

        var result = motor.GetDriveByName("Drive B");

        Assert.Null(result);
    }

    [Fact]
    public void AddDrive_NewDrive_AddsDrive()
    {
        var motor = new MotorDefinition();

        var drive = motor.AddDrive("Test Drive");

        Assert.Single(motor.Drives);
        Assert.Equal("Test Drive", drive.Name);
    }

    [Fact]
    public void AddDrive_DuplicateName_ThrowsInvalidOperationException()
    {
        var motor = new MotorDefinition();
        motor.AddDrive("Test Drive");

        var exception = Assert.Throws<InvalidOperationException>(() => motor.AddDrive("Test Drive"));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void AddDrive_UpdatesMetadataModified()
    {
        var motor = new MotorDefinition();
        var originalModified = motor.Metadata.Modified;

        motor.AddDrive("Test Drive");

        Assert.True(motor.Metadata.Modified >= originalModified);
    }

    [Fact]
    public void RemoveDrive_ExistingDrive_RemovesDrive()
    {
        var motor = new MotorDefinition();
        motor.AddDrive("Drive A");
        motor.AddDrive("Drive B");

        var result = motor.RemoveDrive("Drive A");

        Assert.True(result);
        Assert.Single(motor.Drives);
        Assert.Equal("Drive B", motor.Drives[0].Name);
    }

    [Fact]
    public void RemoveDrive_LastDrive_ThrowsInvalidOperationException()
    {
        var motor = new MotorDefinition();
        motor.AddDrive("Test Drive");

        var exception = Assert.Throws<InvalidOperationException>(() => motor.RemoveDrive("Test Drive"));
        Assert.Contains("Cannot remove the last drive", exception.Message);
    }

    [Fact]
    public void RemoveDrive_NonExistentDrive_ReturnsFalse()
    {
        var motor = new MotorDefinition();
        motor.AddDrive("Drive A");
        motor.AddDrive("Drive B");

        var result = motor.RemoveDrive("Drive C");

        Assert.False(result);
    }

    [Fact]
    public void RemoveDrive_UpdatesMetadataModified()
    {
        var motor = new MotorDefinition();
        motor.AddDrive("Drive A");
        motor.AddDrive("Drive B");
        var originalModified = motor.Metadata.Modified;

        motor.RemoveDrive("Drive A");

        Assert.True(motor.Metadata.Modified >= originalModified);
    }

    [Fact]
    public void GetAllSeries_ReturnsAllSeriesAcrossAllDrivesAndVoltages()
    {
        var motor = new MotorDefinition { MaxSpeed = 5000 };
        
        var drive1 = motor.AddDrive("Drive 1");
        var voltage1a = drive1.AddVoltageConfiguration(208);
        voltage1a.MaxSpeed = 5000;
        voltage1a.AddSeries("Peak", 50);
        voltage1a.AddSeries("Continuous", 40);
        
        var voltage1b = drive1.AddVoltageConfiguration(220);
        voltage1b.MaxSpeed = 5000;
        voltage1b.AddSeries("Peak", 55);
        
        var drive2 = motor.AddDrive("Drive 2");
        var voltage2 = drive2.AddVoltageConfiguration(208);
        voltage2.MaxSpeed = 5000;
        voltage2.AddSeries("Peak", 48);

        var allSeries = motor.GetAllSeries().ToList();

        Assert.Equal(4, allSeries.Count);
    }

    [Fact]
    public void GetAllSeries_EmptyDrives_ReturnsEmpty()
    {
        var motor = new MotorDefinition();

        var allSeries = motor.GetAllSeries().ToList();

        Assert.Empty(allSeries);
    }

    [Fact]
    public void Units_DefaultValues_AreCorrect()
    {
        var motor = new MotorDefinition();

        Assert.Equal("Nm", motor.Units.Torque);
        Assert.Equal("rpm", motor.Units.Speed);
        Assert.Equal("W", motor.Units.Power);
        Assert.Equal("kg", motor.Units.Weight);
    }

    [Fact]
    public void Metadata_DefaultValues_AreSet()
    {
        var motor = new MotorDefinition();

        Assert.True(motor.Metadata.Created <= DateTime.UtcNow);
        Assert.True(motor.Metadata.Modified <= DateTime.UtcNow);
        Assert.Equal(string.Empty, motor.Metadata.Notes);
    }

    [Fact]
    public void BrakeVoltage_CanBeSet()
    {
        var motor = new MotorDefinition
        {
            HasBrake = true,
            BrakeTorque = 12.0,
            BrakeAmperage = 0.5,
            BrakeVoltage = 24
        };

        Assert.True(motor.HasBrake);
        Assert.Equal(12.0, motor.BrakeTorque);
        Assert.Equal(0.5, motor.BrakeAmperage);
        Assert.Equal(24, motor.BrakeVoltage);
    }
}
