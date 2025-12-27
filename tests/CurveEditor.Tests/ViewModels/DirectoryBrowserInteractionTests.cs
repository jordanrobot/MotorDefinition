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

public sealed class DirectoryBrowserInteractionTests
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
    public async Task WhenSelectingFileNode_FileOpenRequestedIsRaisedWithPath()
    {
        var store = new InMemorySettingsStore();
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-test-" + Guid.NewGuid().ToString("N")));
        try
        {
            var filePath = Path.Combine(root.FullName, "a.json");
            await File.WriteAllTextAsync(filePath, TestMotorJson("a"));

            var vm = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(), store);
            await vm.SetRootDirectoryAsync(root.FullName);

            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            vm.FileOpenRequested += path =>
            {
                tcs.TrySetResult(path);
                return Task.CompletedTask;
            };

            var rootNode = Assert.Single(vm.RootItems);
            var fileNode = Assert.Single(rootNode.Children, n => !n.IsDirectory && n.DisplayName == "a.json");

            vm.SelectedNode = fileNode;

            var opened = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(2));
            Assert.Equal(filePath, opened);
        }
        finally
        {
            try { root.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task SyncSelectionToFilePathAsync_WhenFileUnderRoot_SelectsMatchingNodeWithoutThrowing()
    {
        var store = new InMemorySettingsStore();
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-test-" + Guid.NewGuid().ToString("N")));
        try
        {
            var filePath = Path.Combine(root.FullName, "a.json");
            await File.WriteAllTextAsync(filePath, TestMotorJson("a"));

            var vm = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(), store);
            await vm.SetRootDirectoryAsync(root.FullName);

            await vm.SyncSelectionToFilePathAsync(filePath);

            Assert.NotNull(vm.SelectedNode);
            Assert.False(vm.SelectedNode!.IsDirectory);
            Assert.Equal(filePath, vm.SelectedNode.FullPath);
        }
        finally
        {
            try { root.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task FontSizeCommands_ClampAndPersist()
    {
        var store = new InMemorySettingsStore();
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-test-" + Guid.NewGuid().ToString("N")));
        try
        {
            var vm = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(), store);
            await vm.SetRootDirectoryAsync(root.FullName);

            for (var i = 0; i < 200; i++)
            {
                vm.IncreaseFontSizeCommand.Execute(null);
            }

            Assert.Equal(DirectoryBrowserViewModel.MaxFontSize, vm.FontSize);

            for (var i = 0; i < 200; i++)
            {
                vm.DecreaseFontSizeCommand.Execute(null);
            }

            Assert.Equal(DirectoryBrowserViewModel.MinFontSize, vm.FontSize);

            // Debounced persistence.
            await Task.Delay(350);

            var persisted = store.LoadDouble(DirectoryBrowserViewModel.FontSizeKey, defaultValue: -1);
            Assert.Equal(vm.FontSize, persisted);
        }
        finally
        {
            try { root.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task TreeStructure_RootShowsRootFilesAndDirectories_AndUnexpandedDirectoryDoesNotShowItsChildren()
    {
        var store = new InMemorySettingsStore();
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-test-" + Guid.NewGuid().ToString("N")));

        try
        {
            var motorProfilesDir = Directory.CreateDirectory(Path.Combine(root.FullName, "motor profiles"));
            var dir4 = Directory.CreateDirectory(Path.Combine(root.FullName, "directory 4"));

            var fileInMotorProfiles1 = Path.Combine(motorProfilesDir.FullName, "motor profile 1.json");
            var fileInMotorProfiles2 = Path.Combine(motorProfilesDir.FullName, "motor profile 2.json");
            var fileAtRoot3 = Path.Combine(root.FullName, "motor profile 3.json");
            var fileAtRoot4 = Path.Combine(root.FullName, "motor profile 4.json");

            await File.WriteAllTextAsync(fileInMotorProfiles1, TestMotorJson("motor profile 1"));
            await File.WriteAllTextAsync(fileInMotorProfiles2, TestMotorJson("motor profile 2"));
            await File.WriteAllTextAsync(fileAtRoot3, TestMotorJson("motor profile 3"));
            await File.WriteAllTextAsync(fileAtRoot4, TestMotorJson("motor profile 4"));

            var vm = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(), store);
            await vm.SetRootDirectoryAsync(root.FullName);

            var rootNode = Assert.Single(vm.RootItems);

            Assert.Contains(rootNode.Children, n => n.IsDirectory && n.DisplayName == "motor profiles");
            Assert.Contains(rootNode.Children, n => n.IsDirectory && n.DisplayName == "directory 4");

            Assert.Contains(rootNode.Children, n => !n.IsDirectory && n.DisplayName == "motor profile 3.json");
            Assert.Contains(rootNode.Children, n => !n.IsDirectory && n.DisplayName == "motor profile 4.json");

            var motorProfilesNode = Assert.Single(rootNode.Children, n => n.IsDirectory && n.DisplayName == "motor profiles");
            var dir4Node = Assert.Single(rootNode.Children, n => n.IsDirectory && n.DisplayName == "directory 4");

            // Directory children are loaded lazily; until expanded they should not show their contents.
            Assert.Empty(motorProfilesNode.Children.Where(c => !c.IsPlaceholder));
            Assert.Empty(dir4Node.Children.Where(c => !c.IsPlaceholder));
        }
        finally
        {
            try { root.Delete(recursive: true); } catch { }
        }
    }
}
