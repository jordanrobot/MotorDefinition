using Avalonia.Controls;
using CurveEditor.Services;
using CurveEditor.ViewModels;
using CurveEditor.Views;
using JordanRobot.MotorDefinition.Model;
using Xunit;

namespace CurveEditor.Tests.Views;

public class CurveDataPanelBindingTests
{
    private static DocumentTab CreateTabWithSeries(string seriesName, decimal torque)
    {
        var chartViewModel = new ChartViewModel();
        var curveDataTableViewModel = new CurveDataTableViewModel();
        var editingCoordinator = new EditingCoordinator();

        chartViewModel.EditingCoordinator = editingCoordinator;
        chartViewModel.UndoStack = new UndoStack();
        curveDataTableViewModel.EditingCoordinator = editingCoordinator;
        curveDataTableViewModel.UndoStack = new UndoStack();

        var motor = new ServoMotor
        {
            MaxSpeed = 5000,
            Units = new UnitSettings { Torque = "Nm" }
        };

        var voltage = new Voltage(220)
        {
            MaxSpeed = 5000,
            RatedPeakTorque = torque,
            RatedContinuousTorque = torque
        };

        var curve = new Curve(seriesName);
        curve.InitializeData(5000, torque);
        voltage.Curves.Add(curve);

        var drive = new Drive
        {
            Name = "Drive"
        };
        drive.Voltages.Add(voltage);
        motor.Drives.Add(drive);

        var tab = new DocumentTab
        {
            Motor = motor,
            SelectedDrive = drive,
            SelectedVoltage = voltage,
            SelectedSeries = curve,
            ChartViewModel = chartViewModel,
            CurveDataTableViewModel = curveDataTableViewModel,
            EditingCoordinator = editingCoordinator
        };

        tab.AvailableDrives.Add(drive);
        tab.AvailableVoltages.Add(voltage);
        tab.AvailableSeries.Add(curve);
        tab.CurveDataTableViewModel.CurrentVoltage = voltage;

        return tab;
    }

    [Fact]
    public void CurveDataPanel_WhenVoltageSelectedAfterInitialLoad_PopulatesTorqueRowsAndColumns()
    {
        var vm = new MainWindowViewModel();

        var panel = new CurveDataPanel
        {
            DataContext = vm
        };

        panel.Measure(new Avalonia.Size(800, 600));
        panel.Arrange(new Avalonia.Rect(0, 0, 800, 600));

        var dataGrid = panel.FindControl<DataGrid>("DataTable");
        Assert.NotNull(dataGrid);

        var motor = new ServoMotor
        {
            MaxSpeed = 5000,
            Units = new UnitSettings { Torque = "Nm" }
        };

        var voltage = new Voltage(220)
        {
            MaxSpeed = 5000,
            RatedPeakTorque = 50,
            RatedContinuousTorque = 40
        };

        var peak = new Curve("Peak");
        peak.InitializeData(5000, 50);
        voltage.Curves.Add(peak);

        var drive = new Drive
        {
            Name = "Drive"
        };
        drive.Voltages.Add(voltage);
        motor.Drives.Add(drive);

        vm.CurrentMotor = motor;
        vm.SelectedDrive = drive;
        vm.SelectedVoltage = voltage;

        var curveDataTableVm = vm.CurveDataTableViewModel;
        Assert.NotNull(curveDataTableVm);
        Assert.NotEmpty(curveDataTableVm.Rows);
        Assert.NotEmpty(vm.AvailableSeries);
        Assert.True(dataGrid!.Columns.Count >= 3);
        Assert.Equal(50m, curveDataTableVm.Rows[0].GetTorque("Peak"));
    }

    [Fact]
    public void CurveDataPanel_WhenActiveTabChanges_RewiresToNewTorqueRowsAndSeries()
    {
        var vm = new MainWindowViewModel();

        var panel = new CurveDataPanel
        {
            DataContext = vm
        };

        panel.Measure(new Avalonia.Size(800, 600));
        panel.Arrange(new Avalonia.Rect(0, 0, 800, 600));

        var dataGrid = panel.FindControl<DataGrid>("DataTable");
        Assert.NotNull(dataGrid);

        var tab = CreateTabWithSeries("SwitchedSeries", 77m);
        vm.ActiveTab = tab;

        Assert.NotEmpty(vm.AvailableSeries);
        Assert.Equal("SwitchedSeries", vm.AvailableSeries[0].Name);

        var curveDataTableVm = vm.CurveDataTableViewModel;
        Assert.NotNull(curveDataTableVm);
        Assert.NotEmpty(curveDataTableVm.Rows);
        Assert.Equal(77m, curveDataTableVm.Rows[0].GetTorque("SwitchedSeries"));
        Assert.True(dataGrid!.Columns.Count >= 3);
    }
}
