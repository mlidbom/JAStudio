using System;

// ReSharper disable UnusedMember.Global used from python
namespace JAStudio.Core.Note.Collection;

/// <summary>
/// The notes of one note type, as the Anki add-on's Python follows them: it tells them of a study-note deleted in Anki, and writes the study-note<br/>
/// of each note that is updated.
/// </summary>
public interface IStudyNoteUpdateHandler
{
   //review-naming:studying: IStudyNoteUpdateHandler was IExternalNoteUpdateHandler, StudyNoteRemoved was ExternalNoteRemoved, and GetStudyNoteId
   //   was GetExternalNoteId (Claude, 2026-11-15): renamed only so that "external note" is gone everywhere.
   //wip: this interface goes with Anki's notifications, in increment 2 of dev_docs/moving-jastudio-out-of-anki.md: a Sync button replaces them, and
   //   the study-notes are then written at a sync only.
   /// <summary>Called when the study-note with the id <paramref name="studyNoteId"/> was deleted in Anki: its note, if this handler has it, forgets it.</summary>
   void StudyNoteRemoved(long studyNoteId);

   /// <summary>Notifies every updated note of this handler's note type, on the thread that updated it.</summary>
   IObservable<JPNote> NoteUpdated { get; }

   /// <summary>The id of the study-note of the note with the id <paramref name="noteId"/>, or 0 when it has none.</summary>
   long GetStudyNoteId(NoteId noteId);
}
