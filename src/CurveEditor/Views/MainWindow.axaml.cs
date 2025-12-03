using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CurveEditor.Models;
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
    /// Handles the max speed field losing focus to show confirmation dialog.
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
        }
    }

    /// <summary>
    /// Handles the visibility checkbox click event to update chart visibility.
    /// </summary>
    private void OnSeriesVisibilityCheckboxClick(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.DataContext is CurveSeries series)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                // The checkbox binding has already updated IsVisible, so just sync with chart
                viewModel.ChartViewModel.SetSeriesVisibility(series.Name, series.IsVisible);
                viewModel.MarkDirty();
            }
        }
    }
}