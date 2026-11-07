using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using JAStudio.UI.Menus;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Navigation.TagManagement;

namespace JAStudio.UI.Views.Workspace.Navigation.TagManagement;

/// <summary>
/// The view of a <see cref="TagManagementViewModel"/>: the "Tags" tab of the <see cref="NavigationPane"/>, the tag tree with a filter box above it.<br/>
/// It turns keys, clicks and the right-click menu into the view model's operations.
/// </summary>
/// <remarks>
/// Enter or a double-click on a line searches for the notes in its branch, Down in the filter box moves the keyboard to the tree,<br/>
/// and the right-click menu, also on the menu key, has the rest. F5, anywhere in the tab, counts the tags again.
/// </remarks>
public partial class TagManagementView : UserControl
{
   /// <summary>The style class of the content of a line of the tree, its texts, in the view's AXAML.</summary>
   const string LineContentClass = "TagLine";

   public TagManagementView()
   {
      InitializeComponent();
      TagTree.AddHandler(KeyDownEvent, OnKeyDownInTheTree, RoutingStrategies.Tunnel);
      TagTree.ContextRequested += OnContextRequestedInTheTree;
      FilterBox.AddHandler(KeyDownEvent, OnKeyDownInTheFilterBox, RoutingStrategies.Tunnel);
   }

   TagManagementViewModel ViewModel => DataContext as TagManagementViewModel ?? throw new InvalidOperationException("A TagManagementView shows a TagManagementViewModel.");

   void OnKeyDownInTheTree(object? sender, KeyEventArgs e)
   {
      if(e is not { Key: Key.Enter, KeyModifiers: KeyModifiers.None }) return;

      ViewModel.SearchForTheSelectedTag();
      e.Handled = true;
   }

   /// <summary>Down moves the keyboard from the filter box to the tree, onto the selected line, or the first line when none is selected.</summary>
   void OnKeyDownInTheFilterBox(object? sender, KeyEventArgs e)
   {
      if(e is not { Key: Key.Down, KeyModifiers: KeyModifiers.None }) return;

      ViewModel.SelectedLine ??= ViewModel.TopLevelLines is [var firstLine, ..] ? firstLine : null;
      Dispatcher.UIThread.Post(FocusTheSelectedLine, DispatcherPriority.Loaded);
      e.Handled = true;
   }

   /// <summary>
   /// Double-clicking a line searches for the notes in its branch. It's handled on the line's content, before the line itself sees it,<br/>
   /// since a line with lines below it would also expand or collapse on a double-click.
   /// </summary>
   void OnDoubleTappedOnALine(object? sender, TappedEventArgs e)
   {
      if(sender is not Control { DataContext: TagTreeNodeViewModel line })
         throw new InvalidOperationException("A line of the tag tree shows something other than a TagTreeNodeViewModel.");

      e.Handled = true;
      ViewModel.SelectedLine = line;
      ViewModel.SearchForTheSelectedTag();
   }

   /// <summary>Right-clicking a line selects it and shows its menu. From the menu key, the menu opens below the selected line.</summary>
   void OnContextRequestedInTheTree(object? sender, ContextRequestedEventArgs e)
   {
      if((e.Source as Visual)?.FindAncestorOfType<TreeViewItem>(includeSelf: true) is { DataContext: TagTreeNodeViewModel line })
         ViewModel.SelectedLine = line;

      var menuTopLeft = e.TryGetPosition(this, out _) ? WindowPositioner.CursorPosition() : BelowTheSelectedLine();
      var menu = ViewModel.SelectedLineMenu();
      Dispatcher.UIThread.Post(() => PopupMenuWindow.ShowAt(menuTopLeft, menu, whenClosedWithoutAPick: PutTheKeyboardBackOnTheSelectedLine));
      e.Handled = true;
   }

   /// <summary>After a menu closed without a pick, as with Esc. Otherwise the keyboard would be left on the window's main menu.</summary>
   void PutTheKeyboardBackOnTheSelectedLine() => Dispatcher.UIThread.Post(FocusTheSelectedLine, DispatcherPriority.Background);

   void FocusTheSelectedLine()
   {
      if(TagTree.GetVisualDescendants().OfType<TreeViewItem>().FirstOrDefault(it => it.DataContext == ViewModel.SelectedLine) is {} selectedLine)
         selectedLine.Focus();
      else
         TagTree.Focus();
   }

   /// <summary>Just below the text of the selected line, or the tree's top left when no line is selected or it's scrolled out of sight.</summary>
   PixelPoint BelowTheSelectedLine() =>
      TagTree.GetVisualDescendants().OfType<StackPanel>().FirstOrDefault(it => it.Classes.Contains(LineContentClass) && it.DataContext == ViewModel.SelectedLine) is {} selectedLine
         ? selectedLine.PointToScreen(new Point(0, selectedLine.Bounds.Height))
         : TagTree.PointToScreen(new Point(0, 0));
}
