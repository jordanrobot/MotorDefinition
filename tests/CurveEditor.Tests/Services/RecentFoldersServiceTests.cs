using CurveEditor.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace CurveEditor.Tests.Services;

public class RecentFoldersServiceTests : IDisposable
{
    private readonly TestUserSettingsStore _settingsStore;
    private readonly RecentFoldersService _service;
    private readonly string _tempDir;
    private readonly List<string> _testFolders;

    public RecentFoldersServiceTests()
    {
        _settingsStore = new TestUserSettingsStore();
        _service = new RecentFoldersService(_settingsStore);
        
        // Create temporary test folders
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _testFolders = new List<string>();
        
        for (int i = 1; i <= 12; i++)
        {
            var folderPath = Path.Combine(_tempDir, $"testfolder{i}");
            Directory.CreateDirectory(folderPath);
            _testFolders.Add(folderPath);
        }
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Ignore cleanup errors
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void AddRecentFolder_AddsFolderToList()
    {
        // Act
        _service.AddRecentFolder(_testFolders[0]);

        // Assert
        Assert.Single(_service.RecentFolders);
        Assert.Equal(_testFolders[0], _service.RecentFolders[0]);
    }

    [Fact]
    public void AddRecentFolder_AddsSameFolderTwice_KeepsOnlyOneEntry()
    {
        // Act
        _service.AddRecentFolder(_testFolders[0]);
        _service.AddRecentFolder(_testFolders[0]);

        // Assert
        Assert.Single(_service.RecentFolders);
        Assert.Equal(_testFolders[0], _service.RecentFolders[0]);
    }

    [Fact]
    public void AddRecentFolder_AddsSameFolder_MovesToTop()
    {
        // Arrange
        _service.AddRecentFolder(_testFolders[0]);
        _service.AddRecentFolder(_testFolders[1]);
        _service.AddRecentFolder(_testFolders[2]);

        // Act - Add first folder again
        _service.AddRecentFolder(_testFolders[0]);

        // Assert - First folder should be at the top
        Assert.Equal(3, _service.RecentFolders.Count);
        Assert.Equal(_testFolders[0], _service.RecentFolders[0]);
        Assert.Equal(_testFolders[2], _service.RecentFolders[1]);
        Assert.Equal(_testFolders[1], _service.RecentFolders[2]);
    }

    [Fact]
    public void AddRecentFolder_AddsMoreThan10Folders_KeepsOnly10()
    {
        // Act - Add 12 folders
        for (int i = 0; i < 12; i++)
        {
            _service.AddRecentFolder(_testFolders[i]);
        }

        // Assert - Should only have 10 folders
        Assert.Equal(10, _service.RecentFolders.Count);
        
        // The most recent 10 should be kept (in reverse order since we add from 0 to 11)
        Assert.Equal(_testFolders[11], _service.RecentFolders[0]);
        Assert.Equal(_testFolders[10], _service.RecentFolders[1]);
        Assert.Equal(_testFolders[2], _service.RecentFolders[9]);
    }

    [Fact]
    public void RemoveRecentFolder_RemovesFolderFromList()
    {
        // Arrange
        _service.AddRecentFolder(_testFolders[0]);
        _service.AddRecentFolder(_testFolders[1]);

        // Act
        _service.RemoveRecentFolder(_testFolders[0]);

        // Assert
        Assert.Single(_service.RecentFolders);
        Assert.Equal(_testFolders[1], _service.RecentFolders[0]);
    }

    [Fact]
    public void ClearRecentFolders_ClearsAllFolders()
    {
        // Arrange
        _service.AddRecentFolder(_testFolders[0]);
        _service.AddRecentFolder(_testFolders[1]);
        _service.AddRecentFolder(_testFolders[2]);

        // Act
        _service.ClearRecentFolders();

        // Assert
        Assert.Empty(_service.RecentFolders);
    }

    [Fact]
    public void RecentFolders_PersistsAcrossInstances()
    {
        // Arrange - Add folders with first instance
        _service.AddRecentFolder(_testFolders[0]);
        _service.AddRecentFolder(_testFolders[1]);

        // Act - Create new instance with same settings store
        var newService = new RecentFoldersService(_settingsStore);

        // Assert - Folders should be loaded
        Assert.Equal(2, newService.RecentFolders.Count);
        Assert.Equal(_testFolders[1], newService.RecentFolders[0]);
        Assert.Equal(_testFolders[0], newService.RecentFolders[1]);
    }

    [Fact]
    public void RecentFolders_FiltersOutNonExistentFolders_OnLoad()
    {
        // Arrange - Add folders and save
        _service.AddRecentFolder(_testFolders[0]);
        _service.AddRecentFolder(_testFolders[1]);
        _service.AddRecentFolder(_testFolders[2]);

        // Delete one of the folders
        Directory.Delete(_testFolders[1]);

        // Act - Create new instance (should load and filter)
        var newService = new RecentFoldersService(_settingsStore);

        // Assert - Only existing folders should be loaded
        Assert.Equal(2, newService.RecentFolders.Count);
        Assert.Equal(_testFolders[2], newService.RecentFolders[0]);
        Assert.Equal(_testFolders[0], newService.RecentFolders[1]);
    }

    // Test helper class for settings storage
    private class TestUserSettingsStore : IUserSettingsStore
    {
        private readonly Dictionary<string, string?> _stringSettings = new();
        private readonly Dictionary<string, bool> _boolSettings = new();
        private readonly Dictionary<string, double> _doubleSettings = new();

        public string? LoadString(string settingsKey)
        {
            return _stringSettings.TryGetValue(settingsKey, out var value) ? value : null;
        }

        public void SaveString(string settingsKey, string? value)
        {
            _stringSettings[settingsKey] = value;
        }

        public bool LoadBool(string settingsKey, bool defaultValue)
        {
            return _boolSettings.TryGetValue(settingsKey, out var value) ? value : defaultValue;
        }

        public void SaveBool(string settingsKey, bool value)
        {
            _boolSettings[settingsKey] = value;
        }

        public double LoadDouble(string settingsKey, double defaultValue)
        {
            return _doubleSettings.TryGetValue(settingsKey, out var value) ? value : defaultValue;
        }

        public void SaveDouble(string settingsKey, double value)
        {
            _doubleSettings[settingsKey] = value;
        }

        public IReadOnlyList<string> LoadStringArrayFromJson(string settingsKey)
        {
            var json = LoadString(settingsKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return Array.Empty<string>();
            }

            try
            {
                var values = System.Text.Json.JsonSerializer.Deserialize<string[]>(json);
                return values ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public void SaveStringArrayAsJson(string settingsKey, IReadOnlyList<string> values)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(values);
            SaveString(settingsKey, json);
        }
    }
}
