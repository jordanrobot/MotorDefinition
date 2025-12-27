using JordanRobot.MotorDefinition;
using JordanRobot.MotorDefinition.Model;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Service for file operations on motor definition files.
/// </summary>
public class FileService : IFileService
{
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
    public async Task<ServoMotor> LoadAsync(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        Log.Information("Loading motor definition from {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file '{filePath}' was not found.", filePath);
        }

        try
        {
            var motorDefinition = await MotorFile.LoadAsync(filePath).ConfigureAwait(false);
            ValidateOrThrow(motorDefinition, filePath);

            CurrentFilePath = filePath;
            IsDirty = false;

            var totalSeries = motorDefinition.Drives
                .SelectMany(d => d.Voltages)
                .SelectMany(v => v.Curves)
                .Count();
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
    public async Task SaveAsync(ServoMotor motorDefinition)
    {
        if (CurrentFilePath is null)
        {
            throw new InvalidOperationException("No file path set. Use SaveAsAsync for new files.");
        }

        await SaveToFileAsync(motorDefinition, CurrentFilePath);
        IsDirty = false;
    }

    /// <inheritdoc />
    public async Task SaveAsAsync(ServoMotor motorDefinition, string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        await SaveToFileAsync(motorDefinition, filePath);
        CurrentFilePath = filePath;
        IsDirty = false;
    }

    /// <inheritdoc />
    public async Task SaveCopyAsAsync(ServoMotor motorDefinition, string filePath)
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
    public ServoMotor CreateNew(string motorName, double maxSpeed, double maxTorque, double maxPower)
    {
        Log.Information("Creating new motor definition: {MotorName}", motorName);

        var motor = new ServoMotor(motorName)
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

        var voltageConfig = drive.AddVoltage(220);
        voltageConfig.MaxSpeed = maxSpeed;
        voltageConfig.RatedPeakTorque = maxTorque;
        voltageConfig.RatedContinuousTorque = maxTorque * 0.8;
        voltageConfig.Power = maxPower;

        var peakSeries = new Curve("Peak");
        var continuousSeries = new Curve("Continuous");

        peakSeries.Data = _curveGenerator.InterpolateCurve(maxSpeed, maxTorque, maxPower);
        continuousSeries.Data = _curveGenerator.InterpolateCurve(maxSpeed, maxTorque * 0.8, maxPower * 0.8);

        voltageConfig.Curves.Add(peakSeries);
        voltageConfig.Curves.Add(continuousSeries);

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

    private void ValidateOrThrow(ServoMotor motorDefinition, string filePath)
    {
        var errors = _validationService.ValidateServoMotor(motorDefinition);
        if (errors.Count == 0)
        {
            return;
        }

        var message = string.Join("; ", errors);
        Log.Error("Motor definition validation failed for {FilePath}: {ValidationErrors}", filePath, message);
        throw new InvalidOperationException($"The file '{filePath}' is not valid: {errors[0]}");
    }

    private async Task SaveToFileAsync(ServoMotor motorDefinition, string filePath)
    {
        ArgumentNullException.ThrowIfNull(motorDefinition);

        ValidateOrThrow(motorDefinition, filePath);

        Log.Information("Saving motor definition to {FilePath}", filePath);

        motorDefinition.Metadata.UpdateModified();

        try
        {
            await MotorFile.SaveAsync(motorDefinition, filePath).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            Log.Error(ex, "Failed to write motor definition to {FilePath}", filePath);
            throw;
        }

        Log.Debug("Saved motor: {MotorName} to {FilePath}", motorDefinition.MotorName, filePath);
    }
}
