using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurveEditor.Services;
using CurveEditor.ViewModels;
using MotorEditor.Avalonia.Models;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public sealed class MainWindowRestoreTests
{
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
}
