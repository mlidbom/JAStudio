using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.UI.Menus;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;

namespace JAStudio.UI.Views.Workspace.Editors.Matching;

/// <summary>
/// The view of a <see cref="ConditionTreeEditorViewModel"/>: a vocab's match-condition as a tree, on its Matching tab, or a shared-condition's definition,<br/>
/// in its <see cref="SharedConditionWindow"/>. It turns keys, clicks and menus into the view model's operations, and shows a condition's texts in a<br/>
/// <see cref="ConditionTextsDialog"/>.
/// </summary>
/// <remarks>
/// The keys on the tree are the note search's: <c>+</c> or Ctrl++ opens the add menu, and a key from the add menu does what it does there, so <c>p</c><br/>
/// opens the add menu with the preceding token's submenu open. <c>-</c> negates the selected line, Del removes it, Enter or a double-click changes<br/>
/// its texts, and Ctrl+Z undoes. F12 or Ctrl+click on a shared-condition opens its definition, in its window, as an IDE goes to a definition. The<br/>
/// right-click menu, also on the menu key, has the rest.
/// </remarks>
public partial class ConditionTreeEditor : UserControl
{
   bool _theKeyboardGoesBackToTheSelectedLineWhenTheTreeIsEnabled;

   public ConditionTreeEditor()
   {
      //refactor: the keys, menus and focus handling here are NoteSearchView's, for another tree of conditions. A tree editor control both views use
      //   could hold them once.
      InitializeComponent();
      ConditionTree.PropertyChanged += OnTreePropertyChanged;
      ConditionTree.AddHandler(KeyDownEvent, OnKeyDownInTheTree, RoutingStrategies.Tunnel);
      ConditionTree.AddHandler(DoubleTappedEvent, OnDoubleTappedInTheTree, RoutingStrategies.Bubble, handledEventsToo: true);
      ConditionTree.AddHandler(TappedEvent, OnTappedInTheTree, RoutingStrategies.Bubble, handledEventsToo: true);
      ConditionTree.ContextRequested += OnContextRequestedInTheTree;
      DataContextChanged += (_, _) =>
      {
         if(DataContext is ConditionTreeEditorViewModel viewModel)
            viewModel.ChangedByTheTreeEditor += PutTheKeyboardOnTheSelectedLine;
      };
   }

   ConditionTreeEditorViewModel ViewModel => DataContext as ConditionTreeEditorViewModel ?? throw new InvalidOperationException("A MatchConditionEditor shows a ConditionTreeEditorViewModel.");

   Window OwnerWindow => TopLevel.GetTopLevel(this) as Window ?? throw new InvalidOperationException("The match-condition editor is only shown in a window.");

   /// <summary>The tree's lines are made anew on every change, so after one made in the tree, the keyboard is put on the selected line.</summary>
   void PutTheKeyboardOnTheSelectedLine() => Dispatcher.UIThread.Post(FocusTheSelectedLine, DispatcherPriority.Loaded);

   /// <summary>
   /// Every change reparses the vocab's sentences, and the tab is disabled meanwhile, which takes the keyboard off the tree. Once it's enabled again,<br/>
   /// the keyboard goes back to the selected line, so that the next key acts on it.
   /// </summary>
   void OnTreePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
   {
      if(!TheTreeWasEnabledWhileTheKeyboardWaitedToGoBackToTheSelectedLine()) return;

      _theKeyboardGoesBackToTheSelectedLineWhenTheTreeIsEnabled = false;
      Dispatcher.UIThread.Post(FocusTheSelectedLine, DispatcherPriority.Loaded);

      bool TheTreeWasEnabledWhileTheKeyboardWaitedToGoBackToTheSelectedLine() =>
         e.Property == IsEffectivelyEnabledProperty && ConditionTree.IsEffectivelyEnabled && _theKeyboardGoesBackToTheSelectedLineWhenTheTreeIsEnabled;
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
            ViewModel.RemoveTheSelectedLine();
            break;
         case { Key: Key.Enter }:
            ChangeTheSelectedCondition();
            break;
         case { Key: Key.Z, KeyModifiers: KeyModifiers.Control }:
            ViewModel.Undo();
            break;
         case { Key: Key.F12, KeyModifiers: KeyModifiers.None }:
            ViewModel.OpenTheDefinitionOfTheSelectedLine();
            break;
         case { Key: Key.OemPlus or Key.Add, KeyModifiers: KeyModifiers.Control }
           or { KeyModifiers: KeyModifiers.None or KeyModifiers.Shift, KeySymbol: "+" }:
            Dispatcher.UIThread.Post(ShowTheAddMenu);
            break;
         case { KeyModifiers: KeyModifiers.None or KeyModifiers.Shift, KeySymbol: "-" }:
            ViewModel.ToggleNegationOfTheSelectedLine();
            break;
         case { KeyModifiers: KeyModifiers.None or KeyModifiers.Shift, KeySymbol: { Length: 1 } symbol }
            when SpecMenuItem.ItemPickedBy(symbol[0], AddConditionMenu()) is {} menuItem:
            Dispatcher.UIThread.Post(() => DoWhatPickingItDoes(menuItem));
            break;
         default:
            return;
      }

      e.Handled = true;
   }

   /// <summary>Double-clicking a condition changes its texts. On a group, the tree's own double-click expands or collapses it.</summary>
   void OnDoubleTappedInTheTree(object? sender, TappedEventArgs e)
   {
      if(TheLineTheEventIsOn(e) is not { IsGroup: false } line) return;

      e.Handled = true;
      ViewModel.SelectedLine = line;
      ChangeTheSelectedCondition();
   }

   /// <summary>Ctrl+click on a shared-condition opens its definition, as in an IDE.</summary>
   void OnTappedInTheTree(object? sender, TappedEventArgs e)
   {
      if(!e.KeyModifiers.HasFlag(KeyModifiers.Control) || TheLineTheEventIsOn(e) is not { IsASharedCondition: true } line) return;

      e.Handled = true;
      ViewModel.SelectedLine = line;
      ViewModel.OpenTheDefinitionOfTheSelectedLine();
   }

   void OnContextRequestedInTheTree(object? sender, ContextRequestedEventArgs e)
   {
      if(TheLineTheEventIsOn(e) is {} line)
         ViewModel.SelectedLine = line;

      var menuTopLeft = e.TryGetPosition(this, out var pointer) ? this.PointToScreen(pointer) : BelowTheSelectedLine();
      var menu = ViewModel.SelectedLineMenu(openTheAddMenu: () => Dispatcher.UIThread.Post(ShowTheAddMenu),
                                            changeTheSelectedCondition: () => Dispatcher.UIThread.Post(ChangeTheSelectedCondition));
      Dispatcher.UIThread.Post(() => ShowAMenu(menu, menuTopLeft));
      e.Handled = true;
   }

   void OnAddButtonClicked(object? sender, RoutedEventArgs e)
   {
      if(sender is Control { DataContext: ConditionTreeLineViewModel group })
         ViewModel.SelectedLine = group;
      Dispatcher.UIThread.Post(ShowTheAddMenu);
   }

   /// <summary>The add menu, made anew each time. Its items keep their names, which is how the item whose key was typed is found in a newly made one.</summary>
   List<SpecMenuItem> AddConditionMenu() => ViewModel.AddConditionMenu(AddAConditionThrough);

   void ShowTheAddMenu() => ShowAMenu(AddConditionMenu(), BelowTheSelectedLine());

   void AddAConditionThrough(ConditionTextsEditorViewModel editor) => Dispatcher.UIThread.Post(() => ViewModel.AddAConditionThroughAnEditor(editor, EditInADialog));

   /// <summary>
   /// What picking <paramref name="menuItem"/> in the add menu does: run it, or show the add menu with its submenu open,<br/>
   /// so that it's clear what's happening, and another item is one key away.
   /// </summary>
   void DoWhatPickingItDoes(SpecMenuItem menuItem)
   {
      if(menuItem.Kind == SpecMenuItemKind.Submenu)
         PopupMenuWindow.ShowWithSubmenuOpen(BelowTheSelectedLine(), AddConditionMenu(), menuItem, whenClosedWithoutAPick: PutTheKeyboardBackOnTheSelectedLine);
      else
         menuItem.Action();
   }

   void ShowAMenu(IReadOnlyList<SpecMenuItem> menu, PixelPoint menuTopLeft) =>
      PopupMenuWindow.ShowAt(menuTopLeft, menu, whenClosedWithoutAPick: PutTheKeyboardBackOnTheSelectedLine);

   /// <summary>After a menu closed without a pick, as with Esc. Otherwise the keyboard would be left on the window's main menu.</summary>
   void PutTheKeyboardBackOnTheSelectedLine() => Dispatcher.UIThread.Post(FocusTheSelectedLine, DispatcherPriority.Background);

   void ChangeTheSelectedCondition() => ViewModel.ChangeTheSelectedConditionThroughAnEditor(EditInADialog);

   async Task<Condition?> EditInADialog(ConditionTextsEditorViewModel editor)
   {
      var condition = await ConditionTextsDialog.ShowAsync(OwnerWindow, editor, BelowTheSelectedLine());
      Dispatcher.UIThread.Post(FocusTheSelectedLine, DispatcherPriority.Loaded);
      return condition;
   }

   /// <remarks>
   /// It's always posted, so a condition's dialog may have opened meanwhile. Then it does nothing:<br/>
   /// focusing the line would take the keyboard from the dialog, and the dialog puts it back on the line when it closes.<br/>
   /// While the tab is disabled, as while the vocab's sentences are reparsed, it's done when the tab is enabled again.
   /// </remarks>
   void FocusTheSelectedLine()
   {
      if(OwnerWindow.HasAWindowOpenOverIt()) return;

      if(!ConditionTree.IsEffectivelyEnabled)
      {
         _theKeyboardGoesBackToTheSelectedLineWhenTheTreeIsEnabled = true;
         return;
      }

      if(SelectedLineItem() is {} selectedLine)
         selectedLine.Focus();
      else
         ConditionTree.Focus();
   }

   /// <summary>The line whose item in the tree <paramref name="e"/> came from, such as the line clicked, or null when it came from no line's item.</summary>
   static ConditionTreeLineViewModel? TheLineTheEventIsOn(RoutedEventArgs e) =>
      (e.Source as Visual)?.FindAncestorOfType<TreeViewItem>(includeSelf: true)?.DataContext as ConditionTreeLineViewModel;

   TreeViewItem? SelectedLineItem() => ConditionTree.GetVisualDescendants().OfType<TreeViewItem>().FirstOrDefault(it => it.DataContext == ViewModel.SelectedLine);

   /// <summary>Where menus and dialogs opened from the keyboard go: just below the selected line, with the lines of a group under it, or below the tree.</summary>
   PixelPoint BelowTheSelectedLine()
   {
      Visual below = SelectedLineItem() is {} selectedLine ? selectedLine : ConditionTree;
      return below.PointToScreen(new Point(0, below.Bounds.Height));
   }
}
