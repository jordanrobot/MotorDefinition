using JordanRobot.MotorDefinition.Model;
using Serilog;
using System;
using System.Collections.Generic;

namespace CurveEditor.Services;

/// <summary>
/// Service for generating motor torque curves from parameters.
/// </summary>
public class CurveGeneratorService : ICurveGeneratorService
{
    /// <inheritdoc />
    public Curve GenerateCurve(string name, decimal maxRpm, decimal maxTorque, decimal maxPower)
    {
        Log.Debug("Generating curve '{Name}' with maxRpm={MaxRpm}, maxTorque={MaxTorque}, maxPower={MaxPower}",
            name, maxRpm, maxTorque, maxPower);

        var series = new Curve(name)
        {
            Data = InterpolateCurve(maxRpm, maxTorque, maxPower)
        };

        return series;
    }

    /// <inheritdoc />
    public List<DataPoint> InterpolateCurve(decimal maxRpm, decimal maxTorque, decimal maxPower)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxRpm);
        ArgumentOutOfRangeException.ThrowIfNegative(maxTorque);
        ArgumentOutOfRangeException.ThrowIfNegative(maxPower);

        var points = new List<DataPoint>();

        // Handle edge cases
        if (maxRpm <= 0 || maxTorque <= 0 || maxPower <= 0)
        {
            // Return flat curve at zero torque
            for (var percent = 0; percent <= 100; percent++)
            {
                points.Add(new DataPoint(percent, maxRpm * percent / 100m, 0m));
            }
            return points;
        }

        // Calculate corner speed where power limiting begins
        // Power = Torque × Angular velocity = Torque × RPM × (2π / 60)
        // At corner speed: maxPower = maxTorque × cornerRpm × (2π / 60)
        var cornerRpm = CalculateCornerSpeed(maxTorque, maxPower);

        Log.Debug("Calculated corner speed: {CornerRpm} RPM", cornerRpm);

        for (var percent = 0; percent <= 100; percent++)
        {
            var rpm = maxRpm * percent / 100m;
            decimal torque;

            if (rpm <= 0)
            {
                // At zero speed, use max torque
                torque = maxTorque;
            }
            else if (rpm <= cornerRpm)
            {
                // Constant torque region
                torque = maxTorque;
            }
            else
            {
                // Constant power region (torque falls off with speed)
                // Power = Torque × ω, so Torque = Power / ω
                const decimal TwoPi = 6.283185307179586476925286766559m;
                var omega = rpm * TwoPi / 60m;
                torque = omega == 0 ? 0 : maxPower / omega;

                // Ensure torque doesn't go below zero
                torque = Math.Max(0m, torque);
            }

            points.Add(new DataPoint
            {
                Percent = percent,
                Rpm = Math.Round(rpm, 2),
                Torque = Math.Round(torque, 2)
            });
        }

        return points;
    }

    /// <inheritdoc />
    public decimal CalculatePower(decimal torqueNm, decimal rpm)
    {
        // Power (W) = Torque (Nm) × Angular velocity (rad/s)
        // Angular velocity = RPM × 2π / 60
        const decimal TwoPi = 6.283185307179586476925286766559m;
        return torqueNm * rpm * TwoPi / 60m;
    }

    /// <inheritdoc />
    public decimal CalculateCornerSpeed(decimal maxTorque, decimal maxPower)
    {
        if (maxTorque <= 0)
        {
            return 0;
        }

        // cornerRpm = (maxPower × 60) / (maxTorque × 2π)
        const decimal TwoPi = 6.283185307179586476925286766559m;
        return (maxPower * 60m) / (maxTorque * TwoPi);
    }
}
