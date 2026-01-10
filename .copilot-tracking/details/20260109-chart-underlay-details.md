<!-- markdownlint-disable-file -->
# Chart Format Panel & Image Underlay Details

## T1: Panel infrastructure
- Add a PanelRegistry entry for "Chart Format" in the left zone with PanelBar label "Format".
- Add a collapsible panel shell in the left zone grid (mutually exclusive with Browser/Data) controlled by the new PanelBar button; hook IsVisible to ActiveLeftPanelId.
- Ensure default sizing and borders align with existing left panels.

## T2: Chart format controls
- Place checkboxes for:
  - Show Power Curves (bind to ChartViewModel.ShowPowerCurves / ToggleShowPowerCurvesCommand).
  - Show Motor Rated Speed Line (bind to ChartViewModel.ShowMotorRatedSpeedLine / ToggleShowMotorRatedSpeedLineCommand).
  - Show Voltage Max Speed Line (bind to ChartViewModel.ShowVoltageMaxSpeedLine / ToggleShowVoltageMaxSpeedLineCommand).
- Keep menu items and new panel controls in sync; updating either should reflect across tabs and persisted preferences.

## T3: Image underlay UI + interaction
- In the Chart Format panel, add an "Image Underlay" section with:
  - Read-only TextBox showing loaded image file name (blank if none).
  - "Load Image" button with tooltip, opens file picker restricted to PNG/JPG/BMP, stores per-voltage selection, renders image behind chart.
  - Visibility checkbox to toggle image display without unloading file.
  - "Lock Zero" checkbox to anchor image to chart origin (0,0); when true, disable dragging offsets.
  - X-axis and Y-axis scale sliders (0.1–5.0 with sensible defaults) scaling relative to zero point.
- Implement pan by click-dragging over the chart area when an underlay is present and not zero-locked; dragging updates stored offsets.
- Keep one image per voltage; switching voltage swaps the underlay state accordingly.

## T4: Persistence & tests
- Persist underlay metadata per motor/drive/voltage into `.motorEditor` (sibling to the motor file), e.g., `{motorName}-{drive}-{voltage}.json`, containing: image path, isVisible, lockZero, xScale, yScale, offsetX, offsetY.
- Load metadata when opening motor files; if image path is missing on disk, surface an error to the user and clear the underlay for that voltage.
- Removing/unloading an image deletes the metadata file for that voltage.
- Update metadata whenever visibility/lock/scale/offsets change.
- Add focused tests for metadata read/write and viewmodel state transitions (no need for UI automation).

## T5: Underlay coordinate synchronization
- Treat the chart origin (0,0) as the anchor/scale origin for the underlay and keep that point visually fixed when changing explicit X/Y scale or offsets (support negative/out-of-bounds anchors).
- Recalculate and store the transformed chart-origin anchor in the image coordinate system after drag/pan, scale adjustments, or applying metadata so future scaling uses the same origin.
- Refresh the anchor after chart size/layout changes (axes/series updates or draw-margin changes) so the underlay stays aligned with the chart origin.
- Add unit coverage verifying scale-around-origin behavior for positive and negative anchors.
