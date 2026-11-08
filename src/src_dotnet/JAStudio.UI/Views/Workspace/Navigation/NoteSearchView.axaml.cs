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
/// The right-click menu, also on the menu key, has the rest. F5, anywhere in the tab, refreshes the results.
/// </remarks>
/// <remarks>
/// <c>+</c>, <c>-</c> and the menu keys are recognized by the symbol the pressed key types in the current keyboard layout (<see cref="KeyEventArgs.KeySymbol"/>),<br/>
/// not by the key's code, which differs between layouts.<br/>
/// Menus open after the key or click that asked for them has been handled. That was done while Windows' keyboard input was broken (2026-11-07),<br/>
/// on a guess that a menu window opened in the middle of it could lose activation, and may not be needed.<br/>
/// When a menu closes without a pick, as with Esc, the keyboard goes back to the selected line. Otherwise it's left on the window's main menu.
/// </remarks>
public partial class NoteSearchView : UserControl
{
   public NoteSearchView()
   {
      InitializeComponent();
      ConditionTree.AddHandler(KeyDownEvent, OnKeyDownInTheTree, RoutingStrategies.Tunnel);
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

   /// <remarks>
   /// Handling the + and - keys here also keeps the tree from expanding or collapsing the line, as it does on the numpad's + and -.<br/>
   /// Shift is allowed with a symbol, since some layouts need it for +.
   /// </remarks>
   void OnKeyDownInTheTree(object? sender, KeyEventArgs e)
   {
      switch(e)
      {
         case { Key: Key.Delete }:
            ViewModel.RemoveTheSelectedNode();
            break;
         case { Key: Key.Enter }:
            ChangeTheSelectedCondition();
            break;
         case { Key: Key.Z, KeyModifiers: KeyModifiers.Control }:
            ViewModel.Undo();
            break;
         case { Key: Key.OemPlus or Key.Add, KeyModifiers: KeyModifiers.Control }
           or { KeyModifiers: KeyModifiers.None or KeyModifiers.Shift, KeySymbol: "+" }:
            Dispatcher.UIThread.Post(ShowTheAddMenu);
            break;
         case { KeyModifiers: KeyModifiers.None or KeyModifiers.Shift, KeySymbol: "-" }:
            ViewModel.ToggleNegationOfTheSelectedNode();
            break;
         case { KeyModifiers: KeyModifiers.None or KeyModifiers.Shift, KeySymbol: { Length: 1 } symbol }
            when NoteSearchViewModel.ItemPickedBy(symbol[0], AddConditionMenu()) is {} menuItem:
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

      var menuTopLeft = e.TryGetPosition(this, out var pointer) ? this.PointToScreen(pointer) : BelowTheTree();
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
