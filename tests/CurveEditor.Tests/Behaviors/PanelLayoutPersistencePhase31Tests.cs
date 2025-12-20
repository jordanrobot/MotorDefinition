using System;
using System.IO;
using CurveEditor.Behaviors;
using Xunit;

namespace CurveEditor.Tests.Behaviors;

public class PanelLayoutPersistencePhase31Tests
{
    [Fact]
    public void SaveAndLoadBool_RoundTrips()
    {
        var key = $"Test.Bool.{Guid.NewGuid():N}";

        PanelLayoutPersistence.SaveBool(key, value: true);
        var loaded = PanelLayoutPersistence.LoadBool(key);

        Assert.True(loaded);

        CleanupKeyFile(key);
    }

    [Fact]
    public void LoadBool_InvalidValue_FallsBackToDefault()
    {
        var key = $"Test.Bool.Invalid.{Guid.NewGuid():N}";

        PanelLayoutPersistence.SaveString(key, "not-a-bool");
        var loaded = PanelLayoutPersistence.LoadBool(key, defaultValue: false);

        Assert.False(loaded);

        CleanupKeyFile(key);
    }

    [Fact]
    public void SaveAndLoadDouble_RoundTrips()
    {
        var key = $"Test.Double.{Guid.NewGuid():N}";

        PanelLayoutPersistence.SaveDouble(key, 12.5);
        var loaded = PanelLayoutPersistence.LoadDouble(key, defaultValue: 0);

        Assert.Equal(12.5, loaded);

        CleanupKeyFile(key);
    }

    [Fact]
    public void LoadDouble_InvalidValue_FallsBackToDefault()
    {
        var key = $"Test.Double.Invalid.{Guid.NewGuid():N}";

        PanelLayoutPersistence.SaveString(key, "not-a-double");
        var loaded = PanelLayoutPersistence.LoadDouble(key, defaultValue: 99);

        Assert.Equal(99, loaded);

        CleanupKeyFile(key);
    }

    [Fact]
    public void SaveAndLoadStringArrayAsJson_RoundTripsAndNormalizes()
    {
        var key = $"Test.Array.{Guid.NewGuid():N}";

        PanelLayoutPersistence.SaveStringArrayAsJson(key, new[] { "b", "a", "a", "", "  ", "c" });
        var loaded = PanelLayoutPersistence.LoadStringArrayFromJson(key);

        Assert.Equal(new[] { "a", "b", "c" }, loaded);

        CleanupKeyFile(key);
    }

    [Fact]
    public void LoadStringArrayFromJson_InvalidJson_FallsBackToEmpty()
    {
        var key = $"Test.Array.Invalid.{Guid.NewGuid():N}";

        PanelLayoutPersistence.SaveString(key, "{ invalid json }");
        var loaded = PanelLayoutPersistence.LoadStringArrayFromJson(key);

        Assert.Empty(loaded);

        CleanupKeyFile(key);
    }

    private static void CleanupKeyFile(string settingsKey)
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directory = Path.Combine(appData, "CurveEditor");
            var safeKey = settingsKey
                .Replace(Path.DirectorySeparatorChar, '_')
                .Replace(Path.AltDirectorySeparatorChar, '_');

            var path = Path.Combine(directory, $"layout-{safeKey}.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Ignore cleanup errors.
        }
    }
}
