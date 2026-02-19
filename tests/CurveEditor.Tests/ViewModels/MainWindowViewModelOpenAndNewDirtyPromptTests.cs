using CurveEditor.Services;
using CurveEditor.ViewModels;
using JordanRobot.MotorDefinition.Model;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public sealed class MainWindowViewModelOpenAndNewDirtyPromptTests
{
    [Fact]
    public async Task OpenFile_WhenDirtyAndUserCancels_SetsStatusAndDoesNotTryFileDialogs()
    {
        var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
        fileServiceMock.SetupGet(f => f.IsDirty).Returns(false);
        fileServiceMock.SetupGet(f => f.CurrentFilePath).Returns((string?)null);

        var curveGeneratorMock = new Mock<ICurveGeneratorService>(MockBehavior.Loose);
        var validationServiceMock = new Mock<IValidationService>(MockBehavior.Strict);
        validationServiceMock
            .Setup(v => v.ValidateServoMotor(It.IsAny<ServoMotor>()))
            .Returns(Array.Empty<string>());
        var driveVoltageSeriesServiceMock = new Mock<IDriveVoltageSeriesService>(MockBehavior.Loose);
        var workflowMock = new Mock<IMotorConfigurationWorkflow>(MockBehavior.Loose);

        var vm = new MainWindowViewModel(
            fileServiceMock.Object,
            curveGeneratorMock.Object,
            validationServiceMock.Object,
            driveVoltageSeriesServiceMock.Object,
            workflowMock.Object,
            new ChartViewModel(),
            new CurveDataTableViewModel(),
            settingsStore: null,
            unsavedChangesPromptAsync: _ => Task.FromResult(MainWindowViewModel.UnsavedChangesChoice.Cancel));

        vm.IsDirty = true;

        await vm.OpenFileCommand.ExecuteAsync(null);

        Assert.Equal("Open cancelled.", vm.StatusMessage);
    }

    [Fact]
    public async Task NewMotor_WhenDirtyAndUserCancels_DoesNotCreateNewMotor()
    {
        var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
        fileServiceMock.SetupGet(f => f.IsDirty).Returns(false);
        fileServiceMock.SetupGet(f => f.CurrentFilePath).Returns((string?)null);

        var curveGeneratorMock = new Mock<ICurveGeneratorService>(MockBehavior.Loose);
        var validationServiceMock = new Mock<IValidationService>(MockBehavior.Strict);
        validationServiceMock
            .Setup(v => v.ValidateServoMotor(It.IsAny<ServoMotor>()))
            .Returns(Array.Empty<string>());
        var driveVoltageSeriesServiceMock = new Mock<IDriveVoltageSeriesService>(MockBehavior.Loose);
        var workflowMock = new Mock<IMotorConfigurationWorkflow>(MockBehavior.Loose);

        var vm = new MainWindowViewModel(
            fileServiceMock.Object,
            curveGeneratorMock.Object,
            validationServiceMock.Object,
            driveVoltageSeriesServiceMock.Object,
            workflowMock.Object,
            new ChartViewModel(),
            new CurveDataTableViewModel(),
            settingsStore: null,
            unsavedChangesPromptAsync: _ => Task.FromResult(MainWindowViewModel.UnsavedChangesChoice.Cancel));

        var existing = new ServoMotor { MotorName = "Existing" };
        vm.CurrentMotor = existing;
        vm.IsDirty = true;

        await vm.NewMotorCommand.ExecuteAsync(null);

        Assert.Same(existing, vm.CurrentMotor);
        Assert.True(vm.IsDirty);
        Assert.Equal("New file cancelled.", vm.StatusMessage);
        fileServiceMock.Verify(f => f.CreateNew(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
    }
}
