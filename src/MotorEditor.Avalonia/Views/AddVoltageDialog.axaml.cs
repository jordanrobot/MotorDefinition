using Avalonia.Controls;
using Avalonia.Interactivity;
using JordanRobot.MotorDefinition.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurveEditor.Views;

/// <summary>
/// Dialog for adding a new voltage configuration to an existing drive.
/// </summary>
public partial class AddVoltageDialog : Window
{
    private static readonly decimal[] PreferredVoltages = [208, 230, 240, 110, 120, 104, 430, 460];

    /// <summary>
    /// Gets or sets the result of the dialog.
    /// </summary>
    public AddVoltageDialogResult? Result { get; private set; }

    private List<Drive> _availableDrives = [];

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
        decimal maxSpeed,
        decimal peakTorque,
        decimal continuousTorque,
        decimal power)
    {
        _availableDrives = availableDrives.ToList();
        DriveComboBox.ItemsSource = _availableDrives;
        DriveComboBox.SelectedItem = selectedDrive ?? _availableDrives.FirstOrDefault();

        MaxSpeedInput.Text = maxSpeed.ToString("F0");
        PeakTorqueInput.Text = peakTorque.ToString("F2");
        ContinuousTorqueInput.Text = continuousTorque.ToString("F2");
        PowerInput.Text = power.ToString("F0");

        // Set smart default voltage based on what already exists in the selected drive
        SetSmartDefaultVoltage();

        UpdateContinuousTorqueFieldsEnabled();
        UpdatePeakTorqueFieldsEnabled();
        ValidateVoltage();
    }

    private void SetSmartDefaultVoltage()
    {
        if (DriveComboBox.SelectedItem is not Drive selectedDrive)
        {
            VoltageInput.Text = "208";
            return;
        }

        var existingVoltages = selectedDrive.Voltages.Select(v => v.Value).ToHashSet();

        // Find the first preferred voltage that doesn't exist in the drive
        foreach (var voltage in PreferredVoltages)
        {
            if (!existingVoltages.Any(v => Math.Abs(v - voltage) < Drive.DefaultVoltageTolerance))
            {
                VoltageInput.Text = voltage.ToString("F0");
                return;
            }
        }

        // If all preferred voltages exist, default to 208
        VoltageInput.Text = "208";
    }

    private void OnDriveSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SetSmartDefaultVoltage();
        ValidateVoltage();
    }

    private void OnVoltageTextChanged(object? sender, TextChangedEventArgs e)
    {
        ValidateVoltage();
    }

    private void ValidateVoltage()
    {
        if (DriveComboBox.SelectedItem is not Drive selectedDrive)
        {
            return;
        }

        if (!decimal.TryParse(VoltageInput.Text, out var voltage) || voltage <= 0)
        {
            // Invalid voltage value - don't show duplicate error
            ValidationErrorText.IsVisible = false;
            EnableInputControls(true);
            return;
        }

        // Check if this voltage already exists in the selected drive
        var isDuplicate = selectedDrive.Voltages.Any(v => Math.Abs(v.Value - voltage) < Drive.DefaultVoltageTolerance);

        if (isDuplicate)
        {
            ValidationErrorText.Text = $"A voltage of {voltage}V already exists for drive '{selectedDrive.Name}'. Please enter a different voltage or select a different drive.";
            ValidationErrorText.IsVisible = true;
            EnableInputControls(false);
        }
        else
        {
            ValidationErrorText.IsVisible = false;
            EnableInputControls(true);
        }
    }

    private void EnableInputControls(bool enable)
    {
        // Enable/disable all controls except Cancel button and Drive combo box
        ContentPanel.IsEnabled = enable;
        AddButton.IsEnabled = enable;
    }

    private void OnContinuousTorqueChecked(object? sender, RoutedEventArgs e)
    {
        UpdateContinuousTorqueFieldsEnabled();
    }

    private void OnPeakTorqueChecked(object? sender, RoutedEventArgs e)
    {
        UpdatePeakTorqueFieldsEnabled();
    }

    private void OnCalculateCurveChecked(object? sender, RoutedEventArgs e)
    {
        // When calculate curve is checked, disable the manual torque/amperage checkboxes
        var isCalculateEnabled = CalculateCurveCheckBox.IsChecked == true;

        if (isCalculateEnabled)
        {
            // Disable manual curve entry
            AddContinuousTorqueCheckBox.IsEnabled = false;
            AddPeakTorqueCheckBox.IsEnabled = false;
            ContinuousTorquePanel.IsEnabled = false;
            PeakTorquePanel.IsEnabled = false;

            // Show and populate the curve name field
            CalculatedCurveNamePanel.IsVisible = true;
            SetSmartDefaultCurveName();
        }
        else
        {
            // Re-enable manual curve entry
            AddContinuousTorqueCheckBox.IsEnabled = true;
            AddPeakTorqueCheckBox.IsEnabled = true;
            UpdateContinuousTorqueFieldsEnabled();
            UpdatePeakTorqueFieldsEnabled();

            // Hide the curve name field
            CalculatedCurveNamePanel.IsVisible = false;
        }
    }

    private void SetSmartDefaultCurveName()
    {
        if (DriveComboBox.SelectedItem is not Drive selectedDrive)
        {
            CalculatedCurveNameInput.Text = "Curve 1";
            return;
        }

        // Get all voltage configurations for this drive to check all existing curve names
        var existingCurveNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var voltage in selectedDrive.Voltages)
        {
            foreach (var curve in voltage.Curves)
            {
                existingCurveNames.Add(curve.Name);
            }
        }

        // Find the first "Curve X" name that doesn't exist
        var counter = 1;
        string curveName;
        do
        {
            curveName = $"Curve {counter}";
            counter++;
        } while (existingCurveNames.Contains(curveName));

        CalculatedCurveNameInput.Text = curveName;
    }

    private void UpdateContinuousTorqueFieldsEnabled()
    {
        var isEnabled = AddContinuousTorqueCheckBox.IsChecked == true && CalculateCurveCheckBox.IsChecked == false;
        ContinuousTorquePanel.IsEnabled = isEnabled;
    }

    private void UpdatePeakTorqueFieldsEnabled()
    {
        var isEnabled = AddPeakTorqueCheckBox.IsChecked == true && CalculateCurveCheckBox.IsChecked == false;
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

        // Check for duplicate voltage
        if (selectedDrive.Voltages.Any(v => Math.Abs(v.Value - voltage) < Drive.DefaultVoltageTolerance))
        {
            return;
        }

        var calculateCurve = CalculateCurveCheckBox.IsChecked == true;

        // Validate curve-specific fields if curves are enabled
        var addContinuousTorque = AddContinuousTorqueCheckBox.IsChecked == true;
        var addPeakTorque = AddPeakTorqueCheckBox.IsChecked == true;

        decimal continuousTorque = 0;
        decimal continuousCurrent = 0;
        decimal peakTorque = 0;
        decimal peakCurrent = 0;

        if (calculateCurve)
        {
            // Calculate torque from power and speed
            // Power (W) = Torque (Nm) * Speed (rad/s)
            // Speed (rad/s) = RPM * 2π / 60
            // Therefore: Torque (Nm) = Power (W) * 60 / (RPM * 2π)

            if (maxSpeed > 0)
            {
                var speedRadPerSec = maxSpeed * 2.0m * (decimal)Math.PI / 60.0m;
                var calculatedTorque = power / speedRadPerSec;

                // Round to 2 decimal places
                calculatedTorque = Math.Round(calculatedTorque, 2);

                // For calculated curves, we create only a single continuous curve
                continuousTorque = calculatedTorque;
                addContinuousTorque = true;
                addPeakTorque = false;

                // Amperage values are left at 0 since we don't have enough info to calculate them
                continuousCurrent = 0;
                peakCurrent = 0;
            }
        }
        else
        {
            // Manual entry mode
            if (addContinuousTorque)
            {
                if (!TryParseNonNegative(ContinuousTorqueInput.Text, out continuousTorque, "Continuous Torque must be a non-negative number.") ||
                    !TryParseNonNegative(ContinuousCurrentInput.Text, out continuousCurrent, "Continuous Current must be a non-negative number."))
                {
                    return;
                }
            }

            if (addPeakTorque)
            {
                if (!TryParseNonNegative(PeakTorqueInput.Text, out peakTorque, "Peak Torque must be a non-negative number.") ||
                    !TryParseNonNegative(PeakCurrentInput.Text, out peakCurrent, "Peak Current must be a non-negative number."))
                {
                    return;
                }
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
            CalculateCurveFromPowerAndSpeed = calculateCurve,
            CalculatedCurveName = calculateCurve ? CalculatedCurveNameInput.Text?.Trim() : null
        };

        Close();
    }

    /// <summary>
    /// Attempts to parse a string as a positive decimal.
    /// </summary>
    private static bool TryParsePositive(string? text, out decimal value, string errorMessage)
    {
        if (!decimal.TryParse(text, out value) || value <= 0)
        {
            // In a production app, we would show errorMessage to the user
            value = 0;
            return false;
        }
        return true;
    }

    /// <summary>
    /// Attempts to parse a string as a non-negative decimal.
    /// </summary>
    private static bool TryParseNonNegative(string? text, out decimal value, string errorMessage)
    {
        if (!decimal.TryParse(text, out value) || value < 0)
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
    public decimal Voltage { get; set; }
    public decimal Power { get; set; }
    public decimal MaxSpeed { get; set; }
    public bool AddContinuousTorque { get; set; }
    public decimal ContinuousTorque { get; set; }
    public decimal ContinuousCurrent { get; set; }
    public bool AddPeakTorque { get; set; }
    public decimal PeakTorque { get; set; }
    public decimal PeakCurrent { get; set; }
    public bool CalculateCurveFromPowerAndSpeed { get; set; }
    public string? CalculatedCurveName { get; set; }
}
