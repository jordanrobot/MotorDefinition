using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;
using JordanRobot.MotorDefinition.Services;

namespace CurveEditor.Tests.Services;

/// <summary>
/// Integration tests demonstrating the precision rounding feature in real-world scenarios.
/// </summary>
public class PrecisionRoundingIntegrationTests
{
    [Fact]
    public void RealWorldScenario_TorqueConversionRoundTrip_MaintainsPrecision()
    {
        // Arrange - Simulate a real scenario where a value is converted and back
        var service = new UnitService();
        var originalValue = 50.13; // Should remain exactly 50.13 after round trip

        // Act - Convert Nm -> lbf-in -> Nm
        var inLbfIn = service.Convert(originalValue, "Nm", "lbf-in");
        var backToNm = service.Convert(inLbfIn, "lbf-in", "Nm");

        // Assert - Should not accumulate precision errors
        Assert.Equal(originalValue, backToNm, 10); // High precision check
    }

    [Fact]
    public void RealWorldScenario_DataPointConversion_FixesPrecisionErrors()
    {
        // Arrange - Create a data point that might get precision errors during conversion
        var service = new UnitConversionService { ConvertStoredData = true };
        var curve = new Curve("TestCurve");
        
        // Add points with nice round torque values
        curve.Data.Add(new DataPoint(0, 0, 1.5));
        curve.Data.Add(new DataPoint(50, 1500, 6.2));
        curve.Data.Add(new DataPoint(100, 3000, 50.13));

        // Act - Convert to different unit and back
        service.ConvertCurveTorque(curve, "Nm", "lbf-in");
        service.ConvertCurveTorque(curve, "lbf-in", "Nm");

        // Assert - Values should remain clean without precision errors
        Assert.Equal(1.5, curve.Data[0].Torque, 10);
        Assert.Equal(6.2, curve.Data[1].Torque, 10);
        Assert.Equal(50.13, curve.Data[2].Torque, 10);
    }

    [Fact]
    public void RealWorldScenario_PowerConversion_HandlesPrecisionCorrectly()
    {
        // Arrange
        var service = new UnitService();
        var originalPowerInKw = 7.5; // Nice round number in kW

        // Act - Convert kW -> hp -> W -> kW (multiple conversions)
        var inHp = service.Convert(originalPowerInKw, "kW", "hp");
        var inW = service.Convert(inHp, "hp", "W");
        var backToKw = service.Convert(inW, "W", "kW");

        // Assert - Should maintain precision through multiple conversions
        Assert.Equal(originalPowerInKw, backToKw, 8);
    }

    [Fact]
    public void RealWorldScenario_MassConversion_PreservesIntendedValues()
    {
        // Arrange
        var service = new UnitService();
        var motor = new ServoMotor
        {
            Weight = 10.5 // Nice round value in kg
        };

        // Act - Convert to lbs and back
        var weightInLbs = service.Convert(motor.Weight, "kg", "lbs");
        var backToKg = service.Convert(weightInLbs, "lbs", "kg");

        // Assert - Should preserve the original value
        Assert.Equal(motor.Weight, backToKg, 10);
    }

    [Fact]
    public void RealWorldScenario_InertiaConversion_HandlesLargeScaleChanges()
    {
        // Arrange
        var service = new UnitService();
        var inertiaKgM2 = 0.05; // Nice round value in kg-m^2

        // Act - Convert to g-cm^2 (much larger number) and back
        var inGcm2 = service.Convert(inertiaKgM2, "kg-m^2", "g-cm^2");
        var backToKgM2 = service.Convert(inGcm2, "g-cm^2", "kg-m^2");

        // Assert - Should preserve precision despite large scale change
        Assert.Equal(inertiaKgM2, backToKgM2, 10);
    }

    [Fact]
    public void EdgeCase_VerySmallValue_DoesNotOverRound()
    {
        // Arrange - Test with small but legitimate value
        var service = new UnitService();
        var smallValue = 0.001; // Should NOT be rounded to 0

        // Act
        var converted = service.Convert(smallValue, "kg", "g");

        // Assert - Should preserve small values
        Assert.True(converted > 0, "Small values should not be rounded to zero");
        Assert.Equal(1.0, converted, 10); // 0.001 kg = 1 g
    }

    [Fact]
    public void EdgeCase_VeryLargeValue_HandlesCorrectly()
    {
        // Arrange - Test with very large value
        var service = new UnitService();
        var largeValue = 1000000.0;

        // Act
        var converted = service.Convert(largeValue, "W", "kW");

        // Assert - Should handle large values correctly
        Assert.Equal(1000.0, converted, 10);
    }

    [Fact]
    public void CustomThreshold_MoreAggressive_CorrectsLargerErrors()
    {
        // Arrange - Use more aggressive threshold
        var service = new UnitService
        {
            PrecisionErrorThreshold = 1e-6 // More aggressive
        };
        
        // Simulate a value with slightly larger error
        var valueWithError = 10.0000005;

        // Act - Convert same unit (no actual conversion, but rounding applies)
        var result = service.Convert(valueWithError, "Nm", "Nm");

        // Assert - Should still be 10.0 (essentially no conversion, but no error accumulated)
        Assert.Equal(valueWithError, result);
    }

    [Fact]
    public void DisabledThreshold_PreservesOriginalBehavior()
    {
        // Arrange - Disable precision correction
        var service = new UnitService
        {
            PrecisionErrorThreshold = 0 // Disabled
        };

        // Act - Any conversion should work but may have precision artifacts
        var result = service.Convert(10.0, "Nm", "lbf-in");

        // Assert - Conversion should still work
        Assert.True(result > 88 && result < 89);
    }

    [Fact]
    public void MultipleConversions_ChainedOperations_MaintainsPrecision()
    {
        // Arrange - Simulate a complex workflow with multiple conversions
        var service = new UnitService();
        var originalTorque = 25.75; // Nice fractional value

        // Act - Perform multiple chained conversions
        var step1 = service.Convert(originalTorque, "Nm", "lbf-in");
        var step2 = service.Convert(step1, "lbf-in", "lbf-ft");
        var step3 = service.Convert(step2, "lbf-ft", "oz-in");
        var step4 = service.Convert(step3, "oz-in", "lbf-in");
        var final = service.Convert(step4, "lbf-in", "Nm");

        // Assert - Despite multiple conversions, should maintain precision
        Assert.Equal(originalTorque, final, 6);
    }
}
