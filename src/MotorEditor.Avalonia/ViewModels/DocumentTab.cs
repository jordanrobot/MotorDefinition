using CommunityToolkit.Mvvm.ComponentModel;
using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;
using System.Collections.ObjectModel;
using System.IO;

namespace CurveEditor.ViewModels;

/// <summary>
/// Represents the state of a single document/tab.
/// This is a lightweight data holder - the MainWindowViewModel handles operations.
/// </summary>
public partial class DocumentTab : ObservableObject
{
    private readonly UndoStack _undoStack = new();
    private int _cleanCheckpoint;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private ServoMotor? _motor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private bool _isDirty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName), nameof(ToolTip))]
    private string? _filePath;

    [ObservableProperty]
    private Drive? _selectedDrive;

    [ObservableProperty]
    private Voltage? _selectedVoltage;

    [ObservableProperty]
    private Curve? _selectedSeries;

    [ObservableProperty]
    private ChartViewModel? _chartViewModel;

    [ObservableProperty]
    private CurveDataTableViewModel? _curveDataTableViewModel;

    [ObservableProperty]
    private EditingCoordinator? _editingCoordinator;

    [ObservableProperty]
    private ObservableCollection<Drive> _availableDrives = [];

    [ObservableProperty]
    private ObservableCollection<Voltage> _availableVoltages = [];

    [ObservableProperty]
    private ObservableCollection<Curve> _availableSeries = [];

    /// <summary>
    /// Undo stack for this document.
    /// </summary>
    public UndoStack UndoStack => _undoStack;

    /// <summary>
    /// Display name for the tab (file name + dirty indicator).
    /// </summary>
    public string DisplayName
    {
        get
        {
            var name = FilePath != null
                ? Path.GetFileName(FilePath)
                : Motor?.MotorName ?? "Untitled";
            var dirtyMark = IsDirty ? " >" : string.Empty;
            return $"{{ }} {name}{dirtyMark}";
        }
    }

    /// <summary>
    /// Tooltip for the tab (full file path).
    /// </summary>
    public string ToolTip => FilePath ?? "New file";

    /// <summary>
    /// Marks the current undo checkpoint as clean (just saved).
    /// </summary>
    public void MarkClean()
    {
        _cleanCheckpoint = _undoStack.UndoDepth;
        IsDirty = false;
    }

    /// <summary>
    /// Marks the document as dirty.
    /// </summary>
    public void MarkDirty()
    {
        IsDirty = true;
    }

    /// <summary>
    /// Updates dirty state based on undo depth.
    /// </summary>
    public void UpdateDirtyFromUndoDepth()
    {
        var depth = _undoStack.UndoDepth;
        IsDirty = depth != _cleanCheckpoint;
    }

    partial void OnMotorChanged(ServoMotor? value)
    {
        OnPropertyChanged(nameof(DisplayName));
    }

    partial void OnFilePathChanged(string? value)
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(ToolTip));
    }

    partial void OnIsDirtyChanged(bool value)
    {
        OnPropertyChanged(nameof(DisplayName));
    }
}
