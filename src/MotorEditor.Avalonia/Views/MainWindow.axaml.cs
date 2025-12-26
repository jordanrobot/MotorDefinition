using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CurveEditor.Behaviors;
using CurveEditor.ViewModels;
using MotorEditor.Avalonia.Models;

namespace CurveEditor.Views;

public partial class MainWindow : Window
{
    private const string LeftZoneWidthKey = "MainWindow.LeftZone.Width";
    private const string RightZoneWidthKey = "MainWindow.RightZone.Width";
    private const string LegacyLeftZoneWidthKey = "MainWindow.LeftPanel";
    private const string LegacyRightZoneWidthKey = "MainWindow.PropertiesPanel";

    public MainWindow()
    {
        InitializeComponent();
        WindowBoundsPersistence.Attach(this, "MainWindow");
        Opened += OnOpened;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        Opened -= OnOpened;

        var rootGrid = this.FindControl<Grid>("RootLayoutGrid");
        var mainGrid = this.FindControl<Grid>("MainLayoutGrid");
        var centerGrid = this.FindControl<Grid>("CenterGrid");
        var panelBar = this.FindControl<PanelBar>("PanelBarControl");

        if (rootGrid is null || mainGrid is null || centerGrid is null || DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        // Wire up PanelBar to ViewModel
        if (panelBar is not null)
        {
            panelBar.PanelClicked += (_, panelId) =>
            {
                viewModel.TogglePanel(panelId);
            };

            // Update PanelBar when active panel changes in either zone.
            // Multiple zones may be expanded simultaneously; PanelBar must highlight all expanded panels.
            viewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.ActivePanelBarPanelId) ||
                    args.PropertyName == nameof(MainWindowViewModel.ActiveLeftPanelId))
                {
                    panelBar.ActivePanelIds = GetPanelBarActivePanelIds(viewModel);
                }
                else if (args.PropertyName == nameof(MainWindowViewModel.PanelBarDockSide))
                {
                    ApplyPanelBarDockSide(rootGrid, panelBar, viewModel.PanelBarDockSide);
                }
            };

            // Set initial state
            panelBar.ActivePanelIds = GetPanelBarActivePanelIds(viewModel);
            ApplyPanelBarDockSide(rootGrid, panelBar, viewModel.PanelBarDockSide);
        }

        // Load persisted state for both zones
        var activeRightPanelId = PanelLayoutPersistence.LoadString("MainWindow.ActivePanelBarPanelId");
        if (activeRightPanelId is not null && PanelRegistry.GetById(activeRightPanelId) is not null)
        {
            var descriptor = PanelRegistry.GetById(activeRightPanelId);
            if (descriptor?.Zone == PanelZone.Right)
            {
                viewModel.ActivePanelBarPanelId = activeRightPanelId;
            }
        }

        var activeLeftPanelId = PanelLayoutPersistence.LoadString("MainWindow.ActiveLeftPanelId");
        if (activeLeftPanelId is not null && PanelRegistry.GetById(activeLeftPanelId) is not null)
        {
            var descriptor = PanelRegistry.GetById(activeLeftPanelId);
            if (descriptor?.Zone == PanelZone.Left)
            {
                viewModel.ActiveLeftPanelId = activeLeftPanelId;
            }
        }

        // Attach persistence for PanelBarDockSide
        var dockSide = PanelLayoutPersistence.LoadDockSide("MainWindow.PanelBarDockSide");
        if (dockSide.HasValue)
        {
            viewModel.PanelBarDockSide = dockSide.Value;
        }

        // Apply initial layout for expanded/collapsed state and per-zone widths.
        ApplyLeftZoneLayout(mainGrid, viewModel);
        ApplyRightZoneLayout(mainGrid, viewModel);

        // Save panel state on window closing
        Closing += (_, _) =>
        {
            PanelLayoutPersistence.SaveString("MainWindow.ActivePanelBarPanelId", viewModel.ActivePanelBarPanelId);
            PanelLayoutPersistence.SaveString("MainWindow.ActiveLeftPanelId", viewModel.ActiveLeftPanelId);
            PanelLayoutPersistence.SaveDockSide("MainWindow.PanelBarDockSide", viewModel.PanelBarDockSide);

            PersistActiveZoneWidths(mainGrid, viewModel);
        };

        // React to active zone changes.
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.ActiveLeftPanelId))
            {
                PersistLeftZoneWidthIfNeeded(mainGrid);
                ApplyLeftZoneLayout(mainGrid, viewModel);
            }
            else if (args.PropertyName == nameof(MainWindowViewModel.ActivePanelBarPanelId))
            {
                PersistRightZoneWidthIfNeeded(mainGrid);
                ApplyRightZoneLayout(mainGrid, viewModel);
            }
        };

        await viewModel.RestoreSessionAfterWindowOpenedAsync();
    }

    private static IReadOnlyCollection<string> GetPanelBarActivePanelIds(MainWindowViewModel viewModel)
    {
        var ids = new List<string>(capacity: 2);

        if (!string.IsNullOrWhiteSpace(viewModel.ActiveLeftPanelId))
        {
            ids.Add(viewModel.ActiveLeftPanelId);
        }

        if (!string.IsNullOrWhiteSpace(viewModel.ActivePanelBarPanelId))
        {
            ids.Add(viewModel.ActivePanelBarPanelId);
        }

        return ids.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static void ApplyPanelBarDockSide(Grid rootGrid, PanelBar panelBar, PanelBarDockSide dockSide)
    {
        // Root grid is "Auto,*,Auto". Main layout stays in the center column.
        // PanelBar moves between column 0 and column 2.
        panelBar.SetValue(Grid.ColumnProperty, dockSide == PanelBarDockSide.Right ? 2 : 0);
    }

    private static double GetColumnWidth(GridLength columnWidth, double actualWidth)
    {
        var width = actualWidth > 0 ? actualWidth : columnWidth.Value;
        return width;
    }

    private static string GetPanelWidthKey(string panelId) => $"MainWindow.{panelId}.Width";

    private static double GetPanelDefaultWidth(string panelId, double fallback)
    {
        var descriptor = PanelRegistry.GetById(panelId);
        return descriptor?.DefaultWidth is double width && width > 0 ? width : fallback;
    }

    private static double LoadZoneWidthOrFallback(string zoneWidthKey, string legacyZoneKey, string panelId, double defaultWidth)
    {
        if (PanelLayoutPersistence.LoadWidth(zoneWidthKey) is double zoneWidth)
        {
            return zoneWidth;
        }

        if (PanelLayoutPersistence.LoadWidth(legacyZoneKey) is double legacyZoneWidth)
        {
            return legacyZoneWidth;
        }

        // Migration fallback: if the user previously had per-panel widths, adopt the current panel's width
        // as the zone width on first run after the change.
        if (PanelLayoutPersistence.LoadWidth(GetPanelWidthKey(panelId)) is double panelWidth)
        {
            return panelWidth;
        }

        return GetPanelDefaultWidth(panelId, defaultWidth);
    }

    private void ApplyLeftZoneLayout(Grid mainGrid, MainWindowViewModel viewModel)
    {
        var leftColumn = mainGrid.ColumnDefinitions[0];
        var splitterColumn = mainGrid.ColumnDefinitions[1];

        if (string.IsNullOrWhiteSpace(viewModel.ActiveLeftPanelId))
        {
            leftColumn.Width = new GridLength(0, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
            return;
        }

        var panelId = viewModel.ActiveLeftPanelId;
        var width = LoadZoneWidthOrFallback(LeftZoneWidthKey, LegacyLeftZoneWidthKey, panelId, defaultWidth: 200);

        leftColumn.Width = new GridLength(width, GridUnitType.Pixel);
        splitterColumn.Width = new GridLength(4, GridUnitType.Pixel);
    }

    private void ApplyRightZoneLayout(Grid mainGrid, MainWindowViewModel viewModel)
    {
        var splitterColumn = mainGrid.ColumnDefinitions[3];
        var propertiesColumn = mainGrid.ColumnDefinitions[4];

        if (string.IsNullOrWhiteSpace(viewModel.ActivePanelBarPanelId))
        {
            propertiesColumn.Width = new GridLength(0, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
            return;
        }

        var panelId = viewModel.ActivePanelBarPanelId;
        var width = LoadZoneWidthOrFallback(RightZoneWidthKey, LegacyRightZoneWidthKey, panelId, defaultWidth: 280);

        propertiesColumn.Width = new GridLength(width, GridUnitType.Pixel);
        splitterColumn.Width = new GridLength(4, GridUnitType.Pixel);
    }

    private void PersistLeftZoneWidthIfNeeded(Grid mainGrid)
    {
        var leftColumn = mainGrid.ColumnDefinitions[0];
        var currentWidth = GetColumnWidth(leftColumn.Width, leftColumn.ActualWidth);
        if (currentWidth <= 0)
        {
            return;
        }

        PanelLayoutPersistence.UpdateColumnWidth(LeftZoneWidthKey, currentWidth);
    }

    private void PersistRightZoneWidthIfNeeded(Grid mainGrid)
    {
        var column = mainGrid.ColumnDefinitions[4];
        var currentWidth = GetColumnWidth(column.Width, column.ActualWidth);
        if (currentWidth <= 0)
        {
            return;
        }

        PanelLayoutPersistence.UpdateColumnWidth(RightZoneWidthKey, currentWidth);
    }

    private void PersistActiveZoneWidths(Grid mainGrid, MainWindowViewModel viewModel)
    {
        if (!string.IsNullOrWhiteSpace(viewModel.ActiveLeftPanelId))
        {
            var leftColumn = mainGrid.ColumnDefinitions[0];
            var width = GetColumnWidth(leftColumn.Width, leftColumn.ActualWidth);
            if (width > 0)
            {
                PanelLayoutPersistence.UpdateColumnWidth(LeftZoneWidthKey, width);
            }
        }

        if (!string.IsNullOrWhiteSpace(viewModel.ActivePanelBarPanelId))
        {
            var rightColumn = mainGrid.ColumnDefinitions[4];
            var width = GetColumnWidth(rightColumn.Width, rightColumn.ActualWidth);
            if (width > 0)
            {
                PanelLayoutPersistence.UpdateColumnWidth(RightZoneWidthKey, width);
            }
        }
    }

    // Motor properties edit handlers moved to MotorPropertiesPanel.
}