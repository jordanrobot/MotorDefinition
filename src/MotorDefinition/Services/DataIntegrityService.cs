using JordanRobot.MotorDefinition.Model;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace JordanRobot.MotorDefinition.Services;

/// <summary>
/// Provides data integrity verification using SHA-256 cryptographic checksums.
/// </summary>
public class DataIntegrityService : IDataIntegrityService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false, // Compact for consistent hashing
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc/>
    public ValidationSignature SignMotorProperties(ServoMotor motor, string verifiedBy)
    {
        ArgumentNullException.ThrowIfNull(motor);
        ArgumentException.ThrowIfNullOrWhiteSpace(verifiedBy);

        var checksum = ComputeMotorChecksum(motor);
        return new ValidationSignature(checksum, verifiedBy);
    }

    /// <inheritdoc/>
    public bool VerifyMotorProperties(ServoMotor motor)
    {
        ArgumentNullException.ThrowIfNull(motor);

        if (motor.MotorSignature is null || !motor.MotorSignature.IsValid())
        {
            return false;
        }

        var currentChecksum = ComputeMotorChecksum(motor);
        return string.Equals(currentChecksum, motor.MotorSignature.Checksum, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public ValidationSignature SignDrive(Drive drive, string verifiedBy)
    {
        ArgumentNullException.ThrowIfNull(drive);
        ArgumentException.ThrowIfNullOrWhiteSpace(verifiedBy);

        var checksum = ComputeDriveChecksum(drive);
        return new ValidationSignature(checksum, verifiedBy);
    }

    /// <inheritdoc/>
    public bool VerifyDrive(Drive drive)
    {
        ArgumentNullException.ThrowIfNull(drive);

        if (drive.DriveSignature is null || !drive.DriveSignature.IsValid())
        {
            return false;
        }

        var currentChecksum = ComputeDriveChecksum(drive);
        return string.Equals(currentChecksum, drive.DriveSignature.Checksum, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public ValidationSignature SignCurve(Curve curve, string verifiedBy)
    {
        ArgumentNullException.ThrowIfNull(curve);
        ArgumentException.ThrowIfNullOrWhiteSpace(verifiedBy);

        var checksum = ComputeCurveChecksum(curve);
        return new ValidationSignature(checksum, verifiedBy);
    }

    /// <inheritdoc/>
    public bool VerifyCurve(Curve curve)
    {
        ArgumentNullException.ThrowIfNull(curve);

        if (curve.CurveSignature is null || !curve.CurveSignature.IsValid())
        {
            return false;
        }

        var currentChecksum = ComputeCurveChecksum(curve);
        return string.Equals(currentChecksum, curve.CurveSignature.Checksum, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public string ComputeMotorChecksum(ServoMotor motor)
    {
        ArgumentNullException.ThrowIfNull(motor);

        // Create a normalized representation of motor properties (excluding drives and metadata)
        var motorData = new
        {
            motor.SchemaVersion,
            motor.MotorName,
            motor.Manufacturer,
            motor.PartNumber,
            motor.Power,
            motor.MaxSpeed,
            motor.RatedSpeed,
            motor.RatedContinuousTorque,
            motor.RatedPeakTorque,
            motor.Weight,
            motor.RotorInertia,
            motor.FeedbackPpr,
            motor.HasBrake,
            motor.BrakeTorque,
            motor.BrakeAmperage,
            motor.BrakeVoltage,
            motor.BrakeReleaseTime,
            motor.BrakeEngageTimeDiode,
            motor.BrakeEngageTimeMov,
            motor.BrakeBacklash,
            motor.Units
        };

        return ComputeSha256Hash(motorData);
    }

    /// <inheritdoc/>
    public string ComputeDriveChecksum(Drive drive)
    {
        ArgumentNullException.ThrowIfNull(drive);

        // Create a normalized representation of drive properties (including voltages)
        var driveData = new
        {
            drive.Name,
            drive.Manufacturer,
            drive.PartNumber,
            Voltages = drive.Voltages.Select(v => new
            {
                v.Value,
                v.Power,
                v.MaxSpeed,
                v.RatedSpeed,
                v.RatedContinuousTorque,
                v.RatedPeakTorque,
                v.ContinuousAmperage,
                v.PeakAmperage
            }).ToArray()
        };

        return ComputeSha256Hash(driveData);
    }

    /// <inheritdoc/>
    public string ComputeCurveChecksum(Curve curve)
    {
        ArgumentNullException.ThrowIfNull(curve);

        // Create a normalized representation of curve data
        var curveData = new
        {
            curve.Name,
            curve.Locked,
            curve.Notes,
            Data = curve.Data.Select(d => new
            {
                d.Percent,
                d.Rpm,
                d.Torque
            }).ToArray()
        };

        return ComputeSha256Hash(curveData);
    }

    private static string ComputeSha256Hash(object data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
