using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurveEditor.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CurveEditor.ViewModels;

/// <summary>
/// ViewModel for the preferences/options dialog.
/// </summary>
public partial class PreferencesViewModel : ViewModelBase
{
    private readonly IUserPreferencesService _preferencesService;

    [ObservableProperty]
    private int _decimalPrecision;

    [ObservableProperty]
    private string _theme;

    [ObservableProperty]
    private ObservableCollection<string> _curveColors;

    [ObservableProperty]
    private string? _selectedCurveColor;

    /// <summary>
    /// Creates a new PreferencesViewModel.
    /// </summary>
    /// <param name="preferencesService">The preferences service.</param>
    public PreferencesViewModel(IUserPreferencesService preferencesService)
    {
        _preferencesService = preferencesService ?? throw new ArgumentNullException(nameof(preferencesService));

        // Load current preferences
        _decimalPrecision = _preferencesService.Preferences.DecimalPrecision;
        _theme = _preferencesService.Preferences.Theme;
        _curveColors = new ObservableCollection<string>(_preferencesService.Preferences.CurveColors);
    }

    /// <summary>
    /// Saves the current preferences.
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        _preferencesService.Preferences.DecimalPrecision = DecimalPrecision;
        _preferencesService.Preferences.Theme = Theme;
        _preferencesService.Preferences.CurveColors.Clear();
        foreach (var color in CurveColors)
        {
            _preferencesService.Preferences.CurveColors.Add(color);
        }

        _preferencesService.SavePreferences();
    }

    /// <summary>
    /// Adds a new curve color to the list.
    /// </summary>
    [RelayCommand]
    private void AddCurveColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            color = "#000000"; // Default to black
        }

        CurveColors.Add(color);
    }

    /// <summary>
    /// Removes the selected curve color from the list.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRemoveCurveColor))]
    private void RemoveCurveColor()
    {
        if (SelectedCurveColor is not null)
        {
            CurveColors.Remove(SelectedCurveColor);
            SelectedCurveColor = null;
        }
    }

    private bool CanRemoveCurveColor()
    {
        return SelectedCurveColor is not null && CurveColors.Count > 1;
    }

    partial void OnSelectedCurveColorChanged(string? value)
    {
        RemoveCurveColorCommand.NotifyCanExecuteChanged();
    }
}
