using CurveEditor.Services;
using CurveEditor.ViewModels;
using JordanRobot.MotorDefinition.Model;
using JordanRobot.MotorDefinition.Persistence.Dtos;
using MotorEditor.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public sealed class MainWindowRestoreTests
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

        public void SaveDouble(string settingsKey, double value) => SaveString(settingsKey, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

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
        {
            SaveString(settingsKey, System.Text.Json.JsonSerializer.Serialize(values));
        }
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
        public Task<string?> PickFolderAsync(System.Threading.CancellationToken cancellationToken) => Task.FromResult<string?>(null);
    }

    [Fact]
    public async Task WhenLastDirectoryMissingAndBrowserExpanded_RestoreCollapsesBrowserPanel()
    {
        var store = new InMemorySettingsStore();
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        store.SaveBool(DirectoryBrowserViewModel.WasExplicitlyClosedKey, false);
        store.SaveString(DirectoryBrowserViewModel.LastOpenedDirectoryKey, missing);
        store.SaveString(DirectoryBrowserViewModel.LastOpenedMotorFileKey, null);

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

        vm.DirectoryBrowser = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(), store);
        vm.ActiveLeftPanelId = PanelRegistry.PanelIds.DirectoryBrowser;

        await vm.RestoreSessionAfterWindowOpenedAsync();

        Assert.Equal(PanelRegistry.PanelIds.CurveData, vm.ActiveLeftPanelId);
    }

    [Fact]
    public async Task WhenMotorFileRestored_DirectoryBrowserHighlightsThatFile()
    {
        var store = new InMemorySettingsStore();
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-restore-" + Guid.NewGuid().ToString("N")));
        var filePath = Path.Combine(root.FullName, "motor.json");

        try
        {
            await File.WriteAllTextAsync(filePath, TestMotorJson("restored"));

            store.SaveBool(DirectoryBrowserViewModel.WasExplicitlyClosedKey, false);
            store.SaveString(DirectoryBrowserViewModel.LastOpenedDirectoryKey, root.FullName);
            store.SaveString(DirectoryBrowserViewModel.LastOpenedMotorFileKey, filePath);

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

            vm.DirectoryBrowser = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(), store);

            await vm.RestoreSessionAfterWindowOpenedAsync();

            Assert.Equal(filePath, vm.DirectoryBrowser.SelectedNode?.FullPath);
        }
        finally
        {
            try { root.Delete(recursive: true); } catch { }
        }
    }
}
