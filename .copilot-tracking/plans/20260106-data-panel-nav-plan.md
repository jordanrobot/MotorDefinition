<!-- markdownlint-disable-file -->
# Data Panel Navigation UX Plan

## Scope

- Fix Data Panel selection so arrow-key navigation stays consistent across %, RPM, and torque columns without losing the highlighted cell.
- Prevent visual artifacts such as the white border sticking to the first row.
- Ensure curve data reliably appears in the Data Panel after loading a file.

## Tasks

- [ ] T1: Analyze logs/repro to identify why selection is lost when moving between torque and non-series columns, causing jumps to the top-left cell.
- [ ] T2: Implement selection/visual sync fix so left/right navigation across torque/RPM/% keeps the current cell highlighted and in-place.
- [ ] T3: Add or update focused tests covering navigation into non-series columns and selection persistence.
- [ ] T4: Update tracking changes file after each task and capture any supporting artifacts (logs/tests/screenshots).

## References

- Details: `.copilot-tracking/details/20260106-data-panel-nav-details.md`
- Code areas: `src/MotorEditor.Avalonia/ViewModels/CurveDataTableViewModel.cs`, `src/MotorEditor.Avalonia/Views/CurveDataPanel.axaml*`
- Logs: https://gist.githubusercontent.com/jordanrobot/58b06838eeb4570201c888101242f0ca/raw/5453c3b6d141830cef92b8307794debebef5ee37/gistfile1.txt
