using System.Collections.Generic;
using System.Linq;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Anki.StudyNotes;

namespace JAStudio.Anki;

/// <summary>
/// Reads the study-notes, the Anki notes of JAStudio's note types, with all their fields, from an Anki collection's database: JAStudio inside<br/>
/// Anki reads the collection Anki has open, the headless JAStudio the one it was started with.
/// </summary>
public static partial class StudyNotesInAnkisDatabase
{
   /// <summary>Every study-note in <paramref name="database"/>.</summary>
   public static IReadOnlyList<StudyNoteInAnki> ReadAll(AnkiCollectionDatabase database)
   {
      var studyNoteTypes = NoteTypesInAnki.ReadFrom(database).OfStudyNotesById();
      return [.. database.Read<StudyNoteWithItsFieldsRow>(StudyNoteWithItsFieldsRow.SelectEveryOf(studyNoteTypes.Keys)).Select(it => it.AsAStudyNoteOf(studyNoteTypes[it.NoteTypeId]))];
   }
}
