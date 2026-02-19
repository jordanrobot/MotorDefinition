using CurveEditor.Services;
using CurveEditor.ViewModels;
using JordanRobot.MotorDefinition.Model;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public class MainWindowViewModelUndoRedoIntegrationTests
{
    [Fact]
    public void EditMotorName_ThenUndoRedo_RevertsAndReappliesChange()
    {
        var fileServiceMock = new Mock<IFileService>();
        var curveGeneratorMock = new Mock<ICurveGeneratorService>();

        fileServiceMock.SetupGet(f => f.IsDirty).Returns(false);

        var vm = new MainWindowViewModel(fileServiceMock.Object, curveGeneratorMock.Object);

        var motor = new ServoMotor
        {
            MotorName = "Original Name",
            Drives = new List<Drive>
            {
                new()
            }
        };

        vm.CurrentMotor = motor;

        Assert.Equal("Original Name", vm.CurrentMotor.MotorName);
        Assert.Equal("Original Name", vm.MotorNameEditor);

        vm.EditMotorName("New Name");

        Assert.Equal("New Name", vm.CurrentMotor.MotorName);
        Assert.Equal("New Name", vm.MotorNameEditor);
        Assert.True(vm.CanUndo);

        vm.UndoCommand.Execute(null);

        Assert.Equal("Original Name", vm.CurrentMotor.MotorName);
        Assert.Equal("Original Name", vm.MotorNameEditor);

        vm.RedoCommand.Execute(null);

        Assert.Equal("New Name", vm.CurrentMotor.MotorName);
        Assert.Equal("New Name", vm.MotorNameEditor);
    }

    [Fact]
    public void UndoRedo_ThroughDataTable_ChangesAreReversibleAndAffectDirtyState()
    {
        var fileServiceMock = new Mock<IFileService>();
        var curveGeneratorMock = new Mock<ICurveGeneratorService>();

        fileServiceMock.SetupGet(f => f.IsDirty).Returns(false);

        var vm = new MainWindowViewModel(fileServiceMock.Object, curveGeneratorMock.Object);

        var motor = new ServoMotor
        {
            MotorName = "Test Motor",
            Drives = new List<Drive>
            {
                new()
                {
                    Name = "Drive A",
                    Voltages = new List<Voltage>
                    {
                        new()
                        {
                            Value = 208,
                            Curves = new List<Curve>
                            {
                                new()
                                {
                                    Name = "Peak",
                                    Data = new List<DataPoint>
                                    {
                                        new() { Rpm = 1000, Torque = 1.0m },
                                        new() { Rpm = 2000, Torque = 2.0m }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        vm.CurrentMotor = motor;
        vm.SelectedDrive = motor.Drives[0];
        vm.SelectedVoltage = motor.Drives[0].Voltages[0];

        vm.CurveDataTableViewModel.UpdateTorque(1, "Peak", 2.5m);

        Assert.True(vm.IsDirty);
        Assert.Equal(2.5m, motor.Drives[0].Voltages[0].Curves[0].Data[1].Torque);

        vm.UndoCommand.Execute(null);

        Assert.Equal(2.0m, motor.Drives[0].Voltages[0].Curves[0].Data[1].Torque);

        vm.MarkCleanCheckpoint();

        Assert.False(vm.IsDirty);

        vm.CurveDataTableViewModel.UpdateTorque(1, "Peak", 3.0m);

        Assert.True(vm.IsDirty);

        vm.UndoCommand.Execute(null);

        Assert.Equal(2.0m, motor.Drives[0].Voltages[0].Curves[0].Data[1].Torque);
    }

    [Fact]
    public void ClearSelectedTorqueCells_WithUndoStack_IsUndoable()
    {
        var fileServiceMock = new Mock<IFileService>();
        var curveGeneratorMock = new Mock<ICurveGeneratorService>();

        fileServiceMock.SetupGet(f => f.IsDirty).Returns(false);

        var vm = new MainWindowViewModel(fileServiceMock.Object, curveGeneratorMock.Object);

        var motor = new ServoMotor
        {
            MotorName = "Test Motor",
            Drives = new List<Drive>
            {
                new()
                {
                    Name = "Drive A",
                    Voltages = new List<Voltage>
                    {
                        new()
                        {
                            Value = 208,
                            Curves = new List<Curve>
                            {
                                new()
                                {
                                    Name = "Peak",
                                    Data = new List<DataPoint>
                                    {
                                        new() { Rpm = 1000, Torque = 1.0m },
                                        new() { Rpm = 2000, Torque = 2.0m }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        vm.CurrentMotor = motor;
        vm.SelectedDrive = motor.Drives[0];
        vm.SelectedVoltage = motor.Drives[0].Voltages[0];

        // Select the second row, first series torque cell
        vm.CurveDataTableViewModel.SelectCell(1, 2);
        vm.CurveDataTableViewModel.ClearSelectedTorqueCells();

        Assert.Equal(0.0m, motor.Drives[0].Voltages[0].Curves[0].Data[1].Torque);
        Assert.True(vm.CanUndo);

        vm.UndoCommand.Execute(null);

        Assert.Equal(2.0m, motor.Drives[0].Voltages[0].Curves[0].Data[1].Torque);
    }

    [Fact]
    public void ToggleSeriesLock_ThenUndoRedo_RevertsLockedState()
    {
        var fileServiceMock = new Mock<IFileService>();
        var curveGeneratorMock = new Mock<ICurveGeneratorService>();

        fileServiceMock.SetupGet(f => f.IsDirty).Returns(false);

        var vm = new MainWindowViewModel(fileServiceMock.Object, curveGeneratorMock.Object);

        var motor = new ServoMotor
        {
            MotorName = "Test Motor",
            Drives = new List<Drive>
            {
                new()
                {
                    Name = "Drive A",
                    Voltages = new List<Voltage>
                    {
                        new()
                        {
                            Value = 208,
                            Curves = new List<Curve>
                            {
                                new()
                                {
                                    Name = "Peak",
                                    Locked = false
                                }
                            }
                        }
                    }
                }
            }
        };

        vm.CurrentMotor = motor;
        vm.SelectedDrive = motor.Drives[0];
        vm.SelectedVoltage = motor.Drives[0].Voltages[0];

        var series = motor.Drives[0].Voltages[0].Curves[0];

        Assert.False(series.Locked);

        vm.ToggleSeriesLockCommand.Execute(series);

        Assert.True(series.Locked);
        Assert.True(vm.CanUndo);

        vm.UndoCommand.Execute(null);

        Assert.False(series.Locked);

        vm.RedoCommand.Execute(null);

        Assert.True(series.Locked);
    }
}
