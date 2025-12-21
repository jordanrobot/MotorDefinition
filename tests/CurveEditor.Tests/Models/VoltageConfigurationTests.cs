using System;
using CurveEditor.Models;
using Xunit;

namespace CurveEditor.Tests.Models;

public class VoltageConfigurationTests
{
    [Fact]
    public void Constructor_Default_CreatesConfiguration()
    {
        var voltage = new VoltageConfiguration();

        Assert.Empty(voltage.Series);
    }

    [Fact]
    public void Constructor_WithVoltage_CreatesConfiguration()
    {
        var voltage = new VoltageConfiguration(220);

        Assert.Equal(220, voltage.Voltage);
        Assert.Empty(voltage.Series);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Voltage_NonPositive_ThrowsArgumentOutOfRangeException(double invalidVoltage)
    {
        var voltage = new VoltageConfiguration();

        Assert.Throws<ArgumentOutOfRangeException>(() => voltage.Voltage = invalidVoltage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(208)]
    [InlineData(220)]
    [InlineData(480)]
    public void Voltage_ValidValues_SetsVoltage(double validVoltage)
    {
        var voltage = new VoltageConfiguration { Voltage = validVoltage };

        Assert.Equal(validVoltage, voltage.Voltage);
    }

    [Fact]
    public void GetSeriesByName_ExistingSeries_ReturnsSeries()
    {
        var voltage = new VoltageConfiguration(220) { MaxSpeed = 5000 };
        var series = voltage.AddSeries("Peak", 50);

        var result = voltage.GetSeriesByName("Peak");

        Assert.Same(series, result);
    }

    [Fact]
    public void GetSeriesByName_CaseMismatch_ReturnsNull()
    {
        var voltage = new VoltageConfiguration(220) { MaxSpeed = 5000 };
        voltage.AddSeries("Peak", 50);

        var result = voltage.GetSeriesByName("PEAK");

        Assert.Null(result);
    }

    [Fact]
    public void GetSeriesByName_NonExistentSeries_ReturnsNull()
    {
        var voltage = new VoltageConfiguration(220) { MaxSpeed = 5000 };
        voltage.AddSeries("Peak", 50);

        var result = voltage.GetSeriesByName("Continuous");

        Assert.Null(result);
    }

    [Fact]
    public void AddSeries_NewSeries_AddsAndInitializesSeries()
    {
        var voltage = new VoltageConfiguration(220) { MaxSpeed = 5000 };

        var series = voltage.AddSeries("Peak", 50);

        Assert.Single(voltage.Series);
        Assert.Equal("Peak", series.Name);
        Assert.Equal(101, series.Data.Count);
    }

    [Fact]
    public void AddSeries_DuplicateName_ThrowsInvalidOperationException()
    {
        var voltage = new VoltageConfiguration(220) { MaxSpeed = 5000 };
        voltage.AddSeries("Peak", 50);

        var exception = Assert.Throws<InvalidOperationException>(() => voltage.AddSeries("Peak", 45));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void AddSeries_MultipleSeries_AddsAll()
    {
        var voltage = new VoltageConfiguration(220) { MaxSpeed = 5000 };

        voltage.AddSeries("Peak", 55);
        voltage.AddSeries("Continuous", 45);

        Assert.Equal(2, voltage.Series.Count);
    }

    [Fact]
    public void PerformanceProperties_CanBeSet()
    {
        var voltage = new VoltageConfiguration(220)
        {
            Power = 1500,
            MaxSpeed = 5000,
            RatedSpeed = 3000,
            RatedContinuousTorque = 45.0,
            RatedPeakTorque = 55.0,
            ContinuousAmperage = 10.5,
            PeakAmperage = 25.0
        };

        Assert.Equal(1500, voltage.Power);
        Assert.Equal(5000, voltage.MaxSpeed);
        Assert.Equal(3000, voltage.RatedSpeed);
        Assert.Equal(45.0, voltage.RatedContinuousTorque);
        Assert.Equal(55.0, voltage.RatedPeakTorque);
        Assert.Equal(10.5, voltage.ContinuousAmperage);
        Assert.Equal(25.0, voltage.PeakAmperage);
    }

    [Fact]
    public void AddSeries_UsesMaxSpeedForRpm()
    {
        var voltage = new VoltageConfiguration(220) { MaxSpeed = 4000 };

        var series = voltage.AddSeries("Peak", 50);

        // Last point should have RPM = MaxSpeed
        Assert.Equal(4000, series.Data[100].Rpm);
    }
}
