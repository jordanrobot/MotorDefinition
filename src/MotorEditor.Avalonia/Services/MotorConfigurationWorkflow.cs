using System;
using System.Linq;
using JordanRobot.MotorDefinitions.Model;
using CurveEditor.Views;

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

    public (DriveConfiguration Drive, VoltageConfiguration Voltage) CreateDriveWithVoltage(MotorDefinition motor, DriveVoltageDialogResult result)
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

    public (bool IsDuplicate, VoltageConfiguration? Voltage) CreateVoltageWithSeries(DriveConfiguration drive, DriveVoltageDialogResult result)
    {
        ArgumentNullException.ThrowIfNull(drive);
        ArgumentNullException.ThrowIfNull(result);

        // Check if voltage already exists for this drive
        if (drive.Voltages.Any(v => Math.Abs(v.Voltage - result.Voltage) < DriveConfiguration.DefaultVoltageTolerance))
        {
            return (true, null);
        }

        var voltage = _driveVoltageSeriesService.CreateVoltageWithSeries(
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

    public CurveSeries CreateSeries(VoltageConfiguration voltage, AddCurveSeriesResult result)
    {
        ArgumentNullException.ThrowIfNull(voltage);
        ArgumentNullException.ThrowIfNull(result);

        var seriesName = _driveVoltageSeriesService.GenerateUniqueName(
            voltage.Series.Select(s => s.Name),
            result.Name);

        var series = voltage.AddSeries(seriesName, result.BaseTorque);
        series.IsVisible = result.IsVisible;
        series.Locked = result.IsLocked;

        return series;
    }
}
