using System.Collections.Generic;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Note;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Anki;

/// <summary>
/// The study-notes in the collection Anki has open (<see cref="ISRSStudyNotes"/>): read from its database (<see cref="AnkiCollectionDatabase"/>),<br/>
/// and changed through Python (<see cref="AnkiFacade"/>). A note's study-note is its Anki note, found by its id (<see cref="StudyNoteIds"/>).
/// </summary>
public class AnkiStudyNotes : ISRSStudyNotes
{
   readonly AnkiCollectionDatabase _database;
   readonly StudyNoteIds _studyNoteIds;

   public AnkiStudyNotes(AnkiCollectionDatabase database, StudyNoteIds studyNoteIds)
   {
      _database = database;
      _studyNoteIds = studyNoteIds;
   }

   public IReadOnlyList<StudyNoteInTheSRS> ReadAll() => StudyNotesInAnkisDatabase.ReadAll(_database);

   public void Write(JPNote note) => AnkiFacade.StudyNotes.Write(_studyNoteIds.RequiredStudyNoteIdOf(note.GetId()), note);

   public void SuspendAllCardsForNote(NoteId noteId) => AnkiFacade.NoteEx.SuspendAllCardsForNote(_studyNoteIds.RequiredStudyNoteIdOf(noteId));

   public void UnsuspendAllCardsForNote(NoteId noteId) => AnkiFacade.NoteEx.UnsuspendAllCardsForNote(_studyNoteIds.RequiredStudyNoteIdOf(noteId));

   /// <summary>
   /// Removes the note's Anki note, if it has one, then resets the search of Anki's browser, which would otherwise fail when it refreshes the note<br/>
   /// that no longer exists.
   /// </summary>
   public void DeleteTheStudyNoteOf(NoteId noteId)
   {
      var studyNoteId = _studyNoteIds.StudyNoteIdOf(noteId);
      if(!studyNoteId.HasValue) return;

      AnkiFacade.NoteEx.RemoveNote(studyNoteId.Value);
      AnkiFacade.Browser.ExecuteLookup("");
   }
}
