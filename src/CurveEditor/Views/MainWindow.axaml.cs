using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CurveEditor.Behaviors;
using CurveEditor.ViewModels;

namespace CurveEditor.Views;

public partial class MainWindow : Window
{
    private const double MaxSpeedChangeTolerance = 0.1;
    private double _previousMaxSpeed;
    private double _browserColumnWidth;
    private double _propertiesColumnWidth;
    private double _curveDataRowHeight;
    private bool _browserPanelExpanded;
    private bool _propertiesPanelExpanded;
    private bool _curveDataPanelExpanded;

    public MainWindow()
    {
        InitializeComponent();
        WindowBoundsPersistence.Attach(this, "MainWindow");
        Opened += OnOpened;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Opened -= OnOpened;

        var mainGrid = this.FindControl<Grid>("MainLayoutGrid");
        var centerGrid = this.FindControl<Grid>("CenterGrid");

        if (mainGrid is null || centerGrid is null || DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        // Left browser panel (column 0)
        PanelLayoutPersistence.AttachColumn(this, mainGrid, 0, "MainWindow.BrowserPanel");

        // Right properties panel (column 4)
        PanelLayoutPersistence.AttachColumn(this, mainGrid, 4, "MainWindow.PropertiesPanel");

        // Curve data panel row (row 2 inside center grid)
        PanelLayoutPersistence.AttachRow(
            this,
            centerGrid,
            2,
            "MainWindow.CurveDataPanel",
            () => viewModel.IsCurveDataExpanded);

        // Browser panel expanded/collapsed state
        PanelLayoutPersistence.AttachBoolean(
            this,
            () => viewModel.IsBrowserPanelExpanded,
            value => viewModel.IsBrowserPanelExpanded = value,
            "MainWindow.BrowserPanel");

        // Properties panel expanded/collapsed state
        PanelLayoutPersistence.AttachBoolean(
            this,
            () => viewModel.IsPropertiesPanelExpanded,
            value => viewModel.IsPropertiesPanelExpanded = value,
            "MainWindow.PropertiesPanel");

        // Curve data panel expanded/collapsed state
        PanelLayoutPersistence.AttachBoolean(
            this,
            () => viewModel.IsCurveDataExpanded,
            value => viewModel.IsCurveDataExpanded = value,
            "MainWindow.CurveDataPanel");

        // Capture initial sizes (after any persisted values are restored)
        _browserColumnWidth = mainGrid.ColumnDefinitions[0].Width.Value;
        _propertiesColumnWidth = mainGrid.ColumnDefinitions[4].Width.Value;
        _curveDataRowHeight = centerGrid.RowDefinitions[2].Height.Value;

        _browserPanelExpanded = viewModel.IsBrowserPanelExpanded;
        _propertiesPanelExpanded = viewModel.IsPropertiesPanelExpanded;
        _curveDataPanelExpanded = viewModel.IsCurveDataExpanded;

        // React to panel expanded/collapsed properties changing.
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.IsBrowserPanelExpanded))
            {
                ApplyBrowserPanelLayout(mainGrid, viewModel, _browserPanelExpanded);
                _browserPanelExpanded = viewModel.IsBrowserPanelExpanded;
            }
            else if (args.PropertyName == nameof(MainWindowViewModel.IsPropertiesPanelExpanded))
            {
                ApplyPropertiesPanelLayout(mainGrid, viewModel, _propertiesPanelExpanded);
                _propertiesPanelExpanded = viewModel.IsPropertiesPanelExpanded;
            }
            else if (args.PropertyName == nameof(MainWindowViewModel.IsCurveDataExpanded))
            {
                ApplyCurveDataPanelLayout(centerGrid, viewModel, _curveDataPanelExpanded);
                _curveDataPanelExpanded = viewModel.IsCurveDataExpanded;
            }
        };

        // Apply initial layouts based on current expanded/collapsed state.
        ApplyBrowserPanelLayout(mainGrid, viewModel, _browserPanelExpanded);
        ApplyPropertiesPanelLayout(mainGrid, viewModel, _propertiesPanelExpanded);
        ApplyCurveDataPanelLayout(centerGrid, viewModel, _curveDataPanelExpanded);
    }

    private void ApplyBrowserPanelLayout(Grid mainGrid, MainWindowViewModel viewModel, bool wasExpanded)
    {
        var browserColumn = mainGrid.ColumnDefinitions[0];
        var splitterColumn = mainGrid.ColumnDefinitions[1];

        var isExpanded = viewModel.IsBrowserPanelExpanded;

        if (isExpanded)
        {
            if (_browserColumnWidth <= 0)
            {
                var current = browserColumn.ActualWidth > 0 ? browserColumn.ActualWidth : browserColumn.Width.Value;
                _browserColumnWidth = current > 0 ? current : 200;
            }

            browserColumn.Width = new GridLength(_browserColumnWidth, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(4, GridUnitType.Pixel);
        }
        else
        {
            if (wasExpanded)
            {
                var current = browserColumn.ActualWidth > 0 ? browserColumn.ActualWidth : browserColumn.Width.Value;
                if (current > 0)
                {
                    _browserColumnWidth = current;
                    PanelLayoutPersistence.UpdateColumnWidth("MainWindow.BrowserPanel", _browserColumnWidth);
                }
            }

            browserColumn.Width = new GridLength(0, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
        }
    }

    private void ApplyPropertiesPanelLayout(Grid mainGrid, MainWindowViewModel viewModel, bool wasExpanded)
    {
        var splitterColumn = mainGrid.ColumnDefinitions[3];
        var propertiesColumn = mainGrid.ColumnDefinitions[4];

        var isExpanded = viewModel.IsPropertiesPanelExpanded;

        if (isExpanded)
        {
            if (_propertiesColumnWidth <= 0)
            {
                var current = propertiesColumn.ActualWidth > 0 ? propertiesColumn.ActualWidth : propertiesColumn.Width.Value;
                _propertiesColumnWidth = current > 0 ? current : 280;
            }

            propertiesColumn.Width = new GridLength(_propertiesColumnWidth, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(4, GridUnitType.Pixel);
        }
        else
        {
            if (wasExpanded)
            {
                var current = propertiesColumn.ActualWidth > 0 ? propertiesColumn.ActualWidth : propertiesColumn.Width.Value;
                if (current > 0)
                {
                    _propertiesColumnWidth = current;
                    PanelLayoutPersistence.UpdateColumnWidth("MainWindow.PropertiesPanel", _propertiesColumnWidth);
                }
            }

            propertiesColumn.Width = new GridLength(0, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
        }
    }

    private void ApplyCurveDataPanelLayout(Grid centerGrid, MainWindowViewModel viewModel, bool wasExpanded)
    {
        var row = centerGrid.RowDefinitions[2];
        var isExpanded = viewModel.IsCurveDataExpanded;

        if (isExpanded)
        {
            if (_curveDataRowHeight <= 0)
            {
                var current = row.ActualHeight > 0 ? row.ActualHeight : row.Height.Value;
                _curveDataRowHeight = current > 0 ? current : 200;
            }

            row.Height = new GridLength(_curveDataRowHeight, GridUnitType.Pixel);
        }
        else
        {
            if (wasExpanded)
            {
                var current = row.ActualHeight > 0 ? row.ActualHeight : row.Height.Value;
                if (current > 0)
                {
                    _curveDataRowHeight = current;
                    PanelLayoutPersistence.UpdateRowHeight("MainWindow.CurveDataPanel", _curveDataRowHeight);
                }
            }

            // Let the row shrink to the header height when collapsed.
            row.Height = GridLength.Auto;
        }
    }

    private void OnMotorNameLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorName(viewModel.MotorNameEditor);
        }
    }

    private void OnManufacturerLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorManufacturer(viewModel.ManufacturerEditor);
        }
    }

    private void OnPartNumberLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorPartNumber(viewModel.PartNumberEditor);
        }
    }

    /// <summary>
    /// Handles the drive max speed field losing focus to commit via command,
    /// optionally show confirmation dialog, and refresh the chart.
    /// </summary>
    private async void OnMaxSpeedLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel && viewModel.SelectedVoltage is not null)
        {
            // First, commit the edit through the undoable command path so it
            // participates consistently in undo/redo just like other fields.
            viewModel.EditSelectedVoltageMaxSpeed();

            var currentMaxSpeed = viewModel.SelectedVoltage.MaxSpeed;
            
            // Only show dialog if max speed actually changed
            if (Math.Abs(currentMaxSpeed - _previousMaxSpeed) > MaxSpeedChangeTolerance && _previousMaxSpeed > 0)
            {
                await viewModel.ConfirmMaxSpeedChangeAsync();
            }
            
            _previousMaxSpeed = currentMaxSpeed;

            // Refresh the chart to update the x-axis
            viewModel.ChartViewModel.RefreshChart();
        }
    }

    /// <summary>
    /// Handles the motor max speed field losing focus to commit via command and refresh chart.
    /// </summary>
    private void OnMotorMaxSpeedLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorMaxSpeed();
            viewModel.ChartViewModel.RefreshChart();
        }
    }

    private void OnHasBrakeChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorHasBrake();
            viewModel.ChartViewModel.RefreshChart();
        }
    }

    private void OnBrakeTorqueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorBrakeTorque();
            viewModel.ChartViewModel.RefreshChart();
        }
    }

    private void OnRatedSpeedLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorRatedSpeed();
        }
    }

    private void OnRatedPeakTorqueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorRatedPeakTorque();
        }
    }

    private void OnRatedContinuousTorqueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorRatedContinuousTorque();
        }
    }

    private void OnPowerLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorPower();
        }
    }

    private void OnWeightLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorWeight();
        }
    }

    private void OnRotorInertiaLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorRotorInertia();
        }
    }

    private void OnFeedbackPprLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorFeedbackPpr();
        }
    }

    private void OnBrakeAmperageLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorBrakeAmperage();
        }
    }

    private void OnBrakeVoltageLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorBrakeVoltage();
        }
    }

    private void OnDriveNameLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditDriveName();
        }
    }

    private void OnDrivePartNumberLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditDrivePartNumber();
        }
    }

    private void OnDriveManufacturerLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditDriveManufacturer();
        }
    }

    private void OnVoltageValueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltageValue();
        }
    }

    private void OnVoltagePowerLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltagePower();
        }
    }

    private void OnVoltagePeakTorqueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltagePeakTorque();
        }
    }

    private void OnVoltageContinuousTorqueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltageContinuousTorque();
        }
    }

    private void OnVoltageContinuousAmpsLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltageContinuousAmps();
        }
    }

    private void OnVoltagePeakAmpsLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltagePeakAmps();
        }
    }
}