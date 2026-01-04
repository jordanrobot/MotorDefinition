using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CurveEditor.Services;
using CurveEditor.ViewModels;
using CurveEditor.Views;
using Serilog;
using System;
using System.Linq;

namespace CurveEditor;

public partial class App : Application
{
    private IUserPreferencesService? _userPreferencesService;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Create shared preferences service
            _userPreferencesService = new UserPreferencesService();

            // Apply saved theme on startup
            ApplyTheme(_userPreferencesService.Preferences.Theme);

            // Subscribe to preferences changes
            _userPreferencesService.PreferencesChanged += OnPreferencesChanged;

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(_userPreferencesService),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Applies the specified theme to the application.
    /// </summary>
    /// <param name="themeName">The name of the theme: "Light" or "Dark".</param>
    private void ApplyTheme(string themeName)
    {
        try
        {
            var themeVariant = themeName switch
            {
                "Dark" => ThemeVariant.Dark,
                "Light" => ThemeVariant.Light,
                _ => ThemeVariant.Light
            };

            RequestedThemeVariant = themeVariant;
            Log.Information("Applied theme: {ThemeName}", themeName);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to apply theme: {ThemeName}", themeName);
        }
    }

    /// <summary>
    /// Handles preferences changes and applies the new theme.
    /// </summary>
    private void OnPreferencesChanged(object? sender, EventArgs e)
    {
        if (_userPreferencesService is not null)
        {
            ApplyTheme(_userPreferencesService.Preferences.Theme);
        }
    }

    private static async void OnUnhandledDesktopException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled UI exception");

        var message =
            "An unexpected error occurred and was logged. " +
            "You can find log files under %APPDATA%/MotorEditor/logs.\n\n" +
            $"Error: {e.Exception.Message}";

        try
        {
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow is not null)
            {
                var dialog = new MessageDialog
                {
                    Title = "Unexpected Error",
                    Message = message,
                    OkButtonText = "Close",
                    ShowCancelButton = false
                };

                await dialog.ShowDialog(desktop.MainWindow);
            }
        }
        catch (Exception dialogEx)
        {
            Log.Error(dialogEx, "Failed to show unhandled exception dialog");
        }

        e.Handled = true;
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
