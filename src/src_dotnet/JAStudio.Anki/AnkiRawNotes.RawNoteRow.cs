using System.Collections.Generic;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Anki;

public partial class AnkiRawNotes
{
   /// <summary>A raw note as it's read: the id of its Anki note, and its fields and tags as Anki stores them.</summary>
   sealed record RawNoteRow(long AnkiNoteId, string Fields, string Tags)
   {
      /// <summary>The notes with <paramref name="ankiNoteIds"/>.</summary>
      public static string SelectThoseWith(IEnumerable<long> ankiNoteIds) =>
         $"select json_group_array(json_object('ankiNoteId', id, 'fields', flds, 'tags', tags)) from notes where id in {AnkiCollectionDatabase.SqlListOf(ankiNoteIds)}";

      /// <summary>The raw note, a note of <paramref name="noteType"/>, its note type.</summary>
      public RawNote AsARawNoteOf(NoteTypeInAnki noteType) =>
         new(AnkiNoteId, noteType.FieldsByNameIn(new NoteFieldsAsAnkiStoresThem(Fields)), new NoteTagsAsAnkiStoresThem(Tags).Names);
   }
}
