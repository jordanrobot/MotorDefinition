using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CurveEditor.Services;
using Xunit;

namespace CurveEditor.Tests.Services;

public class DirectoryBrowserServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DirectoryBrowserServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Ignore cleanup errors.
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetChildrenAsync_SortsFoldersFirstThenFilesAlphabetically()
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, "b-folder"));
        Directory.CreateDirectory(Path.Combine(_tempDir, "a-folder"));

        await File.WriteAllTextAsync(Path.Combine(_tempDir, "b.json"), "{}");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "a.json"), "{}");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "ignore.txt"), "hello");

        var service = new DirectoryBrowserService();
        var children = await service.GetChildrenAsync(_tempDir, CancellationToken.None);

        var names = children.Select(c => c.Name).ToArray();

        Assert.Equal(new[] { "a-folder", "b-folder", "a.json", "b.json" }, names);
        Assert.DoesNotContain("ignore.txt", names);
    }

    [Fact]
    public async Task GetChildrenAsync_CancellationStopsWork()
    {
        var service = new DirectoryBrowserService();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetChildrenAsync(_tempDir, cts.Token));
    }
}
