using System;
using JAStudio.Core;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using JAStudio.UI.Views;
using JAStudio.Web;

namespace JAStudio.Anki.PythonInterop;

/// <summary>
/// Opens the note behind a clicked note link for the Anki add-on:<br/>
/// in JAStudio's <see cref="NoteViewerWindow"/>, or in Anki's previewer.
/// </summary>
class NoteLinkOpener : INoteLinkOpener
{
   readonly TemporaryServiceCollection _services;

   public NoteLinkOpener(TemporaryServiceCollection services) => _services = services;

   public void OpenInNoteViewer(NoteId noteId) =>
      NoteViewerWindow.OpenNote(_services.CoreApp.Collection.NoteFromNoteId(noteId)
                             ?? throw new ArgumentException($"No note has the id {noteId}.", nameof(noteId)),
                                _services);

   public void OpenInAnkiPreviewer(NoteId noteId)
   {
      var externalId = _services.ExternalNoteIdMap.ToExternalId(noteId);
      if(externalId.HasValue)
         AnkiFacade.Browser.ExecuteLookupAndShowPreviewer($"{AnkiFieldNames.NoteId}:{externalId.Value}");
   }
}
