using System.Collections.Generic;
using JordanRobot.MotorDefinitions.Model;

namespace CurveEditor.Services;

/// <summary>
/// Service for validating motor definition data.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates a data point.
    /// </summary>
    /// <param name="dataPoint">The data point to validate.</param>
    /// <returns>A list of validation errors, or empty if valid.</returns>
    IReadOnlyList<string> ValidateDataPoint(DataPoint dataPoint);

    /// <summary>
    /// Validates a curve series.
    /// </summary>
    /// <param name="series">The curve series to validate.</param>
    /// <returns>A list of validation errors, or empty if valid.</returns>
    IReadOnlyList<string> ValidateCurveSeries(CurveSeries series);

    /// <summary>
    /// Validates a voltage configuration.
    /// </summary>
    /// <param name="voltageConfig">The voltage configuration to validate.</param>
    /// <returns>A list of validation errors, or empty if valid.</returns>
    IReadOnlyList<string> ValidateVoltageConfiguration(VoltageConfiguration voltageConfig);

    /// <summary>
    /// Validates a motor definition.
    /// </summary>
    /// <param name="motor">The motor definition to validate.</param>
    /// <returns>A list of validation errors, or empty if valid.</returns>
    IReadOnlyList<string> ValidateMotorDefinition(MotorDefinition motor);
}
