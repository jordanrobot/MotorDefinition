using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using MotorEditor.Avalonia.Models;

namespace CurveEditor.Views;

public partial class PanelBar : UserControl
{
    public static readonly StyledProperty<PanelBarDockSide> DockSideProperty =
        AvaloniaProperty.Register<PanelBar, PanelBarDockSide>(nameof(DockSide), PanelBarDockSide.Left);

    static PanelBar()
    {
        DockSideProperty.Changed.AddClassHandler<PanelBar>((bar, _) => bar.UpdateDockSideBorder());
    }

    public PanelBar()
    {
        InitializeComponent();
        PanelClickCommand = new RelayCommand<string>(OnPanelClick);
        
        // Set the panel items from the registry
        var items = this.FindControl<ItemsControl>("PanelItems");
        if (items is not null)
        {
            items.ItemsSource = PanelRegistry.PanelBarPanels;
        }

        UpdateDockSideBorder();
    }

    public ICommand PanelClickCommand { get; }

    public PanelBarDockSide DockSide
    {
        get => GetValue(DockSideProperty);
        set => SetValue(DockSideProperty, value);
    }

    private IReadOnlyCollection<string> _activePanelIds = Array.Empty<string>();
    public IReadOnlyCollection<string> ActivePanelIds
    {
        get => _activePanelIds;
        set
        {
            _activePanelIds = value ?? Array.Empty<string>();
            UpdateButtonStyles();
        }
    }

    public event EventHandler<string>? PanelClicked;

    private void OnPanelClick(string? panelId)
    {
        if (panelId is not null)
        {
            PanelClicked?.Invoke(this, panelId);
        }
    }

    private void UpdateButtonStyles()
    {
        var items = this.FindControl<ItemsControl>("PanelItems");
        if (items is null)
        {
            return;
        }

        var activeIds = _activePanelIds.Count == 0
            ? null
            : new HashSet<string>(_activePanelIds, StringComparer.Ordinal);

        // Find all buttons in the visual tree and update their classes
        var buttons = items.GetVisualDescendants().OfType<Button>();
        foreach (var button in buttons)
        {
            var panelId = button.CommandParameter as string;
            var isActive = panelId is not null && activeIds is not null && activeIds.Contains(panelId);
            
            if (isActive && !button.Classes.Contains("Active"))
            {
                button.Classes.Add("Active");
            }
            else if (!isActive && button.Classes.Contains("Active"))
            {
                button.Classes.Remove("Active");
            }
        }
    }

    private void UpdateDockSideBorder()
    {
        var border = this.FindControl<Border>("RootBorder");
        if (border is null)
        {
            return;
        }

        border.BorderThickness = DockSide switch
        {
            PanelBarDockSide.Left => new Thickness(0, 0, 1, 0),
            PanelBarDockSide.Right => new Thickness(1, 0, 0, 0),
            _ => new Thickness(0)
        };
    }
}

