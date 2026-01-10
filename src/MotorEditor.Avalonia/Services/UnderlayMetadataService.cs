using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using MotorEditor.Avalonia.Models;

namespace CurveEditor.Services;

/// <summary>
/// Persists chart underlay metadata alongside motor files in a .motorEditor folder.
/// </summary>
public sealed class UnderlayMetadataService
{
    private const string MetadataFolderName = ".motorEditor";

    /// <summary>
    /// Loads metadata for a drive/voltage combination if a metadata file exists.
    /// </summary>
    public UnderlayMetadata? Load(string? motorFilePath, string driveName, double voltageValue)
    {
        var metadataPath = GetMetadataPath(motorFilePath, driveName, voltageValue, ensureFolder: false);
        if (metadataPath is null || !File.Exists(metadataPath))
        {
            return null;
        }

        var json = File.ReadAllText(metadataPath);
        return JsonSerializer.Deserialize<UnderlayMetadata>(json);
    }

    /// <summary>
    /// Saves metadata for a drive/voltage combination. No-op if the motor file path is not known.
    /// </summary>
    public void Save(string? motorFilePath, string driveName, double voltageValue, UnderlayMetadata metadata)
    {
        var metadataPath = GetMetadataPath(motorFilePath, driveName, voltageValue, ensureFolder: true);
        if (metadataPath is null)
        {
            return;
        }

        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(metadataPath, json);
    }

    /// <summary>
    /// Removes persisted metadata for the specified drive/voltage.
    /// </summary>
    public void Delete(string? motorFilePath, string driveName, double voltageValue)
    {
        var metadataPath = GetMetadataPath(motorFilePath, driveName, voltageValue, ensureFolder: false);
        if (metadataPath is null)
        {
            return;
        }

        if (File.Exists(metadataPath))
        {
            File.Delete(metadataPath);
        }
    }

    private static string? GetMetadataPath(string? motorFilePath, string driveName, double voltageValue, bool ensureFolder)
    {
        if (string.IsNullOrWhiteSpace(motorFilePath))
        {
            return null;
        }

        var folder = Path.GetDirectoryName(motorFilePath);
        if (string.IsNullOrWhiteSpace(folder))
        {
            return null;
        }

        var metadataFolder = Path.Combine(folder, MetadataFolderName);
        if (ensureFolder)
        {
            Directory.CreateDirectory(metadataFolder);
        }

        var motorToken = Sanitize(Path.GetFileNameWithoutExtension(motorFilePath) ?? "motor");
        var driveToken = Sanitize(driveName);
        var voltageToken = Sanitize(voltageValue.ToString("0.###", CultureInfo.InvariantCulture));
        var fileName = $"{motorToken}-{driveToken}-{voltageToken}.json";
        return Path.Combine(metadataFolder, fileName);
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(invalid.Contains(ch) ? '_' : ch);
        }

        return builder.ToString();
    }
}
