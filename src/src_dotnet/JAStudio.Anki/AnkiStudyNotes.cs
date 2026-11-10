using System.Collections.Generic;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Note;

namespace JAStudio.Anki;

/// <summary>The study-notes in the collection Anki has open: read from its database (<see cref="AnkiCollectionDatabase"/>), and written through Anki.</summary>
public class AnkiStudyNotes : IAnkiStudyNotes
{
   readonly AnkiCollectionDatabase _database;
   readonly ExternalNoteIdMap _ankiNoteIds;

   public AnkiStudyNotes(AnkiCollectionDatabase database, ExternalNoteIdMap ankiNoteIds)
   {
      _database = database;
      _ankiNoteIds = ankiNoteIds;
   }

   public IReadOnlyList<StudyNoteInAnki> ReadAll() => StudyNotesInAnkisDatabase.ReadAll(_database);

   public void Write(JPNote note) => AnkiFacade.StudyNotes.Write(_ankiNoteIds.RequireExternalId(note.GetId()), note);
}
