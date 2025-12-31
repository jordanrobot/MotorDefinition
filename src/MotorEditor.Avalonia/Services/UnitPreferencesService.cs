using System;

namespace CurveEditor.Services;

/// <summary>
/// Manages user preferences for unit settings.
/// Provides persistence for preferred units and conversion mode settings.
/// </summary>
public class UnitPreferencesService
{
    private const string TorqueUnitKey = "UnitPreferences.Torque";
    private const string SpeedUnitKey = "UnitPreferences.Speed";
    private const string PowerUnitKey = "UnitPreferences.Power";
    private const string WeightUnitKey = "UnitPreferences.Weight";
    private const string ConversionModeKey = "UnitPreferences.ConversionMode";
    private const string DecimalPlacesKey = "UnitPreferences.DecimalPlaces";

    private readonly IUserSettingsStore _settingsStore;

    /// <summary>
    /// Initializes a new instance of the UnitPreferencesService class.
    /// </summary>
    /// <param name="settingsStore">The settings store for persisting preferences.</param>
    public UnitPreferencesService(IUserSettingsStore settingsStore)
    {
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
    }

    /// <summary>
    /// Gets the preferred torque unit.
    /// </summary>
    /// <returns>The preferred torque unit string, or "Nm" if not set.</returns>
    public string GetPreferredTorqueUnit()
    {
        return _settingsStore.LoadString(TorqueUnitKey) ?? "Nm";
    }

    /// <summary>
    /// Sets the preferred torque unit.
    /// </summary>
    /// <param name="unit">The torque unit string (e.g., "Nm", "lbf-in").</param>
    public void SetPreferredTorqueUnit(string unit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);
        _settingsStore.SaveString(TorqueUnitKey, unit);
    }

    /// <summary>
    /// Gets the preferred speed unit.
    /// </summary>
    /// <returns>The preferred speed unit string, or "rpm" if not set.</returns>
    public string GetPreferredSpeedUnit()
    {
        return _settingsStore.LoadString(SpeedUnitKey) ?? "rpm";
    }

    /// <summary>
    /// Sets the preferred speed unit.
    /// </summary>
    /// <param name="unit">The speed unit string (e.g., "rpm").</param>
    public void SetPreferredSpeedUnit(string unit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);
        _settingsStore.SaveString(SpeedUnitKey, unit);
    }

    /// <summary>
    /// Gets the preferred power unit.
    /// </summary>
    /// <returns>The preferred power unit string, or "W" if not set.</returns>
    public string GetPreferredPowerUnit()
    {
        return _settingsStore.LoadString(PowerUnitKey) ?? "W";
    }

    /// <summary>
    /// Sets the preferred power unit.
    /// </summary>
    /// <param name="unit">The power unit string (e.g., "W", "kW", "hp").</param>
    public void SetPreferredPowerUnit(string unit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);
        _settingsStore.SaveString(PowerUnitKey, unit);
    }

    /// <summary>
    /// Gets the preferred weight (mass) unit.
    /// </summary>
    /// <returns>The preferred weight unit string, or "kg" if not set.</returns>
    public string GetPreferredWeightUnit()
    {
        return _settingsStore.LoadString(WeightUnitKey) ?? "kg";
    }

    /// <summary>
    /// Sets the preferred weight (mass) unit.
    /// </summary>
    /// <param name="unit">The weight unit string (e.g., "kg", "lbs").</param>
    public void SetPreferredWeightUnit(string unit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);
        _settingsStore.SaveString(WeightUnitKey, unit);
    }

    /// <summary>
    /// Gets whether to convert stored data.
    /// </summary>
    /// <returns>True if stored data should be converted; false for display-only conversion.</returns>
    public bool GetConvertStoredData()
    {
        return _settingsStore.LoadBool(ConversionModeKey, defaultValue: false);
    }

    /// <summary>
    /// Sets whether to convert stored data.
    /// </summary>
    /// <param name="convertStoredData">True to convert stored data; false for display-only conversion.</param>
    public void SetConvertStoredData(bool convertStoredData)
    {
        _settingsStore.SaveBool(ConversionModeKey, convertStoredData);
    }

    /// <summary>
    /// Gets the number of decimal places for display.
    /// </summary>
    /// <returns>The number of decimal places, or 2 if not set.</returns>
    public int GetDecimalPlaces()
    {
        return (int)_settingsStore.LoadDouble(DecimalPlacesKey, defaultValue: 2);
    }

    /// <summary>
    /// Sets the number of decimal places for display.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places (must be non-negative).</param>
    public void SetDecimalPlaces(int decimalPlaces)
    {
        if (decimalPlaces < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places must be non-negative.");
        }
        _settingsStore.SaveDouble(DecimalPlacesKey, decimalPlaces);
    }
}
