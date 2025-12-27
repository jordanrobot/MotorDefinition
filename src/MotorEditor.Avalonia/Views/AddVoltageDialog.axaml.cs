using Avalonia.Controls;
using Avalonia.Interactivity;
using JordanRobot.MotorDefinition.Model;
using System.Collections.Generic;
using System.Linq;

namespace CurveEditor.Views;

/// <summary>
/// Dialog for adding a new voltage configuration to an existing drive.
/// </summary>
public partial class AddVoltageDialog : Window
{
    /// <summary>
    /// Gets or sets the result of the dialog.
    /// </summary>
    public AddVoltageDialogResult? Result { get; private set; }

    public AddVoltageDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes the dialog with available drives and default values.
    /// </summary>
    /// <param name="availableDrives">List of available drives to populate the dropdown.</param>
    /// <param name="selectedDrive">The currently selected drive (will be pre-selected).</param>
    /// <param name="maxSpeed">Default max speed value.</param>
    /// <param name="peakTorque">Default peak torque value.</param>
    /// <param name="continuousTorque">Default continuous torque value.</param>
    /// <param name="power">Default power value.</param>
    public void Initialize(
        IEnumerable<Drive> availableDrives,
        Drive? selectedDrive,
        double maxSpeed,
        double peakTorque,
        double continuousTorque,
        double power)
    {
        DriveComboBox.ItemsSource = availableDrives.ToList();
        DriveComboBox.SelectedItem = selectedDrive ?? availableDrives.FirstOrDefault();
        
        MaxSpeedInput.Text = maxSpeed.ToString("F0");
        PeakTorqueInput.Text = peakTorque.ToString("F2");
        ContinuousTorqueInput.Text = continuousTorque.ToString("F2");
        PowerInput.Text = power.ToString("F0");
        
        UpdateContinuousTorqueFieldsEnabled();
        UpdatePeakTorqueFieldsEnabled();
    }

    private void OnContinuousTorqueChecked(object? sender, RoutedEventArgs e)
    {
        UpdateContinuousTorqueFieldsEnabled();
    }

    private void OnPeakTorqueChecked(object? sender, RoutedEventArgs e)
    {
        UpdatePeakTorqueFieldsEnabled();
    }

    private void UpdateContinuousTorqueFieldsEnabled()
    {
        var isEnabled = AddContinuousTorqueCheckBox.IsChecked == true;
        ContinuousTorquePanel.IsEnabled = isEnabled;
    }

    private void UpdatePeakTorqueFieldsEnabled()
    {
        var isEnabled = AddPeakTorqueCheckBox.IsChecked == true;
        PeakTorquePanel.IsEnabled = isEnabled;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Result = null;
        Close();
    }

    private void OnAddClick(object? sender, RoutedEventArgs e)
    {
        // Validate drive selection
        if (DriveComboBox.SelectedItem is not Drive selectedDrive)
        {
            // In a production app, we would show an error message to the user
            return;
        }

        // Validate all required numeric inputs
        if (!TryParsePositive(VoltageInput.Text, out var voltage, "Voltage must be a positive number.") ||
            !TryParseNonNegative(PowerInput.Text, out var power, "Power must be a non-negative number.") ||
            !TryParseNonNegative(MaxSpeedInput.Text, out var maxSpeed, "Max Speed must be a non-negative number."))
        {
            return;
        }

        // Validate curve-specific fields if curves are enabled
        var addContinuousTorque = AddContinuousTorqueCheckBox.IsChecked == true;
        var addPeakTorque = AddPeakTorqueCheckBox.IsChecked == true;

        double continuousTorque = 0;
        double continuousCurrent = 0;
        if (addContinuousTorque)
        {
            if (!TryParseNonNegative(ContinuousTorqueInput.Text, out continuousTorque, "Continuous Torque must be a non-negative number.") ||
                !TryParseNonNegative(ContinuousCurrentInput.Text, out continuousCurrent, "Continuous Current must be a non-negative number."))
            {
                return;
            }
        }

        double peakTorque = 0;
        double peakCurrent = 0;
        if (addPeakTorque)
        {
            if (!TryParseNonNegative(PeakTorqueInput.Text, out peakTorque, "Peak Torque must be a non-negative number.") ||
                !TryParseNonNegative(PeakCurrentInput.Text, out peakCurrent, "Peak Current must be a non-negative number."))
            {
                return;
            }
        }

        Result = new AddVoltageDialogResult
        {
            TargetDrive = selectedDrive,
            Voltage = voltage,
            Power = power,
            MaxSpeed = maxSpeed,
            AddContinuousTorque = addContinuousTorque,
            ContinuousTorque = continuousTorque,
            ContinuousCurrent = continuousCurrent,
            AddPeakTorque = addPeakTorque,
            PeakTorque = peakTorque,
            PeakCurrent = peakCurrent,
            CalculateCurveFromPowerAndSpeed = CalculateCurveCheckBox.IsChecked == true
        };

        Close();
    }

    /// <summary>
    /// Attempts to parse a string as a positive double.
    /// </summary>
    private static bool TryParsePositive(string? text, out double value, string errorMessage)
    {
        if (!double.TryParse(text, out value) || value <= 0)
        {
            // In a production app, we would show errorMessage to the user
            value = 0;
            return false;
        }
        return true;
    }

    /// <summary>
    /// Attempts to parse a string as a non-negative double.
    /// </summary>
    private static bool TryParseNonNegative(string? text, out double value, string errorMessage)
    {
        if (!double.TryParse(text, out value) || value < 0)
        {
            // In a production app, we would show errorMessage to the user
            value = 0;
            return false;
        }
        return true;
    }
}

/// <summary>
/// Result data from the AddVoltageDialog.
/// </summary>
public class AddVoltageDialogResult
{
    public Drive TargetDrive { get; set; } = null!;
    public double Voltage { get; set; }
    public double Power { get; set; }
    public double MaxSpeed { get; set; }
    public bool AddContinuousTorque { get; set; }
    public double ContinuousTorque { get; set; }
    public double ContinuousCurrent { get; set; }
    public bool AddPeakTorque { get; set; }
    public double PeakTorque { get; set; }
    public double PeakCurrent { get; set; }
    public bool CalculateCurveFromPowerAndSpeed { get; set; }
}
