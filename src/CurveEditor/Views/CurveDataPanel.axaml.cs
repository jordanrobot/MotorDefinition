using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using CurveEditor.Models;
using CurveEditor.ViewModels;

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
        
        DataTable.AddHandler(PointerPressedEvent, DataTable_PointerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        DataTable.AddHandler(PointerMovedEvent, DataTable_PointerMoved, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        DataTable.AddHandler(PointerReleasedEvent, DataTable_PointerReleased, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        DataTable.AddHandler(KeyDownEvent, DataTable_KeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
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
            DataTable.RemoveHandler(PointerPressedEvent, DataTable_PointerPressed);
            DataTable.RemoveHandler(PointerMovedEvent, DataTable_PointerMoved);
            DataTable.RemoveHandler(PointerReleasedEvent, DataTable_PointerReleased);
            DataTable.RemoveHandler(KeyDownEvent, DataTable_KeyDown);
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

    private void UpdateCellSelectionVisuals()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        // First, reset all registered borders to unselected state
        // This handles cases where a border was selected but is no longer in the selection
        var bordersToRemove = new List<CellPosition>();
        
        foreach (var kvp in _cellBorders)
        {
            // Check if the border is still valid (has a visual parent)
            if (kvp.Value.GetVisualParent() is null)
            {
                // Border is no longer in the visual tree, mark for removal
                bordersToRemove.Add(kvp.Key);
                continue;
            }
            
            var isSelected = vm.CurveDataTableViewModel.IsCellSelected(kvp.Key.RowIndex, kvp.Key.ColumnIndex);
            UpdateCellBorderVisual(kvp.Value, isSelected);
        }
        
        // Clean up stale entries
        foreach (var pos in bordersToRemove)
        {
            _cellBorders.Remove(pos);
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
            
            // Remove all columns except the first two (% and RPM)
            while (DataTable.Columns.Count > 2)
            {
                DataTable.Columns.RemoveAt(DataTable.Columns.Count - 1);
            }

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
                            BorderBrush = Brushes.White
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
                        
                        // Handle text changes to update the underlying data and refresh chart
                        textBox.LostFocus += (sender, e) =>
                        {
                            if (sender is TextBox tb && row is not null)
                            {
                                if (double.TryParse(tb.Text, out var newValue))
                                {
                                    row.SetTorque(seriesName, newValue);
                                    
                                    // Update chart and mark dirty when focus is lost
                                    if (DataContext is MainWindowViewModel viewModel)
                                    {
                                        viewModel.MarkDirty();
                                        viewModel.ChartViewModel.RefreshChart();
                                    }
                                }
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
        
        var point = e.GetPosition(DataTable);
        var cellPos = GetCellPositionFromPoint(point);
        
        if (cellPos is null) return;

        var pos = cellPos.Value;
        var properties = e.GetCurrentPoint(DataTable).Properties;

        if (properties.IsLeftButtonPressed)
        {
            // Check for double-click to enter edit mode
            // Let the event bubble through to DataGrid for native edit mode handling
            if (e.ClickCount >= 2)
            {
                // Cancel any ongoing drag operation
                _isDragging = false;
                _dragStartCell = null;
                
                // Select the cell in our tracking
                vm.CurveDataTableViewModel.SelectCell(pos.RowIndex, pos.ColumnIndex);
                
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
        // Mark data as dirty when edited
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.MarkDirty();
            viewModel.ChartViewModel.RefreshChart();
        }
        
        // Update cell selection visuals after edit ends
        // This ensures the white border is properly cleared/updated
        UpdateCellSelectionVisuals();
    }

    /// <summary>
    /// Handles double-tap on series name to enable renaming.
    /// </summary>
    private async void OnSeriesNameDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is TextBlock textBlock && textBlock.DataContext is CurveSeries series)
        {
            // Show a simple input dialog for renaming
            var dialog = new Window
            {
                Title = "Rename Series",
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
        if (sender is CheckBox checkBox && checkBox.DataContext is CurveSeries series)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.ChartViewModel.SetSeriesVisibility(series.Name, series.IsVisible);
                viewModel.MarkDirty();
            }
        }
    }

    /// <summary>
    /// Handles lock toggle button click.
    /// </summary>
    private void OnSeriesLockToggleClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.MarkDirty();
            // Rebuild columns to update read-only state
            RebuildDataGridColumns();
        }
    }

    /// <summary>
    /// Handles delete series button click.
    /// </summary>
    private async void OnDeleteSeriesClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is CurveSeries series)
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
                viewModel.SelectedVoltage.Series.Remove(series);
                // IMPORTANT: RefreshData BEFORE RefreshAvailableSeriesPublic to prevent DataGrid column sync issues
                // The column rebuild is triggered by AvailableSeries collection change, so data must be ready first
                viewModel.CurveDataTableViewModel.RefreshData();
                viewModel.RefreshAvailableSeriesPublic();
                viewModel.SelectedSeries = viewModel.SelectedVoltage.Series.FirstOrDefault();
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
            // In edit mode, only handle Escape to cancel and Enter to commit
            if (e.Key == Key.Escape)
            {
                dataGrid.CancelEdit();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                dataGrid.CommitEdit();
                vm.CurveDataTableViewModel.MoveSelection(1, 0);
                ScrollToSelection(dataGrid);
                e.Handled = true;
            }
            // Let all other keys pass through to the TextBox for normal editing
            return;
        }

        // Handle clipboard operations
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            switch (e.Key)
            {
                case Key.C:
                    CopySelectedCells(dataGrid);
                    e.Handled = true;
                    break;
                case Key.V:
                    PasteToSelectedCells(dataGrid);
                    e.Handled = true;
                    break;
                case Key.X:
                    CutSelectedCells(dataGrid);
                    e.Handled = true;
                    break;
            }
        }

        // Handle delete/backspace to clear cells
        if (e.Key == Key.Delete || e.Key == Key.Back)
        {
            ClearSelectedCells(dataGrid);
            e.Handled = true;
        }

        // Handle Enter key to move down
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            vm.CurveDataTableViewModel.MoveSelection(1, 0);
            ScrollToSelection(dataGrid);
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
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            // Shift+Arrow: Extend selection
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
                    vm.CurveDataTableViewModel.MoveSelection(-1, 0);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
                case Key.Down:
                    vm.CurveDataTableViewModel.MoveSelection(1, 0);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
                case Key.Left:
                    vm.CurveDataTableViewModel.MoveSelection(0, -1);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
                case Key.Right:
                    vm.CurveDataTableViewModel.MoveSelection(0, 1);
                    ScrollToSelection(dataGrid);
                    e.Handled = true;
                    break;
            }
        }
    }

    private void ScrollToSelection(DataGrid dataGrid)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        
        var selectedCells = vm.CurveDataTableViewModel.SelectedCells;
        if (selectedCells.Count == 0) return;

        var firstSelected = selectedCells.First();
        if (firstSelected.RowIndex >= 0 && firstSelected.RowIndex < vm.CurveDataTableViewModel.Rows.Count)
        {
            var row = vm.CurveDataTableViewModel.Rows[firstSelected.RowIndex];
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
        if (DataContext is not MainWindowViewModel vm) return;

        var selectedCells = vm.CurveDataTableViewModel.SelectedCells;
        if (selectedCells.Count == 0) return;

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
                    values.Add("");
                    continue;
                }

                var row = vm.CurveDataTableViewModel.Rows.ElementAtOrDefault(rowIndex);
                if (row is null)
                {
                    values.Add("");
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
                        values.Add("");
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
        if (clipboard is null) return;

#pragma warning disable CS0618 // GetTextAsync is obsolete
        var text = await clipboard.GetTextAsync();
#pragma warning restore CS0618
        if (string.IsNullOrEmpty(text)) return;

        if (DataContext is not MainWindowViewModel vm) return;

        var selectedCells = vm.CurveDataTableViewModel.SelectedCells;
        if (selectedCells.Count == 0) return;

        // Find the top-left selected cell as the paste anchor
        var minRow = selectedCells.Min(c => c.RowIndex);
        var minCol = selectedCells.Min(c => c.ColumnIndex);

        var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var values = lines[lineIndex].Split('\t');
            var rowIndex = minRow + lineIndex;
            
            if (rowIndex >= vm.CurveDataTableViewModel.Rows.Count) break;
            
            var row = vm.CurveDataTableViewModel.Rows[rowIndex];
            
            for (var valueIndex = 0; valueIndex < values.Length; valueIndex++)
            {
                var colIndex = minCol + valueIndex;
                
                // Skip % and RPM columns (read-only)
                if (colIndex < 2) continue;
                
                var seriesName = vm.CurveDataTableViewModel.GetSeriesNameForColumn(colIndex);
                if (seriesName is null) continue;
                
                // Check if series is locked
                if (vm.CurveDataTableViewModel.IsSeriesLocked(seriesName)) continue;
                
                if (double.TryParse(values[valueIndex], out var value))
                {
                    row.SetTorque(seriesName, value);
                }
            }
        }

        vm.MarkDirty();
        vm.ChartViewModel.RefreshChart();
    }

    private void CutSelectedCells(DataGrid dataGrid)
    {
        CopySelectedCells(dataGrid);
        ClearSelectedCells(dataGrid);
    }

    private void ClearSelectedCells(DataGrid dataGrid)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var selectedCells = vm.CurveDataTableViewModel.SelectedCells;
        if (selectedCells.Count == 0) return;

        foreach (var cellPos in selectedCells)
        {
            // Skip % and RPM columns (read-only)
            if (cellPos.ColumnIndex < 2) continue;
            
            var seriesName = vm.CurveDataTableViewModel.GetSeriesNameForColumn(cellPos.ColumnIndex);
            if (seriesName is null) continue;
            
            // Check if series is locked
            if (vm.CurveDataTableViewModel.IsSeriesLocked(seriesName)) continue;
            
            if (cellPos.RowIndex >= 0 && cellPos.RowIndex < vm.CurveDataTableViewModel.Rows.Count)
            {
                vm.CurveDataTableViewModel.Rows[cellPos.RowIndex].SetTorque(seriesName, 0);
            }
        }

        vm.MarkDirty();
        vm.ChartViewModel.RefreshChart();
    }
}
