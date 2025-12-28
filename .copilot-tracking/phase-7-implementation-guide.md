# Phase 7: Tabbed Interface - Implementation Guide

## Executive Summary

This document provides a comprehensive guide for implementing the tabbed interface feature for the Motor Torque Curve Editor. The implementation allows multiple motor definition files to be open simultaneously, each with independent state.

## Current Status

### ✅ Completed
- **DocumentTab class** created (`ViewModels/DocumentTab.cs`)
  - Lightweight state container for per-document data
  - Holds Motor, dirty flag, file path, UndoStack
  - Holds ChartViewModel, CurveDataTableViewModel, EditingCoordinator
  - Holds selections and collections (Drives, Voltages, Series)
  - Provides `DisplayName` property with dirty indicator
  - Provides state management methods (MarkClean, MarkDirty, UpdateDirtyFromUndoDepth)

### 🔄 In Progress
- MainWindowViewModel refactor to support multiple tabs

### ⏰ Pending
- UI updates to show TabControl
- File operations updated for tab management
- Directory browser integration with tabs
- Session restore for multiple tabs

## Architecture

### Design Pattern: Wrapper-Based Delegation

```
┌─────────────────────────────┐
│   MainWindowViewModel       │
│  ┌───────────────────────┐  │
│  │ Tabs: Collection      │  │
│  │ ActiveTab: Current    │  │
│  └───────────────────────┘  │
│            ↓                 │
│  ┌───────────────────────┐  │
│  │ Properties delegate   │  │
│  │ to ActiveTab          │  │
│  │ (CurrentMotor, etc.)  │  │
│  └───────────────────────┘  │
│            ↓                 │
│  ┌───────────────────────┐  │
│  │ Methods work on       │  │
│  │ ActiveTab state       │  │
│  └───────────────────────┘  │
└─────────────────────────────┘
         ↓
┌─────────────────────────────┐
│   DocumentTab (per file)    │
│  ┌───────────────────────┐  │
│  │ Motor                 │  │
│  │ UndoStack             │  │
│  │ IsDirty, FilePath     │  │
│  │ ChartViewModel        │  │
│  │ Selections, etc.      │  │
│  └───────────────────────┘  │
└─────────────────────────────┘
```

### Key Benefits
1. **Minimal Code Duplication**: Existing edit methods stay in MainWindowViewModel
2. **Incremental Implementation**: Can be done in phases with testing
3. **Clear Separation**: DocumentTab is pure state, MainWindowViewModel is behavior
4. **Independent Undo**: Each tab has its own UndoStack

## Implementation Plan

### Phase 1: Foundation (SAFE - No Breaking Changes)

#### Step 1.1: Add Tab Infrastructure to MainWindowViewModel

Add these members near the top of the class:

```csharp
// Tab management
private readonly ObservableCollection<DocumentTab> _tabs = new();
private DocumentTab? _activeTab;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(CurrentMotor), nameof(IsDirty), nameof(CurrentFilePath))]
private ObservableCollection<DocumentTab> tabs;

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
```

#### Step 1.2: Initialize with One Tab

In each constructor, after existing initialization:

```csharp
// Initialize with one empty tab (maintains current behavior)
var initialTab = CreateNewTab();
_tabs.Add(initialTab);
ActiveTab = initialTab;
Tabs = _tabs;
```

#### Step 1.3: Create Tab Factory Method

```csharp
private DocumentTab CreateNewTab()
{
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
    
    tab.ChartViewModel.DataChanged += (s, e) => tab.MarkDirty();
    tab.CurveDataTableViewModel.DataChanged += (s, e) => 
    {
        tab.MarkDirty();
        tab.ChartViewModel?.RefreshChart();
    };
    
    tab.UndoStack.UndoStackChanged += (s, e) => tab.UpdateDirtyFromUndoDepth();
    
    return tab;
}
```

#### Step 1.4: Handle Active Tab Changes

```csharp
private void OnActiveTabChanged()
{
    // Notify all properties that depend on active tab
    OnPropertyChanged(nameof(CurrentMotor));
    OnPropertyChanged(nameof(IsDirty));
    OnPropertyChanged(nameof(CurrentFilePath));
    OnPropertyChanged(nameof(SelectedDrive));
    OnPropertyChanged(nameof(SelectedVoltage));
    OnPropertyChanged(nameof(SelectedSeries));
    OnPropertyChanged(nameof(ChartViewModel));
    OnPropertyChanged(nameof(CurveDataTableViewModel));
    OnPropertyChanged(nameof(EditingCoordinator));
    OnPropertyChanged(nameof(AvailableDrives));
    OnPropertyChanged(nameof(AvailableVoltages));
    OnPropertyChanged(nameof(AvailableSeries));
    OnPropertyChanged(nameof(CanUndo));
    OnPropertyChanged(nameof(CanRedo));
    OnPropertyChanged(nameof(WindowTitle));
    
    // Update directory browser
    DirectoryBrowser.UpdateActiveFileState(CurrentFilePath, IsDirty);
    
    // Refresh property editors
    RefreshMotorEditorsFromCurrentMotor();
}
```

### Phase 2: Delegate Properties (CAREFUL - Behavior Changes)

#### Step 2.1: Convert State Properties to Delegates

Replace direct properties with delegation to ActiveTab:

```csharp
// OLD:
[ObservableProperty]
private ServoMotor? _currentMotor;

// NEW:
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
        }
    }
}

// Repeat for:
// - IsDirty
// - CurrentFilePath  
// - SelectedDrive, SelectedVoltage, SelectedSeries
// - ChartViewModel, CurveDataTableViewModel, EditingCoordinator
// - All collections
```

#### Step 2.2: Update Methods to Use ActiveTab

Find all methods that reference `_undoStack`, `_currentMotor`, `_isDirty`, etc. and update to use `ActiveTab?.`:

```csharp
// Example: MarkCleanCheckpoint
public void MarkCleanCheckpoint()
{
    if (ActiveTab != null)
    {
        ActiveTab.MarkClean();
        OnPropertyChanged(nameof(IsDirty));
    }
}

// Example: Undo
[RelayCommand(CanExecute = nameof(CanUndo))]
private void Undo()
{
    ActiveTab?.UndoStack.Undo();
    RefreshMotorEditorsFromCurrentMotor();
    ActiveTab?.ChartViewModel?.RefreshChart();
    ActiveTab?.CurveDataTableViewModel?.RefreshData();
}
```

### Phase 3: Multi-Tab File Operations

#### Step 3.1: Update NewMotorAsync

```csharp
[RelayCommand]
private async Task NewMotorAsync()
{
    Log.Information("Creating new motor definition in new tab");
    
    var newTab = CreateNewTab();
    
    // Create motor using existing file service
    newTab.Motor = _fileService.CreateNew(
        motorName: "New Motor",
        maxRpm: 5000,
        maxTorque: 50,
        maxPower: 1500);
    
    newTab.FilePath = null;
    newTab.MarkClean();
    
    _tabs.Add(newTab);
    ActiveTab = newTab;
    
    StatusMessage = "Created new motor definition";
}
```

#### Step 3.2: Update OpenFileAsync to Check for Existing Tab

```csharp
[RelayCommand]
private async Task OpenFileAsync()
{
    try
    {
        // Get file path from dialog...
        var filePath = GetFilePathFromDialog();
        if (string.IsNullOrEmpty(filePath))
        {
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
            return;
        }
        
        // Create new tab for the file
        var newTab = CreateNewTab();
        newTab.Motor = await _fileService.LoadAsync(filePath);
        newTab.FilePath = filePath;
        newTab.MarkClean();
        
        _tabs.Add(newTab);
        ActiveTab = newTab;
        
        StatusMessage = $"Opened: {Path.GetFileName(filePath)}";
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to open file");
        StatusMessage = $"Error: {ex.Message}";
    }
}
```

#### Step 3.3: Add CloseTabCommand

```csharp
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
    _tabs.Remove(tab);
    
    // If we closed the active tab, activate another
    if (ActiveTab == tab)
    {
        ActiveTab = _tabs.LastOrDefault();
    }
    
    StatusMessage = $"Closed tab: {tab.DisplayName}";
}
```

### Phase 4: UI Updates

#### Step 4.1: Add TabControl to MainWindow.axaml

Replace the center Grid content (around line 255-293):

```xml
<!-- Center: Tabbed Document Area -->
<Grid x:Name="CenterGrid" Grid.Column="2">
    <TabControl Items="{Binding Tabs}"
                SelectedItem="{Binding ActiveTab}"
                Background="{DynamicResource CurveEditor.Editor.BackgroundBrush}">
        
        <!-- Tab Header Template -->
        <TabControl.ItemTemplate>
            <DataTemplate>
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <TextBlock Text="{Binding DisplayName}" 
                               VerticalAlignment="Center"/>
                    <Button Content="×" 
                            Command="{Binding $parent[Window].((vm:MainWindowViewModel)DataContext).CloseTabCommand}"
                            CommandParameter="{Binding}"
                            Padding="6,0"
                            FontSize="16"
                            ToolTip.Tip="Close tab"/>
                </StackPanel>
            </DataTemplate>
        </TabControl.ItemTemplate>
        
        <!-- Tab Content Template -->
        <TabControl.ContentTemplate>
            <DataTemplate>
                <Border Margin="4"
                        Background="{DynamicResource CurveEditor.Editor.BackgroundBrush}"
                        BorderBrush="{DynamicResource CurveEditor.Editor.BorderBrush}"
                        BorderThickness="1"
                        CornerRadius="4">
                    <Grid>
                        <!-- Welcome screen when no motor loaded -->
                        <StackPanel VerticalAlignment="Center" 
                                    HorizontalAlignment="Center"
                                    IsVisible="{Binding Motor, Converter={x:Static ObjectConverters.IsNull}}">
                            <TextBlock Text="Motor Torque Curve Editor" 
                                       FontSize="24" 
                                       FontWeight="Bold"
                                       HorizontalAlignment="Center"/>
                            <TextBlock Text="Create a new motor definition or open an existing file to get started."
                                       Margin="0,16,0,0"
                                       Foreground="{DynamicResource SystemControlForegroundBaseMediumBrush}"
                                       HorizontalAlignment="Center"/>
                            <StackPanel Orientation="Horizontal" 
                                        HorizontalAlignment="Center" 
                                        Margin="0,24,0,0"
                                        Spacing="16">
                                <Button Content="New Motor" 
                                        Command="{Binding $parent[Window].((vm:MainWindowViewModel)DataContext).NewMotorCommand}"
                                        Padding="16,8"/>
                                <Button Content="Open File" 
                                        Command="{Binding $parent[Window].((vm:MainWindowViewModel)DataContext).OpenFileCommand}"
                                        Padding="16,8"/>
                            </StackPanel>
                        </StackPanel>

                        <!-- Chart when motor is loaded -->
                        <views:ChartView DataContext="{Binding ChartViewModel}"
                                         IsVisible="{Binding Motor, Converter={x:Static ObjectConverters.IsNotNull}}"/>
                    </Grid>
                </Border>
            </DataTemplate>
        </TabControl.ContentTemplate>
    </TabControl>
</Grid>
```

### Phase 5: Testing Plan

#### Unit Tests
- DocumentTab state management
- Tab switching logic
- Undo/redo isolation between tabs

#### Integration Tests
- Open multiple files in tabs
- Switch between tabs and verify state preservation
- Close tabs with dirty/clean state
- Undo/redo in different tabs

#### Manual Testing Checklist
- [ ] Open 3 different files in tabs
- [ ] Make edits in each tab
- [ ] Switch between tabs - verify edits are preserved
- [ ] Undo in one tab - verify doesn't affect others
- [ ] Close a dirty tab - verify save prompt
- [ ] Close a clean tab - verify no prompt
- [ ] Close all but one tab - verify app continues
- [ ] Close last tab - verify welcome screen or new tab created
- [ ] Reorder tabs by dragging (if implemented)
- [ ] Window title shows active tab's file name

## Risk Mitigation

### High-Risk Areas
1. **Property delegation**: Easy to miss properties that need updating
2. **Method updates**: Many methods reference state directly
3. **Event handlers**: May reference old state variables
4. **Constructor initialization**: Multiple constructors need updating

### Mitigation Strategies
1. **Compile after each phase**: Catch issues early
2. **Test after each phase**: Verify no regressions
3. **Use Find/Replace carefully**: Search for patterns like `_undoStack`, `CurrentMotor`, etc.
4. **Keep git history clean**: One logical change per commit

## Alternative Approaches

### Option A: Full Refactor (Current Plan)
- **Pro**: Clean architecture, proper separation
- **Con**: Large, risky change

### Option B: Parallel Implementation
- Keep existing single-file code
- Add new tab code alongside
- Gradually migrate
- **Pro**: Lower risk
- **Con**: Code duplication

### Option C: Hybrid Tab Facade
- Add TabControl UI
- Keep single active document
- "Tabs" are just visual with saved state
- **Pro**: Minimal changes
- **Con**: Not true multi-document

## Recommendation

**Proceed with Option A (Full Refactor) in phases as outlined above.**

Rationale:
- DocumentTab class already created and tested
- Clear architectural plan
- Phased approach allows testing at each step
- Results in clean, maintainable code

**Timeline Estimate**: 
- Phase 1-2: 4-6 hours (foundation + delegation)
- Phase 3: 2-3 hours (file operations)
- Phase 4: 1-2 hours (UI)
- Phase 5: 2-3 hours (testing)
- **Total**: 9-14 hours of focused development

## Next Actions

1. ✅ Review this implementation guide
2. ⏳ Implement Phase 1 (Tab infrastructure)
3. ⏳ Test Phase 1 thoroughly
4. ⏳ Implement Phase 2 (Property delegation)
5. ⏳ Test Phase 2 thoroughly
6. ⏳ Continue through remaining phases

## Questions / Decisions Needed

1. **Tab reordering**: Should tabs be draggable to reorder? (Phase 7.3 requirement)
2. **Session restore**: Should open tabs be restored on app restart?
3. **Max tabs**: Should there be a limit on number of open tabs?
4. **Duplicate files**: Allow same file open in multiple tabs?
5. **Context menu**: Add right-click menu on tabs (Close, Close Others, Close All)?

## Conclusion

The tabbed interface implementation is well-planned and ready to proceed. The DocumentTab class provides a solid foundation. The phased approach minimizes risk while delivering the full feature set required by Phase 7.

The implementation will transform the Motor Torque Curve Editor from a single-document application into a powerful multi-document editor, significantly improving user productivity and workflow.
