using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurveEditor.Models;
using CurveEditor.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CurveEditor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFileService _fileService;
    private readonly ICurveGeneratorService _curveGeneratorService;
    private readonly IValidationService _validationService;

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
    [NotifyPropertyChangedFor(nameof(AvailableVoltages))]
    private DriveConfiguration? _selectedDrive;

    // Voltage selection
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvailableSeries))]
    private VoltageConfiguration? _selectedVoltage;

    // Series selection
    [ObservableProperty]
    private CurveSeries? _selectedSeries;

    /// <summary>
    /// ViewModel for the chart component.
    /// </summary>
    [ObservableProperty]
    private ChartViewModel _chartViewModel = new();

    /// <summary>
    /// Whether the units section is expanded.
    /// </summary>
    [ObservableProperty]
    private bool _isUnitsExpanded;

    /// <summary>
    /// Whether the curve data panel is expanded.
    /// </summary>
    [ObservableProperty]
    private bool _isCurveDataExpanded;

    public ObservableCollection<VoltageConfiguration> AvailableVoltages =>
        new(SelectedDrive?.Voltages ?? []);

    public ObservableCollection<CurveSeries> AvailableSeries =>
        new(SelectedVoltage?.Series ?? []);

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

    public MainWindowViewModel()
    {
        _curveGeneratorService = new CurveGeneratorService();
        _fileService = new FileService(_curveGeneratorService);
        _validationService = new ValidationService();
        ChartViewModel.DataChanged += OnChartDataChanged;
    }

    public MainWindowViewModel(IFileService fileService, ICurveGeneratorService curveGeneratorService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _curveGeneratorService = curveGeneratorService ?? throw new ArgumentNullException(nameof(curveGeneratorService));
        _validationService = new ValidationService();
    }

    public string WindowTitle
    {
        get
        {
            var fileName = _fileService.CurrentFilePath is not null
                ? Path.GetFileName(_fileService.CurrentFilePath)
                : CurrentMotor?.MotorName ?? "Untitled";

            var dirtyIndicator = IsDirty ? " *" : "";
            return $"{fileName}{dirtyIndicator} - Curve Editor";
        }
    }

    partial void OnCurrentMotorChanged(MotorDefinition? value)
    {
        // When motor changes, update selections
        SelectedDrive = value?.Drives.FirstOrDefault();
    }

    partial void OnSelectedDriveChanged(DriveConfiguration? value)
    {
        // When drive changes, update voltage selection
        SelectedVoltage = value?.Voltages.FirstOrDefault();
    }

    partial void OnSelectedVoltageChanged(VoltageConfiguration? value)
    {
        // When voltage changes, update series selection
        SelectedSeries = value?.Series.FirstOrDefault();
        
        // Update chart with new voltage configuration
        ChartViewModel.TorqueUnit = CurrentMotor?.Units.Torque ?? "Nm";
        ChartViewModel.CurrentVoltage = value;
    }

    private void OnChartDataChanged(object? sender, EventArgs e)
    {
        MarkDirty();
    }

    [RelayCommand]
    private void NewMotor()
    {
        Log.Information("Creating new motor definition");

        // Create a new motor with default values
        CurrentMotor = _fileService.CreateNew(
            motorName: "New Motor",
            maxRpm: 5000,
            maxTorque: 50,
            maxPower: 1500);

        IsDirty = _fileService.IsDirty;
        StatusMessage = "Created new motor definition";
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        Log.Information("Opening file dialog");

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

            CurrentMotor = await _fileService.LoadAsync(filePath);
            IsDirty = _fileService.IsDirty;
            StatusMessage = $"Loaded: {Path.GetFileName(filePath)}";
            OnPropertyChanged(nameof(WindowTitle));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file");
            StatusMessage = $"Error: {ex.Message}";
        }
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
            StatusMessage = "File saved successfully";
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
            StatusMessage = $"Saved to: {Path.GetFileName(filePath)}";
            OnPropertyChanged(nameof(WindowTitle));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save file");
            StatusMessage = $"Error saving: {ex.Message}";
        }
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

    // Drive management commands
    [RelayCommand]
    private void AddDrive()
    {
        if (CurrentMotor is null) return;

        try
        {
            var newDriveName = GenerateUniqueName(
                CurrentMotor.Drives.Select(d => d.Name),
                "New Drive");
            
            var drive = CurrentMotor.AddDrive(newDriveName);
            
            // Add a default voltage configuration
            var voltage = drive.AddVoltageConfiguration(220);
            voltage.MaxSpeed = CurrentMotor.MaxSpeed;
            voltage.Power = CurrentMotor.Power;
            voltage.RatedPeakTorque = CurrentMotor.RatedPeakTorque;
            voltage.RatedContinuousTorque = CurrentMotor.RatedContinuousTorque;
            
            // Add default series
            var peakSeries = new CurveSeries("Peak");
            var continuousSeries = new CurveSeries("Continuous");
            peakSeries.InitializeData(voltage.MaxSpeed, voltage.RatedPeakTorque);
            continuousSeries.InitializeData(voltage.MaxSpeed, voltage.RatedContinuousTorque);
            voltage.Series.Add(peakSeries);
            voltage.Series.Add(continuousSeries);
            
            SelectedDrive = drive;
            OnPropertyChanged(nameof(CurrentMotor));
            MarkDirty();
            StatusMessage = $"Added drive: {newDriveName}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add drive");
            StatusMessage = $"Error adding drive: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RemoveDrive()
    {
        if (CurrentMotor is null || SelectedDrive is null) return;

        try
        {
            var driveName = SelectedDrive.Name;
            if (CurrentMotor.RemoveDrive(driveName))
            {
                SelectedDrive = CurrentMotor.Drives.FirstOrDefault();
                OnPropertyChanged(nameof(CurrentMotor));
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

    // Voltage management commands
    [RelayCommand]
    private void AddVoltage()
    {
        if (SelectedDrive is null) return;

        try
        {
            // Find the first common voltage not already in use
            var existingVoltages = SelectedDrive.Voltages.Select(v => v.Voltage).ToHashSet();
            var newVoltage = CommonVoltages.FirstOrDefault(v => !existingVoltages.Any(ev => Math.Abs(ev - v) < DriveConfiguration.DefaultVoltageTolerance));
            
            if (newVoltage == 0)
            {
                // All common voltages in use, generate a unique one
                newVoltage = existingVoltages.Max() + 10;
            }
            
            var voltage = SelectedDrive.AddVoltageConfiguration(newVoltage);
            voltage.MaxSpeed = CurrentMotor?.MaxSpeed ?? 5000;
            voltage.Power = CurrentMotor?.Power ?? 1500;
            voltage.RatedPeakTorque = CurrentMotor?.RatedPeakTorque ?? 50;
            voltage.RatedContinuousTorque = CurrentMotor?.RatedContinuousTorque ?? 40;
            
            // Add default series
            var peakSeries = new CurveSeries("Peak");
            var continuousSeries = new CurveSeries("Continuous");
            peakSeries.InitializeData(voltage.MaxSpeed, voltage.RatedPeakTorque);
            continuousSeries.InitializeData(voltage.MaxSpeed, voltage.RatedContinuousTorque);
            voltage.Series.Add(peakSeries);
            voltage.Series.Add(continuousSeries);
            
            SelectedVoltage = voltage;
            OnPropertyChanged(nameof(AvailableVoltages));
            MarkDirty();
            StatusMessage = $"Added voltage: {newVoltage}V";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add voltage");
            StatusMessage = $"Error adding voltage: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RemoveVoltage()
    {
        if (SelectedDrive is null || SelectedVoltage is null) return;

        try
        {
            if (SelectedDrive.Voltages.Count <= 1)
            {
                StatusMessage = "Cannot remove the last voltage configuration.";
                return;
            }

            var voltage = SelectedVoltage.Voltage;
            SelectedDrive.Voltages.Remove(SelectedVoltage);
            SelectedVoltage = SelectedDrive.Voltages.FirstOrDefault();
            OnPropertyChanged(nameof(AvailableVoltages));
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
    private void AddSeries()
    {
        if (SelectedVoltage is null) return;

        try
        {
            var newSeriesName = GenerateUniqueName(
                SelectedVoltage.Series.Select(s => s.Name),
                "New Series");
            
            var series = SelectedVoltage.AddSeries(newSeriesName, SelectedVoltage.RatedContinuousTorque);
            SelectedSeries = series;
            OnPropertyChanged(nameof(AvailableSeries));
            MarkDirty();
            StatusMessage = $"Added series: {newSeriesName}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add series");
            StatusMessage = $"Error adding series: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RemoveSeries()
    {
        if (SelectedVoltage is null || SelectedSeries is null) return;

        try
        {
            if (SelectedVoltage.Series.Count <= 1)
            {
                StatusMessage = "Cannot remove the last series.";
                return;
            }

            var seriesName = SelectedSeries.Name;
            SelectedVoltage.Series.Remove(SelectedSeries);
            SelectedSeries = SelectedVoltage.Series.FirstOrDefault();
            OnPropertyChanged(nameof(AvailableSeries));
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
        if (series is null) return;

        series.Locked = !series.Locked;
        OnPropertyChanged(nameof(AvailableSeries));
        MarkDirty();
        StatusMessage = series.Locked
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
        HasValidationErrors = errors.Count > 0;
        ValidationErrors = errors.Count > 0
            ? string.Join("\n", errors)
            : string.Empty;
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.StorageProvider;
        }
        return null;
    }

    private static string GenerateUniqueName(IEnumerable<string> existingNames, string baseName)
    {
        var names = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!names.Contains(baseName))
        {
            return baseName;
        }

        var counter = 1;
        string newName;
        do
        {
            newName = $"{baseName} {counter++}";
        } while (names.Contains(newName));

        return newName;
    }
}
