using CurveEditor.Models;
using CurveEditor.ViewModels;

namespace CurveEditor.Tests.ViewModels;

/// <summary>
/// Tests for the CurveDataTableViewModel class, including cell selection functionality.
/// </summary>
public class CurveDataTableViewModelTests
{
    [Fact]
    public void SelectCell_SelectsSingleCell()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();

        // Act
        viewModel.SelectCell(5, 2);

        // Assert
        Assert.Single(viewModel.SelectedCells);
        Assert.True(viewModel.IsCellSelected(5, 2));
        Assert.Equal(5, viewModel.SelectedRowIndex);
    }

    [Fact]
    public void SelectCell_ClearsPreviousSelection()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);
        viewModel.SelectCell(3, 2);

        // Act
        viewModel.SelectCell(10, 3);

        // Assert
        Assert.Single(viewModel.SelectedCells);
        Assert.True(viewModel.IsCellSelected(10, 3));
        Assert.False(viewModel.IsCellSelected(5, 2));
        Assert.False(viewModel.IsCellSelected(3, 2));
    }

    [Fact]
    public void ToggleCellSelection_AddsToSelection()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);

        // Act
        viewModel.ToggleCellSelection(10, 3);

        // Assert
        Assert.Equal(2, viewModel.SelectedCells.Count);
        Assert.True(viewModel.IsCellSelected(5, 2));
        Assert.True(viewModel.IsCellSelected(10, 3));
    }

    [Fact]
    public void ToggleCellSelection_RemovesFromSelection()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);
        viewModel.ToggleCellSelection(10, 3);

        // Act
        viewModel.ToggleCellSelection(5, 2);

        // Assert
        Assert.Single(viewModel.SelectedCells);
        Assert.False(viewModel.IsCellSelected(5, 2));
        Assert.True(viewModel.IsCellSelected(10, 3));
    }

    [Fact]
    public void SelectRange_SelectsRectangularRange()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(2, 2); // Set anchor

        // Act
        viewModel.SelectRange(5, 3);

        // Assert
        // Range from (2,2) to (5,3) = 4 rows x 2 columns = 8 cells
        Assert.Equal(8, viewModel.SelectedCells.Count);
        Assert.True(viewModel.IsCellSelected(2, 2));
        Assert.True(viewModel.IsCellSelected(5, 3));
        Assert.True(viewModel.IsCellSelected(3, 2));
        Assert.True(viewModel.IsCellSelected(4, 3));
    }

    [Fact]
    public void SelectRange_WorksWithReversedCoordinates()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 3); // Set anchor at bottom-right

        // Act
        viewModel.SelectRange(2, 2); // Select to top-left

        // Assert
        Assert.Equal(8, viewModel.SelectedCells.Count);
        Assert.True(viewModel.IsCellSelected(2, 2));
        Assert.True(viewModel.IsCellSelected(5, 3));
    }

    [Fact]
    public void SelectRectangularRange_SelectsCorrectCells()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        var start = new CellPosition(0, 2);
        var end = new CellPosition(2, 3);

        // Act
        viewModel.SelectRectangularRange(start, end);

        // Assert
        Assert.Equal(6, viewModel.SelectedCells.Count); // 3 rows x 2 columns
        Assert.True(viewModel.IsCellSelected(0, 2));
        Assert.True(viewModel.IsCellSelected(0, 3));
        Assert.True(viewModel.IsCellSelected(1, 2));
        Assert.True(viewModel.IsCellSelected(1, 3));
        Assert.True(viewModel.IsCellSelected(2, 2));
        Assert.True(viewModel.IsCellSelected(2, 3));
    }

    [Fact]
    public void ClearSelection_RemovesAllSelections()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);
        viewModel.ToggleCellSelection(10, 3);
        viewModel.ToggleCellSelection(15, 2);

        // Act
        viewModel.ClearSelection();

        // Assert
        Assert.Empty(viewModel.SelectedCells);
        Assert.Null(viewModel.AnchorCell);
    }

    [Fact]
    public void ExtendSelection_ExtendsDown()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);

        // Act
        viewModel.ExtendSelection(1, 0);

        // Assert
        Assert.Equal(2, viewModel.SelectedCells.Count);
        Assert.True(viewModel.IsCellSelected(5, 2));
        Assert.True(viewModel.IsCellSelected(6, 2));
    }

    [Fact]
    public void ExtendSelection_ExtendsUp()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);

        // Act
        viewModel.ExtendSelection(-1, 0);

        // Assert
        Assert.Equal(2, viewModel.SelectedCells.Count);
        Assert.True(viewModel.IsCellSelected(5, 2));
        Assert.True(viewModel.IsCellSelected(4, 2));
    }

    [Fact]
    public void ExtendSelection_ExtendsRight()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);

        // Act
        viewModel.ExtendSelection(0, 1);

        // Assert
        Assert.Equal(2, viewModel.SelectedCells.Count);
        Assert.True(viewModel.IsCellSelected(5, 2));
        Assert.True(viewModel.IsCellSelected(5, 3));
    }

    [Fact]
    public void ExtendSelection_ExtendsLeft()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 3);

        // Act
        viewModel.ExtendSelection(0, -1);

        // Assert
        Assert.Equal(2, viewModel.SelectedCells.Count);
        Assert.True(viewModel.IsCellSelected(5, 3));
        Assert.True(viewModel.IsCellSelected(5, 2));
    }

    [Fact]
    public void ExtendSelection_DoesNotExtendBeyondBounds()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(0, 0);

        // Act - try to extend up and left from (0,0)
        viewModel.ExtendSelection(-1, 0);
        viewModel.ExtendSelection(0, -1);

        // Assert - should still only have original cell
        Assert.Single(viewModel.SelectedCells);
        Assert.True(viewModel.IsCellSelected(0, 0));
    }

    [Fact]
    public void MoveSelection_MovesDown()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);

        // Act
        viewModel.MoveSelection(1, 0);

        // Assert
        Assert.Single(viewModel.SelectedCells);
        Assert.False(viewModel.IsCellSelected(5, 2));
        Assert.True(viewModel.IsCellSelected(6, 2));
    }

    [Fact]
    public void MoveSelection_MovesUp()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);

        // Act
        viewModel.MoveSelection(-1, 0);

        // Assert
        Assert.Single(viewModel.SelectedCells);
        Assert.False(viewModel.IsCellSelected(5, 2));
        Assert.True(viewModel.IsCellSelected(4, 2));
    }

    [Fact]
    public void MoveSelection_MovesRight()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);

        // Act
        viewModel.MoveSelection(0, 1);

        // Assert
        Assert.Single(viewModel.SelectedCells);
        Assert.False(viewModel.IsCellSelected(5, 2));
        Assert.True(viewModel.IsCellSelected(5, 3));
    }

    [Fact]
    public void MoveSelection_MovesLeft()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 3);

        // Act
        viewModel.MoveSelection(0, -1);

        // Assert
        Assert.Single(viewModel.SelectedCells);
        Assert.False(viewModel.IsCellSelected(5, 3));
        Assert.True(viewModel.IsCellSelected(5, 2));
    }

    [Fact]
    public void MoveSelection_ClampsToBounds()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(0, 0);

        // Act - try to move up and left from (0,0)
        viewModel.MoveSelection(-1, 0);
        viewModel.MoveSelection(0, -1);

        // Assert - should stay at (0,0)
        Assert.Single(viewModel.SelectedCells);
        Assert.True(viewModel.IsCellSelected(0, 0));
    }

    [Fact]
    public void ExtendSelectionToEnd_ExtendsDownToLastRow()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);

        // Act
        viewModel.ExtendSelectionToEnd(1, 0);

        // Assert
        var maxRow = viewModel.Rows.Count - 1;
        Assert.True(viewModel.IsCellSelected(5, 2));
        Assert.True(viewModel.IsCellSelected(maxRow, 2));
        Assert.All(viewModel.SelectedCells, cell =>
        {
            Assert.Equal(2, cell.ColumnIndex);
            Assert.InRange(cell.RowIndex, 5, maxRow);
        });
    }

    [Fact]
    public void ExtendSelectionToEnd_ExtendsUpToFirstRow()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        var lastRow = viewModel.Rows.Count - 1;
        viewModel.SelectCell(lastRow - 2, 2);

        // Act
        viewModel.ExtendSelectionToEnd(-1, 0);

        // Assert
        Assert.True(viewModel.IsCellSelected(0, 2));
        Assert.True(viewModel.IsCellSelected(lastRow - 2, 2));
        Assert.All(viewModel.SelectedCells, cell =>
        {
            Assert.Equal(2, cell.ColumnIndex);
            Assert.InRange(cell.RowIndex, 0, lastRow - 2);
        });
    }

    [Fact]
    public void ExtendSelectionToEnd_ExtendsRightToLastColumn()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);

        // Act
        viewModel.ExtendSelectionToEnd(0, 1);

        // Assert
        var maxCol = viewModel.ColumnCount - 1;
        Assert.True(viewModel.IsCellSelected(5, 2));
        Assert.True(viewModel.IsCellSelected(5, maxCol));
        Assert.All(viewModel.SelectedCells, cell =>
        {
            Assert.Equal(5, cell.RowIndex);
            Assert.InRange(cell.ColumnIndex, 2, maxCol);
        });
    }

    [Fact]
    public void ExtendSelectionToEnd_ExtendsLeftToFirstColumn()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        var lastCol = viewModel.ColumnCount - 1;
        viewModel.SelectCell(5, lastCol - 1);

        // Act
        viewModel.ExtendSelectionToEnd(0, -1);

        // Assert
        Assert.True(viewModel.IsCellSelected(5, 0));
        Assert.True(viewModel.IsCellSelected(5, lastCol - 1));
        Assert.All(viewModel.SelectedCells, cell =>
        {
            Assert.Equal(5, cell.RowIndex);
            Assert.InRange(cell.ColumnIndex, 0, lastCol - 1);
        });
    }

    [Fact]
    public void GetSeriesNameForColumn_ReturnsNullForFixedColumns()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();

        // Act & Assert
        Assert.Null(viewModel.GetSeriesNameForColumn(0)); // % column
        Assert.Null(viewModel.GetSeriesNameForColumn(1)); // RPM column
    }

    [Fact]
    public void GetSeriesNameForColumn_ReturnsSeriesName()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();

        // Act
        var seriesName = viewModel.GetSeriesNameForColumn(2);

        // Assert
        Assert.Equal("Peak", seriesName);
    }

    [Fact]
    public void GetColumnIndexForSeries_ReturnsCorrectIndex()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();

        // Act
        var peakIndex = viewModel.GetColumnIndexForSeries("Peak");
        var continuousIndex = viewModel.GetColumnIndexForSeries("Continuous");

        // Assert
        Assert.Equal(2, peakIndex);
        Assert.Equal(3, continuousIndex);
    }

    [Fact]
    public void GetColumnIndexForSeries_ReturnsMinusOneForUnknownSeries()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();

        // Act
        var index = viewModel.GetColumnIndexForSeries("NonExistent");

        // Assert
        Assert.Equal(-1, index);
    }

    [Fact]
    public void ColumnCount_ReturnsCorrectCount()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();

        // Assert - 2 fixed columns (%, RPM) + 2 series columns (Peak, Continuous)
        Assert.Equal(4, viewModel.ColumnCount);
    }

    [Fact]
    public void ClearSelection_DoesNotChangeTorqueValues()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        var originalPeak = viewModel.GetTorque(0, "Peak");
        var originalContinuous = viewModel.GetTorque(0, "Continuous");
        viewModel.SelectCell(0, 2); // Peak at row 0
        viewModel.ToggleCellSelection(0, 3); // Continuous at row 0

        // Act
        viewModel.ClearSelection();

        // Assert - clearing selection alone should not modify data
        Assert.Equal(originalPeak, viewModel.GetTorque(0, "Peak"));
        Assert.Equal(originalContinuous, viewModel.GetTorque(0, "Continuous"));
    }

        [Fact]
        public void ApplyTorqueToCells_UpdatesEditableTorqueCells()
        {
            // Arrange
            var viewModel = CreateViewModelWithData();
            var cells = new[]
            {
                new CellPosition(0, 2), // Peak torque at row 0
                new CellPosition(1, 3)  // Continuous torque at row 1
            };

            // Act
            viewModel.ApplyTorqueToCells(cells, 12.34);

            // Assert
            Assert.Equal(12.34, viewModel.GetTorque(0, "Peak"));
            Assert.Equal(12.34, viewModel.GetTorque(1, "Continuous"));
        }

        [Fact]
        public void ApplyTorqueToCells_DoesNotAffectPercentOrRpmColumns()
        {
            // Arrange
            var viewModel = CreateViewModelWithData();
            var originalPercent = viewModel.Rows[0].Percent;
            var originalRpm = viewModel.Rows[0].DisplayRpm;
            var cells = new[]
            {
                new CellPosition(0, 0), // % column
                new CellPosition(0, 1)  // RPM column
            };

            // Act
            viewModel.ApplyTorqueToCells(cells, 99.99);

            // Assert - fixed columns unchanged
            Assert.Equal(originalPercent, viewModel.Rows[0].Percent);
            Assert.Equal(originalRpm, viewModel.Rows[0].DisplayRpm);
        }

        [Fact]
        public void ApplyTorqueToCells_DoesNotModifyLockedSeries()
        {
            // Arrange
            var viewModel = CreateViewModelWithData();
            // Lock the Peak series
            var peakSeries = viewModel.SeriesColumns.First(s => s.Name == "Peak");
            peakSeries.Locked = true;
            var originalPeak = viewModel.GetTorque(0, "Peak");
            var cells = new[] { new CellPosition(0, 2) };

            // Act
            viewModel.ApplyTorqueToCells(cells, originalPeak + 10);

            // Assert - locked series unchanged
            Assert.Equal(originalPeak, viewModel.GetTorque(0, "Peak"));
        }

        [Fact]
        public void ApplyTorqueToCells_RaisesDataChangedWhenAnyCellUpdated()
        {
            // Arrange
            var viewModel = CreateViewModelWithData();
            var cells = new[] { new CellPosition(0, 2) };
            var eventRaised = false;
            viewModel.DataChanged += (s, e) => eventRaised = true;

            // Act
            viewModel.ApplyTorqueToCells(cells, viewModel.GetTorque(0, "Peak") + 1);

            // Assert
            Assert.True(eventRaised);
        }

    [Fact]
    public void SelectionChanged_RaisedWhenSelecting()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        var eventRaised = false;
        viewModel.SelectionChanged += (s, e) => eventRaised = true;

        // Act
        viewModel.SelectCell(5, 2);

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void SelectionChanged_RaisedWhenToggling()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);
        var eventRaised = false;
        viewModel.SelectionChanged += (s, e) => eventRaised = true;

        // Act
        viewModel.ToggleCellSelection(10, 3);

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void SelectionChanged_RaisedWhenClearing()
    {
        // Arrange
        var viewModel = CreateViewModelWithData();
        viewModel.SelectCell(5, 2);
        var eventRaised = false;
        viewModel.SelectionChanged += (s, e) => eventRaised = true;

        // Act
        viewModel.ClearSelection();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void CellPosition_Compare_OrdersByRowThenColumn()
    {
        // Arrange
        var a = new CellPosition(1, 5);
        var b = new CellPosition(2, 3);
        var c = new CellPosition(1, 3);

        // Assert
        Assert.True(CellPosition.Compare(a, b) < 0); // (1,5) < (2,3) because row 1 < row 2
        Assert.True(CellPosition.Compare(c, a) < 0); // (1,3) < (1,5) because same row, column 3 < 5
        Assert.Equal(0, CellPosition.Compare(a, a)); // equal
    }

    private static CurveDataTableViewModel CreateViewModelWithData()
    {
        var viewModel = new CurveDataTableViewModel();
        var voltage = CreateTestVoltageConfiguration();
        viewModel.CurrentVoltage = voltage;
        return viewModel;
    }

    private static VoltageConfiguration CreateTestVoltageConfiguration()
    {
        var voltage = new VoltageConfiguration(220)
        {
            MaxSpeed = 5000,
            Power = 1500,
            RatedPeakTorque = 55,
            RatedContinuousTorque = 45
        };

        var peakSeries = new CurveSeries("Peak");
        peakSeries.InitializeData(5000, 55);
        
        var continuousSeries = new CurveSeries("Continuous");
        continuousSeries.InitializeData(5000, 45);

        voltage.Series.Add(peakSeries);
        voltage.Series.Add(continuousSeries);

        return voltage;
    }
}
