using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;

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

    #region ValidateCurve Tests

    [Fact]
    public void ValidateCurve_ValidCurve_ReturnsNoErrors()
    {
        // Arrange
        var series = new Curve("Peak");
        series.InitializeData(5000, 55);

        // Act
        var errors = _service.ValidateCurve(series);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateCurve_EmptyCurve_IsAllowedForViewing_ReturnsNoErrors()
    {
        // Arrange
        var series = new Curve("Peak");

        // Act
        var errors = _service.ValidateCurve(series);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateCurve_VariablePointCount_IsAllowedForViewing_ReturnsNoErrors()
    {
        // Arrange
        var series = new Curve("Peak");
        series.Data.Add(new DataPoint(0, 0, 50));

        // Act
        var errors = _service.ValidateCurve(series);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateCurve_TooManyPoints_ReturnsErrors()
    {
        var series = new Curve("Peak");

        for (var i = 0; i < 102; i++)
        {
            series.Data.Add(new DataPoint(i, i, 1));
        }

        var errors = _service.ValidateCurve(series);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("0 to 101 data points"));
    }

    #endregion

    #region ValidateVoltage Tests

    [Fact]
    public void ValidateVoltage_ValidConfig_ReturnsNoErrors()
    {
        // Arrange
        var config = CreateValidVoltage();

        // Act
        var errors = _service.ValidateVoltage(config);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateVoltage_NoCurves_ReturnsErrors()
    {
        // Arrange
        var config = new Voltage(220)
        {
            MaxSpeed = 5000,
            Power = 1500,
            RatedPeakTorque = 55,
            RatedContinuousTorque = 45
        };

        // Act
        var errors = _service.ValidateVoltage(config);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("at least one curve series"));
    }

    [Fact]
    public void ValidateVoltage_NegativePower_ReturnsErrors()
    {
        // Arrange
        var config = CreateValidVoltage();
        config.Power = -100;

        // Act
        var errors = _service.ValidateVoltage(config);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Power cannot be negative"));
    }

    [Fact]
    public void ValidateVoltage_ContinuousExceedsPeak_ReturnsErrors()
    {
        // Arrange
        var config = CreateValidVoltage();
        config.RatedContinuousTorque = 60;
        config.RatedPeakTorque = 50;

        // Act
        var errors = _service.ValidateVoltage(config);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("cannot exceed peak torque"));
    }

    [Fact]
    public void ValidateVoltage_MisalignedPercentAxes_ReturnsErrors()
    {
        var config = CreateValidVoltage();
        var second = new Curve("Continuous");
        second.InitializeData(config.MaxSpeed, 45);
        second.Data[10].Percent = 11;
        config.Curves.Add(second);

        var errors = _service.ValidateVoltage(config);

        Assert.Contains(errors, e => e.Contains("percent axis differs"));
    }

    [Fact]
    public void ValidateVoltage_MisalignedRpmAxes_ReturnsErrors()
    {
        var config = CreateValidVoltage();
        var second = new Curve("Continuous");
        second.InitializeData(config.MaxSpeed, 45);
        second.Data[20].Rpm = config.Curves[0].Data[20].Rpm - 10;
        config.Curves.Add(second);

        var errors = _service.ValidateVoltage(config);

        Assert.Contains(errors, e => e.Contains("rpm axis differs"));
    }

    #endregion

    #region ValidateServoMotor Tests

    [Fact]
    public void ValidateServoMotor_ValidMotor_ReturnsNoErrors()
    {
        // Arrange
        var motor = CreateValidServoMotor();

        // Act
        var errors = _service.ValidateServoMotor(motor);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateServoMotor_EmptyName_ReturnsErrors()
    {
        // Arrange
        var motor = CreateValidServoMotor();
        motor.MotorName = string.Empty;

        // Act
        var errors = _service.ValidateServoMotor(motor);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Motor name cannot be empty"));
    }

    [Fact]
    public void ValidateServoMotor_NoDrives_ReturnsErrors()
    {
        // Arrange
        var motor = new ServoMotor("Test Motor")
        {
            MaxSpeed = 5000,
            Power = 1500,
            RatedPeakTorque = 55,
            RatedContinuousTorque = 45
        };

        // Act
        var errors = _service.ValidateServoMotor(motor);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("at least one drive"));
    }

    [Fact]
    public void ValidateServoMotor_NegativeMaxSpeed_ReturnsErrors()
    {
        // Arrange
        var motor = CreateValidServoMotor();
        motor.MaxSpeed = -100;

        // Act
        var errors = _service.ValidateServoMotor(motor);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Max speed cannot be negative"));
    }

    [Fact]
    public void ValidateServoMotor_NegativeBrakeReleaseTime_ReturnsErrors()
    {
        var motor = CreateValidServoMotor();
        motor.BrakeReleaseTime = -1;

        var errors = _service.ValidateServoMotor(motor);

        Assert.Contains(errors, e => e.Contains("Brake release time"));
    }

    #endregion

    #region Helper Methods

    private static Voltage CreateValidVoltage()
    {
        var config = new Voltage(220)
        {
            MaxSpeed = 5000,
            Power = 1500,
            RatedPeakTorque = 55,
            RatedContinuousTorque = 45
        };

        var series = new Curve("Peak");
        series.InitializeData(5000, 55);
        config.Curves.Add(series);

        return config;
    }

    private static ServoMotor CreateValidServoMotor()
    {
        var motor = new ServoMotor("Test Motor")
        {
            MaxSpeed = 5000,
            Power = 1500,
            RatedPeakTorque = 55,
            RatedContinuousTorque = 45
        };

        var drive = new Drive("Test Drive");
        var voltage = CreateValidVoltage();
        drive.Voltages.Add(voltage);
        motor.Drives.Add(drive);

        return motor;
    }

    #endregion
}
