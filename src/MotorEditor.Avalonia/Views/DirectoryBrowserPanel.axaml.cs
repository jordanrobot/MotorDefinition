using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.LogicalTree;
using CurveEditor.ViewModels;
using System.Linq;

namespace CurveEditor.Views;

public partial class DirectoryBrowserPanel : UserControl
{
    private TextBox? _activeRenameTextBox;

    public DirectoryBrowserPanel()
    {
        InitializeComponent();

        var tree = this.FindControl<TreeView>("ExplorerTree");
        if (tree is not null)
        {
            tree.AddHandler(PointerPressedEvent, OnExplorerTreePointerPressed, RoutingStrategies.Tunnel);
            tree.AddHandler(PointerWheelChangedEvent, OnExplorerTreePointerWheelChanged, RoutingStrategies.Tunnel);
            tree.AddHandler(ContextRequestedEvent, OnExplorerTreeContextRequested, RoutingStrategies.Tunnel);
            tree.AddHandler(KeyDownEvent, OnExplorerTreeKeyDown, RoutingStrategies.Tunnel);
        }

        // Handle global key events for rename text box
        this.AddHandler(KeyDownEvent, OnPanelKeyDown, RoutingStrategies.Bubble);
    }

    private void OnPanelKeyDown(object? sender, KeyEventArgs e)
    {
        // Check if we're in a rename text box
        if (e.Source is TextBox textBox && textBox.Name == "RenameTextBox")
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                CompleteRename(textBox);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CancelRename(textBox);
                e.Handled = true;
            }
            // Arrow keys are not handled - TextBox will process them for cursor movement
        }
    }

    private void OnExplorerTreeKeyDown(object? sender, KeyEventArgs e)
    {
        // Block tree navigation shortcuts during rename to prevent conflicts
        if (_activeRenameTextBox is not null)
        {
            return;
        }

        if (DataContext is not DirectoryBrowserViewModel viewModel)
        {
            return;
        }

        // F2 - Rename
        if (e.Key == Key.F2)
        {
            _ = viewModel.StartRenameCommand.ExecuteAsync(null);
            e.Handled = true;
            return;
        }

        // Ctrl+C - Copy
        if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            _ = viewModel.CopySelectedCommand.ExecuteAsync(null);
            e.Handled = true;
            return;
        }

        // Delete - Delete
        if (e.Key == Key.Delete)
        {
            _ = viewModel.DeleteSelectedCommand.ExecuteAsync(null);
            e.Handled = true;
            return;
        }

        // Ctrl+D - Duplicate
        if (e.Key == Key.D && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            _ = viewModel.DuplicateSelectedCommand.ExecuteAsync(null);
            e.Handled = true;
            return;
        }
    }

    private void CompleteRename(TextBox textBox)
    {
        if (DataContext is not DirectoryBrowserViewModel viewModel)
        {
            return;
        }

        if (textBox.DataContext is not ExplorerNodeViewModel node)
        {
            return;
        }

        var newName = textBox.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(newName))
        {
            _ = viewModel.CompleteRenameAsync(node, newName);
        }
        else
        {
            _ = viewModel.CancelRenameAsync(node);
        }
        
        _activeRenameTextBox = null;
    }

    private void CancelRename(TextBox textBox)
    {
        if (DataContext is not DirectoryBrowserViewModel viewModel)
        {
            return;
        }

        if (textBox.DataContext is not ExplorerNodeViewModel node)
        {
            return;
        }

        _ = viewModel.CancelRenameAsync(node);
        _activeRenameTextBox = null;
    }

    /// <summary>
    /// Called when a rename TextBox is attached to the visual tree.
    /// Sets up event handlers and focuses the TextBox with text selected.
    /// </summary>
    private void OnRenameTextBoxAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        _activeRenameTextBox = textBox;

        // Subscribe to LostFocus to auto-complete rename when clicking elsewhere
        textBox.LostFocus += OnRenameTextBoxLostFocus;
        
        // Subscribe to DetachedFromVisualTree to clean up
        textBox.DetachedFromVisualTree += OnRenameTextBoxDetached;

        // Focus the TextBox and select all text
        textBox.Focus();
        textBox.SelectAll();
    }

    /// <summary>
    /// Called when the rename TextBox loses focus.
    /// Completes the rename operation.
    /// </summary>
    private void OnRenameTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        // Unsubscribe from events
        textBox.LostFocus -= OnRenameTextBoxLostFocus;
        textBox.DetachedFromVisualTree -= OnRenameTextBoxDetached;

        // Complete the rename
        CompleteRename(textBox);
    }

    /// <summary>
    /// Called when the rename TextBox is detached from the visual tree.
    /// Cleans up event handlers and state.
    /// </summary>
    private void OnRenameTextBoxDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        // Unsubscribe from events
        textBox.LostFocus -= OnRenameTextBoxLostFocus;
        textBox.DetachedFromVisualTree -= OnRenameTextBoxDetached;

        // Clear the active reference
        if (_activeRenameTextBox == textBox)
        {
            _activeRenameTextBox = null;
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

    private void OnExplorerTreeContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (DataContext is not DirectoryBrowserViewModel viewModel)
        {
            return;
        }

        var treeViewItem = (e.Source as Visual)?.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
        if (treeViewItem?.DataContext is not ExplorerNodeViewModel node)
        {
            return;
        }

        // Don't show context menu for placeholders or root
        if (node.IsPlaceholder || node.IsRoot)
        {
            return;
        }

        // Select the node that was right-clicked
        viewModel.SelectedNode = node;

        var contextMenu = new ContextMenu();
        var commands = node.IsDirectory ? viewModel.DirectoryContextCommands : viewModel.FileContextCommands;

        foreach (var command in commands)
        {
            if (command.CanExecute(node.FullPath, node.IsDirectory))
            {
                var menuItem = new MenuItem
                {
                    Header = command.DisplayName,
                    Command = viewModel.ExecuteContextCommandCommand,
                    CommandParameter = command
                };
                contextMenu.Items.Add(menuItem);
            }
        }

        if (contextMenu.Items.Count > 0)
        {
            contextMenu.Open(treeViewItem);
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
