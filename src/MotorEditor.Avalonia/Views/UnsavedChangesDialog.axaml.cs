using Avalonia.Controls;
using Avalonia.Interactivity;
using CurveEditor.ViewModels;

namespace CurveEditor.Views;

public partial class UnsavedChangesDialog : Window
{
    public MainWindowViewModel.UnsavedChangesChoice Choice { get; private set; } = MainWindowViewModel.UnsavedChangesChoice.Cancel;

    private string _actionDescription = "continue";

    public string ActionDescription
    {
        get => _actionDescription;
        set
        {
            _actionDescription = string.IsNullOrWhiteSpace(value) ? "continue" : value;
            UpdateMessage();
        }
    }

    public UnsavedChangesDialog()
    {
        InitializeComponent();
        UpdateMessage();
    }

    private void UpdateMessage()
    {
        MessageText.Text = $"You have unsaved changes. Save before you {ActionDescription}?";
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Choice = MainWindowViewModel.UnsavedChangesChoice.Cancel;
        Close();
    }

    private void OnIgnoreClick(object? sender, RoutedEventArgs e)
    {
        Choice = MainWindowViewModel.UnsavedChangesChoice.Ignore;
        Close();
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        Choice = MainWindowViewModel.UnsavedChangesChoice.Save;
        Close();
    }
}
