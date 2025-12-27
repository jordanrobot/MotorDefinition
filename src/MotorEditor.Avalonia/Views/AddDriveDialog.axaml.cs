using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CurveEditor.Views;

/// <summary>
/// Dialog for adding a new drive (without voltage configuration).
/// </summary>
public partial class AddDriveDialog : Window
{
    /// <summary>
    /// Gets or sets the result of the dialog.
    /// </summary>
    public AddDriveDialogResult? Result { get; private set; }

    public AddDriveDialog()
    {
        InitializeComponent();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Result = null;
        Close();
    }

    private void OnAddClick(object? sender, RoutedEventArgs e)
    {
        // Validate that at least a name is provided
        if (string.IsNullOrWhiteSpace(NameInput.Text))
        {
            // In a production app, we would show an error message to the user
            return;
        }

        Result = new AddDriveDialogResult
        {
            Name = NameInput.Text?.Trim() ?? "New Drive",
            Manufacturer = ManufacturerInput.Text?.Trim() ?? string.Empty,
            PartNumber = PartNumberInput.Text?.Trim() ?? string.Empty
        };

        Close();
    }
}

/// <summary>
/// Result data from the AddDriveDialog.
/// </summary>
public class AddDriveDialogResult
{
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
}
