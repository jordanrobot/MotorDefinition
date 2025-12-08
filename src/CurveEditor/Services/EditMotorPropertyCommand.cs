using System;
using System.Reflection;
using CurveEditor.Models;

namespace CurveEditor.Services;

/// <summary>
/// Command that edits a scalar property directly on <see cref="MotorDefinition"/>.
/// </summary>
public sealed class EditMotorPropertyCommand : IUndoableCommand
{
    private readonly MotorDefinition _motor;
    private readonly PropertyInfo _property;
    private readonly object? _newValue;
    private readonly object? _oldValue;

    /// <summary>
    /// Creates a new <see cref="EditMotorPropertyCommand"/>.
    /// </summary>
    public EditMotorPropertyCommand(MotorDefinition motor, string propertyName, object? oldValue, object? newValue)
    {
        _motor = motor ?? throw new ArgumentNullException(nameof(motor));

        _property = motor.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new ArgumentException($"Property '{propertyName}' was not found on MotorDefinition.", nameof(propertyName));

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
