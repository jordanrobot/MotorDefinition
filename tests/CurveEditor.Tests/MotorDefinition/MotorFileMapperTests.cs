using JordanRobot.MotorDefinition;
using JordanRobot.MotorDefinition.Model;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Xunit;

namespace CurveEditor.Tests.MotorDefinition;

public class MotorFileMapperTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void SaveAndLoad_RoundTripsSingleVoltageAndSeriesMetadata()
    {
        var motor = CreateMotorDefinition();
        motor.Units.ResponseTime = "ms";
        motor.Units.Percentage = "%";
        motor.Units.Temperature = "C";
        motor.Units.Backlash = "arcmin";
        motor.BrakeBacklash = 0.5m;
        motor.BrakeReleaseTime = 12.5m;

        var tempPath = Path.GetTempFileName();

        try
        {
            MotorFile.Save(motor, tempPath);
            var roundTrip = MotorFile.Load(tempPath);

            Assert.Equal(motor.MotorName, roundTrip.MotorName);
            Assert.Equal(motor.Manufacturer, roundTrip.Manufacturer);
            Assert.Equal(motor.BrakeReleaseTime, roundTrip.BrakeReleaseTime);
            Assert.Equal(motor.BrakeBacklash, roundTrip.BrakeBacklash);
            Assert.Equal(motor.Units.ResponseTime, roundTrip.Units.ResponseTime);
            Assert.Equal(motor.Units.Percentage, roundTrip.Units.Percentage);
            Assert.Equal(motor.Units.Temperature, roundTrip.Units.Temperature);

            var originalSeries = motor.Drives[0].Voltages[0].Curves[0];
            var mappedSeries = roundTrip.Drives[0].Voltages[0].Curves[0];
            Assert.Equal(originalSeries.Name, mappedSeries.Name);
            Assert.Equal(originalSeries.Locked, mappedSeries.Locked);
            Assert.Equal(originalSeries.Data.Count, mappedSeries.Data.Count);
            Assert.Equal(originalSeries.Data[50].Torque, mappedSeries.Data[50].Torque);
            Assert.Equal(originalSeries.Data[50].Percent, mappedSeries.Data[50].Percent);
            Assert.Equal(originalSeries.Data[50].Rpm, mappedSeries.Data[50].Rpm);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void SaveAndLoad_RoundTripsMultipleVoltages()
    {
        var motor = CreateMotorDefinition(withSecondVoltage: true);
        var tempPath = Path.GetTempFileName();

        try
        {
            MotorFile.Save(motor, tempPath);
            var roundTrip = MotorFile.Load(tempPath);

            Assert.Equal(1, roundTrip.Drives.Count);
            Assert.Equal(2, roundTrip.Drives[0].Voltages.Count);
            Assert.Equal(motor.Drives[0].Voltages[1].Value, roundTrip.Drives[0].Voltages[1].Value);
            Assert.Equal(motor.Drives[0].Voltages[1].Curves.Count, roundTrip.Drives[0].Voltages[1].Curves.Count);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void Save_WithMismatchedAxes_ThrowsInvalidOperationException()
    {
        var motor = CreateMotorDefinition();
        motor.Drives[0].Voltages[0].Curves[1].Data[100].Percent = 99;

        var tempPath = Path.GetTempFileName();
        try
        {
            Assert.Throws<InvalidOperationException>(() => MotorFile.Save(motor, tempPath));
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void Load_WithInvalidTorqueLength_ThrowsInvalidOperationException()
    {
        var motor = CreateMotorDefinition();
        var tempPath = Path.GetTempFileName();
        try
        {
            MotorFile.Save(motor, tempPath);

            var node = JsonNode.Parse(File.ReadAllText(tempPath))!;
            var seriesMap = node["drives"]?[0]?["voltages"]?[0]?["series"]?.AsObject();
            if (seriesMap is null)
            {
                throw new InvalidOperationException("Test fixture did not contain expected series map.");
            }

            var firstSeries = seriesMap.First().Value?.AsObject();
            if (firstSeries is null)
            {
                throw new InvalidOperationException("Test fixture did not contain expected series entry.");
            }

            firstSeries["torque"] = new JsonArray(1, 2, 3);

            File.WriteAllText(
                tempPath,
                node.ToJsonString(SerializerOptions));

            Assert.Throws<InvalidOperationException>(() => MotorFile.Load(tempPath));
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    private static ServoMotor CreateMotorDefinition(bool withSecondVoltage = false)
    {
        var motor = new ServoMotor("Test Motor")
        {
            Manufacturer = "Test Mfg",
            PartNumber = "TM-1",
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
            BrakeVoltage = 24,
            BrakeEngageTimeDiode = 5,
            BrakeEngageTimeMov = 7,
            BrakeBacklash = 0.25m
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

        if (withSecondVoltage)
        {
            var voltage2 = drive.AddVoltage(208);
            voltage2.MaxSpeed = 4800;
            voltage2.RatedSpeed = 2800;
            voltage2.RatedContinuousTorque = 42;
            voltage2.RatedPeakTorque = 52;
            voltage2.Power = 1400;
            voltage2.ContinuousAmperage = 9.5m;
            voltage2.PeakAmperage = 22;

            var peak2 = new Curve("Peak") { Locked = false };
            peak2.InitializeData(voltage2.MaxSpeed, 52);
            voltage2.Curves.Add(peak2);
        }

        return motor;
    }
}
