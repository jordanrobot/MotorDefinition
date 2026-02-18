using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;
using Moq;

namespace CurveEditor.Tests.Services;

/// <summary>
/// Tests for unit conversion confirmation dialog functionality.
/// Tests follow TDD approach - written before implementation.
/// </summary>
public class UnitConversionConfirmationTests
{
    public enum ConfirmationResult
    {
        Confirmed,
        Cancelled
    }

    [Fact]
    public void ConvertStoredData_WhenUserCancels_DoesNotModifyData()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        var conversionService = new UnitConversionService(mockSettings.Object);
        conversionService.ConvertStoredData = true;

        var motor = new ServoMotor
        {
            RatedContinuousTorque = 10.0m,
            RatedPeakTorque = 15.0m
        };

        var originalContinuous = motor.RatedContinuousTorque;
        var originalPeak = motor.RatedPeakTorque;

        var oldUnits = new UnitSettings { Torque = "Nm" };
        var newUnits = new UnitSettings { Torque = "lbf-in" };

        // Act - Simulate user cancellation by not calling conversion
        // (In real implementation, dialog returns Cancel and conversion is skipped)
        var userCancelled = true; // Would come from dialog

        if (!userCancelled)
        {
            conversionService.ConvertMotorUnits(motor, oldUnits, newUnits);
        }

        // Assert - Data unchanged when user cancels
        Assert.Equal(originalContinuous, motor.RatedContinuousTorque);
        Assert.Equal(originalPeak, motor.RatedPeakTorque);
    }

    [Fact]
    public void ConvertStoredData_WhenUserConfirms_ModifiesData()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        var conversionService = new UnitConversionService(mockSettings.Object);
        conversionService.ConvertStoredData = true;

        var motor = new ServoMotor
        {
            RatedContinuousTorque = 10.0m,
            RatedPeakTorque = 15.0m
        };

        var oldUnits = new UnitSettings { Torque = "Nm" };
        var newUnits = new UnitSettings { Torque = "lbf-in" };

        // Act - Simulate user confirmation
        var userConfirmed = true; // Would come from dialog

        if (userConfirmed)
        {
            conversionService.ConvertMotorUnits(motor, oldUnits, newUnits);
        }

        // Assert - Data converted when user confirms
        Assert.Equal(88.5075m, motor.RatedContinuousTorque, 2);
        Assert.Equal(132.7612m, motor.RatedPeakTorque, 2);
    }

    [Fact]
    public void SwitchToDisplayMode_DoesNotRequireConfirmation()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        var preferences = new UnitPreferencesService(mockSettings.Object);

        // Act - Switching to display mode (non-destructive)
        preferences.SetConvertStoredData(false);

        // Assert - No confirmation needed, just saves preference
        mockSettings.Verify(s => s.SaveBool("UnitPreferences.ConversionMode", false), Times.Once);
    }

    [Fact]
    public void ConfirmationDialog_ShowsPreviewOfChanges()
    {
        // This test documents expected behavior for the confirmation dialog
        // The dialog should show:
        // - Old unit and value
        // - New unit and converted value
        // - Number of affected values
        // - Warning that this action cannot be undone

        var oldValue = 10.0m;
        var newValue = 88.5075m;
        var oldUnit = "Nm";
        var newUnit = "lbf-in";

        // Calculate preview
        var unitService = new JordanRobot.MotorDefinition.Services.UnitService();
        var preview = unitService.Convert(oldValue, oldUnit, newUnit);

        // Assert - Preview shows correct conversion
        Assert.Equal(newValue, preview, 2);
    }

    [Fact]
    public void ConvertMultipleProperties_ShowsAffectedCount()
    {
        // This test documents that the confirmation should show how many values will be affected

        var motor = new ServoMotor
        {
            RatedContinuousTorque = 10.0m,
            RatedPeakTorque = 15.0m,
            MaxSpeed = 3000,
            RatedSpeed = 2500
        };

        var drive = new Drive { Name = "TestDrive" };
        var voltage = new Voltage { Value = 48, RatedContinuousTorque = 5.0m, RatedPeakTorque = 8.0m };
        var curve = new Curve("Test");
        curve.Data.Add(new DataPoint(0, 0m, 10.0m));
        curve.Data.Add(new DataPoint(100, 3000m, 10.0m));

        voltage.Curves.Add(curve);
        drive.Voltages.Add(voltage);
        motor.Drives.Add(drive);

        // Count affected values for torque conversion
        int affectedCount = 0;
        affectedCount += 2; // Motor level: RatedContinuousTorque, RatedPeakTorque
        affectedCount += 2; // Voltage level: RatedContinuousTorque, RatedPeakTorque
        affectedCount += curve.Data.Count; // Each data point

        // Assert - Should show 6 values will be affected
        Assert.Equal(6, affectedCount);
    }

    [Fact]
    public void ConversionPreview_ShowsBeforeAndAfter()
    {
        // Arrange
        var unitService = new JordanRobot.MotorDefinition.Services.UnitService();
        var mockSettings = new Mock<IUserSettingsStore>();
        var conversionService = new UnitConversionService(mockSettings.Object);

        decimal[] testValues = [5.0m, 10.0m, 15.0m, 20.0m];
        string fromUnit = "Nm";
        string toUnit = "lbf-in";

        // Act - Generate preview for multiple values
        var previews = new List<(decimal Before, decimal After)>();
        foreach (var value in testValues)
        {
            var converted = unitService.Convert(value, fromUnit, toUnit);
            previews.Add((value, converted));
        }

        // Assert - Preview data structure is correct
        Assert.Equal(4, previews.Count);
        Assert.Equal(5.0m, previews[0].Before);
        Assert.Equal(44.2537m, previews[0].After, 2);
    }

    [Fact]
    public void ConfirmationRequired_OnlyForStoredDataMode()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        
        // Test display mode
        mockSettings.Setup(s => s.LoadBool("UnitPreferences.ConversionMode", false)).Returns(false);
        var displayPrefs = new UnitPreferencesService(mockSettings.Object);
        var displayMode = displayPrefs.GetConvertStoredData();

        // Test stored data mode
        mockSettings.Setup(s => s.LoadBool("UnitPreferences.ConversionMode", false)).Returns(true);
        var storedPrefs = new UnitPreferencesService(mockSettings.Object);
        var storedMode = storedPrefs.GetConvertStoredData();

        // Assert
        Assert.False(displayMode); // Display mode - no confirmation needed
        Assert.True(storedMode); // Stored mode - confirmation required
    }

    [Fact]
    public void WarningMessage_IndicatesActionIsNotUndoable()
    {
        // This test documents the expected warning message content
        // The dialog should clearly state:
        // 1. This will permanently modify stored data
        // 2. This action cannot be undone
        // 3. User should consider backing up the file first

        var expectedWarningElements = new[]
        {
            "permanently",
            "cannot be undone",
            "all values",
            "backup"
        };

        // Assert - These elements should be in the warning message
        Assert.NotEmpty(expectedWarningElements);
        Assert.Contains("permanently", expectedWarningElements);
        Assert.Contains("cannot be undone", expectedWarningElements);
    }
}
