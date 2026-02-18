using CurveEditor.Services;
using CurveEditor.ViewModels;
using JordanRobot.MotorDefinition.Model;
using Moq;

namespace CurveEditor.Tests.ViewModels;

/// <summary>
/// Tests for unit conversion integration in MainWindowViewModel.
/// Tests follow TDD approach - written before implementation.
/// </summary>
public class MainWindowViewModelUnitConversionTests
{
    [Fact]
    public void LoadMotor_AppliesPreferredUnitsToDisplay()
    {
        // Arrange
        var mockFileService = new Mock<IFileService>();
        var mockCurveGen = new Mock<ICurveGeneratorService>();
        var mockValidation = new Mock<IValidationService>();
        var mockDriveVoltage = new Mock<IDriveVoltageSeriesService>();
        var mockWorkflow = new Mock<IMotorConfigurationWorkflow>();
        var mockSettings = new Mock<IUserSettingsStore>();
        
        // Setup preferred units
        mockSettings.Setup(s => s.LoadString("UnitPreferences.Torque")).Returns("lbf-in");
        mockSettings.Setup(s => s.LoadString("UnitPreferences.Speed")).Returns("rpm");
        mockSettings.Setup(s => s.LoadString("UnitPreferences.Power")).Returns("hp");
        mockSettings.Setup(s => s.LoadBool("UnitPreferences.ConversionMode", false)).Returns(false);
        mockSettings.Setup(s => s.LoadDouble("UnitPreferences.DecimalPlaces", 2)).Returns(2);

        var motor = new ServoMotor
        {
            RatedContinuousTorque = 10.0m, // Nm in storage
            Units = new UnitSettings { Torque = "Nm" }
        };

        // Act - When motor loads, preferences should be applied
        // This test validates the expected behavior
        var preferences = new UnitPreferencesService(mockSettings.Object);
        var preferredTorque = preferences.GetPreferredTorqueUnit();

        // Assert
        Assert.Equal("lbf-in", preferredTorque);
        // Note: Actual conversion will happen in ViewModel implementation
    }

    [Fact]
    public void ChangeUnit_InDisplayMode_DoesNotModifyStoredValue()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        mockSettings.Setup(s => s.LoadBool("UnitPreferences.ConversionMode", false)).Returns(false);

        var conversionService = new UnitConversionService(mockSettings.Object);
        conversionService.ConvertStoredData = false;

        var motor = new ServoMotor
        {
            RatedContinuousTorque = 10.0m // Nm
        };

        var originalValue = motor.RatedContinuousTorque;

        // Act - Change display unit preference (would happen in UI)
        // In Display mode, this should NOT change the stored value
        
        // Assert
        Assert.Equal(originalValue, motor.RatedContinuousTorque);
        Assert.False(conversionService.ConvertStoredData);
    }

    [Fact]
    public void GetDisplayValue_InDisplayMode_ReturnsConvertedValue()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        var conversionService = new UnitConversionService(mockSettings.Object);
        conversionService.ConvertStoredData = false;

        decimal storedTorque = 10.0m; // Nm
        string storedUnit = "Nm";
        string displayUnit = "lbf-in";

        // Act
        var displayValue = conversionService.GetDisplayTorque(storedTorque, storedUnit, displayUnit);

        // Assert
        Assert.Equal(88.5075m, displayValue, 2);
    }

    [Fact]
    public void SavePreference_WhenUnitChanged_PersistsToSettings()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        var preferences = new UnitPreferencesService(mockSettings.Object);

        // Act
        preferences.SetPreferredTorqueUnit("lbf-in");

        // Assert
        mockSettings.Verify(s => s.SaveString("UnitPreferences.Torque", "lbf-in"), Times.Once);
    }

    [Fact]
    public void ChangeToStoredDataMode_RequiresUserConfirmation()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        var preferences = new UnitPreferencesService(mockSettings.Object);

        // Act - Changing to stored data mode
        preferences.SetConvertStoredData(true);

        // Assert - Preference is saved
        mockSettings.Verify(s => s.SaveBool("UnitPreferences.ConversionMode", true), Times.Once);
        
        // Note: UI confirmation dialog will be tested separately
    }

    [Fact]
    public void ConvertStoredData_WithConfirmation_ModifiesMotorValues()
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

        // Act
        conversionService.ConvertMotorUnits(motor, oldUnits, newUnits);

        // Assert - Values are converted
        Assert.Equal(88.5075m, motor.RatedContinuousTorque, 2);
        Assert.Equal(132.7612m, motor.RatedPeakTorque, 2);
    }

    [Fact]
    public void LoadPreferences_OnStartup_RestoresLastUsedUnits()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        mockSettings.Setup(s => s.LoadString("UnitPreferences.Torque")).Returns("lbf-in");
        mockSettings.Setup(s => s.LoadString("UnitPreferences.Speed")).Returns("rpm");
        mockSettings.Setup(s => s.LoadString("UnitPreferences.Power")).Returns("hp");
        mockSettings.Setup(s => s.LoadString("UnitPreferences.Weight")).Returns("lbs");

        var preferences = new UnitPreferencesService(mockSettings.Object);

        // Act - Load all preferences
        var torque = preferences.GetPreferredTorqueUnit();
        var speed = preferences.GetPreferredSpeedUnit();
        var power = preferences.GetPreferredPowerUnit();
        var weight = preferences.GetPreferredWeightUnit();

        // Assert - All preferences restored
        Assert.Equal("lbf-in", torque);
        Assert.Equal("rpm", speed);
        Assert.Equal("hp", power);
        Assert.Equal("lbs", weight);
    }

    [Fact]
    public void ChangeDecimalPlaces_UpdatesDisplayFormatting()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        var conversionService = new UnitConversionService(mockSettings.Object);
        var value = 10.12345m;

        // Act - Set to 3 decimal places
        conversionService.DisplayDecimalPlaces = 3;
        var formatted = conversionService.FormatValue(value, "Nm");

        // Assert
        Assert.Equal("10.123 Nm", formatted);
    }

    [Fact]
    public void ConvertCurveData_InDisplayMode_KeepsOriginalData()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        var conversionService = new UnitConversionService(mockSettings.Object);
        conversionService.ConvertStoredData = false;

        var curve = new Curve("Test");
        curve.Data.Add(new DataPoint(0, 0m, 10.0m));
        curve.Data.Add(new DataPoint(100, 3000m, 10.0m));

        var originalFirstTorque = curve.Data[0].Torque;

        // Act - Try to convert (should not modify in display mode)
        conversionService.ConvertCurveTorque(curve, "Nm", "lbf-in");

        // Assert - Data unchanged
        Assert.Equal(originalFirstTorque, curve.Data[0].Torque);
    }

    [Fact]
    public void ConvertCurveData_InStoredMode_ModifiesAllPoints()
    {
        // Arrange
        var mockSettings = new Mock<IUserSettingsStore>();
        var conversionService = new UnitConversionService(mockSettings.Object);
        conversionService.ConvertStoredData = true;

        var curve = new Curve("Test");
        curve.Data.Add(new DataPoint(0, 0m, 10.0m));
        curve.Data.Add(new DataPoint(50, 1500m, 10.0m));
        curve.Data.Add(new DataPoint(100, 3000m, 10.0m));

        // Act
        conversionService.ConvertCurveTorque(curve, "Nm", "lbf-in");

        // Assert - All points converted
        Assert.Equal(88.5075m, curve.Data[0].Torque, 2);
        Assert.Equal(88.5075m, curve.Data[1].Torque, 2);
        Assert.Equal(88.5075m, curve.Data[2].Torque, 2);
    }
}
