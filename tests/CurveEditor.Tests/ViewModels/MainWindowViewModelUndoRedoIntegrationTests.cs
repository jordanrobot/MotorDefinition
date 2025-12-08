using System.Collections.Generic;
using CurveEditor.Models;
using CurveEditor.Services;
using CurveEditor.ViewModels;
using Moq;
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

        var motor = new MotorDefinition
        {
            MotorName = "Original Name",
            Drives = new List<DriveConfiguration>
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

        var motor = new MotorDefinition
        {
            MotorName = "Test Motor",
            Drives = new List<DriveConfiguration>
            {
                new()
                {
                    Name = "Drive A",
                    Voltages = new List<VoltageConfiguration>
                    {
                        new()
                        {
                            Voltage = 208,
                            Series = new List<CurveSeries>
                            {
                                new()
                                {
                                    Name = "Peak",
                                    Data = new List<DataPoint>
                                    {
                                        new() { Rpm = 1000, Torque = 1.0 },
                                        new() { Rpm = 2000, Torque = 2.0 }
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

        vm.CurveDataTableViewModel.UpdateTorque(1, "Peak", 2.5);

        Assert.True(vm.IsDirty);
        Assert.Equal(2.5, motor.Drives[0].Voltages[0].Series[0].Data[1].Torque);

        vm.UndoCommand.Execute(null);

        Assert.Equal(2.0, motor.Drives[0].Voltages[0].Series[0].Data[1].Torque);

        vm.MarkCleanCheckpoint();

        Assert.False(vm.IsDirty);

        vm.CurveDataTableViewModel.UpdateTorque(1, "Peak", 3.0);

        Assert.True(vm.IsDirty);

        vm.UndoCommand.Execute(null);

        Assert.Equal(2.0, motor.Drives[0].Voltages[0].Series[0].Data[1].Torque);
    }
}
