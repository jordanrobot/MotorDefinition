using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace CurveEditor.Views;

/// <summary>
/// Dialog for adding a new curve series.
/// </summary>
public partial class AddCurveSeriesDialog : Window
{
    /// <summary>
    /// Gets the result of the dialog.
    /// </summary>
    public AddCurveSeriesResult? Result { get; private set; }

    private double _maxSpeed;

    public AddCurveSeriesDialog()
    {
        InitializeComponent();
        ColorInput.TextChanged += OnColorInputChanged;
    }

    /// <summary>
    /// Initializes the dialog with default values.
    /// </summary>
    public void Initialize(double maxSpeed, double defaultTorque = 40, double defaultPower = 1500)
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
        // Validate name
        var name = NameInput.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "New Series";
        }

        // Validate color
        var colorText = ColorInput.Text?.Trim() ?? "#FF5050";
        if (!Color.TryParse(colorText, out var color))
        {
            color = Colors.Red;
            colorText = "#FF0000";
        }

        // Determine torque calculation mode
        double baseTorque;
        bool usePowerCalculation = PowerBasedRadio.IsChecked == true;
        double power = 0;
        string powerUnit = "W";

        if (usePowerCalculation)
        {
            if (!double.TryParse(PowerInput.Text, out power) || power < 0)
            {
                power = 1500;
            }

            var selectedItem = PowerUnitCombo.SelectedItem as ComboBoxItem;
            powerUnit = selectedItem?.Content?.ToString() ?? "W";

            // Convert to watts if needed
            var powerWatts = powerUnit switch
            {
                "kW" => power * 1000,
                "HP" => power * 745.7,
                _ => power
            };

            // Calculate torque from power at rated speed (assume 50% speed for average)
            // P = T * ω, where ω = 2π * RPM / 60
            // T = P / ω = P * 60 / (2π * RPM)
            var avgSpeed = _maxSpeed * 0.5;
            if (avgSpeed > 0)
            {
                baseTorque = powerWatts * 60 / (2 * Math.PI * avgSpeed);
            }
            else
            {
                baseTorque = 0;
            }
        }
        else
        {
            if (!double.TryParse(TorqueInput.Text, out baseTorque) || baseTorque < 0)
            {
                baseTorque = 40;
            }
        }

        Result = new AddCurveSeriesResult
        {
            Name = name,
            Color = colorText,
            BaseTorque = baseTorque,
            UsePowerCalculation = usePowerCalculation,
            Power = power,
            PowerUnit = powerUnit,
            IsVisible = VisibleCheckBox.IsChecked == true,
            IsLocked = LockedCheckBox.IsChecked == true
        };

        Close();
    }
}

/// <summary>
/// Result data from the AddCurveSeriesDialog.
/// </summary>
public class AddCurveSeriesResult
{
    public string Name { get; set; } = "";
    public string Color { get; set; } = "#FF5050";
    public double BaseTorque { get; set; }
    public bool UsePowerCalculation { get; set; }
    public double Power { get; set; }
    public string PowerUnit { get; set; } = "W";
    public bool IsVisible { get; set; } = true;
    public bool IsLocked { get; set; }
}
