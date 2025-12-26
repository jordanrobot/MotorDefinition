using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinitions.Model;

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
    /// Torque unit: "Nm", "lbf-ft", "lbf-in", or "oz-in".
    /// </summary>
    [JsonPropertyName("torque")]
    public string Torque
    {
        get => _torque;
        set => SetProperty(ref _torque, value);
    }

    /// <summary>
    /// Speed unit: "rpm".
    /// </summary>
    [JsonPropertyName("speed")]
    public string Speed
    {
        get => _speed;
        set => SetProperty(ref _speed, value);
    }

    /// <summary>
    /// Power unit: "kW", "W", or "hp".
    /// </summary>
    [JsonPropertyName("power")]
    public string Power
    {
        get => _power;
        set => SetProperty(ref _power, value);
    }

    /// <summary>
    /// Weight unit: "kg", "g", "lbs", or "oz".
    /// </summary>
    [JsonPropertyName("weight")]
    public string Weight
    {
        get => _weight;
        set => SetProperty(ref _weight, value);
    }

    /// <summary>
    /// Voltage unit: "V" or "kV".
    /// </summary>
    [JsonPropertyName("voltage")]
    public string Voltage
    {
        get => _voltage;
        set => SetProperty(ref _voltage, value);
    }

    /// <summary>
    /// Current unit: "A" or "mA".
    /// </summary>
    [JsonPropertyName("current")]
    public string Current
    {
        get => _current;
        set => SetProperty(ref _current, value);
    }

    /// <summary>
    /// Inertia unit: "kg-m^2" or "g-cm^2".
    /// </summary>
    [JsonPropertyName("inertia")]
    public string Inertia
    {
        get => _inertia;
        set => SetProperty(ref _inertia, value);
    }

    /// <summary>
    /// Torque constant unit: "Nm/A".
    /// </summary>
    [JsonPropertyName("torqueConstant")]
    public string TorqueConstant
    {
        get => _torqueConstant;
        set => SetProperty(ref _torqueConstant, value);
    }

    /// <summary>
    /// Backlash unit: "arcmin" or "arcsec".
    /// </summary>
    [JsonPropertyName("backlash")]
    public string Backlash
    {
        get => _backlash;
        set => SetProperty(ref _backlash, value);
    }

    /// <summary>
    /// Response time unit for brake measurements.
    /// </summary>
    [JsonPropertyName("responseTime")]
    public string ResponseTime
    {
        get => _responseTime;
        set => SetProperty(ref _responseTime, value);
    }

    /// <summary>
    /// Percentage unit label.
    /// </summary>
    [JsonPropertyName("percentage")]
    public string Percentage
    {
        get => _percentage;
        set => SetProperty(ref _percentage, value);
    }

    /// <summary>
    /// Temperature unit label.
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
    public static string[] SupportedSpeedUnits => ["rpm"];

    /// <summary>
    /// Gets the supported weight units.
    /// </summary>
    public static string[] SupportedWeightUnits => ["kg", "g", "lbs", "oz"];

    /// <summary>
    /// Gets the supported torque units.
    /// </summary>
    public static string[] SupportedTorqueUnits => ["Nm", "lbf-ft", "lbf-in", "oz-in"];

    /// <summary>
    /// Gets the supported power units.
    /// </summary>
    public static string[] SupportedPowerUnits => ["kW", "W", "hp"];

    /// <summary>
    /// Gets the supported voltage units.
    /// </summary>
    public static string[] SupportedVoltageUnits => ["V", "kV"];

    /// <summary>
    /// Gets the supported current units.
    /// </summary>
    public static string[] SupportedCurrentUnits => ["A", "mA"];

    /// <summary>
    /// Gets the supported inertia units.
    /// </summary>
    public static string[] SupportedInertiaUnits => ["kg-m^2", "g-cm^2"];

    /// <summary>
    /// Gets the supported torque constant units.
    /// </summary>
    public static string[] SupportedTorqueConstantUnits => ["Nm/A"];

    /// <summary>
    /// Gets the supported backlash units.
    /// </summary>
    public static string[] SupportedBacklashUnits => ["arcmin", "arcsec"];

    /// <summary>
    /// Gets the supported response time units.
    /// </summary>
    public static string[] SupportedResponseTimeUnits => ["ms", "s"];

    /// <summary>
    /// Gets the supported percentage units.
    /// </summary>
    public static string[] SupportedPercentageUnits => ["%"];

    /// <summary>
    /// Gets the supported temperature units.
    /// </summary>
    public static string[] SupportedTemperatureUnits => ["C"];
}
