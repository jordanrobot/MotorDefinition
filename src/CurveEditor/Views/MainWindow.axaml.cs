using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CurveEditor.ViewModels;

namespace CurveEditor.Views;

public partial class MainWindow : Window
{
    private const double MaxSpeedChangeTolerance = 0.1;
    private double _previousMaxSpeed;

    public MainWindow()
    {
        InitializeComponent();
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