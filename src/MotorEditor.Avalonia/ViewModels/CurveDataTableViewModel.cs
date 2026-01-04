using CommunityToolkit.Mvvm.ComponentModel;
using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace CurveEditor.ViewModels;

/// <summary>
/// Represents a cell position in the data table grid.
/// </summary>
public readonly record struct CellPosition(int RowIndex, int ColumnIndex)
{
    /// <summary>
    /// Compares two cell positions for ordering.
    /// </summary>
    public static int Compare(CellPosition a, CellPosition b)
    {
        var rowCompare = a.RowIndex.CompareTo(b.RowIndex);
        return rowCompare != 0 ? rowCompare : a.ColumnIndex.CompareTo(b.ColumnIndex);
    }
}

/// <summary>
/// Represents a single row in the curve data table, containing speed info and torque values for all series.
/// </summary>
public class CurveDataRow : INotifyPropertyChanged
{
    private readonly Voltage _voltage;
    private readonly int _rowIndex;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Creates a new CurveDataRow for the specified row index.
    /// </summary>
    public CurveDataRow(Voltage voltage, int rowIndex)
    {
        _voltage = voltage ?? throw new ArgumentNullException(nameof(voltage));
        _rowIndex = rowIndex;
    }

    /// <summary>
    /// Gets the percentage value for this row based on the first series.
    /// </summary>
    public int Percent
    {
        get
        {
            var firstSeries = _voltage.Curves.FirstOrDefault();
            if (firstSeries is not null && _rowIndex < firstSeries.Data.Count)
            {
                return firstSeries.Data[_rowIndex].Percent;
            }

            return 0;
        }
    }

    /// <summary>
    /// Gets the RPM value for this row based on the first series.
    /// </summary>
    public int DisplayRpm
    {
        get
        {
            var firstSeries = _voltage.Curves.FirstOrDefault();
            if (firstSeries is not null && _rowIndex < firstSeries.Data.Count)
            {
                return firstSeries.Data[_rowIndex].DisplayRpm;
            }
            return 0;
        }
    }

    /// <summary>
    /// Indexer to get/set torque values by series name (for data binding).
    /// </summary>
    public double this[string seriesName]
    {
        get => GetTorque(seriesName);
        set => SetTorque(seriesName, value);
    }

    /// <summary>
    /// Gets the torque value for a specific series at this row.
    /// </summary>
    public double GetTorque(string seriesName)
    {
        var series = _voltage.Curves.FirstOrDefault(s => s.Name == seriesName);
        if (series is not null && _rowIndex < series.Data.Count)
        {
            return series.Data[_rowIndex].Torque;
        }
        return 0;
    }

    /// <summary>
    /// Sets the torque value for a specific series at this row.
    /// </summary>
    public void SetTorque(string seriesName, double value)
    {
        var series = _voltage.Curves.FirstOrDefault(s => s.Name == seriesName);
        if (series is not null && _rowIndex < series.Data.Count)
        {
            series.Data[_rowIndex].Torque = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{seriesName}]"));
        }
    }

    /// <summary>
    /// Gets the row index.
    /// </summary>
    public int RowIndex => _rowIndex;
}

/// <summary>
/// ViewModel for the curve data table that shows all series as columns.
/// </summary>
public partial class CurveDataTableViewModel : ViewModelBase
{
    private Voltage? _currentVoltage;
    private CellPosition? _anchorCell;

    [ObservableProperty]
    private ObservableCollection<CurveDataRow> _rows = [];

    [ObservableProperty]
    private ObservableCollection<Curve> _seriesColumns = [];

    [ObservableProperty]
    private CurveDataRow? _selectedRow;

    [ObservableProperty]
    private Curve? _selectedSeries;

    [ObservableProperty]
    private int _selectedRowIndex = -1;

    [ObservableProperty]
    private string? _selectedSeriesName;

    private UndoStack? _undoStack;

    /// <summary>
    /// Optional undo stack associated with the active document. When set,
    /// torque edits are executed via commands so they can be undone.
    /// </summary>
    public UndoStack? UndoStack
    {
        get => _undoStack;
        set => _undoStack = value;
    }

    /// <summary>
    /// Optional editing coordinator used to share logical point selection
    /// with other views such as the chart.
    /// </summary>
    public EditingCoordinator? EditingCoordinator
    {
        get => _editingCoordinator;
        set
        {
            if (ReferenceEquals(_editingCoordinator, value))
            {
                return;
            }

            if (_editingCoordinator is not null)
            {
                _editingCoordinator.SelectionChanged -= OnCoordinatorSelectionChanged;
            }

            _editingCoordinator = value;

            if (_editingCoordinator is not null)
            {
                _editingCoordinator.SelectionChanged += OnCoordinatorSelectionChanged;
            }
        }
    }

    private EditingCoordinator? _editingCoordinator;

    /// <summary>
    /// Collection of currently selected cells.
    /// </summary>
    public HashSet<CellPosition> SelectedCells { get; } = [];

    /// <summary>
    /// Event raised when data changes.
    /// </summary>
    public event EventHandler? DataChanged;

    /// <summary>
    /// Event raised when cell selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Event raised when an invalid clipboard paste is attempted.
    /// The string parameter contains a human-readable error message
    /// that can be surfaced by the UI layer.
    /// </summary>
    public event EventHandler<string>? ClipboardError
    {
        add { }
        remove { }
    }

    /// <summary>
    /// Gets or sets the current voltage configuration.
    /// </summary>
    public Voltage? CurrentVoltage
    {
        get => _currentVoltage;
        set
        {
            if (_currentVoltage == value) return;
            _currentVoltage = value;
            OnPropertyChanged();
            RefreshData();
        }
    }

    /// <summary>
    /// Applies a scalar torque value to all currently selected cells.
    /// </summary>
    /// <param name="value">The torque value to apply.</param>
    public void ApplyTorqueToSelectedCells(double value)
    {
        if (SelectedCells.Count == 0)
        {
            return;
        }

        ApplyTorqueToCells(SelectedCells, value);
    }

    /// <summary>
    /// Refreshes the data table with current voltage configuration data.
    /// </summary>
    public void RefreshData()
    {
        Rows.Clear();
        SeriesColumns.Clear();

        if (_currentVoltage is null || _currentVoltage.Curves.Count == 0)
        {
            return;
        }

        // Add series columns
        foreach (var series in _currentVoltage.Curves)
        {
            SeriesColumns.Add(series);
        }

        // Determine number of rows (should be 101 for 0-100%)
        var rowCount = _currentVoltage.Curves.FirstOrDefault()?.Data.Count ?? 0;

        // Create rows
        for (var i = 0; i < rowCount; i++)
        {
            Rows.Add(new CurveDataRow(_currentVoltage, i));
        }
    }

    /// <summary>
    /// Updates a torque value in the data table.
    /// </summary>
    public void UpdateTorque(int rowIndex, string seriesName, double value)
    {
        if (rowIndex < 0 || rowIndex >= Rows.Count)
        {
            return;
        }

        if (_currentVoltage is null)
        {
            return;
        }

        var series = _currentVoltage.Curves.FirstOrDefault(s => s.Name == seriesName);
        if (series is null)
        {
            return;
        }

        if (_undoStack is null)
        {
            // Fallback legacy behavior when no undo stack is available.
            Rows[rowIndex].SetTorque(seriesName, value);
            DataChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        var command = new EditPointCommand(series, rowIndex, series.Data[rowIndex].Rpm, value);
        _undoStack.PushAndExecute(command);
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Updates a percent value in the data table for all series.
    /// Percent is shared across all series in a voltage configuration.
    /// </summary>
    public void UpdatePercent(int rowIndex, int value)
    {
        if (rowIndex < 0 || rowIndex >= Rows.Count)
        {
            return;
        }

        if (_currentVoltage is null || _currentVoltage.Curves.Count == 0)
        {
            return;
        }

        // Update percent for all series at this row index
        foreach (var series in _currentVoltage.Curves)
        {
            if (series.LockedPercent)
            {
                continue;
            }

            if (rowIndex >= series.Data.Count)
            {
                continue;
            }

            if (_undoStack is null)
            {
                series.Data[rowIndex].Percent = value;
            }
            else
            {
                var command = new EditPointCommand(
                    series,
                    rowIndex,
                    newPercent: value,
                    newRpm: null,
                    newTorque: null);
                _undoStack.PushAndExecute(command);
            }
        }

        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Updates an RPM value in the data table for all series.
    /// RPM is shared across all series in a voltage configuration.
    /// </summary>
    public void UpdateRpm(int rowIndex, double value)
    {
        if (rowIndex < 0 || rowIndex >= Rows.Count)
        {
            return;
        }

        if (_currentVoltage is null || _currentVoltage.Curves.Count == 0)
        {
            return;
        }

        // Update RPM for all series at this row index
        foreach (var series in _currentVoltage.Curves)
        {
            if (series.LockedRpm)
            {
                continue;
            }

            if (rowIndex >= series.Data.Count)
            {
                continue;
            }

            if (_undoStack is null)
            {
                series.Data[rowIndex].Rpm = value;
            }
            else
            {
                var command = new EditPointCommand(
                    series,
                    rowIndex,
                    newPercent: null,
                    newRpm: value,
                    newTorque: null);
                _undoStack.PushAndExecute(command);
            }
        }

        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the torque value for a specific row and series.
    /// </summary>
    public double GetTorque(int rowIndex, string seriesName)
    {
        if (rowIndex >= 0 && rowIndex < Rows.Count)
        {
            return Rows[rowIndex].GetTorque(seriesName);
        }
        return 0;
    }

    /// <summary>
    /// Checks if a series is locked.
    /// </summary>
    public bool IsSeriesLocked(string seriesName)
    {
        var series = _currentVoltage?.Curves.FirstOrDefault(s => s.Name == seriesName);
        return series?.Locked ?? false;
    }

    /// <summary>
    /// Checks if percent column is locked for any series in the current voltage.
    /// Returns true if all series have percent locked.
    /// </summary>
    public bool IsPercentLocked()
    {
        if (_currentVoltage is null || _currentVoltage.Curves.Count == 0)
        {
            return true;
        }

        // If any series allows percent editing, return false
        return _currentVoltage.Curves.All(s => s.LockedPercent);
    }

    /// <summary>
    /// Checks if RPM column is locked for any series in the current voltage.
    /// Returns true if all series have RPM locked.
    /// </summary>
    public bool IsRpmLocked()
    {
        if (_currentVoltage is null || _currentVoltage.Curves.Count == 0)
        {
            return true;
        }

        // If any series allows RPM editing, return false
        return _currentVoltage.Curves.All(s => s.LockedRpm);
    }

    /// <summary>
    /// Gets the number of columns in the data table (% + RPM + series columns).
    /// </summary>
    public int ColumnCount => 2 + SeriesColumns.Count;

    /// <summary>
    /// Gets the anchor cell for range selections.
    /// </summary>
    public CellPosition? AnchorCell => _anchorCell;

    /// <summary>
    /// Checks if a cell at the specified position is selected.
    /// </summary>
    public bool IsCellSelected(int rowIndex, int columnIndex)
    {
        return SelectedCells.Contains(new CellPosition(rowIndex, columnIndex));
    }

    /// <summary>
    /// Clears all selected cells.
    /// </summary>
    public void ClearSelection()
    {
        SelectedCells.Clear();
        _anchorCell = null;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        PushSelectionToCoordinator();
    }

    /// <summary>
    /// Selects a single cell, clearing any previous selection.
    /// </summary>
    public void SelectCell(int rowIndex, int columnIndex)
    {
        SelectedCells.Clear();
        var cell = new CellPosition(rowIndex, columnIndex);
        SelectedCells.Add(cell);
        _anchorCell = cell;
        SelectedRowIndex = rowIndex;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        PushSelectionToCoordinator();
    }

    /// <summary>
    /// Toggles selection of a cell (for Ctrl+click behavior).
    /// </summary>
    public void ToggleCellSelection(int rowIndex, int columnIndex)
    {
        var cell = new CellPosition(rowIndex, columnIndex);
        if (SelectedCells.Contains(cell))
        {
            SelectedCells.Remove(cell);
        }
        else
        {
            SelectedCells.Add(cell);
            _anchorCell = cell;
        }
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        PushSelectionToCoordinator();
    }

    /// <summary>
    /// Selects a range of cells from the anchor cell to the specified cell (for Shift+click).
    /// </summary>
    public void SelectRange(int rowIndex, int columnIndex)
    {
        if (_anchorCell is null)
        {
            SelectCell(rowIndex, columnIndex);
            return;
        }

        var anchor = _anchorCell.Value;
        var minRow = Math.Min(anchor.RowIndex, rowIndex);
        var maxRow = Math.Max(anchor.RowIndex, rowIndex);
        var minCol = Math.Min(anchor.ColumnIndex, columnIndex);
        var maxCol = Math.Max(anchor.ColumnIndex, columnIndex);

        SelectedCells.Clear();
        for (var row = minRow; row <= maxRow; row++)
        {
            for (var col = minCol; col <= maxCol; col++)
            {
                SelectedCells.Add(new CellPosition(row, col));
            }
        }
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        PushSelectionToCoordinator();
    }

    /// <summary>
    /// Adds cells to selection (for drag selection).
    /// </summary>
    public void AddToSelection(int rowIndex, int columnIndex)
    {
        var cell = new CellPosition(rowIndex, columnIndex);
        if (!SelectedCells.Contains(cell))
        {
            SelectedCells.Add(cell);
        }
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        PushSelectionToCoordinator();
    }

    /// <summary>
    /// Selects a rectangular range from startCell to endCell (for rubber-band selection).
    /// </summary>
    public void SelectRectangularRange(CellPosition startCell, CellPosition endCell)
    {
        var minRow = Math.Min(startCell.RowIndex, endCell.RowIndex);
        var maxRow = Math.Max(startCell.RowIndex, endCell.RowIndex);
        var minCol = Math.Min(startCell.ColumnIndex, endCell.ColumnIndex);
        var maxCol = Math.Max(startCell.ColumnIndex, endCell.ColumnIndex);

        SelectedCells.Clear();
        for (var row = minRow; row <= maxRow; row++)
        {
            for (var col = minCol; col <= maxCol; col++)
            {
                SelectedCells.Add(new CellPosition(row, col));
            }
        }
        _anchorCell = startCell;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        PushSelectionToCoordinator();
    }

    /// <summary>
    /// Extends selection in a direction (for Shift+Arrow keys).
    /// </summary>
    public void ExtendSelection(int rowDelta, int columnDelta)
    {
        if (SelectedCells.Count == 0) return;

        // Find the current bounds of the selection
        var minRow = SelectedCells.Min(c => c.RowIndex);
        var maxRow = SelectedCells.Max(c => c.RowIndex);
        var minCol = SelectedCells.Min(c => c.ColumnIndex);
        var maxCol = SelectedCells.Max(c => c.ColumnIndex);

        // Extend in the specified direction
        if (rowDelta < 0 && minRow > 0)
        {
            // Extend upward
            for (var col = minCol; col <= maxCol; col++)
            {
                SelectedCells.Add(new CellPosition(minRow - 1, col));
            }
        }
        else if (rowDelta > 0 && maxRow < Rows.Count - 1)
        {
            // Extend downward
            for (var col = minCol; col <= maxCol; col++)
            {
                SelectedCells.Add(new CellPosition(maxRow + 1, col));
            }
        }
        else if (columnDelta < 0 && minCol > 0)
        {
            // Extend left
            for (var row = minRow; row <= maxRow; row++)
            {
                SelectedCells.Add(new CellPosition(row, minCol - 1));
            }
        }
        else if (columnDelta > 0 && maxCol < ColumnCount - 1)
        {
            // Extend right
            for (var row = minRow; row <= maxRow; row++)
            {
                SelectedCells.Add(new CellPosition(row, maxCol + 1));
            }
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
        PushSelectionToCoordinator();
    }

    /// <summary>
    /// Moves the selection in a direction (for Arrow keys without Shift).
    /// </summary>
    public void MoveSelection(int rowDelta, int columnDelta)
    {
        if (SelectedCells.Count == 0)
        {
            // If no selection, select the first cell
            if (Rows.Count > 0 && ColumnCount > 0)
            {
                SelectCell(0, 0);
            }
            return;
        }

        // Use the anchor cell or the first selected cell
        var referenceCell = _anchorCell ?? SelectedCells.First();
        var newRow = Math.Clamp(referenceCell.RowIndex + rowDelta, 0, Math.Max(0, Rows.Count - 1));
        var newCol = Math.Clamp(referenceCell.ColumnIndex + columnDelta, 0, Math.Max(0, ColumnCount - 1));

        SelectCell(newRow, newCol);
    }

    /// <summary>
    /// Pushes the current cell selection to the shared editing coordinator as
    /// logical series/index pairs, when available. Non-series columns (%/RPM)
    /// are ignored.
    /// </summary>
    private void PushSelectionToCoordinator()
    {
        if (EditingCoordinator is null)
        {
            return;
        }

        if (_currentVoltage is null || SeriesColumns.Count == 0)
        {
            EditingCoordinator.ClearSelection();
            return;
        }

        if (SelectedCells.Count == 0)
        {
            EditingCoordinator.ClearSelection();
            return;
        }

        var selections = new List<EditingCoordinator.PointSelection>();

        foreach (var cell in SelectedCells)
        {
            // Skip non-series columns
            if (cell.ColumnIndex < 2)
            {
                continue;
            }

            if (cell.RowIndex < 0)
            {
                continue;
            }

            var seriesName = GetSeriesNameForColumn(cell.ColumnIndex);
            if (seriesName is null)
            {
                continue;
            }

            var series = _currentVoltage.Curves.FirstOrDefault(s => s.Name == seriesName);
            if (series is null)
            {
                continue;
            }

            if (cell.RowIndex >= series.Data.Count)
            {
                continue;
            }

            selections.Add(new EditingCoordinator.PointSelection(series, cell.RowIndex));
        }

        if (selections.Count == 0)
        {
            EditingCoordinator.ClearSelection();
        }
        else
        {
            EditingCoordinator.SetSelection(selections);
        }
    }

    /// <summary>
    /// Responds to selection changes coming from the shared editing
    /// coordinator (e.g., graph-driven selection) by updating
    /// <see cref="SelectedCells"/>. This keeps table selection in sync
    /// with graph interactions.
    /// </summary>
    private void OnCoordinatorSelectionChanged(object? sender, EventArgs e)
    {
        if (_editingCoordinator is null || _currentVoltage is null)
        {
            return;
        }

        // Rebuild SelectedCells from the coordinator's logical selection.
        SelectedCells.Clear();

        foreach (var point in _editingCoordinator.SelectedPoints)
        {
            var seriesIndex = SeriesColumns.IndexOf(point.Series);
            if (seriesIndex < 0)
            {
                continue;
            }

            var rowIndex = point.Index;
            if (rowIndex < 0 || rowIndex >= Rows.Count)
            {
                continue;
            }

            var columnIndex = 2 + seriesIndex; // offset for % and RPM columns
            SelectedCells.Add(new CellPosition(rowIndex, columnIndex));
        }

        // When selection is driven externally, reset the anchor to the first
        // selected cell for predictable Shift+Arrow behavior.
        _anchorCell = SelectedCells.FirstOrDefault();

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the series name for a given column index.
    /// Returns null for non-series columns (% and RPM).
    /// </summary>
    public string? GetSeriesNameForColumn(int columnIndex)
    {
        // First two columns are % and RPM
        if (columnIndex < 2) return null;
        var seriesIndex = columnIndex - 2;
        if (seriesIndex >= 0 && seriesIndex < SeriesColumns.Count)
        {
            return SeriesColumns[seriesIndex].Name;
        }
        return null;
    }

    /// <summary>
    /// Gets the column index for a given series name.
    /// Returns -1 if the series is not found.
    /// </summary>
    public int GetColumnIndexForSeries(string seriesName)
    {
        for (var i = 0; i < SeriesColumns.Count; i++)
        {
            if (SeriesColumns[i].Name == seriesName)
            {
                return i + 2; // Add 2 for % and RPM columns
            }
        }
        return -1;
    }

    /// <summary>
    /// Applies a torque value to the specified cell positions.
    /// Respects fixed columns (% and RPM) and locked series, and raises DataChanged
    /// if at least one torque value is updated.
    /// </summary>
    public void ApplyTorqueToCells(IEnumerable<CellPosition> cells, double value)
    {
        ArgumentNullException.ThrowIfNull(cells);

        if (_currentVoltage is null || Rows.Count == 0 || SeriesColumns.Count == 0)
        {
            return;
        }

        var anyChanged = false;

        foreach (var cell in cells)
        {
            if (TrySetTorqueAtCell(cell, value))
            {
                anyChanged = true;
            }
        }

        if (anyChanged)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Applies the provided torque value to all currently selected cells and
    /// raises <see cref="DataChanged"/> if any value changes.
    /// </summary>
    /// <param name="value">The torque value to apply.</param>
    public void ApplyOverrideValue(double value)
    {
        ApplyTorqueToSelectedCells(value);
    }

    /// <summary>
    /// Commits an override-mode edit for the specified cells using the
    /// shared <see cref="UndoStack"/> when available, so that the
    /// operation is undoable as a single logical step.
    /// </summary>
    /// <param name="originalValues">
    /// The original torque values captured at the start of override mode,
    /// keyed by cell position.
    /// </param>
    /// <param name="newValue">The final override torque value.</param>
    /// <returns>True if any cell was changed and an operation was committed; otherwise false.</returns>
    public bool TryCommitOverrideWithUndo(IReadOnlyDictionary<CellPosition, double> originalValues, double newValue)
    {
        ArgumentNullException.ThrowIfNull(originalValues);

        if (_currentVoltage is null || Rows.Count == 0 || SeriesColumns.Count == 0)
        {
            return false;
        }

        if (originalValues.Count == 0)
        {
            return false;
        }

        // When no undo stack is present, fall back to the existing direct
        // mutation path so behavior remains consistent with earlier
        // versions, even though the operation will not be undoable.
        if (_undoStack is null)
        {
            ApplyTorqueToCells(originalValues.Keys, newValue);
            return true;
        }

        var targets = new List<OverrideTorqueCellsCommand.Target>();

        foreach (var kvp in originalValues)
        {
            var cell = kvp.Key;
            var oldTorque = kvp.Value;

            // Skip rows that are now out of range.
            if (cell.RowIndex < 0 || cell.RowIndex >= Rows.Count)
            {
                continue;
            }

            // Skip non-series columns.
            if (cell.ColumnIndex < 2)
            {
                continue;
            }

            var seriesName = GetSeriesNameForColumn(cell.ColumnIndex);
            if (string.IsNullOrEmpty(seriesName))
            {
                continue;
            }

            // Respect locked series.
            if (IsSeriesLocked(seriesName))
            {
                continue;
            }

            var series = _currentVoltage.Curves.FirstOrDefault(s => s.Name == seriesName);
            if (series is null)
            {
                continue;
            }

            var dataIndex = cell.RowIndex;
            if (dataIndex < 0 || dataIndex >= series.Data.Count)
            {
                continue;
            }

            // Skip no-op entries where the override value matches the
            // original torque.
            if (Math.Abs(oldTorque - newValue) <= double.Epsilon)
            {
                continue;
            }

            targets.Add(new OverrideTorqueCellsCommand.Target(series, dataIndex, oldTorque, newValue));
        }

        if (targets.Count == 0)
        {
            return false;
        }

        var command = new OverrideTorqueCellsCommand(targets);
        _undoStack.PushAndExecute(command);
        DataChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Clears torque values for all currently selected cells by setting them to zero.
    /// </summary>
    public void ClearSelectedTorqueCells()
    {
        if (SelectedCells.Count == 0)
        {
            return;
        }

        // When an undo stack is available, route per-cell edits through
        // undoable commands so delete/backspace operations participate in
        // the global undo/redo history. In environments without an undo
        // stack (certain tests or tooling scenarios), fall back to the
        // legacy direct mutation path for simplicity.
        if (_undoStack is null)
        {
            ApplyTorqueToCells(SelectedCells, 0);
            return;
        }

        var anyChanged = false;

        foreach (var cell in SelectedCells)
        {
            if (TryApplyTorqueWithUndo(cell, 0))
            {
                anyChanged = true;
            }
        }

        if (anyChanged)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Attempts to paste clipboard text into the table starting at the current
    /// selection. Returns true if any torque value is updated. Clipboard shape
    /// validation and error reporting are handled here so the view only needs
    /// to provide raw text.
    /// </summary>
    /// <param name="clipboardText">Clipboard text containing tab/newline-delimited values.</param>
    /// <returns>True if at least one cell was updated; otherwise false.</returns>
    public bool TryPasteClipboard(string clipboardText)
    {
        if (string.IsNullOrWhiteSpace(clipboardText))
        {
            return false;
        }

        if (SelectedCells.Count == 0)
        {
            return false;
        }

        if (_currentVoltage is null || Rows.Count == 0 || SeriesColumns.Count == 0)
        {
            return false;
        }

        var selectedCellsSnapshot = SelectedCells.ToList();

        // Normalize clipboard text into lines and values
        var lines = clipboardText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            return false;
        }

        // When no undo stack is present, use the existing direct-mutation
        // path for backward compatibility.
        if (_undoStack is null)
        {
            // Special case: single scalar replicated across all selected cells
            if (lines.Length == 1)
            {
                var parts = lines[0].Split('\t', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1 && double.TryParse(parts[0], out var scalar))
                {
                    ApplyTorqueToCells(selectedCellsSnapshot, scalar);
                    return true;
                }
            }

            // General rectangular paste starting from the top-left selected cell
            var minRowLegacy = selectedCellsSnapshot.Min(c => c.RowIndex);
            var minColLegacy = selectedCellsSnapshot.Min(c => c.ColumnIndex);

            var anyChangedLegacy = false;

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var values = lines[lineIndex].Split('\t');
                var rowIndex = minRowLegacy + lineIndex;

                if (rowIndex >= Rows.Count)
                {
                    break;
                }

                for (var valueIndex = 0; valueIndex < values.Length; valueIndex++)
                {
                    var colIndex = minColLegacy + valueIndex;
                    var raw = values[valueIndex];

                    if (!double.TryParse(raw, out var value))
                    {
                        continue;
                    }

                    var cellPos = new CellPosition(rowIndex, colIndex);
                    if (TrySetTorqueAtCell(cellPos, value))
                    {
                        anyChangedLegacy = true;
                    }
                }
            }

            if (!anyChangedLegacy)
            {
                return false;
            }

            DataChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        // Undo-aware path: create proper undoable commands for each cell that
        // changes so the entire paste operation can be unwound via the shared
        // UndoStack.

        // Special case: single scalar replicated across all selected cells
        if (lines.Length == 1)
        {
            var parts = lines[0].Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1 && double.TryParse(parts[0], out var scalar))
            {
                var anyScalarChanged = false;

                foreach (var cell in selectedCellsSnapshot)
                {
                    if (TryApplyTorqueWithUndo(cell, scalar))
                    {
                        anyScalarChanged = true;
                    }
                }

                if (!anyScalarChanged)
                {
                    return false;
                }

                DataChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
        }

        // General rectangular paste starting from the top-left selected cell
        var minRow = selectedCellsSnapshot.Min(c => c.RowIndex);
        var minCol = selectedCellsSnapshot.Min(c => c.ColumnIndex);

        var anyChanged = false;

        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var values = lines[lineIndex].Split('\t');
            var rowIndex = minRow + lineIndex;

            if (rowIndex >= Rows.Count)
            {
                break;
            }

            for (var valueIndex = 0; valueIndex < values.Length; valueIndex++)
            {
                var colIndex = minCol + valueIndex;
                var raw = values[valueIndex];

                if (!double.TryParse(raw, out var value))
                {
                    continue;
                }

                var cellPos = new CellPosition(rowIndex, colIndex);
                if (TryApplyTorqueWithUndo(cellPos, value))
                {
                    anyChanged = true;
                }
            }
        }

        if (!anyChanged)
        {
            return false;
        }

        DataChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Attempts to set the torque value at a specific cell.
    /// </summary>
    /// <remarks>
    /// This method centralizes all per-cell mutation rules for the curve table and is the
    /// preferred way for views or higher-level helpers to change a single torque value.
    /// It performs the following checks before updating any data:
    /// <list type="bullet">
    /// <item><description>Validates that a current voltage, rows, and series columns are available.</description></item>
    /// <item><description>Ensures the row index is within the bounds of <see cref="Rows"/>.</description></item>
    /// <item><description>Rejects writes to the fixed % and RPM columns (column indices 0 and 1).</description></item>
    /// <item><description>Maps the column to a series and aborts if no matching series exists.</description></item>
    /// <item><description>Honors <see cref="IsSeriesLocked(string)"/> and will not modify locked series.</description></item>
    /// <item><description>Skips updates when the new value is numerically equal to the current value.</description></item>
    /// </list>
    /// Callers that need to update multiple cells (for example, override mode, clipboard
    /// paste, or delete/backspace clearing) should either:
    /// <list type="number">
    /// <item><description>Use <see cref="ApplyTorqueToCells(System.Collections.Generic.IEnumerable{CurveEditor.ViewModels.CellPosition}, double)"/> to apply a single value to many cells and raise <see cref="DataChanged"/> once, or</description></item>
    /// <item><description>Invoke <see cref="TrySetTorqueAtCell(CurveEditor.ViewModels.CellPosition, double)"/> in a loop and handle any additional UI updates (such as text rendering) on success.</description></item>
    /// </list>
    /// The method returns <see langword="true"/> only when a torque value was actually changed;
    /// otherwise it returns <see langword="false"/> and leaves the model untouched.
    /// </remarks>
    public bool TrySetTorqueAtCell(CellPosition cell, double value)
    {
        if (_currentVoltage is null || Rows.Count == 0 || SeriesColumns.Count == 0)
        {
            return false;
        }

        // Skip invalid rows
        if (cell.RowIndex < 0 || cell.RowIndex >= Rows.Count)
        {
            return false;
        }

        // Skip % and RPM columns (read-only)
        if (cell.ColumnIndex < 2)
        {
            return false;
        }

        var seriesName = GetSeriesNameForColumn(cell.ColumnIndex);
        if (string.IsNullOrEmpty(seriesName))
        {
            return false;
        }

        // Respect locked series
        if (IsSeriesLocked(seriesName))
        {
            return false;
        }

        var row = Rows[cell.RowIndex];
        var current = row.GetTorque(seriesName);
        if (Math.Abs(current - value) <= double.Epsilon)
        {
            return false;
        }

        row.SetTorque(seriesName, value);
        return true;
    }

    /// <summary>
    /// Helper used by view code to route single-cell edits through the
    /// undo-aware path without exposing the full command plumbing. This
    /// maintains a clear separation where the view delegates mutation
    /// rules to the view model while still allowing tests to exercise the
    /// lower-level <see cref="TrySetTorqueAtCell"/> API directly.
    /// </summary>
    /// <param name="cell">The logical cell being edited.</param>
    /// <param name="value">The new torque value.</param>
    /// <returns>True if the value changed; otherwise false.</returns>
    public bool TryApplyTorqueWithUndoForView(CellPosition cell, double value)
    {
        return TryApplyTorqueWithUndo(cell, value);
    }

    /// <summary>
    /// Attempts to set the torque value at a specific cell using the shared
    /// <see cref="UndoStack"/> when available so that the change can be
    /// undone and redone. When no undo stack is configured, this falls back
    /// to the same direct-mutation semantics as <see cref="TrySetTorqueAtCell"/>.
    /// </summary>
    /// <param name="cell">The logical cell position.</param>
    /// <param name="value">The new torque value.</param>
    /// <returns>
    /// True if the value changed; otherwise false.
    /// </returns>
    private bool TryApplyTorqueWithUndo(CellPosition cell, double value)
    {
        if (_currentVoltage is null || Rows.Count == 0 || SeriesColumns.Count == 0)
        {
            return false;
        }

        if (cell.RowIndex < 0 || cell.RowIndex >= Rows.Count)
        {
            return false;
        }

        // Skip % and RPM columns (read-only)
        if (cell.ColumnIndex < 2)
        {
            return false;
        }

        var seriesName = GetSeriesNameForColumn(cell.ColumnIndex);
        if (string.IsNullOrEmpty(seriesName))
        {
            return false;
        }

        if (IsSeriesLocked(seriesName))
        {
            return false;
        }

        var series = _currentVoltage.Curves.FirstOrDefault(s => s.Name == seriesName);
        if (series is null)
        {
            return false;
        }

        var row = Rows[cell.RowIndex];
        var dataIndex = row.RowIndex;

        if (dataIndex < 0 || dataIndex >= series.Data.Count)
        {
            return false;
        }

        var current = series.Data[dataIndex].Torque;
        if (Math.Abs(current - value) <= double.Epsilon)
        {
            return false;
        }

        if (_undoStack is null)
        {
            row.SetTorque(seriesName, value);
            return true;
        }

        var rpm = series.Data[dataIndex].Rpm;
        var command = new EditPointCommand(series, dataIndex, rpm, value);
        _undoStack.PushAndExecute(command);
        return true;
    }

    /// <summary>
    /// Extends selection to the end of the row or column in the specified direction (for Ctrl+Shift+Arrow keys).
    /// </summary>
    public void ExtendSelectionToEnd(int rowDelta, int columnDelta)
    {
        if (SelectedCells.Count == 0) return;

        // Find the current bounds of the selection
        var minRow = SelectedCells.Min(c => c.RowIndex);
        var maxRow = SelectedCells.Max(c => c.RowIndex);
        var minCol = SelectedCells.Min(c => c.ColumnIndex);
        var maxCol = SelectedCells.Max(c => c.ColumnIndex);

        // Extend to the end in the specified direction
        if (rowDelta < 0)
        {
            // Extend upward to row 0
            for (var row = minRow - 1; row >= 0; row--)
            {
                for (var col = minCol; col <= maxCol; col++)
                {
                    SelectedCells.Add(new CellPosition(row, col));
                }
            }
        }
        else if (rowDelta > 0)
        {
            // Extend downward to last row
            for (var row = maxRow + 1; row < Rows.Count; row++)
            {
                for (var col = minCol; col <= maxCol; col++)
                {
                    SelectedCells.Add(new CellPosition(row, col));
                }
            }
        }
        else if (columnDelta < 0)
        {
            // Extend left to first column
            for (var col = minCol - 1; col >= 0; col--)
            {
                for (var row = minRow; row <= maxRow; row++)
                {
                    SelectedCells.Add(new CellPosition(row, col));
                }
            }
        }
        else if (columnDelta > 0)
        {
            // Extend right to last column
            for (var col = maxCol + 1; col < ColumnCount; col++)
            {
                for (var row = minRow; row <= maxRow; row++)
                {
                    SelectedCells.Add(new CellPosition(row, col));
                }
            }
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);

        // Keep the shared editing selection in sync so chart overlays
        // reflect Ctrl+Shift+Arrow driven selection changes.
        PushSelectionToCoordinator();
    }

    /// <summary>
    /// Builds a tab/newline-separated clipboard representation of the given selected cells.
    /// The shape is rectangular, defined by the min/max row and column indices in the set.
    /// </summary>
    public string BuildClipboardText(IReadOnlyCollection<CellPosition> selectedCells)
    {
        ArgumentNullException.ThrowIfNull(selectedCells);
        if (selectedCells.Count == 0)
        {
            return string.Empty;
        }

        var minRow = selectedCells.Min(c => c.RowIndex);
        var maxRow = selectedCells.Max(c => c.RowIndex);
        var minCol = selectedCells.Min(c => c.ColumnIndex);
        var maxCol = selectedCells.Max(c => c.ColumnIndex);

        var lines = new List<string>();
        for (var rowIndex = minRow; rowIndex <= maxRow; rowIndex++)
        {
            var values = new List<string>();
            for (var colIndex = minCol; colIndex <= maxCol; colIndex++)
            {
                var cell = new CellPosition(rowIndex, colIndex);
                if (!selectedCells.Contains(cell))
                {
                    values.Add(string.Empty);
                    continue;
                }

                if (rowIndex < 0 || rowIndex >= Rows.Count)
                {
                    values.Add(string.Empty);
                    continue;
                }

                var row = Rows[rowIndex];

                if (colIndex == 0)
                {
                    values.Add(row.Percent.ToString());
                }
                else if (colIndex == 1)
                {
                    values.Add(row.DisplayRpm.ToString());
                }
                else
                {
                    var seriesName = GetSeriesNameForColumn(colIndex);
                    if (seriesName is not null)
                    {
                        values.Add(row.GetTorque(seriesName).ToString("F2"));
                    }
                    else
                    {
                        values.Add(string.Empty);
                    }
                }
            }
            lines.Add(string.Join("\t", values));
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Applies clipboard text to the table starting at the specified top-left cell.
    /// Respects fixed columns and locked series. Returns false and does not modify
    /// data if the clipboard shape does not fit within the table.
    /// </summary>
    public bool TryApplyClipboardText(CellPosition topLeft, string clipboardText)
    {
        if (string.IsNullOrWhiteSpace(clipboardText))
        {
            return false;
        }

        var lines = clipboardText
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            return false;
        }

        var parsed = lines
            .Select(line => line.Split('\t'))
            .ToArray();

        var rowCount = parsed.Length;
        var colCount = parsed.Max(v => v.Length);

        // Ensure the rectangle fits inside the table
        if (topLeft.RowIndex < 0 || topLeft.ColumnIndex < 0)
        {
            return false;
        }

        if (topLeft.RowIndex + rowCount > Rows.Count ||
            topLeft.ColumnIndex + colCount > ColumnCount)
        {
            return false;
        }

        var anyChanged = false;

        for (var r = 0; r < rowCount; r++)
        {
            var values = parsed[r];
            for (var c = 0; c < values.Length; c++)
            {
                var targetRow = topLeft.RowIndex + r;
                var targetCol = topLeft.ColumnIndex + c;

                // Skip % and RPM columns
                if (targetCol < 2)
                {
                    continue;
                }

                var seriesName = GetSeriesNameForColumn(targetCol);
                if (seriesName is null)
                {
                    continue;
                }

                if (IsSeriesLocked(seriesName))
                {
                    continue;
                }

                if (!double.TryParse(values[c], out var torque))
                {
                    continue;
                }

                var row = Rows[targetRow];
                var current = row.GetTorque(seriesName);
                if (Math.Abs(current - torque) > double.Epsilon)
                {
                    row.SetTorque(seriesName, torque);
                    anyChanged = true;
                }
            }
        }

        if (anyChanged)
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        return anyChanged;
    }
}
