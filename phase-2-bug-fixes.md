Please address the following fix/implementation notes:

# Overall Layout

- [X] Properties panel/pane should be resizable by dragging the divider between the properties panel and the curve graph/data table section.

- [X] Browser panel/pane should be resizable by dragging the divider between the browser panel and the curve graph/data table section.

- [X] The curve graph and curve data section should be resizable by dragging the divider between the curve graph and the curve data table section.

- [X] Curve Data table should be collapsible to save space. Default to collapsed. Show an expand/collapse arrow next to the Curve Data Table header so the user knows it is collapsible.

# Properties Panel

- [X] Properties in property pallet are not editable, but should be. Please enable editing of the properties.

- [X] The property pallet should have a more compact layout to reduce wasted space.
  - [X] Change "Properties" column Header to "Motor Properties"
  - [X] Remove the Identification header
  - [X] Remove the Performance (Cut Sheet) header
  - [X] Remove the Physical Properties header
  - [X] Remove the Brake header

- [X] Units should each be represented by a dropdown combobox instead of free text input to avoid invalid entries.
  - [X] These dropdowns should be located to the right of each Unit type under the Unit header.
  - [X] Dropdown should be populated based on the json file data.
  - [ ] If this is a new file, the dropdown should be populated with the first unit listed per property type.
  - [X] Available units per property type:
    - [X] Speed: rpm
    - [X] Weight: kg, g, lbs, oz
    - [X] Torque: Nm, lbf-ft, lbf-in, oz-in
    - [X] Power: kW, W, hp
    - [ ] Voltage: V, kV
    - [ ] Current: A, mA
    - [ ] Inertia: kg-m^2, g-cm^2
    - [ ] torqueConstant: Nm/A
    - [ ] backlash: arcmin, arcsec
    - [ ] Temperature: C, F
- [X] Make the Units header collapsible to save space. Default to collapsed. Show an expand/collapse arrow next to the Units header so the user knows it is collapsible.

- [X] The drives and voltage listboxes are too large. Please change these to dropdown comboboxes instead.
- [X] Rename the Drive Configuration header Drive Properties.
- [X] Consolidate the drive and voltage properties under the "Drive Properties" header.
- [X] Remove the Voltage Configuration header.

- [X] Any boolean property should be represented by a checkbox instead of free text input to avoid invalid entries. Checked = true, unchecked = false.

# General

- [X] Ctrl + S should trigger the Save action.
- [X] Ctrl + O should trigger the Open action.
- [X] Ctrl + N should trigger the New File action.
- [X] Ctrl + Shift + S should trigger the Save As action.

# Properties Panel

- [X] Please add the following units dropdowns to the Units section of the properties panel:
    - [X] Voltage: V, kV
    - [X] Current: A, mA
    - [X] Inertia: kg-m^2, g-cm^2
    - [X] Torque Constant: Nm/A
    - [X] Backlash: arcmin, arcsec

# Properties

- [X] When users change the unit dropdown selection for a property, all relevant property values should show the new unit label next to the property. Later we will implement unit conversions. Please just update the unit labels for now. This was supposed to be done already, but it is not working properly.

- [X] Move Voltage dropdown directly underneath the Drive dropdown within the Drive Properties section.
- [X] For all "+" buttons to add new items (drives, voltages, curve series, etc), please implement a hover tooltip that says "Add new [item]".
- [X] For all trash can icon buttons to delete items (drives, voltages, curve series, etc), please implement a hover tooltip that says "Delete selected [item]".
- [X] For all trash can icon buttons to delete items (drives, voltages, curve series, etc), please implement a confirmation dialog before deleting the selected item. The dialog should say "Are you sure you want to delete the selected [item]?" with "Yes" and "No" buttons.

- [X] For all "+" buttons to add new items (drives, voltages, curve series, etc), please center the + on the button.
- [X] For all trash can icon buttons to delete items (drives, voltages, curve series, etc), please center the trash can icon on the button.

# Drive Properties

- [X] Drive selection is still buggy.

  - [X] Upon a File New action or a file open action, the drive is not selected on file open; the properties and combobox are empty. Please fix this so that the first drive in the file is selected when a file is opened.

  - [X] When changing selected drives via the Drive combobox, the selection doe not update properly. The current buggy behaviour is as follows: the user must select a drive two times, and then select a different drive. The first drive the use selected is then loaded. The correct behaviour should be: the user selects a drive in the combobox, and that drive is immediately loaded. Please fix this.

- [X] When a drive is added via the "Add new Drive" button, and then another drive is selected. The new drive is not able to be re-selected in the dropdown.

- [X] When a new file is created via the New File action, if the current file is in a dirty state, a popup should prompt the user to save changes before creating a new file, and provide buttons to "Save" or "Ignore". Please implement this.

- [X] When a drive or voltage is added, the graph's max speed does not resize to match the new drive/voltage's max speed. Please implement this behavior. Otherwise the graph does update.

- [X] When the drive name is renamed, the name updates in the dropdown, but the currently selected name in the dropdown still shows the old name until another drive is selected. Please fix this so that the selected drive name in the dropdown updates immediately when renamed.

- [X] The Drive Name property should be the same as the selected drive from the drive dropdown.

- [X] If the user renames the Drive Name property, it should update the selected drive's name in the drive dropdown as well.

- [X] Selecting a drive from the drive dropdown doesn't work properly. This should change the selected drive and update the related properties. Please fix this.

- [X] When a drive is selected, the voltage dropdown should select one of the voltages associated with that drive by default. Please implement this behavior. If a 208 V option is available, please use that as the default voltage selection for the drive. If not, use the first voltage option available for that drive.

- [X] Change the "Add new Drive" button to a button with a "+" icon. Place this next to the trash can icon next to the drive dropdown.

- [X] When a new drive or voltage is added, please show a popup window that displays the following fields to set the drive + voltage properties:
  - [X] Name
  - [X] Manufacturer
  - [X] Model
  - [X] Voltage
  - [X] Power
  - [X] Max Speed
  - [X] Peak Torque
  - [X] Continuous Torque
  - [X] Continuous Current
  - [X] Peak Current

- [X] When the new drive/voltage is added perform the following actions:
  - [X] Create two new series in the curve data table section:
    - [X] One for Peak Torque
    - [X] One for Continuous Torque
  - [X] enter the torque values for the drive + voltage into the curve data table. Use the drive + voltage's peak torque and continuous torque values to fill in the torque values for all speed points in the respective curve series.  

- [X] Change the "Add new Voltage" button to a button with a "+" icon. Place this next to the trash can icon next to the voltage dropdown.

- [X] When the user changes the drive voltage maximum speed property, prompt the user with a dialog box with the text "Prompt the user on how to handle this change and existing curve  data". We'll implement this dialog now, and later we can implement the actual logic to handle this case.


# UI Improvements

- [X] The graph's x-axis minimum should always be 0 rpm.
- [X] The graph's x-axis maximum should use the maximum of the two properties:
  -  Motor Properties Max Speed, or
  -  Drive Properties Max Speed
- [X] Graph x-axis: do not increment or round the x-axis maximum value in the graph. E.g. If the max speed is 6500, then the x-axis maximum should be exactly 6500 rpm, not rounded to 7000 rpm, 10000 rpm, etc. Please implement this.

- [X] Graph Zoom: when the user changes the Motor Properties Max Speed or Drive Properties Max Speed property, the graph's x-axis max should update immediately; currently it does not. Please implement this.

# Curve Series Management

- [X] The curve series selection listbox should be removed. These curve series should move into the header row of the data table section as headers for each curve series.
  
  - [X] Each curve series should be represented by a column in the data table.
  
  - [X] Each curve series column should show the curve series name, color, visibility checkbox, and lock checkbox, then followed by the torque values for that curve series at each speed point.

  - [X] The user should be able to add or remove curve series from within the data table section instead of the properties panel. Please add an "Add Curve Series" button to the data table section that opens a dialog to add a new curve series.

  - X When the user enters a new curve series, the series is added to the graph as a line, but the torque values are not displayed in the data table. Please fix this so that when a new curve series is added, the torque values are shown in the data table.

  - [X] Each curve series column should have a trash icon button to remove that curve series. Place this trash icon in the header of each series, next to the lock checkbox.

    - [X] pressing any of the curve series trash icon buttons only prompts to remove one of the curve series. Fix this so the selected curve series is removed when its corresponding trash icon button is pressed. The trash icon button for a series should be in that series' column.
   
    - [X] Pressing the trash icon button for a curve series does not remove that curve series from the graph or the data table. Please fix this so that when a curve series' trash icon is clicked, that series is removed from the data model, graph, and data table. When the user saves the file after removing a curve series, the removed series should not be present in the json file.

- [X] BUG: curve series deletion now works, however, if there is only one curve series left, that curve series cannot be deleted. Can you please fix this so that the last remaining curve series can also be deleted?

- [X] UI Fix: In the Curve Data table, please align the header text (curve series name) to be centered above the torque values for that series. Currently they are not in line. The header text box is wider, which shifts them farther right than the values they represent. Please fix this.

- [X] BUG: when the user adds a new curve series, the program will crash after the user clicks the Add button in the pop-up dialog. Please fix this so that the user can still add a new curve series. This is a repeat of a previous bug that was marked as fixed, but it is still occurring.

- [X] FEATURE: for the add new curve series button, please center the + icon on the button.

- [X] FEATURE: if the motor hasBrake = true, please add a horizontal line to the graph indicating the brake torque value. Please implement this feature. This line should update immediately if the brake torque property is changed. This line does not need any points on it, and doesn't need to be represented in the data table.


# Curve Series Editing

- [X] Curve series visibility checkbox
  - [X] The checkbox does not toggle checked state when clicked. Please fix this.
  - [X] When a curve series visibility checkbox is toggled, the corresponding curve on the graph should show or hide accordingly. Right now it does not change visibility on the graph. Please fix this.

- [X] BUG: the add curve series button is no longer visible. Please fix this so that the user can add new curve series.

- [X] BUG: When a curve series is locked = true = checked, please inhibit series deletion. Currently the user can still delete a locked curve series, which is incorrect. Please fix this so that when a curve series is locked, the trash icon button to delete that series is disabled and greyed out.

- [X] BUG: When a curve series is locked = false = unchecked, please enable deletion of that curve series.

- [X] When a curve series is locked = true = checked, it is still editable in the data table; this is wrong. Please fix this so that when locked = true, the user cannot edit that curve series.

- [X] When a curve series is locked = false = unchecked, ensure the the curve series is editable in the data table.

- [X] When a curve series is added, show a dialog to set the following properties:
  - [X] Name
  - [X] Color (use a color picker)
  - [X] default Locked to false
  - [X] default Visible to true
  - [X] HP or kW
    - [-] calculate torque values based on selected power unit and speed - TODO: refine implementation once we add unit conversion support with the Tare library, please add this todo item to the planning documents.
  - [X] or indicate a "baseline" torque value to use for all speed points

- [X] Add the ability to rename a curve series by double clicking on the curve series name in the data table header.

# Curve Data Table Layout

- [X] When the curve data table is resized, the curve data table rows should expand or contract vertically to fill the available space. E.g. if the table panel is made taller, more rows should be visible, and vice versa.

- [X] The Curve Data Table Collapse button no longer collapses the table pane, it only hides the table rows. Please fix this so the pane collapses properly when the user clicks the collapse button.

- [X] When the Curve Data Table Collapse button is clicked to show the table pane, the pane should only take up 1/2 of the window height, currently it's taking up most of the window height. Please fix this.

# Drive Editing

- [X] BUG: when a drive is added, the Drive combobox does not update to show the newly added drive. It is blank and there are no drive properties shown. Please fix this so that when a new drive is added, it appears in the Drive combobox as the currently selected drive.

# Curve Data Table Section

new terminology: 

- Cell Edit Mode: "Entering" a cell to edit the value. Shall be indicated visually with a thick white border around the cell, and a text edit cursor shall be placed inside the cell to indicate where the user can edit text. The user may enter cell edit mode by pressing F2 or (left) double-clicking the cell. To exit cell edit mode, the user may press Enter to save the value, Esc to cancel editing and restore the previous value, or clicking outside the cell to save the value.

- Selected Cells: cell(s) that are currently selected by the user. Shall be indicated visually with a white border around the cell. Multiple cells can be selected at once.
  - When the user types while cell(s) are selected, immediately insert the typed value into that cell, overwriting the existing value, without requiring the user to enter cell edit mode.

- Selection Set: the set of currently selected cells.

- [X] Curve data table editing improvements:
  - [X] Cell selection shall always be indicated visually so the user knows which cell(s) are selected. Show a cell is currently selected in the UI by adding a white border around the cell and a background highlight with a highlight color.
  - [X] clicking once on a cell should select the cell.
  - [X] clicking, holding, and dragging over the cells should allow for multi-cell selection via a rubber-band style selection.
  - [X] when ctrl-clicking a cell, it should toggle selection of that cell. If other cell(s) are already selected, they should remain selected, and the newly clicked cell should be added to the selection set.
  - [X] when shift-clicking a cell, it should select all cells from the last selected cell to the clicked cell.

- [-] TODO: Selecting a cell in the data table should highlight the corresponding point on the curve graph as well. Implement this later - make a note in Phase 3 planning docs.

- [X] REGRESSION: When a cell is selected, pressing the Enter key should move the selection to the cell directly below the currently selected cell.

- [X] When the user presses an arrow key while a cell is selected, the selection should move to the adjacent cell(s) as indicated by the arrow key.

- [X] When the user presses an arrow key while a cell is selected and the shift key is held, the selection should expand to include the adjacent cell(s) as indicated by the arrow key.


# Curve Data Table Editing

- [X] REGRESSION: When a cell is selected, pressing F2 should enter edit mode, selecting the existing value.

- [X] REGRESSION: When a user double-clicks on a cell, the cell should enter edit mode, selecting the existing value.

- [X] When the cell is single left-clicked, the cell should not enter edit mode. It should just select the cell.

- [X] When a cell is in edit mode, pressing the Enter key should save the entered value and exit edit mode, moving to the cell directly below the edited cell.

- [X] Override Mode: When the user enters text when cell(s) are selected, the text should be immediately inserted into the selected cell(s), overwriting the existing value(s), and without requiring the user to enter cell edit mode. In this way, a user can batch edit torque values by selecting multiple cells and typing a value once.

- [X] When a cell is selected, or edit mode is initiated, the existing value shall not be removed or deleted until the user types a new value.

- [X] When the user presses the delete or backspace key while a cell is selected, the value in the torque cell of the selected row(s) should be cleared.

- [X] Within the Curve Data Table, the user should not be able to edit the speed values (rpm or speed percentages). Speed values should be read-only.

# Curve Data Table Section

  - [X] When a cell or cells are selected, if the user presses Ctrl+Shift+arrow key (up, down, left, right), the selection should expand in the indicated direction to include all of the adjacent cell(s) in that direction. E.g. select all remaining cells between the current selection and the end of the row or column. Please implement this behavior.


# Clipboard Operations for Curve Data Table

- [X] When a cell is selected, pressing Ctrl+C should copy the torque value(s) of the selected cell(s) to the clipboard.

- [X] When multiple contiguous cells are selected (in a rectangular shape), pressing Ctrl+C should copy all torque values of the selected cells to the clipboard in a format that can be pasted into spreadsheet applications (e.g., tab-separated values for multiple columns, newline-separated values for multiple rows).

- [X] When a cell is selected, pressing Ctrl+V should paste the torque value(s) from the clipboard into the torque cell of the selected cell(s). If multiple cells are selected, the pasted value should be applied to all selected cells.

- [X] When a cell's value is pasted via Ctrl+V, the cell should exit edit mode (if it was in edit mode) and immediately update to display the newly pasted value in the graph and the data table.

- [X] When multiple contiguous cells are selected (in a rectangular shape), pressing Ctrl+V should paste values from the clipboard into the selected cells in a way that matches the clipboard format (e.g., tab-separated values for multiple columns, newline-separated values for multiple rows). If the number of values in the clipboard does not match the number of selected cells, Notify the user of the mismatch and do not perform the paste operation.

- [X] When a cell is selected, pressing Ctrl+X should cut the torque value(s) of the selected cell(s) to the clipboard and clear the torque value of the selected cell(s).

- [X] When a cell is selected, pressing Ctrl+C should copy the torque value. When multiple cells are selected, then pressing Ctrl+V should paste the copied value into all selected cells. Please implement this behavior.

- [ ] When a cell is in Override Mode, pressing enter exits that mode and moves the selection to the cell below. Please implement this same exit behavior for the arrow keypresses as well. E.g. if the user is in Override Mode and presses the down arrow key, the selection should move to the cell below and exit Override Mode.

# Border and Override Mode Bug interaction

- [X] When the user is in Override Mode (typing to overwrite selected cell(s)), and presses an arrow key, a second selection border appears around other cells as the user navigates the table cells with the arrow keys. During this, the Override cell is still highlighted and active; if the user types a number, even though another cell looks selected, the typed value is still applied to the Override cell. Please fix this bug so that only one selection border is visible at a time, and the correct cell is active for Override mode.

# Curve Graph

- [X] remove ability to zoom and pan the graph with a variable in the codebase. The graph should be static and show the full range of data.

- [!] When a curve series is selected in the data table, highlight the corresponding curve points on the graph.

- [!] When the user clicks and drags on the graph, all enclosed curve points should be selected. Highlight the selected curve points.

- [!] When curve points are selected on the graph, the user should be able to drag the selected points up or down to adjust the torque values. The corresponding torque values in the data table should update accordingly.

# General

- [X] When a default file is loaded, and the new clicks New File, the program crashes instead of opening a new blank file. Please fix this.