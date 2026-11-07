using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using JAStudio.Core.Note.Search;
using JAStudio.UI.Menus;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.ViewModels.Workspace.Navigation.Search;
using JAStudio.UI.Views.Workspace.Navigation.Search;

namespace JAStudio.UI.Views.Workspace.Navigation;

/// <summary>
/// The view of a <see cref="NoteSearchViewModel"/>: the "Search" tab of the <see cref="NavigationPane"/>, the search's condition tree and the notes found.<br/>
/// It turns keys, clicks and menus into the view model's operations, and shows a condition's editor in a <see cref="SearchConditionDialog"/>.
/// </summary>
/// <remarks>
/// The keys on the tree: <c>+</c> or Ctrl++ opens the add menu, and a key from the add menu does what it does there, so <c>t</c> opens the tag dialog.<br/>
/// <c>-</c> negates the selected line, Del removes it, Enter or a double-click changes it, and Ctrl+Z undoes.<br/>
/// The right-click menu, also on the menu key, has the rest.
/// </remarks>
/// <remarks>
/// <c>+</c>, <c>-</c> and the menu keys are recognized as typed characters, not as keys, because which key types them depends on the keyboard layout.<br/>
/// Menus open after the key or click that asked for them has been handled, since a menu window opened in the middle of it can lose activation at once, and close.<br/>
/// When a menu closes without a pick, as with Esc, the keyboard goes back to the selected line. Otherwise it's left on the window's main menu.
/// </remarks>
public partial class NoteSearchView : UserControl
{
   public NoteSearchView()
   {
      InitializeComponent();
      ConditionTree.AddHandler(KeyDownEvent, OnKeyDownInTheTree, RoutingStrategies.Tunnel);
      ConditionTree.AddHandler(TextInputEvent, OnTextInputInTheTree, RoutingStrategies.Tunnel);
      ConditionTree.AddHandler(DoubleTappedEvent, OnDoubleTappedInTheTree, RoutingStrategies.Bubble, handledEventsToo: true);
      ConditionTree.ContextRequested += OnContextRequestedInTheTree;
      DataContextChanged += (_, _) =>
      {
         if(DataContext is NoteSearchViewModel viewModel)
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
      };
   }

   NoteSearchViewModel ViewModel => DataContext as NoteSearchViewModel ?? throw new InvalidOperationException("A NoteSearchView shows a NoteSearchViewModel.");

   Window OwnerWindow => TopLevel.GetTopLevel(this) as Window ?? throw new InvalidOperationException("The search is only shown in a window.");

   /// <summary>Puts the keyboard on the condition tree's selected line, ready for a key that acts on the line or adds a condition.</summary>
   internal void FocusConditionTree() => FocusTheSelectedLine();

   /// <summary>The tree's lines are made anew on every change, so the keyboard is put back on the selected line.</summary>
   void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
   {
      if(e.PropertyName == nameof(NoteSearchViewModel.Nodes))
         Dispatcher.UIThread.Post(FocusTheSelectedLine, DispatcherPriority.Loaded);
   }

   /// <remarks>The numpad's + and - are marked handled, so the tree doesn't also expand or collapse the line. The characters they type act.</remarks>
   void OnKeyDownInTheTree(object? sender, KeyEventArgs e)
   {
      switch(e.Key)
      {
         case Key.OemPlus or Key.Add when e.KeyModifiers == KeyModifiers.Control:
            Dispatcher.UIThread.Post(ShowTheAddMenu);
            break;
         case Key.Add or Key.Subtract:
            break;
         case Key.Delete:
            ViewModel.RemoveTheSelectedNode();
            break;
         case Key.Enter:
            ChangeTheSelectedCondition();
            break;
         case Key.Z when e.KeyModifiers == KeyModifiers.Control:
            ViewModel.Undo();
            break;
         default:
            return;
      }

      e.Handled = true;
   }

   void OnTextInputInTheTree(object? sender, TextInputEventArgs e)
   {
      switch(e.Text)
      {
         case "+":
            Dispatcher.UIThread.Post(ShowTheAddMenu);
            break;
         case "-":
            ViewModel.ToggleNegationOfTheSelectedNode();
            break;
         case { Length: 1 } typed when NoteSearchViewModel.ItemPickedBy(typed[0], AddConditionMenu()) is {} menuItem:
            Dispatcher.UIThread.Post(() => DoWhatPickingItDoes(menuItem));
            break;
         default:
            return;
      }

      e.Handled = true;
   }

   /// <summary>Double-clicking a condition changes it. On a group, the tree's own double-click expands or collapses it.</summary>
   void OnDoubleTappedInTheTree(object? sender, TappedEventArgs e)
   {
      if((e.Source as Visual)?.FindAncestorOfType<TreeViewItem>(includeSelf: true) is not { DataContext: SearchConditionNodeViewModel { IsGroup: false } condition }) return;

      e.Handled = true;
      ViewModel.SelectedNode = condition;
      ChangeTheSelectedCondition();
   }

   void OnContextRequestedInTheTree(object? sender, ContextRequestedEventArgs e)
   {
      if((e.Source as Visual)?.FindAncestorOfType<TreeViewItem>(includeSelf: true) is { DataContext: SearchConditionNodeViewModel line })
         ViewModel.SelectedNode = line;

      var menuTopLeft = e.TryGetPosition(this, out _) ? WindowPositioner.CursorPosition() : BelowTheTree();
      var menu = ViewModel.SelectedNodeMenu(openTheAddMenu: () => Dispatcher.UIThread.Post(ShowTheAddMenu),
                                            changeTheSelectedCondition: () => Dispatcher.UIThread.Post(ChangeTheSelectedCondition));
      Dispatcher.UIThread.Post(() => ShowAMenu(menu, menuTopLeft));
      e.Handled = true;
   }

   void OnAddButtonClicked(object? sender, RoutedEventArgs e)
   {
      if(sender is Control { DataContext: SearchConditionNodeViewModel group })
         ViewModel.SelectedNode = group;
      Dispatcher.UIThread.Post(ShowTheAddMenu);
   }

   /// <summary>The add menu, made anew each time. Its items keep their names, which is how the item whose key was typed is found in a newly made one.</summary>
   List<SpecMenuItem> AddConditionMenu() => ViewModel.AddConditionMenu(AddAConditionThrough);

   void ShowTheAddMenu() => ShowAMenu(AddConditionMenu(), BelowTheTree());

   void AddAConditionThrough(ConditionEditorViewModel editor) => Dispatcher.UIThread.Post(() => ViewModel.AddAConditionThroughAnEditor(editor, EditInADialog));

   /// <summary>
   /// What picking <paramref name="menuItem"/> in the add menu does: run it, or show the add menu with its submenu open,<br/>
   /// so that it's clear what's happening, and another item is one key away.
   /// </summary>
   void DoWhatPickingItDoes(SpecMenuItem menuItem)
   {
      if(menuItem.Kind == SpecMenuItemKind.Submenu)
         PopupMenuWindow.ShowWithSubmenuOpen(BelowTheTree(), AddConditionMenu(), menuItem, whenClosedWithoutAPick: PutTheKeyboardBackOnTheSelectedLine);
      else
         menuItem.Action();
   }

   void ShowAMenu(IReadOnlyList<SpecMenuItem> menu, PixelPoint menuTopLeft) =>
      PopupMenuWindow.ShowAt(menuTopLeft, menu, whenClosedWithoutAPick: PutTheKeyboardBackOnTheSelectedLine);

   /// <summary>After a menu closed without a pick, as with Esc. Otherwise the keyboard would be left on the window's main menu.</summary>
   void PutTheKeyboardBackOnTheSelectedLine() => Dispatcher.UIThread.Post(FocusTheSelectedLine, DispatcherPriority.Background);

   void ChangeTheSelectedCondition() => ViewModel.ChangeTheSelectedConditionThroughAnEditor(EditInADialog);

   async Task<NoteCondition?> EditInADialog(ConditionEditorViewModel editor)
   {
      var condition = await SearchConditionDialog.ShowAsync(OwnerWindow, editor, BelowTheTree());
      Dispatcher.UIThread.Post(FocusTheSelectedLine, DispatcherPriority.Loaded);
      return condition;
   }

   /// <remarks>
   /// It's always posted, so a condition's dialog may have opened meanwhile. Then it does nothing:<br/>
   /// focusing the line would take the keyboard from the dialog, and the dialog puts it back on the line when it closes.
   /// </remarks>
   void FocusTheSelectedLine()
   {
      if(OwnerWindow.HasAWindowOpenOverIt()) return;

      if(ConditionTree.GetVisualDescendants().OfType<TreeViewItem>().FirstOrDefault(it => it.DataContext == ViewModel.SelectedNode) is {} selectedLine)
         selectedLine.Focus();
      else
         ConditionTree.Focus();
   }

   /// <summary>Where menus and dialogs opened from the keyboard go: just below the condition tree, over the results.</summary>
   PixelPoint BelowTheTree() => ConditionTree.PointToScreen(new Point(0, ConditionTree.Bounds.Height));
}
