using JordanRobot.MotorDefinition.Model;
using System;
using System.Collections.Generic;

namespace CurveEditor.Services;

/// <summary>
/// Command that applies a single torque override value to multiple
/// data points across one or more <see cref="Curve"/> instances.
/// </summary>
public sealed class OverrideTorqueCellsCommand : IUndoableCommand
{
    /// <summary>
    /// Represents a single torque override target.
    /// </summary>
    public readonly struct Target
    {
        public Target(Curve series, int index, double oldTorque, double newTorque)
        {
            Series = series ?? throw new ArgumentNullException(nameof(series));
            Index = index;
            OldTorque = oldTorque;
            NewTorque = newTorque;
        }

        public Curve Series { get; }

        public int Index { get; }

        public double OldTorque { get; }

        public double NewTorque { get; }
    }

    private readonly IReadOnlyList<Target> _targets;

    /// <summary>
    /// Creates a new <see cref="OverrideTorqueCellsCommand"/>.
    /// </summary>
    /// <param name="targets">The set of cells to update.</param>
    public OverrideTorqueCellsCommand(IReadOnlyList<Target> targets)
    {
        _targets = targets ?? throw new ArgumentNullException(nameof(targets));
    }

    /// <inheritdoc />
    public string Description => "Override torque values for selected cells";

    /// <inheritdoc />
    public void Execute()
    {
        foreach (var target in _targets)
        {
            if (target.Index < 0 || target.Index >= target.Series.Data.Count)
            {
                continue;
            }

            target.Series.Data[target.Index].Torque = target.NewTorque;
        }
    }

    /// <inheritdoc />
    public void Undo()
    {
        foreach (var target in _targets)
        {
            if (target.Index < 0 || target.Index >= target.Series.Data.Count)
            {
                continue;
            }

            target.Series.Data[target.Index].Torque = target.OldTorque;
        }
    }
}
