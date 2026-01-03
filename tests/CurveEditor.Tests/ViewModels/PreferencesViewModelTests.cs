using CurveEditor.Services;
using CurveEditor.ViewModels;
using System;
using System.IO;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public class PreferencesViewModelTests : IDisposable
{
    private readonly string _tempDir;
    private readonly UserPreferencesService _preferencesService;

    public PreferencesViewModelTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _preferencesService = new UserPreferencesService(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_LoadsCurrentPreferences()
    {
        var viewModel = new PreferencesViewModel(_preferencesService);

        Assert.Equal(2, viewModel.DecimalPrecision);
        Assert.Equal("Light", viewModel.Theme);
        Assert.NotEmpty(viewModel.CurveColors);
    }

    [Fact]
    public void SaveCommand_UpdatesPreferences()
    {
        var viewModel = new PreferencesViewModel(_preferencesService);
        viewModel.DecimalPrecision = 4;
        viewModel.Theme = "Dark";

        viewModel.SaveCommand.Execute(null);

        Assert.Equal(4, _preferencesService.Preferences.DecimalPrecision);
        Assert.Equal("Dark", _preferencesService.Preferences.Theme);
    }

    [Fact]
    public void AddCurveColorCommand_AddsColorToList()
    {
        var viewModel = new PreferencesViewModel(_preferencesService);
        var initialCount = viewModel.CurveColors.Count;

        viewModel.AddCurveColorCommand.Execute("#AABBCC");

        Assert.Equal(initialCount + 1, viewModel.CurveColors.Count);
        Assert.Contains("#AABBCC", viewModel.CurveColors);
    }

    [Fact]
    public void RemoveCurveColorCommand_RemovesSelectedColor()
    {
        var viewModel = new PreferencesViewModel(_preferencesService);
        viewModel.CurveColors.Add("#TEST123");
        viewModel.SelectedCurveColor = "#TEST123";

        viewModel.RemoveCurveColorCommand.Execute(null);

        Assert.DoesNotContain("#TEST123", viewModel.CurveColors);
        Assert.Null(viewModel.SelectedCurveColor);
    }

    [Fact]
    public void RemoveCurveColorCommand_CannotExecute_WhenNoColorSelected()
    {
        var viewModel = new PreferencesViewModel(_preferencesService);
        viewModel.SelectedCurveColor = null;

        Assert.False(viewModel.RemoveCurveColorCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveCurveColorCommand_CannotExecute_WhenOnlyOneColorRemains()
    {
        var viewModel = new PreferencesViewModel(_preferencesService);
        viewModel.CurveColors.Clear();
        viewModel.CurveColors.Add("#SINGLE");
        viewModel.SelectedCurveColor = "#SINGLE";

        Assert.False(viewModel.RemoveCurveColorCommand.CanExecute(null));
    }

    [Fact]
    public void SaveCommand_PersistsChangesToService()
    {
        var viewModel = new PreferencesViewModel(_preferencesService);
        viewModel.DecimalPrecision = 3;
        viewModel.Theme = "Dark";
        viewModel.CurveColors.Clear();
        viewModel.CurveColors.Add("#111111");
        viewModel.CurveColors.Add("#222222");

        viewModel.SaveCommand.Execute(null);

        // Create new service instance to verify persistence
        var newService = new UserPreferencesService(_tempDir);
        Assert.Equal(3, newService.Preferences.DecimalPrecision);
        Assert.Equal("Dark", newService.Preferences.Theme);
        Assert.Equal(2, newService.Preferences.CurveColors.Count);
        Assert.Contains("#111111", newService.Preferences.CurveColors);
        Assert.Contains("#222222", newService.Preferences.CurveColors);
    }
}
