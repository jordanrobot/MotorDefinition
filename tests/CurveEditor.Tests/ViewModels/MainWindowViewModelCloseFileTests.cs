using CommunityToolkit.Mvvm.Input;
using CurveEditor.Services;
using CurveEditor.ViewModels;
using JordanRobot.MotorDefinition.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public class MainWindowViewModelCloseFileTests
{
    private sealed class InMemorySettingsStore : IUserSettingsStore
    {
        private readonly Dictionary<string, object?> _values = new(StringComparer.Ordinal);

        public void SaveString(string settingsKey, string? value) => _values[settingsKey] = value;

        public string? LoadString(string settingsKey)
            => _values.TryGetValue(settingsKey, out var value) ? value as string : null;

        public void SaveBool(string settingsKey, bool value) => _values[settingsKey] = value;

        public bool LoadBool(string settingsKey, bool defaultValue)
            => _values.TryGetValue(settingsKey, out var value) && value is bool b ? b : defaultValue;

        public void SaveDouble(string settingsKey, double value) => _values[settingsKey] = value;

        public double LoadDouble(string settingsKey, double defaultValue)
            => _values.TryGetValue(settingsKey, out var value) && value is double d ? d : defaultValue;

        public IReadOnlyList<string> LoadStringArrayFromJson(string settingsKey)
            => _values.TryGetValue(settingsKey, out var value) && value is IReadOnlyList<string> values
                ? values
                : Array.Empty<string>();

        public void SaveStringArrayAsJson(string settingsKey, IReadOnlyList<string> values)
            => _values[settingsKey] = values;
    }

    private static MainWindowViewModel CreateViewModel(
        Mock<IFileService> fileServiceMock,
        Func<string, Task<MainWindowViewModel.UnsavedChangesChoice>> prompt)
    {
        var curveGeneratorMock = new Mock<ICurveGeneratorService>(MockBehavior.Loose);
        var validationServiceMock = new Mock<IValidationService>(MockBehavior.Strict);
        validationServiceMock
            .Setup(v => v.ValidateServoMotor(It.IsAny<ServoMotor>()))
            .Returns(Array.Empty<string>());

        var driveVoltageSeriesServiceMock = new Mock<IDriveVoltageSeriesService>(MockBehavior.Loose);
        var workflowMock = new Mock<IMotorConfigurationWorkflow>(MockBehavior.Loose);

        return new MainWindowViewModel(
            fileServiceMock.Object,
            curveGeneratorMock.Object,
            validationServiceMock.Object,
            driveVoltageSeriesServiceMock.Object,
            workflowMock.Object,
            new ChartViewModel(),
            new CurveDataTableViewModel(),
            new InMemorySettingsStore(),
            prompt);
    }

    [Fact]
    public void CloseFileCommand_DoesNotExecuteWhenNoMotorIsLoaded()
    {
        var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
        fileServiceMock.SetupGet(f => f.IsDirty).Returns(false);
        fileServiceMock.SetupGet(f => f.CurrentFilePath).Returns((string?)null);

        var vm = CreateViewModel(fileServiceMock, _ => Task.FromResult(MainWindowViewModel.UnsavedChangesChoice.Cancel));

        Assert.False(vm.CloseFileCommand.CanExecute(null));
    }

    [Fact]
    public async Task CloseFile_WhenNotDirty_ClosesAndResetsState()
    {
        var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
        fileServiceMock.SetupGet(f => f.IsDirty).Returns(false);
        fileServiceMock.SetupGet(f => f.CurrentFilePath).Returns("c:/tmp/current.json");
        fileServiceMock.Setup(f => f.Reset());

        var vm = CreateViewModel(fileServiceMock, _ => Task.FromResult(MainWindowViewModel.UnsavedChangesChoice.Cancel));
        vm.CurrentMotor = new ServoMotor { MotorName = "Loaded" };
        vm.CurrentFilePath = "c:/tmp/current.json";
        vm.IsDirty = false;

        await (vm.CloseFileCommand).ExecuteAsync(null);

        Assert.Null(vm.CurrentMotor);
        Assert.Null(vm.CurrentFilePath);
        Assert.False(vm.IsDirty);
        fileServiceMock.Verify(f => f.Reset(), Times.Once);
    }

    [Fact]
    public async Task CloseFile_WhenDirtyAndUserCancels_DoesNotClose()
    {
        var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
        fileServiceMock.SetupGet(f => f.IsDirty).Returns(false);
        fileServiceMock.SetupGet(f => f.CurrentFilePath).Returns("c:/tmp/current.json");

        var vm = CreateViewModel(fileServiceMock, _ => Task.FromResult(MainWindowViewModel.UnsavedChangesChoice.Cancel));
        vm.CurrentMotor = new ServoMotor { MotorName = "Loaded" };
        vm.CurrentFilePath = "c:/tmp/current.json";
        vm.IsDirty = true;

        await (vm.CloseFileCommand).ExecuteAsync(null);

        Assert.NotNull(vm.CurrentMotor);
        Assert.Equal("c:/tmp/current.json", vm.CurrentFilePath);
        Assert.True(vm.IsDirty);
        fileServiceMock.Verify(f => f.Reset(), Times.Never);
    }

    [Fact]
    public async Task CloseFile_WhenDirtyAndUserSaves_SavesThenCloses()
    {
        var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
        fileServiceMock.SetupGet(f => f.IsDirty).Returns(false);
        fileServiceMock.SetupGet(f => f.CurrentFilePath).Returns("c:/tmp/current.json");
        fileServiceMock.Setup(f => f.SaveAsync(It.IsAny<ServoMotor>())).Returns(Task.CompletedTask);
        fileServiceMock.Setup(f => f.Reset());

        var vm = CreateViewModel(fileServiceMock, _ => Task.FromResult(MainWindowViewModel.UnsavedChangesChoice.Save));
        vm.CurrentMotor = new ServoMotor { MotorName = "Loaded" };
        vm.CurrentFilePath = "c:/tmp/current.json";
        vm.IsDirty = true;

        await (vm.CloseFileCommand).ExecuteAsync(null);

        fileServiceMock.Verify(f => f.SaveAsync(It.IsAny<ServoMotor>()), Times.Once);
        fileServiceMock.Verify(f => f.Reset(), Times.Once);
        Assert.Null(vm.CurrentMotor);
        Assert.Null(vm.CurrentFilePath);
        Assert.False(vm.IsDirty);
    }
}
