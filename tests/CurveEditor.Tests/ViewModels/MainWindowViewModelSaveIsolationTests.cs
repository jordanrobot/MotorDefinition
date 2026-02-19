using CurveEditor.Services;
using CurveEditor.ViewModels;
using JordanRobot.MotorDefinition.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public sealed class MainWindowViewModelSaveIsolationTests
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

    private static MainWindowViewModel CreateViewModel(Mock<IFileService> fileServiceMock)
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
            _ => Task.FromResult(MainWindowViewModel.UnsavedChangesChoice.Cancel));
    }

    [Fact]
    public async Task Save_WhenServicePathDiffersFromActiveTabPath_SavesToActiveTabPath()
    {
        var activeMotor = new ServoMotor
        {
            MotorName = "Active"
        };

        const string activeTabPath = "c:/tmp/active-tab.json";
        const string servicePath = "c:/tmp/other-tab.json";

        var fileServiceMock = new Mock<IFileService>(MockBehavior.Strict);
        fileServiceMock.SetupGet(f => f.IsDirty).Returns(false);
        fileServiceMock.SetupGet(f => f.CurrentFilePath).Returns(servicePath);
        fileServiceMock
            .Setup(f => f.SaveAsAsync(activeMotor, activeTabPath))
            .Returns(Task.CompletedTask);

        var vm = CreateViewModel(fileServiceMock);
        vm.CurrentMotor = activeMotor;
        vm.CurrentFilePath = activeTabPath;
        vm.IsDirty = true;

        await vm.SaveCommand.ExecuteAsync(null);

        fileServiceMock.Verify(f => f.SaveAsAsync(activeMotor, activeTabPath), Times.Once);
        fileServiceMock.Verify(f => f.SaveAsync(It.IsAny<ServoMotor>()), Times.Never);
        Assert.False(vm.IsDirty);
        Assert.Equal(activeTabPath, vm.CurrentFilePath);
    }
}
