using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CurveEditor.Views;

/// <summary>
/// Simple message dialog with OK/Cancel buttons.
/// </summary>
public partial class MessageDialog : Window
{
    /// <summary>
    /// Gets the result of the dialog: true = OK, false = Cancel, null = closed without choosing.
    /// </summary>
    public bool? Result { get; private set; }

    /// <summary>
    /// Gets or sets the message to display.
    /// </summary>
    public string Message
    {
        get => MessageText.Text ?? string.Empty;
        set => MessageText.Text = value;
    }

    /// <summary>
    /// Gets or sets the OK button text.
    /// </summary>
    public string OkButtonText
    {
        get => OkButton.Content?.ToString() ?? "OK";
        set => OkButton.Content = value;
    }

    /// <summary>
    /// Gets or sets the Cancel button text.
    /// </summary>
    public string CancelButtonText
    {
        get => CancelButton.Content?.ToString() ?? "Cancel";
        set => CancelButton.Content = value;
    }

    /// <summary>
    /// Gets or sets whether to show the Cancel button.
    /// </summary>
    public bool ShowCancelButton
    {
        get => CancelButton.IsVisible;
        set => CancelButton.IsVisible = value;
    }

    /// <summary>
    /// Gets whether the user clicked OK (backwards compatibility).
    /// </summary>
    public bool IsConfirmed => Result == true;

    public MessageDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the message to display.
    /// </summary>
    public void SetMessage(string message)
    {
        MessageText.Text = message;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
    }
}
