using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Note;

namespace JAStudio.Anki;

/// <summary>
/// Reads the study-notes, the Anki notes of JAStudio's note types, straight from an Anki collection's database: JAStudio inside Anki reads the<br/>
/// collection Anki has open, the headless JAStudio the one it was started with.
/// </summary>
public static class StudyNotesInAnkisDatabase
{
   const char FieldSeparator = '\x1f';

   /// <summary>Every study-note in the collection whose database is <paramref name="collectionDatabaseFile"/>, <c>collection.anki2</c>.</summary>
   public static IReadOnlyList<StudyNoteInAnki> ReadAll(string collectionDatabaseFile)
   {
      using var db = AnkiDb.OpenReadOnly(collectionDatabaseFile);
      var fields = db.Fields.ToList();
      var fieldNamesByNoteTypeOfStudyNotes = db.NoteTypes.ToList()
                                              .Where(it => NoteTypes.All.Contains(it.Name))
                                              .ToDictionary(it => it.Id, it => AnkiRawNoteTypes.FieldNamesOf(it.Id, fields));
      var noteTypeIdsOfStudyNotes = fieldNamesByNoteTypeOfStudyNotes.Keys.ToList();

      return
      [
         .. db.Notes.Where(it => noteTypeIdsOfStudyNotes.Contains(it.NoteTypeId))
              .ToList()
              .Select(it => StudyNoteFrom(it, fieldNamesByNoteTypeOfStudyNotes[it.NoteTypeId]))
      ];
   }

   static StudyNoteInAnki StudyNoteFrom(NoteRow row, IReadOnlyList<string> fieldNames)
   {
      var fieldValues = row.Fields.Split(FieldSeparator);
      return new StudyNoteInAnki(row.Id, fieldNames.Select((name, ordinal) => (name, ordinal)).ToDictionary(it => it.name, it => fieldValues[it.ordinal]));
   }
}
