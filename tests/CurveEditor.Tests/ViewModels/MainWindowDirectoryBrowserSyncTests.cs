using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CurveEditor.Services;
using CurveEditor.ViewModels;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public sealed class MainWindowDirectoryBrowserSyncTests
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

    [Fact]
    public async Task WhenCurrentFilePathChanges_ExplorerSelectionIsSynced()
    {
        var store = new InMemorySettingsStore();

        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-test-" + Guid.NewGuid().ToString("N")));
        try
        {
            var filePath = Path.Combine(root.FullName, "a.json");
            await File.WriteAllTextAsync(filePath, "{}");

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

            await vm.DirectoryBrowser.SetRootDirectoryAsync(root.FullName);

            vm.CurrentFilePath = filePath;

            // Sync is fire-and-forget; wait until selection lands.
            var deadline = DateTime.UtcNow.AddSeconds(2);
            while (DateTime.UtcNow < deadline)
            {
                if (vm.DirectoryBrowser.SelectedNode?.FullPath == filePath)
                {
                    return;
                }

                await Task.Delay(25);
            }

            Assert.Equal(filePath, vm.DirectoryBrowser.SelectedNode?.FullPath);
        }
        finally
        {
            try { root.Delete(recursive: true); } catch { }
        }
    }
}
