using CurveEditor.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace CurveEditor.Tests.Services;

public class RecentFilesServiceTests : IDisposable
{
    private readonly TestUserSettingsStore _settingsStore;
    private readonly RecentFilesService _service;
    private readonly string _tempDir;
    private readonly List<string> _testFiles;

    public RecentFilesServiceTests()
    {
        _settingsStore = new TestUserSettingsStore();
        _service = new RecentFilesService(_settingsStore);
        
        // Create temporary test files
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _testFiles = new List<string>();
        
        for (int i = 1; i <= 12; i++)
        {
            var filePath = Path.Combine(_tempDir, $"test{i}.json");
            File.WriteAllText(filePath, "{}");
            _testFiles.Add(filePath);
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
    public void AddRecentFile_AddsFileToList()
    {
        // Act
        _service.AddRecentFile(_testFiles[0]);

        // Assert
        Assert.Single(_service.RecentFiles);
        Assert.Equal(_testFiles[0], _service.RecentFiles[0]);
    }

    [Fact]
    public void AddRecentFile_AddsSameFileTwice_KeepsOnlyOneEntry()
    {
        // Act
        _service.AddRecentFile(_testFiles[0]);
        _service.AddRecentFile(_testFiles[0]);

        // Assert
        Assert.Single(_service.RecentFiles);
        Assert.Equal(_testFiles[0], _service.RecentFiles[0]);
    }

    [Fact]
    public void AddRecentFile_AddsSameFile_MovesToTop()
    {
        // Arrange
        _service.AddRecentFile(_testFiles[0]);
        _service.AddRecentFile(_testFiles[1]);
        _service.AddRecentFile(_testFiles[2]);

        // Act - Add first file again
        _service.AddRecentFile(_testFiles[0]);

        // Assert - First file should be at the top
        Assert.Equal(3, _service.RecentFiles.Count);
        Assert.Equal(_testFiles[0], _service.RecentFiles[0]);
        Assert.Equal(_testFiles[2], _service.RecentFiles[1]);
        Assert.Equal(_testFiles[1], _service.RecentFiles[2]);
    }

    [Fact]
    public void AddRecentFile_AddsMoreThan10Files_KeepsOnly10()
    {
        // Act - Add 12 files
        for (int i = 0; i < 12; i++)
        {
            _service.AddRecentFile(_testFiles[i]);
        }

        // Assert - Should only have 10 files
        Assert.Equal(10, _service.RecentFiles.Count);
        
        // The most recent 10 should be kept (in reverse order since we add from 0 to 11)
        Assert.Equal(_testFiles[11], _service.RecentFiles[0]);
        Assert.Equal(_testFiles[10], _service.RecentFiles[1]);
        Assert.Equal(_testFiles[2], _service.RecentFiles[9]);
    }

    [Fact]
    public void RemoveRecentFile_RemovesFileFromList()
    {
        // Arrange
        _service.AddRecentFile(_testFiles[0]);
        _service.AddRecentFile(_testFiles[1]);

        // Act
        _service.RemoveRecentFile(_testFiles[0]);

        // Assert
        Assert.Single(_service.RecentFiles);
        Assert.Equal(_testFiles[1], _service.RecentFiles[0]);
    }

    [Fact]
    public void ClearRecentFiles_ClearsAllFiles()
    {
        // Arrange
        _service.AddRecentFile(_testFiles[0]);
        _service.AddRecentFile(_testFiles[1]);
        _service.AddRecentFile(_testFiles[2]);

        // Act
        _service.ClearRecentFiles();

        // Assert
        Assert.Empty(_service.RecentFiles);
    }

    [Fact]
    public void RecentFiles_PersistsAcrossInstances()
    {
        // Arrange - Add files with first instance
        _service.AddRecentFile(_testFiles[0]);
        _service.AddRecentFile(_testFiles[1]);

        // Act - Create new instance with same settings store
        var newService = new RecentFilesService(_settingsStore);

        // Assert - Files should be loaded
        Assert.Equal(2, newService.RecentFiles.Count);
        Assert.Equal(_testFiles[1], newService.RecentFiles[0]);
        Assert.Equal(_testFiles[0], newService.RecentFiles[1]);
    }

    [Fact]
    public void RecentFiles_FiltersOutNonExistentFiles_OnLoad()
    {
        // Arrange - Add files and save
        _service.AddRecentFile(_testFiles[0]);
        _service.AddRecentFile(_testFiles[1]);
        _service.AddRecentFile(_testFiles[2]);

        // Delete one of the files
        File.Delete(_testFiles[1]);

        // Act - Create new instance (should load and filter)
        var newService = new RecentFilesService(_settingsStore);

        // Assert - Only existing files should be loaded
        Assert.Equal(2, newService.RecentFiles.Count);
        Assert.Equal(_testFiles[2], newService.RecentFiles[0]);
        Assert.Equal(_testFiles[0], newService.RecentFiles[1]);
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
