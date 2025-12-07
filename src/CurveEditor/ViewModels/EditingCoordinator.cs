using System;
using System.Collections.Generic;
using CurveEditor.Models;

namespace CurveEditor.ViewModels;

/// <summary>
/// Coordinates editing and selection state between the chart and data table.
/// Owns the current selection context that can be shared by both views.
/// </summary>
public class EditingCoordinator
{
    /// <summary>
    /// Represents a selected data point in a specific series.
    /// </summary>
    public readonly record struct PointSelection(CurveSeries Series, int Index);

    private readonly List<PointSelection> _selectedPoints = [];

    /// <summary>
    /// Raised when the selection of points changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Gets a snapshot of the current point selection.
    /// </summary>
    public IReadOnlyList<PointSelection> SelectedPoints => _selectedPoints;

    /// <summary>
    /// Clears the current selection.
    /// </summary>
    public void ClearSelection()
    {
        if (_selectedPoints.Count == 0)
        {
            return;
        }

        _selectedPoints.Clear();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Replaces the current selection with the provided set of points.
    /// </summary>
    public void SetSelection(IEnumerable<PointSelection> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        _selectedPoints.Clear();
        _selectedPoints.AddRange(points);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}
