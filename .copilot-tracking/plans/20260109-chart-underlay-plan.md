<!-- markdownlint-disable-file -->
# Chart Format Panel & Image Underlay Plan

## Scope

- Add a Chart Format panel in the left zone with a collapsible PanelBar button.
- Surface existing chart visibility toggles (power curves, motor rated speed line, voltage max speed line) inside the new panel.
- Implement image underlay loading, display, and control (visibility, lock to zero, X/Y scaling, pan).
- Persist underlay metadata per motor/drive/voltage in a `.motorEditor` folder adjacent to the motor file.

## Tasks

- [x] T1: Panel infrastructure
  - Register a new left-zone panel descriptor and PanelBar button labeled "Chart Format".
  - Add the panel layout shell to MainWindow left zone, matching existing collapsible behavior.
- [x] T2: Chart format controls
  - Move/expose existing chart visibility toggles inside the new panel (power curves, motor rated speed line, voltage max speed line) and wire them to current settings.
  - Ensure toggles sync with menu commands and persisted preferences.
- [x] T3: Image underlay UI + interaction
  - Add underlay controls (file name display, Load Image button with dialog, visibility checkbox, lock-zero toggle, X/Y scale sliders, pan via drag).
  - Render the selected image behind the chart with proper z-order and per-voltage isolation.
- [x] T4: Persistence & tests
  - Persist underlay metadata (path, visibility, lock-zero, X-scale, Y-scale, offsets) per motor/drive/voltage in `.motorEditor` JSON files; handle missing files with user error notification and allow clearing metadata.
  - Add targeted tests covering metadata load/save and viewmodel state updates for underlay settings.
  - Update changes tracking after each task and mark tasks complete.
- [x] T5: Underlay coordinate synchronization
  - Keep the image anchor/scale origin aligned to the chart origin during scale/offset changes (including negative/out-of-bounds anchors).
  - Recalculate anchor/scale origin after underlay manipulations and chart size/layout changes so the image stays visually locked to chart (0,0).
  - Add focused tests covering scale-around-origin scenarios.
- [x] T6: Underlay opacity control
  - Add transparency/opacity slider in the Format panel, apply changes live to the image, and persist/load the value with underlay metadata.

## References

- Details: `.copilot-tracking/details/20260109-chart-underlay-details.md`
- Code areas: `src/MotorEditor.Avalonia/ViewModels/MainWindowViewModel.cs`, `src/MotorEditor.Avalonia/ViewModels/ChartViewModel.cs`, `src/MotorEditor.Avalonia/Views/MainWindow.axaml`, `src/MotorEditor.Avalonia/Views/ChartView.axaml*`, `src/MotorEditor.Avalonia/Models/PanelRegistry.cs`, `src/MotorEditor.Avalonia/Services/*`
