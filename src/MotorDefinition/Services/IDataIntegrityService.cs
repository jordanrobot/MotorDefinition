using JordanRobot.MotorDefinition.Model;
using System;

namespace JordanRobot.MotorDefinition.Services;

/// <summary>
/// Provides services for data integrity verification using cryptographic checksums.
/// </summary>
public interface IDataIntegrityService
{
    /// <summary>
    /// Computes a validation signature for motor properties.
    /// </summary>
    /// <param name="motor">The motor whose properties should be signed.</param>
    /// <param name="verifiedBy">The identifier of the person verifying the data (typically email or username).</param>
    /// <returns>A validation signature containing the checksum and metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when motor is null.</exception>
    /// <exception cref="ArgumentException">Thrown when verifiedBy is null or whitespace.</exception>
    ValidationSignature SignMotorProperties(ServoMotor motor, string verifiedBy);

    /// <summary>
    /// Verifies that motor properties have not been tampered with since signing.
    /// </summary>
    /// <param name="motor">The motor whose properties should be verified.</param>
    /// <returns>True if the signature is valid and matches the current data; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when motor is null.</exception>
    /// <remarks>Returns false if the motor has no signature.</remarks>
    bool VerifyMotorProperties(ServoMotor motor);

    /// <summary>
    /// Computes a validation signature for a drive configuration.
    /// </summary>
    /// <param name="drive">The drive whose properties should be signed.</param>
    /// <param name="verifiedBy">The identifier of the person verifying the data (typically email or username).</param>
    /// <returns>A validation signature containing the checksum and metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when drive is null.</exception>
    /// <exception cref="ArgumentException">Thrown when verifiedBy is null or whitespace.</exception>
    ValidationSignature SignDrive(Drive drive, string verifiedBy);

    /// <summary>
    /// Verifies that drive properties have not been tampered with since signing.
    /// </summary>
    /// <param name="drive">The drive whose properties should be verified.</param>
    /// <returns>True if the signature is valid and matches the current data; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when drive is null.</exception>
    /// <remarks>Returns false if the drive has no signature.</remarks>
    bool VerifyDrive(Drive drive);

    /// <summary>
    /// Computes a validation signature for a curve.
    /// </summary>
    /// <param name="curve">The curve whose data should be signed.</param>
    /// <param name="verifiedBy">The identifier of the person verifying the data (typically email or username).</param>
    /// <returns>A validation signature containing the checksum and metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when curve is null.</exception>
    /// <exception cref="ArgumentException">Thrown when verifiedBy is null or whitespace.</exception>
    ValidationSignature SignCurve(Curve curve, string verifiedBy);

    /// <summary>
    /// Verifies that curve data has not been tampered with since signing.
    /// </summary>
    /// <param name="curve">The curve whose data should be verified.</param>
    /// <returns>True if the signature is valid and matches the current data; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when curve is null.</exception>
    /// <remarks>Returns false if the curve has no signature.</remarks>
    bool VerifyCurve(Curve curve);

    /// <summary>
    /// Computes a checksum for motor properties without creating a signature.
    /// </summary>
    /// <param name="motor">The motor whose properties should be hashed.</param>
    /// <returns>A hexadecimal string representing the SHA-256 hash.</returns>
    /// <exception cref="ArgumentNullException">Thrown when motor is null.</exception>
    string ComputeMotorChecksum(ServoMotor motor);

    /// <summary>
    /// Computes a checksum for drive properties without creating a signature.
    /// </summary>
    /// <param name="drive">The drive whose properties should be hashed.</param>
    /// <returns>A hexadecimal string representing the SHA-256 hash.</returns>
    /// <exception cref="ArgumentNullException">Thrown when drive is null.</exception>
    string ComputeDriveChecksum(Drive drive);

    /// <summary>
    /// Computes a checksum for curve data without creating a signature.
    /// </summary>
    /// <param name="curve">The curve whose data should be hashed.</param>
    /// <returns>A hexadecimal string representing the SHA-256 hash.</returns>
    /// <exception cref="ArgumentNullException">Thrown when curve is null.</exception>
    string ComputeCurveChecksum(Curve curve);
}
