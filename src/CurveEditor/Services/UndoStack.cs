using System;
using System.Collections.Generic;
using Serilog;

namespace CurveEditor.Services;

/// <summary>
/// Maintains undo and redo stacks for a sequence of <see cref="IUndoableCommand"/> instances.
/// </summary>
public sealed class UndoStack
{
    private readonly Stack<IUndoableCommand> _undoStack = new();
    private readonly Stack<IUndoableCommand> _redoStack = new();

    /// <summary>
    /// Event raised whenever the undo or redo stacks change.
    /// </summary>
    public event EventHandler? UndoStackChanged;

    /// <summary>
    /// Gets a value indicating whether there is at least one command to undo.
    /// </summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>
    /// Gets a value indicating whether there is at least one command to redo.
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Gets the current number of commands that can be undone.
    /// </summary>
    public int UndoDepth => _undoStack.Count;

    /// <summary>
    /// Clears all undo and redo history.
    /// </summary>
    public void Clear()
    {
        Log.Debug("UndoStack.Clear called. Previous undo depth={UndoDepth}, redo depth={RedoDepth}", _undoStack.Count, _redoStack.Count);
        _undoStack.Clear();
        _redoStack.Clear();
        OnUndoStackChanged();
    }

    /// <summary>
    /// Executes the specified command and records it on the undo stack.
    /// Any existing redo history is cleared.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    public void PushAndExecute(IUndoableCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            Log.Debug("UndoStack: Executing and pushing command '{Description}'", command.Description);
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
            OnUndoStackChanged();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to execute undoable command {Description}", command.Description);
            throw;
        }
    }

    /// <summary>
    /// Undoes the most recently executed command, moving it to the redo stack.
    /// </summary>
    public void Undo()
    {
        if (!CanUndo)
        {
            Log.Debug("UndoStack.Undo called but CanUndo is false.");
            return;
        }

        var command = _undoStack.Pop();

        try
        {
            Log.Debug("UndoStack: Undoing command '{Description}'", command.Description);
            command.Undo();
            _redoStack.Push(command);
            OnUndoStackChanged();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to undo command {Description}", command.Description);
            _undoStack.Push(command);
            throw;
        }
    }

    /// <summary>
    /// Re-executes the most recently undone command, moving it back to the undo stack.
    /// </summary>
    public void Redo()
    {
        if (!CanRedo)
        {
            Log.Debug("UndoStack.Redo called but CanRedo is false.");
            return;
        }

        var command = _redoStack.Pop();

        try
        {
            Log.Debug("UndoStack: Redoing command '{Description}'", command.Description);
            command.Execute();
            _undoStack.Push(command);
            OnUndoStackChanged();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to redo command {Description}", command.Description);
            _redoStack.Push(command);
            throw;
        }
    }

    private void OnUndoStackChanged()
    {
        UndoStackChanged?.Invoke(this, EventArgs.Empty);
    }
}
