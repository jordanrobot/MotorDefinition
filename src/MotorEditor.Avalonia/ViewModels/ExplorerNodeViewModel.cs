using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CurveEditor.ViewModels;

public enum MotorFileValidationState
{
    Unknown = 0,
    Valid = 1,
    Invalid = 2
}

/// <summary>
/// A node in the VS Code-style directory browser explorer tree.
/// </summary>
public partial class ExplorerNodeViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayNameWithDirtyIndicator))]
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

    [ObservableProperty]
    private bool _isPlaceholder;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayNameWithDirtyIndicator))]
    private bool _isActiveFile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayNameWithDirtyIndicator))]
    private bool _isActiveFileDirty;

    [ObservableProperty]
    private MotorFileValidationState _validationState = MotorFileValidationState.Unknown;

    public bool ShowChevron => IsDirectory && !IsRoot;

    public bool ShowValidationMarker => !IsDirectory && !IsPlaceholder;

    public string DisplayNameWithDirtyIndicator
    {
        get
        {
            if (IsDirectory || IsPlaceholder)
            {
                return DisplayName;
            }

            return IsActiveFile && IsActiveFileDirty
                ? DisplayName + "*"
                : DisplayName;
        }
    }

    public string ValidationMarker => ValidationState switch
    {
        // Emoji glyphs render with built-in green/red coloring on supported platforms.
        MotorFileValidationState.Valid => "✅",
        MotorFileValidationState.Invalid => "❌",
        _ => "-"
    };

    public string ValidationToolTip => ValidationState switch
    {
        MotorFileValidationState.Valid => "Valid motor file",
        MotorFileValidationState.Invalid => "Invalid motor file",
        _ => "Motor file validation pending"
    };

    partial void OnValidationStateChanged(MotorFileValidationState value)
    {
        OnPropertyChanged(nameof(ValidationMarker));
        OnPropertyChanged(nameof(ValidationToolTip));
    }

    public ObservableCollection<ExplorerNodeViewModel> Children { get; } = [];
}
