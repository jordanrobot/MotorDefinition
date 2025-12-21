using System;
using System.Collections.Generic;
using CurveEditor.Models;

namespace CurveEditor.Services;

/// <summary>
/// Default implementation of the validation service.
/// </summary>
public class ValidationService : IValidationService
{
    private const double AxisTolerance = 1e-9;

    /// <inheritdoc />
    public IReadOnlyList<string> ValidateDataPoint(DataPoint dataPoint)
    {
        var errors = new List<string>();

        if (dataPoint.Rpm < 0)
        {
            errors.Add($"RPM cannot be negative (current: {dataPoint.Rpm}).");
        }

        if (dataPoint.Percent < 0 || dataPoint.Percent > 100)
        {
            errors.Add($"Percent must be between 0 and 100 (current: {dataPoint.Percent}).");
        }

        return errors;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> ValidateCurveSeries(CurveSeries series)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(series.Name))
        {
            errors.Add("Series name cannot be empty.");
        }

        if (series.Data.Count != 101)
        {
            errors.Add($"Series must have exactly 101 data points (current: {series.Data.Count}).");
        }

        // Validate data integrity
        if (!series.ValidateDataIntegrity())
        {
            errors.Add("Series data points are not in valid order (0-100% in 1% increments).");
        }

        // Validate ascending RPM
        for (var i = 1; i < series.Data.Count; i++)
        {
            if (series.Data[i].Rpm < series.Data[i - 1].Rpm)
            {
                errors.Add($"RPM values must be ascending. Point {i} ({series.Data[i].Rpm} RPM) is less than point {i - 1} ({series.Data[i - 1].Rpm} RPM).");
                break; // Only report first violation
            }
        }

        // Validate each data point
        foreach (var point in series.Data)
        {
            var pointErrors = ValidateDataPoint(point);
            errors.AddRange(pointErrors);
        }

        return errors;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> ValidateVoltageConfiguration(VoltageConfiguration voltageConfig)
    {
        var errors = new List<string>();

        if (voltageConfig.Voltage <= 0)
        {
            errors.Add($"Voltage must be positive (current: {voltageConfig.Voltage}).");
        }

        if (voltageConfig.MaxSpeed < 0)
        {
            errors.Add($"Max speed cannot be negative (current: {voltageConfig.MaxSpeed}).");
        }

        if (voltageConfig.Power < 0)
        {
            errors.Add($"Power cannot be negative (current: {voltageConfig.Power}).");
        }

        if (voltageConfig.RatedPeakTorque < 0)
        {
            errors.Add($"Peak torque cannot be negative (current: {voltageConfig.RatedPeakTorque}).");
        }

        if (voltageConfig.RatedContinuousTorque < 0)
        {
            errors.Add($"Continuous torque cannot be negative (current: {voltageConfig.RatedContinuousTorque}).");
        }

        if (voltageConfig.RatedContinuousTorque > voltageConfig.RatedPeakTorque)
        {
            errors.Add($"Continuous torque ({voltageConfig.RatedContinuousTorque}) cannot exceed peak torque ({voltageConfig.RatedPeakTorque}).");
        }

        if (voltageConfig.Series.Count == 0)
        {
            errors.Add("Voltage configuration must have at least one curve series.");
            return errors;
        }

        foreach (var series in voltageConfig.Series)
        {
            var seriesErrors = ValidateCurveSeries(series);
            foreach (var error in seriesErrors)
            {
                errors.Add($"Series '{series.Name}': {error}");
            }
        }

        if (voltageConfig.Series.Count > 1)
        {
            var baseline = voltageConfig.Series[0];
            for (var s = 1; s < voltageConfig.Series.Count; s++)
            {
                var candidate = voltageConfig.Series[s];
                var pointCount = Math.Min(baseline.Data.Count, candidate.Data.Count);
                for (var i = 0; i < pointCount; i++)
                {
                    if (candidate.Data[i].Percent != baseline.Data[i].Percent)
                    {
                        errors.Add($"Series '{candidate.Name}' percent axis differs from '{baseline.Name}' at index {i}.");
                        break;
                    }

                    if (Math.Abs(candidate.Data[i].Rpm - baseline.Data[i].Rpm) > AxisTolerance)
                    {
                        errors.Add($"Series '{candidate.Name}' rpm axis differs from '{baseline.Name}' at index {i}.");
                        break;
                    }
                }
            }
        }

        return errors;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> ValidateMotorDefinition(MotorDefinition motor)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(motor.MotorName))
        {
            errors.Add("Motor name cannot be empty.");
        }

        if (motor.MaxSpeed < 0)
        {
            errors.Add($"Max speed cannot be negative (current: {motor.MaxSpeed}).");
        }

        if (motor.Power < 0)
        {
            errors.Add($"Power cannot be negative (current: {motor.Power}).");
        }

        if (motor.RatedPeakTorque < 0)
        {
            errors.Add($"Peak torque cannot be negative (current: {motor.RatedPeakTorque}).");
        }

        if (motor.RatedContinuousTorque < 0)
        {
            errors.Add($"Continuous torque cannot be negative (current: {motor.RatedContinuousTorque}).");
        }

        if (motor.BrakeResponseTime < 0)
        {
            errors.Add($"Brake response time cannot be negative (current: {motor.BrakeResponseTime}).");
        }

        if (motor.BrakeEngageTimeDiode < 0)
        {
            errors.Add($"Brake engage time (diode) cannot be negative (current: {motor.BrakeEngageTimeDiode}).");
        }

        if (motor.BrakeEngageTimeMov < 0)
        {
            errors.Add($"Brake engage time (MOV) cannot be negative (current: {motor.BrakeEngageTimeMov}).");
        }

        if (motor.BrakeBacklash < 0)
        {
            errors.Add($"Brake backlash cannot be negative (current: {motor.BrakeBacklash}).");
        }

        if (motor.Drives.Count == 0)
        {
            errors.Add("Motor must have at least one drive configuration.");
        }

        // Validate each drive
        foreach (var drive in motor.Drives)
        {
            if (string.IsNullOrWhiteSpace(drive.Name))
            {
                errors.Add("Drive name cannot be empty.");
            }

            if (drive.Voltages.Count == 0)
            {
                errors.Add($"Drive '{drive.Name}' must have at least one voltage configuration.");
            }

            foreach (var voltage in drive.Voltages)
            {
                var voltageErrors = ValidateVoltageConfiguration(voltage);
                foreach (var error in voltageErrors)
                {
                    errors.Add($"Drive '{drive.Name}', {voltage.Voltage}V: {error}");
                }
            }
        }

        return errors;
    }
}
