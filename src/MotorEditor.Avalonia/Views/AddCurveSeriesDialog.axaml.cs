using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace CurveEditor.Views;

/// <summary>
/// Dialog for adding a new curve series.
/// </summary>
public partial class AddCurveDialog : Window
{
    /// <summary>
    /// Conversion factor from horsepower to watts.
    /// </summary>
    private const decimal HorsepowerToWatts = 745.7m;

    /// <summary>
    /// Conversion factor from kilowatts to watts.
    /// </summary>
    private const decimal KilowattsToWatts = 1000.0m;

    /// <summary>
    /// Gets the result of the dialog.
    /// </summary>
    public AddCurveResult? Result { get; private set; }

    private decimal _maxSpeed;

    public AddCurveDialog()
    {
        InitializeComponent();
        ColorInput.TextChanged += OnColorInputChanged;
    }

    /// <summary>
    /// Initializes the dialog with default values.
    /// </summary>
    public void Initialize(decimal maxSpeed, decimal defaultTorque = 40, decimal defaultPower = 1500)
    {
        _maxSpeed = maxSpeed;
        TorqueInput.Text = defaultTorque.ToString("F2");
        PowerInput.Text = defaultPower.ToString("F0");
    }

    private void OnColorInputChanged(object? sender, TextChangedEventArgs e)
    {
        // Update color preview
        try
        {
            if (Color.TryParse(ColorInput.Text, out var color))
            {
                ColorPreview.Background = new SolidColorBrush(color);
            }
        }
        catch
        {
            // Ignore invalid color strings
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Result = null;
        Close();
    }

    private void OnAddClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Validate name
            var name = NameInput?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "New Curves";
            }

            // Validate color
            var colorText = ColorInput?.Text?.Trim() ?? "#FF5050";
            if (!Color.TryParse(colorText, out var color))
            {
                color = Colors.Red;
                colorText = "#FF0000";
            }

            // Determine torque calculation mode
            decimal baseTorque;
            bool usePowerCalculation = PowerBasedRadio?.IsChecked == true;
            decimal power = 0;
            string powerUnit = "W";

            if (usePowerCalculation)
            {
                if (!decimal.TryParse(PowerInput?.Text, out power) || power < 0)
                {
                    power = 1500m;
                }

                var selectedItem = PowerUnitCombo?.SelectedItem as ComboBoxItem;
                powerUnit = selectedItem?.Content?.ToString() ?? "W";

                // Convert to watts if needed
                var powerWatts = powerUnit switch
                {
                    "kW" => power * KilowattsToWatts,
                    "HP" => power * HorsepowerToWatts,
                    _ => power
                };

                // Calculate torque from power at rated speed (assume 50% speed for average)
                // P = T * ω, where ω = 2π * RPM / 60
                // T = P / ω = P * 60 / (2π * RPM)
                var avgSpeed = _maxSpeed * 0.5m;
                if (avgSpeed > 0)
                {
                    baseTorque = powerWatts * 60m / (2m * (decimal)Math.PI * avgSpeed);
                }
                else
                {
                    baseTorque = 0;
                }
            }
            else
            {
                if (!decimal.TryParse(TorqueInput?.Text, out baseTorque) || baseTorque < 0)
                {
                    baseTorque = 40;
                }
            }

            Result = new AddCurveResult
            {
                Name = name,
                Color = colorText,
                BaseTorque = baseTorque,
                UsePowerCalculation = usePowerCalculation,
                Power = power,
                PowerUnit = powerUnit,
                IsVisible = VisibleCheckBox?.IsChecked == true,
                IsLocked = LockedCheckBox?.IsChecked == true
            };
        }
        catch (Exception ex)
        {
            // If any error occurs during result creation, set a default result
            Result = new AddCurveResult
            {
                Name = "New Curves",
                Color = "#FF5050",
                BaseTorque = 40,
                UsePowerCalculation = false,
                Power = 0,
                PowerUnit = "W",
                IsVisible = true,
                IsLocked = false
            };
            System.Diagnostics.Debug.WriteLine($"Error in OnAddClick: {ex.Message}");
        }

        Close();
    }
}

/// <summary>
/// Result data from the AddCurveDialog.
/// </summary>
public class AddCurveResult
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#FF5050";
    public decimal BaseTorque { get; set; }
    public bool UsePowerCalculation { get; set; }
    public decimal Power { get; set; }
    public string PowerUnit { get; set; } = "W";
    public bool IsVisible { get; set; } = true;
    public bool IsLocked { get; set; }
}
