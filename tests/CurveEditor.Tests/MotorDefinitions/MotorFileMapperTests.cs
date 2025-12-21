using System;
using System.Linq;
using CurveEditor.Models;
using jordanrobot.MotorDefinitions.Dtos;
using jordanrobot.MotorDefinitions.Mapping;
using Xunit;

namespace CurveEditor.Tests.MotorDefinitions;

public class MotorFileMapperTests
{
    [Fact]
    public void ToFileDto_AndBack_RoundTripsSingleVoltageAndSeriesMetadata()
    {
        var motor = CreateMotorDefinition();
        motor.Units.ResponseTime = "ms";
        motor.Units.Percentage = "%";
        motor.Units.Temperature = "C";
        motor.Units.Backlash = "arcmin";
        motor.BrakeBacklash = 0.5;
        motor.BrakeResponseTime = 12.5;

        var dto = MotorFileMapper.ToFileDto(motor);
        var roundTrip = MotorFileMapper.ToRuntimeModel(dto);

        Assert.Equal(motor.MotorName, roundTrip.MotorName);
        Assert.Equal(motor.Manufacturer, roundTrip.Manufacturer);
        Assert.Equal(motor.BrakeResponseTime, roundTrip.BrakeResponseTime);
        Assert.Equal(motor.BrakeBacklash, roundTrip.BrakeBacklash);
        Assert.Equal(motor.Units.ResponseTime, roundTrip.Units.ResponseTime);
        Assert.Equal(motor.Units.Percentage, roundTrip.Units.Percentage);
        Assert.Equal(motor.Units.Temperature, roundTrip.Units.Temperature);

        var originalSeries = motor.Drives[0].Voltages[0].Series[0];
        var mappedSeries = roundTrip.Drives[0].Voltages[0].Series[0];
        Assert.Equal(originalSeries.Name, mappedSeries.Name);
        Assert.Equal(originalSeries.Locked, mappedSeries.Locked);
        Assert.Equal(originalSeries.Data.Count, mappedSeries.Data.Count);
        Assert.Equal(originalSeries.Data[50].Torque, mappedSeries.Data[50].Torque);
        Assert.Equal(originalSeries.Data[50].Percent, mappedSeries.Data[50].Percent);
        Assert.Equal(originalSeries.Data[50].Rpm, mappedSeries.Data[50].Rpm);
    }

    [Fact]
    public void ToFileDto_AndBack_RoundTripsMultipleVoltages()
    {
        var motor = CreateMotorDefinition(withSecondVoltage: true);
        var dto = MotorFileMapper.ToFileDto(motor);
        var roundTrip = MotorFileMapper.ToRuntimeModel(dto);

        Assert.Equal(1, roundTrip.Drives.Count);
        Assert.Equal(2, roundTrip.Drives[0].Voltages.Count);
        Assert.Equal(motor.Drives[0].Voltages[1].Voltage, roundTrip.Drives[0].Voltages[1].Voltage);
        Assert.Equal(motor.Drives[0].Voltages[1].Series.Count, roundTrip.Drives[0].Voltages[1].Series.Count);
    }

    [Fact]
    public void ToFileDto_MismatchedAxes_ThrowsInvalidOperationException()
    {
        var motor = CreateMotorDefinition();
        motor.Drives[0].Voltages[0].Series[1].Data[100].Percent = 99;

        Assert.Throws<InvalidOperationException>(() => MotorFileMapper.ToFileDto(motor));
    }

    [Fact]
    public void ToRuntimeModel_InvalidTorqueLength_ThrowsInvalidOperationException()
    {
        var dto = MotorFileMapper.ToFileDto(CreateMotorDefinition());
        var voltage = dto.Drives[0].Voltages[0];
        voltage.Series.First().Value.Torque = new double[10];

        Assert.Throws<InvalidOperationException>(() => MotorFileMapper.ToRuntimeModel(dto));
    }

    private static MotorDefinition CreateMotorDefinition(bool withSecondVoltage = false)
    {
        var motor = new MotorDefinition("Test Motor")
        {
            Manufacturer = "Test Mfg",
            PartNumber = "TM-1",
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
            BrakeVoltage = 24,
            BrakeEngageTimeDiode = 5,
            BrakeEngageTimeMov = 7,
            BrakeBacklash = 0.25
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

        if (withSecondVoltage)
        {
            var voltage2 = drive.AddVoltageConfiguration(208);
            voltage2.MaxSpeed = 4800;
            voltage2.RatedSpeed = 2800;
            voltage2.RatedContinuousTorque = 42;
            voltage2.RatedPeakTorque = 52;
            voltage2.Power = 1400;
            voltage2.ContinuousAmperage = 9.5;
            voltage2.PeakAmperage = 22;

            var peak2 = new CurveSeries("Peak") { Locked = false };
            peak2.InitializeData(voltage2.MaxSpeed, 52);
            voltage2.Series.Add(peak2);
        }

        return motor;
    }
}
