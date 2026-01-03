using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;
using MotorEditor.Avalonia.Models;
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
    private readonly UnitConversionService _unitConversionService;
    private readonly UnitPreferencesService _unitPreferencesService;
    private readonly IRecentFilesService _recentFilesService;

    // Track previous units for conversion
    private string? _previousTorqueUnit;
    private string? _previousSpeedUnit;
    private string? _previousPowerUnit;
    private string? _previousWeightUnit;
    private string? _previousInertiaUnit;
    private string? _previousCurrentUnit;
    private string? _previousResponseTimeUnit;
    private string? _previousBacklashUnit;

    // Tab management
    private readonly ObservableCollection<DocumentTab> _tabs = new();
    private DocumentTab? _activeTab;

    // During ActiveTab switches, Avalonia ComboBox can temporarily rebind ItemsSource/SelectedItem
    // and push null back through a TwoWay SelectedItem binding. If we accept that null, we destroy
    // the per-tab selection state. We suppress selection setters only while the tab change pipeline
    // is notifying dependent properties.
    private bool _suppressSelectionWriteBack;

    // ComboBox display selections are decoupled from per-tab state.
    // Reason: Avalonia may clear ComboBox.SelectedItem during ItemsSource rebinding while switching tabs.
    // We suppress the null write-back to preserve tab state, but the control can remain visually blank if
    // the binding engine decides the source value "didn't change" (same reference) and doesn't push it back.
    // These properties can be pulsed null -> actual to force the display to refresh.
    private Drive? _selectedDriveForDisplay;
    public Drive? SelectedDriveForDisplay
    {
        get => _selectedDriveForDisplay;
        set
        {
            if (!ReferenceEquals(_selectedDriveForDisplay, value))
            {
                _selectedDriveForDisplay = value;
                OnPropertyChanged();
            }

            // Only propagate user changes when we're not in the middle of a tab switch.
            if (!_suppressSelectionWriteBack && ActiveTab is not null && ActiveTab.SelectedDrive != value)
            {
                SelectedDrive = value;
            }
        }
    }

    private Voltage? _selectedVoltageForDisplay;
    public Voltage? SelectedVoltageForDisplay
    {
        get => _selectedVoltageForDisplay;
        set
        {
            if (!ReferenceEquals(_selectedVoltageForDisplay, value))
            {
                _selectedVoltageForDisplay = value;
                OnPropertyChanged();
            }

            // Only propagate user changes when we're not in the middle of a tab switch.
            if (!_suppressSelectionWriteBack && ActiveTab is not null && ActiveTab.SelectedVoltage != value)
            {
                SelectedVoltage = value;
            }
        }
    }

    private static readonly FilePickerFileType JsonFileType = new("JSON Files")
    {
        Patterns = ["*.json"],
        MimeTypes = ["application/json"]
    };

    /// <summary>
    /// Current motor definition (delegates to active tab).
    /// </summary>
    public ServoMotor? CurrentMotor
    {
        get => ActiveTab?.Motor;
        set
        {
            if (ActiveTab != null && ActiveTab.Motor != value)
            {
                ActiveTab.Motor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowTitle));
                OnCurrentMotorChanged(value);
            }
        }
    }

    /// <summary>
    /// Whether the current document has unsaved changes (delegates to active tab).
    /// </summary>
    public bool IsDirty
    {
        get => ActiveTab?.IsDirty ?? false;
        set
        {
            if (ActiveTab != null && ActiveTab.IsDirty != value)
            {
                ActiveTab.IsDirty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowTitle));
                OnIsDirtyChanged(value);
            }
        }
    }

    private void OnIsDirtyChanged(bool value)
    {
        DirectoryBrowser.UpdateOpenFileStates(CurrentFilePath, GetDirtyFilePaths());
    }

    [ObservableProperty]
    private string _statusMessage = "Ready";

    /// <summary>
    /// Current validation errors for the motor definition.
    /// </summary>
    [ObservableProperty]
    private string _validationErrors = string.Empty;

    /// <summary>
    /// List of validation errors for display in the validation panel.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _validationErrorsList = new();

    /// <summary>
    /// Whether there are validation errors.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveWithValidation))]
    private bool _hasValidationErrors;

    /// <summary>
    /// Selected drive (delegates to active tab).
    /// </summary>
    public Drive? SelectedDrive
    {
        get => ActiveTab?.SelectedDrive;
        set
        {
            if (_suppressSelectionWriteBack)
            {
                Log.Debug(
                    "[SELECTION] SelectedDrive setter suppressed during tab change: Tab={Tab} FilePath={FilePath} RequestedDrive={RequestedDrive}",
                    ActiveTab?.DisplayName,
                    ActiveTab?.FilePath,
                    value?.Name);
                return;
            }

            if (ActiveTab != null && ActiveTab.SelectedDrive != value)
            {
                var oldDrive = ActiveTab.SelectedDrive;

                Log.Information(
                    "[SELECTION] SelectedDrive setter: Tab={Tab} FilePath={FilePath} OldDrive={OldDrive} NewDrive={NewDrive} AvailableDrives={AvailableDrives}",
                    ActiveTab.DisplayName,
                    ActiveTab.FilePath,
                    oldDrive?.Name,
                    value?.Name,
                    ActiveTab.AvailableDrives.Count);

                ActiveTab.SelectedDrive = value;
                // Don't call OnPropertyChanged() here - it causes the binding system to re-evaluate
                // with stale values, which triggers the setter again with null, destroying tab state
                OnSelectedDriveChanged(value);

                // Notify AFTER dependent collections and editor buffers are updated.
                // This keeps ComboBox SelectedItem and IsVisible bindings in sync.
                NotifySelectionRelatedPropertiesChanged(selectionChanged: true, voltageChanged: true, seriesChanged: true);

                // Keep ComboBox display selection in sync with tab state.
                _selectedDriveForDisplay = ActiveTab.SelectedDrive;
                OnPropertyChanged(nameof(SelectedDriveForDisplay));
                _selectedVoltageForDisplay = ActiveTab.SelectedVoltage;
                OnPropertyChanged(nameof(SelectedVoltageForDisplay));
            }
            else
            {
                Log.Debug(
                    "[SELECTION] SelectedDrive setter ignored: ActiveTabNull={ActiveTabNull} SameValue={SameValue}",
                    ActiveTab is null,
                    ActiveTab is not null && ReferenceEquals(ActiveTab.SelectedDrive, value));
            }
        }
    }

    /// <summary>
    /// Selected voltage (delegates to active tab).
    /// </summary>
    public Voltage? SelectedVoltage
    {
        get => ActiveTab?.SelectedVoltage;
        set
        {
            if (_suppressSelectionWriteBack)
            {
                Log.Debug(
                    "[SELECTION] SelectedVoltage setter suppressed during tab change: Tab={Tab} FilePath={FilePath} RequestedVoltage={RequestedVoltage} RequestedDrive={DriveName}",
                    ActiveTab?.DisplayName,
                    ActiveTab?.FilePath,
                    value?.Value,
                    ActiveTab?.SelectedDrive?.Name);
                return;
            }

            if (ActiveTab != null && ActiveTab.SelectedVoltage != value)
            {
                var oldVoltage = ActiveTab.SelectedVoltage;

                Log.Information(
                    "[SELECTION] SelectedVoltage setter: Tab={Tab} FilePath={FilePath} OldVoltage={OldVoltage} NewVoltage={NewVoltage} Drive={DriveName} AvailableVoltages={AvailableVoltages}",
                    ActiveTab.DisplayName,
                    ActiveTab.FilePath,
                    oldVoltage?.Value,
                    value?.Value,
                    ActiveTab.SelectedDrive?.Name,
                    ActiveTab.AvailableVoltages.Count);

                ActiveTab.SelectedVoltage = value;
                // Don't call OnPropertyChanged() here - it causes the binding system to re-evaluate
                // with stale values, which triggers the setter again with null, destroying tab state
                OnSelectedVoltageChanged(value);

                // Notify AFTER dependent collections and editor buffers are updated.
                NotifySelectionRelatedPropertiesChanged(selectionChanged: false, voltageChanged: true, seriesChanged: true);

                // Keep ComboBox display selection in sync with tab state.
                _selectedVoltageForDisplay = ActiveTab.SelectedVoltage;
                OnPropertyChanged(nameof(SelectedVoltageForDisplay));
            }
            else
            {
                Log.Debug(
                    "[SELECTION] SelectedVoltage setter ignored: ActiveTabNull={ActiveTabNull} SameValue={SameValue}",
                    ActiveTab is null,
                    ActiveTab is not null && ReferenceEquals(ActiveTab.SelectedVoltage, value));
            }
        }
    }

    private void NotifySelectionRelatedPropertiesChanged(bool selectionChanged, bool voltageChanged, bool seriesChanged)
    {
        // Note: AvailableDrives/AvailableVoltages/AvailableSeries are ObservableCollections.
        // Their *contents* update via collection change notifications.
        // We still raise property-changed for the Selected* properties so other bindings
        // (e.g., IsVisible, SelectedItem display) update immediately.

        if (selectionChanged)
        {
            OnPropertyChanged(nameof(SelectedDrive));
        }

        if (voltageChanged)
        {
            OnPropertyChanged(nameof(SelectedVoltage));
        }

        if (seriesChanged)
        {
            OnPropertyChanged(nameof(SelectedSeries));
        }
    }

    /// <summary>
    /// Selected series/curve (delegates to active tab).
    /// </summary>
    public Curve? SelectedSeries
    {
        get => ActiveTab?.SelectedSeries;
        set
        {
            if (ActiveTab != null && ActiveTab.SelectedSeries != value)
            {
                ActiveTab.SelectedSeries = value;
                // Don't call OnPropertyChanged() here - it causes the binding system to re-evaluate
                // with stale values, which triggers the setter again with null, destroying tab state
            }
        }
    }

    /// <summary>
    /// Editing coordinator (delegates to active tab).
    /// </summary>
    public EditingCoordinator? EditingCoordinator => ActiveTab?.EditingCoordinator;

    /// <summary>
    /// Chart view model (delegates to active tab).
    /// </summary>
    public ChartViewModel? ChartViewModel => ActiveTab?.ChartViewModel;

    /// <summary>
    /// Curve data table view model (delegates to active tab).
    /// </summary>
    public CurveDataTableViewModel? CurveDataTableViewModel => ActiveTab?.CurveDataTableViewModel;

    /// <summary>
    /// ViewModel for the Directory Browser explorer panel.
    /// </summary>
    [ObservableProperty]
    private DirectoryBrowserViewModel _directoryBrowser = new();

    /// <summary>
    /// Gets the observable list of recent file paths, ordered from most recent to oldest.
    /// </summary>
    public ReadOnlyObservableCollection<string> RecentFiles => _recentFilesService.RecentFiles;

    /// <summary>
    /// Current file path (delegates to active tab).
    /// </summary>
    public string? CurrentFilePath
    {
        get => ActiveTab?.FilePath;
        set
        {
            if (ActiveTab != null && ActiveTab.FilePath != value)
            {
                ActiveTab.FilePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowTitle));
                OnCurrentFilePathChanged(value);
            }
        }
    }

    private void OnCurrentFilePathChanged(string? value)
    {
        _ = DirectoryBrowser.SyncSelectionToFilePathAsync(value);
        DirectoryBrowser.UpdateOpenFileStates(value, GetDirtyFilePaths());
    }

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
    /// Whether the bottom panel is expanded.
    /// Defaults to collapsed and is not persisted between sessions.
    /// </summary>
    [ObservableProperty]
    private bool _isBottomPanelExpanded;

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
    /// Toggles the bottom panel visibility.
    /// </summary>
    [RelayCommand]
    private void ToggleBottomPanel()
    {
        TogglePanel(PanelRegistry.PanelIds.BottomPanel);
    }

    /// <summary>
    /// Refreshes the validation for the current motor.
    /// </summary>
    [RelayCommand]
    private void RefreshValidation()
    {
        ValidateMotor();
    }

    /// <summary>
    /// Toggles the power curves overlay on/off application-wide.
    /// This affects all open tabs and newly opened files.
    /// </summary>
    [RelayCommand]
    private void ToggleShowPowerCurves()
    {
        var chartViewModel = ChartViewModel;
        if (chartViewModel == null)
        {
            return;
        }

        // Toggle the state
        var newState = !chartViewModel.ShowPowerCurves;
        
        // Apply to all open tabs
        foreach (var tab in Tabs)
        {
            if (tab.ChartViewModel != null)
            {
                tab.ChartViewModel.ShowPowerCurves = newState;
            }
        }
        
        // Save preference for future tabs
        _settingsStore.SaveBool("ShowPowerCurves", newState);
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
                // Bottom zone toggle
                // For now, we only have one bottom panel, so just toggle it
                if (panelId == PanelRegistry.PanelIds.BottomPanel)
                {
                    IsBottomPanelExpanded = !IsBottomPanelExpanded;
                }
                break;

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
    /// Available voltages for the selected drive (delegates to active tab).
    /// </summary>
    public ObservableCollection<Voltage> AvailableVoltages => ActiveTab?.AvailableVoltages ?? [];

    /// <summary>
    /// Available series for the selected voltage (delegates to active tab).
    /// </summary>
    public ObservableCollection<Curve> AvailableSeries => ActiveTab?.AvailableSeries ?? [];

    /// <summary>
    /// Available drives from current motor definition (delegates to active tab).
    /// </summary>
    public ObservableCollection<Drive> AvailableDrives => ActiveTab?.AvailableDrives ?? [];

    /// <summary>
    /// Collection of all open document tabs.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DocumentTab> tabs;

    /// <summary>
    /// The currently active document tab.
    /// </summary>
    public DocumentTab? ActiveTab
    {
        get => _activeTab;
        set
        {
            if (_activeTab != value)
            {
                _activeTab = value;
                OnPropertyChanged();
                OnActiveTabChanged();
            }
        }
    }

    /// <summary>
    /// Gets whether there is at least one operation to undo.
    /// </summary>
    public bool CanUndo => ActiveTab?.UndoStack.CanUndo ?? false;

    /// <summary>
    /// Gets whether there is at least one operation to redo.
    /// </summary>
    public bool CanRedo => ActiveTab?.UndoStack.CanRedo ?? false;

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
            var logDirectory = Program.GetLogDirectory();

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
        var chartViewModel = new ChartViewModel();
        var curveDataTableViewModel = new CurveDataTableViewModel();
        var editingCoordinator = new EditingCoordinator();
        _motorConfigurationWorkflow = new MotorConfigurationWorkflow(_driveVoltageSeriesService);
        _settingsStore = new PanelLayoutUserSettingsStore();
        _recentFilesService = new RecentFilesService(_settingsStore);
        UnsavedChangesPromptAsync = ShowUnsavedChangesPromptAsync;
        
        // Initialize unit services
        _unitPreferencesService = new UnitPreferencesService(_settingsStore);
        _unitConversionService = new UnitConversionService(_settingsStore);
        // Use Convert Stored Data mode (hard conversion) as per requirements
        _unitConversionService.ConvertStoredData = true;
        _unitConversionService.DisplayDecimalPlaces = _unitPreferencesService.GetDecimalPlaces();
        
        // Initialize tabs (currently with single shared view models for backward compatibility)
        var initialTab = new DocumentTab
        {
            ChartViewModel = chartViewModel,
            CurveDataTableViewModel = curveDataTableViewModel,
            EditingCoordinator = editingCoordinator
        };
        WireTabIntegration(initialTab);
        _tabs.Add(initialTab);
        ActiveTab = initialTab;
        Tabs = _tabs;
        
        WireEditingCoordinator();
        WireUndoInfrastructure();
        WireDirectoryBrowserIntegration();

        // Load saved power curves preference
        chartViewModel.ShowPowerCurves = _settingsStore.LoadBool("ShowPowerCurves", false);
    }

    public MainWindowViewModel(IFileService fileService, ICurveGeneratorService curveGeneratorService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _curveGeneratorService = curveGeneratorService ?? throw new ArgumentNullException(nameof(curveGeneratorService));
        _validationService = new ValidationService();
        _driveVoltageSeriesService = new DriveVoltageSeriesService();
        var chartViewModel = new ChartViewModel();
        var curveDataTableViewModel = new CurveDataTableViewModel();
        var editingCoordinator = new EditingCoordinator();
        _motorConfigurationWorkflow = new MotorConfigurationWorkflow(_driveVoltageSeriesService);
        _settingsStore = new PanelLayoutUserSettingsStore();
        _recentFilesService = new RecentFilesService(_settingsStore);
        UnsavedChangesPromptAsync = ShowUnsavedChangesPromptAsync;
        
        // Initialize unit services
        _unitPreferencesService = new UnitPreferencesService(_settingsStore);
        _unitConversionService = new UnitConversionService(_settingsStore);
        // Use Convert Stored Data mode (hard conversion) as per requirements
        _unitConversionService.ConvertStoredData = true;
        _unitConversionService.DisplayDecimalPlaces = _unitPreferencesService.GetDecimalPlaces();
        
        // Initialize tabs (currently with single shared view models for backward compatibility)
        var initialTab = new DocumentTab
        {
            ChartViewModel = chartViewModel,
            CurveDataTableViewModel = curveDataTableViewModel,
            EditingCoordinator = editingCoordinator
        };
        WireTabIntegration(initialTab);
        _tabs.Add(initialTab);
        ActiveTab = initialTab;
        Tabs = _tabs;
        
        WireEditingCoordinator();
        WireUndoInfrastructure();
        WireDirectoryBrowserIntegration();

        // Load saved power curves preference
        chartViewModel.ShowPowerCurves = _settingsStore.LoadBool("ShowPowerCurves", false);
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
        _settingsStore = settingsStore ?? new PanelLayoutUserSettingsStore();
        _recentFilesService = new RecentFilesService(_settingsStore);
        UnsavedChangesPromptAsync = unsavedChangesPromptAsync ?? ShowUnsavedChangesPromptAsync;

        // Initialize unit services
        _unitPreferencesService = new UnitPreferencesService(_settingsStore);
        _unitConversionService = new UnitConversionService(_settingsStore);
        // Use Convert Stored Data mode (hard conversion) as per requirements
        _unitConversionService.ConvertStoredData = true;
        _unitConversionService.DisplayDecimalPlaces = _unitPreferencesService.GetDecimalPlaces();

        // Initialize tabs (currently with single shared view models for backward compatibility)
        var editingCoordinator = new EditingCoordinator();
        var initialTab = new DocumentTab
        {
            ChartViewModel = chartViewModel,
            CurveDataTableViewModel = curveDataTableViewModel,
            EditingCoordinator = editingCoordinator
        };
        WireTabIntegration(initialTab);
        _tabs.Add(initialTab);
        ActiveTab = initialTab;
        Tabs = _tabs;

        WireEditingCoordinator();
        WireUndoInfrastructure();
        WireDirectoryBrowserIntegration();

        // Load saved power curves preference
        ChartViewModel!.ShowPowerCurves = _settingsStore.LoadBool("ShowPowerCurves", false);
    }

    private void WireDirectoryBrowserIntegration()
    {
        DirectoryBrowser.FileOpenRequested -= HandleDirectoryBrowserFileOpenRequestedAsync;
        DirectoryBrowser.FileOpenRequested += HandleDirectoryBrowserFileOpenRequestedAsync;

        DirectoryBrowser.PropertyChanged -= OnDirectoryBrowserPropertyChanged;
        DirectoryBrowser.PropertyChanged += OnDirectoryBrowserPropertyChanged;

        CurrentFilePath = _fileService.CurrentFilePath;
        DirectoryBrowser.UpdateOpenFileStates(CurrentFilePath, GetDirtyFilePaths());
    }

    /// <summary>
    /// Creates a new document tab with initialized view models and wiring.
    /// </summary>
    private DocumentTab CreateNewTab()
    {
        Log.Information("[TAB] CreateNewTab() - Creating new tab");
        var tab = new DocumentTab
        {
            ChartViewModel = new ChartViewModel(),
            CurveDataTableViewModel = new CurveDataTableViewModel(),
            EditingCoordinator = new EditingCoordinator()
        };

        // Wire up the view models
        tab.ChartViewModel.EditingCoordinator = tab.EditingCoordinator;
        tab.CurveDataTableViewModel.EditingCoordinator = tab.EditingCoordinator;
        tab.ChartViewModel.UndoStack = tab.UndoStack;
        tab.CurveDataTableViewModel.UndoStack = tab.UndoStack;

        // Load saved power curves preference
        tab.ChartViewModel.ShowPowerCurves = _settingsStore.LoadBool("ShowPowerCurves", false);

        tab.ChartViewModel.DataChanged += (s, e) => tab.MarkDirty();
        tab.CurveDataTableViewModel.DataChanged += (s, e) =>
        {
            tab.MarkDirty();
            tab.ChartViewModel?.RefreshChart();
        };

        tab.UndoStack.UndoStackChanged += (s, e) =>
        {
            tab.UpdateDirtyFromUndoDepth();
            if (tab == ActiveTab)
            {
                OnPropertyChanged(nameof(CanUndo));
                OnPropertyChanged(nameof(CanRedo));
            }
        };

        WireTabIntegration(tab);

        Log.Information("[TAB] CreateNewTab() - Tab created");
        return tab;
    }

    private void WireTabIntegration(DocumentTab tab)
    {
        ArgumentNullException.ThrowIfNull(tab);

        tab.PropertyChanged -= OnTabPropertyChanged;
        tab.PropertyChanged += OnTabPropertyChanged;
    }

    private void UnwireTabIntegration(DocumentTab tab)
    {
        ArgumentNullException.ThrowIfNull(tab);
        tab.PropertyChanged -= OnTabPropertyChanged;
    }

    private void OnTabPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not DocumentTab tab)
        {
            return;
        }

        // Edits typically mark the DocumentTab dirty directly (tab.MarkDirty()), bypassing the
        // MainWindowViewModel.IsDirty setter. Keep Directory Browser dirty indicators in sync.
        if (e.PropertyName is nameof(DocumentTab.IsDirty) or nameof(DocumentTab.FilePath))
        {
            if (tab == ActiveTab)
            {
                OnPropertyChanged(nameof(IsDirty));
                OnPropertyChanged(nameof(CurrentFilePath));
                OnPropertyChanged(nameof(WindowTitle));
            }

            DirectoryBrowser.UpdateOpenFileStates(CurrentFilePath, GetDirtyFilePaths());
        }
    }

    /// <summary>
    /// Initializes a tab's collections and selections after a motor has been loaded into it.
    /// This method should be called AFTER the tab is set as ActiveTab to ensure proper UI binding updates.
    /// </summary>
    private void InitializeActiveTabWithMotor()
    {
        Log.Information(
            "[INIT] InitializeActiveTabWithMotor() - START Tab={Tab} FilePath={FilePath}",
            ActiveTab?.DisplayName,
            ActiveTab?.FilePath);
        if (ActiveTab?.Motor == null)
        {
            Log.Information("[INIT] InitializeActiveTabWithMotor() - ActiveTab or Motor is null, returning");
            return;
        }

        Log.Information(
            "[INIT] Motor loaded into tab: MotorName={MotorName} DriveCount={DriveCount}",
            ActiveTab.Motor.MotorName,
            ActiveTab.Motor.Drives.Count);
        
        // Populate AvailableDrives from the motor (this works through the delegating property)
        Log.Debug("[INIT] Clearing AvailableDrives (before={Count})", AvailableDrives.Count);
        AvailableDrives.Clear();
        foreach (var drive in ActiveTab.Motor.Drives)
        {
            AvailableDrives.Add(drive);
            Log.Debug("[INIT] Added drive: DriveName={DriveName}", drive.Name);
        }

        Log.Information("[INIT] AvailableDrives populated: Count={Count}", AvailableDrives.Count);
        Log.Information("[INIT] Tab.SelectedDrive BEFORE auto-select: {SelectedDrive}", ActiveTab?.SelectedDrive?.Name);
        
        // Select the first drive - this will trigger OnSelectedDriveChanged which will:
        // - Refresh AvailableVoltages
        // - Auto-select preferred voltage
        // - Update chart and editor fields
        if (AvailableDrives.Count > 0)
        {
            SelectedDrive = AvailableDrives.FirstOrDefault();
            Log.Information("[INIT] Auto-selected drive: {SelectedDrive}", SelectedDrive?.Name);
        }

        Log.Information("[INIT] Tab.SelectedDrive AFTER auto-select: {SelectedDrive}", ActiveTab?.SelectedDrive?.Name);
        Log.Information("[INIT] InitializeActiveTabWithMotor() - END");
    }

    /// <summary>
    /// Handles active tab changes by notifying all dependent properties.
    /// </summary>
    private void OnActiveTabChanged()
    {
        _suppressSelectionWriteBack = true;

        Log.Information(
            "[TAB_CHANGE] OnActiveTabChanged() - START Tab={Tab} FilePath={FilePath} MotorName={MotorName} SelectedDrive={SelectedDrive} SelectedVoltage={SelectedVoltage} SelectedSeries={SelectedSeries}",
            ActiveTab?.DisplayName,
            ActiveTab?.FilePath,
            ActiveTab?.Motor?.MotorName,
            ActiveTab?.SelectedDrive?.Name,
            ActiveTab?.SelectedVoltage?.Value,
            ActiveTab?.SelectedSeries?.Name);

        try
        {
            // Notify all properties that depend on active tab
            OnPropertyChanged(nameof(CurrentMotor));
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(CurrentFilePath));
            OnPropertyChanged(nameof(ChartViewModel));
            OnPropertyChanged(nameof(CurveDataTableViewModel));
            OnPropertyChanged(nameof(EditingCoordinator));
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(WindowTitle));

            // Update directory browser
            DirectoryBrowser.UpdateOpenFileStates(CurrentFilePath, GetDirtyFilePaths());

            Log.Information("[TAB_CHANGE] Calling RefreshMotorEditorsFromCurrentMotor()");
            // Refresh property editors to display current tab's drive/voltage/series values
            RefreshMotorEditorsFromCurrentMotor();
            Log.Information("[TAB_CHANGE] Calling RefreshDriveEditorsFromSelectedDrive()");
            RefreshDriveEditorsFromSelectedDrive();
            Log.Information("[TAB_CHANGE] Calling RefreshVoltageEditorsFromSelectedVoltage()");
            RefreshVoltageEditorsFromSelectedVoltage();

            // Notify bindings that depend on ActiveTab. While we notify these, the ComboBox may
            // briefly push null back through SelectedItem due to ItemsSource churn; the selection
            // setters are suppressed until we exit this method.
            Log.Information("[TAB_CHANGE] Notifying combo-box bindings to refresh display");
            OnPropertyChanged(nameof(AvailableDrives));
            OnPropertyChanged(nameof(AvailableVoltages));
            OnPropertyChanged(nameof(AvailableSeries));
            OnPropertyChanged(nameof(SelectedDrive));
            OnPropertyChanged(nameof(SelectedVoltage));
            OnPropertyChanged(nameof(SelectedSeries));

            Log.Information(
                "[TAB_CHANGE] Post-notify snapshot: AvailableDrives={AvailableDrives} AvailableVoltages={AvailableVoltages} AvailableSeries={AvailableSeries} SelectedDrive={SelectedDrive} SelectedVoltage={SelectedVoltage}",
                ActiveTab?.AvailableDrives.Count ?? 0,
                ActiveTab?.AvailableVoltages.Count ?? 0,
                ActiveTab?.AvailableSeries.Count ?? 0,
                ActiveTab?.SelectedDrive?.Name,
                ActiveTab?.SelectedVoltage?.Value);

            Log.Information("[TAB_CHANGE] OnActiveTabChanged() - END");
        }
        finally
        {
            _suppressSelectionWriteBack = false;

            // Force ComboBox display to refresh after tab switch churn.
            // Pulse null -> actual so the binding engine must push the value back into the control.
            _selectedDriveForDisplay = null;
            OnPropertyChanged(nameof(SelectedDriveForDisplay));
            _selectedVoltageForDisplay = null;
            OnPropertyChanged(nameof(SelectedVoltageForDisplay));

            Dispatcher.UIThread.Post(
                () =>
                {
                    _selectedDriveForDisplay = ActiveTab?.SelectedDrive;
                    OnPropertyChanged(nameof(SelectedDriveForDisplay));
                    _selectedVoltageForDisplay = ActiveTab?.SelectedVoltage;
                    OnPropertyChanged(nameof(SelectedVoltageForDisplay));
                },
                DispatcherPriority.Background);
        }
    }

    private IEnumerable<string?> GetDirtyFilePaths()
    {
        if (Tabs is null)
        {
            yield break;
        }

        foreach (var tab in Tabs)
        {
            if (tab.IsDirty)
            {
                yield return tab.FilePath;
            }
        }
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
            DirectoryBrowser.UpdateOpenFileStates(filePath, GetDirtyFilePaths());
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

        try
        {
            // Check if file is already open in a tab
            var existingTab = _tabs.FirstOrDefault(t => 
                string.Equals(t.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            
            if (existingTab != null)
            {
                // Switch to existing tab
                ActiveTab = existingTab;
                StatusMessage = $"Switched to already open file: {Path.GetFileName(filePath)}";
                return;
            }
            
            // Create new tab for the file
            var newTab = CreateNewTab();
            newTab.Motor = await _fileService.LoadAsync(filePath);
            newTab.FilePath = filePath;
            newTab.UndoStack.Clear();
            newTab.MarkClean();
            
            _tabs.Add(newTab);
            
            // Set as active tab first, then initialize
            // This ensures all property setters and handlers are triggered correctly
            ActiveTab = newTab;
            InitializeActiveTabWithMotor();
            
            // Add to recent files
            _recentFilesService.AddRecentFile(filePath);
            
            StatusMessage = $"Opened: {Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file from directory browser");
            StatusMessage = $"Error: {ex.Message}";
        }
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
            ActiveTab?.UndoStack.Clear();
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
        // Undo infrastructure is now wired per-tab in CreateNewTab
        // This method is kept for backward compatibility but does nothing
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

    /// <summary>
    /// Marks the current undo history position as the clean checkpoint
    /// corresponding to the last successful save.
    /// </summary>
    public void MarkCleanCheckpoint()
    {
        ActiveTab?.MarkClean();
    }

    private int GetUndoDepth()
    {
        return ActiveTab?.UndoStack.UndoDepth ?? 0;
    }

    private void UpdateDirtyFromUndoDepth()
    {
        ActiveTab?.UpdateDirtyFromUndoDepth();
    }

    private void RefreshMotorEditorsFromCurrentMotor()
    {
        Log.Information("[REFRESH] RefreshMotorEditorsFromCurrentMotor() called");
        
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
        VoltageValueEditor = SelectedVoltage?.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePowerEditor = SelectedVoltage?.Power.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageMaxSpeedEditor = SelectedVoltage?.MaxSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakTorqueEditor = SelectedVoltage?.RatedPeakTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousTorqueEditor = SelectedVoltage?.RatedContinuousTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousAmpsEditor = SelectedVoltage?.ContinuousAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakAmpsEditor = SelectedVoltage?.PeakAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        
        Log.Information(
            "[REFRESH] Editor values updated: RatedPeakTorque={PeakTorque} RatedContinuousTorque={ContTorque} MaxSpeed={MaxSpeed} Power={Power}",
            RatedPeakTorqueEditor,
            RatedContinuousTorqueEditor,
            MaxSpeedEditor,
            PowerEditor);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.MotorName), oldName, newNameValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.Manufacturer), oldManufacturer, newManufacturerValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.MaxSpeed), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.PartNumber), oldPartNumber, newPartNumberValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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
        if (ActiveTab == null) return;

        Log.Debug(
            "[REFRESH] RefreshAvailableDrives() Tab={Tab} MotorName={MotorName} Before={Before}",
            ActiveTab.DisplayName,
            ActiveTab.Motor?.MotorName,
            ActiveTab.AvailableDrives.Count);

        ActiveTab.AvailableDrives.Clear();
        if (ActiveTab.Motor is not null)
        {
            foreach (var drive in ActiveTab.Motor.Drives)
            {
                ActiveTab.AvailableDrives.Add(drive);
            }
        }

        Log.Debug(
            "[REFRESH] RefreshAvailableDrives() After={After}",
            ActiveTab.AvailableDrives.Count);
    }

    /// <summary>
    /// Refreshes the available voltages collection from the selected drive.
    /// </summary>
    private void RefreshAvailableVoltages()
    {
        if (ActiveTab == null) return;

        Log.Debug(
            "[REFRESH] RefreshAvailableVoltages() Tab={Tab} Drive={DriveName} Before={Before}",
            ActiveTab.DisplayName,
            ActiveTab.SelectedDrive?.Name,
            ActiveTab.AvailableVoltages.Count);

        ActiveTab.AvailableVoltages.Clear();
        if (ActiveTab.SelectedDrive is not null)
        {
            foreach (var voltage in ActiveTab.SelectedDrive.Voltages)
            {
                ActiveTab.AvailableVoltages.Add(voltage);
            }
        }

        Log.Debug(
            "[REFRESH] RefreshAvailableVoltages() After={After}",
            ActiveTab.AvailableVoltages.Count);
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

        var command = new EditDrivePropertyCommand(SelectedDrive, nameof(Drive.Name), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditDrivePropertyCommand(SelectedDrive, nameof(Drive.PartNumber), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditDrivePropertyCommand(SelectedDrive, nameof(Drive.Manufacturer), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
        DriveManufacturerEditor = newValue;
        IsDirty = true;
    }

    public void EditSelectedVoltageValue()
    {
        if (SelectedVoltage is null)
        {
            return;
        }

        var oldValue = SelectedVoltage.Value;
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
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(Voltage.Value), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(Voltage.Power), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(Voltage.MaxSpeed), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(Voltage.RatedPeakTorque), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(Voltage.RatedContinuousTorque), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(Voltage.ContinuousAmperage), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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
        var command = new EditVoltagePropertyCommand(SelectedVoltage, nameof(Voltage.PeakAmperage), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.RatedSpeed), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
        RatedSpeedEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        ChartViewModel.MotorRatedSpeed = newValue;
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.RatedPeakTorque), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.RatedContinuousTorque), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.Power), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.Weight), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.RotorInertia), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.FeedbackPpr), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.HasBrake), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.BrakeTorque), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.BrakeAmperage), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.BrakeVoltage), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.BrakeReleaseTime), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.BrakeEngageTimeMov), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.BrakeEngageTimeDiode), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
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

        var command = new EditMotorPropertyCommand(CurrentMotor, nameof(ServoMotor.BrakeBacklash), oldValue, newValue);
        ActiveTab?.UndoStack.PushAndExecute(command);
        BrakeBacklashEditor = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        IsDirty = true;
    }

    /// <summary>
    /// Refreshes the available series collection from the selected voltage.
    /// </summary>
    private void RefreshAvailableSeries()
    {
        if (ActiveTab == null) return;
        
        ActiveTab.AvailableSeries.Clear();
        if (ActiveTab.SelectedVoltage is not null)
        {
            foreach (var series in ActiveTab.SelectedVoltage.Curves)
            {
                ActiveTab.AvailableSeries.Add(series);
            }
        }
    }

    /// <summary>
    /// Refreshes the drive editor fields from the currently selected drive.
    /// Does not modify selections, only updates the UI display fields.
    /// </summary>
    private void RefreshDriveEditorsFromSelectedDrive()
    {
        Log.Information($"[REFRESH] RefreshDriveEditorsFromSelectedDrive() - ActiveTab.SelectedDrive: {ActiveTab?.SelectedDrive?.Name ?? "null"}");
        var drive = ActiveTab?.SelectedDrive;
        if (drive is not null)
        {
            DriveNameEditor = drive.Name ?? string.Empty;
            DrivePartNumberEditor = drive.PartNumber ?? string.Empty;
            DriveManufacturerEditor = drive.Manufacturer ?? string.Empty;
            Log.Information($"[REFRESH] Set drive editors - Name: {DriveNameEditor}, PartNumber: {DrivePartNumberEditor}");
        }
        else
        {
            DriveNameEditor = string.Empty;
            DrivePartNumberEditor = string.Empty;
            DriveManufacturerEditor = string.Empty;
            Log.Information("[REFRESH] Cleared drive editors - drive is null");
        }
    }

    /// <summary>
    /// Refreshes the voltage editor fields from the currently selected voltage.
    /// Does not modify selections, only updates the UI display fields.
    /// </summary>
    private void RefreshVoltageEditorsFromSelectedVoltage()
    {
        Log.Information($"[REFRESH] RefreshVoltageEditorsFromSelectedVoltage() - ActiveTab.SelectedVoltage: {ActiveTab?.SelectedVoltage?.Value}");
        var voltage = ActiveTab?.SelectedVoltage;
        if (voltage is not null)
        {
            VoltageValueEditor = voltage.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            VoltagePowerEditor = voltage.Power.ToString(System.Globalization.CultureInfo.InvariantCulture);
            VoltageMaxSpeedEditor = voltage.MaxSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture);
            VoltagePeakTorqueEditor = voltage.RatedPeakTorque.ToString(System.Globalization.CultureInfo.InvariantCulture);
            VoltageContinuousTorqueEditor = voltage.RatedContinuousTorque.ToString(System.Globalization.CultureInfo.InvariantCulture);
            VoltageContinuousAmpsEditor = voltage.ContinuousAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture);
            VoltagePeakAmpsEditor = voltage.PeakAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Log.Information($"[REFRESH] Set voltage editors - Value: {VoltageValueEditor}, Power: {VoltagePowerEditor}");
        }
        else
        {
            VoltageValueEditor = string.Empty;
            VoltagePowerEditor = string.Empty;
            VoltageMaxSpeedEditor = string.Empty;
            VoltagePeakTorqueEditor = string.Empty;
            VoltageContinuousTorqueEditor = string.Empty;
            VoltageContinuousAmpsEditor = string.Empty;
            VoltagePeakAmpsEditor = string.Empty;
            Log.Information("[REFRESH] Cleared voltage editors - voltage is null");
        }
    }

    /// <summary>
    /// Public method to refresh the available series collection.
    /// </summary>
    public void RefreshAvailableSeriesPublic()
    {
        RefreshAvailableSeries();
    }

    private void OnCurrentMotorChanged(ServoMotor? value)
    {
        Log.Information(
            "[MOTOR] OnCurrentMotorChanged() Tab={Tab} FilePath={FilePath} MotorName={MotorName} Drives={DriveCount}",
            ActiveTab?.DisplayName,
            ActiveTab?.FilePath,
            value?.MotorName,
            value?.Drives?.Count ?? 0);

        // Unsubscribe from previous motor's Units PropertyChanged
        if (CurrentMotor != null && CurrentMotor.Units != null)
        {
            CurrentMotor.Units.PropertyChanged -= OnUnitsPropertyChanged;
        }

        // Subscribe to new motor's Units PropertyChanged
        if (value != null && value.Units != null)
        {
            value.Units.PropertyChanged += OnUnitsPropertyChanged;
            
            // Initialize previous unit values for tracking
            _previousTorqueUnit = value.Units.Torque;
            _previousSpeedUnit = value.Units.Speed;
            _previousPowerUnit = value.Units.Power;
            _previousWeightUnit = value.Units.Weight;
            _previousInertiaUnit = value.Units.Inertia;
            _previousCurrentUnit = value.Units.Current;
            _previousResponseTimeUnit = value.Units.ResponseTime;
            _previousBacklashUnit = value.Units.Backlash;
        }

        // Refresh the drives collection
        RefreshAvailableDrives();

        // When motor changes, select the first drive
        SelectedDrive = value?.Drives.FirstOrDefault();

        Log.Information(
            "[MOTOR] After motor change: AvailableDrives={AvailableDrives} SelectedDrive={SelectedDrive} SelectedVoltage={SelectedVoltage}",
            ActiveTab?.AvailableDrives.Count ?? 0,
            ActiveTab?.SelectedDrive?.Name,
            ActiveTab?.SelectedVoltage?.Value);

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
            VoltageValueEditor = SelectedVoltage?.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltagePowerEditor = SelectedVoltage?.Power.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltageMaxSpeedEditor = SelectedVoltage?.MaxSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltagePeakTorqueEditor = SelectedVoltage?.RatedPeakTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltageContinuousTorqueEditor = SelectedVoltage?.RatedContinuousTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltageContinuousAmpsEditor = SelectedVoltage?.ContinuousAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            VoltagePeakAmpsEditor = SelectedVoltage?.PeakAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        }

        // Populate ValidationErrors/ValidationErrorsList immediately when switching or loading motors.
        // Without this, errors only show up after an edit (MarkDirty) or manual refresh.
        ValidateMotor();
    }

    private void OnSelectedDriveChanged(Drive? value)
    {
        Log.Information(
            "[SELECTION] OnSelectedDriveChanged() Tab={Tab} FilePath={FilePath} NewDrive={NewDrive}",
            ActiveTab?.DisplayName,
            ActiveTab?.FilePath,
            value?.Name);
        
        // Refresh the available voltages collection
        RefreshAvailableVoltages();

        if (value is null)
        {
            SelectedVoltage = null;
            Log.Information("[SELECTION] OnSelectedDriveChanged() Drive cleared -> SelectedVoltage set to null");
            return;
        }

        // Prefer 208V if available, otherwise use the first voltage
        var preferred = value.Voltages.FirstOrDefault(v => Math.Abs(v.Value - 208) < 0.1);
        SelectedVoltage = preferred ?? value.Voltages.FirstOrDefault();
        Log.Information("[SELECTION] OnSelectedDriveChanged() Auto-selected voltage: {SelectedVoltage}", SelectedVoltage?.Value);

        DriveNameEditor = value.Name ?? string.Empty;
        DrivePartNumberEditor = value.PartNumber ?? string.Empty;
        DriveManufacturerEditor = value.Manufacturer ?? string.Empty;
        Log.Debug(
            "[SELECTION] Drive editors refreshed: DriveNameEditor={DriveNameEditor} PartNumberEditor={PartNumberEditor} ManufacturerEditor={ManufacturerEditor}",
            DriveNameEditor,
            DrivePartNumberEditor,
            DriveManufacturerEditor);
    }

    private void OnSelectedVoltageChanged(Voltage? value)
    {
        Log.Information(
            "[SELECTION] OnSelectedVoltageChanged() Tab={Tab} FilePath={FilePath} Drive={DriveName} NewVoltage={NewVoltage}",
            ActiveTab?.DisplayName,
            ActiveTab?.FilePath,
            ActiveTab?.SelectedDrive?.Name,
            value?.Value);
        
        // Refresh the available series collection
        RefreshAvailableSeries();

        // When voltage changes, update series selection
        SelectedSeries = value?.Curves.FirstOrDefault();
        Log.Information("[SELECTION] OnSelectedVoltageChanged() Auto-selected series: {SelectedSeries}", SelectedSeries?.Name);

        // Update chart with new voltage configuration
        ChartViewModel.TorqueUnit = CurrentMotor?.Units.Torque ?? "Nm";
        ChartViewModel.MotorMaxSpeed = CurrentMotor?.MaxSpeed ?? 0;
        ChartViewModel.MotorRatedSpeed = CurrentMotor?.RatedSpeed ?? 0;
        ChartViewModel.HasBrake = CurrentMotor?.HasBrake ?? false;
        ChartViewModel.BrakeTorque = CurrentMotor?.BrakeTorque ?? 0;
        ChartViewModel.CurrentVoltage = value;

        // Update data table with new voltage configuration
        CurveDataTableViewModel.CurrentVoltage = value;

        VoltageValueEditor = value?.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePowerEditor = value?.Power.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageMaxSpeedEditor = value?.MaxSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakTorqueEditor = value?.RatedPeakTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousTorqueEditor = value?.RatedContinuousTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousAmpsEditor = value?.ContinuousAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakAmpsEditor = value?.PeakAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        Log.Information("[SELECTION] OnSelectedVoltageChanged() - END");
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
    /// Handles changes to unit settings and converts stored motor data.
    /// </summary>
    private void OnUnitsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (CurrentMotor == null || CurrentMotor.Units == null)
        {
            return;
        }

        // Determine old and new units
        string? oldValue = null;
        string? newValue = null;

        switch (e.PropertyName)
        {
            case nameof(UnitSettings.Torque):
                oldValue = _previousTorqueUnit ?? "Nm";
                newValue = CurrentMotor.Units.Torque;
                _previousTorqueUnit = newValue;
                break;
            case nameof(UnitSettings.Speed):
                oldValue = _previousSpeedUnit ?? "rpm";
                newValue = CurrentMotor.Units.Speed;
                _previousSpeedUnit = newValue;
                break;
            case nameof(UnitSettings.Power):
                oldValue = _previousPowerUnit ?? "W";
                newValue = CurrentMotor.Units.Power;
                _previousPowerUnit = newValue;
                break;
            case nameof(UnitSettings.Weight):
                oldValue = _previousWeightUnit ?? "kg";
                newValue = CurrentMotor.Units.Weight;
                _previousWeightUnit = newValue;
                break;
            case nameof(UnitSettings.Inertia):
                oldValue = _previousInertiaUnit ?? "kg-m^2";
                newValue = CurrentMotor.Units.Inertia;
                _previousInertiaUnit = newValue;
                break;
            case nameof(UnitSettings.Current):
                oldValue = _previousCurrentUnit ?? "A";
                newValue = CurrentMotor.Units.Current;
                _previousCurrentUnit = newValue;
                break;
            case nameof(UnitSettings.ResponseTime):
                oldValue = _previousResponseTimeUnit ?? "ms";
                newValue = CurrentMotor.Units.ResponseTime;
                _previousResponseTimeUnit = newValue;
                break;
            case nameof(UnitSettings.Backlash):
                oldValue = _previousBacklashUnit ?? "arcmin";
                newValue = CurrentMotor.Units.Backlash;
                _previousBacklashUnit = newValue;
                break;
        }

        if (oldValue == null || newValue == null || oldValue == newValue)
        {
            return;
        }

        Log.Information(
            "[UNITS] Unit changed: Property={Property} OldValue={Old} NewValue={New}",
            e.PropertyName,
            oldValue,
            newValue);

        // Log motor values before conversion
        Log.Information(
            "[UNITS] Before conversion: RatedPeakTorque={PeakTorque} RatedContinuousTorque={ContTorque} MaxSpeed={MaxSpeed} Power={Power}",
            CurrentMotor.RatedPeakTorque,
            CurrentMotor.RatedContinuousTorque,
            CurrentMotor.MaxSpeed,
            CurrentMotor.Power);

        try
        {
            // Create old and new unit settings for conversion
            var oldUnits = new UnitSettings
            {
                Torque = e.PropertyName == nameof(UnitSettings.Torque) ? oldValue : CurrentMotor.Units.Torque,
                Speed = e.PropertyName == nameof(UnitSettings.Speed) ? oldValue : CurrentMotor.Units.Speed,
                Power = e.PropertyName == nameof(UnitSettings.Power) ? oldValue : CurrentMotor.Units.Power,
                Weight = e.PropertyName == nameof(UnitSettings.Weight) ? oldValue : CurrentMotor.Units.Weight,
                Inertia = e.PropertyName == nameof(UnitSettings.Inertia) ? oldValue : CurrentMotor.Units.Inertia,
                Current = e.PropertyName == nameof(UnitSettings.Current) ? oldValue : CurrentMotor.Units.Current,
                ResponseTime = e.PropertyName == nameof(UnitSettings.ResponseTime) ? oldValue : CurrentMotor.Units.ResponseTime,
                Backlash = e.PropertyName == nameof(UnitSettings.Backlash) ? oldValue : CurrentMotor.Units.Backlash
            };

            // Convert motor data with new units (hard conversion)
            _unitConversionService.ConvertMotorUnits(CurrentMotor, oldUnits, CurrentMotor.Units);

            // Log motor values after conversion
            Log.Information(
                "[UNITS] After conversion: RatedPeakTorque={PeakTorque} RatedContinuousTorque={ContTorque} MaxSpeed={MaxSpeed} Power={Power} BrakeAmperage={BrakeAmperage} BrakeBacklash={BrakeBacklash}",
                CurrentMotor.RatedPeakTorque,
                CurrentMotor.RatedContinuousTorque,
                CurrentMotor.MaxSpeed,
                CurrentMotor.Power,
                CurrentMotor.BrakeAmperage,
                CurrentMotor.BrakeBacklash);

            // Refresh UI to show converted values
            RefreshMotorEditorsFromCurrentMotor();
            RefreshVoltageEditorsFromSelectedVoltage();
            
            // Log editor values after refresh to verify UI update
            Log.Information(
                "[REFRESH] After RefreshMotorEditors: BrakeAmperageEditor={BrakeAmpEditor} BrakeBacklashEditor={BrakeBackEditor} VoltageContinuousAmpsEditor={ContAmpsEditor} VoltagePeakAmpsEditor={PeakAmpsEditor}",
                BrakeAmperageEditor,
                BrakeBacklashEditor,
                VoltageContinuousAmpsEditor,
                VoltagePeakAmpsEditor);
            
            // Update chart unit labels when torque or power units change
            if (ChartViewModel != null)
            {
                if (e.PropertyName == nameof(UnitSettings.Torque))
                {
                    ChartViewModel.TorqueUnit = CurrentMotor.Units.Torque;
                }
                if (e.PropertyName == nameof(UnitSettings.Power))
                {
                    ChartViewModel.PowerUnit = CurrentMotor.Units.Power;
                }
            }
            
            // Refresh chart and curve data table if available
            ChartViewModel?.RefreshChart();
            ActiveTab?.CurveDataTableViewModel?.RefreshData();

            // Mark as dirty since data changed
            MarkDirty();

            StatusMessage = $"Converted {e.PropertyName} from {oldValue} to {newValue}";
            
            Log.Information("[UNITS] Conversion complete, UI refreshed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[UNITS] Error converting units");
            StatusMessage = $"Error converting units: {ex.Message}";
        }
    }

    /// <summary>
    /// Undoes the most recent editing operation, if any.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        ActiveTab?.UndoStack.Undo();
        RefreshMotorEditorsFromCurrentMotor();
        ActiveTab?.ChartViewModel?.RefreshChart();
        ActiveTab?.CurveDataTableViewModel?.RefreshData();
    }

    /// <summary>
    /// Re-applies the most recently undone editing operation, if any.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        ActiveTab?.UndoStack.Redo();
        RefreshMotorEditorsFromCurrentMotor();
        ActiveTab?.ChartViewModel?.RefreshChart();
        ActiveTab?.CurveDataTableViewModel?.RefreshData();
    }

    [RelayCommand]
    private async Task NewMotorAsync()
    {
        Log.Information("Creating new motor definition in new tab");

        // Create a new tab with a new motor
        var newTab = CreateNewTab();
        
        // Create motor using existing file service
        newTab.Motor = _fileService.CreateNew(
            motorName: "New Motor",
            maxRpm: 5000,
            maxTorque: 50,
            maxPower: 1500);
        
        newTab.FilePath = null;
        newTab.UndoStack.Clear();
        newTab.MarkClean();
        
        _tabs.Add(newTab);
        
        // Set as active tab first, then initialize
        // This ensures all property setters and handlers are triggered correctly
        ActiveTab = newTab;
        InitializeActiveTabWithMotor();
        
        StatusMessage = "Created new motor definition in new tab";
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        Log.Information("[FILE_OP] OpenFileAsync() - START");

        try
        {
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
            Log.Information($"[FILE_OP] Selected file: {filePath}");

            // Check if file is already open in a tab
            var existingTab = _tabs.FirstOrDefault(t => 
                string.Equals(t.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            
            if (existingTab != null)
            {
                Log.Information($"[FILE_OP] File already open in tab: {existingTab.DisplayName}, switching to it");
                // Switch to existing tab
                ActiveTab = existingTab;
                StatusMessage = $"Switched to already open file: {Path.GetFileName(filePath)}";
                // Still add to recent files even if already open
                _recentFilesService.AddRecentFile(filePath);
                return;
            }
            
            // Create new tab for the file
            Log.Information("[FILE_OP] Creating new tab for file");
            var newTab = CreateNewTab();
            newTab.Motor = await _fileService.LoadAsync(filePath);
            newTab.FilePath = filePath;
            newTab.UndoStack.Clear();
            newTab.MarkClean();
            Log.Information($"[FILE_OP] Loaded motor with {newTab.Motor.Drives.Count} drives");
            
            _tabs.Add(newTab);
            Log.Information($"[FILE_OP] Added tab to collection, total tabs: {_tabs.Count}");
            
            // Set as active tab first, then initialize
            // This ensures all property setters and handlers are triggered correctly
            Log.Information("[FILE_OP] Setting ActiveTab");
            ActiveTab = newTab;
            Log.Information("[FILE_OP] Calling InitializeActiveTabWithMotor()");
            InitializeActiveTabWithMotor();
            
            await DirectoryBrowser.SyncSelectionToFilePathAsync(filePath).ConfigureAwait(true);
            
            // Add to recent files
            _recentFilesService.AddRecentFile(filePath);
            
            StatusMessage = $"Opened: {Path.GetFileName(filePath)}";
            Log.Information("[FILE_OP] OpenFileAsync() - END");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file");
            StatusMessage = $"Error: {ex.Message}";
            Log.Information($"[FILE_OP] ERROR: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task OpenRecentFileAsync(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        try
        {
            // Check if file exists
            if (!File.Exists(filePath))
            {
                StatusMessage = $"File not found: {Path.GetFileName(filePath)}";
                // Remove from recent files list
                _recentFilesService.RemoveRecentFile(filePath);
                return;
            }

            // Check if file is already open in a tab
            var existingTab = _tabs.FirstOrDefault(t => 
                string.Equals(t.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            
            if (existingTab != null)
            {
                // Switch to existing tab
                ActiveTab = existingTab;
                StatusMessage = $"Switched to already open file: {Path.GetFileName(filePath)}";
                // Move to top of recent files
                _recentFilesService.AddRecentFile(filePath);
                return;
            }
            
            // Create new tab for the file
            var newTab = CreateNewTab();
            newTab.Motor = await _fileService.LoadAsync(filePath);
            newTab.FilePath = filePath;
            newTab.UndoStack.Clear();
            newTab.MarkClean();
            
            _tabs.Add(newTab);
            ActiveTab = newTab;
            InitializeActiveTabWithMotor();
            
            await DirectoryBrowser.SyncSelectionToFilePathAsync(filePath).ConfigureAwait(true);
            
            // Add to recent files (moves to top)
            _recentFilesService.AddRecentFile(filePath);
            
            StatusMessage = $"Opened: {Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open recent file: {FilePath}", filePath);
            StatusMessage = $"Error opening file: {ex.Message}";
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

        ActiveTab?.UndoStack.Clear();
        MarkCleanCheckpoint();

        CurrentFilePath = null;
        CurrentMotor = null;
        SelectedDrive = null;
        SelectedVoltage = null;
        SelectedSeries = null;

        if (ChartViewModel != null)
        {
            ChartViewModel.CurrentVoltage = null;
        }
        if (CurveDataTableViewModel != null)
        {
            CurveDataTableViewModel.CurrentVoltage = null;
        }

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
    /// Shows the Add Drive/Value dialog and creates the drive with curve series.
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
    /// Shows the Add Drive/Value dialog for adding a new voltage configuration.
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
            Message = $"Are you sure you want to delete the selected voltage '{SelectedVoltage.Value}V'?"
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

            var voltage = SelectedVoltage.Value;
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

    // Curves management commands
    [RelayCommand]
    private async Task AddSeriesAsync()
    {
        await AddSeriesInternalAsync();
    }

    /// <summary>
    /// Core workflow for adding a new drive without voltage configuration.
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
            var dialog = new Views.AddDriveDialog();

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

            var drive = _motorConfigurationWorkflow.CreateDrive(CurrentMotor, result);

            // Add the new drive directly to the collection (don't clear/refresh)
            AvailableDrives.Add(drive);

            // Select the new drive - this will trigger OnSelectedDriveChanged which updates voltages
            SelectedDrive = drive;

            // Explicitly refresh chart to ensure axes are updated
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
        if (CurrentMotor is null || CurrentMotor.Drives.Count == 0)
        {
            StatusMessage = "Cannot add voltage: no drives available. Add a drive first.";
            return;
        }

        try
        {
            var dialog = new Views.AddVoltageDialog();

            dialog.Initialize(
                CurrentMotor.Drives,
                SelectedDrive,
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

            // The target drive is selected in the dialog
            var targetDrive = result.TargetDrive;

            // Delegate duplicate check and creation to the workflow
            var voltageResult = _motorConfigurationWorkflow.CreateVoltageWithOptionalSeries(targetDrive, result);
            if (voltageResult.IsDuplicate)
            {
                StatusMessage = $"Voltage {result.Voltage}V already exists for drive '{targetDrive.Name}'.";
                return;
            }

            var voltage = voltageResult.Voltage;

            // If the target drive is different from the currently selected drive, switch to it
            if (SelectedDrive != targetDrive)
            {
                SelectedDrive = targetDrive;
            }
            else
            {
                // If the same drive, refresh the available voltages
                RefreshAvailableVoltages();
            }

            // Select the new voltage
            SelectedVoltage = voltage;

            // Force chart refresh to update axes based on new max speed
            ChartViewModel.RefreshChart();

            MarkDirty();
            StatusMessage = $"Added voltage: {result.Voltage}V to drive '{targetDrive.Name}'";
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
            var dialog = new Views.AddCurveDialog();
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
            if (SelectedVoltage.Curves.Count <= 1)
            {
                StatusMessage = "Cannot remove the last series.";
                return;
            }

            var seriesName = SelectedSeries.Name;
            SelectedVoltage.Curves.Remove(SelectedSeries);
            RefreshAvailableSeries();
            SelectedSeries = SelectedVoltage.Curves.FirstOrDefault();
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
    private void ToggleSeriesLock(Curve? series)
    {
        if (series is null)
        {
            return;
        }

        var newLocked = !series.Locked;

        var command = new EditSeriesCommand(series, newLocked: newLocked);
        ActiveTab?.UndoStack.PushAndExecute(command);
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
    private void ToggleSeriesVisibility(Curve? series)
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
    private async Task CloseTabAsync(DocumentTab? tab)
    {
        if (tab == null) return;

        // Prompt if dirty
        if (tab.IsDirty)
        {
            // Temporarily make it active for the prompt
            var wasActive = ActiveTab;
            ActiveTab = tab;

            if (!await ConfirmLoseUnsavedChangesOrCancelAsync("close this tab", "Close cancelled."))
            {
                ActiveTab = wasActive;
                return;
            }

            ActiveTab = wasActive;
        }

        // Remove the tab
        UnwireTabIntegration(tab);
        _tabs.Remove(tab);

        // If we closed the active tab, activate another
        if (ActiveTab == tab)
        {
            ActiveTab = _tabs.LastOrDefault();
        }

        // If we closed a dirty background tab, ensure the Directory Browser drops its marker.
        DirectoryBrowser.UpdateOpenFileStates(CurrentFilePath, GetDirtyFilePaths());

        StatusMessage = $"Closed tab: {tab.DisplayName}";
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
            ValidationErrorsList.Clear();
            return;
        }

        var errors = _validationService.ValidateServoMotor(CurrentMotor);

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
        
        // Update the list for the validation panel
        ValidationErrorsList.Clear();
        foreach (var error in errors)
        {
            ValidationErrorsList.Add(error);
        }

        // If validation fails, open the validation panel so the user can see the full list.
        // Note: Bottom panel is not tied to validation.
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
