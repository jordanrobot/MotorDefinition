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

/// <summary>
/// Integration tests verifying the complete context menu flow for the directory browser.
/// </summary>
public sealed class DirectoryBrowserContextMenuIntegrationTests
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
    public async Task FileContextMenu_HasRevealInFileExplorerCommand()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        var tempDir = Directory.CreateTempSubdirectory("integration-test-");
        var testFile = Path.Combine(tempDir.FullName, "test.json");
        try
        {
            await File.WriteAllTextAsync(testFile, "{}");
            await vm.SetRootDirectoryAsync(tempDir.FullName);

            // Verify file context commands are available
            Assert.NotEmpty(vm.FileContextCommands);
            var revealCommand = Assert.Single(vm.FileContextCommands, c => c.DisplayName == "Reveal in File Explorer");
            Assert.NotNull(revealCommand);

            // Verify the command can execute on the test file
            Assert.True(revealCommand.CanExecute(testFile, isDirectory: false));
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task DirectoryContextMenu_HasRevealInFileExplorerCommand()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        var tempDir = Directory.CreateTempSubdirectory("integration-test-");
        var subDir = Directory.CreateDirectory(Path.Combine(tempDir.FullName, "subdir"));
        try
        {
            await vm.SetRootDirectoryAsync(tempDir.FullName);

            // Verify directory context commands are available
            Assert.NotEmpty(vm.DirectoryContextCommands);
            var revealCommand = Assert.Single(vm.DirectoryContextCommands, c => c.DisplayName == "Reveal in File Explorer");
            Assert.NotNull(revealCommand);

            // Verify the command can execute on the subdirectory
            Assert.True(revealCommand.CanExecute(subDir.FullName, isDirectory: true));
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task ContextMenu_FileAndDirectoryCommandsAreShared()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        // File commands: RevealInFileExplorer, OpenInTextEditor, CopyFile, Duplicate, Delete
        Assert.Equal(5, vm.FileContextCommands.Count);
        
        // Directory commands: RevealInFileExplorer, OpenInTextEditor, NewDirectory, Duplicate, Delete
        Assert.Equal(5, vm.DirectoryContextCommands.Count);
        
        // Verify the shared commands are the same instances
        Assert.Same(vm.FileContextCommands[0], vm.DirectoryContextCommands[0]); // RevealInFileExplorer
        Assert.Same(vm.FileContextCommands[1], vm.DirectoryContextCommands[1]); // OpenInTextEditor
        Assert.Same(vm.FileContextCommands[3], vm.DirectoryContextCommands[3]); // Duplicate
        Assert.Same(vm.FileContextCommands[4], vm.DirectoryContextCommands[4]); // Delete
    }

    [Fact]
    public async Task SelectedNode_CanExecuteContextCommand_ForFile()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        var tempDir = Directory.CreateTempSubdirectory("integration-test-");
        var testFile = Path.Combine(tempDir.FullName, "test.json");
        try
        {
            await File.WriteAllTextAsync(testFile, "{}");
            await vm.SetRootDirectoryAsync(tempDir.FullName);

            var rootNode = Assert.Single(vm.RootItems);
            var fileNode = Assert.Single(rootNode.Children, n => !n.IsDirectory && n.DisplayName == "test.json");
            
            vm.SelectedNode = fileNode;

            // Verify the ExecuteContextCommand can be executed
            var command = vm.FileContextCommands[0];
            Assert.True(command.CanExecute(fileNode.FullPath, fileNode.IsDirectory));
            
            // Execute should not throw
            await vm.ExecuteContextCommandCommand.ExecuteAsync(command);
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task SelectedNode_CanExecuteContextCommand_ForDirectory()
    {
        var vm = new TestDirectoryBrowserViewModel(
            new DirectoryBrowserService(),
            new StubFolderPicker(),
            new InMemorySettingsStore());

        var tempDir = Directory.CreateTempSubdirectory("integration-test-");
        var subDir = Directory.CreateDirectory(Path.Combine(tempDir.FullName, "subdir"));
        try
        {
            await vm.SetRootDirectoryAsync(tempDir.FullName);

            var rootNode = Assert.Single(vm.RootItems);
            var dirNode = Assert.Single(rootNode.Children, n => n.IsDirectory && n.DisplayName == "subdir");
            
            vm.SelectedNode = dirNode;

            // Verify the ExecuteContextCommand can be executed
            var command = vm.DirectoryContextCommands[0];
            Assert.True(command.CanExecute(dirNode.FullPath, dirNode.IsDirectory));
            
            // Execute should not throw
            await vm.ExecuteContextCommandCommand.ExecuteAsync(command);
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }
}
