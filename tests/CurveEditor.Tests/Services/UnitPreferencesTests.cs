using CurveEditor.Services;
using Moq;

namespace CurveEditor.Tests.Services;

/// <summary>
/// Tests for unit preference persistence functionality.
/// </summary>
public class UnitPreferencesTests
{
    private const string TorqueUnitKey = "UnitPreferences.Torque";
    private const string SpeedUnitKey = "UnitPreferences.Speed";
    private const string PowerUnitKey = "UnitPreferences.Power";
    private const string ConversionModeKey = "UnitPreferences.ConversionMode";
    private const string DecimalPlacesKey = "UnitPreferences.DecimalPlaces";

    [Fact]
    public void SaveTorqueUnit_StoresInSettingsStore()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        preferences.SetPreferredTorqueUnit("lbf-in");

        // Assert
        mockStore.Verify(s => s.SaveString(TorqueUnitKey, "lbf-in"), Times.Once);
    }

    [Fact]
    public void LoadTorqueUnit_ReturnsStoredValue()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        mockStore.Setup(s => s.LoadString(TorqueUnitKey)).Returns("lbf-in");
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        var unit = preferences.GetPreferredTorqueUnit();

        // Assert
        Assert.Equal("lbf-in", unit);
    }

    [Fact]
    public void LoadTorqueUnit_WhenNotSet_ReturnsDefaultNm()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        mockStore.Setup(s => s.LoadString(TorqueUnitKey)).Returns((string?)null);
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        var unit = preferences.GetPreferredTorqueUnit();

        // Assert
        Assert.Equal("Nm", unit);
    }

    [Fact]
    public void SaveSpeedUnit_StoresInSettingsStore()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        preferences.SetPreferredSpeedUnit("rpm");

        // Assert
        mockStore.Verify(s => s.SaveString(SpeedUnitKey, "rpm"), Times.Once);
    }

    [Fact]
    public void SavePowerUnit_StoresInSettingsStore()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        preferences.SetPreferredPowerUnit("hp");

        // Assert
        mockStore.Verify(s => s.SaveString(PowerUnitKey, "hp"), Times.Once);
    }

    [Fact]
    public void SaveConversionMode_ConvertOnDisplay_StoresFalse()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        preferences.SetConvertStoredData(false);

        // Assert
        mockStore.Verify(s => s.SaveBool(ConversionModeKey, false), Times.Once);
    }

    [Fact]
    public void SaveConversionMode_ConvertStoredData_StoresTrue()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        preferences.SetConvertStoredData(true);

        // Assert
        mockStore.Verify(s => s.SaveBool(ConversionModeKey, true), Times.Once);
    }

    [Fact]
    public void LoadConversionMode_ReturnsStoredValue()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        mockStore.Setup(s => s.LoadBool(ConversionModeKey, false)).Returns(true);
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        var mode = preferences.GetConvertStoredData();

        // Assert
        Assert.True(mode);
    }

    [Fact]
    public void LoadConversionMode_WhenNotSet_ReturnsFalseDefault()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        mockStore.Setup(s => s.LoadBool(ConversionModeKey, false)).Returns(false);
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        var mode = preferences.GetConvertStoredData();

        // Assert
        Assert.False(mode);
    }

    [Fact]
    public void SaveDecimalPlaces_StoresInSettingsStore()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        preferences.SetDecimalPlaces(3);

        // Assert
        mockStore.Verify(s => s.SaveDouble(DecimalPlacesKey, 3), Times.Once);
    }

    [Fact]
    public void LoadDecimalPlaces_ReturnsStoredValue()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        mockStore.Setup(s => s.LoadDouble(DecimalPlacesKey, 2)).Returns(3);
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        var decimalPlaces = preferences.GetDecimalPlaces();

        // Assert
        Assert.Equal(3, decimalPlaces);
    }

    [Fact]
    public void LoadDecimalPlaces_WhenNotSet_ReturnsDefault2()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        mockStore.Setup(s => s.LoadDouble(DecimalPlacesKey, 2)).Returns(2);
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        var decimalPlaces = preferences.GetDecimalPlaces();

        // Assert
        Assert.Equal(2, decimalPlaces);
    }

    [Fact]
    public void LoadAllPreferences_ReturnsCompletePreferences()
    {
        // Arrange
        var mockStore = new Mock<IUserSettingsStore>();
        mockStore.Setup(s => s.LoadString(TorqueUnitKey)).Returns("lbf-in");
        mockStore.Setup(s => s.LoadString(SpeedUnitKey)).Returns("rpm");
        mockStore.Setup(s => s.LoadString(PowerUnitKey)).Returns("hp");
        mockStore.Setup(s => s.LoadBool(ConversionModeKey, false)).Returns(true);
        mockStore.Setup(s => s.LoadDouble(DecimalPlacesKey, 2)).Returns(3);
        var preferences = new UnitPreferencesService(mockStore.Object);

        // Act
        var torque = preferences.GetPreferredTorqueUnit();
        var speed = preferences.GetPreferredSpeedUnit();
        var power = preferences.GetPreferredPowerUnit();
        var mode = preferences.GetConvertStoredData();
        var decimals = preferences.GetDecimalPlaces();

        // Assert
        Assert.Equal("lbf-in", torque);
        Assert.Equal("rpm", speed);
        Assert.Equal("hp", power);
        Assert.True(mode);
        Assert.Equal(3, decimals);
    }
}
