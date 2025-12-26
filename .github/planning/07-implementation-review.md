# Motor Torque Curve Editor - Implementation Review

This document provides a critical review of the implementation plans, identifying potential gaps, risks, and recommendations for improvement.

---

## Executive Summary

The planning documents are comprehensive and well-structured. However, several areas require additional attention before implementation begins:

1. **Missing Edge Case Specifications** - Error handling and boundary conditions need clarification
2. **Data Validation Rules** - Explicit validation constraints not fully defined
3. **Performance Considerations** - Chart performance with large datasets needs planning
4. **Accessibility** - No accessibility requirements specified
5. **Error Recovery** - Backup and recovery mechanisms not addressed

---

## Gap Analysis

### 1. Data Model Gaps

#### 1.1 DataPoint Validation

**Current State**: DataPoint model defined but validation rules not explicit.

**Gap**: No specification for:
- Minimum/maximum percent values (is negative allowed? What about decimals?)
- RPM validation (negative values? max limit?)
- Torque validation (negative torque? units validation?)

**Recommendation**: Add explicit validation rules:

```csharp
public class DataPoint
{
    private int _percent;
    public int Percent
    {
        get => _percent;
        set
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException(nameof(value), "Percent must be 0-100");
            _percent = value;
        }
    }
    
    private double _rpm;
    public double Rpm
    {
        get => _rpm;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "RPM cannot be negative");
            _rpm = value;
        }
    }
    
    // Torque can be negative for regenerative braking scenarios
    public double Torque { get; set; }
}
```

#### 1.2 Series Name Uniqueness

**Gap**: No specification about whether series names must be unique within a motor definition.

**Recommendation**: Enforce unique series names per motor:
- Validate on add/rename
- Provide clear error message
- Consider case-insensitive comparison

#### 1.3 Motor Definition Completeness

**Gap**: No specification for required vs optional fields in motor definition.

**Recommendation**: Define field requirements:

| Field | Required | Default |
|-------|----------|---------|
| motorName | Yes | - |
| manufacturer | No | "" |
| partNumber | No | "" |
| maxRpm | Yes | - |
| series | Yes | At least one |

---

### 2. File Operations Gaps

#### 2.1 File Locking

**Gap**: No mention of file locking behavior.

**Concern**: What happens if:
- User has file open in app and tries to open in another app?
- External process modifies file while open in editor?
- Two users on network share both open same file?

**Recommendation**:
- Use file stream with appropriate FileShare mode
- Detect external modifications and prompt user
- Consider read-only mode when file is locked

```csharp
public async Task<MotorDefinition?> LoadAsync(string filePath)
{
    using var stream = new FileStream(
        filePath, 
        FileMode.Open, 
        FileAccess.Read, 
        FileShare.Read); // Allow other readers
    
    // Track file modification time
    _lastModified = File.GetLastWriteTimeUtc(filePath);
}

public bool HasExternalModification(string filePath)
{
    return File.GetLastWriteTimeUtc(filePath) > _lastModified;
}
```

#### 2.2 Backup and Recovery

**Gap**: No backup mechanism specified.

**Concern**: User loses work due to crash, power failure, or accidental overwrite.

**Recommendation**:
- Auto-save to temp file every N minutes
- Create .bak file before overwriting
- Offer recovery of auto-saved files on next launch

```csharp
public class AutoSaveService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(2);
    
    public string GetAutoSavePath(string originalPath)
    {
        return Path.ChangeExtension(originalPath, ".autosave.json");
    }
    
    public async Task AutoSaveAsync(MotorDefinition data, string originalPath)
    {
        var autoSavePath = GetAutoSavePath(originalPath);
        await File.WriteAllTextAsync(autoSavePath, JsonSerializer.Serialize(data));
    }
    
    public IEnumerable<string> FindRecoverableFiles(string directory)
    {
        return Directory.GetFiles(directory, "*.autosave.json");
    }
}
```

#### 2.3 JSON Schema Versioning

**Gap**: No versioning strategy for JSON schema.

**Concern**: Future schema changes could break compatibility with older files.

**Recommendation**: Add schema version to JSON:

```json
{
  "schemaVersion": "1.0",
  "motorName": "...",
  ...
}
```

Migration strategy:
```csharp
public async Task<MotorDefinition> LoadWithMigrationAsync(string filePath)
{
    var json = await File.ReadAllTextAsync(filePath);
    var document = JsonDocument.Parse(json);
    
    var version = document.RootElement.TryGetProperty("schemaVersion", out var v) 
        ? v.GetString() 
        : "0.0"; // Pre-versioning files
    
    return version switch
    {
        "1.0" => JsonSerializer.Deserialize<MotorDefinition>(json),
        "0.0" => MigrateFromLegacy(document),
        _ => throw new NotSupportedException($"Unknown schema version: {version}")
    };
}
```

---

### 3. Chart/Visualization Gaps

#### 3.1 Large Dataset Performance

**Gap**: No performance strategy for large files or many series.

**Concern**: With 101 points per series × multiple series, chart rendering could become slow.

**Recommendation**:
- Implement data decimation for zoomed-out views
- Use canvas-based rendering (SkiaSharp)
- Profile with 10+ series

```csharp
public IEnumerable<DataPoint> GetDecimatedPoints(IList<DataPoint> points, int targetCount)
{
    if (points.Count <= targetCount)
        return points;
    
    var step = (double)points.Count / targetCount;
    return Enumerable.Range(0, targetCount)
        .Select(i => points[(int)(i * step)]);
}
```

#### 3.2 Chart Interaction Edge Cases

**Gap**: EQ-style editing behavior not fully specified.

**Questions needing answers**:
1. What happens when dragging at min/max boundaries?
2. Can user drag point below 0 torque?
3. What if Q influence would push neighbor beyond valid range?
4. Undo behavior for drag operations?

**Recommendation**: Add constraints to drag behavior:

```csharp
public void ApplyDrag(int pointIndex, double deltaY)
{
    var point = DataPoints[pointIndex];
    var newTorque = point.Torque + deltaY;
    
    // Apply constraints
    if (MinTorqueConstraint != null && newTorque < MinTorqueConstraint)
        newTorque = MinTorqueConstraint.Value;
    if (MaxTorqueConstraint != null && newTorque > MaxTorqueConstraint)
        newTorque = MaxTorqueConstraint.Value;
    
    point.Torque = newTorque;
    ApplyQInfluence(pointIndex, deltaY);
}
```

#### 3.3 Background Image Handling

**Gap**: Memory management for large background images not addressed.

**Concern**: Loading large images could cause memory issues.

**Recommendation**:
- Limit image dimensions (warn if >4K)
- Resize images on load to fit display
- Dispose image resources properly

```csharp
public async Task<SKBitmap?> LoadBackgroundImageAsync(string path, Size maxSize)
{
    using var original = SKBitmap.Decode(path);
    
    if (original.Width > maxSize.Width || original.Height > maxSize.Height)
    {
        var scale = Math.Min(
            maxSize.Width / (double)original.Width,
            maxSize.Height / (double)original.Height);
        
        var newWidth = (int)(original.Width * scale);
        var newHeight = (int)(original.Height * scale);
        
        return original.Resize(new SKSizeI(newWidth, newHeight), SKFilterQuality.High);
    }
    
    return original.Copy();
}
```

---

### 4. User Experience Gaps

#### 4.1 Accessibility

**Gap**: No accessibility requirements specified.

**Concern**: Application may not be usable with screen readers or keyboard-only.

**Recommendation**: Add accessibility requirements:
- Full keyboard navigation
- Screen reader support (ARIA labels)
- High contrast mode support
- Minimum touch target sizes

```xml
<!-- Avalonia XAML with accessibility -->
<Button Content="Save" 
        AutomationProperties.Name="Save current file"
        AutomationProperties.HelpText="Saves all changes to the current motor definition file"
        AccessKey="S"/>
```

#### 4.2 Localization

**Gap**: No localization strategy specified.

**Concern**: Application limited to English users.

**Recommendation**: 
- Use resource files for all user-facing strings
- Design UI to accommodate text expansion
- Support RTL languages (future consideration)

```csharp
// Resources/Strings.resx
public static class Strings
{
    public static string FileMenu => Resources.FileMenu;
    public static string SaveCommand => Resources.SaveCommand;
    public static string DirtyPromptTitle => Resources.DirtyPromptTitle;
}
```

#### 4.3 Error Messages

**Gap**: Error message strategy not defined.

**Concern**: Users may not understand what went wrong or how to fix it.

**Recommendation**: Standardize error handling:
- User-friendly message
- Technical details available (expandable)
- Suggested resolution

```csharp
public class UserFriendlyException : Exception
{
    public string UserMessage { get; }
    public string Resolution { get; }
    
    public UserFriendlyException(string userMessage, string resolution, Exception? inner = null)
        : base(userMessage, inner)
    {
        UserMessage = userMessage;
        Resolution = resolution;
    }
}

// Usage
throw new UserFriendlyException(
    "Unable to save file",
    "Check that you have write permission to this location and that the disk is not full.",
    innerException);
```

---

### 5. Technical Gaps

#### 5.1 Undo/Redo Architecture

**Gap**: Marked as "Optional MVP" without clear specification.

**Concern**: Users expect undo functionality; adding it later is architecturally difficult.

**Recommendation**: Design for undo from the start:

```csharp
public interface IUndoableCommand
{
    void Execute();
    void Undo();
    string Description { get; }
}

public class EditPointCommand : IUndoableCommand
{
    private readonly DataPoint _point;
    private readonly double _oldTorque;
    private readonly double _newTorque;
    
    public EditPointCommand(DataPoint point, double newTorque)
    {
        _point = point;
        _oldTorque = point.Torque;
        _newTorque = newTorque;
    }
    
    public string Description => $"Edit point at {_point.Percent}%";
    
    public void Execute() => _point.Torque = _newTorque;
    public void Undo() => _point.Torque = _oldTorque;
}

public class UndoStack
{
    private readonly Stack<IUndoableCommand> _undoStack = new();
    private readonly Stack<IUndoableCommand> _redoStack = new();
    
    public void Execute(IUndoableCommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
    }
    
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    
    public void Undo()
    {
        if (!CanUndo) return;
        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
    }
    
    public void Redo()
    {
        if (!CanRedo) return;
        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
    }
}
```

#### 5.2 Exception Handling Strategy

**Gap**: No global exception handling strategy.

**Recommendation**:
```csharp
// In App.xaml.cs
public override void OnFrameworkInitializationCompleted()
{
    AppDomain.CurrentDomain.UnhandledException += (s, e) =>
    {
        Log.Fatal((Exception)e.ExceptionObject, "Unhandled exception");
        ShowCrashDialog((Exception)e.ExceptionObject);
    };
    
    TaskScheduler.UnobservedTaskException += (s, e) =>
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved();
    };
}
```

#### 5.3 Logging Strategy

**Gap**: No logging requirements specified.

**Recommendation**: Add structured logging:
```csharp
public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    
    public async Task<MotorDefinition?> LoadAsync(string filePath)
    {
        _logger.LogInformation("Loading file: {FilePath}", filePath);
        
        try
        {
            var data = // load...
            _logger.LogDebug("Loaded motor: {MotorName} with {SeriesCount} series", 
                data.MotorName, data.Series.Count);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load file: {FilePath}", filePath);
            throw;
        }
    }
}
```

---

### 6. Requirements Clarifications Needed

#### 6.1 Curve Generation Algorithm

**Question**: The corner speed calculation assumes constant torque then constant power. Is this the correct model for all motors?

```
Current implementation assumes:
- Below corner speed: Torque = maxTorque (constant)
- Above corner speed: Power = maxPower (constant), Torque = Power/ω

Alternative models exist:
- Field weakening curve
- Thermal derating curve
- Voltage-limited curve
```

**Recommendation**: Document the expected curve shape or provide curve type options.

#### 6.2 Data Point Interpolation

**Question**: When user specifies fewer than 101 points, should the system:
- a) Interpolate to fill missing points?
- b) Store only provided points?
- c) Reject the input?

**Recommendation**: Store the points as provided (0–101), and only interpolate to 101 points when needed for standard curve generation or specific UI workflows:
```csharp
public List<DataPoint> InterpolateToStandard(List<DataPoint> sparsePoints)
{
    var result = new List<DataPoint>();
    
    for (int percent = 0; percent <= 100; percent++)
    {
        var torque = InterpolateTorqueAtPercent(sparsePoints, percent);
        result.Add(new DataPoint
        {
            Percent = percent,
            Rpm = _maxRpm * percent / 100.0,
            Torque = torque
        });
    }
    
    return result;
}
```

#### 6.3 Series Deletion Behavior

**Question**: Can the last series be deleted? Should there always be at least one series?

**Recommendation**: Require at least one series; prevent deletion of last series.

---

## Risk Assessment

### High Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| LiveCharts2 limitations | Could block features | Early prototyping |
| File corruption | Data loss | Backup mechanism |
| Performance issues | Poor UX | Early performance testing |

### Medium Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| Schema changes | Compatibility issues | Versioning from start |
| Complex undo/redo | Feature creep | Design early, implement later |
| Image memory | App crashes | Size limits |

### Low Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| Localization | Limited audience | Resource files ready |
| Accessibility | Legal/usability | Follow standards |

---

## Recommendations Summary

### Before Phase 1 Begins

1. ✅ Define explicit validation rules for all model properties
2. ✅ Add schema versioning to JSON format
3. ✅ Design undo/redo architecture (implement later)
4. ✅ Set up logging framework
5. ✅ Create test project structure

### During Phase 1

1. Implement data validation in models
2. Add file backup mechanism
3. Set up CI/CD pipeline
4. Write unit tests alongside implementation

### Future Considerations

1. Accessibility audit before release
2. Localization strategy for international users
3. Performance optimization pass
4. User feedback mechanism

---

## Conclusion

The planning documents provide an excellent foundation for the Motor Torque Curve Editor. Addressing the gaps identified in this review will result in a more robust, maintainable, and user-friendly application.

Key priorities:
1. **Data integrity** - Validation, backup, versioning
2. **User experience** - Error handling, undo/redo
3. **Maintainability** - Testing, logging, documentation

The recommended approach (Avalonia + LiveCharts2) remains valid. The identified gaps are implementation details that can be addressed as the project progresses.
