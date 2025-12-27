using CurveEditor.Views;
using JordanRobot.MotorDefinition.Model;
using System;
using System.Linq;

namespace CurveEditor.Services;

/// <summary>
/// Default implementation of <see cref="IMotorConfigurationWorkflow"/> that
/// composes <see cref="IDriveVoltageSeriesService"/> and applies common
/// configuration rules for motor, drive, voltage, and series creation.
/// </summary>
public class MotorConfigurationWorkflow : IMotorConfigurationWorkflow
{
    private readonly IDriveVoltageSeriesService _driveVoltageSeriesService;

    public MotorConfigurationWorkflow(IDriveVoltageSeriesService driveVoltageSeriesService)
    {
        _driveVoltageSeriesService = driveVoltageSeriesService ?? throw new ArgumentNullException(nameof(driveVoltageSeriesService));
    }

    public (Drive Drive, Voltage Voltage) CreateDriveWithVoltage(ServoMotor motor, DriveVoltageDialogResult result)
    {
        ArgumentNullException.ThrowIfNull(motor);
        ArgumentNullException.ThrowIfNull(result);

        return _driveVoltageSeriesService.CreateDriveWithVoltage(
            motor,
            result.Name,
            result.PartNumber,
            result.Manufacturer,
            result.Voltage,
            result.MaxSpeed,
            result.Power,
            result.PeakTorque,
            result.ContinuousTorque,
            result.ContinuousCurrent,
            result.PeakCurrent);
    }

    public (bool IsDuplicate, Voltage? Voltage) CreateVoltageWithSeries(Drive drive, DriveVoltageDialogResult result)
    {
        ArgumentNullException.ThrowIfNull(drive);
        ArgumentNullException.ThrowIfNull(result);

        // Check if voltage already exists for this drive
        if (drive.Voltages.Any(v => Math.Abs(v.Value - result.Voltage) < Drive.DefaultVoltageTolerance))
        {
            return (true, null);
        }

        var voltage = _driveVoltageSeriesService.CreateVoltageWithCurve(
            drive,
            result.Voltage,
            result.MaxSpeed,
            result.Power,
            result.PeakTorque,
            result.ContinuousTorque,
            result.ContinuousCurrent,
            result.PeakCurrent);

        return (false, voltage);
    }

    public Curve CreateSeries(Voltage voltage, AddCurveResult result)
    {
        ArgumentNullException.ThrowIfNull(voltage);
        ArgumentNullException.ThrowIfNull(result);

        var seriesName = _driveVoltageSeriesService.GenerateUniqueName(
            voltage.Curves.Select(s => s.Name),
            result.Name);

        var series = voltage.AddSeries(seriesName, result.BaseTorque);
        series.IsVisible = result.IsVisible;
        series.Locked = result.IsLocked;

        return series;
    }
}
