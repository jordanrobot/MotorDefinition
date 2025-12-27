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
    public Curve GenerateCurve(string name, double maxRpm, double maxTorque, double maxPower)
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
    public List<DataPoint> InterpolateCurve(double maxRpm, double maxTorque, double maxPower)
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
                points.Add(new DataPoint(percent, maxRpm * percent / 100.0, 0));
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
            var rpm = maxRpm * percent / 100.0;
            double torque;

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
                var omega = rpm * 2 * Math.PI / 60;
                torque = maxPower / omega;

                // Ensure torque doesn't go below zero
                torque = Math.Max(0, torque);
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
    public double CalculatePower(double torqueNm, double rpm)
    {
        // Power (W) = Torque (Nm) × Angular velocity (rad/s)
        // Angular velocity = RPM × 2π / 60
        return torqueNm * rpm * 2 * Math.PI / 60;
    }

    /// <inheritdoc />
    public double CalculateCornerSpeed(double maxTorque, double maxPower)
    {
        if (maxTorque <= 0)
        {
            return 0;
        }

        // cornerRpm = (maxPower × 60) / (maxTorque × 2π)
        return (maxPower * 60) / (maxTorque * 2 * Math.PI);
    }
}
