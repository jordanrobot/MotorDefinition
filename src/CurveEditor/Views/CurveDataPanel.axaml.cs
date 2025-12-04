using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using CurveEditor.Models;
using CurveEditor.ViewModels;

namespace CurveEditor.Views;

/// <summary>
/// Panel for displaying and editing curve series data points.
/// </summary>
public partial class CurveDataPanel : UserControl
{
    /// <summary>
    /// Creates a new CurveDataPanel instance.
    /// </summary>
    public CurveDataPanel()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += OnUnloaded;
    }

    private MainWindowViewModel? _subscribedViewModel;

    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Unsubscribe from events to prevent memory leaks
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.CurveDataTableViewModel.PropertyChanged -= OnCurveDataTablePropertyChanged;
            _subscribedViewModel.AvailableSeries.CollectionChanged -= OnAvailableSeriesCollectionChanged;
            _subscribedViewModel = null;
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // Unsubscribe from old view model
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.CurveDataTableViewModel.PropertyChanged -= OnCurveDataTablePropertyChanged;
            _subscribedViewModel.AvailableSeries.CollectionChanged -= OnAvailableSeriesCollectionChanged;
        }

        if (DataContext is MainWindowViewModel vm)
        {
            // Subscribe to series changes to rebuild columns
            vm.CurveDataTableViewModel.PropertyChanged += OnCurveDataTablePropertyChanged;
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

    private void OnAvailableSeriesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Rebuild columns when series are added or removed
        RebuildDataGridColumns();
    }

    private void OnCurveDataTablePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
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

        // Remove all columns except the first two (% and RPM)
        while (DataTable.Columns.Count > 2)
        {
            DataTable.Columns.RemoveAt(DataTable.Columns.Count - 1);
        }

        // Add a column for each series
        foreach (var series in vm.AvailableSeries)
        {
            var column = new DataGridTemplateColumn
            {
                Header = series.Name,
                Width = new DataGridLength(80),
                IsReadOnly = series.Locked
            };

            // Create cell template
            var cellTemplate = new FuncDataTemplate<CurveDataRow>((row, _) =>
            {
                var textBlock = new TextBlock
                {
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(4, 2)
                };
                textBlock.Bind(TextBlock.TextProperty, 
                    new Avalonia.Data.Binding($"[{series.Name}]") 
                    { 
                        Mode = Avalonia.Data.BindingMode.OneWay,
                        StringFormat = "N2"
                    });
                return textBlock;
            });
            column.CellTemplate = cellTemplate;

            // Create editing template
            if (!series.Locked)
            {
                var editingTemplate = new FuncDataTemplate<CurveDataRow>((row, _) =>
                {
                    var textBox = new TextBox
                    {
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Margin = new Avalonia.Thickness(2)
                    };
                    textBox.Bind(TextBox.TextProperty,
                        new Avalonia.Data.Binding($"[{series.Name}]")
                        {
                            Mode = Avalonia.Data.BindingMode.TwoWay,
                            StringFormat = "N2"
                        });
                    return textBox;
                });
                column.CellEditingTemplate = editingTemplate;
            }

            DataTable.Columns.Add(column);
        }
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
                Margin = new Avalonia.Thickness(16),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Margin = new Avalonia.Thickness(8, 0, 0, 0)
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
                Margin = new Avalonia.Thickness(16, 0, 16, 16)
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
            MoveSelectionDown(dataGrid);
            e.Handled = true;
        }

        // Handle F2 to enter edit mode
        if (e.Key == Key.F2)
        {
            dataGrid.BeginEdit();
            e.Handled = true;
        }

        // Handle up/down arrows for navigation
        if (e.Key == Key.Up && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            MoveSelectionUp(dataGrid);
            e.Handled = true;
        }

        if (e.Key == Key.Down && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            MoveSelectionDown(dataGrid);
            e.Handled = true;
        }

        // Handle Shift+Up/Down to extend selection
        if (e.Key == Key.Up && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            ExtendSelectionUp(dataGrid);
            e.Handled = true;
        }

        if (e.Key == Key.Down && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            ExtendSelectionDown(dataGrid);
            e.Handled = true;
        }
    }

    private void CopySelectedCells(DataGrid dataGrid)
    {
        var selectedItems = dataGrid.SelectedItems;
        if (selectedItems.Count == 0) return;

        var rows = selectedItems.OfType<CurveDataRow>().OrderBy(r => r.RowIndex).ToList();
        if (rows.Count == 0) return;

        // For now, copy torque values from first series
        if (DataContext is MainWindowViewModel vm && vm.AvailableSeries.Count > 0)
        {
            var seriesName = vm.AvailableSeries[0].Name;
            var values = rows.Select(r => r.GetTorque(seriesName).ToString("F2"));
            var clipboardText = string.Join(Environment.NewLine, values);
            TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(clipboardText);
        }
    }

    private async void PasteToSelectedCells(DataGrid dataGrid)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null) return;

#pragma warning disable CS0618 // GetTextAsync is obsolete
        var text = await clipboard.GetTextAsync();
#pragma warning restore CS0618
        if (string.IsNullOrEmpty(text)) return;

        var selectedItems = dataGrid.SelectedItems;
        if (selectedItems.Count == 0) return;

        var rows = selectedItems.OfType<CurveDataRow>().OrderBy(r => r.RowIndex).ToList();
        if (rows.Count == 0) return;

        var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        if (DataContext is MainWindowViewModel vm && vm.AvailableSeries.Count > 0)
        {
            var series = vm.AvailableSeries[0];
            if (series.Locked) return;

            for (var i = 0; i < Math.Min(rows.Count, lines.Length); i++)
            {
                if (double.TryParse(lines[i], out var value))
                {
                    rows[i].SetTorque(series.Name, value);
                }
            }

            vm.MarkDirty();
            vm.ChartViewModel.RefreshChart();
        }
    }

    private void CutSelectedCells(DataGrid dataGrid)
    {
        CopySelectedCells(dataGrid);
        ClearSelectedCells(dataGrid);
    }

    private void ClearSelectedCells(DataGrid dataGrid)
    {
        var selectedItems = dataGrid.SelectedItems;
        if (selectedItems.Count == 0) return;

        var rows = selectedItems.OfType<CurveDataRow>().ToList();
        if (rows.Count == 0) return;

        if (DataContext is MainWindowViewModel vm && vm.AvailableSeries.Count > 0)
        {
            var series = vm.AvailableSeries[0];
            if (series.Locked) return;

            foreach (var row in rows)
            {
                row.SetTorque(series.Name, 0);
            }

            vm.MarkDirty();
            vm.ChartViewModel.RefreshChart();
        }
    }

    private void MoveSelectionDown(DataGrid dataGrid)
    {
        var currentIndex = dataGrid.SelectedIndex;
        if (dataGrid.ItemsSource is System.Collections.ICollection collection)
        {
            if (currentIndex >= 0 && currentIndex < collection.Count - 1)
            {
                dataGrid.SelectedIndex = currentIndex + 1;
                dataGrid.ScrollIntoView(dataGrid.SelectedItem, null);
            }
        }
    }

    private void MoveSelectionUp(DataGrid dataGrid)
    {
        var currentIndex = dataGrid.SelectedIndex;
        if (currentIndex > 0)
        {
            dataGrid.SelectedIndex = currentIndex - 1;
            dataGrid.ScrollIntoView(dataGrid.SelectedItem, null);
        }
    }

    private void ExtendSelectionDown(DataGrid dataGrid)
    {
        if (dataGrid.ItemsSource is System.Collections.ICollection collection)
        {
            // Get current selection bounds
            var selectedRows = dataGrid.SelectedItems.OfType<CurveDataRow>().ToList();
            if (selectedRows.Count == 0) return;

            var maxIndex = selectedRows.Max(r => r.RowIndex);
            if (maxIndex < collection.Count - 1)
            {
                // Find the item at the next index
                var allRows = dataGrid.ItemsSource.Cast<CurveDataRow>().ToList();
                if (maxIndex + 1 < allRows.Count)
                {
                    var nextRow = allRows[maxIndex + 1];
                    if (!dataGrid.SelectedItems.Contains(nextRow))
                    {
                        dataGrid.SelectedItems.Add(nextRow);
                    }
                    dataGrid.ScrollIntoView(nextRow, null);
                }
            }
        }
    }

    private void ExtendSelectionUp(DataGrid dataGrid)
    {
        // Get current selection bounds
        var selectedRows = dataGrid.SelectedItems.OfType<CurveDataRow>().ToList();
        if (selectedRows.Count == 0) return;

        var minIndex = selectedRows.Min(r => r.RowIndex);
        if (minIndex > 0)
        {
            // Find the item at the previous index
            var allRows = dataGrid.ItemsSource.Cast<CurveDataRow>().ToList();
            if (minIndex - 1 >= 0)
            {
                var prevRow = allRows[minIndex - 1];
                if (!dataGrid.SelectedItems.Contains(prevRow))
                {
                    dataGrid.SelectedItems.Add(prevRow);
                }
                dataGrid.ScrollIntoView(prevRow, null);
            }
        }
    }
}
