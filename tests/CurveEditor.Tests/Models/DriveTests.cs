using JordanRobot.MotorDefinition.Model;
using System;
using Xunit;

namespace CurveEditor.Tests.Models;

public class DriveTests
{
    [Fact]
    public void Constructor_Default_CreatesUnnamedDrive()
    {
        var drive = new Drive();

        Assert.Equal("Unnamed Drive", drive.Name);
        Assert.Empty(drive.Voltages);
    }

    [Fact]
    public void Constructor_WithName_CreatesDrive()
    {
        var drive = new Drive("Test Drive");

        Assert.Equal("Test Drive", drive.Name);
        Assert.Empty(drive.Voltages);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_NullOrEmpty_ThrowsArgumentException(string? invalidName)
    {
        var drive = new Drive();

        Assert.Throws<ArgumentException>(() => drive.Name = invalidName!);
    }

    [Fact]
    public void GetVoltage_ExistingVoltage_ReturnsCorrectly()
    {
        var drive = new Drive("Test Drive");
        var voltage = drive.AddVoltage(220);

        var result = drive.GetVoltage(220);

        Assert.Same(voltage, result);
    }

    [Fact]
    public void GetVoltage_WithDefaultTolerance_ReturnsCorrectly()
    {
        var drive = new Drive("Test Drive");
        drive.AddVoltage(220);

        // Within default tolerance of 0.1V
        var result = drive.GetVoltage(220.05m);

        Assert.NotNull(result);
        Assert.Equal(220m, result.Value);
    }

    [Fact]
    public void GetVoltage_WithTolerance_ReturnsCorrectly()
    {
        var drive = new Drive("Test Drive");
        drive.AddVoltage(220);

        var result = drive.GetVoltage(220.05m, tolerance: 0.1m);

        Assert.NotNull(result);
        Assert.Equal(220m, result.Value);
    }

    [Fact]
    public void GetVoltage_NonExistentVoltage_ReturnsNull()
    {
        var drive = new Drive("Test Drive");
        drive.AddVoltage(220);

        var result = drive.GetVoltage(208);

        Assert.Null(result);
    }

    [Fact]
    public void AddVoltage_NewVoltage_AddsVoltage()
    {
        var drive = new Drive("Test Drive");

        var voltage = drive.AddVoltage(220);

        Assert.Single(drive.Voltages);
        Assert.Equal(220m, voltage.Value);
    }

    [Fact]
    public void AddVoltage_DuplicateVoltage_ThrowsInvalidOperationException()
    {
        var drive = new Drive("Test Drive");
        drive.AddVoltage(220);

        var exception = Assert.Throws<InvalidOperationException>(() => drive.AddVoltage(220));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void AddVoltage_MultipleVoltages_AddsAll()
    {
        var drive = new Drive("Test Drive");

        drive.AddVoltage(208);
        drive.AddVoltage(220);
        drive.AddVoltage(480);

        Assert.Equal(3, drive.Voltages.Count);
    }

    [Fact]
    public void PartNumber_CanBeSet()
    {
        var drive = new Drive("Test Drive")
        {
            PartNumber = "SD-1234"
        };

        Assert.Equal("SD-1234", drive.PartNumber);
    }

    [Fact]
    public void Manufacturer_CanBeSet()
    {
        var drive = new Drive("Test Drive")
        {
            Manufacturer = "Acme Drives"
        };

        Assert.Equal("Acme Drives", drive.Manufacturer);
    }
}
