using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Represents a command that can be executed on a file or directory in the directory browser.
/// </summary>
public interface IDirectoryBrowserCommand
{
    /// <summary>
    /// Gets the display name of the command shown in the context menu.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Determines if this command can execute on the specified path.
    /// </summary>
    /// <param name="path">The full path to the file or directory.</param>
    /// <param name="isDirectory">True if the path is a directory, false if it's a file.</param>
    /// <returns>True if the command can execute, false otherwise.</returns>
    bool CanExecute(string path, bool isDirectory);

    /// <summary>
    /// Executes the command on the specified path.
    /// </summary>
    /// <param name="path">The full path to the file or directory.</param>
    /// <param name="isDirectory">True if the path is a directory, false if it's a file.</param>
    Task ExecuteAsync(string path, bool isDirectory);
}
