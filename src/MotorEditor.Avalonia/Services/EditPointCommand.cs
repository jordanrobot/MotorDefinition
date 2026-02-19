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
    private readonly int? _newPercent;
    private readonly decimal? _newRpm;
    private readonly decimal? _newTorque;
    private int _oldPercent;
    private decimal _oldRpm;
    private decimal _oldTorque;

    /// <summary>
    /// Creates a new <see cref="EditPointCommand"/> for editing RPM and Torque.
    /// </summary>
    public EditPointCommand(Curve series, int index, decimal newRpm, decimal newTorque)
        : this(series, index, null, newRpm, newTorque)
    {
    }

    /// <summary>
    /// Creates a new <see cref="EditPointCommand"/> for editing Percent, RPM, and/or Torque.
    /// </summary>
    /// <param name="series">The curve series containing the point.</param>
    /// <param name="index">The index of the point to edit.</param>
    /// <param name="newPercent">The new percent value, or null to leave unchanged.</param>
    /// <param name="newRpm">The new RPM value, or null to leave unchanged.</param>
    /// <param name="newTorque">The new torque value, or null to leave unchanged.</param>
    public EditPointCommand(Curve series, int index, int? newPercent, decimal? newRpm, decimal? newTorque)
    {
        _series = series ?? throw new ArgumentNullException(nameof(series));
        _index = index;
        _newPercent = newPercent;
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
        _oldPercent = point.Percent;
        _oldRpm = point.Rpm;
        _oldTorque = point.Torque;

        if (_newPercent.HasValue)
        {
            point.Percent = _newPercent.Value;
        }
        if (_newRpm.HasValue)
        {
            point.Rpm = _newRpm.Value;
        }
        if (_newTorque.HasValue)
        {
            point.Torque = _newTorque.Value;
        }
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_index < 0 || _index >= _series.Data.Count)
        {
            throw new InvalidOperationException("Data point index is out of range.");
        }

        var point = _series.Data[_index];
        point.Percent = _oldPercent;
        point.Rpm = _oldRpm;
        point.Torque = _oldTorque;
    }
}
