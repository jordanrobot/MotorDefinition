using System;

namespace CurveEditor.Services;

/// <summary>
/// Represents a command that can be executed and later undone.
/// </summary>
public interface IUndoableCommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    void Execute();

    /// <summary>
    /// Undoes the effects of a previously executed command.
    /// </summary>
    void Undo();

    /// <summary>
    /// Optional human-readable description of the command for diagnostics or UI.
    /// </summary>
    string Description { get; }
}
