using CurveEditor.Services;
using CurveEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public sealed class DirectoryBrowserRenameTests
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
    public async Task StartRenameAsync_SelectedNode_SetsRenamingAndOriginalName()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        var tempDir = Directory.CreateTempSubdirectory("rename-start-");
        var tempFile = Path.Combine(tempDir.FullName, "sample.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, "{}");
            await vm.SetRootDirectoryAsync(tempDir.FullName);

            var root = Assert.Single(vm.RootItems);
            var node = Assert.Single(root.Children, n => !n.IsDirectory);

            vm.SelectedNode = node;
            await vm.StartRenameAsync();

            Assert.True(node.IsRenaming);
            Assert.Equal("sample.json", node.OriginalDisplayName);
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task CancelRenameAsync_WhenNameEdited_RestoresOriginalName()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        var tempDir = Directory.CreateTempSubdirectory("rename-cancel-");
        var tempFile = Path.Combine(tempDir.FullName, "cancel.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, "{}");
            await vm.SetRootDirectoryAsync(tempDir.FullName);

            var root = Assert.Single(vm.RootItems);
            var node = Assert.Single(root.Children, n => !n.IsDirectory);

            vm.SelectedNode = node;
            await vm.StartRenameAsync();

            node.DisplayName = "renaming.json";
            await vm.CancelRenameAsync(node);

            Assert.False(node.IsRenaming);
            Assert.Equal("cancel.json", node.DisplayName);
            Assert.Null(node.OriginalDisplayName);
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task CompleteRenameAsync_WithNewName_RenamesFileAndClearsState()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        var tempDir = Directory.CreateTempSubdirectory("rename-complete-");
        var tempFile = Path.Combine(tempDir.FullName, "complete.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, "{}");
            await vm.SetRootDirectoryAsync(tempDir.FullName);

            var root = Assert.Single(vm.RootItems);
            var node = Assert.Single(root.Children, n => !n.IsDirectory);

            vm.SelectedNode = node;
            await vm.StartRenameAsync();

            var success = await vm.CompleteRenameAsync(node, "renamed.json");

            Assert.True(success);
            Assert.False(File.Exists(tempFile));
            Assert.True(File.Exists(Path.Combine(tempDir.FullName, "renamed.json")));
            Assert.False(node.IsRenaming);
            Assert.Equal("renamed.json", node.DisplayName);
            Assert.Null(node.OriginalDisplayName);
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }
}
