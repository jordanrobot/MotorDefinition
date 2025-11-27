# Motor Torque Curve Editor - Recommended Approach

## Primary Recommendation: Avalonia UI with LiveCharts2

After analyzing the requirements against the available options, **Avalonia UI** is the recommended framework for the motor torque curve editor.

### Why Avalonia?

1. **Perfect Fit for Requirements**
   - ✅ C# and .NET 8 native
   - ✅ Single-file portable executable
   - ✅ No installation required
   - ✅ Direct NuGet package support (Tare)
   - ✅ Cross-platform with Linux support
   - ✅ Excellent charting with LiveCharts2

2. **Lightweight and Fast**
   - Smaller bundle size than MAUI or Electron
   - Fast startup time
   - Lower memory footprint

3. **Future-Proof**
   - Can expand to macOS and Linux later
   - Active development and growing community
   - Modern MVVM architecture

4. **Developer Experience**
   - XAML-based (familiar to WPF/MAUI developers)
   - Hot reload support
   - Good IDE support (Visual Studio, Rider)

---

## Alternative Recommendation: WPF with LiveCharts2

If cross-platform is not important and you want the most stable, proven solution:

**WPF** is a solid choice for Windows-only development with:
- Mature and battle-tested
- Smallest learning curve for C# developers
- Excellent tooling support

---

## Recommended Architecture

### MVVM Pattern

```
┌─────────────────────────────────────────────────────────────────┐
│                           View Layer                             │
│  ┌─────────────┐  ┌─────────────────┐  ┌──────────────────────┐ │
│  │  MainWindow │  │   ChartView     │  │   PropertiesPanel    │ │
│  │  (Shell)    │  │   (LiveCharts2) │  │   (DataGrid/Form)    │ │
│  └─────────────┘  └─────────────────┘  └──────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        ViewModel Layer                           │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ MainViewModel   │  │ CurveViewModel  │  │ PointViewModel  │  │
│  │ - File commands │  │ - Chart data    │  │ - RPM           │  │
│  │ - Navigation    │  │ - Edit commands │  │ - Torque        │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         Model Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  MotorData      │  │  CurveSeries    │  │  DataPoint      │  │
│  │  - Name         │  │  - Name         │  │  - Percent      │  │
│  │  - MaxRpm       │  │  - Data[]       │  │  - RPM          │  │
│  │  - Series[]     │  │  - Color        │  │  - Torque       │  │
│  │  - Unit         │  │  - IsVisible    │  │                 │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Services Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  FileService    │  │  UnitService    │  │  ValidationSvc  │  │
│  │  - Load JSON    │  │  - Convert Nm   │  │  - Validate     │  │
│  │  - Save JSON    │  │  - Convert lbf  │  │  - Check limits │  │
│  │  - File dialogs │  │  - (uses Tare)  │  │                 │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Project Structure

```
CurveEditor/
├── CurveEditor.sln
├── src/
│   └── CurveEditor/
│       ├── CurveEditor.csproj
│       ├── App.axaml
│       ├── App.axaml.cs
│       ├── Program.cs
│       ├── Models/
│       │   ├── MotorData.cs
│       │   ├── CurveSeries.cs
│       │   ├── DataPoint.cs
│       │   └── MotorMetadata.cs
│       ├── ViewModels/
│       │   ├── ViewModelBase.cs
│       │   ├── MainWindowViewModel.cs
│       │   ├── ChartViewModel.cs
│       │   ├── SeriesViewModel.cs
│       │   ├── DirectoryBrowserViewModel.cs
│       │   └── PointViewModel.cs
│       ├── Views/
│       │   ├── MainWindow.axaml
│       │   ├── MainWindow.axaml.cs
│       │   ├── ChartView.axaml
│       │   ├── DirectoryBrowserPane.axaml
│       │   └── PropertiesPanel.axaml
│       ├── Services/
│       │   ├── IFileService.cs
│       │   ├── FileService.cs
│       │   ├── IDirectoryService.cs
│       │   ├── DirectoryService.cs
│       │   ├── IUnitService.cs
│       │   ├── UnitService.cs
│       │   ├── IUserPreferencesService.cs
│       │   └── UserPreferencesService.cs
│       └── Assets/
│           └── app-icon.ico
├── tests/
│   └── CurveEditor.Tests/
│       ├── CurveEditor.Tests.csproj
│       ├── Models/
│       │   └── TorqueCurveTests.cs
│       └── Services/
│           └── FileServiceTests.cs
└── samples/
    └── example-curve.json
```

---

## Technology Stack

### Core Framework
- **.NET 8** - Long-term support, latest features
- **Avalonia UI 11.x** - Cross-platform UI framework
- **CommunityToolkit.Mvvm** - MVVM helpers and source generators

### Charting
- **LiveCharts2** - Best interactive charting for Avalonia
  - Supports drag-to-edit points
  - Smooth animations
  - Good documentation

### Additional UI Components
- **SkiaSharp** - For background image rendering and scaling
- Custom slider controls for Q value and axis scaling

### Dependencies (NuGet Packages)
```xml
<ItemGroup>
  <PackageReference Include="Avalonia" Version="11.1.*" />
  <PackageReference Include="Avalonia.Desktop" Version="11.1.*" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.1.*" />
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.*" />
  <PackageReference Include="LiveChartsCore.SkiaSharpView.Avalonia" Version="2.0.*" />
  <PackageReference Include="System.Text.Json" Version="8.0.*" />
  <!-- Future: Add Tare for units -->
  <!-- <PackageReference Include="Tare" Version="x.x.x" /> -->
</ItemGroup>
```

---

## Key Features Implementation

### 1. EQ-Style Interactive Chart Editing

LiveCharts2 supports interactive point manipulation with drag behavior:

```csharp
// Example: Draggable points on chart with Q-based smoothing
public partial class CurveViewModel : ViewModelBase
{
    [ObservableProperty]
    private double _qValue = 0.5; // Range 0.0 to 1.0
    
    public ISeries[] Series => new ISeries[]
    {
        new LineSeries<ObservablePoint>
        {
            Values = CurvePoints,
            Fill = null,
            GeometrySize = 12,
            LineSmoothness = QValue, // Q affects smoothness
            DataPointerDown = OnPointClicked,
        }
    };
    
    // Apply Q-based influence to adjacent points when dragging
    private void ApplyQInfluence(int pointIndex, double deltaY)
    {
        // Lower Q = sharper changes (affects fewer neighbors)
        // Higher Q = gradual changes (affects more neighbors)
        int influence = (int)(QValue * 5); // 0-5 adjacent points
        for (int i = 1; i <= influence; i++)
        {
            double factor = 1.0 - (i / (influence + 1.0));
            // Apply scaled delta to neighboring points
        }
    }
}
```

### 2. Background Image Overlay

Load and scale reference images behind the chart:

```csharp
public partial class ChartViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _backgroundImagePath;
    
    [ObservableProperty]
    private double _imageScaleX = 1.0;
    
    [ObservableProperty]
    private double _imageScaleY = 1.0;
    
    public async Task LoadBackgroundImageAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Load Background Image",
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Images", Extensions = { "png", "jpg", "jpeg", "bmp" } }
            }
        };
        
        var result = await dialog.ShowAsync(_window);
        if (result?.Length > 0)
        {
            BackgroundImagePath = result[0];
        }
    }
}
```

### 3. Axis Scaling with Sliders

```csharp
public partial class ChartViewModel : ViewModelBase
{
    [ObservableProperty]
    private double _xAxisMin = 0;
    
    [ObservableProperty]
    private double _xAxisMax = 5000;
    
    [ObservableProperty]
    private double _yAxisMin = 0;
    
    [ObservableProperty]
    private double _yAxisMax = 100;
    
    public Axis[] XAxes => new Axis[]
    {
        new Axis
        {
            Name = "RPM",
            MinLimit = XAxisMin,
            MaxLimit = XAxisMax,
            // Auto-calculate nice step values
            MinStep = CalculateNiceStep(XAxisMax - XAxisMin),
            LabelsPaint = new SolidColorPaint(SKColors.DarkGray),
            SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(100))
        }
    };
    
    private double CalculateNiceStep(double range)
    {
        // Return nice round numbers: 100, 250, 500, 1000, etc.
        double rough = range / 10;
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rough)));
        double normalized = rough / magnitude;
        
        if (normalized < 2) return magnitude;
        if (normalized < 5) return 2 * magnitude;
        return 5 * magnitude;
    }
}
```

### 4. Grid Lines and Labels

LiveCharts2 automatically creates grid lines with the separator paint:

```csharp
public Axis[] YAxes => new Axis[]
{
    new Axis
    {
        Name = "Torque (Nm)",
        MinLimit = YAxisMin,
        MaxLimit = YAxisMax,
        MinStep = CalculateNiceStep(YAxisMax - YAxisMin),
        // Faded grid lines
        SeparatorsPaint = new SolidColorPaint(
            new SKColor(200, 200, 200, 80) // Light gray with transparency
        ),
        // Axis labels
        LabelsPaint = new SolidColorPaint(SKColors.DarkGray),
        LabelsRotation = 0
    }
};
```

### 5. Hover Tooltip

```csharp
public partial class ChartViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _showHoverTooltip = true; // User preference
    
    public SolidColorPaint? TooltipBackgroundPaint => ShowHoverTooltip 
        ? new SolidColorPaint(new SKColor(240, 240, 240, 220))
        : null;
    
    // Display RPM rounded to nearest whole number
    public Func<ChartPoint, string> TooltipFormatter => point =>
        $"RPM: {Math.Round(point.SecondaryValue):N0}\nTorque: {point.PrimaryValue:N1} Nm";
}
```

XAML for tooltip positioning:
```xml
<lvc:CartesianChart 
    Series="{Binding Series}"
    TooltipPosition="Top"
    TooltipBackgroundPaint="{Binding TooltipBackgroundPaint}"
    TooltipTextPaint="{Binding TooltipTextPaint}">
</lvc:CartesianChart>
```

### 6. Multiple Series Support

Load and display multiple curve series with individual visibility control:

```csharp
public partial class ChartViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<SeriesViewModel> _curveSeries = new();
    
    public ISeries[] ChartSeries => CurveSeries
        .Where(s => s.IsVisible)
        .Select(s => new LineSeries<DataPoint>
        {
            Values = s.DataPoints,
            Name = s.Name,
            Stroke = new SolidColorPaint(s.Color, 2),
            GeometryStroke = new SolidColorPaint(s.Color, 2),
            GeometryFill = new SolidColorPaint(s.Color),
            GeometrySize = 8,
            Fill = null,
            Mapping = (point, _) => new Coordinate(Math.Round(point.Rpm), point.Torque)
        })
        .ToArray();
    
    public void CreateDefaultSeries()
    {
        CurveSeries.Add(new SeriesViewModel("Peak", SKColors.Blue));
        CurveSeries.Add(new SeriesViewModel("Continuous", SKColors.Green));
    }
    
    public void AddSeries(string name)
    {
        var color = _preferencesService.GetColorForSeries(name) 
            ?? GenerateNextColor();
        CurveSeries.Add(new SeriesViewModel(name, color));
    }
}

public partial class SeriesViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name;
    
    [ObservableProperty]
    private bool _isVisible = true;
    
    [ObservableProperty]
    private SKColor _color;
    
    [ObservableProperty]
    private ObservableCollection<DataPoint> _dataPoints = new();
    
    partial void OnColorChanged(SKColor value)
    {
        // Notify chart to refresh
        OnPropertyChanged(nameof(Color));
    }
}
```

### 7. Series List with Visibility Checkboxes

XAML for series list panel:
```xml
<ItemsControl ItemsSource="{Binding CurveSeries}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal" Margin="5">
                <CheckBox IsChecked="{Binding IsVisible}" />
                <Rectangle Width="20" Height="3" 
                          Fill="{Binding Color, Converter={StaticResource ColorToBrush}}"
                          Margin="5,0"/>
                <TextBox Text="{Binding Name}" MinWidth="100"/>
                <Button Content="🎨" Command="{Binding EditColorCommand}" 
                        ToolTip.Tip="Edit color"/>
                <Button Content="❌" Command="{Binding $parent[ItemsControl].DataContext.DeleteSeriesCommand}"
                        CommandParameter="{Binding}"
                        ToolTip.Tip="Delete series"/>
            </StackPanel>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
<Button Content="+ Add Series" Command="{Binding AddSeriesCommand}"/>
```

### 8. Data Format (1% Increments)

Data model for percentage-based curve data:

```csharp
public class DataPoint
{
    /// <summary>Percentage (0-100) where 0% = 0 RPM, 100% = MaxRpm</summary>
    public int Percent { get; set; }
    
    /// <summary>RPM value (calculated from Percent × MaxRpm)</summary>
    public double Rpm { get; set; }
    
    /// <summary>Torque value at this percentage point</summary>
    public double Torque { get; set; }
    
    /// <summary>Get RPM rounded to nearest whole number for display</summary>
    public int DisplayRpm => (int)Math.Round(Rpm);
}

public class CurveSeries
{
    public string Name { get; set; } = "Peak";
    public List<DataPoint> Data { get; set; } = new();
    
    /// <summary>Generate 101 data points (0% to 100%)</summary>
    public void InitializeData(double maxRpm, double defaultTorque)
    {
        Data.Clear();
        for (int percent = 0; percent <= 100; percent++)
        {
            Data.Add(new DataPoint
            {
                Percent = percent,
                Rpm = percent / 100.0 * maxRpm,
                Torque = defaultTorque
            });
        }
    }
}
```

### 9. User Preferences Service

Persist series colors and other user settings:

```csharp
public interface IUserPreferencesService
{
    SKColor? GetColorForSeries(string seriesName);
    void SetColorForSeries(string seriesName, SKColor color);
    bool ShowHoverTooltip { get; set; }
    void Save();
    void Load();
}

public class UserPreferencesService : IUserPreferencesService
{
    private readonly string _prefsPath;
    private UserPreferences _prefs = new();
    
    public UserPreferencesService()
    {
        _prefsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CurveEditor",
            "preferences.json"
        );
        Load();
    }
    
    public SKColor? GetColorForSeries(string seriesName)
    {
        if (_prefs.SeriesColors.TryGetValue(seriesName, out var hex))
            return SKColor.Parse(hex);
        return null;
    }
    
    public void SetColorForSeries(string seriesName, SKColor color)
    {
        _prefs.SeriesColors[seriesName] = color.ToString();
        Save();
    }
}

public class UserPreferences
{
    public Dictionary<string, string> SeriesColors { get; set; } = new()
    {
        ["Peak"] = "#FF0000FF",      // Blue
        ["Continuous"] = "#FF00AA00"  // Green
    };
    public bool ShowHoverTooltip { get; set; } = true;
}
```

### 10. File Operations

File service with Save, Save As, and Save Copy As:

```csharp
public interface IFileService
{
    Task<MotorData?> LoadAsync(string filePath);
    Task SaveAsync(MotorData data, string filePath);
    string? CurrentFilePath { get; }
    bool IsDirty { get; }
    void MarkDirty();
    void ClearDirty();
}

public class FileService : IFileService
{
    public string? CurrentFilePath { get; private set; }
    public bool IsDirty { get; private set; }
    
    public async Task<MotorData?> LoadAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<MotorData>(json);
        CurrentFilePath = filePath;
        IsDirty = false;
        return data;
    }
    
    public async Task SaveAsync(MotorData data, string filePath)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(filePath, json); // Always overwrites
        CurrentFilePath = filePath;
        IsDirty = false;
    }
    
    public void MarkDirty() => IsDirty = true;
    public void ClearDirty() => IsDirty = false;
}
```

Save As vs Save Copy As:

```csharp
public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>Save current file (overwrite)</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_fileService.CurrentFilePath != null)
        {
            await _fileService.SaveAsync(_motorData, _fileService.CurrentFilePath);
        }
        else
        {
            await SaveAsAsync();
        }
    }
    
    /// <summary>Save as new file, new file becomes active</summary>
    [RelayCommand]
    private async Task SaveAsAsync()
    {
        var path = await ShowSaveDialogAsync();
        if (path != null)
        {
            await _fileService.SaveAsync(_motorData, path);
            // New file is now the active file (CurrentFilePath updated in SaveAsync)
            UpdateWindowTitle();
        }
    }
    
    /// <summary>Save copy to new file, original file stays active</summary>
    [RelayCommand]
    private async Task SaveCopyAsAsync()
    {
        var path = await ShowSaveDialogAsync();
        if (path != null)
        {
            var originalPath = _fileService.CurrentFilePath;
            await _fileService.SaveAsync(_motorData, path);
            // Restore original file as active
            _fileService.CurrentFilePath = originalPath;
            // Note: Dirty state preserved from original file
        }
    }
}
```

### 11. Directory Browser (VS Code-style)

Side pane for browsing and selecting files:

```csharp
public partial class DirectoryBrowserViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _currentDirectory;
    
    [ObservableProperty]
    private ObservableCollection<FileItemViewModel> _files = new();
    
    [ObservableProperty]
    private FileItemViewModel? _selectedFile;
    
    [RelayCommand]
    private async Task OpenDirectoryAsync()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Open Directory"
        };
        
        var result = await dialog.ShowAsync(_window);
        if (result != null)
        {
            CurrentDirectory = result;
            RefreshFileList();
        }
    }
    
    private void RefreshFileList()
    {
        Files.Clear();
        if (CurrentDirectory == null) return;
        
        var jsonFiles = Directory.GetFiles(CurrentDirectory, "*.json");
        foreach (var file in jsonFiles.OrderBy(f => f))
        {
            Files.Add(new FileItemViewModel
            {
                FileName = Path.GetFileName(file),
                FullPath = file
            });
        }
    }
    
    partial void OnSelectedFileChanged(FileItemViewModel? value)
    {
        if (value != null)
        {
            // Notify main view model to load the file
            _messenger.Send(new FileSelectedMessage(value.FullPath));
        }
    }
}

public class FileItemViewModel
{
    public string FileName { get; set; } = "";
    public string FullPath { get; set; } = "";
}
```

XAML for directory browser pane:
```xml
<UserControl x:Class="CurveEditor.Views.DirectoryBrowserPane">
    <DockPanel>
        <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Margin="5">
            <Button Content="📁 Open Folder" Command="{Binding OpenDirectoryCommand}"/>
            <Button Content="🔄" Command="{Binding RefreshCommand}" ToolTip.Tip="Refresh"/>
        </StackPanel>
        
        <TextBlock DockPanel.Dock="Top" Text="{Binding CurrentDirectory}" 
                   FontSize="11" Margin="5" TextTrimming="CharacterEllipsis"/>
        
        <ListBox ItemsSource="{Binding Files}" 
                 SelectedItem="{Binding SelectedFile}">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <StackPanel Orientation="Horizontal">
                        <TextBlock Text="📄" Margin="0,0,5,0"/>
                        <TextBlock Text="{Binding FileName}"/>
                    </StackPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
    </DockPanel>
</UserControl>
```

### 12. Dirty State and Save Prompts

Track unsaved changes and prompt before losing data:

```csharp
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isDirty;
    
    public string WindowTitle => _fileService.CurrentFilePath != null
        ? $"{Path.GetFileName(_fileService.CurrentFilePath)}{(IsDirty ? " *" : "")} - Curve Editor"
        : "Curve Editor";
    
    /// <summary>Mark file as dirty when any edit is made</summary>
    private void OnDataChanged()
    {
        IsDirty = true;
        _fileService.MarkDirty();
        OnPropertyChanged(nameof(WindowTitle));
    }
    
    /// <summary>Check for unsaved changes before opening new file</summary>
    public async Task<bool> ConfirmDiscardChangesAsync()
    {
        if (!IsDirty) return true;
        
        var result = await ShowSavePromptAsync(
            "Unsaved Changes",
            "Do you want to save changes before continuing?"
        );
        
        switch (result)
        {
            case SavePromptResult.Save:
                await SaveAsync();
                return true;
            case SavePromptResult.DontSave:
                return true;
            case SavePromptResult.Cancel:
            default:
                return false;
        }
    }
    
    /// <summary>Handle file selected from directory browser</summary>
    private async Task OnFileSelectedAsync(string filePath)
    {
        if (!await ConfirmDiscardChangesAsync())
            return;
            
        await LoadFileAsync(filePath);
    }
    
    /// <summary>Handle window closing</summary>
    public async Task<bool> OnClosingAsync()
    {
        return await ConfirmDiscardChangesAsync();
    }
}

public enum SavePromptResult
{
    Save,
    DontSave,
    Cancel
}
```

Save prompt dialog:
```csharp
private async Task<SavePromptResult> ShowSavePromptAsync(string title, string message)
{
    var dialog = new ContentDialog
    {
        Title = title,
        Content = message,
        PrimaryButtonText = "Save",
        SecondaryButtonText = "Don't Save",
        CloseButtonText = "Cancel"
    };
    
    var result = await dialog.ShowAsync();
    
    return result switch
    {
        ContentDialogResult.Primary => SavePromptResult.Save,
        ContentDialogResult.Secondary => SavePromptResult.DontSave,
        _ => SavePromptResult.Cancel
    };
}
```

### 13. Unit Conversion (Future)

Prepared for Tare integration:

```csharp
public interface IUnitService
{
    double ConvertTorque(double value, TorqueUnit from, TorqueUnit to);
    TorqueUnit CurrentUnit { get; set; }
}

// Future implementation with Tare:
public class UnitService : IUnitService
{
    public double ConvertTorque(double value, TorqueUnit from, TorqueUnit to)
    {
        // Using Tare package for conversions
        // return Tare.Torque.Convert(value, from, to);
        throw new NotImplementedException("Implement with Tare package");
    }
}
```

---

## Deployment

### Publish as Single File

```bash
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true

# Optional: Trim unused code for smaller size
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true
```

### Expected Output Size
- Self-contained single file: ~50-70 MB
- Trimmed version: ~30-50 MB

---

## Getting Started

See [04-mvp-roadmap.md](./04-mvp-roadmap.md) for the implementation roadmap.
