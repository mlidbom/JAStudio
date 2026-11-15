using System.Collections.Generic;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Note;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Anki;

/// <summary>The study-notes in the collection Anki has open: read from its database (<see cref="AnkiCollectionDatabase"/>), and written through Anki.</summary>
public class AnkiStudyNotes : ISpacedRepetitionSystemStudyNotes
{
   readonly AnkiCollectionDatabase _database;
   readonly StudyNoteIds _ankiNoteIds;

   public AnkiStudyNotes(AnkiCollectionDatabase database, StudyNoteIds ankiNoteIds)
   {
      _database = database;
      _ankiNoteIds = ankiNoteIds;
   }

   public IReadOnlyList<StudyNoteInTheSpacedRepetitionSystem> ReadAll() => StudyNotesInAnkisDatabase.ReadAll(_database);

   public void Write(JPNote note) => AnkiFacade.StudyNotes.Write(_ankiNoteIds.RequiredStudyNoteIdOf(note.GetId()), note);
}
