using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CurveEditor.Views;

/// <summary>
/// Dialog for adding a new drive/voltage configuration.
/// </summary>
public partial class AddDriveVoltageDialog : Window
{
    /// <summary>
    /// Gets or sets the result of the dialog.
    /// </summary>
    public DriveVoltageDialogResult? Result { get; private set; }

    public AddDriveVoltageDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes the dialog with default values from a motor.
    /// </summary>
    public void Initialize(double maxSpeed, double peakTorque, double continuousTorque, double power)
    {
        MaxSpeedInput.Text = maxSpeed.ToString("F0");
        PeakTorqueInput.Text = peakTorque.ToString("F2");
        ContinuousTorqueInput.Text = continuousTorque.ToString("F2");
        PowerInput.Text = power.ToString("F0");
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Result = null;
        Close();
    }

    private void OnAddClick(object? sender, RoutedEventArgs e)
    {
        // Validate all required numeric inputs
        if (!TryParsePositive(VoltageInput.Text, out var voltage, "Value must be a positive number.") ||
            !TryParseNonNegative(PowerInput.Text, out var power, "Power must be a non-negative number.") ||
            !TryParseNonNegative(MaxSpeedInput.Text, out var maxSpeed, "Max Speed must be a non-negative number.") ||
            !TryParseNonNegative(PeakTorqueInput.Text, out var peakTorque, "Peak Torque must be a non-negative number.") ||
            !TryParseNonNegative(ContinuousTorqueInput.Text, out var continuousTorque, "Continuous Torque must be a non-negative number.") ||
            !TryParseNonNegative(ContinuousCurrentInput.Text, out var continuousCurrent, "Continuous Current must be a non-negative number.") ||
            !TryParseNonNegative(PeakCurrentInput.Text, out var peakCurrent, "Peak Current must be a non-negative number."))
        {
            return;
        }

        Result = new DriveVoltageDialogResult
        {
            Name = NameInput.Text ?? "New Drive",
            Manufacturer = ManufacturerInput.Text ?? string.Empty,
            PartNumber = ModelInput.Text ?? string.Empty,
            Voltage = voltage,
            Power = power,
            MaxSpeed = maxSpeed,
            PeakTorque = peakTorque,
            ContinuousTorque = continuousTorque,
            ContinuousCurrent = continuousCurrent,
            PeakCurrent = peakCurrent
        };

        Close();
    }

    /// <summary>
    /// Attempts to parse a string as a positive double.
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
    /// Attempts to parse a string as a non-negative double.
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
/// Result data from the AddDriveVoltageDialog.
/// </summary>
public class DriveVoltageDialogResult
{
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public decimal Voltage { get; set; }
    public decimal Power { get; set; }
    public decimal MaxSpeed { get; set; }
    public decimal PeakTorque { get; set; }
    public decimal ContinuousTorque { get; set; }
    public decimal ContinuousCurrent { get; set; }
    public decimal PeakCurrent { get; set; }
}
