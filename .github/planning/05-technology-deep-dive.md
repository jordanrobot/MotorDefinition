# Motor Torque Curve Editor - Technology Deep Dive

## Framework Comparison Details

This document provides additional technical details to support the architecture decision.

---

## Avalonia UI (Recommended)

### What is Avalonia?

Avalonia is a cross-platform XAML-based UI framework for .NET. It's often described as "WPF for everywhere" because it shares many concepts with WPF but runs on Windows, macOS, Linux, iOS, Android, and WebAssembly.

### Example Application Structure

```csharp
// Program.cs
using Avalonia;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
```

```xml
<!-- MainWindow.axaml -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:CurveEditor.ViewModels"
        xmlns:lvc="using:LiveChartsCore.SkiaSharpView.Avalonia"
        x:Class="CurveEditor.Views.MainWindow"
        Title="{Binding WindowTitle}"
        Width="1200" Height="800">
    
    <DockPanel>
        <!-- Menu Bar -->
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_New" Command="{Binding NewCommand}" InputGesture="Ctrl+N"/>
                <MenuItem Header="_Open..." Command="{Binding OpenCommand}" InputGesture="Ctrl+O"/>
                <Separator/>
                <MenuItem Header="_Save" Command="{Binding SaveCommand}" InputGesture="Ctrl+S"/>
                <MenuItem Header="Save _As..." Command="{Binding SaveAsCommand}"/>
                <Separator/>
                <MenuItem Header="E_xit" Command="{Binding ExitCommand}"/>
            </MenuItem>
        </Menu>
        
        <!-- Main Content -->
        <Grid ColumnDefinitions="*, 300">
            <!-- Chart Area -->
            <lvc:CartesianChart Grid.Column="0"
                               Series="{Binding Series}"
                               XAxes="{Binding XAxes}"
                               YAxes="{Binding YAxes}"/>
            
            <!-- Properties Panel -->
            <Border Grid.Column="1" BorderThickness="1,0,0,0" BorderBrush="Gray">
                <StackPanel Margin="10">
                    <TextBlock Text="Curve Properties" FontWeight="Bold"/>
                    <TextBox Text="{Binding CurveName}" Watermark="Curve Name"/>
                    
                    <TextBlock Text="Data Points" FontWeight="Bold" Margin="0,20,0,5"/>
                    <DataGrid ItemsSource="{Binding DataPoints}"
                             AutoGenerateColumns="False"
                             CanUserAddRows="True">
                        <DataGrid.Columns>
                            <DataGridTextColumn Header="RPM" Binding="{Binding Rpm}"/>
                            <DataGridTextColumn Header="Torque (Nm)" Binding="{Binding Torque}"/>
                        </DataGrid.Columns>
                    </DataGrid>
                </StackPanel>
            </Border>
        </Grid>
    </DockPanel>
</Window>
```

### LiveCharts2 Integration

```csharp
// CurveViewModel.cs
public partial class CurveViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<DataPoint> _dataPoints = new();

    public ISeries[] Series => new ISeries[]
    {
        new LineSeries<DataPoint>
        {
            Values = DataPoints,
            Mapping = (point, index) => new Coordinate(point.Rpm, point.Torque),
            Fill = null,
            GeometrySize = 10,
            GeometryFill = new SolidColorPaint(SKColors.SteelBlue),
            GeometryStroke = new SolidColorPaint(SKColors.White, 2),
            LineSmoothness = 0.5
        }
    };

    public Axis[] XAxes => new Axis[]
    {
        new Axis
        {
            Name = "RPM",
            NameTextSize = 14,
            MinLimit = 0
        }
    };

    public Axis[] YAxes => new Axis[]
    {
        new Axis
        {
            Name = "Torque (Nm)",
            NameTextSize = 14,
            MinLimit = 0
        }
    };
}
```

---

## Blazor Hybrid Alternative

If web technologies are preferred, Blazor Hybrid offers a good balance:

### Structure

```
CurveEditor.Blazor/
├── MauiProgram.cs
├── wwwroot/
│   ├── css/
│   └── index.html
├── Components/
│   ├── TorqueChart.razor
│   └── PropertiesPanel.razor
├── Pages/
│   └── Index.razor
└── Services/
    └── CurveService.cs
```

### Chart with ApexCharts Blazor

```razor
@* TorqueChart.razor *@
@using ApexCharts

<ApexChart TItem="DataPoint"
          Title="Torque Curve"
          Height="400">
    <ApexPointSeries TItem="DataPoint"
                    Items="DataPoints"
                    XValue="@(p => p.Rpm)"
                    YValue="@(p => p.Torque)"
                    SeriesType="SeriesType.Line"/>
</ApexChart>

@code {
    [Parameter]
    public List<DataPoint> DataPoints { get; set; } = new();
}
```

---

## WPF Alternative (Windows Only)

For maximum Windows integration:

```xml
<!-- MainWindow.xaml -->
<Window x:Class="CurveEditor.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:lvc="clr-namespace:LiveChartsCore.SkiaSharpView.WPF;assembly=LiveChartsCore.SkiaSharpView.WPF"
        Title="Torque Curve Editor" Height="800" Width="1200">
    
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="300"/>
        </Grid.ColumnDefinitions>
        
        <lvc:CartesianChart Grid.Column="0"
                           Series="{Binding Series}"
                           XAxes="{Binding XAxes}"
                           YAxes="{Binding YAxes}"/>
        
        <DataGrid Grid.Column="1"
                  ItemsSource="{Binding DataPoints}"
                  AutoGenerateColumns="False">
            <DataGrid.Columns>
                <DataGridTextColumn Header="RPM" Binding="{Binding Rpm}"/>
                <DataGridTextColumn Header="Torque" Binding="{Binding Torque}"/>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</Window>
```

---

## Charting Library Comparison

### LiveCharts2
- **Pros**: Modern, animated, works with Avalonia/WPF/MAUI, good docs
- **Cons**: Commercial license for some features
- **Best for**: General purpose interactive charts
- **Install**: `dotnet add package LiveChartsCore.SkiaSharpView.Avalonia`

### ScottPlot
- **Pros**: Free, scientific focus, lightweight, easy to use
- **Cons**: Less interactive features, simpler styling
- **Best for**: Scientific/engineering plots
- **Install**: `dotnet add package ScottPlot.Avalonia`

### OxyPlot
- **Pros**: Mature, scientific focus, many chart types
- **Cons**: Less active development, styling more complex
- **Best for**: Complex scientific visualization
- **Install**: `dotnet add package OxyPlot.Avalonia`

### Recommendation
For interactive torque curve editing, **LiveCharts2** provides the best balance of features, modern design, and ease of use.

---

## Publishing Configuration

### Self-Contained Single File (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
    <PublishSingleFile>true</PublishSingleFile>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
    
    <!-- Optional: Trim to reduce size -->
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>partial</TrimMode>
    
    <!-- Application Info -->
    <AssemblyName>CurveEditor</AssemblyName>
    <Version>1.0.0</Version>
    <Company>Your Company</Company>
    <Product>Torque Curve Editor</Product>
  </PropertyGroup>
</Project>
```

### Build Scripts

**PowerShell (Windows)**
```powershell
# build.ps1
$outputDir = ".\publish"
$project = ".\src\CurveEditor\CurveEditor.csproj"

# Clean
Remove-Item -Recurse -Force $outputDir -ErrorAction SilentlyContinue

# Publish
dotnet publish $project -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $outputDir

Write-Host "Published to $outputDir"
Write-Host "Executable: $outputDir\CurveEditor.exe"
```

**Shell (Cross-platform)**
```bash
#!/bin/bash
# build.sh

OUTPUT_DIR="./publish"
PROJECT="./src/CurveEditor/CurveEditor.csproj"

# Detect platform and architecture
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    if [[ "$(uname -m)" == "aarch64" ]]; then
        RID="linux-arm64"
    else
        RID="linux-x64"
    fi
elif [[ "$OSTYPE" == "darwin"* ]]; then
    if [[ "$(uname -m)" == "arm64" ]]; then
        RID="osx-arm64"
    else
        RID="osx-x64"
    fi
else
    RID="win-x64"
fi

# Clean and publish
rm -rf $OUTPUT_DIR
dotnet publish $PROJECT -c Release -r $RID --self-contained true \
    -p:PublishSingleFile=true \
    -o $OUTPUT_DIR

echo "Published to $OUTPUT_DIR for $RID"
```

---

## Development Environment Setup

### Prerequisites
1. **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **IDE Options**:
   - Visual Studio 2022 (Windows)
   - JetBrains Rider (Cross-platform)
   - VS Code with C# extension

### Avalonia Setup

```bash
# Install Avalonia templates
dotnet new install Avalonia.Templates

# Create project
dotnet new avalonia.mvvm -n CurveEditor -o src/CurveEditor

# Add dependencies
cd src/CurveEditor
dotnet add package LiveChartsCore.SkiaSharpView.Avalonia --version 2.0.0-rc2
dotnet add package CommunityToolkit.Mvvm --version 8.2.2

# Run
dotnet run
```

### IDE Extensions
- **Visual Studio**: Avalonia for Visual Studio
- **Rider**: Avalonia Support (built-in)
- **VS Code**: Avalonia for VS Code

---

## Conclusion

Based on the requirements:
- **C# / .NET 8**: ✅ All recommended options support this
- **Portable**: ✅ Single-file publish available
- **Windows 11**: ✅ Full support
- **Cross-platform potential**: ✅ Avalonia provides this
- **Interactive charting**: ✅ LiveCharts2 excels here
- **Tare NuGet**: ✅ Direct integration possible

**Final Recommendation**: **Avalonia UI + LiveCharts2**

This combination provides the best balance of:
- Modern development experience
- Cross-platform potential
- Interactive charting capability
- Reasonable bundle size
- Strong community and documentation
