using JordanRobot.MotorDefinition.Model;
using System;

namespace CurveEditor.Services;

/// <summary>
/// Command that changes simple properties on a <see cref="Curve"/>.
/// Currently supports renaming and locked-state changes.
/// </summary>
public sealed class EditSeriesCommand : IUndoableCommand
{
    private readonly Curve _series;
    private readonly string? _newName;
    private readonly bool? _newLocked;
    private string? _oldName;
    private bool _oldLocked;

    /// <summary>
    /// Creates a new <see cref="EditSeriesCommand"/>.
    /// </summary>
    public EditSeriesCommand(Curve series, string? newName = null, bool? newLocked = null)
    {
        _series = series ?? throw new ArgumentNullException(nameof(series));
        _newName = newName;
        _newLocked = newLocked;
    }

    /// <inheritdoc />
    public string Description => $"Edit series '{_series.Name}'";

    /// <inheritdoc />
    public void Execute()
    {
        _oldName = _series.Name;
        _oldLocked = _series.Locked;

        if (_newName is not null)
        {
            _series.Name = _newName;
        }

        if (_newLocked.HasValue)
        {
            _series.Locked = _newLocked.Value;
        }
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_oldName is not null)
        {
            _series.Name = _oldName;
        }

        _series.Locked = _oldLocked;
    }
}
