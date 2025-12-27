using JordanRobot.MotorDefinition.Model;
using System;

namespace CurveEditor.Services;

/// <summary>
/// Command that edits a single data point within a <see cref="Curve"/>.
/// </summary>
public sealed class EditPointCommand : IUndoableCommand
{
    private readonly Curve _series;
    private readonly int _index;
    private readonly double _newRpm;
    private readonly double _newTorque;
    private double _oldRpm;
    private double _oldTorque;

    /// <summary>
    /// Creates a new <see cref="EditPointCommand"/>.
    /// </summary>
    public EditPointCommand(Curve series, int index, double newRpm, double newTorque)
    {
        _series = series ?? throw new ArgumentNullException(nameof(series));
        _index = index;
        _newRpm = newRpm;
        _newTorque = newTorque;
    }

    /// <inheritdoc />
    public string Description => $"Edit point {_index} in series '{_series.Name}'";

    /// <inheritdoc />
    public void Execute()
    {
        if (_index < 0 || _index >= _series.Data.Count)
        {
            throw new InvalidOperationException("Data point index is out of range.");
        }

        var point = _series.Data[_index];
        _oldRpm = point.Rpm;
        _oldTorque = point.Torque;
        point.Rpm = _newRpm;
        point.Torque = _newTorque;
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_index < 0 || _index >= _series.Data.Count)
        {
            throw new InvalidOperationException("Data point index is out of range.");
        }

        var point = _series.Data[_index];
        point.Rpm = _oldRpm;
        point.Torque = _oldTorque;
    }
}
