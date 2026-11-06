using Avalonia.Controls;
using Avalonia.Input;
using JAStudio.UI.Controls;
using JAStudio.UI.ViewModels.Workspace;

namespace JAStudio.UI.Views.Workspace.Editors;

/// <summary>
/// The workspace's editor section, on the right of its tabs: the editors of the shown tab's note,<br/>
/// from that tab's <see cref="NoteTabViewModel.EditorSection"/>, such as a <see cref="VocabEditorSection"/>.<br/>
/// It also holds the look of the editors' rows and sections, which every note type's editors share.
/// </summary>
public partial class EditorSection : UserControl
{
   public EditorSection()
   {
      InitializeComponent();
      AddHandler(LostFocusEvent, CommitWhenTheKeyboardLeavesACommittingTextBox);
   }

   /// <summary>
   /// Commits what's typed in the text box of a <see cref="CommittingTextFieldEditorViewModel"/> when the keyboard leaves it.<br/>
   /// Enter commits through the text box's key binding, but there's no command to bind leaving the box to,<br/>
   /// so the section catches it as it bubbles up from any of its text boxes.
   /// </summary>
   static void CommitWhenTheKeyboardLeavesACommittingTextBox(object? sender, FocusChangedEventArgs e)
   {
      if(e.Source is TextBox { DataContext: CommittingTextFieldEditorViewModel field }) field.Commit();
   }
}
