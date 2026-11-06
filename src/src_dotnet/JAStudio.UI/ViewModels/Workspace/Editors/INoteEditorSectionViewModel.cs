namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// What the workspace's editor section shows for a note: the note's editors.<br/>
/// Each note type that can be edited in the workspace has its own, such as <see cref="VocabEditorSectionViewModel"/> and <see cref="KanjiEditorSectionViewModel"/>.
/// </summary>
/// <remarks>
/// One holds its note's id, not the note object, because the collection replaces a note's object when Anki edits the note<br/>
/// and when it reloads, and an editor holding the old object would edit an orphan.
/// </remarks>
interface INoteEditorSectionViewModel
{
   /// <summary>Shows everything as the note has it now. Called by the tab showing the note whenever the note is updated.</summary>
   void Refresh();
}
