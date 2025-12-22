using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CurveEditor.ViewModels;
using CurveEditor.Views;
using Serilog;
using System;
using System.Linq;

namespace CurveEditor;

public partial class App : Application
{
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
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
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
