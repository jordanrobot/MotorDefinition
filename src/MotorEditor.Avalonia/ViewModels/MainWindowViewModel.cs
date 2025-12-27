using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;
using MotorEditor.Avalonia.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CurveEditor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public enum UnsavedChangesChoice
    {
        Save,
        Ignore,
        Cancel
    }

    internal Func<string, Task<UnsavedChangesChoice>> UnsavedChangesPromptAsync { get; set; }

    private readonly IFileService _fileService;
    private readonly ICurveGeneratorService _curveGeneratorService;
    private readonly IValidationService _validationService;
    private readonly IDriveVoltageSeriesService _driveVoltageSeriesService;
    private readonly IMotorConfigurationWorkflow _motorConfigurationWorkflow;
    private readonly IUserSettingsStore _settingsStore;
    private readonly UndoStack _undoStack = new();
    private int _cleanCheckpoint;

    private static readonly FilePickerFileType JsonFileType = new("JSON Files")
    {
        Patterns = ["*.json"],
        MimeTypes = ["application/json"]
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private MotorDefinition? _currentMotor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    /// <summary>
    /// Current validation errors for the motor definition.
    /// </summary>
    [ObservableProperty]
    private string _validationErrors = string.Empty;

    /// <summary>
    /// Whether there are validation errors.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveWithValidation))]
    private bool _hasValidationErrors;

    // Drive selection
    [ObservableProperty]
    private DriveConfiguration? _selectedDrive;

    // Voltage selection
    [ObservableProperty]
    private VoltageConfiguration? _selectedVoltage;

    // Series selection
    [ObservableProperty]
    private CurveSeries? _selectedSeries;

    /// <summary>
    /// Coordinates editing and selection between chart and data table.
    /// </summary>
    [ObservableProperty]
    private EditingCoordinator _editingCoordinator = new();

    /// <summary>
    /// ViewModel for the chart component.
    /// </summary>
    [ObservableProperty]
    private ChartViewModel _chartViewModel;

    /// <summary>
    /// ViewModel for the curve data table.
    /// </summary>
    [ObservableProperty]
    private CurveDataTableViewModel _curveDataTableViewModel;

    /// <summary>
    /// ViewModel for the Directory Browser explorer panel.
    /// </summary>
    [ObservableProperty]
    private DirectoryBrowserViewModel _directoryBrowser = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private string? _currentFilePath;

    /// <summary>
    /// Whether the units section is expanded.
    /// </summary>
    [ObservableProperty]
    private bool _isUnitsExpanded;

    /// <summary>
    /// Whether the curve data panel is expanded.
    /// Derived from ActiveLeftPanelId since Curve Data is in the left zone.
    /// </summary>
    public bool IsCurveDataExpanded =>
        ActiveLeftPanelId == PanelRegistry.PanelIds.CurveData;

    /// <summary>
    /// Whether the directory browser panel is expanded.
    /// Derived from ActiveLeftPanelId since Browser is in the left zone.
    /// </summary>
    public bool IsBrowserPanelExpanded =>
        ActiveLeftPanelId == PanelRegistry.PanelIds.DirectoryBrowser;

    /// <summary>
    /// Whether the properties panel is expanded.
    /// Derived from ActivePanelBarPanelId since Properties is in the right zone.
    /// </summary>
    public bool IsPropertiesPanelExpanded =>
        ActivePanelBarPanelId == PanelRegistry.PanelIds.MotorProperties;

    /// <summary>
    /// The ID of the currently active panel in the Panel Bar, or null if all are collapsed.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBrowserPanelExpanded))]
    [NotifyPropertyChangedFor(nameof(IsPropertiesPanelExpanded))]
    [NotifyPropertyChangedFor(nameof(IsCurveDataExpanded))]
    private string? _activePanelBarPanelId = PanelRegistry.PanelIds.MotorProperties; // Default to Motor Properties expanded

    /// <summary>
    /// The ID of the active panel in the left zone.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBrowserPanelExpanded))]
    [NotifyPropertyChangedFor(nameof(IsCurveDataExpanded))]
    private string? _activeLeftPanelId = PanelRegistry.PanelIds.DirectoryBrowser; // Default to Browser expanded

    /// <summary>
    /// Which side of the window the Panel Bar is docked to.
    /// </summary>
    [ObservableProperty]
    private PanelBarDockSide _panelBarDockSide = PanelBarDockSide.Left;

    /// <summary>
    /// Toggles the browser panel visibility.
    /// </summary>
    [RelayCommand]
    private void ToggleBrowserPanel()
    {
        TogglePanel(PanelRegistry.PanelIds.DirectoryBrowser);
    }

    /// <summary>
    /// Toggles the properties panel visibility.
    /// </summary>
    [RelayCommand]
    private void TogglePropertiesPanel()
    {
        TogglePanel(PanelRegistry.PanelIds.MotorProperties);
    }

    /// <summary>
    /// Toggles the curve data panel visibility.
    /// </summary>
    [RelayCommand]
    private void ToggleCurveDataPanel()
    {
        TogglePanel(PanelRegistry.PanelIds.CurveData);
    }

    /// <summary>
    /// Toggles a panel by its ID, implementing zone-based exclusivity.
    /// Panels only collapse others in the same zone.
    /// </summary>
    public void TogglePanel(string panelId)
    {
        var descriptor = PanelRegistry.GetById(panelId);
        if (descriptor == null)
        {
            return;
        }

        // Determine which zone this panel belongs to
        switch (descriptor.Zone)
        {
            case PanelZone.Left:
                // Toggle within left zone
                if (ActiveLeftPanelId == panelId)
                {
                    ActiveLeftPanelId = null; // Collapse it
                }
                else
                {
                    ActiveLeftPanelId = panelId; // Expand it (collapses other left panels)
                }
                break;

            case PanelZone.Right:
                // Toggle within right zone
                if (ActivePanelBarPanelId == panelId)
                {
                    ActivePanelBarPanelId = null; // Collapse it
                }
                else
                {
                    ActivePanelBarPanelId = panelId; // Expand it (collapses other right panels)
                }
                break;

            case PanelZone.Bottom:
            case PanelZone.Center:
                // Not used in current configuration
                break;
        }
    }

    // Motor text editor buffers used to drive command-based edits.
    [ObservableProperty]
    private string _motorNameEditor = string.Empty;

    [ObservableProperty]
    private string _manufacturerEditor = string.Empty;

    [ObservableProperty]
    private string _partNumberEditor = string.Empty;

    // Motor scalar editor buffers used to drive command-based edits.

    [ObservableProperty]
    private string _maxSpeedEditor = string.Empty;

    [ObservableProperty]
    private string _ratedSpeedEditor = string.Empty;

    [ObservableProperty]
    private string _ratedPeakTorqueEditor = string.Empty;

    [ObservableProperty]
    private string _ratedContinuousTorqueEditor = string.Empty;

    [ObservableProperty]
    private string _powerEditor = string.Empty;

    [ObservableProperty]
    private string _weightEditor = string.Empty;

    [ObservableProperty]
    private string _rotorInertiaEditor = string.Empty;

    [ObservableProperty]
    private string _feedbackPprEditor = string.Empty;

    [ObservableProperty]
    private bool _hasBrakeEditor;

    [ObservableProperty]
    private string _brakeTorqueEditor = string.Empty;

    [ObservableProperty]
    private string _brakeAmperageEditor = string.Empty;

    [ObservableProperty]
    private string _brakeVoltageEditor = string.Empty;

    [ObservableProperty]
    private string _brakeReleaseTimeEditor = string.Empty;

    [ObservableProperty]
    private string _brakeEngageTimeMovEditor = string.Empty;

    [ObservableProperty]
    private string _brakeEngageTimeDiodeEditor = string.Empty;

    [ObservableProperty]
    private string _brakeBacklashEditor = string.Empty;

    // Drive editor buffers used to drive command-based edits.

    [ObservableProperty]
    private string _driveNameEditor = string.Empty;

    [ObservableProperty]
    private string _drivePartNumberEditor = string.Empty;

    [ObservableProperty]
    private string _driveManufacturerEditor = string.Empty;

    // Selected voltage editor buffers used to drive command-based edits.

    [ObservableProperty]
    private string _voltageValueEditor = string.Empty;

    [ObservableProperty]
    private string _voltagePowerEditor = string.Empty;

    [ObservableProperty]
    private string _voltageMaxSpeedEditor = string.Empty;

    [ObservableProperty]
    private string _voltagePeakTorqueEditor = string.Empty;

    [ObservableProperty]
    private string _voltageContinuousTorqueEditor = string.Empty;

    [ObservableProperty]
    private string _voltageContinuousAmpsEditor = string.Empty;

    [ObservableProperty]
    private string _voltagePeakAmpsEditor = string.Empty;

    /// <summary>
    /// Cached list of available voltages for the selected drive.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<VoltageConfiguration> _availableVoltages = [];

    /// <summary>
    /// Cached list of available series for the selected voltage.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<CurveSeries> _availableSeries = [];

    /// <summary>
    /// Cached list of available drives from current motor definition.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DriveConfiguration> _availableDrives = [];

    /// <summary>
    /// Gets whether there is at least one operation to undo.
    /// </summary>
    public bool CanUndo => _undoStack.CanUndo;

    /// <summary>
    /// Gets whether there is at least one operation to redo.
    /// </summary>
    public bool CanRedo => _undoStack.CanRedo;

    /// <summary>
    /// Whether save is enabled (motor exists and no validation errors).
    /// </summary>
    public bool CanSaveWithValidation => CurrentMotor is not null && !HasValidationErrors;

    /// <summary>
    /// Supported speed units for dropdowns.
    /// </summary>
    public static string[] SpeedUnits => UnitSettings.SupportedSpeedUnits;

    /// <summary>
    /// Supported weight units for dropdowns.
    /// </summary>
    public static string[] WeightUnits => UnitSettings.SupportedWeightUnits;

    /// <summary>
    /// Supported torque units for dropdowns.
    /// </summary>
    public static string[] TorqueUnits => UnitSettings.SupportedTorqueUnits;

    /// <summary>
    /// Supported power units for dropdowns.
    /// </summary>
    public static string[] PowerUnits => UnitSettings.SupportedPowerUnits;

    /// <summary>
    /// Supported voltage units for dropdowns.
    /// </summary>
    public static string[] VoltageUnits => UnitSettings.SupportedVoltageUnits;

    /// <summary>
    /// Supported current units for dropdowns.
    /// </summary>
    public static string[] CurrentUnits => UnitSettings.SupportedCurrentUnits;

    /// <summary>
    /// Supported inertia units for dropdowns.
    /// </summary>
    public static string[] InertiaUnits => UnitSettings.SupportedInertiaUnits;

    /// <summary>
    /// Supported torque constant units for dropdowns.
    /// </summary>
    public static string[] TorqueConstantUnits => UnitSettings.SupportedTorqueConstantUnits;

    /// <summary>
    /// Supported backlash units for dropdowns.
    /// </summary>
    public static string[] BacklashUnits => UnitSettings.SupportedBacklashUnits;

    /// <summary>
    /// Supported time units for dropdowns.
    /// </summary>
    public static string[] TimeUnits => UnitSettings.SupportedResponseTimeUnits;

    /// <summary>
    /// Opens the folder where application log files are stored.
    /// </summary>
    [RelayCommand]
    private void OpenLogsFolder()
    {
        try
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CurveEditor",
                "logs");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            Log.Information("Opening logs folder at {LogDirectory}", logDirectory);

            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = logDirectory,
                    UseShellExecute = true
                }
            };

            process.Start();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open logs folder");
            StatusMessage = "Failed to open logs folder. See log for details.";
        }
    }

    public MainWindowViewModel()
    {
        _curveGeneratorService = new CurveGeneratorService();
        _fileService = new FileService(_curveGeneratorService);
        _validationService = new ValidationService();
        _driveVoltageSeriesService = new DriveVoltageSeriesService();
        _chartViewModel = new ChartViewModel();
        _curveDataTableViewModel = new CurveDataTableViewModel();
        _motorConfigurationWorkflow = new MotorConfigurationWorkflow(_driveVoltageSeriesService);
        _settingsStore = new PanelLayoutUserSettingsStore();
        UnsavedChangesPromptAsync = ShowUnsavedChangesPromptAsync;
        WireEditingCoordinator();
        WireUndoInfrastructure();
        WireDirectoryBrowserIntegration();
    }

    public MainWindowViewModel(IFileService fileService, ICurveGeneratorService curveGeneratorService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _curveGeneratorService = curveGeneratorService ?? throw new ArgumentNullException(nameof(curveGeneratorService));
        _validationService = new ValidationService();
        _driveVoltageSeriesService = new DriveVoltageSeriesService();
        _chartViewModel = new ChartViewModel();
        _curveDataTableViewModel = new CurveDataTableViewModel();
        _motorConfigurationWorkflow = new MotorConfigurationWorkflow(_driveVoltageSeriesService);
        _settingsStore = new PanelLayoutUserSettingsStore();
        UnsavedChangesPromptAsync = ShowUnsavedChangesPromptAsync;
        WireEditingCoordinator();
        WireUndoInfrastructure();
        WireDirectoryBrowserIntegration();
    }

    /// <summary>
    /// Public constructor intended for tests and advanced composition scenarios
    /// where all dependencies, including workflow services, are supplied
    /// explicitly.
    /// </summary>
    public MainWindowViewModel(
        IFileService fileService,
        ICurveGeneratorService curveGeneratorService,
        IValidationService validationService,
        IDriveVoltageSeriesService driveVoltageSeriesService,
        IMotorConfigurationWorkflow motorConfigurationWorkflow,
        ChartViewModel chartViewModel,
        CurveDataTableViewModel curveDataTableViewModel,
        IUserSettingsStore? settingsStore = null,
        Func<string, Task<UnsavedChangesChoice>>? unsavedChangesPromptAsync = null)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _curveGeneratorService = curveGeneratorService ?? throw new ArgumentNullException(nameof(curveGeneratorService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _driveVoltageSeriesService = driveVoltageSeriesService ?? throw new ArgumentNullException(nameof(driveVoltageSeriesService));
        _motorConfigurationWorkflow = motorConfigurationWorkflow ?? throw new ArgumentNullException(nameof(motorConfigurationWorkflow));
        _chartViewModel = chartViewModel ?? throw new ArgumentNullException(nameof(chartViewModel));
        _curveDataTableViewModel = curveDataTableViewModel ?? throw new ArgumentNullException(nameof(curveDataTableViewModel));
        _settingsStore = settingsStore ?? new PanelLayoutUserSettingsStore();
        UnsavedChangesPromptAsync = unsavedChangesPromptAsync ?? ShowUnsavedChangesPromptAsync;

        WireEditingCoordinator();
        WireUndoInfrastructure();
        WireDirectoryBrowserIntegration();
    }

    private void WireDirectoryBrowserIntegration()
    {
        DirectoryBrowser.FileOpenRequested -= HandleDirectoryBrowserFileOpenRequestedAsync;
        DirectoryBrowser.FileOpenRequested += HandleDirectoryBrowserFileOpenRequestedAsync;

        DirectoryBrowser.PropertyChanged -= OnDirectoryBrowserPropertyChanged;
        DirectoryBrowser.PropertyChanged += OnDirectoryBrowserPropertyChanged;

        CurrentFilePath = _fileService.CurrentFilePath;
        DirectoryBrowser.UpdateActiveFileState(CurrentFilePath, IsDirty);
    }

    private void OnDirectoryBrowserPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DirectoryBrowserViewModel.RootDirectoryPath))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentFilePath))
        {
            return;
        }

        _ = SyncDirectoryBrowserSelectionToCurrentFileAfterRootReadyAsync();
    }

    private async Task SyncDirectoryBrowserSelectionToCurrentFileAfterRootReadyAsync()
    {
        var filePath = CurrentFilePath;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var expectedRoot = DirectoryBrowser.RootDirectoryPath;
        if (string.IsNullOrWhiteSpace(expectedRoot))
        {
            return;
        }

        // RootDirectoryPath is set before the DirectoryBrowser has finished refreshing its tree.
        // Also, RootItems may still contain the *previous* root at this point.
        // Wait until the displayed root node matches the expected root.
        try
        {
            var start = DateTimeOffset.UtcNow;
            while (DateTimeOffset.UtcNow - start < TimeSpan.FromSeconds(2))
            {
                var currentRootNode = DirectoryBrowser.RootItems.FirstOrDefault();
                if (currentRootNode is not null &&
                    string.Equals(currentRootNode.FullPath, expectedRoot, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                await Task.Delay(25).ConfigureAwait(true);
            }

            await DirectoryBrowser.SyncSelectionToFilePathAsync(filePath).ConfigureAwait(true);
            DirectoryBrowser.UpdateActiveFileState(filePath, IsDirty);
        }
        catch
        {
            // Best-effort. We don't want root-change highlighting to destabilize the app.
        }
    }

    partial void OnDirectoryBrowserChanged(DirectoryBrowserViewModel value)
    {
        if (value is null)
        {
            return;
        }

        WireDirectoryBrowserIntegration();
    }

    private Task HandleDirectoryBrowserFileOpenRequestedAsync(string filePath)
        => OpenMotorFileFromDirectoryBrowserAsync(filePath);

    [RelayCommand]
    private async Task OpenFolderAsync()
    {
        if (ActiveLeftPanelId != PanelRegistry.PanelIds.DirectoryBrowser)
        {
            ActiveLeftPanelId = PanelRegistry.PanelIds.DirectoryBrowser;
        }

        try
        {
            await DirectoryBrowser.OpenFolderCommand.ExecuteAsync(null).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open folder");
            StatusMessage = $"Error opening folder: {ex.Message}";
        }
    }

    private async Task OpenMotorFileFromDirectoryBrowserAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        if (!await ConfirmLoseUnsavedChangesOrCancelAsync("open another file", "Open cancelled.").ConfigureAwait(true))
        {
            return;
        }

        await OpenMotorFileInternalAsync(filePath, updateExplorerSelection: false).ConfigureAwait(true);
    }

    private static async Task<UnsavedChangesChoice> ShowUnsavedChangesPromptAsync(string actionDescription)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return UnsavedChangesChoice.Cancel;
        }

        var dialog = new Views.UnsavedChangesDialog
        {
            Title = "Unsaved Changes",
            ActionDescription = actionDescription
        };

        await dialog.ShowDialog(desktop.MainWindow);
        return dialog.Choice;
    }

    private async Task<bool> ConfirmLoseUnsavedChangesOrCancelAsync(string actionDescription, string cancelledStatusMessage)
    {
        if (!IsDirty)
        {
            return true;
        }

        var choice = await UnsavedChangesPromptAsync(actionDescription).ConfigureAwait(true);
        if (choice == UnsavedChangesChoice.Cancel)
        {
            StatusMessage = cancelledStatusMessage;
            return false;
        }

        if (choice == UnsavedChangesChoice.Save)
        {
            await SaveAsync().ConfigureAwait(true);
            if (IsDirty)
            {
                StatusMessage = cancelledStatusMessage;
                return false;
            }
        }

        return true;
    }

    internal Task<bool> ConfirmCloseAppOrCancelAsync()
        => ConfirmLoseUnsavedChangesOrCancelAsync("close the app", "Close cancelled.");

    /// <summary>
    /// Opens a motor definition by path and synchronizes Directory Browser selection when applicable.
    /// </summary>
    public Task OpenMotorFileByPathAsync(string filePath)
        => OpenMotorFileInternalAsync(filePath, updateExplorerSelection: true);

    private async Task OpenMotorFileInternalAsync(string filePath, bool updateExplorerSelection)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        try
        {
            CurrentMotor = await _fileService.LoadAsync(filePath).ConfigureAwait(true);
            _undoStack.Clear();
            MarkCleanCheckpoint();
            IsDirty = _fileService.IsDirty;
            CurrentFilePath = _fileService.CurrentFilePath;

            StatusMessage = $"Loaded: {Path.GetFileName(filePath)}";
            OnPropertyChanged(nameof(WindowTitle));

            _settingsStore.SaveString(DirectoryBrowserViewModel.LastOpenedMotorFileKey, filePath);

            if (updateExplorerSelection)
            {
                await DirectoryBrowser.SyncSelectionToFilePathAsync(filePath).ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file {FilePath}", filePath);
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void WireEditingCoordinator()
    {
        ChartViewModel.DataChanged += OnChartDataChanged;
        CurveDataTableViewModel.DataChanged += OnDataTableDataChanged;
        ChartViewModel.EditingCoordinator = EditingCoordinator;
        CurveDataTableViewModel.EditingCoordinator = EditingCoordinator;
    }

    private void WireUndoInfrastructure()
    {
        ChartViewModel.UndoStack = _undoStack;
        CurveDataTableViewModel.UndoStack = _undoStack;

        _undoStack.UndoStackChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            UpdateDirtyFromUndoDepth();
        };
    }

    public string WindowTitle
    {
        get
        {
            var fileName = CurrentFilePath is not null
                ? Path.GetFileName(CurrentFilePath)
                : CurrentMotor?.MotorName ?? "Untitled";

            var dirtyIndicator = IsDirty ? " *" : string.Empty;
            return $"{fileName}{dirtyIndicator} - Curve Editor";
        }
    }

    partial void OnCurrentFilePathChanged(string? value)
    {
        _ = DirectoryBrowser.SyncSelectionToFilePathAsync(value);
        DirectoryBrowser.UpdateActiveFileState(value, IsDirty);
        OnPropertyChanged(nameof(WindowTitle));
    }

    partial void OnIsDirtyChanged(bool value)
    {
        DirectoryBrowser.UpdateActiveFileState(CurrentFilePath, value);
    }

    /// <summary>
    /// Marks the current undo history position as the clean checkpoint
    /// corresponding to the last successful save.
    /// </summary>
    public void MarkCleanCheckpoint()
    {
        _cleanCheckpoint = GetUndoDepth();
        IsDirty = false;
    }

    private int GetUndoDepth()
    {
        return _undoStack.UndoDepth;
    }

    private void UpdateDirtyFromUndoDepth()
    {
        var depth = GetUndoDepth();

        if (depth == _cleanCheckpoint && !_fileService.IsDirty)
        {
            IsDirty = false;
        }
        else if (depth != _cleanCheckpoint || _fileService.IsDirty)
        {
            IsDirty = true;
        }
    }

    private void RefreshMotorEditorsFromCurrentMotor()
    {
        if (CurrentMotor is null)
        {
            MotorNameEditor = string.Empty;
            ManufacturerEditor = string.Empty;
            PartNumberEditor = string.Empty;
            MaxSpeedEditor = string.Empty;
            RatedSpeedEditor = string.Empty;
            RatedPeakTorqueEditor = string.Empty;
            RatedContinuousTorqueEditor = string.Empty;
            PowerEditor = string.Empty;
            WeightEditor = string.Empty;
            RotorInertiaEditor = string.Empty;
            FeedbackPprEditor = string.Empty;
            HasBrakeEditor = false;
            BrakeTorqueEditor = string.Empty;
            BrakeAmperageEditor = string.Empty;
            BrakeVoltageEditor = string.Empty;
            BrakeReleaseTimeEditor = string.Empty;
            BrakeEngageTimeMovEditor = string.Empty;
            BrakeEngageTimeDiodeEditor = string.Empty;
            BrakeBacklashEditor = string.Empty;
            DriveNameEditor = string.Empty;
            DrivePartNumberEditor = string.Empty;
            DriveManufacturerEditor = string.Empty;
            VoltageValueEditor = string.Empty;
            VoltagePowerEditor = string.Empty;
            VoltageMaxSpeedEditor = string.Empty;
            VoltagePeakTorqueEditor = string.Empty;
            VoltageContinuousTorqueEditor = string.Empty;
            VoltageContinuousAmpsEditor = string.Empty;
            VoltagePeakAmpsEditor = string.Empty;
            return;
        }

        MotorNameEditor = CurrentMotor.MotorName ?? string.Empty;
        ManufacturerEditor = CurrentMotor.Manufacturer ?? string.Empty;
        PartNumberEditor = CurrentMotor.PartNumber ?? string.Empty;
        MaxSpeedEditor = CurrentMotor.MaxSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RatedSpeedEditor = CurrentMotor.RatedSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RatedPeakTorqueEditor = CurrentMotor.RatedPeakTorque.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RatedContinuousTorqueEditor = CurrentMotor.RatedContinuousTorque.ToString(System.Globalization.CultureInfo.InvariantCulture);
        PowerEditor = CurrentMotor.Power.ToString(System.Globalization.CultureInfo.InvariantCulture);
        WeightEditor = CurrentMotor.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RotorInertiaEditor = CurrentMotor.RotorInertia.ToString(System.Globalization.CultureInfo.InvariantCulture);
        FeedbackPprEditor = CurrentMotor.FeedbackPpr.ToString(System.Globalization.CultureInfo.InvariantCulture);
        HasBrakeEditor = CurrentMotor.HasBrake;
        BrakeTorqueEditor = CurrentMotor.BrakeTorque.ToString(System.Globalization.CultureInfo.InvariantCulture);
        BrakeAmperageEditor = CurrentMotor.BrakeAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture);
        BrakeVoltageEditor = CurrentMotor.BrakeVoltage.ToString(System.Globalization.CultureInfo.InvariantCulture);
        BrakeReleaseTimeEditor = CurrentMotor.BrakeReleaseTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
        BrakeEngageTimeMovEditor = CurrentMotor.BrakeEngageTimeMov.ToString(System.Globalization.CultureInfo.InvariantCulture);
        BrakeEngageTimeDiodeEditor = CurrentMotor.BrakeEngageTimeDiode.ToString(System.Globalization.CultureInfo.InvariantCulture);
        BrakeBacklashEditor = CurrentMotor.BrakeBacklash.ToString(System.Globalization.CultureInfo.InvariantCulture);
        DriveNameEditor = SelectedDrive?.Name ?? string.Empty;
        DrivePartNumberEditor = SelectedDrive?.PartNumber ?? string.Empty;
        DriveManufacturerEditor = SelectedDrive?.Manufacturer ?? string.Empty;
        VoltageValueEditor = SelectedVoltage?.Voltage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePowerEditor = SelectedVoltage?.Power.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageMaxSpeedEditor = SelectedVoltage?.MaxSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakTorqueEditor = SelectedVoltage?.RatedPeakTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousTorqueEditor = SelectedVoltage?.RatedContinuousTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousAmpsEditor = SelectedVoltage?.ContinuousAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakAmpsEditor = SelectedVoltage?.PeakAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>
    /// Edits the motor name via an undoable command.
    /// </summary>
    /// <param name="newName">The new motor name.</param>
    public void EditMotorName(string newName)
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldName = CurrentMotor.MotorName ?? string.Empty;
        var newNameValue = newName ?? string.Empty;

        if (string.Equals(oldName, newNameValue, StringComparison.Ordinal))
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.MotorName), oldName, newNameValue);
        _undoStack.PushAndExecute(command);
        UpdateDirtyFromUndoDepth();
        MotorNameEditor = CurrentMotor.MotorName ?? string.Empty;
        OnPropertyChanged(nameof(WindowTitle));
    }

    /// <summary>
    /// Edits the motor manufacturer via an undoable command.
    /// </summary>
    /// <param name="newManufacturer">The new manufacturer.</param>
    public void EditMotorManufacturer(string newManufacturer)
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldManufacturer = CurrentMotor.Manufacturer ?? string.Empty;
        var newManufacturerValue = newManufacturer ?? string.Empty;

        if (string.Equals(oldManufacturer, newManufacturerValue, StringComparison.Ordinal))
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.Manufacturer), oldManufacturer, newManufacturerValue);
        _undoStack.PushAndExecute(command);
        UpdateDirtyFromUndoDepth();
        ManufacturerEditor = CurrentMotor.Manufacturer ?? string.Empty;
    }

    /// <summary>
    /// Edits the motor max speed via an undoable command.
    /// </summary>
    public void EditMotorMaxSpeed()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.MaxSpeed;
        if (!TryParseDouble(MaxSpeedEditor, oldValue, out var newValue))
        {
            MaxSpeedEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.MaxSpeed), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        MaxSpeedEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ChartViewModel.MotorMaxSpeed = newValue;
        IsDirty = true;
    }

    /// <summary>
    /// Edits the motor part number via an undoable command.
    /// </summary>
    /// <param name="newPartNumber">The new part number.</param>
    public void EditMotorPartNumber(string newPartNumber)
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldPartNumber = CurrentMotor.PartNumber ?? string.Empty;
        var newPartNumberValue = newPartNumber ?? string.Empty;
        if (string.Equals(oldPartNumber, newPartNumberValue, StringComparison.Ordinal))
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.PartNumber), oldPartNumber, newPartNumberValue);
        _undoStack.PushAndExecute(command);
        UpdateDirtyFromUndoDepth();
        PartNumberEditor = CurrentMotor.PartNumber ?? string.Empty;
    }

    private static bool TryParseDouble(string text, double currentValue, out double parsed)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            parsed = currentValue;
            return true;
        }

        if (double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            parsed = value;
            return true;
        }

        parsed = currentValue;
        return false;
    }

    /// <summary>
    /// Refreshes the available drives collection from the current motor.
    /// </summary>
    private void RefreshAvailableDrives()
    {
        AvailableDrives.Clear();
        if (CurrentMotor is not null)
        {
            foreach (var drive in CurrentMotor.Drives)
            {
                AvailableDrives.Add(drive);
            }
        }
    }

    /// <summary>
    /// Refreshes the available voltages collection from the selected drive.
    /// </summary>
    private void RefreshAvailableVoltages()
    {
        AvailableVoltages.Clear();
        if (SelectedDrive is not null)
        {
            foreach (var voltage in SelectedDrive.Voltages)
            {
                AvailableVoltages.Add(voltage);
            }
        }
    }

    public void EditDriveName()
    {
        if (SelectedDrive is null)
        {
            return;
        }

        var oldValue = SelectedDrive.Name ?? string.Empty;
        var newValue = DriveNameEditor ?? string.Empty;

        if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            return;
        }

        var command = new EditDrivePropertyCommand(SelectedDrive, nameof(DriveConfiguration.Name), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        DriveNameEditor = newValue;
        IsDirty = true;
    }

    public void EditDrivePartNumber()
    {
        if (SelectedDrive is null)
        {
            return;
        }

        var oldValue = SelectedDrive.PartNumber ?? string.Empty;
        var newValue = DrivePartNumberEditor ?? string.Empty;

        if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            return;
        }

        var command = new EditDrivePropertyCommand(SelectedDrive, nameof(DriveConfiguration.PartNumber), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        DrivePartNumberEditor = newValue;
        IsDirty = true;
    }

    public void EditDriveManufacturer()
    {
        if (SelectedDrive is null)
        {
            return;
        }

        var oldValue = SelectedDrive.Manufacturer ?? string.Empty;
        var newValue = DriveManufacturerEditor ?? string.Empty;

        if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            return;
        }

        var command = new EditDrivePropertyCommand(SelectedDrive, nameof(DriveConfiguration.Manufacturer), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        DriveManufacturerEditor = newValue;
        IsDirty = true;
    }

    public void EditSelectedVoltageValue()
    {
        if (SelectedVoltage is null)
        {
            return;
        }

        var oldValue = SelectedVoltage.Voltage;
        if (!TryParseDouble(VoltageValueEditor, oldValue, out var newValue))
        {
            VoltageValueEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        Log.Debug("EditSelectedVoltageValue: old={Old}, new={New}", oldValue, newValue);
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(VoltageConfiguration.Voltage), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        VoltageValueEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ChartViewModel.RefreshChart();
        CurveDataTableViewModel.RefreshData();
        IsDirty = true;
    }

    public void EditSelectedVoltagePower()
    {
        if (SelectedVoltage is null)
        {
            return;
        }

        var oldValue = SelectedVoltage.Power;
        if (!TryParseDouble(VoltagePowerEditor, oldValue, out var newValue))
        {
            VoltagePowerEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        Log.Debug("EditSelectedVoltagePower: old={Old}, new={New}", oldValue, newValue);
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(VoltageConfiguration.Power), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        VoltagePowerEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ChartViewModel.RefreshChart();
        CurveDataTableViewModel.RefreshData();
        IsDirty = true;
    }

    public void EditSelectedVoltageMaxSpeed()
    {
        if (SelectedVoltage is null)
        {
            return;
        }

        var oldValue = SelectedVoltage.MaxSpeed;
        if (!TryParseDouble(VoltageMaxSpeedEditor, oldValue, out var newValue))
        {
            VoltageMaxSpeedEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        Log.Debug("EditSelectedVoltageMaxSpeed: old={Old}, new={New}", oldValue, newValue);
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(VoltageConfiguration.MaxSpeed), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        VoltageMaxSpeedEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ChartViewModel.RefreshChart();
        CurveDataTableViewModel.RefreshData();
        IsDirty = true;
    }

    public void EditSelectedVoltagePeakTorque()
    {
        if (SelectedVoltage is null)
        {
            return;
        }

        var oldValue = SelectedVoltage.RatedPeakTorque;
        if (!TryParseDouble(VoltagePeakTorqueEditor, oldValue, out var newValue))
        {
            VoltagePeakTorqueEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        Log.Debug("EditSelectedVoltagePeakTorque: old={Old}, new={New}", oldValue, newValue);
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(VoltageConfiguration.RatedPeakTorque), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        VoltagePeakTorqueEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ChartViewModel.RefreshChart();
        CurveDataTableViewModel.RefreshData();
        IsDirty = true;
    }

    public void EditSelectedVoltageContinuousTorque()
    {
        if (SelectedVoltage is null)
        {
            return;
        }

        var oldValue = SelectedVoltage.RatedContinuousTorque;
        if (!TryParseDouble(VoltageContinuousTorqueEditor, oldValue, out var newValue))
        {
            VoltageContinuousTorqueEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        Log.Debug("EditSelectedVoltageContinuousTorque: old={Old}, new={New}", oldValue, newValue);
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(VoltageConfiguration.RatedContinuousTorque), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        VoltageContinuousTorqueEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ChartViewModel.RefreshChart();
        CurveDataTableViewModel.RefreshData();
        IsDirty = true;
    }

    public void EditSelectedVoltageContinuousAmps()
    {
        if (SelectedVoltage is null)
        {
            return;
        }

        var oldValue = SelectedVoltage.ContinuousAmperage;
        if (!TryParseDouble(VoltageContinuousAmpsEditor, oldValue, out var newValue))
        {
            VoltageContinuousAmpsEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        Log.Debug("EditSelectedVoltageContinuousAmps: old={Old}, new={New}", oldValue, newValue);
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(VoltageConfiguration.ContinuousAmperage), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        VoltageContinuousAmpsEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ChartViewModel.RefreshChart();
        CurveDataTableViewModel.RefreshData();
        IsDirty = true;
    }

    public void EditSelectedVoltagePeakAmps()
    {
        if (SelectedVoltage is null)
        {
            return;
        }

        var oldValue = SelectedVoltage.PeakAmperage;
        if (!TryParseDouble(VoltagePeakAmpsEditor, oldValue, out var newValue))
        {
            VoltagePeakAmpsEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        Log.Debug("EditSelectedVoltagePeakAmps: old={Old}, new={New}", oldValue, newValue);
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(VoltageConfiguration.PeakAmperage), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        VoltagePeakAmpsEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ChartViewModel.RefreshChart();
        CurveDataTableViewModel.RefreshData();
        IsDirty = true;
    }

    public void EditMotorRatedSpeed()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.RatedSpeed;
        if (!TryParseDouble(RatedSpeedEditor, oldValue, out var newValue))
        {
            RatedSpeedEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.RatedSpeed), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        RatedSpeedEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorRatedPeakTorque()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.RatedPeakTorque;
        if (!TryParseDouble(RatedPeakTorqueEditor, oldValue, out var newValue))
        {
            RatedPeakTorqueEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.RatedPeakTorque), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        RatedPeakTorqueEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorRatedContinuousTorque()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.RatedContinuousTorque;
        if (!TryParseDouble(RatedContinuousTorqueEditor, oldValue, out var newValue))
        {
            RatedContinuousTorqueEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.RatedContinuousTorque), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        RatedContinuousTorqueEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorPower()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.Power;
        if (!TryParseDouble(PowerEditor, oldValue, out var newValue))
        {
            PowerEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.Power), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        PowerEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorWeight()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.Weight;
        if (!TryParseDouble(WeightEditor, oldValue, out var newValue))
        {
            WeightEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.Weight), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        WeightEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorRotorInertia()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.RotorInertia;
        if (!TryParseDouble(RotorInertiaEditor, oldValue, out var newValue))
        {
            RotorInertiaEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.RotorInertia), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        RotorInertiaEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorFeedbackPpr()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.FeedbackPpr;
        if (!int.TryParse(FeedbackPprEditor, out var newValue))
        {
            FeedbackPprEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (oldValue == newValue)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.FeedbackPpr), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        FeedbackPprEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorHasBrake()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.HasBrake;
        var newValue = HasBrakeEditor;

        if (oldValue == newValue)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.HasBrake), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        HasBrakeEditor = newValue;
        ChartViewModel.HasBrake = newValue;
        IsDirty = true;
    }

    public void EditMotorBrakeTorque()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.BrakeTorque;
        if (!TryParseDouble(BrakeTorqueEditor, oldValue, out var newValue))
        {
            BrakeTorqueEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.BrakeTorque), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        BrakeTorqueEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ChartViewModel.BrakeTorque = newValue;
        IsDirty = true;
    }

    public void EditMotorBrakeAmperage()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.BrakeAmperage;
        if (!TryParseDouble(BrakeAmperageEditor, oldValue, out var newValue))
        {
            BrakeAmperageEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.BrakeAmperage), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        BrakeAmperageEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorBrakeVoltage()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.BrakeVoltage;
        if (!TryParseDouble(BrakeVoltageEditor, oldValue, out var newValue))
        {
            BrakeVoltageEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.BrakeVoltage), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        BrakeVoltageEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorBrakeReleaseTime()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.BrakeReleaseTime;
        if (!TryParseDouble(BrakeReleaseTimeEditor, oldValue, out var newValue))
        {
            BrakeReleaseTimeEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.BrakeReleaseTime), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        BrakeReleaseTimeEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorBrakeEngageTimeMov()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.BrakeEngageTimeMov;
        if (!TryParseDouble(BrakeEngageTimeMovEditor, oldValue, out var newValue))
        {
            BrakeEngageTimeMovEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.BrakeEngageTimeMov), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        BrakeEngageTimeMovEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorBrakeEngageTimeDiode()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.BrakeEngageTimeDiode;
        if (!TryParseDouble(BrakeEngageTimeDiodeEditor, oldValue, out var newValue))
        {
            BrakeEngageTimeDiodeEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.BrakeEngageTimeDiode), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        BrakeEngageTimeDiodeEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    public void EditMotorBrakeBacklash()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        var oldValue = CurrentMotor.BrakeBacklash;
        if (!TryParseDouble(BrakeBacklashEditor, oldValue, out var newValue))
        {
            BrakeBacklashEditor = oldValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return;
        }

        if (Math.Abs(oldValue - newValue) < 0.000001)
        {
            return;
        }

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(MotorDefinition.BrakeBacklash), oldValue, newValue);
        _undoStack.PushAndExecute(command);
        BrakeBacklashEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    /// <summary>
    /// Refreshes the available series collection from the selected voltage.
    /// </summary>
    private void RefreshAvailableSeries()
    {
        AvailableSeries.Clear();
        if (SelectedVoltage is not null)
        {
            foreach (var series in SelectedVoltage.Series)
            {
                AvailableSeries.Add(series);
            }
        }
    }

    /// <summary>
    /// Public method to refresh the available series collection.
    /// </summary>
    public void RefreshAvailableSeriesPublic()
    {
        RefreshAvailableSeries();
    }

    partial void OnCurrentMotorChanged(MotorDefinition? value)
    {
        // Refresh the drives collection
        RefreshAvailableDrives();

        // When motor changes, select the first drive
        SelectedDrive = value?.Drives.FirstOrDefault();

        // Update motor editor buffers from the current motor so that
        // the UI reflects the active document while ensuring that
        // subsequent edits flow through the command-based path.
        MotorNameEditor = value?.MotorName ?? string.Empty;
        ManufacturerEditor = value?.Manufacturer ?? string.Empty;
        PartNumberEditor = value?.PartNumber ?? string.Empty;

        if (value is null)
        {
            MaxSpeedEditor = string.Empty;
            RatedSpeedEditor = string.Empty;
            RatedPeakTorqueEditor = string.Empty;
            RatedContinuousTorqueEditor = string.Empty;
            PowerEditor = string.Empty;
            WeightEditor = string.Empty;
            RotorInertiaEditor = string.Empty;
            FeedbackPprEditor = string.Empty;
            HasBrakeEditor = false;
            BrakeTorqueEditor = string.Empty;
            BrakeAmperageEditor = string.Empty;
            BrakeVoltageEditor = string.Empty;
            BrakeReleaseTimeEditor = string.Empty;
            BrakeEngageTimeMovEditor = string.Empty;
            BrakeEngageTimeDiodeEditor = string.Empty;
            BrakeBacklashEditor = string.Empty;
            DriveNameEditor = string.Empty;
            DrivePartNumberEditor = string.Empty;
            DriveManufacturerEditor = string.Empty;
            VoltageValueEditor = string.Empty;
            VoltagePowerEditor = string.Empty;
            VoltageMaxSpeedEditor = string.Empty;
            VoltagePeakTorqueEditor = string.Empty;
            VoltageContinuousTorqueEditor = string.Empty;
            VoltageContinuousAmpsEditor = string.Empty;
            VoltagePeakAmpsEditor = string.Empty;
        }
        else
        {
            MaxSpeedEditor = value.MaxSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture);
            RatedSpeedEditor = value.RatedSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture);
            RatedPeakTorqueEditor = value.RatedPeakTorque.ToString(System.Globalization.CultureInfo.InvariantCulture);
            RatedContinuousTorqueEditor = value.RatedContinuousTorque.ToString(System.Globalization.CultureInfo.InvariantCulture);
            PowerEditor = value.Power.ToString(System.Globalization.CultureInfo.InvariantCulture);
            WeightEditor = value.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture);
            RotorInertiaEditor = value.RotorInertia.ToString(System.Globalization.CultureInfo.InvariantCulture);
            FeedbackPprEditor = value.FeedbackPpr.ToString(System.Globalization.CultureInfo.InvariantCulture);
            HasBrakeEditor = value.HasBrake;
            BrakeTorqueEditor = value.BrakeTorque.ToString(System.Globalization.CultureInfo.InvariantCulture);
            BrakeAmperageEditor = value.BrakeAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture);
            BrakeVoltageEditor = value.BrakeVoltage.ToString(System.Globalization.CultureInfo.InvariantCulture);
            BrakeReleaseTimeEditor = value.BrakeReleaseTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
            BrakeEngageTimeMovEditor = value.BrakeEngageTimeMov.ToString(System.Globalization.CultureInfo.InvariantCulture);
            BrakeEngageTimeDiodeEditor = value.BrakeEngageTimeDiode.ToString(System.Globalization.CultureInfo.InvariantCulture);
            BrakeBacklashEditor = value.BrakeBacklash.ToString(System.Globalization.CultureInfo.InvariantCulture);
            DriveNameEditor = SelectedDrive?.Name ?? string.Empty;
            DrivePartNumberEditor = SelectedDrive?.PartNumber ?? string.Empty;
            DriveManufacturerEditor = SelectedDrive?.Manufacturer ?? string.Empty;
            VoltageValueEditor = SelectedVoltage?.Voltage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltagePowerEditor = SelectedVoltage?.Power.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltageMaxSpeedEditor = SelectedVoltage?.MaxSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltagePeakTorqueEditor = SelectedVoltage?.RatedPeakTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltageContinuousTorqueEditor = SelectedVoltage?.RatedContinuousTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltageContinuousAmpsEditor = SelectedVoltage?.ContinuousAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltagePeakAmpsEditor = SelectedVoltage?.PeakAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        }
    }

    partial void OnSelectedDriveChanged(DriveConfiguration? value)
    {
        // Refresh the available voltages collection
        RefreshAvailableVoltages();

        if (value is null)
        {
            SelectedVoltage = null;
            return;
        }

        // Prefer 208V if available, otherwise use the first voltage
        var preferred = value.Voltages.FirstOrDefault(v => Math.Abs(v.Voltage - 208) < 0.1);
        SelectedVoltage = preferred ?? value.Voltages.FirstOrDefault();

        DriveNameEditor = value.Name ?? string.Empty;
        DrivePartNumberEditor = value.PartNumber ?? string.Empty;
        DriveManufacturerEditor = value.Manufacturer ?? string.Empty;
    }

    partial void OnSelectedVoltageChanged(VoltageConfiguration? value)
    {
        // Refresh the available series collection
        RefreshAvailableSeries();

        // When voltage changes, update series selection
        SelectedSeries = value?.Series.FirstOrDefault();

        // Update chart with new voltage configuration
        ChartViewModel.TorqueUnit = CurrentMotor?.Units.Torque ?? "Nm";
        ChartViewModel.MotorMaxSpeed = CurrentMotor?.MaxSpeed ?? 0;
        ChartViewModel.HasBrake = CurrentMotor?.HasBrake ?? false;
        ChartViewModel.BrakeTorque = CurrentMotor?.BrakeTorque ?? 0;
        ChartViewModel.CurrentVoltage = value;

        // Update data table with new voltage configuration
        CurveDataTableViewModel.CurrentVoltage = value;

        VoltageValueEditor = value?.Voltage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePowerEditor = value?.Power.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageMaxSpeedEditor = value?.MaxSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakTorqueEditor = value?.RatedPeakTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousTorqueEditor = value?.RatedContinuousTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousAmpsEditor = value?.ContinuousAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakAmpsEditor = value?.PeakAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private void OnChartDataChanged(object? sender, EventArgs e)
    {
        MarkDirty();
    }

    private void OnDataTableDataChanged(object? sender, EventArgs e)
    {
        MarkDirty();
        ChartViewModel.RefreshChart();
    }

    /// <summary>
    /// Undoes the most recent editing operation, if any.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _undoStack.Undo();
        RefreshMotorEditorsFromCurrentMotor();
        ChartViewModel.RefreshChart();
        CurveDataTableViewModel.RefreshData();
    }

    /// <summary>
    /// Re-applies the most recently undone editing operation, if any.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        _undoStack.Redo();
        RefreshMotorEditorsFromCurrentMotor();
        ChartViewModel.RefreshChart();
        CurveDataTableViewModel.RefreshData();
    }

    [RelayCommand]
    private async Task NewMotorAsync()
    {
        Log.Information("Creating new motor definition");

        if (!await ConfirmLoseUnsavedChangesOrCancelAsync("create a new file", "New file cancelled.").ConfigureAwait(true))
        {
            return;
        }

        // Create a new motor with default values
        CurrentMotor = _fileService.CreateNew(
            motorName: "New Motor",
            maxRpm: 5000,
            maxTorque: 50,
            maxPower: 1500);

        _undoStack.Clear();
        MarkCleanCheckpoint();

        IsDirty = _fileService.IsDirty;
        CurrentFilePath = _fileService.CurrentFilePath;
        await DirectoryBrowser.SyncSelectionToFilePathAsync(CurrentFilePath).ConfigureAwait(true);
        StatusMessage = "Created new motor definition";
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        Log.Information("Opening file dialog");

        try
        {
            if (!await ConfirmLoseUnsavedChangesOrCancelAsync("open another file", "Open cancelled.").ConfigureAwait(true))
            {
                return;
            }

            var storageProvider = GetStorageProvider();
            if (storageProvider is null)
            {
                StatusMessage = "File dialogs are not supported on this platform.";
                return;
            }

            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Motor Definition",
                AllowMultiple = false,
                FileTypeFilter = [JsonFileType]
            });

            if (files.Count == 0)
            {
                StatusMessage = "Open cancelled.";
                return;
            }

            var file = files[0];
            var filePath = file.Path.LocalPath;

            await OpenMotorFileInternalAsync(filePath, updateExplorerSelection: true).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanCloseFile))]
    private async Task CloseFileAsync()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        if (!await ConfirmLoseUnsavedChangesOrCancelAsync("close this file", "Close cancelled.").ConfigureAwait(true))
        {
            return;
        }

        CloseCurrentFileInternal();
        StatusMessage = "Closed file.";
    }

    private bool CanCloseFile() => CurrentMotor is not null;

    private void CloseCurrentFileInternal()
    {
        _fileService.Reset();

        _undoStack.Clear();
        MarkCleanCheckpoint();

        CurrentFilePath = null;
        CurrentMotor = null;
        SelectedDrive = null;
        SelectedVoltage = null;
        SelectedSeries = null;

        ChartViewModel.CurrentVoltage = null;
        CurveDataTableViewModel.CurrentVoltage = null;

        ValidationErrors = string.Empty;
        HasValidationErrors = false;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (CurrentMotor is null) return;

        // Validate before saving
        ValidateMotor();
        if (HasValidationErrors)
        {
            StatusMessage = "Cannot save: validation errors exist";
            return;
        }

        try
        {
            if (_fileService.CurrentFilePath is null)
            {
                // No file path yet, use SaveAs
                await SaveAsAsync();
                return;
            }

            await _fileService.SaveAsync(CurrentMotor);
            IsDirty = false;
            MarkCleanCheckpoint();
            StatusMessage = "File saved successfully";
            CurrentFilePath = _fileService.CurrentFilePath;
            OnPropertyChanged(nameof(WindowTitle));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save file");
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }

    private bool CanSave() => CurrentMotor is not null;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsAsync()
    {
        if (CurrentMotor is null) return;

        // Validate before saving
        ValidateMotor();
        if (HasValidationErrors)
        {
            StatusMessage = "Cannot save: validation errors exist";
            return;
        }

        try
        {
            var storageProvider = GetStorageProvider();
            if (storageProvider is null)
            {
                StatusMessage = "File dialogs are not supported on this platform.";
                return;
            }

            var suggestedFileName = !string.IsNullOrWhiteSpace(CurrentMotor.MotorName)
                ? $"{CurrentMotor.MotorName}.json"
                : "motor.json";

            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Motor Definition As",
                SuggestedFileName = suggestedFileName,
                DefaultExtension = "json",
                FileTypeChoices = [JsonFileType]
            });

            if (file is null)
            {
                StatusMessage = "Save cancelled.";
                return;
            }

            var filePath = file.Path.LocalPath;
            await _fileService.SaveAsAsync(CurrentMotor, filePath);
            IsDirty = false;
            MarkCleanCheckpoint();
            StatusMessage = $"Saved to: {Path.GetFileName(filePath)}";
            OnPropertyChanged(nameof(WindowTitle));

            _settingsStore.SaveString(DirectoryBrowserViewModel.LastOpenedMotorFileKey, filePath);
            CurrentFilePath = _fileService.CurrentFilePath;
            await DirectoryBrowser.SyncSelectionToFilePathAsync(filePath).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save file");
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }

    public async Task RestoreSessionAfterWindowOpenedAsync()
    {
        var restoreResult = await DirectoryBrowser.TryRestoreSessionAsync().ConfigureAwait(true);
        if (restoreResult == DirectoryBrowserViewModel.RestoreResult.MissingDirectory)
        {
            if (ActiveLeftPanelId == PanelRegistry.PanelIds.DirectoryBrowser)
            {
                ActiveLeftPanelId = PanelRegistry.PanelIds.CurveData;
            }
        }

        var lastFile = _settingsStore.LoadString(DirectoryBrowserViewModel.LastOpenedMotorFileKey);
        if (string.IsNullOrWhiteSpace(lastFile) || !File.Exists(lastFile))
        {
            return;
        }

        // Ensure the directory browser has a root that can contain the file so it can be highlighted.
        // This matters when a motor file is restored but the browser has no session (or a different root).
        var containingDirectory = Path.GetDirectoryName(lastFile);
        if (!string.IsNullOrWhiteSpace(containingDirectory))
        {
            var root = DirectoryBrowser.RootDirectoryPath;
            var isUnderRoot = false;
            if (!string.IsNullOrWhiteSpace(root))
            {
                try
                {
                    var fullFile = Path.GetFullPath(lastFile);
                    var fullRoot = Path.GetFullPath(root)
                        .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        + Path.DirectorySeparatorChar;
                    isUnderRoot = fullFile.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    isUnderRoot = false;
                }
            }

            if (string.IsNullOrWhiteSpace(root) || !isUnderRoot)
            {
                await DirectoryBrowser.SetRootDirectoryAsync(containingDirectory).ConfigureAwait(true);
            }
        }

        await OpenMotorFileInternalAsync(lastFile, updateExplorerSelection: true).ConfigureAwait(true);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveCopyAsAsync()
    {
        if (CurrentMotor is null) return;

        try
        {
            var storageProvider = GetStorageProvider();
            if (storageProvider is null)
            {
                StatusMessage = "File dialogs are not supported on this platform.";
                return;
            }

            var suggestedFileName = !string.IsNullOrWhiteSpace(CurrentMotor.MotorName)
                ? $"{CurrentMotor.MotorName}_copy.json"
                : "motor_copy.json";

            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Copy As",
                SuggestedFileName = suggestedFileName,
                DefaultExtension = "json",
                FileTypeChoices = [JsonFileType]
            });

            if (file is null)
            {
                StatusMessage = "Save copy cancelled.";
                return;
            }

            var filePath = file.Path.LocalPath;
            await _fileService.SaveCopyAsAsync(CurrentMotor, filePath);
            StatusMessage = $"Copy saved to: {Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save copy");
            StatusMessage = $"Error saving copy: {ex.Message}";
        }
    }

    /// <summary>
    /// Shows the Add Drive/Voltage dialog and creates the drive with curve series.
    /// </summary>
    [RelayCommand]
    private async Task AddDriveAsync()
    {
        await AddDriveInternalAsync();
    }

    [RelayCommand]
    private async Task RemoveDriveAsync()
    {
        if (CurrentMotor is null || SelectedDrive is null) return;

        // Show confirmation dialog
        var dialog = new Views.MessageDialog
        {
            Title = "Confirm Delete",
            Message = $"Are you sure you want to delete the selected drive '{SelectedDrive.Name}'?"
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await dialog.ShowDialog(desktop.MainWindow!);
        }

        if (!dialog.IsConfirmed) return;

        try
        {
            var driveName = SelectedDrive.Name;
            if (CurrentMotor.RemoveDrive(driveName))
            {
                RefreshAvailableDrives();
                SelectedDrive = CurrentMotor.Drives.FirstOrDefault();
                MarkDirty();
                StatusMessage = $"Removed drive: {driveName}";
            }
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to remove drive");
            StatusMessage = $"Error removing drive: {ex.Message}";
        }
    }

    // Common voltage values used in industrial motor applications
    private static readonly double[] CommonVoltages = [110, 115, 120, 200, 208, 220, 230, 240, 277, 380, 400, 415, 440, 460, 480, 500, 575, 600, 690];

    /// <summary>
    /// Shows the Add Drive/Voltage dialog for adding a new voltage configuration.
    /// </summary>
    [RelayCommand]
    private async Task AddVoltageAsync()
    {
        await AddVoltageInternalAsync();
    }

    [RelayCommand]
    private async Task RemoveVoltageAsync()
    {
        if (SelectedDrive is null || SelectedVoltage is null) return;

        // Show confirmation dialog
        var dialog = new Views.MessageDialog
        {
            Title = "Confirm Delete",
            Message = $"Are you sure you want to delete the selected voltage '{SelectedVoltage.Voltage}V'?"
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await dialog.ShowDialog(desktop.MainWindow!);
        }

        if (!dialog.IsConfirmed) return;

        try
        {
            if (SelectedDrive.Voltages.Count <= 1)
            {
                StatusMessage = "Cannot remove the last voltage configuration.";
                return;
            }

            var voltage = SelectedVoltage.Voltage;
            SelectedDrive.Voltages.Remove(SelectedVoltage);
            RefreshAvailableVoltages();
            SelectedVoltage = SelectedDrive.Voltages.FirstOrDefault();
            MarkDirty();
            StatusMessage = $"Removed voltage: {voltage}V";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to remove voltage");
            StatusMessage = $"Error removing voltage: {ex.Message}";
        }
    }

    // Series management commands
    [RelayCommand]
    private async Task AddSeriesAsync()
    {
        await AddSeriesInternalAsync();
    }

    /// <summary>
    /// Core workflow for adding a new drive with an initial voltage configuration.
    /// Kept internal to simplify future extraction into a dedicated workflow service.
    /// </summary>
    private async Task AddDriveInternalAsync()
    {
        if (CurrentMotor is null)
        {
            return;
        }

        try
        {
            var dialog = new Views.AddDriveVoltageDialog();
            dialog.Initialize(
                CurrentMotor.MaxSpeed,
                CurrentMotor.RatedPeakTorque,
                CurrentMotor.RatedContinuousTorque,
                CurrentMotor.Power);

            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow is null)
            {
                StatusMessage = "Cannot show dialog - no main window available";
                return;
            }

            await dialog.ShowDialog(desktop.MainWindow);
            if (dialog.Result is null)
            {
                return;
            }

            var result = dialog.Result;

            var (drive, _) = _motorConfigurationWorkflow.CreateDriveWithVoltage(CurrentMotor, result);

            // Add the new drive directly to the collection (don't clear/refresh)
            AvailableDrives.Add(drive);

            // Select the new drive - this will trigger OnSelectedDriveChanged which updates voltages
            SelectedDrive = drive;

            // Explicitly refresh chart to ensure axes are updated with new max speed
            ChartViewModel.RefreshChart();

            MarkDirty();
            StatusMessage = $"Added drive: {drive.Name}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add drive");
            StatusMessage = $"Error adding drive: {ex.Message}";
        }
    }

    /// <summary>
    /// Core workflow for adding a new voltage configuration to the selected drive.
    /// </summary>
    private async Task AddVoltageInternalAsync()
    {
        if (SelectedDrive is null)
        {
            return;
        }

        try
        {
            var dialog = new Views.AddDriveVoltageDialog
            {
                Title = "Add New Voltage Configuration"
            };

            dialog.Initialize(
                CurrentMotor?.MaxSpeed ?? 5000,
                CurrentMotor?.RatedPeakTorque ?? 50,
                CurrentMotor?.RatedContinuousTorque ?? 40,
                CurrentMotor?.Power ?? 1500);

            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow is null)
            {
                StatusMessage = "Cannot show dialog - no main window available";
                return;
            }

            await dialog.ShowDialog(desktop.MainWindow);
            if (dialog.Result is null)
            {
                return;
            }

            var result = dialog.Result;

            // Delegate duplicate check and creation to the workflow
            var voltageResult = _motorConfigurationWorkflow.CreateVoltageWithSeries(SelectedDrive, result);
            if (voltageResult.IsDuplicate)
            {
                StatusMessage = $"Voltage {result.Voltage}V already exists for this drive.";
                return;
            }

            var voltage = voltageResult.Voltage;

            // Refresh the available voltages and select the new one
            RefreshAvailableVoltages();
            SelectedVoltage = voltage;

            // Force chart refresh to update axes based on new max speed
            ChartViewModel.RefreshChart();

            MarkDirty();
            StatusMessage = $"Added voltage: {result.Voltage}V";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add voltage");
            StatusMessage = $"Error adding voltage: {ex.Message}";
        }
    }

    /// <summary>
    /// Core workflow for adding a new curve series to the selected voltage.
    /// </summary>
    private async Task AddSeriesInternalAsync()
    {
        if (SelectedVoltage is null)
        {
            return;
        }

        try
        {
            var dialog = new Views.AddCurveSeriesDialog();
            dialog.Initialize(
                SelectedVoltage.MaxSpeed,
                SelectedVoltage.RatedContinuousTorque,
                SelectedVoltage.Power);

            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow is null)
            {
                StatusMessage = "Cannot show dialog - no main window available";
                return;
            }

            await dialog.ShowDialog(desktop.MainWindow);
            if (dialog.Result is null)
            {
                return;
            }

            // Re-check SelectedVoltage in case it changed during async operation
            if (SelectedVoltage is null)
            {
                StatusMessage = "No voltage selected";
                return;
            }

            var result = dialog.Result;

            var series = _motorConfigurationWorkflow.CreateSeries(SelectedVoltage, result);

            // IMPORTANT: RefreshData BEFORE RefreshAvailableSeries to prevent DataGrid column sync issues
            // The column rebuild is triggered by AvailableSeries collection change, so data must be ready first
            CurveDataTableViewModel.RefreshData();
            RefreshAvailableSeries();
            SelectedSeries = series;
            ChartViewModel.RefreshChart();
            MarkDirty();
            StatusMessage = $"Added series: {series.Name}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add series");
            StatusMessage = $"Error adding series: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RemoveSeriesAsync()
    {
        if (SelectedVoltage is null || SelectedSeries is null) return;

        // Show confirmation dialog
        var dialog = new Views.MessageDialog
        {
            Title = "Confirm Delete",
            Message = $"Are you sure you want to delete the selected series '{SelectedSeries.Name}'?"
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await dialog.ShowDialog(desktop.MainWindow!);
        }

        if (!dialog.IsConfirmed) return;

        try
        {
            if (SelectedVoltage.Series.Count <= 1)
            {
                StatusMessage = "Cannot remove the last series.";
                return;
            }

            var seriesName = SelectedSeries.Name;
            SelectedVoltage.Series.Remove(SelectedSeries);
            RefreshAvailableSeries();
            SelectedSeries = SelectedVoltage.Series.FirstOrDefault();
            MarkDirty();
            StatusMessage = $"Removed series: {seriesName}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to remove series");
            StatusMessage = $"Error removing series: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ToggleSeriesLock(CurveSeries? series)
    {
        if (series is null)
        {
            return;
        }

        var newLocked = !series.Locked;

        var command = new EditSeriesCommand(series, newLocked: newLocked);
        _undoStack.PushAndExecute(command);
        UpdateDirtyFromUndoDepth();

        // Refresh the curve data table so that the DataGrid columns
        // are rebuilt with the correct read-only state for the
        // affected series. This keeps the editor behavior and
        // header lock icon in sync with the model state.
        CurveDataTableViewModel.RefreshData();

        StatusMessage = newLocked
            ? $"Locked series: {series.Name}"
            : $"Unlocked series: {series.Name}";
    }

    /// <summary>
    /// Toggles the visibility of a series on the chart.
    /// </summary>
    /// <param name="series">The series to toggle visibility for.</param>
    [RelayCommand]
    private void ToggleSeriesVisibility(CurveSeries? series)
    {
        if (series is null) return;

        series.IsVisible = !series.IsVisible;
        ChartViewModel.SetSeriesVisibility(series.Name, series.IsVisible);
        OnPropertyChanged(nameof(AvailableSeries));
        StatusMessage = series.IsVisible
            ? $"Showing series: {series.Name}"
            : $"Hiding series: {series.Name}";
    }

    [RelayCommand]
    private static void Exit()
    {
        Log.Information("Exiting application");

        // Use Avalonia's proper shutdown mechanism
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    /// <summary>
    /// Called when any property on the motor changes.
    /// </summary>
    public void MarkDirty()
    {
        _fileService.MarkDirty();
        IsDirty = true;
        ValidateMotor();
    }

    /// <summary>
    /// Validates the current motor definition and updates validation state.
    /// </summary>
    private void ValidateMotor()
    {
        if (CurrentMotor is null)
        {
            HasValidationErrors = false;
            ValidationErrors = string.Empty;
            return;
        }

        var errors = _validationService.ValidateMotorDefinition(CurrentMotor);

        if (errors.Count > 0)
        {
            Log.Information("Validation failed for motor {MotorName} with {ErrorCount} errors", CurrentMotor.MotorName, errors.Count);
            foreach (var error in errors)
            {
                Log.Debug("Validation error for motor {MotorName}: {ErrorMessage}", CurrentMotor.MotorName, error);
            }
        }
        else
        {
            Log.Debug("Validation succeeded for motor {MotorName}", CurrentMotor.MotorName);
        }

        HasValidationErrors = errors.Count > 0;
        ValidationErrors = errors.Count > 0
            ? string.Join("\n", errors)
            : string.Empty;
    }

    /// <summary>
    /// Shows a confirmation dialog for max speed change.
    /// </summary>
    public async Task<bool> ConfirmMaxSpeedChangeAsync()
    {
        var dialog = new Views.MessageDialog
        {
            Title = "Confirm Max Speed Change",
            Message = "Changing the maximum speed will affect existing curve data. " +
                      "The curve data points are based on speed percentages, so changing the " +
                      "maximum speed will shift the RPM values of all data points.\n\n" +
                      "Do you want to proceed with this change?"
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await dialog.ShowDialog(desktop.MainWindow!);
        }

        return dialog.IsConfirmed;
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.StorageProvider;
        }
        return null;
    }

}
