using JordanRobot.MotorDefinition.Model;
using System;
using Xunit;

namespace CurveEditor.Tests.Models;

public class VoltageTests
{
    [Fact]
    public void Constructor_Default_CreatesVoltage()
    {
        var voltage = new Voltage();

        Assert.Empty(voltage.Curves);
    }

    [Fact]
    public void Constructor_WithVoltage_CreatesVoltage()
    {
        var voltage = new Voltage(220);

        Assert.Equal(220m, voltage.Value);
        Assert.Empty(voltage.Curves);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Voltage_NonPositive_ThrowsArgumentOutOfRangeException(double invalidVoltage)
    {
        var voltage = new Voltage();

        Assert.Throws<ArgumentOutOfRangeException>(() => voltage.Value = (decimal)invalidVoltage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(208)]
    [InlineData(220)]
    [InlineData(480)]
    public void Voltage_ValidValues_SetsVoltage(double validVoltage)
    {
        var voltage = new Voltage { Value = (decimal)validVoltage };

        Assert.Equal((decimal)validVoltage, voltage.Value);
    }

    [Fact]
    public void GetSeriesByName_ExistingSeries_ReturnsSeries()
    {
        var voltage = new Voltage(220) { MaxSpeed = 5000 };
        var series = voltage.AddSeries("Peak", 50);

        var result = voltage.GetSeriesByName("Peak");

        Assert.Same(series, result);
    }

    [Fact]
    public void GetSeriesByName_CaseMismatch_ReturnsNull()
    {
        var voltage = new Voltage(220) { MaxSpeed = 5000 };
        voltage.AddSeries("Peak", 50);

        var result = voltage.GetSeriesByName("PEAK");

        Assert.Null(result);
    }

    [Fact]
    public void GetSeriesByName_NonExistentSeries_ReturnsNull()
    {
        var voltage = new Voltage(220) { MaxSpeed = 5000 };
        voltage.AddSeries("Peak", 50);

        var result = voltage.GetSeriesByName("Continuous");

        Assert.Null(result);
    }

    [Fact]
    public void AddSeries_NewSeries_AddsAndInitializesSeries()
    {
        var voltage = new Voltage(220) { MaxSpeed = 5000 };

        var series = voltage.AddSeries("Peak", 50);

        Assert.Single(voltage.Curves);
        Assert.Equal("Peak", series.Name);
        Assert.Equal(101, series.Data.Count);
    }

    [Fact]
    public void AddSeries_DuplicateName_ThrowsInvalidOperationException()
    {
        var voltage = new Voltage(220) { MaxSpeed = 5000 };
        voltage.AddSeries("Peak", 50);

        var exception = Assert.Throws<InvalidOperationException>(() => voltage.AddSeries("Peak", 45));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void AddSeries_MultipleSeries_AddsAll()
    {
        var voltage = new Voltage(220) { MaxSpeed = 5000 };

        voltage.AddSeries("Peak", 55);
        voltage.AddSeries("Continuous", 45);

        Assert.Equal(2, voltage.Curves.Count);
    }

    [Fact]
    public void PerformanceProperties_CanBeSet()
    {
        var voltage = new Voltage(220)
        {
            Power = 1500,
            MaxSpeed = 5000,
            RatedSpeed = 3000,
            RatedContinuousTorque = 45.0m,
            RatedPeakTorque = 55.0m,
            ContinuousAmperage = 10.5m,
            PeakAmperage = 25.0m
        };

        Assert.Equal(1500m, voltage.Power);
        Assert.Equal(5000m, voltage.MaxSpeed);
        Assert.Equal(3000m, voltage.RatedSpeed);
        Assert.Equal(45.0m, voltage.RatedContinuousTorque);
        Assert.Equal(55.0m, voltage.RatedPeakTorque);
        Assert.Equal(10.5m, voltage.ContinuousAmperage);
        Assert.Equal(25.0m, voltage.PeakAmperage);
    }

    [Fact]
    public void AddSeries_UsesMaxSpeedForRpm()
    {
        var voltage = new Voltage(220) { MaxSpeed = 4000 };

        var series = voltage.AddSeries("Peak", 50);

        // Last point should have RPM = MaxSpeed
        Assert.Equal(4000m, series.Data[100].Rpm);
    }
}
