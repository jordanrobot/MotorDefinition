using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using CurveEditor.ViewModels;
using JordanRobot.MotorDefinition.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurveEditor.Views;

/// <summary>
/// Panel for displaying and editing curve series data points.
/// </summary>
public partial class CurveDataPanel : UserControl
{
    private MainWindowViewModel? _subscribedViewModel;
    private bool _isRebuildingColumns;
    private bool _isDragging;
    private CellPosition? _dragStartCell;
    private readonly Dictionary<CellPosition, Border> _cellBorders = [];
    private bool _eventHandlersRegistered;

    // Override Mode: when user types while cells are selected (not in edit mode),
    // the typed value replaces the existing values of all selected cells
    private bool _isInOverrideMode;
    private string _overrideText = string.Empty;
    private readonly Dictionary<CellPosition, double> _originalValues = [];
    private CellPosition? _editCell;
    private double? _editOriginalTorque;

    /// <summary>
    /// Captures the original torque values for the provided cells so they can be restored later.
    /// Used by both Override Mode and edit-mode Esc behavior.
    /// Now also supports % and RPM columns.
    /// </summary>
    private void SnapshotOriginalValues(IEnumerable<CellPosition> cells)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        _originalValues.Clear();

        foreach (var cellPos in cells)
        {
            // Handle % column (column 0)
            if (cellPos.ColumnIndex == 0)
            {
                if (cellPos.RowIndex >= 0 && cellPos.RowIndex < vm.CurveDataTableViewModel.Rows.Count)
                {
                    var originalValue = (double)vm.CurveDataTableViewModel.Rows[cellPos.RowIndex].Percent;
                    _originalValues[cellPos] = originalValue;
                }
                continue;
            }

            // Handle RPM column (column 1)
            if (cellPos.ColumnIndex == 1)
            {
                if (cellPos.RowIndex >= 0 && cellPos.RowIndex < vm.CurveDataTableViewModel.Rows.Count)
                {
                    var originalValue = (double)vm.CurveDataTableViewModel.Rows[cellPos.RowIndex].DisplayRpm;
                    _originalValues[cellPos] = originalValue;
                }
                continue;
            }

            // Handle torque columns (column 2+)
            var seriesName = vm.CurveDataTableViewModel.GetSeriesNameForColumn(cellPos.ColumnIndex);
            if (seriesName is null) continue;
            if (vm.CurveDataTableViewModel.IsSeriesLocked(seriesName)) continue;

            if (cellPos.RowIndex >= 0 && cellPos.RowIndex < vm.CurveDataTableViewModel.Rows.Count)
            {
                var originalValue = vm.CurveDataTableViewModel.Rows[cellPos.RowIndex].GetTorque(seriesName);
                _originalValues[cellPos] = originalValue;
            }
        }
    }

    /// <summary>
    /// Creates a new CurveDataPanel instance.
    /// </summary>
    public CurveDataPanel()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += OnUnloaded;

        // Use tunnel routing to capture pointer and key events before the DataGrid handles them
        // This is necessary because DataGrid intercepts these events for its own selection handling
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Prevent duplicate event handler registrations
        if (_eventHandlersRegistered || DataTable is null) return;

        DataTable.BeginningEdit += DataTable_BeginningEdit;
        DataTable.AddHandler(PointerPressedEvent, DataTable_PointerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        DataTable.AddHandler(PointerMovedEvent, DataTable_PointerMoved, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        DataTable.AddHandler(PointerReleasedEvent, DataTable_PointerReleased, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        DataTable.AddHandler(KeyDownEvent, DataTable_KeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        DataTable.AddHandler(TextInputEvent, DataTable_TextInput, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        _eventHandlersRegistered = true;
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        // Unsubscribe from events to prevent memory leaks
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.CurveDataTableViewModel.PropertyChanged -= OnCurveDataTablePropertyChanged;
            _subscribedViewModel.CurveDataTableViewModel.SelectionChanged -= OnSelectionChanged;
            _subscribedViewModel.AvailableSeries.CollectionChanged -= OnAvailableSeriesCollectionChanged;
            _subscribedViewModel = null;
        }

        // Remove event handlers when unloaded
        if (_eventHandlersRegistered && DataTable is not null)
        {
            DataTable.BeginningEdit -= DataTable_BeginningEdit;
            DataTable.RemoveHandler(PointerPressedEvent, DataTable_PointerPressed);
            DataTable.RemoveHandler(PointerMovedEvent, DataTable_PointerMoved);
            DataTable.RemoveHandler(PointerReleasedEvent, DataTable_PointerReleased);
            DataTable.RemoveHandler(KeyDownEvent, DataTable_KeyDown);
            DataTable.RemoveHandler(TextInputEvent, DataTable_TextInput);
            _eventHandlersRegistered = false;
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // Unsubscribe from old view model
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.CurveDataTableViewModel.PropertyChanged -= OnCurveDataTablePropertyChanged;
            _subscribedViewModel.CurveDataTableViewModel.SelectionChanged -= OnSelectionChanged;
            _subscribedViewModel.AvailableSeries.CollectionChanged -= OnAvailableSeriesCollectionChanged;
        }

        if (DataContext is MainWindowViewModel vm)
        {
            // Subscribe to series changes to rebuild columns
            vm.CurveDataTableViewModel.PropertyChanged += OnCurveDataTablePropertyChanged;
            vm.CurveDataTableViewModel.SelectionChanged += OnSelectionChanged;
            // Subscribe to the AvailableSeries collection changes
            vm.AvailableSeries.CollectionChanged += OnAvailableSeriesCollectionChanged;
            _subscribedViewModel = vm;
            RebuildDataGridColumns();
        }
        else
        {
            _subscribedViewModel = null;
        }
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        UpdateCellSelectionVisuals();
    }

    private void DataTable_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        if (DataTable is null) return;
        if (e.Row.DataContext is not CurveDataRow dataRow)
        {
            _editCell = null;
            _editOriginalTorque = null;
            return;
        }

        var rowIndex = dataRow.RowIndex;
        var columnIndex = e.Column.DisplayIndex;

        var cellPos = new CellPosition(rowIndex, columnIndex);
        _editCell = cellPos;

        _editOriginalTorque = null;

        // Only snapshot torque cells (columns >= 2)
        if (columnIndex >= 2 && rowIndex >= 0 && rowIndex < vm.CurveDataTableViewModel.Rows.Count)
        {
            var seriesName = vm.CurveDataTableViewModel.GetSeriesNameForColumn(columnIndex);
            if (seriesName is not null)
            {
                var row = vm.CurveDataTableViewModel.Rows[rowIndex];
                _editOriginalTorque = row.GetTorque(seriesName);
            }
        }
    }

    private void UpdateCellSelectionVisuals()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var selectedCells = vm.CurveDataTableViewModel.SelectedCells;
        Log.Debug("[BORDER] UpdateCellSelectionVisuals: {Count} cells selected, {BorderCount} borders tracked", 
            selectedCells.Count, _cellBorders.Count);

        var bordersToRemove = new List<CellPosition>();

        // First pass: Reset ALL borders to unselected state and clean up stale entries
        foreach (var kvp in _cellBorders)
        {
            // Check if the border is still valid (has a visual parent)
            if (kvp.Value.GetVisualParent() is null)
            {
                // Border is no longer in the visual tree, mark for removal
                bordersToRemove.Add(kvp.Key);
                continue;
            }

            // Reset ALL borders to unselected state first
            // This ensures that when selection changes (e.g., after edit mode or arrow keys),
            // previously selected borders are properly cleared
            UpdateCellBorderVisual(kvp.Value, false);
        }

        // Clean up stale entries
        foreach (var pos in bordersToRemove)
        {
            _cellBorders.Remove(pos);
        }

        // Second pass: Apply selected state only to currently selected cells
        foreach (var cellPos in selectedCells)
        {
            if (_cellBorders.TryGetValue(cellPos, out var border))
            {
                Log.Debug("[BORDER] Highlighting cell at Row={Row}, Col={Col}", cellPos.RowIndex, cellPos.ColumnIndex);
                UpdateCellBorderVisual(border, true);
            }
            else
            {
                Log.Debug("[BORDER] WARNING: No border found for selected cell at Row={Row}, Col={Col}", 
                    cellPos.RowIndex, cellPos.ColumnIndex);
            }
        }
    }

    private static void UpdateCellBorderVisual(Border border, bool isSelected)
    {
        if (isSelected)
        {
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.White;
            border.Background = new SolidColorBrush(Color.FromArgb(51, 255, 255, 255)); // #33FFFFFF
        }
        else
        {
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = Brushes.Transparent;
            border.Background = Brushes.Transparent;
        }
    }

    private void OnAvailableSeriesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Skip if we're already in the middle of rebuilding columns
        // This prevents race conditions when RefreshAvailableSeries() clears and repopulates the collection
        if (_isRebuildingColumns) return;

        // Only rebuild on Add action, not on Reset (Clear) which would cause empty columns
        // The columns will be properly rebuilt when all items are added
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
        {
            return;
        }

        // Rebuild columns when series are added or removed
        RebuildDataGridColumns();
    }

    private void OnCurveDataTablePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Skip if we're already in the middle of rebuilding columns
        if (_isRebuildingColumns) return;

        if (e.PropertyName == nameof(CurveDataTableViewModel.SeriesColumns) ||
            e.PropertyName == nameof(CurveDataTableViewModel.CurrentVoltage))
        {
            RebuildDataGridColumns();
        }
    }

    private void RebuildDataGridColumns()
    {
        if (DataContext is not MainWindowViewModel vm) return;
        if (DataTable is null) return;
        if (_isRebuildingColumns) return;

        _isRebuildingColumns = true;
        _cellBorders.Clear();

        try
        {
            // Critical fix: Temporarily disconnect ItemsSource to prevent layout issues
            // The DataGrid can crash when columns are modified while it's trying to layout
            // existing rows that expect a different column count
            var savedItemsSource = DataTable.ItemsSource;
            DataTable.ItemsSource = null;

            // Clear all columns including % and RPM
            DataTable.Columns.Clear();

            // Determine if % and RPM columns are locked
            var isPercentLocked = vm.CurveDataTableViewModel.IsPercentLocked();
            var isRpmLocked = vm.CurveDataTableViewModel.IsRpmLocked();

            // Update lock toggle button visual states
            UpdateLockToggleStates(isPercentLocked, isRpmLocked);

            // Add % column with template (like torque columns for selection support)
            var percentColumn = new DataGridTemplateColumn
            {
                Header = "%",
                Width = new DataGridLength(50),
                IsReadOnly = isPercentLocked
            };

            // Create cell template for % column with selection border
            var percentCellTemplate = new FuncDataTemplate<CurveDataRow>((row, _) =>
            {
                if (row is null) return new TextBlock { Text = "0" };

                var border = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Transparent,
                    Background = Brushes.Transparent,
                    Padding = new Thickness(2)
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Thickness(2),
                    Text = row.Percent.ToString()
                };

                border.Child = textBlock;

                // Register the border for selection updates (column index 0)
                var cellPos = new CellPosition(row.RowIndex, 0);
                _cellBorders[cellPos] = border;

                // Update visual based on current selection state
                if (DataContext is MainWindowViewModel viewModel)
                {
                    var isSelected = viewModel.CurveDataTableViewModel.IsCellSelected(row.RowIndex, 0);
                    UpdateCellBorderVisual(border, isSelected);
                }

                return border;
            });
            percentColumn.CellTemplate = percentCellTemplate;

            // Create editing template for % column
            if (!isPercentLocked)
            {
                var percentEditingTemplate = new FuncDataTemplate<CurveDataRow>((row, _) =>
                {
                    var textBox = new TextBox
                    {
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Margin = new Thickness(2),
                        Text = row?.Percent.ToString() ?? "0",
                        BorderThickness = new Thickness(2),
                        BorderBrush = Brushes.White,
                        IsUndoEnabled = false
                    };

                    textBox.AttachedToVisualTree += (sender, e) =>
                    {
                        if (sender is TextBox tb)
                        {
                            tb.SelectAll();
                            tb.Focus();
                        }
                    };

                    textBox.LostFocus += (sender, e) =>
                    {
                        if (sender is not TextBox tb || row is null)
                        {
                            return;
                        }

                        if (!int.TryParse(tb.Text, out var newValue) || newValue < 0)
                        {
                            return;
                        }

                        if (DataContext is not MainWindowViewModel viewModel)
                        {
                            return;
                        }

                        var rowIndex = row.RowIndex;
                        viewModel.CurveDataTableViewModel.UpdatePercent(rowIndex, newValue);
                        viewModel.MarkDirty();
                        viewModel.ChartViewModel.RefreshChart();
                    };

                    return textBox;
                });
                percentColumn.CellEditingTemplate = percentEditingTemplate;
            }

            DataTable.Columns.Add(percentColumn);

            // Add RPM column with template (like torque columns for selection support)
            var rpmColumn = new DataGridTemplateColumn
            {
                Header = "RPM",
                Width = new DataGridLength(70),
                IsReadOnly = isRpmLocked
            };

            // Create cell template for RPM column with selection border
            var rpmCellTemplate = new FuncDataTemplate<CurveDataRow>((row, _) =>
            {
                if (row is null) return new TextBlock { Text = "0" };

                var border = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Transparent,
                    Background = Brushes.Transparent,
                    Padding = new Thickness(2)
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Thickness(2),
                    Text = row.DisplayRpm.ToString()
                };

                border.Child = textBlock;

                // Register the border for selection updates (column index 1)
                var cellPos = new CellPosition(row.RowIndex, 1);
                _cellBorders[cellPos] = border;

                // Update visual based on current selection state
                if (DataContext is MainWindowViewModel viewModel)
                {
                    var isSelected = viewModel.CurveDataTableViewModel.IsCellSelected(row.RowIndex, 1);
                    UpdateCellBorderVisual(border, isSelected);
                }

                return border;
            });
            rpmColumn.CellTemplate = rpmCellTemplate;

            // Create editing template for RPM column
            if (!isRpmLocked)
            {
                var rpmEditingTemplate = new FuncDataTemplate<CurveDataRow>((row, _) =>
                {
                    var textBox = new TextBox
                    {
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Margin = new Thickness(2),
                        Text = row?.DisplayRpm.ToString() ?? "0",
                        BorderThickness = new Thickness(2),
                        BorderBrush = Brushes.White,
                        IsUndoEnabled = false
                    };

                    textBox.AttachedToVisualTree += (sender, e) =>
                    {
                        if (sender is TextBox tb)
                        {
                            tb.SelectAll();
                            tb.Focus();
                        }
                    };

                    textBox.LostFocus += (sender, e) =>
                    {
                        if (sender is not TextBox tb || row is null)
                        {
                            return;
                        }

                        if (!double.TryParse(tb.Text, out var newValue) || newValue < 0)
                        {
                            return;
                        }

                        if (DataContext is not MainWindowViewModel viewModel)
                        {
                            return;
                        }

                        var rowIndex = row.RowIndex;
                        viewModel.CurveDataTableViewModel.UpdateRpm(rowIndex, newValue);
                        viewModel.MarkDirty();
                        viewModel.ChartViewModel.RefreshChart();
                    };

                    return textBox;
                });
                rpmColumn.CellEditingTemplate = rpmEditingTemplate;
            }

            DataTable.Columns.Add(rpmColumn);

            // Add a column for each series
            var columnIndex = 2; // Start after % and RPM columns
            foreach (var series in vm.AvailableSeries)
            {
                // Capture series name and column index to avoid closure issues
                var seriesName = series.Name;
                var isLocked = series.Locked;
                var currentColumnIndex = columnIndex;

                var column = new DataGridTemplateColumn
                {
                    Header = seriesName,
                    Width = new DataGridLength(80),
                    IsReadOnly = isLocked
                };

                // Create cell template with selection border
                var cellTemplate = new FuncDataTemplate<CurveDataRow>((row, _) =>
                {
                    if (row is null) return new TextBlock { Text = "0.00" };

                    var border = new Border
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = Brushes.Transparent,
                        Background = Brushes.Transparent,
                        Padding = new Thickness(2)
                    };

                    var textBlock = new TextBlock
                    {
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Margin = new Thickness(2),
                        Text = row.GetTorque(seriesName).ToString("N2")
                    };

                    border.Child = textBlock;

                    // Register the border for selection updates
                    var cellPos = new CellPosition(row.RowIndex, currentColumnIndex);
                    _cellBorders[cellPos] = border;

                    // Update visual based on current selection state
                    if (DataContext is MainWindowViewModel viewModel)
                    {
                        var isSelected = viewModel.CurveDataTableViewModel.IsCellSelected(row.RowIndex, currentColumnIndex);
                        UpdateCellBorderVisual(border, isSelected);
                    }

                    return border;
                });
                column.CellTemplate = cellTemplate;

                // Create editing template
                if (!isLocked)
                {
                    var editingTemplate = new FuncDataTemplate<CurveDataRow>((row, _) =>
                    {
                        var textBox = new TextBox
                        {
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Margin = new Thickness(2),
                            Text = row?.GetTorque(seriesName).ToString("N2") ?? "0.00",
                            BorderThickness = new Thickness(2),
                            BorderBrush = Brushes.White,
                            // Disable Avalonia's per-TextBox undo stack so that
                            // Ctrl+Z / Ctrl+Y bubble up to the window-level
                            // undo/redo commands backed by the shared UndoStack.
                            IsUndoEnabled = false
                        };

                        // Select all text when the TextBox is attached to visual tree (edit mode starts)
                        textBox.AttachedToVisualTree += (sender, e) =>
                        {
                            if (sender is TextBox tb)
                            {
                                tb.SelectAll();
                                tb.Focus();
                            }
                        };

                        // Commit edits via the view-model helper so that changes
                        // participate in the shared undo/redo history when an
                        // UndoStack is available.
                        textBox.LostFocus += (sender, e) =>
                        {
                            if (sender is not TextBox tb || row is null)
                            {
                                return;
                            }

                            if (!double.TryParse(tb.Text, out var newValue))
                            {
                                return;
                            }

                            if (DataContext is not MainWindowViewModel viewModel)
                            {
                                return;
                            }

                            var rowIndex = row.RowIndex;
                            var cellPos = new CellPosition(rowIndex, currentColumnIndex);

                            if (viewModel.CurveDataTableViewModel.TryApplyTorqueWithUndoForView(cellPos, newValue))
                            {
                                viewModel.MarkDirty();
                                viewModel.ChartViewModel.RefreshChart();
                            }
                        };

                        return textBox;
                    });
                    column.CellEditingTemplate = editingTemplate;
                }

                DataTable.Columns.Add(column);
                columnIndex++;
            }

            // Restore ItemsSource after columns are rebuilt
            DataTable.ItemsSource = savedItemsSource;
        }
        finally
        {
            _isRebuildingColumns = false;
        }
    }

    /// <summary>
    /// Gets the cell position from a pointer position.
    /// </summary>
    private CellPosition? GetCellPositionFromPoint(Point point)
    {
        if (DataContext is not MainWindowViewModel vm) return null;
        if (DataTable is null) return null;

        var rowIndex = -1;
        var columnIndex = -1;

        // Try to find elements at the point using visual tree traversal
        // First, try InputHitTest which is the standard approach
        var hitElement = DataTable.InputHitTest(point);

        // Walk up the visual tree to find the DataGridCell and DataGridRow
        // We capture the first (innermost) cell found as we traverse upward
        var element = hitElement as Visual;
        DataGridRow? row = null;
        DataGridCell? cell = null;

        while (element is not null)
        {
            // Capture the first DataGridCell encountered (closest to hit point)
            if (element is DataGridCell foundCell && cell is null)
            {
                cell = foundCell;
            }
            if (element is DataGridRow foundRow)
            {
                row = foundRow;
                break;
            }
            element = element.GetVisualParent();
        }

        // If we didn't find a row via InputHitTest, try alternative approach using GetVisualsAt
        // This can help when the click is on the border/padding between cells
        if (row is null)
        {
            var visualsAtPoint = DataTable.GetVisualsAt(point).ToList();
            foreach (var visual in visualsAtPoint)
            {
                // Only look for cell if we haven't found one yet
                if (visual is DataGridCell foundCell && cell is null)
                {
                    cell = foundCell;
                }
                if (visual is DataGridRow foundRow)
                {
                    row = foundRow;
                    break; // Found a row, stop searching
                }
            }
        }

        if (row is null) return null;

        // Get the row index from the data context
        if (row.DataContext is CurveDataRow dataRow)
        {
            rowIndex = dataRow.RowIndex;
        }
        else
        {
            return null;
        }

        // Get the column index from the cell if we found one
        if (cell is not null)
        {
            // Find the column index by looking at the cell's position in the row
            var cellsPresenter = row.GetVisualDescendants().OfType<DataGridCellsPresenter>().FirstOrDefault();
            if (cellsPresenter is not null)
            {
                var cells = cellsPresenter.GetVisualChildren().OfType<DataGridCell>().ToList();
                columnIndex = cells.IndexOf(cell);
            }
        }

        // Fallback: calculate column from X position if cell wasn't found or IndexOf failed
        if (columnIndex < 0)
        {
            var x = point.X;
            var accumulatedWidth = 0.0;
            for (var i = 0; i < DataTable.Columns.Count; i++)
            {
                var colWidth = DataTable.Columns[i].ActualWidth;
                if (colWidth <= 0) colWidth = 80; // Default width

                if (x >= accumulatedWidth && x < accumulatedWidth + colWidth)
                {
                    columnIndex = i;
                    break;
                }
                accumulatedWidth += colWidth;
            }
        }

        if (rowIndex < 0 || columnIndex < 0) return null;

        return new CellPosition(rowIndex, columnIndex);
    }

    /// <summary>
    /// Handles pointer press on the data table for cell selection.
    /// </summary>
    private void DataTable_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        if (DataTable is null) return;

        // If we're in Override Mode, commit it when clicking
        if (_isInOverrideMode)
        {
            CommitOverrideMode();
        }

        var point = e.GetPosition(DataTable);
        var cellPos = GetCellPositionFromPoint(point);

        if (cellPos is null) return;

        var pos = cellPos.Value;
        var properties = e.GetCurrentPoint(DataTable).Properties;

        if (properties.IsLeftButtonPressed)
        {
            // If a TextBox editor currently has focus inside the DataGrid, commit
            // its edit before we change the selection via mouse. This keeps the
            // DataGrid, view model selection, and border visuals in sync.
            if (TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is TextBox)
            {
                DataTable.CommitEdit();
            }

            // Check for double-click to enter edit mode
            // Let the event bubble through to DataGrid for native edit mode handling
            if (e.ClickCount >= 2)
            {
                // Cancel any ongoing drag operation
                _isDragging = false;
                _dragStartCell = null;

                // Select the cell in our tracking
                vm.CurveDataTableViewModel.SelectCell(pos.RowIndex, pos.ColumnIndex);
                // Snapshot the original value for this cell for potential Esc revert
                SnapshotOriginalValues(new[] { pos });
                _editCell = pos;
                // Ensure visuals are in sync before DataGrid enters edit mode
                UpdateCellSelectionVisuals();

                // Select the row and column in the DataGrid to enable editing the correct cell
                SelectDataGridCell(pos.RowIndex, pos.ColumnIndex);

                // DON'T set e.Handled = true - let the DataGrid handle the double-click
                // for its native edit mode functionality
                return;
            }

            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                // Ctrl+click: Toggle selection without clearing existing selection
                vm.CurveDataTableViewModel.ToggleCellSelection(pos.RowIndex, pos.ColumnIndex);
            }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                // Shift+click: Select range from anchor to clicked cell
                vm.CurveDataTableViewModel.SelectRange(pos.RowIndex, pos.ColumnIndex);
            }
            else
            {
                // Normal single click: Start new selection and begin drag tracking
                vm.CurveDataTableViewModel.SelectCell(pos.RowIndex, pos.ColumnIndex);
                UpdateCellSelectionVisuals();
                _isDragging = true;
                _dragStartCell = pos;

                // Capture pointer for reliable drag tracking
                e.Pointer.Capture(DataTable);
            }

            // Ensure the DataGrid has focus for keyboard events
            DataTable.Focus();

            e.Handled = true;
        }
    }

    /// <summary>
    /// Handles pointer move on the data table for drag selection.
    /// </summary>
    private void DataTable_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _dragStartCell is null) return;
        if (DataContext is not MainWindowViewModel vm) return;

        var point = e.GetPosition(DataTable);
        var cellPos = GetCellPositionFromPoint(point);

        if (cellPos is null) return;

        // Update selection to cover the range from drag start to current position
        vm.CurveDataTableViewModel.SelectRectangularRange(_dragStartCell.Value, cellPos.Value);
    }

    /// <summary>
    /// Handles pointer release on the data table.
    /// </summary>
    private void DataTable_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            // Release pointer capture
            e.Pointer.Capture(null);
        }

        _isDragging = false;
        _dragStartCell = null;
    }

    /// <summary>
    /// Handles when a cell edit is completed to trigger dirty state.
    /// </summary>
    private void DataGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel) return;

        // Note: % and RPM editing is now handled in the LostFocus event of their TextBox templates
        // This method just handles cleanup and ensuring the UI is in sync

        // Mark data as dirty when edited
        viewModel.MarkDirty();

        // Update cell selection visuals after edit ends
        // This ensures the white border is properly cleared/updated
        UpdateCellSelectionVisuals();
    }

    /// <summary>
    /// Handles double-tap on series name to enable renaming.
    /// </summary>
    private async void OnSeriesNameDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is TextBlock textBlock && textBlock.DataContext is Curve series)
        {
            // Show a simple input dialog for renaming
            var dialog = new Window
            {
                Title = "Rename Curves",
                Width = 300,
                Height = 120,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var textBox = new TextBox
            {
                Text = series.Name,
                Margin = new Thickness(16),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Margin = new Thickness(8, 0, 0, 0)
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Margin = new Thickness(16, 0, 16, 16)
            };
            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(okButton);

            var mainPanel = new StackPanel();
            mainPanel.Children.Add(textBox);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;

            var result = false;
            okButton.Click += (s, args) =>
            {
                result = true;
                dialog.Close();
            };
            cancelButton.Click += (s, args) => dialog.Close();

            if (TopLevel.GetTopLevel(this) is Window parentWindow)
            {
                await dialog.ShowDialog(parentWindow);

                if (result && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    series.Name = textBox.Text;
                    if (DataContext is MainWindowViewModel viewModel)
                    {
                        viewModel.MarkDirty();
                        viewModel.ChartViewModel.RefreshChart();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles visibility checkbox click to sync with chart.
    /// </summary>
    private void OnSeriesVisibilityCheckboxClick(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.DataContext is Curve series)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.ChartViewModel.SetSeriesVisibility(series.Name, series.IsVisible);
                viewModel.MarkDirty();
            }
        }
    }

    /// <summary>
    /// Handles lock toggle button click. Delegates to the
    /// MainWindowViewModel's ToggleSeriesLock command so that
    /// the operation participates in the shared undo/redo stack.
    /// </summary>
    private void OnSeriesLockToggleClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton toggleButton || toggleButton.DataContext is not Curve series)
        {
            return;
        }

        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        if (viewModel.ToggleSeriesLockCommand.CanExecute(series))
        {
            viewModel.ToggleSeriesLockCommand.Execute(series);
        }
    }

    /// <summary>
    /// Handles delete series button click.
    /// </summary>
    private async void OnDeleteSeriesClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Curve series)
        {
            if (DataContext is MainWindowViewModel viewModel && viewModel.SelectedVoltage is not null)
            {
                // Check if series is locked - prevent deletion of locked series
                if (series.Locked)
                {
                    viewModel.StatusMessage = "Cannot delete a locked series. Unlock it first.";
                    return;
                }

                // Show confirmation dialog
                var dialog = new MessageDialog
                {
                    Title = "Confirm Delete",
                    Message = $"Are you sure you want to delete the series '{series.Name}'?"
                };

                if (TopLevel.GetTopLevel(this) is Window parentWindow)
                {
                    await dialog.ShowDialog(parentWindow);
                }

                if (!dialog.IsConfirmed) return;

                // Remove the series from the voltage configuration (allow removing last series)
                var seriesName = series.Name;
                viewModel.SelectedVoltage.Curves.Remove(series);
                // IMPORTANT: RefreshData BEFORE RefreshAvailableSeriesPublic to prevent DataGrid column sync issues
                // The column rebuild is triggered by AvailableSeries collection change, so data must be ready first
                viewModel.CurveDataTableViewModel.RefreshData();
                viewModel.RefreshAvailableSeriesPublic();
                viewModel.SelectedSeries = viewModel.SelectedVoltage.Curves.FirstOrDefault();
                viewModel.ChartViewModel.RefreshChart();
                viewModel.MarkDirty();
                viewModel.StatusMessage = $"Removed series: {seriesName}";
            }
        }
    }

    /// <summary>
    /// Handles key down events in the data table.
    /// </summary>
    private void DataTable_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;
        if (DataContext is not MainWindowViewModel vm) return;

        // If we're in edit mode (event source is a TextBox), let most keys pass through
        // to allow normal text editing behavior
        var isInEditMode = e.Source is TextBox;

        if (isInEditMode)
        {
            // In edit mode, explicitly handle navigation keys so our
            // selection model and visuals stay in sync with DataGrid focus.
            switch (e.Key)
            {
                case Key.Escape:
                    // Revert the edited cell to its original value and exit edit mode
                    if (DataContext is MainWindowViewModel vmEsc && _editCell is CellPosition editCell && _editOriginalTorque is not null)
                    {
                        var originalValue = _editOriginalTorque.Value;

                        if (vmEsc.CurveDataTableViewModel.TrySetTorqueAtCell(editCell, originalValue))
                        {
                            if (_cellBorders.TryGetValue(editCell, out var border) && border.Child is TextBlock textBlock)
                            {
                                textBlock.Text = originalValue.ToString("N2");
                            }

                            vmEsc.ChartViewModel.RefreshChart();
                        }
                    }

                    dataGrid.CancelEdit();
                    // Do not move selection; just refresh visuals to clear edit artifacts
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    _editCell = null;
                    _editOriginalTorque = null;
                    return;

                case Key.Enter:
                    dataGrid.CommitEdit();
                    vm.CurveDataTableViewModel.MoveSelection(1, 0);
                    ScrollToSelection(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    _editCell = null;
                    return;

                case Key.Tab:
                    {
                        // Commit edit and move horizontally, honoring Shift for reverse tab
                        dataGrid.CommitEdit();
                        var deltaCol = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? -1 : 1;
                        vm.CurveDataTableViewModel.MoveSelection(0, deltaCol);
                        ScrollToSelection(dataGrid);
                        UpdateCellSelectionVisuals();
                        e.Handled = true;
                        _editCell = null;
                        return;
                    }

                case Key.Left:
                    dataGrid.CommitEdit();
                    vm.CurveDataTableViewModel.MoveSelection(0, -1);
                    ScrollToSelection(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    return;

                case Key.Right:
                    dataGrid.CommitEdit();
                    vm.CurveDataTableViewModel.MoveSelection(0, 1);
                    ScrollToSelection(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    return;

                case Key.Up:
                    dataGrid.CommitEdit();
                    vm.CurveDataTableViewModel.MoveSelection(-1, 0);
                    ScrollToSelection(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    return;

                case Key.Down:
                    dataGrid.CommitEdit();
                    vm.CurveDataTableViewModel.MoveSelection(1, 0);
                    ScrollToSelection(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    return;
            }

            // Let all other keys pass through to the TextBox for normal editing
            return;
        }

        // Handle Override Mode key events
        if (_isInOverrideMode)
        {
            if (e.Key == Key.Enter)
            {
                // Commit override and move selection down one row, matching
                // the normal Enter navigation behavior from non-override mode.
                CommitOverrideMode();
                vm.CurveDataTableViewModel.MoveSelection(1, 0);
                ScrollToSelection(dataGrid);
                UpdateCellSelectionVisuals();
                e.Handled = true;
                return;
            }
            else if (e.Key == Key.Tab)
            {
                // Commit override and move selection horizontally, honoring
                // Shift for reverse tab navigation, consistent with edit mode.
                CommitOverrideMode();
                var deltaCol = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? -1 : 1;
                vm.CurveDataTableViewModel.MoveSelection(0, deltaCol);
                ScrollToSelection(dataGrid);
                UpdateCellSelectionVisuals();
                e.Handled = true;
                return;
            }
            else if (e.Key == Key.Escape)
            {
                // Cancel override without applying
                CancelOverrideMode();
                e.Handled = true;
                return;
            }
            else if (e.Key == Key.Back)
            {
                // Handle backspace in Override Mode - remove last character
                if (_overrideText.Length > 0)
                {
                    _overrideText = _overrideText[..^1];
                    if (_overrideText.Length == 0)
                    {
                        // If all text deleted, exit Override Mode and restore original values
                        _isInOverrideMode = false;
                        RestoreOriginalValues();
                        ForceDataGridRefresh(dataGrid);
                    }
                    else
                    {
                        // Update cell displays with remaining text
                        UpdateOverrideModeDisplay();
                        ForceDataGridRefresh(dataGrid);
                    }
                }
                e.Handled = true;
                return;
            }
            // Arrow keys: commit the current override and then move the
            // selection in the corresponding direction, matching the
            // normal navigation behavior for non-override mode.
            else if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right)
            {
                CommitOverrideMode();

                switch (e.Key)
                {
                    case Key.Up:
                        vm.CurveDataTableViewModel.MoveSelection(-1, 0);
                        break;
                    case Key.Down:
                        vm.CurveDataTableViewModel.MoveSelection(1, 0);
                        break;
                    case Key.Left:
                        vm.CurveDataTableViewModel.MoveSelection(0, -1);
                        break;
                    case Key.Right:
                        vm.CurveDataTableViewModel.MoveSelection(0, 1);
                        break;
                }

                ScrollToSelection(dataGrid);
                UpdateCellSelectionVisuals();
                e.Handled = true;
                return;
            }

            // Handle character input while in Override Mode
            var charInOverride = GetCharacterFromKey(e.Key, e.KeyModifiers);
            if (charInOverride is not null)
            {
                // Enforce a single leading minus sign and a single
                // decimal separator within the override text.
                if (charInOverride == '-' && _overrideText.Length > 0)
                {
                    e.Handled = true;
                    return;
                }

                if (charInOverride == '.' && _overrideText.Contains('.'))
                {
                    e.Handled = true;
                    return;
                }

                _overrideText += charInOverride;
                UpdateOverrideModeDisplay();
                ForceDataGridRefresh(dataGrid);
                e.Handled = true;
                return;
            }

            // Ignore all other keys while in Override Mode
            return;
        }

        // Handle clipboard operations
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            switch (e.Key)
            {
                case Key.C:
                    CopySelectedCells(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    break;
                case Key.V:
                    PasteToSelectedCells(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    break;
                case Key.X:
                    CutSelectedCells(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    break;
            }
        }

        // Handle delete/backspace to clear cells (when not in Override Mode)
        if (e.Key == Key.Delete || e.Key == Key.Back)
        {
            ClearSelectedCells(dataGrid);
            UpdateCellSelectionVisuals();
            e.Handled = true;
        }

        // Handle Enter key to move down
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            vm.CurveDataTableViewModel.MoveSelection(1, 0);
            ScrollToSelection(dataGrid);
            UpdateCellSelectionVisuals();
            e.Handled = true;
        }

        // Handle F2 to enter edit mode
        if (e.Key == Key.F2)
        {
            // Select the row and column in the DataGrid to enable editing the correct cell
            var selectedCells = vm.CurveDataTableViewModel.SelectedCells;
            if (selectedCells.Count > 0)
            {
                var firstCell = selectedCells.First();
                SelectDataGridCell(firstCell.RowIndex, firstCell.ColumnIndex);
                // DON'T set e.Handled = true - let the DataGrid's native F2 handler work
                // The BeginEdit() call doesn't work properly when using tunnel routing
            }
            return; // Early return to prevent other handlers from running
        }

        // Handle arrow keys for navigation and selection extension
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            // Ctrl+Shift+Arrow: Extend selection to end of row/column
            switch (e.Key)
            {
                case Key.Up:
                    vm.CurveDataTableViewModel.ExtendSelectionToEnd(-1, 0);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
                case Key.Down:
                    vm.CurveDataTableViewModel.ExtendSelectionToEnd(1, 0);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
                case Key.Left:
                    vm.CurveDataTableViewModel.ExtendSelectionToEnd(0, -1);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
                case Key.Right:
                    vm.CurveDataTableViewModel.ExtendSelectionToEnd(0, 1);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
            }
        }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            // Shift+Arrow: Extend selection by one cell
            switch (e.Key)
            {
                case Key.Up:
                    vm.CurveDataTableViewModel.ExtendSelection(-1, 0);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
                case Key.Down:
                    vm.CurveDataTableViewModel.ExtendSelection(1, 0);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
                case Key.Left:
                    vm.CurveDataTableViewModel.ExtendSelection(0, -1);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
                case Key.Right:
                    vm.CurveDataTableViewModel.ExtendSelection(0, 1);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
            }
        }
        else
        {
            // Arrow: Move selection
            switch (e.Key)
            {
                case Key.Up:
                    Log.Debug("[NAV] Arrow Up pressed");
                    vm.CurveDataTableViewModel.MoveSelection(-1, 0);
                    ScrollToSelection(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    break;
                case Key.Down:
                    Log.Debug("[NAV] Arrow Down pressed");
                    vm.CurveDataTableViewModel.MoveSelection(1, 0);
                    ScrollToSelection(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    break;
                case Key.Left:
                    Log.Debug("[NAV] Arrow Left pressed");
                    vm.CurveDataTableViewModel.MoveSelection(0, -1);
                    ScrollToSelection(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    break;
                case Key.Right:
                    Log.Debug("[NAV] Arrow Right pressed");
                    vm.CurveDataTableViewModel.MoveSelection(0, 1);
                    ScrollToSelection(dataGrid);
                    UpdateCellSelectionVisuals();
                    e.Handled = true;
                    break;
            }
        }

        // Handle character input for Override Mode (start new override)
        // If the key is a digit, minus, or decimal point, enter Override Mode
        var character = GetCharacterFromKey(e.Key, e.KeyModifiers);
        if (character is not null && vm.CurveDataTableViewModel.SelectedCells.Count > 0)
        {
            if (!_isInOverrideMode)
            {
                _isInOverrideMode = true;
                _overrideText = string.Empty;
            }

            // Enforce a single leading minus sign and a single decimal
            // separator even when starting a new override via key down.
            if (character == '-' && _overrideText.Length > 0)
            {
                e.Handled = true;
                return;
            }

            if (character == '.' && _overrideText.Contains('.'))
            {
                e.Handled = true;
                return;
            }

            _overrideText += character;

            // Update cell displays immediately as user types
            UpdateOverrideModeDisplay();
            ForceDataGridRefresh(dataGrid);

            e.Handled = true;
        }
    }

    /// <summary>
    /// Converts a key to a character for Override Mode input.
    /// Returns null if the key is not a valid input character (digit, minus, decimal point).
    /// </summary>
    private static char? GetCharacterFromKey(Key key, KeyModifiers modifiers)
    {
        // Only handle unmodified keys (Ctrl, Alt, and Shift all produce different characters)
        if (modifiers.HasFlag(KeyModifiers.Control) ||
            modifiers.HasFlag(KeyModifiers.Alt) ||
            modifiers.HasFlag(KeyModifiers.Shift))
            return null;

        return key switch
        {
            Key.D0 => '0',
            Key.D1 => '1',
            Key.D2 => '2',
            Key.D3 => '3',
            Key.D4 => '4',
            Key.D5 => '5',
            Key.D6 => '6',
            Key.D7 => '7',
            Key.D8 => '8',
            Key.D9 => '9',
            Key.NumPad0 => '0',
            Key.NumPad1 => '1',
            Key.NumPad2 => '2',
            Key.NumPad3 => '3',
            Key.NumPad4 => '4',
            Key.NumPad5 => '5',
            Key.NumPad6 => '6',
            Key.NumPad7 => '7',
            Key.NumPad8 => '8',
            Key.NumPad9 => '9',
            Key.OemMinus => '-',
            Key.Subtract => '-',
            Key.OemPeriod => '.',
            Key.Decimal => '.',
            _ => null
        };
    }

    private void ScrollToSelection(DataGrid dataGrid)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var selectedCells = vm.CurveDataTableViewModel.SelectedCells;
        if (selectedCells.Count == 0) return;

        var firstSelected = selectedCells.First();
        Log.Debug("[NAV] ScrollToSelection: Row={Row}, Col={Col}", firstSelected.RowIndex, firstSelected.ColumnIndex);
        
        if (firstSelected.RowIndex >= 0 && firstSelected.RowIndex < vm.CurveDataTableViewModel.Rows.Count)
        {
            var row = vm.CurveDataTableViewModel.Rows[firstSelected.RowIndex];
            // Ensure the DataGrid has a current row before updating CurrentColumn to avoid InvalidOperationException.
            dataGrid.SelectedIndex = firstSelected.RowIndex;
            dataGrid.SelectedItem = row;
            
            // Update DataGrid's CurrentColumn to keep navigation in sync
            // but don't pass it to ScrollIntoView to avoid DataGrid's native selection
            if (dataGrid.SelectedItem is not null &&
                firstSelected.ColumnIndex >= 0 &&
                firstSelected.ColumnIndex < dataGrid.Columns.Count)
            {
                try
                {
                    var oldColumn = dataGrid.CurrentColumn?.DisplayIndex ?? -1;
                    dataGrid.CurrentColumn = dataGrid.Columns[firstSelected.ColumnIndex];
                    Log.Debug("[NAV] Updated CurrentColumn from {OldCol} to {NewCol}", oldColumn, firstSelected.ColumnIndex);
                }
                catch (InvalidOperationException ex)
                {
                    Log.Warning(ex, "[NAV] Skipped updating CurrentColumn because no current row was active");
                }
            }
            
            // Pass null for column to avoid DataGrid showing its own selection border
            dataGrid.ScrollIntoView(row, null);
        }
    }

    /// <summary>
    /// Selects a row and column in the DataGrid to enable editing operations.
    /// </summary>
    private void SelectDataGridCell(int rowIndex, int columnIndex)
    {
        if (DataContext is not MainWindowViewModel vm || DataTable is null) return;

        if (rowIndex >= 0 && rowIndex < vm.CurveDataTableViewModel.Rows.Count)
        {
            var row = vm.CurveDataTableViewModel.Rows[rowIndex];
            DataTable.SelectedItem = row;

            // Set the current column if within bounds
            if (columnIndex >= 0 && columnIndex < DataTable.Columns.Count)
            {
                DataTable.CurrentColumn = DataTable.Columns[columnIndex];
            }

            DataTable.ScrollIntoView(row, DataTable.CurrentColumn);
        }
    }

    /// <summary>
    /// Selects a row in the DataGrid by index to enable editing operations.
    /// </summary>
    private void SelectDataGridRow(int rowIndex)
    {
        if (DataContext is not MainWindowViewModel vm || DataTable is null) return;

        if (rowIndex >= 0 && rowIndex < vm.CurveDataTableViewModel.Rows.Count)
        {
            var row = vm.CurveDataTableViewModel.Rows[rowIndex];
            DataTable.SelectedItem = row;
            DataTable.ScrollIntoView(row, null);
        }
    }

    private void CopySelectedCells(DataGrid dataGrid)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var selectedCells = vm.CurveDataTableViewModel.SelectedCells;
        if (selectedCells.Count == 0)
        {
            return;
        }

        // Get unique rows and columns
        var rows = selectedCells.Select(c => c.RowIndex).Distinct().OrderBy(r => r).ToList();
        var cols = selectedCells.Select(c => c.ColumnIndex).Distinct().OrderBy(c => c).ToList();

        var lines = new List<string>();
        foreach (var rowIndex in rows)
        {
            var values = new List<string>();
            foreach (var colIndex in cols)
            {
                if (!selectedCells.Contains(new CellPosition(rowIndex, colIndex)))
                {
                    values.Add(string.Empty);
                    continue;
                }

                var row = vm.CurveDataTableViewModel.Rows.ElementAtOrDefault(rowIndex);
                if (row is null)
                {
                    values.Add(string.Empty);
                    continue;
                }

                // Get value based on column
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
                    var seriesName = vm.CurveDataTableViewModel.GetSeriesNameForColumn(colIndex);
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

        var clipboardText = string.Join(Environment.NewLine, lines);
        TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(clipboardText);
    }

    private async void PasteToSelectedCells(DataGrid dataGrid)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
        {
            return;
        }

#pragma warning disable CS0618 // GetTextAsync is obsolete
        var text = await clipboard.GetTextAsync();
#pragma warning restore CS0618
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (!vm.CurveDataTableViewModel.TryPasteClipboard(text))
        {
            return;
        }

        // When the view-model accepts the paste, mark the document dirty and
        // refresh the chart.
        vm.MarkDirty();
        vm.ChartViewModel.RefreshChart();

        // Immediately update visible cells for the current selection so users
        // see pasted values without needing to scroll.
        foreach (var cellPos in vm.CurveDataTableViewModel.SelectedCells)
        {
            var rowIndex = cellPos.RowIndex;
            if (rowIndex < 0 || rowIndex >= vm.CurveDataTableViewModel.Rows.Count)
            {
                continue;
            }

            var row = vm.CurveDataTableViewModel.Rows[rowIndex];

            // Handle % column (column 0)
            if (cellPos.ColumnIndex == 0)
            {
                if (_cellBorders.TryGetValue(cellPos, out var border) && border.Child is TextBlock textBlock)
                {
                    textBlock.Text = row.Percent.ToString();
                }
                continue;
            }

            // Handle RPM column (column 1)
            if (cellPos.ColumnIndex == 1)
            {
                if (_cellBorders.TryGetValue(cellPos, out var border) && border.Child is TextBlock textBlock)
                {
                    textBlock.Text = row.DisplayRpm.ToString();
                }
                continue;
            }

            // Handle torque columns (column 2+)
            var seriesName = vm.CurveDataTableViewModel.GetSeriesNameForColumn(cellPos.ColumnIndex);
            if (seriesName is null)
            {
                continue;
            }

            var value = row.GetTorque(seriesName);
            if (_cellBorders.TryGetValue(cellPos, out var torqueBorder) && torqueBorder.Child is TextBlock torqueTextBlock)
            {
                torqueTextBlock.Text = value.ToString("N2");
            }
        }

        // As a fallback for virtualized rows/columns, force a grid refresh.
        ForceDataGridRefresh(dataGrid);
    }

    private void CutSelectedCells(DataGrid dataGrid)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        // Copy first so clipboard reflects the pre-cut values
        CopySelectedCells(dataGrid);

        // Then clear the cells through the view-model helper
        vm.CurveDataTableViewModel.ClearSelectedTorqueCells();
        vm.MarkDirty();
        vm.ChartViewModel.RefreshChart();

        // Immediately update visible cells to reflect cleared values (0.00)
        foreach (var cellPos in vm.CurveDataTableViewModel.SelectedCells)
        {
            if (cellPos.ColumnIndex < 2)
            {
                continue;
            }

            if (_cellBorders.TryGetValue(cellPos, out var border) && border.Child is TextBlock textBlock)
            {
                textBlock.Text = "0.00";
            }
        }

        // Ensure any virtualized rows/columns are also refreshed
        ForceDataGridRefresh(dataGrid);
    }

    private void ClearSelectedCells(DataGrid dataGrid)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        vm.CurveDataTableViewModel.ClearSelectedTorqueCells();
        vm.MarkDirty();
        vm.ChartViewModel.RefreshChart();

        // Keep the visual representation in sync after clearing cells.
        ForceDataGridRefresh(dataGrid);
    }

    /// <summary>
    /// Handles text input for Override Mode.
    /// When cells are selected and user types, the typed value replaces all selected cells.
    /// </summary>
    private void DataTable_TextInput(object? sender, TextInputEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        // Don't handle if we're in edit mode (TextBox is focused)
        var isInEditMode = e.Source is TextBox;
        if (isInEditMode) return;

        // If we're already in Override Mode, rely on key handling for
        // additional characters so we don't double-append input.
        if (_isInOverrideMode) return;

        // Don't handle if no cells selected
        var selectedCells = vm.CurveDataTableViewModel.SelectedCells;
        if (selectedCells.Count == 0) return;

        var text = e.Text;
        if (string.IsNullOrEmpty(text)) return;

        // Only start Override Mode for numeric-like characters: digits,
        // a single leading minus sign, or a decimal separator. This
        // ensures users cannot write arbitrary non-numeric text into
        // torque cells when beginning an override.
        if (text.Length != 1)
        {
            return;
        }

        var ch = text[0];
        var isDigit = char.IsDigit(ch);
        var isMinus = ch == '-';
        var isDecimal = ch == '.';
        if (!isDigit && !isMinus && !isDecimal)
        {
            // Let non-numeric first characters fall through so they are
            // not written into the cells at all.
            return;
        }

        // Enter Override Mode and accumulate typed text
        _isInOverrideMode = true;
        _overrideText = text;

        // Immediately update the display so the user sees exactly what
        // they typed, even if it is not a valid number. Validation and
        // potential revert happen when Override Mode is committed or
        // cancelled.
        UpdateOverrideModeDisplay();

        if (sender is DataGrid grid)
        {
            ForceDataGridRefresh(grid);
        }
        else if (DataTable is not null)
        {
            ForceDataGridRefresh(DataTable);
        }

        // Prevent the event from reaching the DataGrid
        e.Handled = true;
    }

    /// <summary>
    /// Commits the Override Mode text to all selected cells and exits Override Mode.
    /// If the text cannot be parsed as a number, the original values are restored.
    /// Now supports %, RPM, and torque columns with undo/redo.
    /// </summary>
    private void CommitOverrideMode()
    {
        if (!_isInOverrideMode) return;
        if (DataContext is not MainWindowViewModel vm) return;

        // Try to parse the accumulated text as a number
        if (double.TryParse(_overrideText, out var value))
        {
            // Separate % and RPM cells from torque cells
            var percentCells = new List<CellPosition>();
            var rpmCells = new List<CellPosition>();
            var torqueCells = new Dictionary<CellPosition, double>();

            foreach (var kvp in _originalValues)
            {
                var cell = kvp.Key;
                
                if (cell.ColumnIndex == 0)
                {
                    percentCells.Add(cell);
                }
                else if (cell.ColumnIndex == 1)
                {
                    rpmCells.Add(cell);
                }
                else
                {
                    torqueCells[cell] = kvp.Value;
                }
            }

            // Commit % cells with undo support
            if (percentCells.Count > 0 && value >= 0 && value == Math.Floor(value))
            {
                var intValue = (int)value;
                foreach (var cell in percentCells)
                {
                    if (cell.RowIndex >= 0 && cell.RowIndex < vm.CurveDataTableViewModel.Rows.Count)
                    {
                        vm.CurveDataTableViewModel.UpdatePercent(cell.RowIndex, intValue);
                    }
                }
                vm.MarkDirty();
            }

            // Commit RPM cells with undo support
            if (rpmCells.Count > 0 && value >= 0)
            {
                foreach (var cell in rpmCells)
                {
                    if (cell.RowIndex >= 0 && cell.RowIndex < vm.CurveDataTableViewModel.Rows.Count)
                    {
                        vm.CurveDataTableViewModel.UpdateRpm(cell.RowIndex, value);
                    }
                }
                vm.MarkDirty();
            }

            // Commit torque cells with undo support
            if (torqueCells.Count > 0)
            {
                var committedViaUndo = vm.CurveDataTableViewModel.TryCommitOverrideWithUndo(torqueCells, value);

                // If the view-model did not route through the undo stack (for
                // example, in environments without an UndoStack), fall back to
                // simply marking the document dirty so validation and UI state
                // stay consistent with prior behavior.
                if (!committedViaUndo)
                {
                    vm.MarkDirty();
                }
            }

            // Update chart after all edits
            if (_originalValues.Count > 0)
            {
                vm.ChartViewModel.RefreshChart();
            }
        }
        else
        {
            // If parsing failed, restore original values
            RestoreOriginalValues();
        }

        // Exit Override Mode and clean up
        _isInOverrideMode = false;
        _overrideText = string.Empty;
        _originalValues.Clear();
    }

    /// <summary>
    /// Cancels Override Mode without applying changes and restores original values.
    /// </summary>
    private void CancelOverrideMode()
    {
        RestoreOriginalValues();
        _isInOverrideMode = false;
        _overrideText = string.Empty;
        _originalValues.Clear();
    }

    /// <summary>
    /// Updates the display of all selected cells to show the current override text.
    /// This provides real-time visual feedback as the user types.
    /// Now supports %, RPM, and torque columns.
    /// </summary>
    private void UpdateOverrideModeDisplay()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var selectedCells = vm.CurveDataTableViewModel.SelectedCells;
        if (selectedCells.Count == 0) return;

        // Store original values on first character entry
        if (_overrideText.Length == 1)
        {
            SnapshotOriginalValues(selectedCells);
        }

        // If the text parses, update the underlying values and show
        // the formatted numeric value. Otherwise, leave the model unchanged
        // but show the raw typed text so the user sees exactly what they
        // entered; validation and revert happen when Override Mode exits.
        if (double.TryParse(_overrideText, out var value))
        {
            foreach (var cellPos in selectedCells)
            {
                var rowIndex = cellPos.RowIndex;
                if (rowIndex < 0 || rowIndex >= vm.CurveDataTableViewModel.Rows.Count)
                {
                    continue;
                }

                var row = vm.CurveDataTableViewModel.Rows[rowIndex];

                // Handle % column (column 0)
                if (cellPos.ColumnIndex == 0)
                {
                    if (!vm.CurveDataTableViewModel.IsPercentLocked() && value >= 0 && value == Math.Floor(value))
                    {
                        var intValue = (int)value;
                        // Update the value directly (will be committed to undo stack later)
                        if (_cellBorders.TryGetValue(cellPos, out var border) && border.Child is TextBlock textBlock)
                        {
                            textBlock.Text = intValue.ToString();
                        }
                    }
                    continue;
                }

                // Handle RPM column (column 1)
                if (cellPos.ColumnIndex == 1)
                {
                    if (!vm.CurveDataTableViewModel.IsRpmLocked() && value >= 0)
                    {
                        // Update the value directly (will be committed to undo stack later)
                        if (_cellBorders.TryGetValue(cellPos, out var border) && border.Child is TextBlock textBlock)
                        {
                            textBlock.Text = Math.Round(value).ToString();
                        }
                    }
                    continue;
                }

                // Handle torque columns (column 2+)
                var seriesName = vm.CurveDataTableViewModel.GetSeriesNameForColumn(cellPos.ColumnIndex);
                if (seriesName is null)
                {
                    continue;
                }

                // Delegate mutation rules to the view model helper, then update
                // visible cells for immediate feedback.
                vm.CurveDataTableViewModel.ApplyOverrideValue(value);

                var torque = row.GetTorque(seriesName);
                if (_cellBorders.TryGetValue(cellPos, out var torqueBorder) && torqueBorder.Child is TextBlock torqueTextBlock)
                {
                    torqueTextBlock.Text = torque.ToString("N2");
                }
            }

            // Refresh chart to show updated values
            vm.ChartViewModel.RefreshChart();
        }
        else
        {
            // Show raw text for all selected cells when not a valid number
            foreach (var cellPos in selectedCells)
            {
                if (_cellBorders.TryGetValue(cellPos, out var border) && border.Child is TextBlock textBlock)
                {
                    textBlock.Text = _overrideText;
                }
            }
        }
    }

    /// <summary>
    /// Restores the original values that were stored when Override Mode started.
    /// </summary>
    private void RestoreOriginalValues()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        foreach (var kvp in _originalValues)
        {
            var cellPos = kvp.Key;
            var originalValue = kvp.Value;

            // Handle % column (column 0)
            if (cellPos.ColumnIndex == 0)
            {
                var intValue = (int)originalValue;
                if (cellPos.RowIndex >= 0 && cellPos.RowIndex < vm.CurveDataTableViewModel.Rows.Count)
                {
                    vm.CurveDataTableViewModel.UpdatePercent(cellPos.RowIndex, intValue);
                }

                if (_cellBorders.TryGetValue(cellPos, out var border) && border.Child is TextBlock textBlock)
                {
                    textBlock.Text = intValue.ToString();
                }
                continue;
            }

            // Handle RPM column (column 1)
            if (cellPos.ColumnIndex == 1)
            {
                if (cellPos.RowIndex >= 0 && cellPos.RowIndex < vm.CurveDataTableViewModel.Rows.Count)
                {
                    vm.CurveDataTableViewModel.UpdateRpm(cellPos.RowIndex, originalValue);
                }

                if (_cellBorders.TryGetValue(cellPos, out var border) && border.Child is TextBlock textBlock)
                {
                    textBlock.Text = Math.Round(originalValue).ToString();
                }
                continue;
            }

            // Handle torque columns (column 2+)
            if (!vm.CurveDataTableViewModel.TrySetTorqueAtCell(cellPos, originalValue))
            {
                continue;
            }

            // Directly update the TextBlock in the cell for immediate visual feedback
            if (_cellBorders.TryGetValue(cellPos, out var torqueBorder) && torqueBorder.Child is TextBlock torqueTextBlock)
            {
                torqueTextBlock.Text = originalValue.ToString("N2");
            }
        }

        // Refresh chart to show restored values
        vm.ChartViewModel.RefreshChart();
    }

    /// <summary>
    /// Applies a value to all selected cells.
    /// </summary>
    private void ApplyValueToSelectedCells(double value)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var selectedCells = vm.CurveDataTableViewModel.SelectedCells;
        if (selectedCells.Count == 0) return;

        foreach (var cellPos in selectedCells)
        {
            vm.CurveDataTableViewModel.TrySetTorqueAtCell(cellPos, value);
        }

        vm.MarkDirty();
        vm.ChartViewModel.RefreshChart();
    }

    /// <summary>
    /// Forces the DataGrid to refresh its visual display.
    /// This is needed because virtualized cells don't always update when data changes.
    /// </summary>
    private static void ForceDataGridRefresh(DataGrid dataGrid)
    {
        // Force visual refresh by invalidating the visual
        dataGrid.InvalidateVisual();

        // Force re-measure and re-arrange which causes cells to re-render
        dataGrid.InvalidateMeasure();
        dataGrid.InvalidateArrange();

        // Update layout immediately
        dataGrid.UpdateLayout();
    }

    /// <summary>
    /// Updates the visual state of the lock toggle buttons for % and RPM columns.
    /// </summary>
    private void UpdateLockToggleStates(bool isPercentLocked, bool isRpmLocked)
    {
        if (PercentLockToggle is not null)
        {
            PercentLockToggle.IsChecked = isPercentLocked;
            if (PercentLockIcon is not null)
            {
                PercentLockIcon.IsVisible = isPercentLocked;
            }
            if (PercentUnlockIcon is not null)
            {
                PercentUnlockIcon.IsVisible = !isPercentLocked;
            }
        }

        if (RpmLockToggle is not null)
        {
            RpmLockToggle.IsChecked = isRpmLocked;
            if (RpmLockIcon is not null)
            {
                RpmLockIcon.IsVisible = isRpmLocked;
            }
            if (RpmUnlockIcon is not null)
            {
                RpmUnlockIcon.IsVisible = !isRpmLocked;
            }
        }
    }

    /// <summary>
    /// Handles percent lock toggle button click for all series.
    /// Toggles the LockedPercent property for all curves in the current voltage.
    /// </summary>
    private void OnPercentLockToggleClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel || viewModel.SelectedVoltage is null)
        {
            return;
        }

        // Determine current lock state (if any series allows percent editing, we consider it unlocked)
        var isCurrentlyLocked = viewModel.SelectedVoltage.Curves.All(s => s.LockedPercent);
        var newLockedState = !isCurrentlyLocked;

        // Apply to all curves in the voltage
        foreach (var curve in viewModel.SelectedVoltage.Curves)
        {
            curve.LockedPercent = newLockedState;
        }

        // Update the DataGrid columns to reflect the new lock state
        RebuildDataGridColumns();
        
        viewModel.MarkDirty();
        viewModel.StatusMessage = newLockedState 
            ? "Locked % column for all series" 
            : "Unlocked % column for all series";
    }

    /// <summary>
    /// Handles RPM lock toggle button click for all series.
    /// Toggles the LockedRpm property for all curves in the current voltage.
    /// </summary>
    private void OnRpmLockToggleClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel || viewModel.SelectedVoltage is null)
        {
            return;
        }

        // Determine current lock state (if any series allows RPM editing, we consider it unlocked)
        var isCurrentlyLocked = viewModel.SelectedVoltage.Curves.All(s => s.LockedRpm);
        var newLockedState = !isCurrentlyLocked;

        // Apply to all curves in the voltage
        foreach (var curve in viewModel.SelectedVoltage.Curves)
        {
            curve.LockedRpm = newLockedState;
        }

        // Update the DataGrid columns to reflect the new lock state
        RebuildDataGridColumns();
        
        viewModel.MarkDirty();
        viewModel.StatusMessage = newLockedState 
            ? "Locked RPM column for all series" 
            : "Unlocked RPM column for all series";
    }
}
