using CurveEditor.ViewModels;
using JordanRobot.MotorDefinition.Model;

namespace CurveEditor.Tests.ViewModels;

/// <summary>
/// Tests for the ChartViewModel class.
/// </summary>
public class ChartViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultAxes()
    {
        // Arrange & Act
        var viewModel = new ChartViewModel();

        // Assert
        Assert.NotNull(viewModel.XAxes);
        Assert.NotNull(viewModel.YAxes);
        Assert.Single(viewModel.XAxes);
        Assert.Single(viewModel.YAxes);
    }

    [Fact]
    public void CurrentVoltage_WhenNull_SeriesIsEmpty()
    {
        // Arrange
        var viewModel = new ChartViewModel();

        // Act
        viewModel.CurrentVoltage = null;

        // Assert
        Assert.Empty(viewModel.Series);
    }

    [Fact]
    public void CurrentVoltage_WhenSet_UpdatesSeriesWithCurveData()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.Equal(2, viewModel.Series.Count);
    }

    [Fact]
    public void CurrentVoltage_WhenSet_TitleShowsVoltage()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.Contains("220V", viewModel.Title);
    }

    [Fact]
    public void SetSeriesVisibility_HidesSeries()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();
        viewModel.CurrentVoltage = voltage;

        // Act
        viewModel.SetSeriesVisibility("Peak", false);

        // Assert
        Assert.False(viewModel.IsSeriesVisible("Peak"));
    }

    [Fact]
    public void SetSeriesVisibility_ShowsSeries()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();
        viewModel.CurrentVoltage = voltage;
        viewModel.SetSeriesVisibility("Peak", false);

        // Act
        viewModel.SetSeriesVisibility("Peak", true);

        // Assert
        Assert.True(viewModel.IsSeriesVisible("Peak"));
    }

    [Fact]
    public void IsSeriesVisible_DefaultsToTrue()
    {
        // Arrange
        var viewModel = new ChartViewModel();

        // Act & Assert
        Assert.True(viewModel.IsSeriesVisible("NonExistentSeries"));
    }

    [Fact]
    public void RefreshChart_UpdatesSeriesFromVoltage()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();
        viewModel.CurrentVoltage = voltage;
        var initialCount = viewModel.Series.Count;

        // Add a new series to the voltage
        voltage.Curves.Add(new Curve("New Curves"));

        // Act
        viewModel.RefreshChart();

        // Assert
        Assert.Equal(initialCount + 1, viewModel.Series.Count);
    }

    [Fact]
    public void DataChanged_RaisedWhenUpdateDataPointCalled()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();
        viewModel.CurrentVoltage = voltage;
        var eventRaised = false;
        viewModel.DataChanged += (s, e) => eventRaised = true;

        // Act
        viewModel.UpdateDataPoint("Peak", 0, 0, 55.0);

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void UpdateDataPoint_WithInvalidSeriesName_DoesNotThrow()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();
        viewModel.CurrentVoltage = voltage;

        // Act & Assert - should not throw
        viewModel.UpdateDataPoint("NonExistent", 0, 100, 50);
    }

    [Fact]
    public void UpdateDataPoint_WithInvalidIndex_DoesNotThrow()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();
        viewModel.CurrentVoltage = voltage;

        // Act & Assert - should not throw
        viewModel.UpdateDataPoint("Peak", 999, 100, 50);
    }

    [Fact]
    public void TorqueUnit_DefaultsToNm()
    {
        // Arrange & Act
        var viewModel = new ChartViewModel();

        // Assert
        Assert.Equal("Nm", viewModel.TorqueUnit);
    }

    [Fact]
    public void TorqueUnit_WhenSet_UpdatesAxisLabel()
    {
        // Arrange
        var viewModel = new ChartViewModel();

        // Act
        viewModel.TorqueUnit = "lbf-in";
        viewModel.CurrentVoltage = CreateTestVoltage();

        // Assert
        Assert.Contains("lbf-in", viewModel.YAxes[0].Name);
    }

    [Fact]
    public void UpdateAxes_UsesZeroAsXAxisMinimum()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6500
        };
        viewModel.CurrentVoltage = CreateTestVoltage();

        // Act
        var xAxis = viewModel.XAxes[0];

        // Assert
        Assert.Equal(0, xAxis.MinLimit);
    }

    [Fact]
    public void UpdateAxes_UsesMaxOfMotorAndDriveMaxSpeedAsXAxisMaximum()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6500
        };
        var voltage = CreateTestVoltage();
        voltage.MaxSpeed = 4800;

        // Act
        viewModel.CurrentVoltage = voltage;
        var xAxis = viewModel.XAxes[0];

        // Assert - exact max, no rounding
        Assert.Equal(6500, xAxis.MaxLimit);
    }

    [Fact]
    public void UpdateAxes_UsesDriveMaxSpeedWhenGreaterThanMotorMaxSpeed()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 4000
        };
        var voltage = CreateTestVoltage();
        voltage.MaxSpeed = 7200;

        // Act
        viewModel.CurrentVoltage = voltage;
        var xAxis = viewModel.XAxes[0];

        // Assert - exact max, no rounding
        Assert.Equal(7200, xAxis.MaxLimit);
    }

    private static Voltage CreateTestVoltage()
    {
        var voltage = new Voltage(220)
        {
            MaxSpeed = 5000,
            Power = 1500,
            RatedPeakTorque = 55,
            RatedContinuousTorque = 45
        };

        var peakSeries = new Curve("Peak");
        peakSeries.InitializeData(5000, 55);

        var continuousSeries = new Curve("Continuous");
        continuousSeries.InitializeData(5000, 45);

        voltage.Curves.Add(peakSeries);
        voltage.Curves.Add(continuousSeries);

        return voltage;
    }
}
