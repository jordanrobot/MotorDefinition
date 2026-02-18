using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;

namespace CurveEditor.Tests.Services;

/// <summary>
/// Tests for the UnitConversionService class.
/// </summary>
public class UnitConversionServiceTests
{
    [Fact]
    public void ConvertTorque_NmToLbfIn_ConvertsCorrectly()
    {
        // Arrange
        var service = new UnitConversionService();
        var value = 10.0m;

        // Act
        var result = service.ConvertTorque(value, "Nm", "lbf-in");

        // Assert
        Assert.Equal(88.5075m, result, 2);
    }

    [Fact]
    public void ConvertStoredData_WhenFalse_DoesNotModifyCurveData()
    {
        // Arrange
        var service = new UnitConversionService { ConvertStoredData = false };
        var curve = new Curve("Test");
        curve.Data.Add(new DataPoint(0, 0m, 10.0m));
        curve.Data.Add(new DataPoint(50, 1500m, 10.0m));
        var originalTorque = curve.Data[0].Torque;

        // Act
        service.ConvertCurveTorque(curve, "Nm", "lbf-in");

        // Assert - data should remain unchanged
        Assert.Equal(originalTorque, curve.Data[0].Torque);
    }

    [Fact]
    public void ConvertStoredData_WhenTrue_ModifiesCurveData()
    {
        // Arrange
        var service = new UnitConversionService { ConvertStoredData = true };
        var curve = new Curve("Test");
        curve.Data.Add(new DataPoint(0, 0m, 10.0m));
        curve.Data.Add(new DataPoint(50, 1500m, 10.0m));

        // Act
        service.ConvertCurveTorque(curve, "Nm", "lbf-in");

        // Assert - data should be converted
        Assert.Equal(88.5075m, curve.Data[0].Torque, 2);
        Assert.Equal(88.5075m, curve.Data[1].Torque, 2);
    }

    [Fact]
    public void ConvertCurveSpeed_ConvertsAllDataPoints()
    {
        // Arrange
        var service = new UnitConversionService { ConvertStoredData = true };
        var curve = new Curve("Test");
        curve.Data.Add(new DataPoint(0, 0m, 10.0m));
        curve.Data.Add(new DataPoint(50, 1500m, 10.0m));
        curve.Data.Add(new DataPoint(100, 3000m, 10.0m));

        // Act
        service.ConvertCurveSpeed(curve, "rpm", "rpm"); // Same unit - no change

        // Assert
        Assert.Equal(0, curve.Data[0].Rpm);
        Assert.Equal(1500, curve.Data[1].Rpm);
        Assert.Equal(3000, curve.Data[2].Rpm);
    }

    [Fact]
    public void FormatValue_UsesConfiguredDecimalPlaces()
    {
        // Arrange
        var service = new UnitConversionService { DisplayDecimalPlaces = 3 };
        var value = 10.12345m;

        // Act
        var result = service.FormatValue(value, "Nm");

        // Assert
        Assert.Equal("10.123 Nm", result);
    }

    [Fact]
    public void FormatValue_DefaultDecimalPlaces_FormatsWith2Decimals()
    {
        // Arrange
        var service = new UnitConversionService();
        var value = 10.12345m;

        // Act
        var result = service.FormatValue(value, "Nm");

        // Assert
        Assert.Equal("10.12 Nm", result);
    }

    [Fact]
    public void GetDisplayTorque_WhenConvertStoredDataTrue_ReturnsStoredValue()
    {
        // Arrange
        var service = new UnitConversionService { ConvertStoredData = true };
        var storedValue = 10.0m;

        // Act
        var result = service.GetDisplayTorque(storedValue, "Nm", "lbf-in");

        // Assert - returns stored value without conversion
        Assert.Equal(storedValue, result);
    }

    [Fact]
    public void GetDisplayTorque_WhenConvertStoredDataFalse_ConvertsForDisplay()
    {
        // Arrange
        var service = new UnitConversionService { ConvertStoredData = false };
        var storedValue = 10.0m;

        // Act
        var result = service.GetDisplayTorque(storedValue, "Nm", "lbf-in");

        // Assert - returns converted value for display
        Assert.Equal(88.5075m, result, 2);
    }

    [Fact]
    public void GetStoredTorque_WhenConvertStoredDataFalse_ConvertsToStorageUnit()
    {
        // Arrange
        var service = new UnitConversionService { ConvertStoredData = false };
        var displayValue = 88.5075m;

        // Act
        var result = service.GetStoredTorque(displayValue, "lbf-in", "Nm");

        // Assert - returns converted value for storage
        Assert.Equal(10.0m, result, 2);
    }

    [Fact]
    public void ConvertMotorUnits_WhenConvertStoredDataFalse_DoesNotModifyMotor()
    {
        // Arrange
        var service = new UnitConversionService { ConvertStoredData = false };
        var motor = new ServoMotor
        {
            RatedContinuousTorque = 10.0m,
            RatedPeakTorque = 15.0m,
            MaxSpeed = 3000,
            RatedSpeed = 2500,
            Power = 1000,
            Weight = 5.0m
        };
        var oldUnits = new UnitSettings();
        var newUnits = new UnitSettings { Torque = "lbf-in" };

        // Act
        service.ConvertMotorUnits(motor, oldUnits, newUnits);

        // Assert - motor values should remain unchanged
        Assert.Equal(10.0m, motor.RatedContinuousTorque);
        Assert.Equal(15.0m, motor.RatedPeakTorque);
    }

    [Fact]
    public void ConvertMotorUnits_WhenConvertStoredDataTrue_ModifiesMotorValues()
    {
        // Arrange
        var service = new UnitConversionService { ConvertStoredData = true };
        var motor = new ServoMotor
        {
            RatedContinuousTorque = 10.0m,
            RatedPeakTorque = 15.0m,
            MaxSpeed = 3000,
            RatedSpeed = 2500,
            Power = 1000,
            Weight = 5.0m
        };
        var oldUnits = new UnitSettings { Torque = "Nm", Weight = "kg" };
        var newUnits = new UnitSettings { Torque = "lbf-in", Weight = "lbs" };

        // Act
        service.ConvertMotorUnits(motor, oldUnits, newUnits);

        // Assert - motor values should be converted
        Assert.Equal(88.5075m, motor.RatedContinuousTorque, 2);
        Assert.Equal(132.7612m, motor.RatedPeakTorque, 2);
        Assert.Equal(11.0231m, motor.Weight, 2);
    }

    [Fact]
    public void IsUnitSupported_ValidUnit_ReturnsTrue()
    {
        // Arrange
        var service = new UnitConversionService();

        // Act
        var result = service.IsUnitSupported("Nm");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsUnitSupported_InvalidUnit_ReturnsFalse()
    {
        // Arrange
        var service = new UnitConversionService();

        // Act
        var result = service.IsUnitSupported("invalid");

        // Assert
        Assert.False(result);
    }
}
