using Avalonia.Controls;
using Avalonia.Interactivity;
using CurveEditor.ViewModels;

namespace CurveEditor.Views;

public partial class PreferencesWindow : Window
{
    public PreferencesWindow()
    {
        InitializeComponent();
    }

    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        // ViewModel's SaveCommand is already executed via binding
        Close(true);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
