using JordanRobot.MotorDefinition;
using JordanRobot.MotorDefinition.Model;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace CurveEditor.Tests.Services;

public class MotorFileSizeBenchmarkTests
{
    private static readonly JsonSerializerOptions SaveOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void TableFormat_IsSmallerThanLegacyPointFormat()
    {
        var motor = CreateSampleMotor();

        var tempPath = Path.GetTempFileName();
        try
        {
            MotorFile.Save(motor, tempPath);
            var tableJson = File.ReadAllText(tempPath);
            var legacyJson = SerializeLegacy(motor);

            Assert.True(tableJson.Length < legacyJson.Length, $"Expected table format to be smaller. Table={tableJson.Length}, Legacy={legacyJson.Length}");
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    private static string SerializeLegacy(ServoMotor motor)
    {
        var legacy = new
        {
            schemaVersion = ServoMotor.CurrentSchemaVersion,
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
                    voltage = voltage.Value,
                    power = voltage.Power,
                    maxSpeed = voltage.MaxSpeed,
                    ratedSpeed = voltage.RatedSpeed,
                    ratedContinuousTorque = voltage.RatedContinuousTorque,
                    ratedPeakTorque = voltage.RatedPeakTorque,
                    continuousAmperage = voltage.ContinuousAmperage,
                    peakAmperage = voltage.PeakAmperage,
                    series = voltage.Curves.Select(series => new
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

        return JsonSerializer.Serialize(legacy, SaveOptions);
    }

    private static ServoMotor CreateSampleMotor()
    {
        var motor = new ServoMotor("Sample")
        {
            Manufacturer = "Sample Corp",
            PartNumber = "SC-1",
            Power = 1500,
            MaxSpeed = 5000,
            RatedSpeed = 3000,
            RatedContinuousTorque = 45,
            RatedPeakTorque = 55,
            Weight = 10,
            RotorInertia = 0.002m,
            FeedbackPpr = 4096,
            HasBrake = true,
            BrakeTorque = 10,
            BrakeAmperage = 0.5m,
            BrakeVoltage = 24
        };

        var drive = motor.AddDrive("Drive A");
        var voltage = drive.AddVoltage(220);
        voltage.MaxSpeed = 5000;
        voltage.RatedSpeed = 3000;
        voltage.RatedContinuousTorque = 45;
        voltage.RatedPeakTorque = 55;
        voltage.Power = 1500;
        voltage.ContinuousAmperage = 10;
        voltage.PeakAmperage = 25;

        var peak = new Curve("Peak") { Locked = false, Notes = "peak" };
        peak.InitializeData(voltage.MaxSpeed, 55);
        var continuous = new Curve("Continuous") { Locked = true, Notes = "continuous" };
        continuous.InitializeData(voltage.MaxSpeed, 45);

        voltage.Curves.Add(peak);
        voltage.Curves.Add(continuous);

        return motor;
    }
}
