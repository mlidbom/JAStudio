using System;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.UI.Views.Workspace;
using JAStudio.Web;

namespace JAStudio.HeadlessHost;

/// <summary>
/// Opens the note behind a note link clicked on a page of the headless JAStudio's card server: in its workspace, as JAStudio inside Anki does.<br/>
/// Anki's previewer it refuses: there is no Anki.
/// </summary>
class HeadlessNoteLinkOpener : INoteLinkOpener
{
   readonly TemporaryServiceCollection _services;

   public HeadlessNoteLinkOpener(TemporaryServiceCollection services) => _services = services;

   public void OpenInWorkspace(NoteId noteId) =>
      WorkspaceWindow.OpenNote(_services.CoreApp.Collection.NoteFromNoteId(noteId) ?? throw new ArgumentException($"No note has the id {noteId}.", nameof(noteId)));

   public void OpenInAnkiPreviewer(NoteId noteId) => throw new NotSupportedException("The headless JAStudio has no Anki previewer to open a note in.");
}
