using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurveEditor.Services;
using JordanRobot.MotorDefinition;
using JordanRobot.MotorDefinition.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CurveEditor.ViewModels;

/// <summary>
/// ViewModel for the Directory Browser explorer panel (Phase 3.1).
/// </summary>
public partial class DirectoryBrowserViewModel : ObservableObject
{
    public const string LastOpenedDirectoryKey = "DirectoryBrowser.LastOpenedDirectory";
    public const string WasExplicitlyClosedKey = "DirectoryBrowser.WasExplicitlyClosed";
    public const string ExpandedDirectoryPathsKey = "DirectoryBrowser.ExpandedDirectoryPaths";
    public const string SelectedPathKey = "DirectoryBrowser.SelectedPath";
    public const string FontSizeKey = "DirectoryBrowser.FontSize";

    public const string LastOpenedMotorFileKey = "File.LastOpenedMotorFile";

    public const double DefaultFontSize = 12;
    public const double MinFontSize = 8;
    public const double MaxFontSize = 24;

    [ObservableProperty]
    private string? _rootDirectoryPath;

    public string? RootDirectoryDisplayName
        => string.IsNullOrWhiteSpace(RootDirectoryPath) ? null : GetRootDisplayName(RootDirectoryPath);

    [ObservableProperty]
    private ExplorerNodeViewModel? _selectedNode;

    [ObservableProperty]
    private double _fontSize = DefaultFontSize;

    private string? _activeFileRelativePath;
    private HashSet<string> _dirtyFileRelativePaths = new(StringComparer.OrdinalIgnoreCase);

    private readonly IDirectoryBrowserService _directoryBrowserService;
    private readonly IFolderPicker _folderPicker;
    private readonly IUserSettingsStore _settings;
    private CancellationTokenSource? _refreshCts;

    private readonly HashSet<string> _expandedDirectoryPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _expandedDirectoryPathsGate = new();
    private string? _selectedRelativePath;

    private CancellationTokenSource? _persistExpandedCts;
    private CancellationTokenSource? _persistFontSizeCts;
    private CancellationTokenSource? _persistSelectedPathCts;

    private bool _isRestoring;
    private bool _suppressFileOpenOnSelection;

    /// <summary>
    /// Raised when a file node is selected in the explorer and should be opened.
    /// </summary>
    public event Func<string, Task>? FileOpenRequested;

    /// <summary>
    /// Callback to request confirmation from the user. Returns true if confirmed, false if cancelled.
    /// </summary>
    public Func<string, string, Task<bool>>? RequestConfirmation { get; set; }

    public enum RestoreResult
    {
        NoSession,
        ExplicitlyClosed,
        Restored,
        MissingDirectory
    }

    public DirectoryBrowserViewModel()
        : this(new DirectoryBrowserService(), new StorageProviderFolderPicker(), new PanelLayoutUserSettingsStore())
    {
    }

    public DirectoryBrowserViewModel(IDirectoryBrowserService directoryBrowserService, IFolderPicker folderPicker)
        : this(directoryBrowserService, folderPicker, new PanelLayoutUserSettingsStore())
    {
    }

    public DirectoryBrowserViewModel(IDirectoryBrowserService directoryBrowserService, IFolderPicker folderPicker, IUserSettingsStore settings)
    {
        _directoryBrowserService = directoryBrowserService ?? throw new ArgumentNullException(nameof(directoryBrowserService));
        _folderPicker = folderPicker ?? throw new ArgumentNullException(nameof(folderPicker));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        var persistedFontSize = _settings.LoadDouble(FontSizeKey, DefaultFontSize);
        FontSize = Math.Clamp(persistedFontSize, MinFontSize, MaxFontSize);

        InitializeContextMenuCommands();
    }

    public ObservableCollection<ExplorerNodeViewModel> RootItems { get; } = [];

    public ObservableCollection<IDirectoryBrowserCommand> FileContextCommands { get; } = [];
    public ObservableCollection<IDirectoryBrowserCommand> DirectoryContextCommands { get; } = [];

    private void InitializeContextMenuCommands()
    {
        var revealCommand = new RevealInFileExplorerCommand();
        var openInEditorCommand = new OpenInTextEditorCommand();
        var renameCommand = new RenameCommand();
        var duplicateCommand = new DuplicateCommand();
        var deleteCommand = new DeleteCommand();
        var pasteCommand = new PasteCommand();

        // File context menu commands
        FileContextCommands.Add(revealCommand);
        FileContextCommands.Add(openInEditorCommand);
        FileContextCommands.Add(new CopyFileCommand());
        FileContextCommands.Add(renameCommand);
        FileContextCommands.Add(duplicateCommand);
        FileContextCommands.Add(pasteCommand);
        FileContextCommands.Add(deleteCommand);

        // Directory context menu commands
        DirectoryContextCommands.Add(revealCommand);
        DirectoryContextCommands.Add(openInEditorCommand);
        DirectoryContextCommands.Add(new NewDirectoryCommand());
        DirectoryContextCommands.Add(renameCommand);
        DirectoryContextCommands.Add(duplicateCommand);
        DirectoryContextCommands.Add(pasteCommand);
        DirectoryContextCommands.Add(deleteCommand);
    }

    public void UpdateOpenFileStates(string? activeFilePath, IEnumerable<string?> dirtyFilePaths)
    {
        _activeFileRelativePath = TryGetRelativePathUnderRoot(activeFilePath);
        _dirtyFileRelativePaths = new HashSet<string>(
            GetRelativePathsUnderRoot(dirtyFilePaths),
            StringComparer.OrdinalIgnoreCase);
        ApplyActiveFileStateToLoadedNodes();
    }

    private IEnumerable<string> GetRelativePathsUnderRoot(IEnumerable<string?> dirtyFilePaths)
    {
        foreach (var filePath in dirtyFilePaths)
        {
            var relative = TryGetRelativePathUnderRoot(filePath);
            if (!string.IsNullOrWhiteSpace(relative))
            {
                yield return relative;
            }
        }
    }

    private string? TryGetRelativePathUnderRoot(string? activeFilePath)
    {
        var rootDirectoryPath = RootDirectoryPath;
        if (string.IsNullOrWhiteSpace(activeFilePath) || string.IsNullOrWhiteSpace(rootDirectoryPath))
        {
            return null;
        }

        if (!IsUnderRoot(activeFilePath, rootDirectoryPath))
        {
            return null;
        }

        var relative = NormalizeRelativePath(Path.GetRelativePath(rootDirectoryPath, activeFilePath));
        if (string.IsNullOrWhiteSpace(relative) || relative == ".")
        {
            return null;
        }

        return relative;
    }

    private void ApplyActiveFileStateToLoadedNodes()
    {
        foreach (var root in RootItems)
        {
            ApplyActiveFileStateToNodeRecursive(root);
        }
    }

    private void ApplyActiveFileStateToNodeRecursive(ExplorerNodeViewModel node)
    {
        ApplyActiveFileStateToSingleNode(node);

        if (node.Children.Count == 0)
        {
            return;
        }

        foreach (var child in node.Children)
        {
            ApplyActiveFileStateToNodeRecursive(child);
        }
    }

    private void ApplyActiveFileStateToSingleNode(ExplorerNodeViewModel node)
    {
        if (node.IsDirectory || node.IsPlaceholder)
        {
            node.IsActiveFile = false;
            node.IsDirty = false;
            return;
        }

        var activeRel = _activeFileRelativePath;
        var isActive = !string.IsNullOrWhiteSpace(activeRel)
            && string.Equals(node.RelativePath, activeRel, StringComparison.OrdinalIgnoreCase);

        node.IsActiveFile = isActive;
        node.IsDirty = _dirtyFileRelativePaths.Contains(node.RelativePath);
    }

    partial void OnRootDirectoryPathChanged(string? value)
    {
        OnPropertyChanged(nameof(RootDirectoryDisplayName));
    }

    [RelayCommand]
    private Task RefreshAsync() => RefreshInternalAsync();

    [RelayCommand]
    private async Task OpenFolderAsync()
    {
        var folder = await _folderPicker.PickFolderAsync(CancellationToken.None).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }

        await SetRootDirectoryAsync(folder, CancellationToken.None).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task CloseFolderAsync()
    {
        _refreshCts?.Cancel();

        RootDirectoryPath = null;
        _expandedDirectoryPaths.Clear();
        _selectedRelativePath = null;

        _settings.SaveBool(WasExplicitlyClosedKey, true);
        _settings.SaveString(LastOpenedDirectoryKey, null);
        _settings.SaveString(SelectedPathKey, null);
        _settings.SaveStringArrayAsJson(ExpandedDirectoryPathsKey, []);

        await InvokeOnUiAsync(() =>
        {
            RootItems.Clear();
            SelectedNode = null;
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the root directory and loads the first-level children.
    /// </summary>
    public Task SetRootDirectoryAsync(string rootDirectoryPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rootDirectoryPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(rootDirectoryPath));
        }

        RootDirectoryPath = rootDirectoryPath;
        _settings.SaveBool(WasExplicitlyClosedKey, false);
        _settings.SaveString(LastOpenedDirectoryKey, rootDirectoryPath);
        return RefreshInternalAsync(cancellationToken);
    }

    /// <summary>
    /// Attempts to restore the last directory browser session from persisted settings.
    /// </summary>
    public async Task<RestoreResult> TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        _isRestoring = true;
        try
        {
            var explicitlyClosed = _settings.LoadBool(WasExplicitlyClosedKey, defaultValue: false);
            if (explicitlyClosed)
            {
                return RestoreResult.ExplicitlyClosed;
            }

            var rootDirectoryPath = _settings.LoadString(LastOpenedDirectoryKey);
            if (string.IsNullOrWhiteSpace(rootDirectoryPath))
            {
                return RestoreResult.NoSession;
            }

            if (!Directory.Exists(rootDirectoryPath))
            {
                Log.Information("Last opened directory no longer exists: {DirectoryPath}", rootDirectoryPath);
                return RestoreResult.MissingDirectory;
            }

            _expandedDirectoryPaths.Clear();
            foreach (var path in _settings.LoadStringArrayFromJson(ExpandedDirectoryPathsKey))
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    _expandedDirectoryPaths.Add(NormalizeRelativePath(path));
                }
            }

            _selectedRelativePath = NormalizeRelativePath(_settings.LoadString(SelectedPathKey) ?? string.Empty);

            RootDirectoryPath = rootDirectoryPath;
            await RefreshInternalAsync(cancellationToken).ConfigureAwait(false);

            if (_expandedDirectoryPaths.Count > 0)
            {
                await ApplyExpandedStateAsync(cancellationToken).ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(_selectedRelativePath))
            {
                await RevealAndSelectAsync(_selectedRelativePath!, cancellationToken).ConfigureAwait(false);
            }

            return RestoreResult.Restored;
        }
        finally
        {
            _isRestoring = false;
        }
    }

    [RelayCommand]
    private void ToggleNodeExpansion(ExplorerNodeViewModel? node)
    {
        if (node is null || !node.IsDirectory)
        {
            return;
        }

        node.IsExpanded = !node.IsExpanded;
    }

    private async Task RefreshInternalAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(RootDirectoryPath) || !Directory.Exists(RootDirectoryPath))
        {
            await InvokeOnUiAsync(() =>
            {
                RootItems.Clear();
                SelectedNode = null;
            });
            return;
        }

        _refreshCts?.Cancel();
        _refreshCts?.Dispose();
        _refreshCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var ct = _refreshCts.Token;

        try
        {
            var children = await _directoryBrowserService.GetChildrenAsync(RootDirectoryPath, ct).ConfigureAwait(false);

            var rootNode = CreateRootNode(RootDirectoryPath);
            var childNodes = children.Select(entry => CreateNode(entry, rootNode.FullPath, relativeBase: rootNode.FullPath)).ToArray();

            await InvokeOnUiAsync(() =>
            {
                rootNode.Children.Clear();
                foreach (var node in childNodes)
                {
                    rootNode.Children.Add(node);
                }

                rootNode.HasLoadedChildren = true;

                RootItems.Clear();
                RootItems.Add(rootNode);
            }).ConfigureAwait(false);

            StartValidatingMotorFiles(rootNode, ct);
        }
        catch (OperationCanceledException)
        {
            // No-op.
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Directory browser refresh failed for root {RootDirectoryPath}", RootDirectoryPath);
        }
    }

    private ExplorerNodeViewModel CreateRootNode(string rootDirectoryPath)
    {
        var displayName = GetRootDisplayName(rootDirectoryPath);

        var node = new ExplorerNodeViewModel
        {
            DisplayName = displayName,
            FullPath = rootDirectoryPath,
            RelativePath = string.Empty,
            IsDirectory = true,
            IsRoot = true,
            IsExpanded = true,
            HasLoadedChildren = false,
            IsLoadingChildren = false
        };

        // Ensure consistent TreeView indentation by making directories appear to have children
        // even before the first async load populates them.
        node.Children.Add(CreatePlaceholderChild());

        node.PropertyChanged += OnNodePropertyChanged;
        return node;
    }

    private static ExplorerNodeViewModel CreatePlaceholderChild()
        => new()
        {
            DisplayName = string.Empty,
            FullPath = string.Empty,
            RelativePath = string.Empty,
            IsDirectory = true,
            IsRoot = false,
            IsExpanded = false,
            HasLoadedChildren = true,
            IsLoadingChildren = false,
            IsPlaceholder = true
        };

    private static string GetRootDisplayName(string rootDirectoryPath)
    {
        var trimmed = rootDirectoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var displayName = Path.GetFileName(trimmed);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            // Drive roots (e.g., C:\) end up with an empty filename; show "C:".
            if (Path.GetPathRoot(rootDirectoryPath) is { Length: > 0 } root)
            {
                return root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            return trimmed;
        }

        return displayName;
    }

    private ExplorerNodeViewModel CreateNode(DirectoryBrowserEntry entry, string rootDirectoryPath, string relativeBase)
    {
        var relativePath = Path.GetRelativePath(relativeBase, entry.FullPath);
        var normalizedRelativePath = NormalizeRelativePath(relativePath);

        var shouldStartExpanded = entry.IsDirectory && _expandedDirectoryPaths.Contains(normalizedRelativePath);

        var node = new ExplorerNodeViewModel
        {
            DisplayName = entry.Name,
            FullPath = entry.FullPath,
            RelativePath = normalizedRelativePath,
            IsDirectory = entry.IsDirectory,
            IsRoot = false,
            // Important: IsExpanded may be restored from persisted state. Set it after subscribing to
            // PropertyChanged so the expand handler runs and loads children.
            IsExpanded = false,
            HasLoadedChildren = false,
            IsLoadingChildren = false
        };

        ApplyActiveFileStateToSingleNode(node);

        if (node.IsDirectory)
        {
            // Keep indentation stable before the first child load.
            node.Children.Add(CreatePlaceholderChild());
        }

        node.PropertyChanged += OnNodePropertyChanged;

        if (shouldStartExpanded)
        {
            node.IsExpanded = true;
        }

        return node;
    }

    private async void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ExplorerNodeViewModel node)
        {
            return;
        }

        if (e.PropertyName == nameof(ExplorerNodeViewModel.IsExpanded))
        {
            if (node.IsRoot)
            {
                if (!node.IsExpanded)
                {
                    node.IsExpanded = true;
                }
                return;
            }

            if (!_isRestoring && node.IsDirectory)
            {
                lock (_expandedDirectoryPathsGate)
                {
                    if (node.IsExpanded)
                    {
                        _expandedDirectoryPaths.Add(node.RelativePath);
                    }
                    else
                    {
                        _expandedDirectoryPaths.Remove(node.RelativePath);
                    }
                }

                DebouncedPersistExpandedPaths();
            }

            if (node.IsExpanded)
            {
                await EnsureChildrenLoadedAsync(node, GetRefreshTokenOrNone()).ConfigureAwait(false);
            }
        }
    }

    private CancellationToken GetRefreshTokenOrNone()
    {
        try
        {
            return _refreshCts?.Token ?? CancellationToken.None;
        }
        catch (ObjectDisposedException)
        {
            return CancellationToken.None;
        }
    }

    private async Task EnsureChildrenLoadedAsync(ExplorerNodeViewModel node, CancellationToken cancellationToken)
    {
        if (!node.IsDirectory || node.HasLoadedChildren || node.IsLoadingChildren)
        {
            return;
        }

        node.IsLoadingChildren = true;

        try
        {
            var rootDirectoryPath = RootDirectoryPath;
            if (string.IsNullOrWhiteSpace(rootDirectoryPath))
            {
                return;
            }

            var children = await _directoryBrowserService.GetChildrenAsync(node.FullPath, cancellationToken).ConfigureAwait(false);
            var childNodes = children.Select(entry => CreateNode(entry, rootDirectoryPath, relativeBase: rootDirectoryPath)).ToArray();

            await InvokeOnUiAsync(() =>
            {
                node.Children.Clear();
                if (childNodes.Length == 0)
                {
                    // Keep indentation stable for empty directories.
                    node.Children.Add(CreatePlaceholderChild());
                }
                else
                {
                    foreach (var child in childNodes)
                    {
                        node.Children.Add(child);
                    }
                }

                node.HasLoadedChildren = true;
            }).ConfigureAwait(false);

            StartValidatingMotorFiles(node, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // No-op.
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Failed to load directory children for {DirectoryPath}", node.FullPath);
        }
        finally
        {
            node.IsLoadingChildren = false;
        }
    }

    private void StartValidatingMotorFiles(ExplorerNodeViewModel parentNode, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(parentNode);

        var children = parentNode.Children;
        if (children.Count == 0)
        {
            return;
        }

        // Requirement: show folders + *.json candidates first, then validate in the background.
        // Do not remove invalid files; mark them invalid so the user can still open and see errors.
        var fileNodes = children.Where(n => !n.IsDirectory && !n.IsPlaceholder).ToArray();
        if (fileNodes.Length == 0)
        {
            return;
        }

        var task = Task.Run(async () =>
        {
            foreach (var fileNode in fileNodes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var isValid = await IsValidMotorDefinitionFileAsync(fileNode.FullPath, cancellationToken).ConfigureAwait(false);
                await InvokeOnUiAsync(() =>
                {
                    // Node may already be gone due to refresh/expand changes.
                    if (children.Contains(fileNode))
                    {
                        fileNode.ValidationState = isValid
                            ? MotorFileValidationState.Valid
                            : MotorFileValidationState.Invalid;
                    }
                }).ConfigureAwait(false);
            }
        }, cancellationToken);

        _ = task.ContinueWith(
            t =>
            {
                if (t.Exception is not null)
                {
                    Log.Information(t.Exception, "Background motor file validation failed.");
                }
            },
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }

    private static async Task<bool> IsValidMotorDefinitionFileAsync(string filePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            // Treat a motor file as "valid" only if it can be loaded and passes semantic validation.
            // This keeps the explorer marker aligned with what the user can actually work with.
            cancellationToken.ThrowIfCancellationRequested();
            var motor = MotorFile.Load(filePath);
            var validationService = new ValidationService();
            var errors = validationService.ValidateServoMotor(motor);
            return errors.Count == 0;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        return relativePath.Replace('\\', '/');
    }

    [RelayCommand]
    private void IncreaseFontSize() => FontSize = Math.Min(MaxFontSize, FontSize + 1);

    [RelayCommand]
    private void DecreaseFontSize() => FontSize = Math.Max(MinFontSize, FontSize - 1);

    partial void OnFontSizeChanged(double value)
    {
        if (_isRestoring)
        {
            return;
        }

        DebouncedPersistFontSize();
    }

    partial void OnSelectedNodeChanged(ExplorerNodeViewModel? value)
    {
        if (_isRestoring)
        {
            return;
        }

        _selectedRelativePath = value?.RelativePath;
        DebouncedPersistSelectedPath();

        if (_suppressFileOpenOnSelection)
        {
            return;
        }

        if (value is not null && !value.IsDirectory)
        {
            RequestOpenFile(value.FullPath);
        }
    }

    private void RequestOpenFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var handler = FileOpenRequested;
        if (handler is null)
        {
            return;
        }

        _ = InvokeFileOpenRequestedAsync(handler, filePath);
    }

    private static async Task InvokeFileOpenRequestedAsync(Func<string, Task> handler, string filePath)
    {
        try
        {
            await handler(filePath).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Log.Information(ex, "Directory browser file open request failed for {FilePath}", filePath);
        }
    }

    /// <summary>
    /// Attempts to sync the explorer selection to the given absolute file path.
    /// If the file is not under the current root directory, selection is cleared.
    /// </summary>
    public async Task SyncSelectionToFilePathAsync(string? filePath, CancellationToken cancellationToken = default)
    {
        var rootDirectoryPath = RootDirectoryPath;
        if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(rootDirectoryPath))
        {
            await InvokeOnUiAsync(() => SelectedNode = null).ConfigureAwait(false);
            return;
        }

        if (!IsUnderRoot(filePath, rootDirectoryPath))
        {
            await InvokeOnUiAsync(() => SelectedNode = null).ConfigureAwait(false);
            return;
        }

        var relativePath = NormalizeRelativePath(Path.GetRelativePath(rootDirectoryPath, filePath));
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath == ".")
        {
            await InvokeOnUiAsync(() => SelectedNode = null).ConfigureAwait(false);
            return;
        }

        await RevealAndSelectWithoutOpeningAsync(relativePath, cancellationToken).ConfigureAwait(false);
    }

    private static bool IsUnderRoot(string filePath, string rootDirectoryPath)
    {
        try
        {
            var fullFile = Path.GetFullPath(filePath);
            var fullRoot = Path.GetFullPath(rootDirectoryPath);

            fullRoot = fullRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            return fullFile.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private async Task RevealAndSelectWithoutOpeningAsync(string relativePath, CancellationToken cancellationToken)
    {
        var node = await FindNodeByRelativePathAsync(relativePath, cancellationToken).ConfigureAwait(false);
        if (node is null)
        {
            return;
        }

        await InvokeOnUiAsync(() =>
        {
            _suppressFileOpenOnSelection = true;
            try
            {
                SelectedNode = node;
                node.IsSelected = true;
            }
            finally
            {
                _suppressFileOpenOnSelection = false;
            }
        }).ConfigureAwait(false);
    }

    private void DebouncedPersistExpandedPaths()
    {
        _persistExpandedCts?.Cancel();
        _persistExpandedCts?.Dispose();
        _persistExpandedCts = new CancellationTokenSource();
        var ct = _persistExpandedCts.Token;
        var rootDirectoryPath = RootDirectoryPath;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(250, ct).ConfigureAwait(false);

                string[] values;
                lock (_expandedDirectoryPathsGate)
                {
                    values = _expandedDirectoryPaths
                        .Where(static p => !string.IsNullOrWhiteSpace(p))
                        .OrderBy(static p => p, StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                }

                var pruned = PruneExpandedPaths(values, rootDirectoryPath);
                if (pruned.StalePaths.Length > 0)
                {
                    await InvokeOnUiAsync(() =>
                    {
                        lock (_expandedDirectoryPathsGate)
                        {
                            foreach (var stale in pruned.StalePaths)
                            {
                                _expandedDirectoryPaths.Remove(stale);
                            }
                        }
                    }).ConfigureAwait(false);
                }

                _settings.SaveStringArrayAsJson(ExpandedDirectoryPathsKey, pruned.Paths);
            }
            catch (OperationCanceledException)
            {
                // No-op.
            }
        }, CancellationToken.None);
    }

    private static (string[] Paths, string[] StalePaths) PruneExpandedPaths(string[] expandedRelativePaths, string? rootDirectoryPath)
    {
        if (string.IsNullOrWhiteSpace(rootDirectoryPath) || expandedRelativePaths.Length == 0)
        {
            return (expandedRelativePaths, Array.Empty<string>());
        }

        var root = rootDirectoryPath;
        var kept = new List<string>(expandedRelativePaths.Length);
        var stale = new List<string>();

        foreach (var rel in expandedRelativePaths)
        {
            var candidate = rel.Replace('/', Path.DirectorySeparatorChar);
            var full = Path.Combine(root, candidate);

            try
            {
                if (Directory.Exists(full))
                {
                    kept.Add(rel);
                }
                else
                {
                    stale.Add(rel);
                }
            }
            catch
            {
                // If we can't validate existence reliably, keep it to avoid flapping.
                kept.Add(rel);
            }
        }

        return (kept.ToArray(), stale.ToArray());
    }

    private void DebouncedPersistFontSize()
    {
        _persistFontSizeCts?.Cancel();
        _persistFontSizeCts?.Dispose();
        _persistFontSizeCts = new CancellationTokenSource();
        var ct = _persistFontSizeCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(250, ct).ConfigureAwait(false);
                _settings.SaveDouble(FontSizeKey, FontSize);
            }
            catch (OperationCanceledException)
            {
                // No-op.
            }
        }, CancellationToken.None);
    }

    private void DebouncedPersistSelectedPath()
    {
        _persistSelectedPathCts?.Cancel();
        _persistSelectedPathCts?.Dispose();
        _persistSelectedPathCts = new CancellationTokenSource();
        var ct = _persistSelectedPathCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(250, ct).ConfigureAwait(false);
                _settings.SaveString(SelectedPathKey, _selectedRelativePath);
            }
            catch (OperationCanceledException)
            {
                // No-op.
            }
        }, CancellationToken.None);
    }

    private async Task ApplyExpandedStateAsync(CancellationToken cancellationToken)
    {
        // Ensure parents expand before children.
        string[] ordered;
        lock (_expandedDirectoryPathsGate)
        {
            ordered = _expandedDirectoryPaths
                .Where(static p => !string.IsNullOrWhiteSpace(p))
                .OrderBy(static p => p.Count(c => c == '/'))
                .ThenBy(static p => p, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        foreach (var relativePath in ordered)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExpandDirectoryAsync(relativePath, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ExpandDirectoryAsync(string relativePath, CancellationToken cancellationToken)
    {
        var node = await FindNodeByRelativePathAsync(relativePath, cancellationToken).ConfigureAwait(false);
        if (node is null || !node.IsDirectory || node.IsRoot)
        {
            return;
        }

        node.IsExpanded = true;
        await EnsureChildrenLoadedAsync(node, cancellationToken).ConfigureAwait(false);
    }

    private async Task RevealAndSelectAsync(string relativePath, CancellationToken cancellationToken)
    {
        var node = await FindNodeByRelativePathAsync(relativePath, cancellationToken).ConfigureAwait(false);
        if (node is null)
        {
            return;
        }

        await InvokeOnUiAsync(() =>
        {
            SelectedNode = node;
            node.IsSelected = true;
        }).ConfigureAwait(false);
    }

    private async Task<ExplorerNodeViewModel?> FindNodeByRelativePathAsync(string relativePath, CancellationToken cancellationToken)
    {
        var normalized = NormalizeRelativePath(relativePath);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return null;
        }

        var root = RootItems.FirstOrDefault();
        if (root is null)
        {
            return null;
        }

        var current = root;
        for (var i = 0; i < parts.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (current.IsDirectory)
            {
                await EnsureChildrenLoadedAsync(current, cancellationToken).ConfigureAwait(false);
                current.IsExpanded = true;
            }

            var match = current.Children.FirstOrDefault(child =>
                string.Equals(child.DisplayName, parts[i], StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                return null;
            }

            current = match;
        }

        return current;
    }

    [RelayCommand]
    private async Task ExecuteContextCommandAsync(IDirectoryBrowserCommand? command)
    {
        if (command is null || SelectedNode is null)
        {
            return;
        }

        var path = SelectedNode.FullPath;
        var isDirectory = SelectedNode.IsDirectory;

        if (!command.CanExecute(path, isDirectory))
        {
            return;
        }

        // Handle rename command specially - just enter rename mode
        if (command is RenameCommand)
        {
            await BeginRenameAsync(SelectedNode).ConfigureAwait(false);
            return;
        }

        // Request confirmation if required
        if (command.RequiresConfirmation)
        {
            var confirmationMessage = command.GetConfirmationMessage(path, isDirectory);
            if (!string.IsNullOrWhiteSpace(confirmationMessage) && RequestConfirmation is not null)
            {
                var confirmed = await RequestConfirmation("Confirm Action", confirmationMessage).ConfigureAwait(false);
                if (!confirmed)
                {
                    return;
                }
            }
        }

        await command.ExecuteAsync(path, isDirectory).ConfigureAwait(false);

        // Refresh if the command modified the file system
        if (command.RequiresRefresh)
        {
            await RefreshInternalAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Places a node into rename mode while preserving its original display name.
    /// </summary>
    private Task BeginRenameAsync(ExplorerNodeViewModel node)
    {
        return InvokeOnUiAsync(() =>
        {
            node.OriginalDisplayName = node.DisplayName;
            node.IsRenaming = true;
        });
    }

    /// <summary>
    /// Completes a rename operation on the selected node.
    /// </summary>
    public async Task<bool> CompleteRenameAsync(ExplorerNodeViewModel node, string newName)
    {
        if (node is null)
        {
            return false;
        }

        var trimmedName = newName?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            await CancelRenameAsync(node).ConfigureAwait(false);
            return false;
        }

        var originalName = node.OriginalDisplayName ?? node.DisplayName;
        var directory = Path.GetDirectoryName(node.FullPath);
        var success = await RenameCommand.PerformRenameAsync(node.FullPath, trimmedName, node.IsDirectory).ConfigureAwait(false);

        await InvokeOnUiAsync(() =>
        {
            if (success)
            {
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    var newPath = Path.Combine(directory, trimmedName);
                    node.FullPath = newPath;

                    var rootDirectoryPath = RootDirectoryPath;
                    if (!string.IsNullOrWhiteSpace(rootDirectoryPath))
                    {
                        node.RelativePath = NormalizeRelativePath(Path.GetRelativePath(rootDirectoryPath, newPath));
                    }
                }

                node.DisplayName = trimmedName;
            }
            else
            {
                node.DisplayName = originalName;
            }

            node.OriginalDisplayName = null;
            node.IsRenaming = false;
        }).ConfigureAwait(false);
        return success;
    }

    /// <summary>
    /// Cancels a rename operation on the selected node.
    /// </summary>
    public async Task CancelRenameAsync(ExplorerNodeViewModel node)
    {
        if (node is null)
        {
            return;
        }

        await InvokeOnUiAsync(() =>
        {
            if (!string.IsNullOrWhiteSpace(node.OriginalDisplayName))
            {
                node.DisplayName = node.OriginalDisplayName!;
            }

            node.IsRenaming = false;
            node.OriginalDisplayName = null;
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Starts rename mode on the currently selected node (keyboard shortcut handler).
    /// </summary>
    [RelayCommand]
    public async Task StartRenameAsync()
    {
        if (SelectedNode is null || SelectedNode.IsRoot || SelectedNode.IsPlaceholder)
        {
            return;
        }

        await BeginRenameAsync(SelectedNode).ConfigureAwait(false);
    }

    /// <summary>
    /// Copies the selected node (keyboard shortcut handler).
    /// </summary>
    [RelayCommand]
    public async Task CopySelectedAsync()
    {
        if (SelectedNode is null || SelectedNode.IsPlaceholder)
        {
            return;
        }

        var copyCommand = new CopyFileCommand();
        if (copyCommand.CanExecute(SelectedNode.FullPath, SelectedNode.IsDirectory))
        {
            await copyCommand.ExecuteAsync(SelectedNode.FullPath, SelectedNode.IsDirectory).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Deletes the selected node (keyboard shortcut handler).
    /// </summary>
    [RelayCommand]
    public async Task DeleteSelectedAsync()
    {
        if (SelectedNode is null || SelectedNode.IsRoot || SelectedNode.IsPlaceholder)
        {
            return;
        }

        var deleteCommand = new DeleteCommand();
        await ExecuteContextCommandAsync(deleteCommand).ConfigureAwait(false);
    }

    /// <summary>
    /// Duplicates the selected node (keyboard shortcut handler).
    /// </summary>
    [RelayCommand]
    public async Task DuplicateSelectedAsync()
    {
        if (SelectedNode is null || SelectedNode.IsRoot || SelectedNode.IsPlaceholder)
        {
            return;
        }

        var duplicateCommand = new DuplicateCommand();
        if (duplicateCommand.CanExecute(SelectedNode.FullPath, SelectedNode.IsDirectory))
        {
            await duplicateCommand.ExecuteAsync(SelectedNode.FullPath, SelectedNode.IsDirectory).ConfigureAwait(false);
            await RefreshInternalAsync().ConfigureAwait(false);
            
            // TODO: Find the newly created item and start rename mode
        }
    }

    protected virtual async Task InvokeOnUiAsync(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(action);
    }
}
