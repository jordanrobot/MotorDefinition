using JordanRobot.MotorDefinition.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurveEditor.Services;

/// <summary>
/// Encapsulates drive, voltage, and associated curve series creation logic so that
/// <see cref="CurveEditor.ViewModels.MainWindowViewModel"/> can focus on
/// orchestration concerns.
/// </summary>
public interface IDriveVoltageSeriesService
{
    (Drive drive, Voltage voltage) CreateDriveWithVoltage(
        ServoMotor motor,
        string? name,
        string? partNumber,
        string? manufacturer,
        double voltageValue,
        double maxSpeed,
        double power,
        double peakTorque,
        double continuousTorque,
        double continuousCurrent,
        double peakCurrent);

    Voltage CreateVoltageWithCurve(
        Drive drive,
        double voltageValue,
        double maxSpeed,
        double power,
        double peakTorque,
        double continuousTorque,
        double continuousCurrent,
        double peakCurrent);

    string GenerateUniqueName(IEnumerable<string> existingNames, string baseName);
}

public sealed class DriveVoltageSeriesService : IDriveVoltageSeriesService
{
    public (Drive drive, Voltage voltage) CreateDriveWithVoltage(
        ServoMotor motor,
        string? name,
        string? partNumber,
        string? manufacturer,
        double voltageValue,
        double maxSpeed,
        double power,
        double peakTorque,
        double continuousTorque,
        double continuousCurrent,
        double peakCurrent)
    {
        if (motor is null) throw new ArgumentNullException(nameof(motor));

        var driveName = string.IsNullOrWhiteSpace(name)
            ? GenerateUniqueName(motor.Drives.Select(d => d.Name), "New Drive")
            : GenerateUniqueName(motor.Drives.Select(d => d.Name), name!);

        var drive = motor.AddDrive(driveName);
        drive.PartNumber = partNumber ?? string.Empty;
        drive.Manufacturer = manufacturer ?? string.Empty;

        var voltage = CreateVoltageWithCurve(
            drive,
            voltageValue,
            maxSpeed,
            power,
            peakTorque,
            continuousTorque,
            continuousCurrent,
            peakCurrent);

        return (drive, voltage);
    }

    public Voltage CreateVoltageWithCurve(
        Drive drive,
        double voltageValue,
        double maxSpeed,
        double power,
        double peakTorque,
        double continuousTorque,
        double continuousCurrent,
        double peakCurrent)
    {
        if (drive is null) throw new ArgumentNullException(nameof(drive));

        var voltage = drive.AddVoltage(voltageValue);
        voltage.MaxSpeed = maxSpeed;
        voltage.Power = power;
        voltage.RatedPeakTorque = peakTorque;
        voltage.RatedContinuousTorque = continuousTorque;
        voltage.ContinuousAmperage = continuousCurrent;
        voltage.PeakAmperage = peakCurrent;

        // Create Peak and Continuous torque series
        var peakSeries = new Curve("Peak");
        var continuousSeries = new Curve("Continuous");
        peakSeries.InitializeData(voltage.MaxSpeed, voltage.RatedPeakTorque);
        continuousSeries.InitializeData(voltage.MaxSpeed, voltage.RatedContinuousTorque);
        voltage.Curves.Add(peakSeries);
        voltage.Curves.Add(continuousSeries);

        return voltage;
    }

    public string GenerateUniqueName(IEnumerable<string> existingNames, string baseName)
    {
        var names = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!names.Contains(baseName))
        {
            return baseName;
        }

        var counter = 1;
        string newName;
        do
        {
            newName = $"{baseName} {counter++}";
        } while (names.Contains(newName));

        return newName;
    }
}
