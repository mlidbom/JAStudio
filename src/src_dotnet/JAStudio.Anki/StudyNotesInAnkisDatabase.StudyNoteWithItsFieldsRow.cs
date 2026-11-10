using System.Collections.Generic;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Anki.StudyNotes;

namespace JAStudio.Anki;

public static partial class StudyNotesInAnkisDatabase
{
   /// <summary>A study-note as it's read: the id of its Anki note and of its note type, and its fields as Anki stores them.</summary>
   sealed record StudyNoteWithItsFieldsRow(long AnkiNoteId, long NoteTypeId, string Fields)
   {
      /// <summary>Every study-note of the note types with <paramref name="noteTypeIds"/>.</summary>
      public static string SelectEveryOf(IEnumerable<long> noteTypeIds) =>
         $"select json_group_array(json_object('ankiNoteId', id, 'noteTypeId', mid, 'fields', flds)) from notes where mid in {AnkiCollectionDatabase.SqlListOf(noteTypeIds)}";

      /// <summary>The study-note, a note of <paramref name="noteType"/>, its note type.</summary>
      public StudyNoteInAnki AsAStudyNoteOf(NoteTypeInAnki noteType) => new(AnkiNoteId, noteType.FieldsByNameIn(new NoteFieldsAsAnkiStoresThem(Fields)));
   }
}
