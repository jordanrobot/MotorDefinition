using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using JordanRobot.MotorDefinition.Services;

namespace JordanRobot.MotorDefinition.Model;

/// <summary>
/// Specifies the units used for various motor properties.
/// </summary>
public class UnitSettings : INotifyPropertyChanged
{
    private string _torque = "Nm";
    private string _speed = "rpm";
    private string _power = "W";
    private string _weight = "kg";
    private string _voltage = "V";
    private string _current = "A";
    private string _inertia = "kg-m^2";
    private string _torqueConstant = "Nm/A";
    private string _backlash = "arcmin";
    private string _responseTime = "ms";
    private string _percentage = "%";
    private string _temperature = "C";

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the torque unit label.
    /// </summary>
    /// <remarks>
    /// Supported values include "Nm", "lbf-ft", "lbf-in", and "oz-in".
    /// </remarks>
    [JsonPropertyName("torque")]
    public string Torque
    {
        get => _torque;
        set => SetProperty(ref _torque, value);
    }

    /// <summary>
    /// Gets or sets the speed unit label.
    /// </summary>
    /// <remarks>
    /// Supported values include "rpm".
    /// </remarks>
    [JsonPropertyName("speed")]
    public string Speed
    {
        get => _speed;
        set => SetProperty(ref _speed, value);
    }

    /// <summary>
    /// Gets or sets the power unit label.
    /// </summary>
    /// <remarks>
    /// Supported values include "kW", "W", and "hp".
    /// </remarks>
    [JsonPropertyName("power")]
    public string Power
    {
        get => _power;
        set => SetProperty(ref _power, value);
    }

    /// <summary>
    /// Gets or sets the weight unit label.
    /// </summary>
    /// <remarks>
    /// Supported values include "kg", "g", "lbs", and "oz".
    /// </remarks>
    [JsonPropertyName("weight")]
    public string Weight
    {
        get => _weight;
        set => SetProperty(ref _weight, value);
    }

    /// <summary>
    /// Gets or sets the voltage unit label.
    /// </summary>
    /// <remarks>
    /// Supported values include "V" and "kV".
    /// </remarks>
    [JsonPropertyName("voltage")]
    public string Voltage
    {
        get => _voltage;
        set => SetProperty(ref _voltage, value);
    }

    /// <summary>
    /// Gets or sets the current unit label.
    /// </summary>
    /// <remarks>
    /// Supported values include "A" and "mA".
    /// </remarks>
    [JsonPropertyName("current")]
    public string Current
    {
        get => _current;
        set => SetProperty(ref _current, value);
    }

    /// <summary>
    /// Gets or sets the inertia unit label.
    /// </summary>
    /// <remarks>
    /// Supported values include "kg-m^2" and "g-cm^2".
    /// </remarks>
    [JsonPropertyName("inertia")]
    public string Inertia
    {
        get => _inertia;
        set => SetProperty(ref _inertia, value);
    }

    /// <summary>
    /// Gets or sets the torque constant unit label.
    /// </summary>
    /// <remarks>
    /// Supported values include "Nm/A".
    /// </remarks>
    [JsonPropertyName("torqueConstant")]
    public string TorqueConstant
    {
        get => _torqueConstant;
        set => SetProperty(ref _torqueConstant, value);
    }

    /// <summary>
    /// Gets or sets the backlash unit label.
    /// </summary>
    /// <remarks>
    /// Supported values include "arcmin" and "arcsec".
    /// </remarks>
    [JsonPropertyName("backlash")]
    public string Backlash
    {
        get => _backlash;
        set => SetProperty(ref _backlash, value);
    }

    /// <summary>
    /// Gets or sets the response time unit label.
    /// </summary>
    /// <remarks>
    /// Supported values include "ms" and "s".
    /// </remarks>
    [JsonPropertyName("responseTime")]
    public string ResponseTime
    {
        get => _responseTime;
        set => SetProperty(ref _responseTime, value);
    }

    /// <summary>
    /// Gets or sets the percentage unit label.
    /// </summary>
    [JsonPropertyName("percentage")]
    public string Percentage
    {
        get => _percentage;
        set => SetProperty(ref _percentage, value);
    }

    /// <summary>
    /// Gets or sets the temperature unit label.
    /// </summary>
    [JsonPropertyName("temperature")]
    public string Temperature
    {
        get => _temperature;
        set => SetProperty(ref _temperature, value);
    }

    /// <summary>
    /// Sets the property value and raises PropertyChanged if the value changed.
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gets the supported speed units.
    /// </summary>
    public static string[] SupportedSpeedUnits => UnitService.SupportedSpeedUnits;

    /// <summary>
    /// Gets the supported weight units.
    /// </summary>
    public static string[] SupportedWeightUnits => UnitService.SupportedWeightUnits;

    /// <summary>
    /// Gets the supported torque units.
    /// </summary>
    public static string[] SupportedTorqueUnits => UnitService.SupportedTorqueUnits;

    /// <summary>
    /// Gets the supported power units.
    /// </summary>
    public static string[] SupportedPowerUnits => UnitService.SupportedPowerUnits;

    /// <summary>
    /// Gets the supported voltage units.
    /// </summary>
    public static string[] SupportedVoltageUnits => UnitService.SupportedVoltageUnits;

    /// <summary>
    /// Gets the supported current units.
    /// </summary>
    public static string[] SupportedCurrentUnits => UnitService.SupportedCurrentUnits;

    /// <summary>
    /// Gets the supported inertia units.
    /// </summary>
    public static string[] SupportedInertiaUnits => UnitService.SupportedInertiaUnits;

    /// <summary>
    /// Gets the supported torque constant units.
    /// </summary>
    public static string[] SupportedTorqueConstantUnits => UnitService.SupportedTorqueConstantUnits;

    /// <summary>
    /// Gets the supported backlash units.
    /// </summary>
    public static string[] SupportedBacklashUnits => UnitService.SupportedBacklashUnits;

    /// <summary>
    /// Gets the supported response time units.
    /// </summary>
    public static string[] SupportedResponseTimeUnits => UnitService.SupportedResponseTimeUnits;

    /// <summary>
    /// Gets the supported percentage units.
    /// </summary>
    public static string[] SupportedPercentageUnits => UnitService.SupportedPercentageUnits;

    /// <summary>
    /// Gets the supported temperature units.
    /// </summary>
    public static string[] SupportedTemperatureUnits => UnitService.SupportedTemperatureUnits;
}
