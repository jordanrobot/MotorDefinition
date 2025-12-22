using CurveEditor.Models;
using CurveEditor.Views;

namespace CurveEditor.Services;

/// <summary>
/// Encapsulates higher-level workflows for configuring drives, voltages,
/// and series on a <see cref="MotorDefinition"/>. This keeps orchestration
/// logic out of the main window view model while remaining easy to test.
/// </summary>
public interface IMotorConfigurationWorkflow
{
    (DriveConfiguration Drive, VoltageConfiguration Voltage) CreateDriveWithVoltage(MotorDefinition motor, DriveVoltageDialogResult result);

    (bool IsDuplicate, VoltageConfiguration? Voltage) CreateVoltageWithSeries(DriveConfiguration drive, DriveVoltageDialogResult result);

    CurveSeries CreateSeries(VoltageConfiguration voltage, AddCurveSeriesResult result);
}
