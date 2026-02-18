using JordanRobot.MotorDefinition.Model;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Services;

/// <summary>
/// Provides data integrity verification using SHA-256 cryptographic checksums.
/// </summary>
/// <remarks>
/// <para>
/// This service uses a normalized JSON representation with specific serialization options
/// to compute checksums. The serialization format is fixed to ensure checksum stability:
/// - Compact JSON (no indentation)
/// - camelCase property names
/// - SHA-256 hash algorithm
/// - Lowercase hexadecimal output
/// </para>
/// <para>
/// IMPORTANT: Changes to the serialization format will invalidate all existing signatures.
/// The format is considered stable and should not be changed without a major version bump.
/// </para>
/// <para>
/// Checksum Coverage:
/// - Motor checksum: Motor-level properties only (excludes drives and metadata)
/// - Drive checksum: Drive properties and all voltages (excludes curve data)
/// - Curve checksum: Curve name, locked flag, notes, and all data points
/// </para>
/// </remarks>
public class DataIntegrityService : IDataIntegrityService
{
    // IMPORTANT: These options must remain stable to preserve checksum compatibility
    // Changes here will invalidate all existing signatures
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false, // Compact for consistent hashing
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new NormalizedDecimalJsonConverter() }
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
        // NOTE: Drive signature covers drive metadata and voltage configurations,
        // but NOT the curve data. Curves must be signed independently to allow
        // for drive configuration changes without invalidating curve signatures.
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
        // Lowercase hex format for consistency and easier comparison
        // This format is part of the stable API and should not be changed
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Normalizes decimal serialization by stripping trailing zeros.
    /// Ensures that <c>2304.00m</c> and <c>2304m</c> produce the same JSON token (<c>2304</c>),
    /// which is required for stable SHA-256 hashing after save/load round-trips.
    /// </summary>
    private sealed class NormalizedDecimalJsonConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetDecimal();

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            // G29 strips trailing zeros: 2304.00m → "2304", 8.50m → "8.5"
            var normalized = value.ToString("G29", CultureInfo.InvariantCulture);
            writer.WriteRawValue(normalized);
        }
    }
}
