using JordanRobot.MotorDefinition.Services;

namespace CurveEditor.Tests.Services;

/// <summary>
/// Tests for the PrecisionRounding helper.
/// </summary>
public class PrecisionRoundingTests
{
    [Fact]
    public void CorrectPrecisionError_ValueWithinThreshold_RoundsToExpectedPlaces()
    {
        var value = 50.1300000000034;

        var result = PrecisionRounding.CorrectPrecisionError(value);

        Assert.Equal(50.13, result, 12);
    }

    [Fact]
    public void CorrectPrecisionError_ValueNearWholeNumber_RoundsToWholeNumberFirst()
    {
        var value = 9.99999999997;

        var result = PrecisionRounding.CorrectPrecisionError(value);

        Assert.Equal(10, result, 12);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.0001)]
    public void CorrectPrecisionError_NonPositiveThreshold_ReturnsOriginal(double threshold)
    {
        const double value = 12.3456789;

        var result = PrecisionRounding.CorrectPrecisionError(value, threshold);

        Assert.Equal(value, result);
    }
}
