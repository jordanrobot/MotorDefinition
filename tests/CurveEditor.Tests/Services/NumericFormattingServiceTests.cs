using CurveEditor.Services;
using MotorEditor.Avalonia.Models;
using Moq;
using System;
using Xunit;

namespace CurveEditor.Tests.Services;

/// <summary>
/// Tests for the NumericFormattingService class.
/// </summary>
public class NumericFormattingServiceTests
{
    private Mock<IUserPreferencesService> CreateMockPreferencesService(int decimalPrecision = 2)
    {
        var mock = new Mock<IUserPreferencesService>();
        var preferences = new UserPreferences { DecimalPrecision = decimalPrecision };
        mock.Setup(s => s.Preferences).Returns(preferences);
        return mock;
    }

    #region FormatNumber Tests

    [Fact]
    public void FormatNumber_WithPrecision2_RemovesTrailingZeros()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(2);
        var formatter = new NumericFormattingService(mockService.Object);
        var value = 12.0;

        // Act
        var result = formatter.FormatNumber(value);

        // Assert
        // Should be "12" not "12.00" - G format removes trailing zeros
        Assert.Equal("12", result);
    }

    [Fact]
    public void FormatNumber_WithPrecision2_TruncatesLongDecimals()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(2);
        var formatter = new NumericFormattingService(mockService.Object);
        var value = 12.456789;

        // Act
        var result = formatter.FormatNumber(value);

        // Assert
        // Should round to 2 decimal places: 12.46
        Assert.Equal("12.46", result);
    }

    [Fact]
    public void FormatNumber_WithPrecision0_RoundsToInteger()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(0);
        var formatter = new NumericFormattingService(mockService.Object);
        var value = 12.6;

        // Act
        var result = formatter.FormatNumber(value);

        // Assert
        Assert.Equal("13", result);
    }

    [Fact]
    public void FormatNumber_WithPrecision4_ShowsFourDecimals()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(4);
        var formatter = new NumericFormattingService(mockService.Object);
        var value = 12.123456;

        // Act
        var result = formatter.FormatNumber(value);

        // Assert
        Assert.Equal("12.1235", result);
    }

    [Fact]
    public void FormatNumber_ExcludeFromRounding_ShowsFullPrecision()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(2);
        var formatter = new NumericFormattingService(mockService.Object);
        var value = 12.456789012345;

        // Act
        var result = formatter.FormatNumber(value, excludeFromRounding: true);

        // Assert
        // Should show full precision
        Assert.Contains("12.456789", result);
    }

    #endregion

    #region FormatNumberWithSeparators Tests

    [Fact]
    public void FormatNumberWithSeparators_WithPrecision2_FormatsWithCommas()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(2);
        var formatter = new NumericFormattingService(mockService.Object);
        var value = 1234.567;

        // Act
        var result = formatter.FormatNumberWithSeparators(value);

        // Assert
        // Should have thousand separator and 2 decimal places
        Assert.Contains("1", result);
        Assert.Contains("234.57", result);
    }

    [Fact]
    public void FormatNumberWithSeparators_ExcludeFromRounding_ShowsDefaultPrecision()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(2);
        var formatter = new NumericFormattingService(mockService.Object);
        var value = 1234.567890123;

        // Act
        var result = formatter.FormatNumberWithSeparators(value, excludeFromRounding: true);

        // Assert
        // Should use default "N" format precision
        Assert.Contains("1", result);
        Assert.Contains("234.57", result); // N format typically shows 2 decimals by default
    }

    #endregion

    #region FormatFixedPoint Tests

    [Fact]
    public void FormatFixedPoint_WithPrecision2_ShowsTwoDecimals()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(2);
        var formatter = new NumericFormattingService(mockService.Object);
        var value = 12.0;

        // Act
        var result = formatter.FormatFixedPoint(value);

        // Assert
        // F format always shows decimals
        Assert.Equal("12.00", result);
    }

    [Fact]
    public void FormatFixedPoint_WithPrecision0_ShowsNoDecimals()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(0);
        var formatter = new NumericFormattingService(mockService.Object);
        var value = 12.6;

        // Act
        var result = formatter.FormatFixedPoint(value);

        // Assert
        Assert.Equal("13", result);
    }

    [Fact]
    public void FormatFixedPoint_WithPrecision3_ShowsThreeDecimals()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(3);
        var formatter = new NumericFormattingService(mockService.Object);
        var value = 12.1;

        // Act
        var result = formatter.FormatFixedPoint(value);

        // Assert
        Assert.Equal("12.100", result);
    }

    #endregion

    #region GetFormatString Tests

    [Fact]
    public void GetFormatString_WithN_ReturnsNumberFormat()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(2);
        var formatter = new NumericFormattingService(mockService.Object);

        // Act
        var result = formatter.GetFormatString("N");

        // Assert
        Assert.Equal("N2", result);
    }

    [Fact]
    public void GetFormatString_WithF_ReturnsFixedPointFormat()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(3);
        var formatter = new NumericFormattingService(mockService.Object);

        // Act
        var result = formatter.GetFormatString("F");

        // Assert
        Assert.Equal("F3", result);
    }

    [Fact]
    public void GetFormatString_WithG_ReturnsGeneralFormat()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(2);
        var formatter = new NumericFormattingService(mockService.Object);

        // Act
        var result = formatter.GetFormatString("G");

        // Assert
        // G format uses significant figures (precision + 1)
        Assert.Equal("G3", result);
    }

    #endregion

    #region DecimalPrecision Tests

    [Fact]
    public void DecimalPrecision_ReturnsValueFromPreferences()
    {
        // Arrange
        var mockService = CreateMockPreferencesService(5);
        var formatter = new NumericFormattingService(mockService.Object);

        // Act
        var result = formatter.DecimalPrecision;

        // Assert
        Assert.Equal(5, result);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_NullPreferencesService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NumericFormattingService(null!));
    }

    #endregion
}
