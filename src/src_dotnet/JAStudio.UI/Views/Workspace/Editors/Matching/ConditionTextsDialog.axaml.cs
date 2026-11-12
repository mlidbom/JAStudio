using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;

namespace JAStudio.UI.Views.Workspace.Editors.Matching;

/// <summary>
/// The small modal dialog the tree editor edits a condition's texts in, such as the endings of <c>EndsWithOneOf</c>, for a new<br/>
/// condition or one being changed. It opens with the keyboard in the text box. Enter, or OK, closes it with the condition, Esc or Cancel without one.
/// </summary>
partial class ConditionTextsDialog : Window
{
   [Obsolete("For the XAML designer/previewer only")]
   public ConditionTextsDialog() => InitializeComponent();

   ConditionTextsDialog(ConditionTextsEditorViewModel editor)
   {
      editor.AcceptCommand = new RelayCommand(() => CloseWithTheConditionOf(editor));
      editor.CancelCommand = new RelayCommand(() => Close(null));
      DataContext = editor;
      InitializeComponent();
      Opened += (_, _) => Dispatcher.UIThread.Post(() => DialogInitialFocus.PutTheKeyboardInTheMainInputOf(this), DispatcherPriority.Loaded);
   }

   /// <summary>
   /// Shows <paramref name="editor"/> in a modal dialog over <paramref name="owner"/>, with its top left corner at <paramref name="topLeft"/>,<br/>
   /// and returns the condition it closed with, or null when it was cancelled.
   /// </summary>
   internal static Task<Condition?> ShowAsync(Window owner, ConditionTextsEditorViewModel editor, PixelPoint topLeft) =>
      new ConditionTextsDialog(editor) { Position = topLeft }.ShowDialog<Condition?>(owner);

   void CloseWithTheConditionOf(ConditionTextsEditorViewModel editor)
   {
      if(editor.Condition is {} condition)
         Close(condition);
   }
}
