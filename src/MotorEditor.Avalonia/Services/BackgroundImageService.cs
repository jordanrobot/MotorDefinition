using MotorEditor.Avalonia.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Interface for background image management service.
/// </summary>
public interface IBackgroundImageService
{
    /// <summary>
    /// Loads background image settings for a motor file.
    /// </summary>
    /// <param name="motorFilePath">Path to the motor definition file.</param>
    /// <returns>The loaded settings, or empty settings if none exist.</returns>
    Task<MotorBackgroundImageSettings> LoadSettingsAsync(string motorFilePath);

    /// <summary>
    /// Saves background image settings for a motor file.
    /// </summary>
    /// <param name="motorFilePath">Path to the motor definition file.</param>
    /// <param name="settings">Settings to save.</param>
    Task SaveSettingsAsync(string motorFilePath, MotorBackgroundImageSettings settings);

    /// <summary>
    /// Gets the settings file path for a motor file.
    /// </summary>
    /// <param name="motorFilePath">Path to the motor definition file.</param>
    /// <returns>Path to the settings file.</returns>
    string GetSettingsFilePath(string motorFilePath);
}

/// <summary>
/// Service for managing background image settings persistence.
/// </summary>
public class BackgroundImageService : IBackgroundImageService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<BackgroundImageService>();
    private const string SettingsFolderName = ".motorEditor";
    private const string SettingsFileName = "background-images.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc/>
    public string GetSettingsFilePath(string motorFilePath)
    {
        if (string.IsNullOrEmpty(motorFilePath))
        {
            throw new ArgumentException("Motor file path cannot be null or empty.", nameof(motorFilePath));
        }

        var motorDirectory = Path.GetDirectoryName(motorFilePath);
        if (string.IsNullOrEmpty(motorDirectory))
        {
            throw new ArgumentException("Cannot determine directory from motor file path.", nameof(motorFilePath));
        }

        var settingsDirectory = Path.Combine(motorDirectory, SettingsFolderName);
        return Path.Combine(settingsDirectory, SettingsFileName);
    }

    /// <inheritdoc/>
    public async Task<MotorBackgroundImageSettings> LoadSettingsAsync(string motorFilePath)
    {
        try
        {
            var settingsPath = GetSettingsFilePath(motorFilePath);

            if (!File.Exists(settingsPath))
            {
                Log.Debug("No background image settings file found at {SettingsPath}", settingsPath);
                return new MotorBackgroundImageSettings();
            }

            var json = await File.ReadAllTextAsync(settingsPath);
            var settings = JsonSerializer.Deserialize<MotorBackgroundImageSettings>(json, JsonOptions);

            if (settings == null)
            {
                Log.Warning("Failed to deserialize background image settings from {SettingsPath}", settingsPath);
                return new MotorBackgroundImageSettings();
            }

            // Convert relative paths to absolute paths
            var motorDirectory = Path.GetDirectoryName(motorFilePath);
            if (!string.IsNullOrEmpty(motorDirectory))
            {
                foreach (var image in settings.Images)
                {
                    if (!Path.IsPathRooted(image.ImagePath))
                    {
                        image.ImagePath = Path.GetFullPath(Path.Combine(motorDirectory, image.ImagePath));
                    }
                }
            }

            Log.Information("Loaded {Count} background image settings from {SettingsPath}", 
                settings.Images.Count, settingsPath);

            return settings;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load background image settings from {MotorFilePath}", motorFilePath);
            return new MotorBackgroundImageSettings();
        }
    }

    /// <inheritdoc/>
    public async Task SaveSettingsAsync(string motorFilePath, MotorBackgroundImageSettings settings)
    {
        try
        {
            var settingsPath = GetSettingsFilePath(motorFilePath);
            var settingsDirectory = Path.GetDirectoryName(settingsPath);

            if (string.IsNullOrEmpty(settingsDirectory))
            {
                throw new InvalidOperationException("Cannot determine settings directory.");
            }

            // Create directory if it doesn't exist
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
                Log.Debug("Created settings directory: {SettingsDirectory}", settingsDirectory);
            }

            // Convert absolute paths to relative paths for portability
            var motorDirectory = Path.GetDirectoryName(motorFilePath);
            var settingsToSave = new MotorBackgroundImageSettings
            {
                Images = settings.Images.Select(img => new BackgroundImageSettings
                {
                    ImagePath = MakeRelativePath(motorDirectory ?? string.Empty, img.ImagePath),
                    DriveName = img.DriveName,
                    VoltageValue = img.VoltageValue,
                    IsVisible = img.IsVisible,
                    IsLockedToZero = img.IsLockedToZero,
                    OffsetX = img.OffsetX,
                    OffsetY = img.OffsetY,
                    ScaleX = img.ScaleX,
                    ScaleY = img.ScaleY
                }).ToList()
            };

            var json = JsonSerializer.Serialize(settingsToSave, JsonOptions);
            await File.WriteAllTextAsync(settingsPath, json);

            Log.Information("Saved {Count} background image settings to {SettingsPath}", 
                settings.Images.Count, settingsPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save background image settings to {MotorFilePath}", motorFilePath);
            throw;
        }
    }

    /// <summary>
    /// Converts an absolute path to a relative path.
    /// </summary>
    private string MakeRelativePath(string basePath, string targetPath)
    {
        if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(targetPath))
        {
            return targetPath;
        }

        try
        {
            var baseUri = new Uri(Path.GetFullPath(basePath) + Path.DirectorySeparatorChar);
            var targetUri = new Uri(Path.GetFullPath(targetPath));

            if (baseUri.Scheme != targetUri.Scheme)
            {
                return targetPath; // Different schemes, can't make relative
            }

            var relativeUri = baseUri.MakeRelativeUri(targetUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            // Replace forward slashes with platform-specific separators
            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
        catch
        {
            // If we can't make it relative, return the original path
            return targetPath;
        }
    }
}
