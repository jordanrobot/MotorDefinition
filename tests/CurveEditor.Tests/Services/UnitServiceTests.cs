using JordanRobot.MotorDefinition.Services;

namespace CurveEditor.Tests.Services;

/// <summary>
/// Tests for the UnitService class.
/// </summary>
public class UnitServiceTests
{
    private readonly UnitService _service = new();

    #region Convert Tests

    [Fact]
    public void Convert_SameUnit_ReturnsOriginalValue()
    {
        // Arrange
        var value = 10.5;
        var unit = "Nm";

        // Act
        var result = _service.Convert(value, unit, unit);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void Convert_TorqueNmToLbfIn_ConvertsCorrectly()
    {
        // Arrange
        var value = 10.0; // 10 Nm
        var fromUnit = "Nm";
        var toUnit = "lbf-in";

        // Act
        var result = _service.Convert(value, fromUnit, toUnit);

        // Assert
        // 1 Nm = 8.85075 lbf-in approximately
        Assert.Equal(88.5075, result, 2);
    }

    [Fact]
    public void Convert_TorqueLbfInToNm_ConvertsCorrectly()
    {
        // Arrange
        var value = 88.5075; // approximately 10 Nm
        var fromUnit = "lbf-in";
        var toUnit = "Nm";

        // Act
        var result = _service.Convert(value, fromUnit, toUnit);

        // Assert
        Assert.Equal(10.0, result, 2);
    }

    [Fact]
    public void Convert_PowerWToKw_ConvertsCorrectly()
    {
        // Arrange
        var value = 1000.0; // 1000 W
        var fromUnit = "W";
        var toUnit = "kW";

        // Act
        var result = _service.Convert(value, fromUnit, toUnit);

        // Assert
        Assert.Equal(1.0, result, 2);
    }

    [Fact]
    public void Convert_PowerHpToW_ConvertsCorrectly()
    {
        // Arrange
        var value = 1.0; // 1 hp
        var fromUnit = "hp";
        var toUnit = "W";

        // Act
        var result = _service.Convert(value, fromUnit, toUnit);

        // Assert
        // 1 hp = 745.7 W approximately
        Assert.Equal(745.7, result, 0);
    }

    [Fact]
    public void Convert_MassKgToLbs_ConvertsCorrectly()
    {
        // Arrange
        var value = 1.0; // 1 kg
        var fromUnit = "kg";
        var toUnit = "lbs";

        // Act
        var result = _service.Convert(value, fromUnit, toUnit);

        // Assert
        // 1 kg = 2.20462 lbs approximately
        Assert.Equal(2.20462, result, 2);
    }

    [Fact]
    public void Convert_TimeSecToMs_ConvertsCorrectly()
    {
        // Arrange
        var value = 1.0; // 1 second
        var fromUnit = "s";
        var toUnit = "ms";

        // Act
        var result = _service.Convert(value, fromUnit, toUnit);

        // Assert
        Assert.Equal(1000.0, result, 2);
    }

    [Fact]
    public void Convert_NullFromUnit_ThrowsArgumentException()
    {
        // Arrange
        var value = 10.0;
        string? fromUnit = null;
        var toUnit = "Nm";

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => _service.Convert(value, fromUnit!, toUnit));
        Assert.Contains("fromUnit", ex.Message);
    }

    [Fact]
    public void Convert_NullToUnit_ThrowsArgumentException()
    {
        // Arrange
        var value = 10.0;
        var fromUnit = "Nm";
        string? toUnit = null;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => _service.Convert(value, fromUnit, toUnit!));
        Assert.Contains("toUnit", ex.Message);
    }

    [Fact]
    public void Convert_UnsupportedFromUnit_ThrowsArgumentException()
    {
        // Arrange
        var value = 10.0;
        var fromUnit = "invalid";
        var toUnit = "Nm";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.Convert(value, fromUnit, toUnit));
    }

    [Fact]
    public void Convert_UnsupportedToUnit_ThrowsArgumentException()
    {
        // Arrange
        var value = 10.0;
        var fromUnit = "Nm";
        var toUnit = "invalid";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.Convert(value, fromUnit, toUnit));
    }

    #endregion

    #region TryConvert Tests

    [Fact]
    public void TryConvert_ValidConversion_ReturnsTrue()
    {
        // Arrange
        var value = 10.0;
        var fromUnit = "Nm";
        var toUnit = "lbf-in";

        // Act
        var success = _service.TryConvert(value, fromUnit, toUnit, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(88.5075, result, 2);
    }

    [Fact]
    public void TryConvert_SameUnit_ReturnsTrueWithOriginalValue()
    {
        // Arrange
        var value = 10.0;
        var unit = "Nm";

        // Act
        var success = _service.TryConvert(value, unit, unit, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(value, result);
    }

    [Fact]
    public void TryConvert_NullFromUnit_ReturnsFalse()
    {
        // Arrange
        var value = 10.0;
        string? fromUnit = null;
        var toUnit = "Nm";

        // Act
        var success = _service.TryConvert(value, fromUnit!, toUnit, out var result);

        // Assert
        Assert.False(success);
        Assert.Equal(value, result);
    }

    [Fact]
    public void TryConvert_InvalidUnit_ReturnsFalse()
    {
        // Arrange
        var value = 10.0;
        var fromUnit = "invalid";
        var toUnit = "Nm";

        // Act
        var success = _service.TryConvert(value, fromUnit, toUnit, out var result);

        // Assert
        Assert.False(success);
        Assert.Equal(value, result);
    }

    #endregion

    #region Format Tests

    [Fact]
    public void Format_WithDefaultDecimalPlaces_FormatsCorrectly()
    {
        // Arrange
        var value = 10.5678;
        var unit = "Nm";

        // Act
        var result = _service.Format(value, unit);

        // Assert
        Assert.Equal("10.57 Nm", result);
    }

    [Fact]
    public void Format_WithCustomDecimalPlaces_FormatsCorrectly()
    {
        // Arrange
        var value = 10.5678;
        var unit = "Nm";
        var decimalPlaces = 3;

        // Act
        var result = _service.Format(value, unit, decimalPlaces);

        // Assert
        Assert.Equal("10.568 Nm", result);
    }

    [Fact]
    public void Format_WithZeroDecimalPlaces_FormatsCorrectly()
    {
        // Arrange
        var value = 10.5678;
        var unit = "Nm";
        var decimalPlaces = 0;

        // Act
        var result = _service.Format(value, unit, decimalPlaces);

        // Assert
        Assert.Equal("11 Nm", result);
    }

    [Fact]
    public void Format_NullUnit_ThrowsArgumentException()
    {
        // Arrange
        var value = 10.0;
        string? unit = null;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => _service.Format(value, unit!));
        Assert.Contains("unit", ex.Message);
    }

    [Fact]
    public void Format_NegativeDecimalPlaces_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = 10.0;
        var unit = "Nm";
        var decimalPlaces = -1;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Format(value, unit, decimalPlaces));
    }

    #endregion

    #region IsUnitSupported Tests

    [Fact]
    public void IsUnitSupported_TorqueNm_ReturnsTrue()
    {
        // Act
        var result = _service.IsUnitSupported("Nm");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsUnitSupported_TorqueLbfIn_ReturnsTrue()
    {
        // Act
        var result = _service.IsUnitSupported("lbf-in");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsUnitSupported_PowerKw_ReturnsTrue()
    {
        // Act
        var result = _service.IsUnitSupported("kW");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsUnitSupported_InvalidUnit_ReturnsFalse()
    {
        // Act
        var result = _service.IsUnitSupported("invalid");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsUnitSupported_NullUnit_ReturnsFalse()
    {
        // Act
        var result = _service.IsUnitSupported(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsUnitSupported_EmptyUnit_ReturnsFalse()
    {
        // Act
        var result = _service.IsUnitSupported("");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Supported Units Array Tests

    [Fact]
    public void SupportedTorqueUnits_ContainsExpectedUnits()
    {
        // Act
        var units = UnitService.SupportedTorqueUnits;

        // Assert
        Assert.Contains("Nm", units);
        Assert.Contains("lbf-ft", units);
        Assert.Contains("lbf-in", units);
        Assert.Contains("oz-in", units);
    }

    [Fact]
    public void SupportedPowerUnits_ContainsExpectedUnits()
    {
        // Act
        var units = UnitService.SupportedPowerUnits;

        // Assert
        Assert.Contains("kW", units);
        Assert.Contains("W", units);
        Assert.Contains("hp", units);
    }

    [Fact]
    public void SupportedWeightUnits_ContainsExpectedUnits()
    {
        // Act
        var units = UnitService.SupportedWeightUnits;

        // Assert
        Assert.Contains("kg", units);
        Assert.Contains("g", units);
        Assert.Contains("lbs", units);
        Assert.Contains("oz", units);
    }

    #endregion
}
