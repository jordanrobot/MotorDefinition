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

        // Assert - 2 curves + 1 vertical line for Voltage Max Speed
        Assert.Equal(3, viewModel.Series.Count);
        Assert.Contains(viewModel.Series, s => s.Name == "Peak");
        Assert.Contains(viewModel.Series, s => s.Name == "Continuous");
        Assert.Contains(viewModel.Series, s => s.Name == "Voltage Max Speed");
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

    [Fact]
    public void UpdateAxes_IncludesMotorRatedSpeedInMaxCalculation()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 4000,
            MotorRatedSpeed = 8000  // Higher than motor max speed
        };
        var voltage = CreateTestVoltage();
        voltage.MaxSpeed = 5000;

        // Act
        viewModel.CurrentVoltage = voltage;
        var xAxis = viewModel.XAxes[0];

        // Assert - should use Motor Rated Speed as the max
        Assert.Equal(8000, xAxis.MaxLimit);
    }

    [Fact]
    public void UpdateAxes_IncludesVoltageRatedSpeedInMaxCalculation()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 4000,
            MotorRatedSpeed = 5000
        };
        var voltage = CreateTestVoltage();
        voltage.MaxSpeed = 6000;
        voltage.RatedSpeed = 9000;  // Higher than all other speeds

        // Act
        viewModel.CurrentVoltage = voltage;
        var xAxis = viewModel.XAxes[0];

        // Assert - should use Voltage Rated Speed as the max
        Assert.Equal(9000, xAxis.MaxLimit);
    }

    [Fact]
    public void UpdateAxes_IncludesMaxRpmFromCurveDataInMaxCalculation()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 4000,
            MotorRatedSpeed = 5000
        };
        var voltage = CreateTestVoltage();
        voltage.MaxSpeed = 6000;
        voltage.RatedSpeed = 7000;
        
        // Add curve data with a higher max RPM
        voltage.Curves[0].Data.Add(new DataPoint { Rpm = 10000, Torque = 10 });

        // Act
        viewModel.CurrentVoltage = voltage;
        var xAxis = viewModel.XAxes[0];

        // Assert - should use max RPM from curve data
        Assert.Equal(10000, xAxis.MaxLimit);
    }

    [Fact]
    public void UpdateChart_AddsMotorRatedSpeedVerticalLine_WhenMotorRatedSpeedIsPositive()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6000,
            MotorRatedSpeed = 3000
        };
        var voltage = CreateTestVoltage();

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.Contains(viewModel.Series, s => s.Name == "Motor Rated Speed");
    }

    [Fact]
    public void UpdateChart_DoesNotAddMotorRatedSpeedVerticalLine_WhenMotorRatedSpeedIsZero()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6000,
            MotorRatedSpeed = 0
        };
        var voltage = CreateTestVoltage();

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.DoesNotContain(viewModel.Series, s => s.Name == "Motor Rated Speed");
    }

    [Fact]
    public void UpdateChart_AddsVoltageMaxSpeedVerticalLine_WhenVoltageMaxSpeedIsPositive()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6000
        };
        var voltage = CreateTestVoltage();
        voltage.MaxSpeed = 5000;

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.Contains(viewModel.Series, s => s.Name == "Voltage Max Speed");
    }

    [Fact]
    public void UpdateChart_DoesNotAddVoltageMaxSpeedVerticalLine_WhenVoltageMaxSpeedIsZero()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6000
        };
        var voltage = CreateTestVoltage();
        voltage.MaxSpeed = 0;

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.DoesNotContain(viewModel.Series, s => s.Name == "Voltage Max Speed");
    }

    [Fact]
    public void MotorRatedSpeed_WhenChanged_UpdatesAxes()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 4000
        };
        var voltage = CreateTestVoltage();
        voltage.MaxSpeed = 5000;
        viewModel.CurrentVoltage = voltage;
        var oldMaxLimit = viewModel.XAxes[0].MaxLimit;

        // Act
        viewModel.MotorRatedSpeed = 8000;  // Higher than all other speeds
        var newMaxLimit = viewModel.XAxes[0].MaxLimit;

        // Assert
        Assert.NotEqual(oldMaxLimit, newMaxLimit);
        Assert.Equal(8000, newMaxLimit);
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

    [Fact]
    public void ShowPowerCurves_DefaultsToFalse()
    {
        // Arrange & Act
        var viewModel = new ChartViewModel();

        // Assert
        Assert.False(viewModel.ShowPowerCurves);
    }

    [Fact]
    public void ShowPowerCurves_WhenEnabled_AddsPowerSeriesToChart()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();
        viewModel.CurrentVoltage = voltage;
        var initialCount = viewModel.Series.Count;

        // Act
        viewModel.ShowPowerCurves = true;

        // Assert - should have original torque series + power series
        Assert.True(viewModel.Series.Count > initialCount);
        Assert.Contains(viewModel.Series, s => s.Name != null && s.Name.Contains("(Power)"));
    }

    [Fact]
    public void ShowPowerCurves_WhenDisabled_RemovesPowerSeriesFromChart()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();
        viewModel.ShowPowerCurves = true;
        viewModel.CurrentVoltage = voltage;

        // Act
        viewModel.ShowPowerCurves = false;

        // Assert - should only have torque series
        Assert.DoesNotContain(viewModel.Series, s => s.Name != null && s.Name.Contains("(Power)"));
    }

    [Fact]
    public void PowerUnit_DefaultsToKw()
    {
        // Arrange & Act
        var viewModel = new ChartViewModel();

        // Assert
        Assert.Equal("kW", viewModel.PowerUnit);
    }

    [Fact]
    public void ShowPowerCurves_AddsSecondaryYAxis()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();
        viewModel.CurrentVoltage = voltage;

        // Act
        viewModel.ShowPowerCurves = true;

        // Assert - should have 2 Y-axes (torque and power)
        Assert.Equal(2, viewModel.YAxes.Length);
        Assert.Contains(viewModel.YAxes, axis => axis.Name != null && axis.Name.Contains("Power"));
    }

    [Fact]
    public void ShowPowerCurves_WhenDisabled_HasOnlyOneYAxis()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();
        viewModel.ShowPowerCurves = false;
        viewModel.CurrentVoltage = voltage;

        // Act & Assert - should only have 1 Y-axis (torque)
        Assert.Single(viewModel.YAxes);
        Assert.Contains("Torque", viewModel.YAxes[0].Name);
    }

    [Theory]
    [InlineData("W")]
    [InlineData("kW")]
    [InlineData("hp")]
    public void PowerAxisMaxValue_RoundsToStandardizedIncrements(string powerUnit)
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            PowerUnit = powerUnit,
            ShowPowerCurves = true
        };
        
        // Create a voltage with specific power values
        var voltage = new Voltage(220)
        {
            MaxSpeed = 5000,
            RatedPeakTorque = 10,
            RatedContinuousTorque = 8
        };
        
        var peakSeries = new Curve("Peak");
        peakSeries.Data.Add(new DataPoint { Rpm = 3000, Torque = 10 });
        voltage.Curves.Add(peakSeries);
        
        viewModel.CurrentVoltage = voltage;

        // Act - Get the power Y-axis
        var powerAxis = viewModel.YAxes[1]; // Second axis is power

        // Assert - The max limit should be a standardized round number
        var maxLimit = powerAxis.MaxLimit ?? 0;
        
        if (powerUnit == "W")
        {
            // For W, max should be one of: 10, 20, 50, 100, 250, 500, 1000, 1500, 2000, etc.
            var wIncrements = new[] { 10.0, 20, 50, 100, 250, 500, 1000, 1500, 2000, 5000, 10000 };
            Assert.Contains(maxLimit, wIncrements);
        }
        else
        {
            // For kW/HP, max should be one of: 1, 5, 10, 20, 50, 100, 200, etc.
            var powerIncrements = new[] { 1.0, 5, 10, 20, 50, 100, 200, 500 };
            Assert.Contains(maxLimit, powerIncrements);
        }
    }

    [Theory]
    [InlineData("W", 100)]
    [InlineData("W", 500)]
    [InlineData("W", 1000)]
    [InlineData("W", 2000)]
    [InlineData("kW", 10)]
    [InlineData("kW", 50)]
    [InlineData("kW", 100)]
    [InlineData("hp", 10)]
    [InlineData("hp", 50)]
    [InlineData("hp", 100)]
    public void PowerAxisStep_UsesAppropriateIncrementsForUnit(string powerUnit, double approximateMaxPower)
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            PowerUnit = powerUnit,
            ShowPowerCurves = true
        };
        
        var voltage = new Voltage(220)
        {
            MaxSpeed = 5000,
            RatedPeakTorque = 20,
            RatedContinuousTorque = 15
        };
        
        var peakSeries = new Curve("Peak");
        peakSeries.Data.Add(new DataPoint { Rpm = 3000, Torque = 20 });
        voltage.Curves.Add(peakSeries);
        
        viewModel.CurrentVoltage = voltage;

        // Act
        var powerAxis = viewModel.YAxes[1];

        // Assert - Check that the step is appropriate
        // The step should result in readable number of labels (typically 4-12 labels)
        var numberOfLabels = (powerAxis.MaxLimit ?? 0) / powerAxis.MinStep;
        Assert.InRange(numberOfLabels, 4, 12); // Should have 4-12 labels for readability
        
        // Verify step is reasonable for the unit
        if (powerUnit == "W")
        {
            // For W, steps should be larger (10, 20, 50, 100, etc.)
            Assert.True(powerAxis.MinStep >= 10, $"W step should be >= 10, was {powerAxis.MinStep}");
        }
        else
        {
            // For kW/HP, steps can be smaller (0.1, 0.5, 1, 2, etc.)
            Assert.True(powerAxis.MinStep >= 0.1, $"{powerUnit} step should be >= 0.1, was {powerAxis.MinStep}");
        }
    }

    [Fact]
    public void PowerAxisLabels_UseWholeNumbers()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            PowerUnit = "W",
            ShowPowerCurves = true
        };
        
        var voltage = CreateTestVoltage();
        viewModel.CurrentVoltage = voltage;

        // Act
        var powerAxis = viewModel.YAxes[1];
        
        // Assert - The labeler should format values appropriately
        // Test a few values to ensure they're whole numbers for W
        var label1000 = powerAxis.Labeler(1000);
        var label500 = powerAxis.Labeler(500);
        
        // For W unit with large values, should not show decimals
        Assert.DoesNotContain(".", label1000);
        Assert.DoesNotContain(".", label500);
    }

    [Fact]
    public void ShowMotorRatedSpeedLine_DefaultsToTrue()
    {
        // Arrange & Act
        var viewModel = new ChartViewModel();

        // Assert
        Assert.True(viewModel.ShowMotorRatedSpeedLine);
    }

    [Fact]
    public void ShowVoltageMaxSpeedLine_DefaultsToTrue()
    {
        // Arrange & Act
        var viewModel = new ChartViewModel();

        // Assert
        Assert.True(viewModel.ShowVoltageMaxSpeedLine);
    }

    [Fact]
    public void UpdateChart_HidesMotorRatedSpeedLine_WhenShowMotorRatedSpeedLineIsFalse()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6000,
            MotorRatedSpeed = 3000,
            ShowMotorRatedSpeedLine = false
        };
        var voltage = CreateTestVoltage();

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.DoesNotContain(viewModel.Series, s => s.Name == "Motor Rated Speed");
    }

    [Fact]
    public void UpdateChart_HidesVoltageMaxSpeedLine_WhenShowVoltageMaxSpeedLineIsFalse()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6000,
            ShowVoltageMaxSpeedLine = false
        };
        var voltage = CreateTestVoltage();
        voltage.MaxSpeed = 5000;

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.DoesNotContain(viewModel.Series, s => s.Name == "Voltage Max Speed");
    }

    [Fact]
    public void LegendItems_ContainsTorqueCurveSeries()
    {
        // Arrange
        var viewModel = new ChartViewModel();
        var voltage = CreateTestVoltage();

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.Contains(viewModel.LegendItems, item => item.Name == "Peak");
        Assert.Contains(viewModel.LegendItems, item => item.Name == "Continuous");
    }

    [Fact]
    public void LegendItems_ContainsBrakeTorque_WhenBrakeIsPresent()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            HasBrake = true,
            BrakeTorque = 30
        };
        var voltage = CreateTestVoltage();

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.Contains(viewModel.LegendItems, item => item.Name == "Brake Torque");
    }

    [Fact]
    public void LegendItems_ContainsMotorRatedSpeed_WhenVisible()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6000,
            MotorRatedSpeed = 3000,
            ShowMotorRatedSpeedLine = true
        };
        var voltage = CreateTestVoltage();

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.Contains(viewModel.LegendItems, item => item.Name == "Motor Rated Speed");
    }

    [Fact]
    public void LegendItems_DoesNotContainMotorRatedSpeed_WhenHidden()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6000,
            MotorRatedSpeed = 3000,
            ShowMotorRatedSpeedLine = false
        };
        var voltage = CreateTestVoltage();

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.DoesNotContain(viewModel.LegendItems, item => item.Name == "Motor Rated Speed");
    }

    [Fact]
    public void LegendItems_ContainsVoltageMaxSpeed_WhenVisible()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6000,
            ShowVoltageMaxSpeedLine = true
        };
        var voltage = CreateTestVoltage();
        voltage.MaxSpeed = 5000;

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.Contains(viewModel.LegendItems, item => item.Name == "Voltage Max Speed");
    }

    [Fact]
    public void LegendItems_DoesNotContainVoltageMaxSpeed_WhenHidden()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            MotorMaxSpeed = 6000,
            ShowVoltageMaxSpeedLine = false
        };
        var voltage = CreateTestVoltage();
        voltage.MaxSpeed = 5000;

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.DoesNotContain(viewModel.LegendItems, item => item.Name == "Voltage Max Speed");
    }

    [Fact]
    public void LegendItems_ContainsPowerCurves_WhenPowerCurvesShown()
    {
        // Arrange
        var viewModel = new ChartViewModel
        {
            ShowPowerCurves = true
        };
        var voltage = CreateTestVoltage();

        // Act
        viewModel.CurrentVoltage = voltage;

        // Assert
        Assert.Contains(viewModel.LegendItems, item => item.Name == "Peak (Power)");
        Assert.Contains(viewModel.LegendItems, item => item.Name == "Continuous (Power)");
    }
}
