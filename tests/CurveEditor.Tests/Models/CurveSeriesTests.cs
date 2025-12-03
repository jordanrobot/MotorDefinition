using System;
using CurveEditor.Models;
using Xunit;

namespace CurveEditor.Tests.Models;

public class CurveSeriesTests
{
    [Fact]
    public void Constructor_Default_CreatesUnnamedSeries()
    {
        var series = new CurveSeries();

        Assert.Equal("Unnamed", series.Name);
        Assert.Empty(series.Data);
    }

    [Fact]
    public void Constructor_WithName_CreatesSeries()
    {
        var series = new CurveSeries("Peak");

        Assert.Equal("Peak", series.Name);
        Assert.Empty(series.Data);
    }

    [Fact]
    public void Notes_DefaultsToEmpty()
    {
        var series = new CurveSeries();

        Assert.Equal(string.Empty, series.Notes);
    }

    [Fact]
    public void Notes_CanBeSet()
    {
        var series = new CurveSeries("Peak")
        {
            Notes = "Measured at 25°C ambient temperature"
        };

        Assert.Equal("Measured at 25°C ambient temperature", series.Notes);
    }

    [Fact]
    public void Locked_DefaultsToFalse()
    {
        var series = new CurveSeries();

        Assert.False(series.Locked);
    }

    [Fact]
    public void Locked_CanBeSetToTrue()
    {
        var series = new CurveSeries("Peak")
        {
            Locked = true
        };

        Assert.True(series.Locked);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_NullOrEmpty_ThrowsArgumentException(string? invalidName)
    {
        var series = new CurveSeries();

        Assert.Throws<ArgumentException>(() => series.Name = invalidName!);
    }

    [Fact]
    public void InitializeData_Creates101Points()
    {
        var series = new CurveSeries("Peak");

        series.InitializeData(5000, 50);

        Assert.Equal(101, series.Data.Count);
        Assert.Equal(101, series.PointCount);
    }

    [Fact]
    public void InitializeData_PointsAtExact1PercentIncrements()
    {
        var series = new CurveSeries("Peak");

        series.InitializeData(5000, 50);

        for (var i = 0; i <= 100; i++)
        {
            Assert.Equal(i, series.Data[i].Percent);
        }
    }

    [Fact]
    public void InitializeData_RpmCalculatedCorrectly()
    {
        var series = new CurveSeries("Peak");

        series.InitializeData(5000, 50);

        Assert.Equal(0, series.Data[0].Rpm);
        Assert.Equal(2500, series.Data[50].Rpm);
        Assert.Equal(5000, series.Data[100].Rpm);
    }

    [Fact]
    public void InitializeData_TorqueSetToDefault()
    {
        var series = new CurveSeries("Continuous");

        series.InitializeData(3000, 45.5);

        foreach (var point in series.Data)
        {
            Assert.Equal(45.5, point.Torque);
        }
    }

    [Fact]
    public void InitializeData_ZeroMaxRpm_CreatesZeroRpmPoints()
    {
        var series = new CurveSeries("Peak");

        series.InitializeData(0, 50);

        Assert.Equal(101, series.Data.Count);
        Assert.All(series.Data, p => Assert.Equal(0, p.Rpm));
    }

    [Fact]
    public void InitializeData_NegativeMaxRpm_ThrowsArgumentOutOfRangeException()
    {
        var series = new CurveSeries("Peak");

        Assert.Throws<ArgumentOutOfRangeException>(() => series.InitializeData(-1, 50));
    }

    [Fact]
    public void InitializeData_ClearsExistingData()
    {
        var series = new CurveSeries("Peak");
        series.Data.Add(new DataPoint(0, 0, 100));
        series.Data.Add(new DataPoint(1, 50, 100));

        series.InitializeData(5000, 50);

        Assert.Equal(101, series.Data.Count);
        Assert.Equal(50, series.Data[0].Torque);
    }

    [Fact]
    public void ValidateDataIntegrity_ValidData_ReturnsTrue()
    {
        var series = new CurveSeries("Peak");
        series.InitializeData(5000, 50);

        Assert.True(series.ValidateDataIntegrity());
    }

    [Fact]
    public void ValidateDataIntegrity_WrongPointCount_ReturnsFalse()
    {
        var series = new CurveSeries("Peak");
        series.Data.Add(new DataPoint(0, 0, 50));

        Assert.False(series.ValidateDataIntegrity());
    }

    [Fact]
    public void ValidateDataIntegrity_WrongPercentOrder_ReturnsFalse()
    {
        var series = new CurveSeries("Peak");
        series.InitializeData(5000, 50);
        // Swap two points to break order
        (series.Data[50], series.Data[51]) = (series.Data[51], series.Data[50]);

        Assert.False(series.ValidateDataIntegrity());
    }

    [Fact]
    public void InitializeData_First_And_LastPercent_AreCorrect()
    {
        var series = new CurveSeries("Peak");
        series.InitializeData(5000, 50);

        Assert.Equal(0, series.Data[0].Percent);
        Assert.Equal(100, series.Data[100].Percent);
    }

    [Fact]
    public void InitializeData_FirstRpm_IsZero()
    {
        var series = new CurveSeries("Peak");
        series.InitializeData(5000, 50);

        Assert.Equal(0, series.Data[0].Rpm);
    }

    [Fact]
    public void InitializeData_LastRpm_EqualsMaxRpm()
    {
        var series = new CurveSeries("Peak");
        series.InitializeData(5000, 50);

        Assert.Equal(5000, series.Data[100].Rpm);
    }

    [Fact]
    public void IsVisible_DefaultsToTrue()
    {
        var series = new CurveSeries();

        Assert.True(series.IsVisible);
    }

    [Fact]
    public void IsVisible_CanBeSetToFalse()
    {
        var series = new CurveSeries("Peak")
        {
            IsVisible = false
        };

        Assert.False(series.IsVisible);
    }

    [Fact]
    public void IsVisible_CanBeToggled()
    {
        var series = new CurveSeries("Peak");
        
        series.IsVisible = false;
        Assert.False(series.IsVisible);
        
        series.IsVisible = true;
        Assert.True(series.IsVisible);
    }
}
