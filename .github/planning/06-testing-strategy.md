# Motor Torque Curve Editor - Testing Strategy

This document outlines comprehensive testing strategies for regression prevention and bug detection throughout the development lifecycle.

---

## Testing Philosophy

### Guiding Principles

1. **Test Early, Test Often** - Write tests alongside or before implementation
2. **Behavior-Driven** - Tests describe expected behavior, not implementation details
3. **Regression Prevention** - Every bug fix includes a test that would have caught it
4. **Isolation** - Unit tests are isolated; integration tests verify component interaction

---

## Test Categories

### 1. Unit Tests

Unit tests verify individual components in isolation.

#### Model Tests

| Test Area | Test Cases | Priority |
|-----------|------------|----------|
| **DataPoint** | Valid construction (percent >= 0, positive RPM/torque) | High |
| | Invalid percent values (negative) throws | High |
| | DisplayRpm rounds correctly | Medium |
| | Equality comparison | Low |
| **CurveSeries** | InitializeData creates 101 points (0%-100%) | High |
| | Default points are at exact 1% increments | High |
| | RPM values calculated correctly from percent × maxRpm | High |
| | Name validation (non-empty) | Medium |
| **MotorDefinition** | All properties serialize to JSON correctly | High |
| | JSON deserialization preserves all values | High |
| | Default values are sensible | Medium |
| | Units object defaults correctly | Medium |
| **UnitSettings** | Valid unit strings accepted | Medium |
| | Invalid unit strings rejected | Medium |

```csharp
// Example: DataPoint Tests
public class DataPointTests
{
    [Fact]
    public void Constructor_ValidValues_CreatesDataPoint()
    {
        var point = new DataPoint { Percent = 50, Rpm = 2500, Torque = 45.5 };
        
        Assert.Equal(50, point.Percent);
        Assert.Equal(2500, point.Rpm);
        Assert.Equal(45.5, point.Torque);
    }

    [Theory]
    [InlineData(-1)]
    public void Percent_OutOfRange_ThrowsArgumentException(int invalidPercent)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new DataPoint { Percent = invalidPercent, Rpm = 0, Torque = 0 });
    }

    [Theory]
    [InlineData(1000.4, 1000)]
    [InlineData(1000.5, 1001)]
    [InlineData(1000.6, 1001)]
    public void DisplayRpm_RoundsToNearestWholeNumber(double rpm, int expected)
    {
        var point = new DataPoint { Percent = 50, Rpm = rpm, Torque = 45.0 };
        Assert.Equal(expected, point.DisplayRpm);
    }
}
```

#### Service Tests

| Service | Test Cases | Priority |
|---------|------------|----------|
| **FileService** | Load valid JSON file | High |
| | Load file with missing optional fields | High |
| | Load invalid JSON throws appropriate exception | High |
| | Save creates valid JSON | High |
| | SaveAs updates CurrentFilePath | High |
| | SaveCopyAs does not update CurrentFilePath | High |
| | IsDirty flag management | High |
| | File not found handling | Medium |
| **CurveGeneratorService** | InterpolateCurve creates 101 points | High |
| | Corner speed calculated correctly | High |
| | Constant torque region correct | High |
| | Constant power falloff region correct | High |
| | CalculatePower formula correct | High |
| | Edge case: maxPower = 0 | Medium |
| | Edge case: maxTorque = 0 | Medium |
| **UserPreferencesService** | GetColorForSeries returns saved color | Medium |
| | SetColorForSeries persists | Medium |
| | Default colors for Peak/Continuous | Medium |
| | Preferences survive app restart | Medium |

```csharp
// Example: CurveGeneratorService Tests
public class CurveGeneratorServiceTests
{
    private readonly CurveGeneratorService _service = new();

    [Fact]
    public void InterpolateCurve_Creates101DataPoints()
    {
        var points = _service.InterpolateCurve(maxRpm: 5000, maxTorque: 50, maxPower: 1500);
        
        Assert.Equal(101, points.Count);
        Assert.Equal(0, points.First().Percent);
        Assert.Equal(100, points.Last().Percent);
    }

    [Fact]
    public void InterpolateCurve_PointsAtExact1PercentIncrements()
    {
        var points = _service.InterpolateCurve(maxRpm: 5000, maxTorque: 50, maxPower: 1500);
        
        for (int i = 0; i <= 100; i++)
        {
            Assert.Equal(i, points[i].Percent);
        }
    }

    [Fact]
    public void InterpolateCurve_RpmCalculatedCorrectly()
    {
        var points = _service.InterpolateCurve(maxRpm: 5000, maxTorque: 50, maxPower: 1500);
        
        Assert.Equal(0, points[0].Rpm);
        Assert.Equal(2500, points[50].Rpm);
        Assert.Equal(5000, points[100].Rpm);
    }

    [Theory]
    [InlineData(50.0, 1000, 5235.99)]  // P = T × RPM × 2π/60
    [InlineData(45.0, 3000, 14137.17)]
    public void CalculatePower_FormulaCorrect(double torque, double rpm, double expectedWatts)
    {
        var result = _service.CalculatePower(torque, rpm);
        Assert.Equal(expectedWatts, result, precision: 1);
    }

    [Fact]
    public void InterpolateCurve_ConstantTorqueRegion_MaintainsTorque()
    {
        // At low speeds (before corner speed), torque should be constant
        var points = _service.InterpolateCurve(maxRpm: 5000, maxTorque: 50, maxPower: 1500);
        
        // Find points in constant torque region (before corner speed)
        var cornerRpm = (1500.0 * 60) / (50 * 2 * Math.PI); // ~286 RPM
        var lowSpeedPoints = points.Where(p => p.Rpm < cornerRpm * 0.9).ToList();
        
        foreach (var point in lowSpeedPoints)
        {
            Assert.Equal(50, point.Torque, precision: 1);
        }
    }
}
```

### 2. ViewModel Tests

ViewModel tests verify presentation logic and command execution.

| ViewModel | Test Cases | Priority |
|-----------|------------|----------|
| **MainWindowViewModel** | WindowTitle shows filename | Medium |
| | WindowTitle shows asterisk when dirty | High |
| | SaveCommand enabled when dirty | High |
| | OpenCommand prompts to save if dirty | High |
| | ExitCommand prompts to save if dirty | High |
| **ChartViewModel** | Series property returns correct ISeries array | High |
| | Hidden series excluded from chart | High |
| | Color changes reflect in series | Medium |
| | Q value affects curve smoothness | Medium |
| **SeriesViewModel** | IsVisible toggle updates chart | High |
| | Name change marks file dirty | High |
| | Color change persisted to preferences | Medium |
| **DirectoryBrowserViewModel** | Files list populated on directory open | High |
| | Only JSON files shown | Medium |
| | File selection triggers load | High |

```csharp
// Example: MainWindowViewModel Tests
public class MainWindowViewModelTests
{
    private readonly Mock<IFileService> _mockFileService = new();
    
    [Fact]
    public void WindowTitle_NoFile_ShowsAppName()
    {
        _mockFileService.Setup(f => f.CurrentFilePath).Returns((string?)null);
        var vm = new MainWindowViewModel(_mockFileService.Object);
        
        Assert.Equal("Curve Editor", vm.WindowTitle);
    }

    [Fact]
    public void WindowTitle_FileLoaded_ShowsFilename()
    {
        _mockFileService.Setup(f => f.CurrentFilePath).Returns(@"C:\motors\motor1.json");
        var vm = new MainWindowViewModel(_mockFileService.Object);
        
        Assert.Contains("motor1.json", vm.WindowTitle);
    }

    [Fact]
    public void WindowTitle_DirtyFile_ShowsAsterisk()
    {
        _mockFileService.Setup(f => f.CurrentFilePath).Returns(@"C:\motors\motor1.json");
        _mockFileService.Setup(f => f.IsDirty).Returns(true);
        var vm = new MainWindowViewModel(_mockFileService.Object);
        
        Assert.Contains("*", vm.WindowTitle);
    }
}
```

### 3. Integration Tests

Integration tests verify component interaction.

| Integration Area | Test Cases | Priority |
|-----------------|------------|----------|
| **File Round-Trip** | Save then load preserves all data | Critical |
| | Multiple series preserved | High |
| | 101 data points preserved per series | High |
| | Metadata preserved | Medium |
| **Chart-Data Binding** | Data grid edit updates chart | High |
| | Chart edit updates data grid | High |
| | Series visibility toggle works | High |
| **Directory Browser** | Click file loads into editor | High |
| | Dirty prompt shown when switching files | High |
| **Save Workflow** | Save overwrites existing file | High |
| | Save As creates new file, becomes active | High |
| | Save Copy As creates copy, original stays active | High |

```csharp
// Example: File Round-Trip Integration Test
public class FileRoundTripTests
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public FileRoundTripTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task SaveAndLoad_PreservesAllMotorProperties()
    {
        var fileService = new FileService();
        var original = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "test-motor.json");

        // Save
        await fileService.SaveAsync(original, filePath);

        // Load
        var loaded = await fileService.LoadAsync(filePath);

        // Verify all properties
        Assert.Equal(original.MotorName, loaded.MotorName);
        Assert.Equal(original.Manufacturer, loaded.Manufacturer);
        Assert.Equal(original.PartNumber, loaded.PartNumber);
        Assert.Equal(original.MaxRpm, loaded.MaxRpm);
        Assert.Equal(original.RatedPeakTorque, loaded.RatedPeakTorque);
        Assert.Equal(original.HasBrake, loaded.HasBrake);
        // ... verify all other properties
    }

    [Fact]
    public async Task SaveAndLoad_PreservesAllSeriesData()
    {
        var fileService = new FileService();
        var original = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "test-motor.json");

        await fileService.SaveAsync(original, filePath);
        var loaded = await fileService.LoadAsync(filePath);

        Assert.Equal(original.Series.Count, loaded.Series.Count);
        
        for (int s = 0; s < original.Series.Count; s++)
        {
            var origSeries = original.Series[s];
            var loadSeries = loaded.Series[s];
            
            Assert.Equal(origSeries.Name, loadSeries.Name);
            Assert.Equal(101, loadSeries.Data.Count);
            
            for (int p = 0; p < 101; p++)
            {
                Assert.Equal(origSeries.Data[p].Percent, loadSeries.Data[p].Percent);
                Assert.Equal(origSeries.Data[p].Rpm, loadSeries.Data[p].Rpm);
                Assert.Equal(origSeries.Data[p].Torque, loadSeries.Data[p].Torque);
            }
        }
    }

    private MotorDefinition CreateTestMotorDefinition()
    {
        var motor = new MotorDefinition
        {
            MotorName = "Test Motor",
            Manufacturer = "Test Mfg",
            PartNumber = "TM-1234",
            MaxRpm = 5000,
            RatedPeakTorque = 55.0,
            HasBrake = true
        };
        
        var series = new CurveSeries { Name = "Peak" };
        series.InitializeData(5000, 55.0);
        motor.Series.Add(series);
        
        return motor;
    }

    public void Dispose()
    {
        Directory.Delete(_tempDir, recursive: true);
    }
}
```

### 4. UI/Acceptance Tests

UI tests verify user workflows using Playwright or similar.

| Workflow | Test Cases | Priority |
|----------|------------|----------|
| **New Motor Workflow** | Create new motor with wizard | High |
| | Generated curves have 101 points | High |
| | Default Peak/Continuous series created | High |
| **Load/Save Workflow** | Open directory, select file, loads | High |
| | Edit value, asterisk appears in title | High |
| | Save removes asterisk | High |
| | Close with dirty file shows prompt | High |
| **Curve Editing** | Drag point, value updates | High |
| | Q slider affects adjacent points | Medium |
| | Grid edit updates chart | High |
| **Series Management** | Add new series | Medium |
| | Delete series with confirmation | Medium |
| | Toggle visibility | Medium |
| | Edit series color | Low |

```csharp
// Example: Playwright UI Test (conceptual - requires Playwright for desktop setup)
public class NewMotorWorkflowTests : PageTest
{
    [Fact]
    public async Task CreateNewMotor_GeneratesCurves()
    {
        // Open new motor dialog
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "File" }).ClickAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "New" }).ClickAsync();

        // Fill wizard
        await Page.GetByLabel("Motor Name").FillAsync("Test Motor");
        await Page.GetByLabel("Max RPM").FillAsync("5000");
        await Page.GetByLabel("Max Torque").FillAsync("50");
        await Page.GetByLabel("Max Power").FillAsync("1500");
        
        // Create
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();

        // Verify series created
        await Expect(Page.GetByText("Peak")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Continuous")).ToBeVisibleAsync();
        
        // Verify chart is displayed
        await Expect(Page.Locator(".chart-container")).ToBeVisibleAsync();
    }
}
```

---

## Potential Bug Areas and Testing Focus

### High-Risk Areas Requiring Extra Testing

#### 1. Data Precision Issues

**Risk**: Floating-point precision loss during calculations or serialization.

```csharp
[Fact]
public void TorqueValue_PrecisionPreserved()
{
    var original = new DataPoint { Percent = 50, Rpm = 2500, Torque = 45.123456789 };
    var json = JsonSerializer.Serialize(original);
    var restored = JsonSerializer.Deserialize<DataPoint>(json);
    
    // Should preserve reasonable precision
    Assert.Equal(original.Torque, restored.Torque, precision: 6);
}

[Fact]
public void PowerCalculation_AvoidsPrecisionLoss()
{
    // Test with values that could cause precision issues
    var service = new CurveGeneratorService();
    
    // Small torque, high RPM
    var power1 = service.CalculatePower(0.001, 10000);
    Assert.True(power1 > 0);
    
    // Large torque, low RPM
    var power2 = service.CalculatePower(1000000, 0.001);
    Assert.True(power2 > 0);
}
```

#### 2. Boundary Conditions

**Risk**: Off-by-one errors in percent values, RPM calculations.

```csharp
[Fact]
public void CurveSeries_ExactlyPercentValues()
{
    var series = new CurveSeries();
    series.InitializeData(5000, 50);
    
    // Verify first and last
    Assert.Equal(0, series.Data.First().Percent);
    Assert.Equal(100, series.Data.Last().Percent);
    
    // Verify no gaps
    for (int i = 0; i < series.Data.Count; i++)
    {
        Assert.Equal(i, series.Data[i].Percent);
    }
}

[Fact]
public void CurveSeries_RpmAt100Percent_EqualsMaxRpm()
{
    var series = new CurveSeries();
    series.InitializeData(maxRpm: 5000, defaultTorque: 50);
    
    Assert.Equal(5000, series.Data[100].Rpm);
}
```

#### 3. Q-Value Curve Editing

**Risk**: Q algorithm produces unexpected curve shapes or affects wrong points.

```csharp
[Theory]
[InlineData(0.0, 0)]   // Q=0: affects only the dragged point
[InlineData(0.5, 3)]   // Q=0.5: affects ~3 neighbors
[InlineData(1.0, 5)]   // Q=1.0: affects ~5 neighbors
public void QValueEditing_AffectsCorrectNeighborCount(double qValue, int expectedNeighbors)
{
    var vm = new ChartViewModel { QValue = qValue };
    var originalPoints = CreateTestCurve();
    vm.LoadCurve(originalPoints);
    
    // Simulate dragging point at index 50
    vm.ApplyDrag(pointIndex: 50, deltaY: 10);
    
    // Count how many points changed
    int changedCount = 0;
    for (int i = 0; i < originalPoints.Count; i++)
    {
        if (Math.Abs(vm.DataPoints[i].Torque - originalPoints[i].Torque) > 0.001)
            changedCount++;
    }
    
    // Should be 1 (the dragged point) + expectedNeighbors on each side
    int expectedChanged = 1 + (2 * expectedNeighbors);
    Assert.Equal(expectedChanged, changedCount);
}
```

#### 4. File Operations Race Conditions

**Risk**: Concurrent file access, incomplete writes.

```csharp
[Fact]
public async Task Save_WhileLoading_DoesNotCorruptFile()
{
    var fileService = new FileService();
    var filePath = Path.Combine(_tempDir, "concurrent-test.json");
    var motor = CreateTestMotorDefinition();
    
    // Initial save
    await fileService.SaveAsync(motor, filePath);
    
    // Attempt concurrent operations (simulate race condition)
    var loadTask = fileService.LoadAsync(filePath);
    var saveTask = fileService.SaveAsync(motor, filePath);
    
    await Task.WhenAll(loadTask, saveTask);
    
    // File should still be valid
    var reloaded = await fileService.LoadAsync(filePath);
    Assert.NotNull(reloaded);
    Assert.Equal(motor.MotorName, reloaded.MotorName);
}
```

#### 5. UI State Synchronization

**Risk**: Chart and data grid get out of sync.

```csharp
[Fact]
public void DataGridEdit_UpdatesChart()
{
    var vm = new MainWindowViewModel();
    vm.LoadTestData();
    
    // Edit value in view model (simulating grid edit)
    vm.CurrentSeries.DataPoints[50].Torque = 99.9;
    
    // Chart series should reflect the change
    var chartPoint = vm.ChartViewModel.Series[0].Values.ElementAt(50);
    Assert.Equal(99.9, chartPoint.Torque);
}
```

#### 6. Dirty State Management

**Risk**: Dirty flag not set/cleared correctly.

```csharp
[Fact]
public void EditAnyProperty_SetsDirty()
{
    var vm = CreateCleanViewModel();
    Assert.False(vm.IsDirty);
    
    // Each edit should set dirty
    vm.MotorDefinition.MotorName = "Changed";
    Assert.True(vm.IsDirty);
}

[Fact]
public void Save_ClearsDirty()
{
    var vm = CreateDirtyViewModel();
    Assert.True(vm.IsDirty);
    
    vm.SaveCommand.Execute(null);
    
    Assert.False(vm.IsDirty);
}

[Fact]
public void SaveCopyAs_DoesNotClearDirty()
{
    var vm = CreateDirtyViewModel();
    Assert.True(vm.IsDirty);
    
    vm.SaveCopyAsCommand.Execute("copy.json");
    
    // Dirty should remain because we didn't save to the active file
    Assert.True(vm.IsDirty);
}
```

---

## Regression Test Suite

### Critical Path Tests (Run on Every Commit)

1. JSON serialization/deserialization round-trip
2. Default curve generation produces 101 data points at 1% increments
3. Dirty state tracking
4. Save/Load file operations
5. Curve generation formulas

### Extended Tests (Run Before Release)

1. All unit tests
2. Integration tests
3. UI workflow tests
4. Performance tests
5. Stress tests (large files, many series)

---

## Performance Testing

### Benchmarks

```csharp
[MemoryDiagnoser]
public class CurveGeneratorBenchmarks
{
    private readonly CurveGeneratorService _service = new();

    [Benchmark]
    public List<DataPoint> GenerateCurve_StandardMotor()
    {
        return _service.InterpolateCurve(5000, 50, 1500);
    }
}

[MemoryDiagnoser]
public class FileServiceBenchmarks
{
    private readonly FileService _service = new();
    private readonly string _testFile;

    public FileServiceBenchmarks()
    {
        // Setup test file
    }

    [Benchmark]
    public async Task<MotorDefinition> LoadLargeFile()
    {
        return await _service.LoadAsync(_testFile);
    }
}
```

### Performance Criteria

| Operation | Target | Maximum |
|-----------|--------|---------|
| Load JSON file (<100KB) | <50ms | 200ms |
| Save JSON file | <50ms | 200ms |
| Chart render (2 series) | <100ms | 500ms |
| Point drag response | <16ms | 50ms |
| Axis scale update | <16ms | 50ms |

---

## Test Infrastructure

### Project Structure

```
tests/
├── CurveEditor.Tests.Unit/
│   ├── Models/
│   │   ├── DataPointTests.cs
│   │   ├── CurveSeriesTests.cs
│   │   └── MotorDefinitionTests.cs
│   ├── Services/
│   │   ├── FileServiceTests.cs
│   │   ├── CurveGeneratorServiceTests.cs
│   │   └── UserPreferencesServiceTests.cs
│   └── ViewModels/
│       ├── MainWindowViewModelTests.cs
│       ├── ChartViewModelTests.cs
│       └── DirectoryBrowserViewModelTests.cs
├── CurveEditor.Tests.Integration/
│   ├── FileRoundTripTests.cs
│   ├── ChartDataBindingTests.cs
│   └── SaveWorkflowTests.cs
├── CurveEditor.Tests.UI/
│   ├── NewMotorWorkflowTests.cs
│   ├── LoadSaveWorkflowTests.cs
│   └── CurveEditingTests.cs
└── CurveEditor.Tests.Performance/
    ├── CurveGeneratorBenchmarks.cs
    └── FileServiceBenchmarks.cs
```

### Test Project Configuration

```xml
<!-- CurveEditor.Tests.Unit.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\CurveEditor\CurveEditor.csproj" />
  </ItemGroup>
</Project>
```

---

## Continuous Integration

### GitHub Actions Workflow

```yaml
name: Test Suite

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Run Unit Tests
        run: dotnet test tests/CurveEditor.Tests.Unit --no-build --verbosity normal
      
      - name: Run Integration Tests
        run: dotnet test tests/CurveEditor.Tests.Integration --no-build --verbosity normal
      
      - name: Generate Coverage Report
        run: |
          dotnet test --collect:"XPlat Code Coverage"
          dotnet tool install -g dotnet-reportgenerator-globaltool
          reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage
      
      - name: Upload Coverage
        uses: actions/upload-artifact@v4
        with:
          name: coverage-report
          path: coverage/
```

---

## Test Coverage Goals

| Layer | Target Coverage | Minimum |
|-------|----------------|---------|
| Models | 95% | 90% |
| Services | 90% | 85% |
| ViewModels | 80% | 70% |
| Views (UI) | 50% | 30% |
| Overall | 85% | 75% |

---

## Summary

This testing strategy provides comprehensive coverage for:

1. **Unit Tests** - Verify individual components work correctly
2. **Integration Tests** - Verify components work together
3. **UI Tests** - Verify user workflows
4. **Performance Tests** - Verify application responsiveness
5. **Regression Tests** - Prevent bugs from reappearing

### Key Testing Priorities

1. **File Operations** - Data integrity is critical
2. **Curve Generation** - Mathematical correctness
3. **Dirty State** - Prevent data loss
4. **Chart-Data Sync** - User experience
5. **1% Increment Data** - Core requirement

### Test-First Development

For each feature in the MVP roadmap, write tests before or alongside implementation:

1. Review requirements from planning documents
2. Write test cases based on expected behavior
3. Implement feature to make tests pass
4. Refactor while keeping tests green
5. Add regression tests for any bugs found
