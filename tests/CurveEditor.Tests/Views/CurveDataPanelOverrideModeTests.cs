using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using CurveEditor.Models;
using CurveEditor.ViewModels;
using CurveEditor.Views;
using Xunit;

namespace CurveEditor.Tests.Views;

public class CurveDataPanelOverrideModeTests
{
    [Fact]
    public void ArrowKeys_DoNotMoveSelectionWhileInOverrideMode()
    {
        var motor = new MotorDefinition
        {
            MaxSpeed = 5000,
            Units = new UnitSettings { Torque = "Nm" }
        };

        var voltage = new VoltageConfiguration(220)
        {
            MaxSpeed = 5000,
            RatedPeakTorque = 50,
            RatedContinuousTorque = 40
        };

        var peak = new CurveSeries("Peak");
        peak.InitializeData(5000, 50);
        var cont = new CurveSeries("Continuous");
        cont.InitializeData(5000, 40);
        voltage.Series.Add(peak);
        voltage.Series.Add(cont);
        motor.Drives.Add(new DriveConfiguration
        {
            Name = "Drive",
            Voltages = { voltage }
        });

        var vm = new MainWindowViewModel
        {
            CurrentMotor = motor,
            SelectedDrive = motor.Drives[0],
            SelectedVoltage = voltage
        };

        var panel = new CurveDataPanel
        {
            DataContext = vm
        };

        // Force template creation
        panel.Measure(new Avalonia.Size(800, 600));
        panel.Arrange(new Avalonia.Rect(0, 0, 800, 600));

        var dataGrid = panel.FindControl<DataGrid>("DataTable");
        Assert.NotNull(dataGrid);

        // Select a single cell
        vm.CurveDataTableViewModel.SelectCell(0, 2);
        var initialCell = vm.CurveDataTableViewModel.SelectedCells.Single();

        // Directly put the panel into override mode by simulating
        // the internal state change, then send a Down arrow key.
        var keyEvent = new KeyEventArgs
        {
            Key = Key.D1,
            Source = dataGrid,
            RoutedEvent = InputElement.KeyDownEvent
        };
        // Call the handler directly to avoid DataGrid's own keyboard handling,
        // which expects a full Avalonia application environment.
        var keyDownMethod = typeof(CurveDataPanel)
            .GetMethod("DataTable_KeyDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(keyDownMethod);

        keyDownMethod!.Invoke(panel, new object?[] { dataGrid, keyEvent });

        // Now press Down arrow while in override mode
        var downEvent = new KeyEventArgs
        {
            Key = Key.Down,
            Source = dataGrid,
            RoutedEvent = InputElement.KeyDownEvent
        };
        keyDownMethod.Invoke(panel, new object?[] { dataGrid, downEvent });

        // Selection should not have moved while override mode is active
        var afterDown = vm.CurveDataTableViewModel.SelectedCells.Single();
        Assert.Equal(initialCell, afterDown);
    }
}
