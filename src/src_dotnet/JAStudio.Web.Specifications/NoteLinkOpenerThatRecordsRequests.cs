using System.Collections.Generic;
using JAStudio.Core.Note;

namespace JAStudio.Web.Specifications;

/// <summary>
/// An <see cref="INoteLinkOpener"/> for specifications.<br/>
/// The real one opens windows in the Anki add-on; this one opens nothing and records what it was asked to open.
/// </summary>
class NoteLinkOpenerThatRecordsRequests : INoteLinkOpener
{
   public List<NoteId> NotesOpenedInNoteViewer { get; } = [];
   public List<NoteId> NotesOpenedInAnkiPreviewer { get; } = [];

   public void OpenInNoteViewer(NoteId noteId) => NotesOpenedInNoteViewer.Add(noteId);
   public void OpenInAnkiPreviewer(NoteId noteId) => NotesOpenedInAnkiPreviewer.Add(noteId);
}
