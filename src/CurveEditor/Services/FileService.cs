using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CurveEditor.Models;
using Serilog;

namespace CurveEditor.Services;

/// <summary>
/// Service for file operations on motor definition files.
/// </summary>
public class FileService : IFileService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly ICurveGeneratorService _curveGenerator;

    /// <summary>
    /// Creates a new FileService instance.
    /// </summary>
    /// <param name="curveGenerator">The curve generator service for creating new motor definitions.</param>
    public FileService(ICurveGeneratorService curveGenerator)
    {
        _curveGenerator = curveGenerator ?? throw new ArgumentNullException(nameof(curveGenerator));
    }

    /// <summary>
    /// Creates a new FileService instance with a default curve generator.
    /// </summary>
    public FileService() : this(new CurveGeneratorService())
    {
    }

    /// <inheritdoc />
    public string? CurrentFilePath { get; private set; }

    /// <inheritdoc />
    public bool IsDirty { get; private set; }

    /// <inheritdoc />
    public async Task<MotorDefinition> LoadAsync(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        Log.Information("Loading motor definition from {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file '{filePath}' was not found.", filePath);
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var motorDefinition = JsonSerializer.Deserialize<MotorDefinition>(json, JsonOptions);

            if (motorDefinition is null)
            {
                throw new InvalidOperationException("Failed to deserialize motor definition: result was null.");
            }

            CurrentFilePath = filePath;
            IsDirty = false;

            var totalSeries = motorDefinition.GetAllSeries().Count();
            Log.Debug("Loaded motor: {MotorName} with {DriveCount} drives and {SeriesCount} total series", 
                motorDefinition.MotorName, motorDefinition.Drives.Count, totalSeries);

            return motorDefinition;
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to parse JSON from {FilePath}", filePath);
            throw new InvalidOperationException($"The file '{filePath}' contains invalid JSON.", ex);
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(MotorDefinition motorDefinition)
    {
        if (CurrentFilePath is null)
        {
            throw new InvalidOperationException("No file path set. Use SaveAsAsync for new files.");
        }

        await SaveToFileAsync(motorDefinition, CurrentFilePath);
        IsDirty = false;
    }

    /// <inheritdoc />
    public async Task SaveAsAsync(MotorDefinition motorDefinition, string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        await SaveToFileAsync(motorDefinition, filePath);
        CurrentFilePath = filePath;
        IsDirty = false;
    }

    /// <inheritdoc />
    public async Task SaveCopyAsAsync(MotorDefinition motorDefinition, string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        await SaveToFileAsync(motorDefinition, filePath);
        // Note: CurrentFilePath and IsDirty are NOT changed
    }

    /// <inheritdoc />
    public void MarkDirty()
    {
        IsDirty = true;
    }

    /// <inheritdoc />
    public void ClearDirty()
    {
        IsDirty = false;
    }

    /// <inheritdoc />
    public MotorDefinition CreateNew(string motorName, double maxSpeed, double maxTorque, double maxPower)
    {
        Log.Information("Creating new motor definition: {MotorName}", motorName);

        var motor = new MotorDefinition(motorName)
        {
            MaxSpeed = maxSpeed,
            RatedPeakTorque = maxTorque,
            Power = maxPower,
            Metadata = new MotorMetadata
            {
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            }
        };

        // Create default drive configuration
        var drive = motor.AddDrive("Default Drive");
        
        // Add a default voltage configuration
        var voltageConfig = drive.AddVoltageConfiguration(220);
        voltageConfig.MaxSpeed = maxSpeed;
        voltageConfig.RatedPeakTorque = maxTorque;
        voltageConfig.RatedContinuousTorque = maxTorque * 0.8;
        voltageConfig.Power = maxPower;

        // Create default Peak and Continuous series
        var peakSeries = new CurveSeries("Peak");
        var continuousSeries = new CurveSeries("Continuous");

        // Use injected curve generator
        peakSeries.Data = _curveGenerator.InterpolateCurve(maxSpeed, maxTorque, maxPower);
        continuousSeries.Data = _curveGenerator.InterpolateCurve(maxSpeed, maxTorque * 0.8, maxPower * 0.8);

        voltageConfig.Series.Add(peakSeries);
        voltageConfig.Series.Add(continuousSeries);

        CurrentFilePath = null;
        IsDirty = true;

        return motor;
    }

    /// <inheritdoc />
    public void Reset()
    {
        CurrentFilePath = null;
        IsDirty = false;
    }

    private static async Task SaveToFileAsync(MotorDefinition motorDefinition, string filePath)
    {
        ArgumentNullException.ThrowIfNull(motorDefinition);

        Log.Information("Saving motor definition to {FilePath}", filePath);

        motorDefinition.Metadata.UpdateModified();

        var json = JsonSerializer.Serialize(motorDefinition, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);

        Log.Debug("Saved motor: {MotorName} to {FilePath}", motorDefinition.MotorName, filePath);
    }
}
