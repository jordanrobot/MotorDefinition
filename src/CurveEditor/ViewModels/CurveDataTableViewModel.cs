using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurveEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace CurveEditor.ViewModels;

/// <summary>
/// Represents a single row in the curve data table, containing speed info and torque values for all series.
/// </summary>
public class CurveDataRow : INotifyPropertyChanged
{
    private readonly VoltageConfiguration _voltage;
    private readonly int _rowIndex;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Creates a new CurveDataRow for the specified row index.
    /// </summary>
    public CurveDataRow(VoltageConfiguration voltage, int rowIndex)
    {
        _voltage = voltage ?? throw new ArgumentNullException(nameof(voltage));
        _rowIndex = rowIndex;
    }

    /// <summary>
    /// Gets the percentage value for this row (0-100).
    /// </summary>
    public int Percent => _rowIndex;

    /// <summary>
    /// Gets the RPM value for this row based on the first series.
    /// </summary>
    public int DisplayRpm
    {
        get
        {
            var firstSeries = _voltage.Series.FirstOrDefault();
            if (firstSeries is not null && _rowIndex < firstSeries.Data.Count)
            {
                return firstSeries.Data[_rowIndex].DisplayRpm;
            }
            return 0;
        }
    }

    /// <summary>
    /// Gets the torque value for a specific series at this row.
    /// </summary>
    public double GetTorque(string seriesName)
    {
        var series = _voltage.Series.FirstOrDefault(s => s.Name == seriesName);
        if (series is not null && _rowIndex < series.Data.Count)
        {
            return series.Data[_rowIndex].Torque;
        }
        return 0;
    }

    /// <summary>
    /// Sets the torque value for a specific series at this row.
    /// </summary>
    public void SetTorque(string seriesName, double value)
    {
        var series = _voltage.Series.FirstOrDefault(s => s.Name == seriesName);
        if (series is not null && _rowIndex < series.Data.Count)
        {
            series.Data[_rowIndex].Torque = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Torque_{seriesName}"));
        }
    }

    /// <summary>
    /// Gets the row index.
    /// </summary>
    public int RowIndex => _rowIndex;
}

/// <summary>
/// ViewModel for the curve data table that shows all series as columns.
/// </summary>
public partial class CurveDataTableViewModel : ViewModelBase
{
    private VoltageConfiguration? _currentVoltage;

    [ObservableProperty]
    private ObservableCollection<CurveDataRow> _rows = [];

    [ObservableProperty]
    private ObservableCollection<CurveSeries> _seriesColumns = [];

    [ObservableProperty]
    private CurveDataRow? _selectedRow;

    [ObservableProperty]
    private CurveSeries? _selectedSeries;

    [ObservableProperty]
    private int _selectedRowIndex = -1;

    [ObservableProperty]
    private string? _selectedSeriesName;

    /// <summary>
    /// Event raised when data changes.
    /// </summary>
    public event EventHandler? DataChanged;

    /// <summary>
    /// Gets or sets the current voltage configuration.
    /// </summary>
    public VoltageConfiguration? CurrentVoltage
    {
        get => _currentVoltage;
        set
        {
            if (_currentVoltage == value) return;
            _currentVoltage = value;
            OnPropertyChanged();
            RefreshData();
        }
    }

    /// <summary>
    /// Refreshes the data table with current voltage configuration data.
    /// </summary>
    public void RefreshData()
    {
        Rows.Clear();
        SeriesColumns.Clear();

        if (_currentVoltage is null || _currentVoltage.Series.Count == 0)
        {
            return;
        }

        // Add series columns
        foreach (var series in _currentVoltage.Series)
        {
            SeriesColumns.Add(series);
        }

        // Determine number of rows (should be 101 for 0-100%)
        var rowCount = _currentVoltage.Series.FirstOrDefault()?.Data.Count ?? 0;

        // Create rows
        for (var i = 0; i < rowCount; i++)
        {
            Rows.Add(new CurveDataRow(_currentVoltage, i));
        }
    }

    /// <summary>
    /// Updates a torque value in the data table.
    /// </summary>
    public void UpdateTorque(int rowIndex, string seriesName, double value)
    {
        if (rowIndex >= 0 && rowIndex < Rows.Count)
        {
            Rows[rowIndex].SetTorque(seriesName, value);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets the torque value for a specific row and series.
    /// </summary>
    public double GetTorque(int rowIndex, string seriesName)
    {
        if (rowIndex >= 0 && rowIndex < Rows.Count)
        {
            return Rows[rowIndex].GetTorque(seriesName);
        }
        return 0;
    }

    /// <summary>
    /// Checks if a series is locked.
    /// </summary>
    public bool IsSeriesLocked(string seriesName)
    {
        var series = _currentVoltage?.Series.FirstOrDefault(s => s.Name == seriesName);
        return series?.Locked ?? false;
    }
}
