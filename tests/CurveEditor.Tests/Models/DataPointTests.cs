using JordanRobot.MotorDefinition.Model;
using System;
using Xunit;

namespace CurveEditor.Tests.Models;

public class DataPointTests
{
    [Fact]
    public void Constructor_ValidValues_CreatesDataPoint()
    {
        var point = new DataPoint { Percent = 50, Rpm = 2500, Torque = 45.5m };

        Assert.Equal(50, point.Percent);
        Assert.Equal(2500m, point.Rpm);
        Assert.Equal(45.5m, point.Torque);
    }

    [Fact]
    public void Constructor_WithParameters_CreatesDataPoint()
    {
        var point = new DataPoint(50, 2500m, 45.5m);

        Assert.Equal(50, point.Percent);
        Assert.Equal(2500m, point.Rpm);
        Assert.Equal(45.5m, point.Torque);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Percent_Negative_ThrowsArgumentOutOfRangeException(int invalidPercent)
    {
        var point = new DataPoint();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => point.Percent = invalidPercent);
        Assert.Contains("Percent cannot be negative", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(99)]
    [InlineData(100)]
    [InlineData(101)]
    [InlineData(200)]
    [InlineData(150)] // Overspeed value
    [InlineData(250)] // Overspeed value
    public void Percent_ValidValues_SetsPercent(int validPercent)
    {
        var point = new DataPoint { Percent = validPercent };

        Assert.Equal(validPercent, point.Percent);
    }

    [Fact]
    public void Rpm_NegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var point = new DataPoint();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => point.Rpm = -1);
        Assert.Contains("RPM cannot be negative", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    [InlineData(5000.5)]
    public void Rpm_ValidValues_SetsRpm(double validRpm)
    {
        var point = new DataPoint { Rpm = (decimal)validRpm };

        Assert.Equal((decimal)validRpm, point.Rpm);
    }

    [Fact]
    public void Torque_NegativeValue_Allowed()
    {
        // Negative torque is allowed for regenerative braking
        var point = new DataPoint { Torque = -10.5m };

        Assert.Equal(-10.5m, point.Torque);
    }

    [Theory]
    [InlineData(1000.4, 1000)]
    [InlineData(1000.5, 1000)] // Math.Round uses banker's rounding (1000.5 rounds to 1000, the nearest even)
    [InlineData(1000.6, 1001)]
    [InlineData(2500.49, 2500)]
    [InlineData(2500.51, 2501)]
    public void DisplayRpm_RoundsToNearestWholeNumber(double rpm, int expected)
    {
        var point = new DataPoint { Percent = 50, Rpm = (decimal)rpm, Torque = 45.0m };

        Assert.Equal(expected, point.DisplayRpm);
    }

    [Fact]
    public void DisplayRpm_ZeroRpm_ReturnsZero()
    {
        var point = new DataPoint { Percent = 0, Rpm = 0, Torque = 55.0m };

        Assert.Equal(0, point.DisplayRpm);
    }
}
