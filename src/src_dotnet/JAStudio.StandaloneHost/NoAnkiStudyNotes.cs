using System;
using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.StandaloneHost;

/// <summary>
/// The study-notes in Anki, as the standalone JAStudio has them: none, since it has no Anki. So no note has a study-note, nor its cards' studying<br/>
/// statuses, nor Anki tags: every note shows as not studied. Suspending and unsuspending are offered only for a note with active or suspended cards.
/// </summary>
sealed class NoAnkiStudyNotes : ISRSStudyNotes
{
   public IReadOnlyList<StudyNoteInTheSRS> ReadAll() => [];

   public void Write(JPNote note) => throw new InvalidOperationException($"The standalone JAStudio has no Anki, so {note.GetQuestion()} has no study-note to write.");

   public void SuspendAllCardsForNote(NoteId noteId) => throw NoCards(noteId);
   public void UnsuspendAllCardsForNote(NoteId noteId) => throw NoCards(noteId);

   /// <summary>Does nothing: without Anki, the note has no study-note to delete.</summary>
   public void DeleteTheStudyNoteOf(NoteId noteId) {}

   static InvalidOperationException NoCards(NoteId noteId) => new($"The standalone JAStudio has no Anki, so the note {noteId} has no cards to change.");
}
