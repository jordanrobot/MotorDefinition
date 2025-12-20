using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurveEditor.Services;
using CurveEditor.ViewModels;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public sealed class DirectoryBrowserRestoreTests
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
    public async Task WhenLastDirectoryMissing_RestoreReturnsMissingDirectory()
    {
        var store = new InMemorySettingsStore();
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        store.SaveBool(DirectoryBrowserViewModel.WasExplicitlyClosedKey, false);
        store.SaveString(DirectoryBrowserViewModel.LastOpenedDirectoryKey, missing);

        var vm = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(), store);
        var result = await vm.TryRestoreSessionAsync();

        Assert.Equal(DirectoryBrowserViewModel.RestoreResult.MissingDirectory, result);
    }

    [Fact]
    public async Task WhenLastDirectoryExists_RestoreRecreatesRootNode()
    {
        var store = new InMemorySettingsStore();
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-test-" + Guid.NewGuid().ToString("N")));
        try
        {
            File.WriteAllText(Path.Combine(root.FullName, "a.json"), "{}");

            store.SaveBool(DirectoryBrowserViewModel.WasExplicitlyClosedKey, false);
            store.SaveString(DirectoryBrowserViewModel.LastOpenedDirectoryKey, root.FullName);
            store.SaveStringArrayAsJson(DirectoryBrowserViewModel.ExpandedDirectoryPathsKey, Array.Empty<string>());

            var vm = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(), store);
            var result = await vm.TryRestoreSessionAsync();

            Assert.Equal(DirectoryBrowserViewModel.RestoreResult.Restored, result);
            Assert.Single(vm.RootItems);
            Assert.True(vm.RootItems[0].IsRoot);
            Assert.True(vm.RootItems[0].IsExpanded);
        }
        finally
        {
            try { root.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task WhenExpandedPathsPersisted_RestoreExpandsDirectories()
    {
        var store = new InMemorySettingsStore();
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "curve-test-" + Guid.NewGuid().ToString("N")));
        var subdir = Directory.CreateDirectory(Path.Combine(root.FullName, "subdir"));

        try
        {
            File.WriteAllText(Path.Combine(subdir.FullName, "a.json"), "{}");

            store.SaveBool(DirectoryBrowserViewModel.WasExplicitlyClosedKey, false);
            store.SaveString(DirectoryBrowserViewModel.LastOpenedDirectoryKey, root.FullName);
            store.SaveStringArrayAsJson(DirectoryBrowserViewModel.ExpandedDirectoryPathsKey, new[] { "subdir" });

            var vm = new TestDirectoryBrowserViewModel(new DirectoryBrowserService(), new StubFolderPicker(), store);
            var result = await vm.TryRestoreSessionAsync();

            Assert.Equal(DirectoryBrowserViewModel.RestoreResult.Restored, result);
            var rootNode = Assert.Single(vm.RootItems);
            var expandedNode = Assert.Single(rootNode.Children, child => child.DisplayName == "subdir");
            Assert.True(expandedNode.IsExpanded);
        }
        finally
        {
            try { root.Delete(recursive: true); } catch { }
        }
    }
}
