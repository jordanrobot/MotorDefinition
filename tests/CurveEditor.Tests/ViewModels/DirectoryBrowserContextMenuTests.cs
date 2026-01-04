using CurveEditor.Services;
using CurveEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public sealed class DirectoryBrowserContextMenuTests
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

    private sealed class TestCommand : IDirectoryBrowserCommand
    {
        public string DisplayName => "Test Command";
        public bool CanExecuteCalled { get; private set; }
        public bool ExecuteCalled { get; private set; }
        public string? LastPath { get; private set; }
        public bool LastIsDirectory { get; private set; }

        public bool CanExecute(string path, bool isDirectory)
        {
            CanExecuteCalled = true;
            return true;
        }

        public Task ExecuteAsync(string path, bool isDirectory)
        {
            ExecuteCalled = true;
            LastPath = path;
            LastIsDirectory = isDirectory;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void DirectoryBrowserViewModel_InitializesWithFileContextCommands()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        Assert.NotEmpty(vm.FileContextCommands);
        Assert.Equal(7, vm.FileContextCommands.Count);
        Assert.IsType<RevealInFileExplorerCommand>(vm.FileContextCommands[0]);
        Assert.IsType<OpenInTextEditorCommand>(vm.FileContextCommands[1]);
        Assert.IsType<CopyFileCommand>(vm.FileContextCommands[2]);
        Assert.IsType<RenameCommand>(vm.FileContextCommands[3]);
        Assert.IsType<DuplicateCommand>(vm.FileContextCommands[4]);
        Assert.IsType<PasteCommand>(vm.FileContextCommands[5]);
        Assert.IsType<DeleteCommand>(vm.FileContextCommands[6]);
    }

    [Fact]
    public void DirectoryBrowserViewModel_InitializesWithDirectoryContextCommands()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        Assert.NotEmpty(vm.DirectoryContextCommands);
        Assert.Equal(7, vm.DirectoryContextCommands.Count);
        Assert.IsType<RevealInFileExplorerCommand>(vm.DirectoryContextCommands[0]);
        Assert.IsType<OpenInTextEditorCommand>(vm.DirectoryContextCommands[1]);
        Assert.IsType<NewDirectoryCommand>(vm.DirectoryContextCommands[2]);
        Assert.IsType<RenameCommand>(vm.DirectoryContextCommands[3]);
        Assert.IsType<DuplicateCommand>(vm.DirectoryContextCommands[4]);
        Assert.IsType<PasteCommand>(vm.DirectoryContextCommands[5]);
        Assert.IsType<DeleteCommand>(vm.DirectoryContextCommands[6]);
    }

    [Fact]
    public async Task ExecuteContextCommandAsync_WithValidCommand_ExecutesCommand()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        var tempDir = Directory.CreateTempSubdirectory("context-menu-test-");
        var tempFile = Path.Combine(tempDir.FullName, "test.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, "{}");
            await vm.SetRootDirectoryAsync(tempDir.FullName);

            var rootNode = Assert.Single(vm.RootItems);
            var fileName = Path.GetFileName(tempFile);
            var fileNode = rootNode.Children.FirstOrDefault(n => n.DisplayName == fileName);
            Assert.NotNull(fileNode);

            vm.SelectedNode = fileNode;

            var testCommand = new TestCommand();
            await vm.ExecuteContextCommandCommand.ExecuteAsync(testCommand);

            Assert.True(testCommand.ExecuteCalled);
            Assert.Equal(tempFile, testCommand.LastPath);
            Assert.False(testCommand.LastIsDirectory);
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task ExecuteContextCommandAsync_WithNullCommand_DoesNotThrow()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        await vm.ExecuteContextCommandCommand.ExecuteAsync(null);
    }

    [Fact]
    public async Task ExecuteContextCommandAsync_WithNoSelectedNode_DoesNotThrow()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        var testCommand = new TestCommand();
        await vm.ExecuteContextCommandCommand.ExecuteAsync(testCommand);

        Assert.False(testCommand.ExecuteCalled);
    }
}
