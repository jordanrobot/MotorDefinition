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

    /// <summary>
    /// Handles the drive max speed field losing focus to show confirmation dialog and refresh chart.
    /// </summary>
    private async void OnMaxSpeedLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel && viewModel.SelectedVoltage is not null)
        {
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
    /// Handles the motor max speed field losing focus to refresh chart.
    /// </summary>
    private void OnMotorMaxSpeedLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel && viewModel.CurrentMotor is not null)
        {
            // Update the motor max speed in the chart view model (triggers OnMotorMaxSpeedChanged which updates axes)
            viewModel.ChartViewModel.MotorMaxSpeed = viewModel.CurrentMotor.MaxSpeed;
            viewModel.MarkDirty();
        }
    }

    /// <summary>
    /// Handles when the HasBrake checkbox changes to update the brake torque line on the chart.
    /// </summary>
    private void OnHasBrakeChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel && viewModel.CurrentMotor is not null)
        {
            viewModel.ChartViewModel.HasBrake = viewModel.CurrentMotor.HasBrake;
            viewModel.ChartViewModel.BrakeTorque = viewModel.CurrentMotor.BrakeTorque;
            viewModel.ChartViewModel.RefreshChart();
            viewModel.MarkDirty();
        }
    }

    /// <summary>
    /// Handles when the BrakeTorque field loses focus to update the brake torque line on the chart.
    /// </summary>
    private void OnBrakeTorqueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel && viewModel.CurrentMotor is not null)
        {
            viewModel.ChartViewModel.BrakeTorque = viewModel.CurrentMotor.BrakeTorque;
            viewModel.ChartViewModel.RefreshChart();
            viewModel.MarkDirty();
        }
    }
}