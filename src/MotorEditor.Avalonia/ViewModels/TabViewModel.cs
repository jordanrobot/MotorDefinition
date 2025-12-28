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
using System.Linq;
using System.Threading.Tasks;

namespace CurveEditor.ViewModels;

/// <summary>
/// Represents a single tab/document in the tabbed interface.
/// Encapsulates all state for one motor definition file.
/// </summary>
public partial class TabViewModel : ViewModelBase
{
    private readonly IFileService _fileService;
    private readonly ICurveGeneratorService _curveGeneratorService;
    private readonly IValidationService _validationService;
    private readonly IDriveVoltageSeriesService _driveVoltageSeriesService;
    private readonly IMotorConfigurationWorkflow _motorConfigurationWorkflow;
    private readonly UndoStack _undoStack = new();
    private int _cleanCheckpoint;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TabHeader), nameof(TabTooltip))]
    private ServoMotor? _currentMotor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TabHeader))]
    private bool _isDirty;

    [ObservableProperty]
    private string _validationErrors = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveWithValidation))]
    private bool _hasValidationErrors;

    [ObservableProperty]
    private Drive? _selectedDrive;

    [ObservableProperty]
    private Voltage? _selectedVoltage;

    [ObservableProperty]
    private Curve? _selectedSeries;

    [ObservableProperty]
    private EditingCoordinator _editingCoordinator = new();

    [ObservableProperty]
    private ChartViewModel _chartViewModel;

    [ObservableProperty]
    private CurveDataTableViewModel _curveDataTableViewModel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TabHeader), nameof(TabTooltip))]
    private string? _currentFilePath;

    [ObservableProperty]
    private ObservableCollection<Voltage> _availableVoltages = [];

    [ObservableProperty]
    private ObservableCollection<Curve> _availableSeries = [];

    [ObservableProperty]
    private ObservableCollection<Drive> _availableDrives = [];

    // Motor property editor buffers
    [ObservableProperty]
    private string _motorNameEditor = string.Empty;

    [ObservableProperty]
    private string _manufacturerEditor = string.Empty;

    [ObservableProperty]
    private string _partNumberEditor = string.Empty;

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

    [ObservableProperty]
    private string _driveNameEditor = string.Empty;

    [ObservableProperty]
    private string _drivePartNumberEditor = string.Empty;

    [ObservableProperty]
    private string _driveManufacturerEditor = string.Empty;

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
    /// Tab header text showing file name and dirty indicator.
    /// </summary>
    public string TabHeader
    {
        get
        {
            var fileName = CurrentFilePath is not null
                ? System.IO.Path.GetFileName(CurrentFilePath)
                : CurrentMotor?.MotorName ?? "Untitled";

            var dirtyIndicator = IsDirty ? " *" : string.Empty;
            return $"{fileName}{dirtyIndicator}";
        }
    }

    /// <summary>
    /// Tooltip showing full file path.
    /// </summary>
    public string TabTooltip => CurrentFilePath ?? "New file";

    public TabViewModel(
        IFileService fileService,
        ICurveGeneratorService curveGeneratorService,
        IValidationService validationService,
        IDriveVoltageSeriesService driveVoltageSeriesService,
        IMotorConfigurationWorkflow motorConfigurationWorkflow,
        ChartViewModel chartViewModel,
        CurveDataTableViewModel curveDataTableViewModel)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _curveGeneratorService = curveGeneratorService ?? throw new ArgumentNullException(nameof(curveGeneratorService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _driveVoltageSeriesService = driveVoltageSeriesService ?? throw new ArgumentNullException(nameof(driveVoltageSeriesService));
        _motorConfigurationWorkflow = motorConfigurationWorkflow ?? throw new ArgumentNullException(nameof(motorConfigurationWorkflow));
        _chartViewModel = chartViewModel ?? throw new ArgumentNullException(nameof(chartViewModel));
        _curveDataTableViewModel = curveDataTableViewModel ?? throw new ArgumentNullException(nameof(curveDataTableViewModel));

        WireEditingCoordinator();
        WireUndoInfrastructure();
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
    }

    /// <summary>
    /// Undoes the most recent editing operation, if any.
    /// </summary>
    public void Undo()
    {
        _undoStack.Undo();
        RefreshMotorEditorsFromCurrentMotor();
        ChartViewModel.RefreshChart();
        CurveDataTableViewModel.RefreshData();
    }

    /// <summary>
    /// Re-applies the most recently undone editing operation, if any.
    /// </summary>
    public void Redo()
    {
        _undoStack.Redo();
        RefreshMotorEditorsFromCurrentMotor();
        ChartViewModel.RefreshChart();
        CurveDataTableViewModel.RefreshData();
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
        VoltageValueEditor = SelectedVoltage?.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePowerEditor = SelectedVoltage?.Power.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageMaxSpeedEditor = SelectedVoltage?.MaxSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakTorqueEditor = SelectedVoltage?.RatedPeakTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousTorqueEditor = SelectedVoltage?.RatedContinuousTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousAmpsEditor = SelectedVoltage?.ContinuousAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakAmpsEditor = SelectedVoltage?.PeakAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    partial void OnCurrentMotorChanged(ServoMotor? value)
    {
        RefreshAvailableDrives();
        SelectedDrive = value?.Drives.FirstOrDefault();

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
    }

    partial void OnSelectedDriveChanged(Drive? value)
    {
        RefreshAvailableVoltages();

        if (value is null)
        {
            SelectedVoltage = null;
            return;
        }

        var preferred = value.Voltages.FirstOrDefault(v => Math.Abs(v.Value - 208) < 0.1);
        SelectedVoltage = preferred ?? value.Voltages.FirstOrDefault();

        DriveNameEditor = value.Name ?? string.Empty;
        DrivePartNumberEditor = value.PartNumber ?? string.Empty;
        DriveManufacturerEditor = value.Manufacturer ?? string.Empty;
    }

    partial void OnSelectedVoltageChanged(Voltage? value)
    {
        RefreshAvailableSeries();
        SelectedSeries = value?.Curves.FirstOrDefault();

        ChartViewModel.TorqueUnit = CurrentMotor?.Units.Torque ?? "Nm";
        ChartViewModel.MotorMaxSpeed = CurrentMotor?.MaxSpeed ?? 0;
        ChartViewModel.MotorRatedSpeed = CurrentMotor?.RatedSpeed ?? 0;
        ChartViewModel.HasBrake = CurrentMotor?.HasBrake ?? false;
        ChartViewModel.BrakeTorque = CurrentMotor?.BrakeTorque ?? 0;
        ChartViewModel.CurrentVoltage = value;

        CurveDataTableViewModel.CurrentVoltage = value;

        VoltageValueEditor = value?.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePowerEditor = value?.Power.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageMaxSpeedEditor = value?.MaxSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakTorqueEditor = value?.RatedPeakTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousTorqueEditor = value?.RatedContinuousTorque.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltageContinuousAmpsEditor = value?.ContinuousAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        VoltagePeakAmpsEditor = value?.PeakAmperage.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

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

    private void RefreshAvailableSeries()
    {
        AvailableSeries.Clear();
        if (SelectedVoltage is not null)
        {
            foreach (var series in SelectedVoltage.Curves)
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

    // TODO: Add all the edit methods from MainWindowViewModel that manipulate the motor state
    // These will need to be moved here to work with the per-tab UndoStack
}
