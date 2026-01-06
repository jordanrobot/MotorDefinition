<!-- markdownlint-disable-file -->
# Data Panel Navigation UX Details

## T1: Diagnose selection loss and jump behavior
- Reproduce/inspect logs for arrow navigation where selection disappears after moving from a torque column into RPM/%.
- Identify how selection state is cleared (e.g., coordinator-driven resets) and why the highlight reverts to (0,0).

## T2: Stabilize navigation and visuals across all columns
- Ensure arrow keys move the active cell one step in the requested direction across torque, RPM, and % columns without clearing selection.
- Keep the white border/highlight on the actual selected cell; avoid phantom borders in the first row.
- Avoid breaking chart-selection sync; only change selection logic where necessary.

## T3: Tests
- Add/adjust unit tests that cover navigating into non-series columns while an EditingCoordinator is present, ensuring SelectedCells remains accurate.
- Keep tests focused on the new navigation/selection behavior to minimize surface area.
