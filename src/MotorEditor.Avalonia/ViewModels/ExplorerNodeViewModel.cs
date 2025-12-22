using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CurveEditor.ViewModels;

/// <summary>
/// A node in the VS Code-style directory browser explorer tree.
/// </summary>
public partial class ExplorerNodeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _fullPath = string.Empty;

    [ObservableProperty]
    private string _relativePath = string.Empty;

    [ObservableProperty]
    private bool _isDirectory;

    [ObservableProperty]
    private bool _isRoot;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _hasLoadedChildren;

    [ObservableProperty]
    private bool _isLoadingChildren;

    public bool ShowChevron => IsDirectory && !IsRoot;

    public ObservableCollection<ExplorerNodeViewModel> Children { get; } = [];
}
