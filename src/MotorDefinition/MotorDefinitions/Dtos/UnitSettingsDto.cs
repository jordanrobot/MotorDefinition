using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinitions.Persistence.Dtos;

/// <summary>
/// Persistence DTO for unit settings in motor definition files.
/// </summary>
internal sealed class UnitSettingsDto
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

    [JsonPropertyName("torque")]
    public string Torque
    {
        get => _torque;
        set => _torque = string.IsNullOrWhiteSpace(value) ? "Nm" : value;
    }

    [JsonPropertyName("speed")]
    public string Speed
    {
        get => _speed;
        set => _speed = string.IsNullOrWhiteSpace(value) ? "rpm" : value;
    }

    [JsonPropertyName("power")]
    public string Power
    {
        get => _power;
        set => _power = string.IsNullOrWhiteSpace(value) ? "W" : value;
    }

    [JsonPropertyName("weight")]
    public string Weight
    {
        get => _weight;
        set => _weight = string.IsNullOrWhiteSpace(value) ? "kg" : value;
    }

    [JsonPropertyName("voltage")]
    public string Voltage
    {
        get => _voltage;
        set => _voltage = string.IsNullOrWhiteSpace(value) ? "V" : value;
    }

    [JsonPropertyName("current")]
    public string Current
    {
        get => _current;
        set => _current = string.IsNullOrWhiteSpace(value) ? "A" : value;
    }

    [JsonPropertyName("inertia")]
    public string Inertia
    {
        get => _inertia;
        set => _inertia = string.IsNullOrWhiteSpace(value) ? "kg-m^2" : value;
    }

    [JsonPropertyName("torqueConstant")]
    public string TorqueConstant
    {
        get => _torqueConstant;
        set => _torqueConstant = string.IsNullOrWhiteSpace(value) ? "Nm/A" : value;
    }

    [JsonPropertyName("backlash")]
    public string Backlash
    {
        get => _backlash;
        set => _backlash = string.IsNullOrWhiteSpace(value) ? "arcmin" : value;
    }

    [JsonPropertyName("responseTime")]
    public string ResponseTime
    {
        get => _responseTime;
        set => _responseTime = string.IsNullOrWhiteSpace(value) ? "ms" : value;
    }

    [JsonPropertyName("percentage")]
    public string Percentage
    {
        get => _percentage;
        set => _percentage = string.IsNullOrWhiteSpace(value) ? "%" : value;
    }

    [JsonPropertyName("temperature")]
    public string Temperature
    {
        get => _temperature;
        set => _temperature = string.IsNullOrWhiteSpace(value) ? "C" : value;
    }
}
