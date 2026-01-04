using CurveEditor.Services;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace CurveEditor.Tests.Services;

public sealed class DirectoryBrowserCommandTests
{
    [Fact]
    public void RevealInFileExplorerCommand_HasCorrectDisplayName()
    {
        var command = new RevealInFileExplorerCommand();
        Assert.Equal("Reveal in File Explorer", command.DisplayName);
    }

    [Fact]
    public void RevealInFileExplorerCommand_CanExecute_ReturnsFalseForNullPath()
    {
        var command = new RevealInFileExplorerCommand();
        Assert.False(command.CanExecute(null!, isDirectory: false));
        Assert.False(command.CanExecute(string.Empty, isDirectory: false));
        Assert.False(command.CanExecute("   ", isDirectory: false));
    }

    [Fact]
    public void RevealInFileExplorerCommand_CanExecute_ReturnsFalseForNonExistentFile()
    {
        var command = new RevealInFileExplorerCommand();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent-" + System.Guid.NewGuid() + ".txt");
        Assert.False(command.CanExecute(nonExistentPath, isDirectory: false));
    }

    [Fact]
    public void RevealInFileExplorerCommand_CanExecute_ReturnsTrueForExistingFile()
    {
        var command = new RevealInFileExplorerCommand();
        var tempFile = Path.GetTempFileName();
        try
        {
            Assert.True(command.CanExecute(tempFile, isDirectory: false));
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public void RevealInFileExplorerCommand_CanExecute_ReturnsFalseForNonExistentDirectory()
    {
        var command = new RevealInFileExplorerCommand();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent-" + System.Guid.NewGuid());
        Assert.False(command.CanExecute(nonExistentPath, isDirectory: true));
    }

    [Fact]
    public void RevealInFileExplorerCommand_CanExecute_ReturnsTrueForExistingDirectory()
    {
        var command = new RevealInFileExplorerCommand();
        var tempDir = Directory.CreateTempSubdirectory("reveal-test-");
        try
        {
            Assert.True(command.CanExecute(tempDir.FullName, isDirectory: true));
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    // OpenInTextEditorCommand Tests

    [Fact]
    public void OpenInTextEditorCommand_HasCorrectDisplayName()
    {
        var command = new OpenInTextEditorCommand();
        Assert.Equal("Open in Text Editor", command.DisplayName);
    }

    [Fact]
    public void OpenInTextEditorCommand_CanExecute_ReturnsFalseForNullPath()
    {
        var command = new OpenInTextEditorCommand();
        Assert.False(command.CanExecute(null!, isDirectory: false));
        Assert.False(command.CanExecute(string.Empty, isDirectory: false));
        Assert.False(command.CanExecute("   ", isDirectory: false));
    }

    [Fact]
    public void OpenInTextEditorCommand_CanExecute_ReturnsFalseForNonExistentFile()
    {
        var command = new OpenInTextEditorCommand();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent-" + System.Guid.NewGuid() + ".txt");
        Assert.False(command.CanExecute(nonExistentPath, isDirectory: false));
    }

    [Fact]
    public void OpenInTextEditorCommand_CanExecute_ReturnsTrueForExistingFile()
    {
        var command = new OpenInTextEditorCommand();
        var tempFile = Path.GetTempFileName();
        try
        {
            Assert.True(command.CanExecute(tempFile, isDirectory: false));
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    // CopyFileCommand Tests

    [Fact]
    public void CopyFileCommand_HasCorrectDisplayName()
    {
        var command = new CopyFileCommand();
        Assert.Equal("Copy File", command.DisplayName);
    }

    [Fact]
    public void CopyFileCommand_CanExecute_ReturnsFalseForNullPath()
    {
        var command = new CopyFileCommand();
        Assert.False(command.CanExecute(null!, isDirectory: false));
        Assert.False(command.CanExecute(string.Empty, isDirectory: false));
        Assert.False(command.CanExecute("   ", isDirectory: false));
    }

    [Fact]
    public void CopyFileCommand_CanExecute_ReturnsTrueForDirectory()
    {
        var command = new CopyFileCommand();
        var tempDir = Directory.CreateTempSubdirectory("copy-test-");
        try
        {
            Assert.True(command.CanExecute(tempDir.FullName, isDirectory: true));
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public void CopyFileCommand_CanExecute_ReturnsTrueForExistingFile()
    {
        var command = new CopyFileCommand();
        var tempFile = Path.GetTempFileName();
        try
        {
            Assert.True(command.CanExecute(tempFile, isDirectory: false));
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    // DuplicateCommand Tests

    [Fact]
    public void DuplicateCommand_HasCorrectDisplayName()
    {
        var command = new DuplicateCommand();
        Assert.Equal("Duplicate", command.DisplayName);
    }

    [Fact]
    public void DuplicateCommand_CanExecute_ReturnsFalseForNullPath()
    {
        var command = new DuplicateCommand();
        Assert.False(command.CanExecute(null!, isDirectory: false));
        Assert.False(command.CanExecute(string.Empty, isDirectory: false));
        Assert.False(command.CanExecute("   ", isDirectory: false));
    }

    [Fact]
    public void DuplicateCommand_CanExecute_ReturnsTrueForExistingFile()
    {
        var command = new DuplicateCommand();
        var tempFile = Path.GetTempFileName();
        try
        {
            Assert.True(command.CanExecute(tempFile, isDirectory: false));
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public void DuplicateCommand_CanExecute_ReturnsTrueForExistingDirectory()
    {
        var command = new DuplicateCommand();
        var tempDir = Directory.CreateTempSubdirectory("duplicate-test-");
        try
        {
            Assert.True(command.CanExecute(tempDir.FullName, isDirectory: true));
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task DuplicateCommand_ExecuteAsync_CreatesFileCopy()
    {
        var command = new DuplicateCommand();
        var tempDir = Directory.CreateTempSubdirectory("duplicate-file-test-");
        try
        {
            var sourceFile = Path.Combine(tempDir.FullName, "test.txt");
            await File.WriteAllTextAsync(sourceFile, "test content");

            await command.ExecuteAsync(sourceFile, isDirectory: false);

            var expectedCopy = Path.Combine(tempDir.FullName, "test_copy.txt");
            Assert.True(File.Exists(expectedCopy));
            Assert.Equal("test content", await File.ReadAllTextAsync(expectedCopy));
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task DuplicateCommand_ExecuteAsync_CreatesDirectoryCopy()
    {
        var command = new DuplicateCommand();
        var tempDir = Directory.CreateTempSubdirectory("duplicate-dir-test-");
        try
        {
            var sourceDir = Path.Combine(tempDir.FullName, "source");
            Directory.CreateDirectory(sourceDir);
            var sourceFile = Path.Combine(sourceDir, "file.txt");
            await File.WriteAllTextAsync(sourceFile, "content");

            await command.ExecuteAsync(sourceDir, isDirectory: true);

            var expectedCopy = Path.Combine(tempDir.FullName, "source_copy");
            Assert.True(Directory.Exists(expectedCopy));
            var copiedFile = Path.Combine(expectedCopy, "file.txt");
            Assert.True(File.Exists(copiedFile));
            Assert.Equal("content", await File.ReadAllTextAsync(copiedFile));
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    // DeleteCommand Tests

    [Fact]
    public void DeleteCommand_HasCorrectDisplayName()
    {
        var command = new DeleteCommand();
        Assert.Equal("Delete", command.DisplayName);
    }

    [Fact]
    public void DeleteCommand_CanExecute_ReturnsFalseForNullPath()
    {
        var command = new DeleteCommand();
        Assert.False(command.CanExecute(null!, isDirectory: false));
        Assert.False(command.CanExecute(string.Empty, isDirectory: false));
        Assert.False(command.CanExecute("   ", isDirectory: false));
    }

    [Fact]
    public void DeleteCommand_CanExecute_ReturnsTrueForExistingFile()
    {
        var command = new DeleteCommand();
        var tempFile = Path.GetTempFileName();
        try
        {
            Assert.True(command.CanExecute(tempFile, isDirectory: false));
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public void DeleteCommand_CanExecute_ReturnsTrueForExistingDirectory()
    {
        var command = new DeleteCommand();
        var tempDir = Directory.CreateTempSubdirectory("delete-test-");
        try
        {
            Assert.True(command.CanExecute(tempDir.FullName, isDirectory: true));
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task DeleteCommand_ExecuteAsync_DeletesFile()
    {
        var command = new DeleteCommand();
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "test");

        await command.ExecuteAsync(tempFile, isDirectory: false);

        Assert.False(File.Exists(tempFile));
    }

    [Fact]
    public async Task DeleteCommand_ExecuteAsync_DeletesDirectory()
    {
        var command = new DeleteCommand();
        var tempDir = Directory.CreateTempSubdirectory("delete-dir-test-");
        var subFile = Path.Combine(tempDir.FullName, "file.txt");
        await File.WriteAllTextAsync(subFile, "content");

        await command.ExecuteAsync(tempDir.FullName, isDirectory: true);

        Assert.False(Directory.Exists(tempDir.FullName));
    }

    // NewDirectoryCommand Tests

    [Fact]
    public void NewDirectoryCommand_HasCorrectDisplayName()
    {
        var command = new NewDirectoryCommand();
        Assert.Equal("New Directory", command.DisplayName);
    }

    [Fact]
    public void NewDirectoryCommand_CanExecute_ReturnsFalseForNullPath()
    {
        var command = new NewDirectoryCommand();
        Assert.False(command.CanExecute(null!, isDirectory: true));
        Assert.False(command.CanExecute(string.Empty, isDirectory: true));
        Assert.False(command.CanExecute("   ", isDirectory: true));
    }

    [Fact]
    public void NewDirectoryCommand_CanExecute_ReturnsFalseForFiles()
    {
        var command = new NewDirectoryCommand();
        var tempFile = Path.GetTempFileName();
        try
        {
            Assert.False(command.CanExecute(tempFile, isDirectory: false));
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public void NewDirectoryCommand_CanExecute_ReturnsTrueForExistingDirectory()
    {
        var command = new NewDirectoryCommand();
        var tempDir = Directory.CreateTempSubdirectory("newdir-test-");
        try
        {
            Assert.True(command.CanExecute(tempDir.FullName, isDirectory: true));
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task NewDirectoryCommand_ExecuteAsync_CreatesNewDirectory()
    {
        var command = new NewDirectoryCommand();
        var tempDir = Directory.CreateTempSubdirectory("newdir-create-test-");
        try
        {
            await command.ExecuteAsync(tempDir.FullName, isDirectory: true);

            var expectedNewDir = Path.Combine(tempDir.FullName, "NewFolder");
            Assert.True(Directory.Exists(expectedNewDir));
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task NewDirectoryCommand_ExecuteAsync_CreatesNumberedDirectoryWhenNameExists()
    {
        var command = new NewDirectoryCommand();
        var tempDir = Directory.CreateTempSubdirectory("newdir-numbered-test-");
        try
        {
            // Create existing directory
            var existingDir = Path.Combine(tempDir.FullName, "NewFolder");
            Directory.CreateDirectory(existingDir);

            await command.ExecuteAsync(tempDir.FullName, isDirectory: true);

            var expectedNewDir = Path.Combine(tempDir.FullName, "NewFolder1");
            Assert.True(Directory.Exists(expectedNewDir));
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    // Command behavior tests

    [Fact]
    public void DeleteCommand_RequiresConfirmation()
    {
        var command = new DeleteCommand();
        Assert.True(command.RequiresConfirmation);
    }

    [Fact]
    public void DeleteCommand_RequiresRefresh()
    {
        var command = new DeleteCommand();
        Assert.True(command.RequiresRefresh);
    }

    [Fact]
    public void DeleteCommand_GetConfirmationMessage_ForFile()
    {
        var command = new DeleteCommand();
        var tempFile = Path.GetTempFileName();
        try
        {
            var message = command.GetConfirmationMessage(tempFile, isDirectory: false);
            Assert.Contains("file", message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(Path.GetFileName(tempFile), message);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public void DeleteCommand_GetConfirmationMessage_ForDirectory()
    {
        var command = new DeleteCommand();
        var tempDir = Directory.CreateTempSubdirectory("delete-msg-test-");
        try
        {
            var message = command.GetConfirmationMessage(tempDir.FullName, isDirectory: true);
            Assert.Contains("directory", message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("All contents will be deleted", message);
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    [Fact]
    public void DuplicateCommand_RequiresRefresh()
    {
        var command = new DuplicateCommand();
        Assert.True(command.RequiresRefresh);
    }

    [Fact]
    public void NewDirectoryCommand_RequiresRefresh()
    {
        var command = new NewDirectoryCommand();
        Assert.True(command.RequiresRefresh);
    }
}

