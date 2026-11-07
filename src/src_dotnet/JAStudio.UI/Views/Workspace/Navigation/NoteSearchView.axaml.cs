using System;
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
/// The keys on the tree: a letter or digit starts a searchable-text condition with it already typed, <c>+</c> opens the add menu,<br/>
/// <c>-</c> negates the selected line, Del removes it, Enter changes it, and Ctrl+Z undoes. The right-click menu, also on the menu key, has the rest.
/// </remarks>
public partial class NoteSearchView : UserControl
{
   /// <summary>
   /// The editor of a searchable-text condition started by typing, from when its dialog is asked for until the dialog has the keyboard.<br/>
   /// What's typed meanwhile is added to it, so a quickly typed word isn't split between the tree and the dialog.
   /// </summary>
   TextConditionEditorViewModel? _editorStartedByTyping;

   public NoteSearchView()
   {
      InitializeComponent();
      ConditionTree.AddHandler(KeyDownEvent, OnKeyDownInTheTree, RoutingStrategies.Tunnel);
      ConditionTree.AddHandler(TextInputEvent, OnTextInputInTheTree, RoutingStrategies.Tunnel);
      ConditionTree.ContextRequested += OnContextRequestedInTheTree;
      DataContextChanged += (_, _) =>
      {
         if(DataContext is NoteSearchViewModel viewModel)
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
      };
   }

   NoteSearchViewModel ViewModel => DataContext as NoteSearchViewModel ?? throw new InvalidOperationException("A NoteSearchView shows a NoteSearchViewModel.");

   Window OwnerWindow => TopLevel.GetTopLevel(this) as Window ?? throw new InvalidOperationException("The search is only shown in a window.");

   /// <summary>Puts the keyboard on the condition tree's selected line, ready for typing a search or a key that acts on the line.</summary>
   internal void FocusConditionTree() => FocusTheSelectedLine();

   /// <summary>The tree's lines are made anew on every change, so the keyboard is put back on the selected line.</summary>
   void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
   {
      if(e.PropertyName == nameof(NoteSearchViewModel.Nodes))
         Dispatcher.UIThread.Post(FocusTheSelectedLine, DispatcherPriority.Loaded);
   }

   void OnKeyDownInTheTree(object? sender, KeyEventArgs e)
   {
      switch(e.Key)
      {
         case Key.OemPlus or Key.Add:
            OpenTheAddMenu();
            break;
         case Key.OemMinus or Key.Subtract:
            ViewModel.ToggleNegationOfTheSelectedNode();
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
      if(e.Text is not { Length: > 0 } typed || !char.IsLetterOrDigit(typed[0])) return;

      e.Handled = true;
      if(_editorStartedByTyping is {} editor)
         editor.Text += typed;
      else
         ViewModel.AddAConditionThroughAnEditor(NoteProperties.SearchableText, EditInADialogStartedByTyping, typed);
   }

   void OnContextRequestedInTheTree(object? sender, ContextRequestedEventArgs e)
   {
      if((e.Source as Visual)?.FindAncestorOfType<TreeViewItem>(includeSelf: true) is { DataContext: SearchConditionNodeViewModel line })
         ViewModel.SelectedNode = line;

      var menuTopLeft = e.TryGetPosition(this, out _) ? WindowPositioner.CursorPosition() : BelowTheTree();
      PopupMenuWindow.ShowAt(menuTopLeft,
                             ViewModel.SelectedNodeMenu(openTheAddMenu: () => Dispatcher.UIThread.Post(OpenTheAddMenu),
                                                        changeTheSelectedCondition: () => Dispatcher.UIThread.Post(ChangeTheSelectedCondition)));
      e.Handled = true;
   }

   void OnAddButtonClicked(object? sender, RoutedEventArgs e)
   {
      if(sender is Control { DataContext: SearchConditionNodeViewModel group })
         ViewModel.SelectedNode = group;
      OpenTheAddMenu();
   }

   void OpenTheAddMenu() =>
      PopupMenuWindow.ShowAt(BelowTheTree(),
                             NoteSearchViewModel.AddConditionMenu(property => Dispatcher.UIThread.Post(() => ViewModel.AddAConditionThroughAnEditor(property, EditInADialog))));

   void ChangeTheSelectedCondition() => ViewModel.ChangeTheSelectedConditionThroughAnEditor(EditInADialog);

   Task<NoteCondition?> EditInADialog(ConditionEditorViewModel editor) => EditInADialog(editor, onceItHasTheKeyboard: () => {});

   async Task<NoteCondition?> EditInADialogStartedByTyping(ConditionEditorViewModel editor)
   {
      _editorStartedByTyping = editor as TextConditionEditorViewModel ?? throw new InvalidOperationException("Typing starts a condition on a text property.");
      try
      {
         return await EditInADialog(editor, onceItHasTheKeyboard: () => _editorStartedByTyping = null);
      }
      finally
      {
         _editorStartedByTyping = null;
      }
   }

   async Task<NoteCondition?> EditInADialog(ConditionEditorViewModel editor, Action onceItHasTheKeyboard)
   {
      var condition = await SearchConditionDialog.ShowAsync(OwnerWindow, editor, BelowTheTree(), onceItHasTheKeyboard);
      Dispatcher.UIThread.Post(FocusTheSelectedLine, DispatcherPriority.Loaded);
      return condition;
   }

   void FocusTheSelectedLine()
   {
      if(ConditionTree.GetVisualDescendants().OfType<TreeViewItem>().FirstOrDefault(it => it.DataContext == ViewModel.SelectedNode) is {} selectedLine)
         selectedLine.Focus();
      else
         ConditionTree.Focus();
   }

   /// <summary>Where menus and dialogs opened from the keyboard go: just below the condition tree, over the results.</summary>
   PixelPoint BelowTheTree() => ConditionTree.PointToScreen(new Point(0, ConditionTree.Bounds.Height));
}
