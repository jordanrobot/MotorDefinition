using JordanRobot.MotorDefinition.Model;
using System;
using System.Reflection;

namespace CurveEditor.Services;

/// <summary>
/// Command that edits a scalar property directly on <see cref="ServoMotor"/>.
/// </summary>
public sealed class EditMotorPropertyCommand : IUndoableCommand
{
    private readonly ServoMotor _motor;
    private readonly PropertyInfo _property;
    private readonly object? _newValue;
    private readonly object? _oldValue;

    /// <summary>
    /// Creates a new <see cref="EditMotorPropertyCommand"/>.
    /// </summary>
    public EditMotorPropertyCommand(ServoMotor motor, string propertyName, object? oldValue, object? newValue)
    {
        _motor = motor ?? throw new ArgumentNullException(nameof(motor));

        _property = motor.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new ArgumentException($"Property '{propertyName}' was not found on ServoMotor.", nameof(propertyName));

        if (!_property.CanRead || !_property.CanWrite)
        {
            throw new ArgumentException($"Property '{propertyName}' must be readable and writable.", nameof(propertyName));
        }

        _oldValue = oldValue;
        _newValue = newValue;
    }

    /// <inheritdoc />
    public string Description => $"Edit motor property '{_property.Name}'";

    /// <inheritdoc />
    public void Execute()
    {
        _property.SetValue(_motor, _newValue);
    }

    /// <inheritdoc />
    public void Undo()
    {
        _property.SetValue(_motor, _oldValue);
    }
}

/// <summary>
/// Command that edits a scalar property directly on <see cref="Drive"/>.
/// </summary>
public sealed class EditDrivePropertyCommand : IUndoableCommand
{
    private readonly Drive _drive;
    private readonly PropertyInfo _property;
    private readonly object? _newValue;
    private readonly object? _oldValue;

    public EditDrivePropertyCommand(Drive drive, string propertyName, object? oldValue, object? newValue)
    {
        _drive = drive ?? throw new ArgumentNullException(nameof(drive));

        _property = drive.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new ArgumentException($"Property '{propertyName}' was not found on Drive.", nameof(propertyName));

        if (!_property.CanRead || !_property.CanWrite)
        {
            throw new ArgumentException($"Property '{propertyName}' must be readable and writable.", nameof(propertyName));
        }

        _oldValue = oldValue;
        _newValue = newValue;
    }

    public string Description => $"Edit drive property '{_property.Name}'";

    public void Execute()
    {
        _property.SetValue(_drive, _newValue);
    }

    public void Undo()
    {
        _property.SetValue(_drive, _oldValue);
    }
}

/// <summary>
/// Command that edits a scalar property directly on <see cref="Voltage"/>.
/// </summary>
public sealed class EditVoltagePropertyCommand : IUndoableCommand
{
    private readonly Voltage _voltage;
    private readonly PropertyInfo _property;
    private readonly object? _newValue;
    private readonly object? _oldValue;

    public EditVoltagePropertyCommand(Voltage voltage, string propertyName, object? oldValue, object? newValue)
    {
        _voltage = voltage ?? throw new ArgumentNullException(nameof(voltage));

        _property = voltage.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new ArgumentException($"Property '{propertyName}' was not found on Voltage.", nameof(propertyName));

        if (!_property.CanRead || !_property.CanWrite)
        {
            throw new ArgumentException($"Property '{propertyName}' must be readable and writable.", nameof(propertyName));
        }

        _oldValue = oldValue;
        _newValue = newValue;
    }

    public string Description => $"Edit voltage property '{_property.Name}'";

    public void Execute()
    {
        _property.SetValue(_voltage, _newValue);
    }

    public void Undo()
    {
        _property.SetValue(_voltage, _oldValue);
    }
}
