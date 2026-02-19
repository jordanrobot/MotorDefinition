using CurveEditor.Services;
using CurveEditor.ViewModels;
using JordanRobot.MotorDefinition.Model;
using JordanRobot.MotorDefinition.Persistence.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public sealed class MainWindowDirectoryBrowserOpenFolderHighlightTests
{
    private static string TestMotorJson(string motorName)
    {
        var percent = Enumerable.Range(0, 101).ToArray();
        var rpm = percent.Select(p => (decimal)p).ToArray();

        var dto = new MotorDefinitionFileDto
        {
            SchemaVersion = ServoMotor.CurrentSchemaVersion,
            MotorName = motorName,
            Drives =
            [
                new DriveFileDto
                {
                    Name = "Default Drive",
                    Voltages =
                    [
                        new VoltageFileDto
                        {
                            Voltage = 220,
                            Percent = percent,
                            Rpm = rpm,
                            Series = new SortedDictionary<string, SeriesEntryDto>
                            {
                                ["Peak"] = new SeriesEntryDto { Locked = false, Torque = rpm.ToArray() }
                            }
                        }
                    ]
                }
            ]
        };

        return System.Text.Json.JsonSerializer.Serialize(dto);
    }

    private sealed class InMemorySettingsStore : IUserSettingsStore
    {
        private readonly Dictionary<string, string?> _values = new(StringComparer.Ordinal);

        public string? LoadString(string settingsKey) => _values.TryGetValue(settingsKey, out var value) ? value : null;
        public void SaveString(string settingsKey, string? value) => _values[settingsKey] = value;

        public bool LoadBool(string settingsKey, bool defaultValue)
        {
            var value = LoadString(settingsKey);
            return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
        }

        public void SaveBool(string settingsKey, bool value) => SaveString(settingsKey, value.ToString());

        public double LoadDouble(string settingsKey, double defaultValue)
        {
            var value = LoadString(settingsKey);
            return double.TryParse(value, out var parsed) ? parsed : defaultValue;
        }

        public void SaveDouble(string settingsKey, double value)
            => SaveString(settingsKey, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

        public IReadOnlyList<string> LoadStringArrayFromJson(string settingsKey)
        {
            var value = LoadString(settingsKey);
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }

            return System.Text.Json.JsonSerializer.Deserialize<string[]>(value) ?? Array.Empty<string>();
        }

        public void SaveStringArrayAsJson(string settingsKey, IReadOnlyList<string> values)
            => SaveString(settingsKey, System.Text.Json.JsonSerializer.Serialize(values));
    }

    private sealed class TestDirectoryBrowserViewModel : DirectoryBrowserViewModel
    {
        public TestDirectoryBrowserViewModel(IDirectoryBrowserService service, IFolderPicker folderPicker, IUserSettingsStore settings)
            : base(service, folderPicker, settings)
        {
        }

        protected override Task InvokeOnUiAsync(Action action)
        {
            action();
            return Task.CompletedTask;
        }
    }

    private sealed class StubFolderPicker : IFolderPicker
    {
        private readonly string _folder;
        public StubFolderPicker(string folder) => _folder = folder;

        public Task<string?> PickFolderAsync(CancellationToken cancellationToken)
            => Task.FromResult<string?>(_folder);
    }

    [Fact]
    public async Task WhenMotorFileIsOpen_AndUserOpensContainingFolder_FileIsHighlighted()
    {
        var store = new InMemorySettingsStore();
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-openfolder-" + Guid.NewGuid().ToString("N")));

        try
        {
            var filePath = Path.Combine(root.FullName, "motor.json");
            await File.WriteAllTextAsync(filePath, TestMotorJson("open"));

            var curveGenerator = new CurveGeneratorService();
            var fileService = new FileService(curveGenerator);
            var validationService = new ValidationService();
            var driveVoltageSeriesService = new DriveVoltageSeriesService();
            var workflow = new MotorConfigurationWorkflow(driveVoltageSeriesService);

            var vm = new MainWindowViewModel(
                fileService,
                curveGenerator,
                validationService,
                driveVoltageSeriesService,
                workflow,
                new ChartViewModel(),
                new CurveDataTableViewModel(),
                settingsStore: store);

            vm.DirectoryBrowser = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(root.FullName), store);

            // Open a motor file first (browser selection update is fine either way).
            await vm.OpenMotorFileByPathAsync(filePath);

            // Simulate user opening the containing folder afterwards.
            await vm.OpenFolderCommand.ExecuteAsync(null);

            // Selection is synchronized asynchronously when the browser root changes.
            for (var i = 0; i < 50; i++)
            {
                if (vm.DirectoryBrowser.SelectedNode?.FullPath == filePath)
                {
                    break;
                }

                await Task.Delay(10);
            }

            Assert.Equal(filePath, vm.DirectoryBrowser.SelectedNode?.FullPath);
        }
        finally
        {
            try { root.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task WhenBrowserAlreadyHasAnotherRoot_AndUserOpensContainingFolder_FileIsHighlighted()
    {
        var store = new InMemorySettingsStore();
        var root1 = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-openfolder-a-" + Guid.NewGuid().ToString("N")));
        var root2 = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-openfolder-b-" + Guid.NewGuid().ToString("N")));

        try
        {
            var filePath = Path.Combine(root2.FullName, "motor.json");
            await File.WriteAllTextAsync(filePath, TestMotorJson("open"));

            var curveGenerator = new CurveGeneratorService();
            var fileService = new FileService(curveGenerator);
            var validationService = new ValidationService();
            var driveVoltageSeriesService = new DriveVoltageSeriesService();
            var workflow = new MotorConfigurationWorkflow(driveVoltageSeriesService);

            var vm = new MainWindowViewModel(
                fileService,
                curveGenerator,
                validationService,
                driveVoltageSeriesService,
                workflow,
                new ChartViewModel(),
                new CurveDataTableViewModel(),
                settingsStore: store);

            vm.DirectoryBrowser = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(root2.FullName), store);

            // Populate browser with a different root first.
            await vm.DirectoryBrowser.SetRootDirectoryAsync(root1.FullName);

            // Open a motor file that is NOT under the current root.
            await vm.OpenMotorFileByPathAsync(filePath);

            // Now user opens a folder that contains the file.
            await vm.OpenFolderCommand.ExecuteAsync(null);

            for (var i = 0; i < 80; i++)
            {
                if (vm.DirectoryBrowser.SelectedNode?.FullPath == filePath)
                {
                    break;
                }

                await Task.Delay(10);
            }

            Assert.Equal(filePath, vm.DirectoryBrowser.SelectedNode?.FullPath);
        }
        finally
        {
            try { root1.Delete(recursive: true); } catch { }
            try { root2.Delete(recursive: true); } catch { }
        }
    }
}
