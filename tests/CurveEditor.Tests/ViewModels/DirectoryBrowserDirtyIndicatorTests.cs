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

public sealed class DirectoryBrowserDirtyIndicatorTests
{
    private static string TestMotorJson(string motorName)
    {
        var percent = Enumerable.Range(0, 101).ToArray();
        var rpm = percent.Select(p => (double)p).ToArray();

        var dto = new MotorDefinitionFileDto
        {
            SchemaVersion = ServoMotor.CurrentSchemaVersion,
            MotorName = motorName,
            Drives =
            [
                new DriveFileDto
                {
                    Manufacturer = string.Empty,
                    PartNumber = string.Empty,
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
        public Task<string?> PickFolderAsync(CancellationToken cancellationToken) => Task.FromResult<string?>(null);
    }

    [Fact]
    public async Task UpdateOpenFileStates_WhenAnyFileIsDirty_AppendsStarRegardlessOfActiveFile()
    {
        var store = new InMemorySettingsStore();
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-test-" + Guid.NewGuid().ToString("N")));
        try
        {
            var fileA = Path.Combine(root.FullName, "a.json");
            var fileB = Path.Combine(root.FullName, "b.json");
            await File.WriteAllTextAsync(fileA, TestMotorJson("a"));
            await File.WriteAllTextAsync(fileB, TestMotorJson("b"));

            var vm = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(), store);
            await vm.SetRootDirectoryAsync(root.FullName);

            vm.UpdateOpenFileStates(activeFilePath: fileA, dirtyFilePaths: new[] { fileA });

            var rootNode = Assert.Single(vm.RootItems);
            var fileNodeA = Assert.Single(rootNode.Children, n => !n.IsDirectory && n.DisplayName == "a.json");
            var fileNodeB = Assert.Single(rootNode.Children, n => !n.IsDirectory && n.DisplayName == "b.json");

            Assert.Equal("a.json*", fileNodeA.DisplayNameWithDirtyIndicator);
            Assert.Equal("b.json", fileNodeB.DisplayNameWithDirtyIndicator);

            // Dirty marker should persist even if a different file is active.
            vm.UpdateOpenFileStates(activeFilePath: fileB, dirtyFilePaths: new[] { fileA });
            Assert.Equal("a.json*", fileNodeA.DisplayNameWithDirtyIndicator);
            Assert.Equal("b.json", fileNodeB.DisplayNameWithDirtyIndicator);

            // Multiple dirty files should each show a star.
            vm.UpdateOpenFileStates(activeFilePath: fileB, dirtyFilePaths: new[] { fileA, fileB });
            Assert.Equal("a.json*", fileNodeA.DisplayNameWithDirtyIndicator);
            Assert.Equal("b.json*", fileNodeB.DisplayNameWithDirtyIndicator);

            // Clearing dirty state should remove the star.
            vm.UpdateOpenFileStates(activeFilePath: fileB, dirtyFilePaths: Array.Empty<string>());
            Assert.Equal("a.json", fileNodeA.DisplayNameWithDirtyIndicator);
            Assert.Equal("b.json", fileNodeB.DisplayNameWithDirtyIndicator);
        }
        finally
        {
            try { root.Delete(recursive: true); } catch { }
        }
    }
}
