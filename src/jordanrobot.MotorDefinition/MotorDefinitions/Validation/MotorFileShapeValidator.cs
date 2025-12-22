using System;
using System.Collections.Generic;
using System.Linq;
using CurveEditor.Models;
using jordanrobot.MotorDefinitions.Dtos;

namespace jordanrobot.MotorDefinitions.Validation;

/// <summary>
/// Provides shape validation helpers for motor definition persistence DTOs.
/// </summary>
internal static class MotorFileShapeValidator
{
    private const double AxisTolerance = 1e-9;

    public static void ValidateVoltageDto(VoltageFileDto voltage, string driveLabel)
    {
        if (voltage.Percent is null)
        {
            throw new InvalidOperationException($"Voltage '{driveLabel}' is missing the percent axis.");
        }

        if (voltage.Rpm is null)
        {
            throw new InvalidOperationException($"Voltage '{driveLabel}' is missing the rpm axis.");
        }

        if (voltage.Series is null)
        {
            throw new InvalidOperationException($"Voltage '{driveLabel}' is missing the series map.");
        }

        if (voltage.Percent.Length != 101)
        {
            throw new InvalidOperationException($"Voltage '{driveLabel}' percent axis must have 101 entries (found {voltage.Percent.Length}).");
        }

        if (voltage.Rpm.Length != 101)
        {
            throw new InvalidOperationException($"Voltage '{driveLabel}' rpm axis must have 101 entries (found {voltage.Rpm.Length}).");
        }

        if (voltage.Series.Count == 0)
        {
            throw new InvalidOperationException($"Voltage '{driveLabel}' must contain at least one series.");
        }

        foreach (var kvp in voltage.Series)
        {
            if (kvp.Value.Torque is null)
            {
                throw new InvalidOperationException($"Voltage '{driveLabel}' series '{kvp.Key}' is missing torque data.");
            }

            if (kvp.Value.Torque.Length != 101)
            {
                throw new InvalidOperationException($"Voltage '{driveLabel}' series '{kvp.Key}' torque array must have 101 entries (found {kvp.Value.Torque.Length}).");
            }
        }
    }

    public static void ValidateRuntimeVoltage(VoltageConfiguration voltage)
    {
        if (voltage.Series.Count == 0)
        {
            throw new InvalidOperationException($"Voltage {voltage.Voltage}V must contain at least one series.");
        }

        var firstSeries = voltage.Series[0];
        if (firstSeries.Data.Count != 101)
        {
            throw new InvalidOperationException($"Voltage {voltage.Voltage}V series '{firstSeries.Name}' must have exactly 101 points.");
        }

        var percentAxis = firstSeries.Data.Select(p => p.Percent).ToArray();
        var rpmAxis = firstSeries.Data.Select(p => p.Rpm).ToArray();

        if (percentAxis.Length != 101)
        {
            throw new InvalidOperationException($"Voltage {voltage.Voltage}V percent axis must have 101 entries (found {percentAxis.Length}).");
        }

        foreach (var series in voltage.Series)
        {
            if (series.Data.Count != 101)
            {
                throw new InvalidOperationException($"Voltage {voltage.Voltage}V series '{series.Name}' must have exactly 101 points.");
            }

            for (var i = 0; i < 101; i++)
            {
                if (series.Data[i].Percent != percentAxis[i])
                {
                    throw new InvalidOperationException($"Voltage {voltage.Voltage}V series '{series.Name}' percent axis differs at index {i}.");
                }

                if (Math.Abs(series.Data[i].Rpm - rpmAxis[i]) > AxisTolerance)
                {
                    throw new InvalidOperationException($"Voltage {voltage.Voltage}V series '{series.Name}' rpm axis differs at index {i}.");
                }
            }
        }
    }
}
