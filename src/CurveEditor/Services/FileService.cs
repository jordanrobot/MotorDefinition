using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CurveEditor.Models;
using jordanrobot.MotorDefinitions.Dtos;
using jordanrobot.MotorDefinitions.Mapping;
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
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ICurveGeneratorService _curveGenerator;
    private readonly IValidationService _validationService;

    /// <summary>
    /// Creates a new FileService instance.
    /// </summary>
    /// <param name="curveGenerator">The curve generator service for creating new motor definitions.</param>
    /// <param name="validationService">Validation service for semantic checks.</param>
    public FileService(ICurveGeneratorService curveGenerator, IValidationService validationService)
    {
        _curveGenerator = curveGenerator ?? throw new ArgumentNullException(nameof(curveGenerator));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    /// <summary>
    /// Creates a new FileService instance with a default validation service.
    /// </summary>
    /// <param name="curveGenerator">The curve generator service for creating new motor definitions.</param>
    public FileService(ICurveGeneratorService curveGenerator)
        : this(curveGenerator, new ValidationService())
    {
    }

    /// <summary>
    /// Creates a new FileService instance with default services.
    /// </summary>
    public FileService() : this(new CurveGeneratorService(), new ValidationService())
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
            await using var stream = File.OpenRead(filePath);
            var dto = await JsonSerializer.DeserializeAsync<MotorDefinitionFileDto>(stream, JsonOptions).ConfigureAwait(false);
            if (dto is null)
            {
                throw new InvalidOperationException("Failed to deserialize motor definition: result was null.");
            }

            var motorDefinition = MotorFileMapper.ToRuntimeModel(dto);
            ValidateOrThrow(motorDefinition, filePath);

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
        catch (InvalidOperationException ex)
        {
            Log.Error(ex, "Failed to load motor definition from {FilePath}", filePath);
            throw;
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

        var drive = motor.AddDrive("Default Drive");

        var voltageConfig = drive.AddVoltageConfiguration(220);
        voltageConfig.MaxSpeed = maxSpeed;
        voltageConfig.RatedPeakTorque = maxTorque;
        voltageConfig.RatedContinuousTorque = maxTorque * 0.8;
        voltageConfig.Power = maxPower;

        var peakSeries = new CurveSeries("Peak");
        var continuousSeries = new CurveSeries("Continuous");

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

    private void ValidateOrThrow(MotorDefinition motorDefinition, string filePath)
    {
        var errors = _validationService.ValidateMotorDefinition(motorDefinition);
        if (errors.Count == 0)
        {
            return;
        }

        var message = string.Join("; ", errors);
        Log.Error("Motor definition validation failed for {FilePath}: {ValidationErrors}", filePath, message);
        throw new InvalidOperationException($"The file '{filePath}' is not valid: {errors[0]}");
    }

    private static async Task SerializeAsync(MotorDefinitionFileDto dto, string filePath)
    {
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    private async Task SaveToFileAsync(MotorDefinition motorDefinition, string filePath)
    {
        ArgumentNullException.ThrowIfNull(motorDefinition);

        ValidateOrThrow(motorDefinition, filePath);

        Log.Information("Saving motor definition to {FilePath}", filePath);

        motorDefinition.Metadata.UpdateModified();

        try
        {
            var dto = MotorFileMapper.ToFileDto(motorDefinition);
            await SerializeAsync(dto, filePath).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            Log.Error(ex, "Failed to write motor definition to {FilePath}", filePath);
            throw;
        }

        Log.Debug("Saved motor: {MotorName} to {FilePath}", motorDefinition.MotorName, filePath);
    }
}
