using System.Collections.Generic;
using System.Linq;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Anki;

/// <summary>
/// Reads the study-notes, the Anki notes of JAStudio's note types, from an Anki collection's database (<see cref="AnkiCollectionDatabase"/>), each<br/>
/// with the corpus note it is, its fields, its tags and the studying statuses of its cards: JAStudio inside Anki reads the collection Anki has<br/>
/// open, through Anki's own connection, and the headless JAStudio the collection file of an Anki profile.
/// </summary>
public static partial class StudyNotesInAnkisDatabase
{
   /// <summary>
   /// Every study-note in <paramref name="database"/>, read with a SELECT each of the note types, the study-notes and the cards, one after<br/>
   /// another: inside Anki, Anki's backend runs one at a time anyway.
   /// </summary>
   public static IReadOnlyList<StudyNoteInTheSRS> ReadAll(AnkiCollectionDatabase database)
   {
      var studyNoteTypes = NoteTypesInAnki.ReadFrom(database).OfStudyNotesById();
      var studyNotes = database.Read<StudyNoteRow>(StudyNoteRow.SelectEveryOf(studyNoteTypes.Keys));
      var cardsOfEachAnkiNote = database.Read<CardRow>(CardRow.SelectEvery).ToLookup(it => it.AnkiNoteId);

      return [.. studyNotes.Select(it => it.AsAStudyNoteOf(studyNoteTypes[it.NoteTypeId], cardsOfEachAnkiNote[it.AnkiNoteId]))];
   }
}
