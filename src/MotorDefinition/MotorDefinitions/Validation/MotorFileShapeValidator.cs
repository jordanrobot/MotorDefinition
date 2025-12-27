using JordanRobot.MotorDefinition.Model;
using JordanRobot.MotorDefinition.Persistence.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JordanRobot.MotorDefinition.Persistence.Validation;

/// <summary>
/// Provides shape validation helpers for motor definition persistence DTOs.
/// </summary>
internal static class MotorFileShapeValidator
{
    private const double AxisTolerance = 1e-9;
    private const int MaxSupportedPointCount = 101;

    public static void ValidateVoltageDto(VoltageFileDto voltage, string driveLabel)
    {
        if (voltage.Percent is null)
        {
            throw new InvalidOperationException($"Value '{driveLabel}' is missing the percent axis.");
        }

        if (voltage.Rpm is null)
        {
            throw new InvalidOperationException($"Value '{driveLabel}' is missing the rpm axis.");
        }

        if (voltage.Series is null)
        {
            throw new InvalidOperationException($"Value '{driveLabel}' is missing the series map.");
        }

        if (voltage.Percent.Length > MaxSupportedPointCount)
        {
            throw new InvalidOperationException($"Value '{driveLabel}' percent axis must have 0 to {MaxSupportedPointCount} entries (found {voltage.Percent.Length}).");
        }

        if (voltage.Rpm.Length > MaxSupportedPointCount)
        {
            throw new InvalidOperationException($"Value '{driveLabel}' rpm axis must have 0 to {MaxSupportedPointCount} entries (found {voltage.Rpm.Length}).");
        }

        if (voltage.Rpm.Length != voltage.Percent.Length)
        {
            throw new InvalidOperationException($"Value '{driveLabel}' rpm axis length ({voltage.Rpm.Length}) must match percent axis length ({voltage.Percent.Length}).");
        }

        if (voltage.Series.Count == 0)
        {
            throw new InvalidOperationException($"Value '{driveLabel}' must contain at least one series.");
        }

        var previousPercent = -1;
        for (var i = 0; i < voltage.Percent.Length; i++)
        {
            var percent = voltage.Percent[i];

            if (percent < 0)
            {
                throw new InvalidOperationException($"Value '{driveLabel}' percent axis contains a negative value at index {i}.");
            }

            if (percent <= previousPercent)
            {
                throw new InvalidOperationException($"Value '{driveLabel}' percent axis must be strictly increasing (index {i}).");
            }

            previousPercent = percent;

            if (voltage.Rpm[i] < 0)
            {
                throw new InvalidOperationException($"Value '{driveLabel}' rpm axis contains a negative value at index {i}.");
            }

            if (i > 0 && voltage.Rpm[i] + AxisTolerance < voltage.Rpm[i - 1])
            {
                throw new InvalidOperationException($"Value '{driveLabel}' rpm axis must be non-decreasing (index {i}).");
            }
        }

        foreach (var kvp in voltage.Series)
        {
            if (kvp.Value.Torque is null)
            {
                throw new InvalidOperationException($"Value '{driveLabel}' series '{kvp.Key}' is missing torque data.");
            }

            if (kvp.Value.Torque.Length != voltage.Percent.Length)
            {
                throw new InvalidOperationException($"Value '{driveLabel}' series '{kvp.Key}' torque array length ({kvp.Value.Torque.Length}) must match axis length ({voltage.Percent.Length}).");
            }
        }
    }

    public static void ValidateRuntimeVoltage(Voltage voltage)
    {
        if (voltage.Curves.Count == 0)
        {
            throw new InvalidOperationException($"Value {voltage.Value}V must contain at least one series.");
        }

        var firstSeries = voltage.Curves[0];
        var pointCount = firstSeries.Data.Count;
        if (pointCount > MaxSupportedPointCount)
        {
            throw new InvalidOperationException($"Value {voltage.Value}V series '{firstSeries.Name}' must have 0 to {MaxSupportedPointCount} points.");
        }

        if (pointCount == 0)
        {
            // No curve points. This is valid; UI may fall back to rated torques for visualization.
            return;
        }

        var percentAxis = firstSeries.Data.Select(p => p.Percent).ToArray();
        var rpmAxis = firstSeries.Data.Select(p => p.Rpm).ToArray();

        var previousPercent = -1;
        for (var i = 0; i < percentAxis.Length; i++)
        {
            if (percentAxis[i] < 0)
            {
                throw new InvalidOperationException($"Value {voltage.Value}V percent axis contains a negative value at index {i}.");
            }

            if (percentAxis[i] <= previousPercent)
            {
                throw new InvalidOperationException($"Value {voltage.Value}V percent axis must be strictly increasing (index {i}).");
            }

            previousPercent = percentAxis[i];

            if (rpmAxis[i] < 0)
            {
                throw new InvalidOperationException($"Value {voltage.Value}V rpm axis contains a negative value at index {i}.");
            }

            if (i > 0 && rpmAxis[i] + AxisTolerance < rpmAxis[i - 1])
            {
                throw new InvalidOperationException($"Value {voltage.Value}V rpm axis must be non-decreasing (index {i}).");
            }
        }

        foreach (var series in voltage.Curves)
        {
            if (series.Data.Count != pointCount)
            {
                throw new InvalidOperationException($"Value {voltage.Value}V series '{series.Name}' must have exactly {pointCount} points.");
            }

            for (var i = 0; i < pointCount; i++)
            {
                if (series.Data[i].Percent != percentAxis[i])
                {
                    throw new InvalidOperationException($"Value {voltage.Value}V series '{series.Name}' percent axis differs at index {i}.");
                }

                if (Math.Abs(series.Data[i].Rpm - rpmAxis[i]) > AxisTolerance)
                {
                    throw new InvalidOperationException($"Value {voltage.Value}V series '{series.Name}' rpm axis differs at index {i}.");
                }
            }
        }
    }
}
