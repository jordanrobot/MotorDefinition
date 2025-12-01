using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurveEditor.Models;
using CurveEditor.Services;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CurveEditor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFileService _fileService;
    private readonly ICurveGeneratorService _curveGeneratorService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private MotorDefinition? _currentMotor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public MainWindowViewModel()
    {
        _curveGeneratorService = new CurveGeneratorService();
        _fileService = new FileService(_curveGeneratorService);
    }

    public MainWindowViewModel(IFileService fileService, ICurveGeneratorService curveGeneratorService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _curveGeneratorService = curveGeneratorService ?? throw new ArgumentNullException(nameof(curveGeneratorService));
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
            // TODO: In a full implementation, this would open a file dialog.
            // For now, we search for the sample file in common locations.
            var samplePath = FindSampleFile();

            if (samplePath is not null && File.Exists(samplePath))
            {
                CurrentMotor = await _fileService.LoadAsync(samplePath);
                IsDirty = _fileService.IsDirty;
                StatusMessage = $"Loaded: {Path.GetFileName(samplePath)}";
                OnPropertyChanged(nameof(WindowTitle));
            }
            else
            {
                StatusMessage = "Sample file not found. Use File > New to create a motor.";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private static string? FindSampleFile()
    {
        // Search for the sample file in multiple locations
        var possiblePaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "example-motor.json"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "samples", "example-motor.json"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "samples", "example-motor.json"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "samples", "example-motor.json"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "samples", "example-motor.json"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "samples", "example-motor.json"),
        };

        foreach (var path in possiblePaths)
        {
            var normalizedPath = Path.GetFullPath(path);
            if (File.Exists(normalizedPath))
            {
                return normalizedPath;
            }
        }

        return null;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (CurrentMotor is null) return;

        try
        {
            if (_fileService.CurrentFilePath is null)
            {
                // No file path yet, would need SaveAs dialog
                StatusMessage = "Use Save As to save new file";
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

        try
        {
            // For now, save to a temp location - in real implementation, show dialog
            var tempPath = Path.Combine(Path.GetTempPath(), $"{CurrentMotor.MotorName}.json");
            await _fileService.SaveAsAsync(CurrentMotor, tempPath);
            IsDirty = false;
            StatusMessage = $"Saved to: {tempPath}";
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
            // Save a copy without changing the current file
            var copyPath = Path.Combine(Path.GetTempPath(), $"{CurrentMotor.MotorName}_copy.json");
            await _fileService.SaveCopyAsAsync(CurrentMotor, copyPath);
            StatusMessage = $"Copy saved to: {copyPath}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save copy");
            StatusMessage = $"Error saving copy: {ex.Message}";
        }
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
    }
}
