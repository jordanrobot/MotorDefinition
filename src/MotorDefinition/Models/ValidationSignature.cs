using System;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Model;

/// <summary>
/// Represents a validation signature for data integrity verification.
/// </summary>
/// <remarks>
/// Contains a checksum, timestamp, and user information to verify that
/// data has not been tampered with since it was signed.
/// </remarks>
public class ValidationSignature
{
    /// <summary>
    /// Gets or sets the cryptographic hash of the signed data.
    /// </summary>
    /// <remarks>
    /// The checksum is computed using the algorithm specified in <see cref="Algorithm"/>.
    /// For SHA256, this will be a 64-character hexadecimal string.
    /// </remarks>
    [JsonPropertyName("checksum")]
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the data was signed.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the person who verified the data.
    /// </summary>
    /// <remarks>
    /// Typically an email address or username.
    /// </remarks>
    [JsonPropertyName("verifiedBy")]
    public string VerifiedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashing algorithm used to compute the checksum.
    /// </summary>
    /// <remarks>
    /// Default is "SHA256". This allows for future algorithm upgrades.
    /// </remarks>
    [JsonPropertyName("algorithm")]
    public string Algorithm { get; set; } = "SHA256";

    /// <summary>
    /// Creates a new ValidationSignature with default values.
    /// </summary>
    public ValidationSignature()
    {
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new ValidationSignature with the specified parameters.
    /// </summary>
    /// <param name="checksum">The cryptographic hash of the signed data.</param>
    /// <param name="verifiedBy">The identifier of the person who verified the data.</param>
    public ValidationSignature(string checksum, string verifiedBy)
    {
        Checksum = checksum;
        VerifiedBy = verifiedBy;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new ValidationSignature with the specified parameters and timestamp.
    /// </summary>
    /// <param name="checksum">The cryptographic hash of the signed data.</param>
    /// <param name="verifiedBy">The identifier of the person who verified the data.</param>
    /// <param name="timestamp">The timestamp when the data was signed.</param>
    public ValidationSignature(string checksum, string verifiedBy, DateTime timestamp)
    {
        Checksum = checksum;
        VerifiedBy = verifiedBy;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Determines whether this signature has valid content.
    /// </summary>
    /// <returns>True if the signature contains a checksum and verifier; otherwise false.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Checksum) &&
               !string.IsNullOrWhiteSpace(VerifiedBy);
    }
}
