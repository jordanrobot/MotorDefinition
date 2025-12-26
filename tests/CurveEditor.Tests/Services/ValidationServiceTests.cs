using JordanRobot.MotorDefinitions.Model;
using CurveEditor.Services;

namespace CurveEditor.Tests.Services;

/// <summary>
/// Tests for the ValidationService class.
/// </summary>
public class ValidationServiceTests
{
    private readonly ValidationService _service = new();

    #region ValidateDataPoint Tests

    [Fact]
    public void ValidateDataPoint_ValidPoint_ReturnsNoErrors()
    {
        // Arrange
        var point = new DataPoint(50, 2500, 45.0);

        // Act
        var errors = _service.ValidateDataPoint(point);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateDataPoint_ZeroRpm_ReturnsNoErrors()
    {
        // Arrange
        var point = new DataPoint(0, 0, 50.0);

        // Act
        var errors = _service.ValidateDataPoint(point);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateDataPoint_PercentAbove100_IsAllowedForViewing_ReturnsNoErrors()
    {
        var point = new DataPoint(110, 6000, 45.0);

        var errors = _service.ValidateDataPoint(point);

        Assert.Empty(errors);
    }

    #endregion

    #region ValidateCurveSeries Tests

    [Fact]
    public void ValidateCurveSeries_ValidSeries_ReturnsNoErrors()
    {
        // Arrange
        var series = new CurveSeries("Peak");
        series.InitializeData(5000, 55);

        // Act
        var errors = _service.ValidateCurveSeries(series);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateCurveSeries_EmptySeries_IsAllowedForViewing_ReturnsNoErrors()
    {
        // Arrange
        var series = new CurveSeries("Peak");

        // Act
        var errors = _service.ValidateCurveSeries(series);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateCurveSeries_VariablePointCount_IsAllowedForViewing_ReturnsNoErrors()
    {
        // Arrange
        var series = new CurveSeries("Peak");
        series.Data.Add(new DataPoint(0, 0, 50));

        // Act
        var errors = _service.ValidateCurveSeries(series);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateCurveSeries_TooManyPoints_ReturnsErrors()
    {
        var series = new CurveSeries("Peak");

        for (var i = 0; i < 102; i++)
        {
            series.Data.Add(new DataPoint(i, i, 1));
        }

        var errors = _service.ValidateCurveSeries(series);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("0 to 101 data points"));
    }

    #endregion

    #region ValidateVoltageConfiguration Tests

    [Fact]
    public void ValidateVoltageConfiguration_ValidConfig_ReturnsNoErrors()
    {
        // Arrange
        var config = CreateValidVoltageConfiguration();

        // Act
        var errors = _service.ValidateVoltageConfiguration(config);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateVoltageConfiguration_NoSeries_ReturnsErrors()
    {
        // Arrange
        var config = new VoltageConfiguration(220)
        {
            MaxSpeed = 5000,
            Power = 1500,
            RatedPeakTorque = 55,
            RatedContinuousTorque = 45
        };

        // Act
        var errors = _service.ValidateVoltageConfiguration(config);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("at least one curve series"));
    }

    [Fact]
    public void ValidateVoltageConfiguration_NegativePower_ReturnsErrors()
    {
        // Arrange
        var config = CreateValidVoltageConfiguration();
        config.Power = -100;

        // Act
        var errors = _service.ValidateVoltageConfiguration(config);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Power cannot be negative"));
    }

    [Fact]
    public void ValidateVoltageConfiguration_ContinuousExceedsPeak_ReturnsErrors()
    {
        // Arrange
        var config = CreateValidVoltageConfiguration();
        config.RatedContinuousTorque = 60;
        config.RatedPeakTorque = 50;

        // Act
        var errors = _service.ValidateVoltageConfiguration(config);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("cannot exceed peak torque"));
    }

    [Fact]
    public void ValidateVoltageConfiguration_MisalignedPercentAxes_ReturnsErrors()
    {
        var config = CreateValidVoltageConfiguration();
        var second = new CurveSeries("Continuous");
        second.InitializeData(config.MaxSpeed, 45);
        second.Data[10].Percent = 11;
        config.Series.Add(second);

        var errors = _service.ValidateVoltageConfiguration(config);

        Assert.Contains(errors, e => e.Contains("percent axis differs"));
    }

    [Fact]
    public void ValidateVoltageConfiguration_MisalignedRpmAxes_ReturnsErrors()
    {
        var config = CreateValidVoltageConfiguration();
        var second = new CurveSeries("Continuous");
        second.InitializeData(config.MaxSpeed, 45);
        second.Data[20].Rpm = config.Series[0].Data[20].Rpm - 10;
        config.Series.Add(second);

        var errors = _service.ValidateVoltageConfiguration(config);

        Assert.Contains(errors, e => e.Contains("rpm axis differs"));
    }

    #endregion

    #region ValidateMotorDefinition Tests

    [Fact]
    public void ValidateMotorDefinition_ValidMotor_ReturnsNoErrors()
    {
        // Arrange
        var motor = CreateValidMotorDefinition();

        // Act
        var errors = _service.ValidateMotorDefinition(motor);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateMotorDefinition_EmptyName_ReturnsErrors()
    {
        // Arrange
        var motor = CreateValidMotorDefinition();
        motor.MotorName = "";

        // Act
        var errors = _service.ValidateMotorDefinition(motor);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Motor name cannot be empty"));
    }

    [Fact]
    public void ValidateMotorDefinition_NoDrives_ReturnsErrors()
    {
        // Arrange
        var motor = new MotorDefinition("Test Motor")
        {
            MaxSpeed = 5000,
            Power = 1500,
            RatedPeakTorque = 55,
            RatedContinuousTorque = 45
        };

        // Act
        var errors = _service.ValidateMotorDefinition(motor);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("at least one drive"));
    }

    [Fact]
    public void ValidateMotorDefinition_NegativeMaxSpeed_ReturnsErrors()
    {
        // Arrange
        var motor = CreateValidMotorDefinition();
        motor.MaxSpeed = -100;

        // Act
        var errors = _service.ValidateMotorDefinition(motor);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Max speed cannot be negative"));
    }

    [Fact]
    public void ValidateMotorDefinition_NegativeBrakeReleaseTime_ReturnsErrors()
    {
        var motor = CreateValidMotorDefinition();
        motor.BrakeReleaseTime = -1;

        var errors = _service.ValidateMotorDefinition(motor);

        Assert.Contains(errors, e => e.Contains("Brake release time"));
    }

    #endregion

    #region Helper Methods

    private static VoltageConfiguration CreateValidVoltageConfiguration()
    {
        var config = new VoltageConfiguration(220)
        {
            MaxSpeed = 5000,
            Power = 1500,
            RatedPeakTorque = 55,
            RatedContinuousTorque = 45
        };

        var series = new CurveSeries("Peak");
        series.InitializeData(5000, 55);
        config.Series.Add(series);

        return config;
    }

    private static MotorDefinition CreateValidMotorDefinition()
    {
        var motor = new MotorDefinition("Test Motor")
        {
            MaxSpeed = 5000,
            Power = 1500,
            RatedPeakTorque = 55,
            RatedContinuousTorque = 45
        };

        var drive = new DriveConfiguration("Test Drive");
        var voltage = CreateValidVoltageConfiguration();
        drive.Voltages.Add(voltage);
        motor.Drives.Add(drive);

        return motor;
    }

    #endregion
}
