using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CurveEditor.Models;
using jordanrobot.MotorDefinitions.Mapping;
using Xunit;

namespace CurveEditor.Tests.Services;

public class MotorFileSizeBenchmarkTests
{
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void TableFormat_IsSmallerThanLegacyPointFormat()
    {
        var motor = CreateSampleMotor();

        var tableJson = JsonSerializer.Serialize(MotorFileMapper.ToFileDto(motor), CompactOptions);
        var legacyJson = SerializeLegacy(motor);

        Assert.True(tableJson.Length < legacyJson.Length, $"Expected table format to be smaller. Table={tableJson.Length}, Legacy={legacyJson.Length}");
    }

    private static string SerializeLegacy(MotorDefinition motor)
    {
        var legacy = new
        {
            schemaVersion = MotorDefinition.CurrentSchemaVersion,
            motorName = motor.MotorName,
            manufacturer = motor.Manufacturer,
            partNumber = motor.PartNumber,
            power = motor.Power,
            maxSpeed = motor.MaxSpeed,
            ratedSpeed = motor.RatedSpeed,
            ratedContinuousTorque = motor.RatedContinuousTorque,
            ratedPeakTorque = motor.RatedPeakTorque,
            weight = motor.Weight,
            rotorInertia = motor.RotorInertia,
            feedbackPpr = motor.FeedbackPpr,
            hasBrake = motor.HasBrake,
            brakeTorque = motor.BrakeTorque,
            brakeAmperage = motor.BrakeAmperage,
            brakeVoltage = motor.BrakeVoltage,
            units = new
            {
                torque = motor.Units.Torque,
                speed = motor.Units.Speed,
                power = motor.Units.Power,
                weight = motor.Units.Weight
            },
            drives = motor.Drives.Select(drive => new
            {
                name = drive.Name,
                partNumber = drive.PartNumber,
                manufacturer = drive.Manufacturer,
                voltages = drive.Voltages.Select(voltage => new
                {
                    voltage = voltage.Voltage,
                    power = voltage.Power,
                    maxSpeed = voltage.MaxSpeed,
                    ratedSpeed = voltage.RatedSpeed,
                    ratedContinuousTorque = voltage.RatedContinuousTorque,
                    ratedPeakTorque = voltage.RatedPeakTorque,
                    continuousAmperage = voltage.ContinuousAmperage,
                    peakAmperage = voltage.PeakAmperage,
                    series = voltage.Series.Select(series => new
                    {
                        name = series.Name,
                        notes = series.Notes,
                        locked = series.Locked,
                        data = series.Data.Select(p => new { percent = p.Percent, rpm = p.Rpm, torque = p.Torque }).ToArray()
                    }).ToArray()
                }).ToArray()
            }).ToArray(),
            metadata = new
            {
                created = motor.Metadata.Created,
                modified = motor.Metadata.Modified,
                notes = motor.Metadata.Notes
            }
        };

        return JsonSerializer.Serialize(legacy, CompactOptions);
    }

    private static MotorDefinition CreateSampleMotor()
    {
        var motor = new MotorDefinition("Sample")
        {
            Manufacturer = "Sample Corp",
            PartNumber = "SC-1",
            Power = 1500,
            MaxSpeed = 5000,
            RatedSpeed = 3000,
            RatedContinuousTorque = 45,
            RatedPeakTorque = 55,
            Weight = 10,
            RotorInertia = 0.002,
            FeedbackPpr = 4096,
            HasBrake = true,
            BrakeTorque = 10,
            BrakeAmperage = 0.5,
            BrakeVoltage = 24
        };

        var drive = motor.AddDrive("Drive A");
        var voltage = drive.AddVoltageConfiguration(220);
        voltage.MaxSpeed = 5000;
        voltage.RatedSpeed = 3000;
        voltage.RatedContinuousTorque = 45;
        voltage.RatedPeakTorque = 55;
        voltage.Power = 1500;
        voltage.ContinuousAmperage = 10;
        voltage.PeakAmperage = 25;

        var peak = new CurveSeries("Peak") { Locked = false, Notes = "peak" };
        peak.InitializeData(voltage.MaxSpeed, 55);
        var continuous = new CurveSeries("Continuous") { Locked = true, Notes = "continuous" };
        continuous.InitializeData(voltage.MaxSpeed, 45);

        voltage.Series.Add(peak);
        voltage.Series.Add(continuous);

        return motor;
    }
}
