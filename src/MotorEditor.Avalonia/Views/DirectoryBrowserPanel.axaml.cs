using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.VisualTree;
using CurveEditor.ViewModels;
using System.Linq;

namespace CurveEditor.Views;

public partial class DirectoryBrowserPanel : UserControl
{
    public DirectoryBrowserPanel()
    {
        InitializeComponent();

        var tree = this.FindControl<TreeView>("ExplorerTree");
        if (tree is not null)
        {
            tree.AddHandler(PointerPressedEvent, OnExplorerTreePointerPressed, RoutingStrategies.Tunnel);
            tree.AddHandler(PointerWheelChangedEvent, OnExplorerTreePointerWheelChanged, RoutingStrategies.Tunnel);
        }
    }

    private void OnExplorerTreePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Let the built-in expander caret handle its own clicks.
        if (IsWithinExpander(e.Source as Visual))
        {
            return;
        }

        var treeViewItem = (e.Source as Visual)?.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
        if (treeViewItem?.DataContext is not ExplorerNodeViewModel node)
        {
            return;
        }

        // Folder name click toggles expand/collapse and should not select.
        if (node.IsDirectory && !node.IsRoot)
        {
            node.IsExpanded = !node.IsExpanded;
            e.Handled = true;
        }
    }

    private void OnExplorerTreePointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            return;
        }

        if (DataContext is not DirectoryBrowserViewModel viewModel)
        {
            return;
        }

        if (e.Delta.Y > 0)
        {
            viewModel.IncreaseFontSizeCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Delta.Y < 0)
        {
            viewModel.DecreaseFontSizeCommand.Execute(null);
            e.Handled = true;
        }
    }

    private static bool IsWithinExpander(Visual? source)
    {
        if (source is null)
        {
            return false;
        }

        // The default TreeViewItem template uses a ToggleButton for the expander.
        return source is ToggleButton || source.GetVisualAncestors().OfType<ToggleButton>().Any();
    }
}
