using System.Threading;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Abstraction for choosing a folder, so folder selection can be unit-tested.
/// </summary>
public interface IFolderPicker
{
    /// <summary>
    /// Prompts the user to pick a folder and returns its local path, or <c>null</c> if cancelled.
    /// </summary>
    Task<string?> PickFolderAsync(CancellationToken cancellationToken);
}
