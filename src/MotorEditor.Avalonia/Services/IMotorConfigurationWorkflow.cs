using CurveEditor.Views;
using JordanRobot.MotorDefinition.Model;

namespace CurveEditor.Services;

/// <summary>
/// Encapsulates higher-level workflows for configuring drives, voltages,
/// and series on a <see cref="ServoMotor"/>. This keeps orchestration
/// logic out of the main window view model while remaining easy to test.
/// </summary>
public interface IMotorConfigurationWorkflow
{
    (Drive Drive, Voltage Voltage) CreateDriveWithVoltage(ServoMotor motor, DriveVoltageDialogResult result);

    (bool IsDuplicate, Voltage? Voltage) CreateVoltageWithSeries(Drive drive, DriveVoltageDialogResult result);

    Curve CreateSeries(Voltage voltage, AddCurveResult result);
}
