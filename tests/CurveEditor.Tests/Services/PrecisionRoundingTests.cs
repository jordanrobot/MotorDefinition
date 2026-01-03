using JordanRobot.MotorDefinition.Services;

namespace CurveEditor.Tests.Services;

/// <summary>
/// Tests for the PrecisionRounding utility class.
/// </summary>
public class PrecisionRoundingTests
{
    #region CorrectPrecisionError Tests

    [Fact]
    public void CorrectPrecisionError_ValueWithTinyError_CorrectsToRoundNumber()
    {
        // Arrange - simulate floating-point error near 50.13
        var value = 50.1300000000034;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(50.13, result);
    }

    [Fact]
    public void CorrectPrecisionError_ValueNear1Point5_CorrectsToExact()
    {
        // Arrange - simulate floating-point error near 1.5
        var value = 1.4999999999998;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(1.5, result);
    }

    [Fact]
    public void CorrectPrecisionError_ValueNear6Point2_CorrectsToExact()
    {
        // Arrange - simulate floating-point error near 6.2
        var value = 6.2000000000001;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(6.2, result);
    }

    [Fact]
    public void CorrectPrecisionError_ExactValue_RemainsUnchanged()
    {
        // Arrange
        var value = 10.5;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void CorrectPrecisionError_WholeNumber_RemainsUnchanged()
    {
        // Arrange
        var value = 42.0;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void CorrectPrecisionError_LegitimateDecimalValue_RemainsUnchanged()
    {
        // Arrange - value that has legitimate precision
        var value = 3.14159265;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void CorrectPrecisionError_NegativeValueWithError_CorrectsCorrectly()
    {
        // Arrange
        var value = -50.1300000000034;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(-50.13, result);
    }

    [Fact]
    public void CorrectPrecisionError_VerySmallValue_HandlesCorrectly()
    {
        // Arrange
        var value = 0.0000001 + 1e-15;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(0.0000001, result);
    }

    [Fact]
    public void CorrectPrecisionError_Zero_ReturnsZero()
    {
        // Arrange
        var value = 0.0;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void CorrectPrecisionError_ThresholdDisabled_ReturnsOriginalValue()
    {
        // Arrange
        var value = 50.1300000000034;
        var threshold = 0.0; // Disabled

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void CorrectPrecisionError_NegativeThreshold_ReturnsOriginalValue()
    {
        // Arrange
        var value = 50.1300000000034;
        var threshold = -1.0; // Disabled

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void CorrectPrecisionError_NaN_ReturnsNaN()
    {
        // Arrange
        var value = double.NaN;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.True(double.IsNaN(result));
    }

    [Fact]
    public void CorrectPrecisionError_PositiveInfinity_ReturnsInfinity()
    {
        // Arrange
        var value = double.PositiveInfinity;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.True(double.IsPositiveInfinity(result));
    }

    [Fact]
    public void CorrectPrecisionError_NegativeInfinity_ReturnsNegativeInfinity()
    {
        // Arrange
        var value = double.NegativeInfinity;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.True(double.IsNegativeInfinity(result));
    }

    [Fact]
    public void CorrectPrecisionError_DefaultThreshold_UsesCorrectValue()
    {
        // Arrange
        var value = 50.1300000000034;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value);

        // Assert
        Assert.Equal(50.13, result);
    }

    [Fact]
    public void CorrectPrecisionError_LargerThreshold_CorrectsMoreAggressively()
    {
        // Arrange - value with slightly larger error
        var value = 10.0000001;
        var threshold = 1e-6; // More aggressive threshold

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(10.0, result);
    }

    [Fact]
    public void CorrectPrecisionError_SmallerThreshold_CorrectsOnlyTinyErrors()
    {
        // Arrange - value with slightly larger error
        var value = 10.0000001;
        var threshold = 1e-10; // Very conservative threshold

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert - should NOT correct because error is too large
        Assert.Equal(value, result);
    }

    #endregion

    #region CorrectPrecisionErrors Array Tests

    [Fact]
    public void CorrectPrecisionErrors_Array_CorrectsAllValues()
    {
        // Arrange
        var values = new[]
        {
            50.1300000000034,
            1.4999999999998,
            6.2000000000001,
            10.5
        };
        var threshold = 1e-10;

        // Act
        var results = PrecisionRounding.CorrectPrecisionErrors(values, threshold);

        // Assert
        Assert.Equal(50.13, results[0]);
        Assert.Equal(1.5, results[1]);
        Assert.Equal(6.2, results[2]);
        Assert.Equal(10.5, results[3]);
    }

    [Fact]
    public void CorrectPrecisionErrors_EmptyArray_ReturnsEmptyArray()
    {
        // Arrange
        var values = Array.Empty<double>();
        var threshold = 1e-10;

        // Act
        var results = PrecisionRounding.CorrectPrecisionErrors(values, threshold);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void CorrectPrecisionErrors_NullArray_ThrowsArgumentNullException()
    {
        // Arrange
        double[]? values = null;
        var threshold = 1e-10;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            PrecisionRounding.CorrectPrecisionErrors(values!, threshold));
    }

    [Fact]
    public void CorrectPrecisionErrors_DefaultThreshold_UsesCorrectValue()
    {
        // Arrange
        var values = new[] { 50.1300000000034, 1.4999999999998 };

        // Act
        var results = PrecisionRounding.CorrectPrecisionErrors(values);

        // Assert
        Assert.Equal(50.13, results[0]);
        Assert.Equal(1.5, results[1]);
    }

    #endregion

    #region Real-World Conversion Scenarios

    [Fact]
    public void CorrectPrecisionError_TorqueConversionError_IsFixed()
    {
        // Arrange - simulates a real conversion scenario that might produce errors
        // Converting 10 Nm to lbf-in and back might introduce precision errors
        var value = 88.50749999999999; // Should be 88.5075
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(88.5075, result);
    }

    [Fact]
    public void CorrectPrecisionError_PowerConversionError_IsFixed()
    {
        // Arrange - simulates hp to kW conversion error
        var value = 0.7456998720000001; // Should be 0.745699872
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(0.745699872, result, 10);
    }

    [Fact]
    public void CorrectPrecisionError_MultipleDecimalPlaces_PreservesIntendedPrecision()
    {
        // Arrange - value with legitimate 5 decimal places plus error
        var value = 12.345670000000001;
        var threshold = 1e-10;

        // Act
        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        // Assert
        Assert.Equal(12.34567, result);
    }

    #endregion
}
