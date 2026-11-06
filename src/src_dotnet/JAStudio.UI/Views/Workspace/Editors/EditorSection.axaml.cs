using Avalonia.Controls;
using JAStudio.UI.ViewModels.Workspace;
using JAStudio.UI.ViewModels.Workspace.Editors;

namespace JAStudio.UI.Views.Workspace.Editors;

/// <summary>
/// The workspace's editor section, on the right of its tabs: the editors of the shown tab's note,<br/>
/// from that tab's <see cref="NoteTabViewModel.EditorSection"/>, such as a <see cref="VocabEditorSection"/>.<br/>
/// It also holds the look of the editors' rows and sections, which every note type's editors share.
/// </summary>
public partial class EditorSection : UserControl
{
   public EditorSection() => InitializeComponent();
}
