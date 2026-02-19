using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CurveEditor.Tests.Services;

public class CurveGeneratorServiceTests
{
    private readonly CurveGeneratorService _service = new();

    [Fact]
    public void InterpolateCurve_Creates101DataPoints()
    {
        var points = _service.InterpolateCurve(maxRpm: 5000, maxTorque: 50, maxPower: 1500);

        Assert.Equal(101, points.Count);
    }

    [Fact]
    public void InterpolateCurve_FirstPointIsZeroPercent()
    {
        var points = _service.InterpolateCurve(maxRpm: 5000, maxTorque: 50, maxPower: 1500);

        Assert.Equal(0, points.First().Percent);
    }

    [Fact]
    public void InterpolateCurve_LastPointIs100Percent()
    {
        var points = _service.InterpolateCurve(maxRpm: 5000, maxTorque: 50, maxPower: 1500);

        Assert.Equal(100, points.Last().Percent);
    }

    [Fact]
    public void InterpolateCurve_PointsAtExact1PercentIncrements()
    {
        var points = _service.InterpolateCurve(maxRpm: 5000, maxTorque: 50, maxPower: 1500);

        for (var i = 0; i <= 100; i++)
        {
            Assert.Equal(i, points[i].Percent);
        }
    }

    [Fact]
    public void InterpolateCurve_RpmCalculatedCorrectly()
    {
        var points = _service.InterpolateCurve(maxRpm: 5000, maxTorque: 50, maxPower: 1500);

        Assert.Equal(0m, points[0].Rpm);
        Assert.Equal(2500m, points[50].Rpm);
        Assert.Equal(5000m, points[100].Rpm);
    }

    [Fact]
    public void InterpolateCurve_ConstantTorqueRegion_MaintainsTorque()
    {
        var maxTorque = 50.0m;
        var maxPower = 1500.0m;
        var maxRpm = 5000.0m;

        var points = _service.InterpolateCurve(maxRpm, maxTorque, maxPower);

        // Calculate corner speed
        var cornerRpm = _service.CalculateCornerSpeed(maxTorque, maxPower);

        // Find points in constant torque region (before corner speed)
        var lowSpeedPoints = points.Where(p => p.Rpm > 0 && p.Rpm < cornerRpm * 0.9m).ToList();

        foreach (var point in lowSpeedPoints)
        {
            Assert.Equal(maxTorque, point.Torque, precision: 1);
        }
    }

    [Fact]
    public void InterpolateCurve_ConstantPowerRegion_TorqueDecreases()
    {
        var maxTorque = 50.0m;
        var maxPower = 1500.0m;
        var maxRpm = 5000.0m;

        var points = _service.InterpolateCurve(maxRpm, maxTorque, maxPower);

        var cornerRpm = _service.CalculateCornerSpeed(maxTorque, maxPower);

        // Find points in constant power region (after corner speed)
        var highSpeedPoints = points.Where(p => p.Rpm > cornerRpm * 1.2m).ToList();

        // Torque should decrease as speed increases in constant power region
        for (var i = 0; i < highSpeedPoints.Count - 1; i++)
        {
            Assert.True(highSpeedPoints[i].Torque >= highSpeedPoints[i + 1].Torque,
                $"Torque should decrease: {highSpeedPoints[i].Torque} >= {highSpeedPoints[i + 1].Torque}");
        }
    }

    [Theory]
    [InlineData(50.0, 1000, 5235.99)]  // P = T × RPM × 2π/60
    [InlineData(45.0, 3000, 14137.17)]
    [InlineData(0, 1000, 0)]
    [InlineData(50, 0, 0)]
    public void CalculatePower_FormulaCorrect(decimal torque, decimal rpm, decimal expectedWatts)
    {
        var result = _service.CalculatePower(torque, rpm);

        Assert.Equal(expectedWatts, result, precision: 1);
    }

    [Fact]
    public void CalculateCornerSpeed_FormulaCorrect()
    {
        var maxTorque = 50.0m;
        var maxPower = 1500.0m;

        var cornerRpm = _service.CalculateCornerSpeed(maxTorque, maxPower);

        // cornerRpm = (maxPower × 60) / (maxTorque × 2π)
        var expected = (maxPower * 60m) / (maxTorque * 2m * (decimal)Math.PI);
        Assert.Equal(expected, cornerRpm, precision: 2);
    }

    [Fact]
    public void CalculateCornerSpeed_ZeroTorque_ReturnsZero()
    {
        var cornerRpm = _service.CalculateCornerSpeed(0, 1500);

        Assert.Equal(0, cornerRpm);
    }

    [Fact]
    public void GenerateCurve_ReturnsSeriesWithCorrectName()
    {
        var series = _service.GenerateCurve("Peak", 5000, 50, 1500);

        Assert.Equal("Peak", series.Name);
    }

    [Fact]
    public void GenerateCurve_ReturnsSeriesWith101Points()
    {
        var series = _service.GenerateCurve("Peak", 5000, 50, 1500);

        Assert.Equal(101, series.Data.Count);
    }

    [Fact]
    public void InterpolateCurve_ZeroMaxRpm_CreatesZeroRpmPoints()
    {
        var points = _service.InterpolateCurve(0, 50, 1500);

        Assert.Equal(101, points.Count);
        Assert.All(points, p => Assert.Equal(0, p.Rpm));
    }

    [Fact]
    public void InterpolateCurve_ZeroMaxTorque_CreatesZeroTorquePoints()
    {
        var points = _service.InterpolateCurve(5000, 0, 1500);

        Assert.Equal(101, points.Count);
        Assert.All(points, p => Assert.Equal(0, p.Torque));
    }

    [Fact]
    public void InterpolateCurve_ZeroMaxPower_CreatesZeroTorquePoints()
    {
        var points = _service.InterpolateCurve(5000, 50, 0);

        Assert.Equal(101, points.Count);
        Assert.All(points, p => Assert.Equal(0, p.Torque));
    }

    [Fact]
    public void InterpolateCurve_NegativeMaxRpm_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.InterpolateCurve(-1, 50, 1500));
    }

    [Fact]
    public void InterpolateCurve_NegativeMaxTorque_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.InterpolateCurve(5000, -1, 1500));
    }

    [Fact]
    public void InterpolateCurve_NegativeMaxPower_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.InterpolateCurve(5000, 50, -1));
    }

    [Fact]
    public void InterpolateCurve_TorqueNeverNegative()
    {
        var points = _service.InterpolateCurve(5000, 50, 1500);

        Assert.All(points, p => Assert.True(p.Torque >= 0));
    }

    [Fact]
    public void InterpolateCurve_FirstPoint_HasMaxTorque()
    {
        var points = _service.InterpolateCurve(5000, 50, 1500);

        Assert.Equal(50, points[0].Torque);
    }
}
