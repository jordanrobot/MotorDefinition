using JordanRobot.MotorDefinition.Model;

namespace CurveEditor.Tests.MotorDefinition;

/// <summary>
/// Tests for the ValidationSignature class.
/// </summary>
public class ValidationSignatureTests
{
    [Fact]
    public void Constructor_DefaultConstructor_SetsTimestampToUtcNow()
    {
        // Arrange & Act
        var before = DateTime.UtcNow;
        var signature = new ValidationSignature();
        var after = DateTime.UtcNow;

        // Assert
        Assert.InRange(signature.Timestamp, before, after);
        Assert.Equal("SHA256", signature.Algorithm);
        Assert.Empty(signature.Checksum);
        Assert.Empty(signature.VerifiedBy);
    }

    [Fact]
    public void Constructor_WithChecksumAndVerifier_SetsProperties()
    {
        // Arrange
        var checksum = "abc123def456";
        var verifiedBy = "user@example.com";

        // Act
        var signature = new ValidationSignature(checksum, verifiedBy);

        // Assert
        Assert.Equal(checksum, signature.Checksum);
        Assert.Equal(verifiedBy, signature.VerifiedBy);
        Assert.Equal("SHA256", signature.Algorithm);
        Assert.InRange(signature.Timestamp, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        var checksum = "abc123def456";
        var verifiedBy = "user@example.com";
        var timestamp = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var signature = new ValidationSignature(checksum, verifiedBy, timestamp);

        // Assert
        Assert.Equal(checksum, signature.Checksum);
        Assert.Equal(verifiedBy, signature.VerifiedBy);
        Assert.Equal(timestamp, signature.Timestamp);
        Assert.Equal("SHA256", signature.Algorithm);
    }

    [Fact]
    public void IsValid_WithChecksumAndVerifier_ReturnsTrue()
    {
        // Arrange
        var signature = new ValidationSignature("abc123", "user@example.com");

        // Act
        var result = signature.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithEmptyChecksum_ReturnsFalse()
    {
        // Arrange
        var signature = new ValidationSignature(string.Empty, "user@example.com");

        // Act
        var result = signature.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithEmptyVerifier_ReturnsFalse()
    {
        // Arrange
        var signature = new ValidationSignature("abc123", string.Empty);

        // Act
        var result = signature.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithWhitespaceChecksum_ReturnsFalse()
    {
        // Arrange
        var signature = new ValidationSignature("   ", "user@example.com");

        // Act
        var result = signature.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithWhitespaceVerifier_ReturnsFalse()
    {
        // Arrange
        var signature = new ValidationSignature("abc123", "   ");

        // Act
        var result = signature.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithDefaultConstructor_ReturnsFalse()
    {
        // Arrange
        var signature = new ValidationSignature();

        // Act
        var result = signature.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Algorithm_CanBeChanged()
    {
        // Arrange
        var signature = new ValidationSignature("abc123", "user@example.com")
        {
            Algorithm = "SHA512"
        };

        // Act & Assert
        Assert.Equal("SHA512", signature.Algorithm);
    }
}
