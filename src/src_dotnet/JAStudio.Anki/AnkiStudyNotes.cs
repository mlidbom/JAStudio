using System.Collections.Generic;
using JAStudio.Anki.AnkisFunctions;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Note;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Anki;

/// <summary>
/// The study-notes in the collection Anki has open (<see cref="ISRSStudyNotes"/>): read from its database (<see cref="AnkiCollectionDatabase"/>),<br/>
/// and changed by calling Anki (<see cref="IAnkisNotes"/>). A note's study-note is its Anki note, found by its id (<see cref="StudyNoteIds"/>).
/// </summary>
public class AnkiStudyNotes : ISRSStudyNotes
{
   readonly AnkiCollectionDatabase _database;
   readonly StudyNoteIds _studyNoteIds;
   readonly IAnkisNotes _ankisNotes;
   readonly IAnkisWindows _ankisWindows;

   public AnkiStudyNotes(AnkiCollectionDatabase database, StudyNoteIds studyNoteIds, IAnkisNotes ankisNotes, IAnkisWindows ankisWindows)
   {
      _database = database;
      _studyNoteIds = studyNoteIds;
      _ankisNotes = ankisNotes;
      _ankisWindows = ankisWindows;
   }

   public IReadOnlyList<StudyNoteInTheSRS> ReadAll() => StudyNotesInAnkisDatabase.ReadAll(_database);

   public void Write(JPNote note) => _ankisNotes.WriteTheFields(_studyNoteIds.RequiredStudyNoteIdOf(note.GetId()), note.StudyNoteFields());

   public void SuspendAllCardsForNote(NoteId noteId) => _ankisNotes.SuspendAllCardsOf(_studyNoteIds.RequiredStudyNoteIdOf(noteId));

   public void UnsuspendAllCardsForNote(NoteId noteId) => _ankisNotes.UnsuspendAllCardsOf(_studyNoteIds.RequiredStudyNoteIdOf(noteId));

   /// <summary>
   /// Removes the note's Anki note, if it has one, then resets the search of Anki's browser, which would otherwise fail when it refreshes the note<br/>
   /// that no longer exists.
   /// </summary>
   public void DeleteTheStudyNoteOf(NoteId noteId)
   {
      var studyNoteId = _studyNoteIds.StudyNoteIdOf(noteId);
      if(!studyNoteId.HasValue) return;

      _ankisNotes.Delete(studyNoteId.Value);
      _ankisWindows.OpenTheBrowserOn("");
   }
}
