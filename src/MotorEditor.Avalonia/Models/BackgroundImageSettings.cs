using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MotorEditor.Avalonia.Models;

/// <summary>
/// Settings for a background image associated with a specific drive and voltage.
/// </summary>
public partial class BackgroundImageSettings : ObservableObject
{
    /// <summary>
    /// Path to the background image file.
    /// </summary>
    [ObservableProperty]
    private string _imagePath = string.Empty;

    /// <summary>
    /// Name of the drive this image is associated with.
    /// </summary>
    [ObservableProperty]
    private string _driveName = string.Empty;

    /// <summary>
    /// Voltage value this image is associated with.
    /// </summary>
    [ObservableProperty]
    private double _voltageValue;

    /// <summary>
    /// Whether the image is currently visible.
    /// </summary>
    [ObservableProperty]
    private bool _isVisible = true;

    /// <summary>
    /// Whether the image is locked to the graph zero point.
    /// </summary>
    [ObservableProperty]
    private bool _isLockedToZero = true;

    /// <summary>
    /// X-axis offset from the graph zero point (in data units).
    /// </summary>
    [ObservableProperty]
    private double _offsetX;

    /// <summary>
    /// Y-axis offset from the graph zero point (in data units).
    /// </summary>
    [ObservableProperty]
    private double _offsetY;

    /// <summary>
    /// X-axis scale factor (1.0 = 100%).
    /// </summary>
    [ObservableProperty]
    private double _scaleX = 1.0;

    /// <summary>
    /// Y-axis scale factor (1.0 = 100%).
    /// </summary>
    [ObservableProperty]
    private double _scaleY = 1.0;
}

/// <summary>
/// Container for all background image settings for a motor file.
/// </summary>
public partial class MotorBackgroundImageSettings : ObservableObject
{
    /// <summary>
    /// List of background image settings for each drive/voltage combination.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<BackgroundImageSettings> _images = new();
}
