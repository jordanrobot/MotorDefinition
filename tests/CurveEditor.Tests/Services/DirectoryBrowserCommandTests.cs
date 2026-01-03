using CurveEditor.Services;
using System.IO;
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
}
