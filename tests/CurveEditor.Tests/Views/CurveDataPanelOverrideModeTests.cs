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
    public void ArrowKeys_CommitOverrideAndMoveSelection()
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

        // Start override mode by simulating a numeric key press,
        // then send a Down arrow key.
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

        // Now press Down arrow while in override mode, which should
        // commit the override and move the selection.
        var downEvent = new KeyEventArgs
        {
            Key = Key.Down,
            Source = dataGrid,
            RoutedEvent = InputElement.KeyDownEvent
        };
        keyDownMethod.Invoke(panel, new object?[] { dataGrid, downEvent });

        // Selection should have moved to the next row
        var afterDown = vm.CurveDataTableViewModel.SelectedCells.Single();
        Assert.NotEqual(initialCell, afterDown);
    }

    [Fact]
    public void OverrideModeCommit_IsUndoableViaGlobalUndo()
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
        voltage.Series.Add(peak);
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

        panel.Measure(new Avalonia.Size(800, 600));
        panel.Arrange(new Avalonia.Rect(0, 0, 800, 600));

        var dataGrid = panel.FindControl<DataGrid>("DataTable");
        Assert.NotNull(dataGrid);

        // Select a single torque cell
        vm.CurveDataTableViewModel.SelectCell(0, 2);
        var originalTorque = voltage.Series[0].Data[0].Torque;

        // Get access to the internal key handler
        var keyDownMethod = typeof(CurveDataPanel)
            .GetMethod("DataTable_KeyDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(keyDownMethod);

        // Simulate typing "12" in override mode via KeyDown
        var key1 = new KeyEventArgs
        {
            Key = Key.D1,
            Source = dataGrid,
            RoutedEvent = InputElement.KeyDownEvent
        };
        keyDownMethod!.Invoke(panel, new object?[] { dataGrid, key1 });

        var key2 = new KeyEventArgs
        {
            Key = Key.D2,
            Source = dataGrid,
            RoutedEvent = InputElement.KeyDownEvent
        };
        keyDownMethod.Invoke(panel, new object?[] { dataGrid, key2 });

        // Value should now reflect the override
        Assert.Equal(12, voltage.Series[0].Data[0].Torque, 3);

        // Press Enter to commit override mode (which should now push an
        // undoable command onto the shared UndoStack)
        var enterEvent = new KeyEventArgs
        {
            Key = Key.Enter,
            Source = dataGrid,
            RoutedEvent = InputElement.KeyDownEvent
        };
        keyDownMethod.Invoke(panel, new object?[] { dataGrid, enterEvent });

        Assert.True(vm.CanUndo);

        vm.UndoCommand.Execute(null);

        // After undo, the torque should return to its original value
        Assert.Equal(originalTorque, voltage.Series[0].Data[0].Torque, 3);
    }
}
