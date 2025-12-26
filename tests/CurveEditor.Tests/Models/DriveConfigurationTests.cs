using System;
using JordanRobot.MotorDefinitions.Model;
using Xunit;

namespace CurveEditor.Tests.Models;

public class DriveConfigurationTests
{
    [Fact]
    public void Constructor_Default_CreatesUnnamedDrive()
    {
        var drive = new DriveConfiguration();

        Assert.Equal("Unnamed Drive", drive.Name);
        Assert.Empty(drive.Voltages);
    }

    [Fact]
    public void Constructor_WithName_CreatesDrive()
    {
        var drive = new DriveConfiguration("Test Drive");

        Assert.Equal("Test Drive", drive.Name);
        Assert.Empty(drive.Voltages);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_NullOrEmpty_ThrowsArgumentException(string? invalidName)
    {
        var drive = new DriveConfiguration();

        Assert.Throws<ArgumentException>(() => drive.Name = invalidName!);
    }

    [Fact]
    public void GetVoltageConfiguration_ExistingVoltage_ReturnsConfiguration()
    {
        var drive = new DriveConfiguration("Test Drive");
        var voltage = drive.AddVoltageConfiguration(220);

        var result = drive.GetVoltageConfiguration(220);

        Assert.Same(voltage, result);
    }

    [Fact]
    public void GetVoltageConfiguration_WithDefaultTolerance_ReturnsConfiguration()
    {
        var drive = new DriveConfiguration("Test Drive");
        drive.AddVoltageConfiguration(220);

        // Within default tolerance of 0.1V
        var result = drive.GetVoltageConfiguration(220.05);

        Assert.NotNull(result);
        Assert.Equal(220, result.Voltage);
    }

    [Fact]
    public void GetVoltageConfiguration_WithTolerance_ReturnsConfiguration()
    {
        var drive = new DriveConfiguration("Test Drive");
        drive.AddVoltageConfiguration(220);

        var result = drive.GetVoltageConfiguration(220.05, tolerance: 0.1);

        Assert.NotNull(result);
        Assert.Equal(220, result.Voltage);
    }

    [Fact]
    public void GetVoltageConfiguration_NonExistentVoltage_ReturnsNull()
    {
        var drive = new DriveConfiguration("Test Drive");
        drive.AddVoltageConfiguration(220);

        var result = drive.GetVoltageConfiguration(208);

        Assert.Null(result);
    }

    [Fact]
    public void AddVoltageConfiguration_NewVoltage_AddsConfiguration()
    {
        var drive = new DriveConfiguration("Test Drive");

        var voltage = drive.AddVoltageConfiguration(220);

        Assert.Single(drive.Voltages);
        Assert.Equal(220, voltage.Voltage);
    }

    [Fact]
    public void AddVoltageConfiguration_DuplicateVoltage_ThrowsInvalidOperationException()
    {
        var drive = new DriveConfiguration("Test Drive");
        drive.AddVoltageConfiguration(220);

        var exception = Assert.Throws<InvalidOperationException>(() => drive.AddVoltageConfiguration(220));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void AddVoltageConfiguration_MultipleVoltages_AddsAll()
    {
        var drive = new DriveConfiguration("Test Drive");

        drive.AddVoltageConfiguration(208);
        drive.AddVoltageConfiguration(220);
        drive.AddVoltageConfiguration(480);

        Assert.Equal(3, drive.Voltages.Count);
    }

    [Fact]
    public void PartNumber_CanBeSet()
    {
        var drive = new DriveConfiguration("Test Drive")
        {
            PartNumber = "SD-1234"
        };

        Assert.Equal("SD-1234", drive.PartNumber);
    }

    [Fact]
    public void Manufacturer_CanBeSet()
    {
        var drive = new DriveConfiguration("Test Drive")
        {
            Manufacturer = "Acme Drives"
        };

        Assert.Equal("Acme Drives", drive.Manufacturer);
    }
}
