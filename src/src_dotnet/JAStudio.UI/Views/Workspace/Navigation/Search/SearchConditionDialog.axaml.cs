using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Search;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Navigation.Search;

namespace JAStudio.UI.Views.Workspace.Navigation.Search;

/// <summary>
/// The small modal dialog the search editor edits a condition in, new or existing, with the editor for its kind of property inside.<br/>
/// It opens with the keyboard in the editor's main input, the one marked with the <c>initialFocus</c> class.<br/>
/// Enter, or OK, closes it with the condition, Esc or Cancel closes it without one, and so does double-clicking a choice in a list.
/// </summary>
partial class SearchConditionDialog : Window
{
   const string InitialFocusClass = "initialFocus";

   readonly ConditionEditorViewModel _editor;

#pragma warning disable CS8618 // For the XAML designer, which needs no editor.
   [Obsolete("For the XAML designer/previewer only")]
   public SearchConditionDialog() => InitializeComponent();
#pragma warning restore CS8618

   SearchConditionDialog(ConditionEditorViewModel editor)
   {
      _editor = editor;
      editor.AcceptCommand = new RelayCommand(Accept);
      editor.CancelCommand = new RelayCommand(() => Close(null));
      DataContext = editor;
      InitializeComponent();
      Opened += (_, _) => Dispatcher.UIThread.Post(FocusTheInitialInput, DispatcherPriority.Loaded);
      AddHandler(DoubleTappedEvent, OnDoubleTapped);
   }

   /// <summary>
   /// Shows <paramref name="editor"/> in a modal dialog over <paramref name="owner"/>, with its top left corner at <paramref name="topLeft"/>,<br/>
   /// and returns the condition it closed with, or null when it was cancelled.
   /// </summary>
   internal static Task<NoteCondition?> ShowAsync(Window owner, ConditionEditorViewModel editor, PixelPoint topLeft) =>
      new SearchConditionDialog(editor) { Position = topLeft }.ShowDialog<NoteCondition?>(owner);

   void Accept()
   {
      if(_editor.Condition is {} condition)
         Close(condition);
   }

   void OnDoubleTapped(object? sender, TappedEventArgs e)
   {
      if((e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true) is not null)
         Accept();
   }

   /// <summary>Puts the keyboard in the editor's main input, at the end of any text already in it, as when changing a condition.</summary>
   /// <remarks>
   /// The dialog first takes the Windows keyboard focus for its own native window. Focusing the input only moves Avalonia's focus,<br/>
   /// and if the workspace behind had the Windows keyboard focus, the keys would keep going there.
   /// </remarks>
   void FocusTheInitialInput()
   {
      NativeKeyboardFocus.MoveToWindowItself(this);
      var input = this.GetVisualDescendants().OfType<InputElement>().FirstOrDefault(it => it.Classes.Contains(InitialFocusClass))
               ?? throw new InvalidOperationException($"The editor {_editor.GetType().Name} marks no input with the {InitialFocusClass} class.");
      input.Focus();
      if(input is TextBox textBox)
         textBox.CaretIndex = textBox.Text?.Length ?? 0;
   }
}
