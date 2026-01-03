using CurveEditor.Services;
using MotorEditor.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace CurveEditor.Tests.Services;

public class UserPreferencesServiceTests : IDisposable
{
    private readonly string _tempDir;

    public UserPreferencesServiceTests()
    {
        // Create a temporary directory for testing
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            // Clean up temp directory
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
    public void Constructor_CreatesDefaultPreferences()
    {
        var service = new UserPreferencesService(_tempDir);

        Assert.NotNull(service.Preferences);
        Assert.Equal(2, service.Preferences.DecimalPrecision);
        Assert.Equal("Light", service.Preferences.Theme);
        Assert.NotEmpty(service.Preferences.CurveColors);
    }

    [Fact]
    public void Constructor_CreatesDefaultState()
    {
        var service = new UserPreferencesService(_tempDir);

        Assert.NotNull(service.State);
        Assert.True(service.State.ShowPowerCurves);
        Assert.True(service.State.IsBrowserPanelVisible);
    }

    [Fact]
    public void SavePreferences_PersistsToFile()
    {
        var service = new UserPreferencesService(_tempDir);
        service.Preferences.DecimalPrecision = 4;
        service.Preferences.Theme = "Dark";

        service.SavePreferences();

        // Create new service to verify persistence
        var newService = new UserPreferencesService(_tempDir);
        Assert.Equal(4, newService.Preferences.DecimalPrecision);
        Assert.Equal("Dark", newService.Preferences.Theme);
    }

    [Fact]
    public void SaveState_PersistsToFile()
    {
        var service = new UserPreferencesService(_tempDir);
        service.State.CurrentFilePath = "/test/file.motor";
        service.State.ShowPowerCurves = false;
        service.State.WindowWidth = 1024;
        service.State.WindowHeight = 768;

        service.SaveState();

        // Create new service to verify persistence
        var newService = new UserPreferencesService(_tempDir);
        Assert.Equal("/test/file.motor", newService.State.CurrentFilePath);
        Assert.False(newService.State.ShowPowerCurves);
        Assert.Equal(1024, newService.State.WindowWidth);
        Assert.Equal(768, newService.State.WindowHeight);
    }

    [Fact]
    public void SavePreferences_RaisesPreferencesChangedEvent()
    {
        var service = new UserPreferencesService(_tempDir);
        var eventRaised = false;
        service.PreferencesChanged += (_, _) => eventRaised = true;

        service.SavePreferences();

        Assert.True(eventRaised);
    }

    [Fact]
    public void SaveState_RaisesStateChangedEvent()
    {
        var service = new UserPreferencesService(_tempDir);
        var eventRaised = false;
        service.StateChanged += (_, _) => eventRaised = true;

        service.SaveState();

        Assert.True(eventRaised);
    }

    [Fact]
    public void SavePreferences_WithCustomColors_PersistsCorrectly()
    {
        var service = new UserPreferencesService(_tempDir);
        var customColors = new List<string> { "#123456", "#ABCDEF", "#FF00FF" };
        service.Preferences.CurveColors = customColors;

        service.SavePreferences();

        var newService = new UserPreferencesService(_tempDir);
        Assert.Equal(3, newService.Preferences.CurveColors.Count);
        Assert.Equal("#123456", newService.Preferences.CurveColors[0]);
        Assert.Equal("#ABCDEF", newService.Preferences.CurveColors[1]);
        Assert.Equal("#FF00FF", newService.Preferences.CurveColors[2]);
    }

    [Fact]
    public void SaveState_WithOpenTabs_PersistsCorrectly()
    {
        var service = new UserPreferencesService(_tempDir);
        service.State.OpenTabs = new List<string> { "/file1.motor", "/file2.motor", "/file3.motor" };

        service.SaveState();

        var newService = new UserPreferencesService(_tempDir);
        Assert.Equal(3, newService.State.OpenTabs.Count);
        Assert.Contains("/file1.motor", newService.State.OpenTabs);
        Assert.Contains("/file2.motor", newService.State.OpenTabs);
        Assert.Contains("/file3.motor", newService.State.OpenTabs);
    }

    [Fact]
    public void UserPreferences_Clone_CreatesIndependentCopy()
    {
        var original = new UserPreferences
        {
            DecimalPrecision = 3,
            Theme = "Dark",
            CurveColors = new List<string> { "#FF0000", "#00FF00" }
        };

        var clone = original.Clone();

        // Modify clone
        clone.DecimalPrecision = 5;
        clone.Theme = "Light";
        clone.CurveColors.Add("#0000FF");

        // Original should remain unchanged
        Assert.Equal(3, original.DecimalPrecision);
        Assert.Equal("Dark", original.Theme);
        Assert.Equal(2, original.CurveColors.Count);
    }

    [Fact]
    public void ApplicationState_Clone_CreatesIndependentCopy()
    {
        var original = new ApplicationState
        {
            CurrentFilePath = "/test.motor",
            ShowPowerCurves = false,
            OpenTabs = new List<string> { "/file1.motor" }
        };

        var clone = original.Clone();

        // Modify clone
        clone.CurrentFilePath = "/other.motor";
        clone.ShowPowerCurves = true;
        clone.OpenTabs.Add("/file2.motor");

        // Original should remain unchanged
        Assert.Equal("/test.motor", original.CurrentFilePath);
        Assert.False(original.ShowPowerCurves);
        Assert.Single(original.OpenTabs);
    }
}
