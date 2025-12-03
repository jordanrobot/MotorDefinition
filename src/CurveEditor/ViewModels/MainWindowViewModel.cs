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
    private DriveConfiguration? _selectedDrive;

    // Voltage selection
    [ObservableProperty]
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
    /// ViewModel for the curve data table.
    /// </summary>
    [ObservableProperty]
    private CurveDataTableViewModel _curveDataTableViewModel = new();

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

    public MainWindowViewModel()
    {
        _curveGeneratorService = new CurveGeneratorService();
        _fileService = new FileService(_curveGeneratorService);
        _validationService = new ValidationService();
        ChartViewModel.DataChanged += OnChartDataChanged;
        CurveDataTableViewModel.DataChanged += OnDataTableDataChanged;
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

    partial void OnCurrentMotorChanged(MotorDefinition? value)
    {
        // Refresh the drives collection
        RefreshAvailableDrives();
        
        // When motor changes, select the first drive
        SelectedDrive = value?.Drives.FirstOrDefault();
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
        ChartViewModel.CurrentVoltage = value;

        // Update data table with new voltage configuration
        CurveDataTableViewModel.CurrentVoltage = value;
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

    [RelayCommand]
    private async Task NewMotorAsync()
    {
        Log.Information("Creating new motor definition");

        // Check if current file is dirty and prompt user
        if (IsDirty)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialog = new Views.MessageDialog
                {
                    Title = "Unsaved Changes",
                    Message = "You have unsaved changes. Would you like to save them before creating a new file?",
                    ShowCancelButton = true,
                    OkButtonText = "Save",
                    CancelButtonText = "Ignore"
                };
                await dialog.ShowDialog(desktop.MainWindow!);
                
                if (dialog.Result == true)
                {
                    // User wants to save
                    await SaveAsync();
                    if (IsDirty)
                    {
                        // Save was cancelled or failed
                        StatusMessage = "New file cancelled.";
                        return;
                    }
                }
                else if (dialog.Result is null)
                {
                    // User closed dialog without choosing
                    StatusMessage = "New file cancelled.";
                    return;
                }
                // If dialog.Result == false, user chose "Ignore", continue with new file
            }
        }

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

    /// <summary>
    /// Shows the Add Drive/Voltage dialog and creates the drive with curve series.
    /// </summary>
    [RelayCommand]
    private async Task AddDriveAsync()
    {
        if (CurrentMotor is null) return;

        try
        {
            var dialog = new Views.AddDriveVoltageDialog();
            dialog.Initialize(
                CurrentMotor.MaxSpeed,
                CurrentMotor.RatedPeakTorque,
                CurrentMotor.RatedContinuousTorque,
                CurrentMotor.Power);

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await dialog.ShowDialog(desktop.MainWindow!);
            }

            if (dialog.Result is null) return;

            var result = dialog.Result;
            var driveName = string.IsNullOrWhiteSpace(result.Name)
                ? GenerateUniqueName(CurrentMotor.Drives.Select(d => d.Name), "New Drive")
                : result.Name;

            var drive = CurrentMotor.AddDrive(driveName);
            drive.PartNumber = result.PartNumber;
            drive.Manufacturer = result.Manufacturer;

            // Add the voltage configuration
            var voltage = drive.AddVoltageConfiguration(result.Voltage);
            voltage.MaxSpeed = result.MaxSpeed;
            voltage.Power = result.Power;
            voltage.RatedPeakTorque = result.PeakTorque;
            voltage.RatedContinuousTorque = result.ContinuousTorque;
            voltage.ContinuousAmperage = result.ContinuousCurrent;
            voltage.PeakAmperage = result.PeakCurrent;

            // Create Peak and Continuous torque series
            var peakSeries = new CurveSeries("Peak");
            var continuousSeries = new CurveSeries("Continuous");
            peakSeries.InitializeData(voltage.MaxSpeed, voltage.RatedPeakTorque);
            continuousSeries.InitializeData(voltage.MaxSpeed, voltage.RatedContinuousTorque);
            voltage.Series.Add(peakSeries);
            voltage.Series.Add(continuousSeries);

            // Refresh the drive list in UI
            RefreshAvailableDrives();
            
            // Select the new drive and update chart
            SelectedDrive = drive;
            
            // Explicitly refresh chart to ensure axes are updated with new max speed
            ChartViewModel.RefreshChart();
            
            MarkDirty();
            StatusMessage = $"Added drive: {driveName}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add drive");
            StatusMessage = $"Error adding drive: {ex.Message}";
        }
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
        if (SelectedDrive is null) return;

        try
        {
            var dialog = new Views.AddDriveVoltageDialog();
            dialog.Title = "Add New Voltage Configuration";
            dialog.Initialize(
                CurrentMotor?.MaxSpeed ?? 5000,
                CurrentMotor?.RatedPeakTorque ?? 50,
                CurrentMotor?.RatedContinuousTorque ?? 40,
                CurrentMotor?.Power ?? 1500);

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await dialog.ShowDialog(desktop.MainWindow!);
            }

            if (dialog.Result is null) return;

            var result = dialog.Result;
            
            // Check if voltage already exists
            if (SelectedDrive.Voltages.Any(v => Math.Abs(v.Voltage - result.Voltage) < DriveConfiguration.DefaultVoltageTolerance))
            {
                StatusMessage = $"Voltage {result.Voltage}V already exists for this drive.";
                return;
            }

            var voltage = SelectedDrive.AddVoltageConfiguration(result.Voltage);
            voltage.MaxSpeed = result.MaxSpeed;
            voltage.Power = result.Power;
            voltage.RatedPeakTorque = result.PeakTorque;
            voltage.RatedContinuousTorque = result.ContinuousTorque;
            voltage.ContinuousAmperage = result.ContinuousCurrent;
            voltage.PeakAmperage = result.PeakCurrent;

            // Create Peak and Continuous torque series
            var peakSeries = new CurveSeries("Peak");
            var continuousSeries = new CurveSeries("Continuous");
            peakSeries.InitializeData(voltage.MaxSpeed, voltage.RatedPeakTorque);
            continuousSeries.InitializeData(voltage.MaxSpeed, voltage.RatedContinuousTorque);
            voltage.Series.Add(peakSeries);
            voltage.Series.Add(continuousSeries);

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
        if (SelectedVoltage is null) return;

        try
        {
            var dialog = new Views.AddCurveSeriesDialog();
            dialog.Initialize(
                SelectedVoltage.MaxSpeed,
                SelectedVoltage.RatedContinuousTorque,
                SelectedVoltage.Power);

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await dialog.ShowDialog(desktop.MainWindow!);
            }

            if (dialog.Result is null) return;

            var result = dialog.Result;
            var seriesName = GenerateUniqueName(
                SelectedVoltage.Series.Select(s => s.Name),
                result.Name);
            
            var series = SelectedVoltage.AddSeries(seriesName, result.BaseTorque);
            series.IsVisible = result.IsVisible;
            series.Locked = result.IsLocked;
            
            RefreshAvailableSeries();
            CurveDataTableViewModel.RefreshData();
            SelectedSeries = series;
            ChartViewModel.RefreshChart();
            MarkDirty();
            StatusMessage = $"Added series: {seriesName}";
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
