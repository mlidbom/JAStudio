using System;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.UI.Web;

namespace JAStudio.UI.Views.Workspace;

/// <summary>
/// Opens the note behind a note link clicked on a card page: in JAStudio's <see cref="WorkspaceWindow"/>, or in Anki's previewer through Anki's<br/>
/// windows (<see cref="Core.Anki.IAnkiWindows"/>). A note has an Anki note only while there's an Anki, so without Anki the previewer is never asked.
/// </summary>
public class NoteLinkOpener : INoteLinkOpener
{
   readonly TemporaryServiceCollection _services;

   public NoteLinkOpener(TemporaryServiceCollection services) => _services = services;

   public void OpenInWorkspace(NoteId noteId) =>
      WorkspaceWindow.OpenNote(_services.CoreApp.Collection.NoteFromNoteId(noteId)
                            ?? throw new ArgumentException($"No note has the id {noteId}.", nameof(noteId)));

   public void OpenInAnkiPreviewer(NoteId noteId)
   {
      var externalId = _services.ExternalNoteIdMap.ToExternalId(noteId);
      if(externalId.HasValue)
         _services.AnkiWindows.OpenThePreviewerOn($"{Builtin.NoteId}:{externalId.Value}");
   }
}
