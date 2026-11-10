using System;
using System.Collections.Generic;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;

namespace JAStudio.Anki;

public sealed partial class AnkiBackendDataLoader
{
   /// <summary>
   /// A study-note as the load reads it: the id of its Anki note and of its note type, its <c>jas_note_id</c> field, which holds the id of the<br/>
   /// corpus note it is, and its tags, as Anki stores them.
   /// </summary>
   sealed record StudyNoteRow(long AnkiNoteId, long NoteTypeId, string JasNoteId, string Tags)
   {
      /// <summary>Every study-note of the note types with <paramref name="noteTypeIds"/>: JAStudio's own, each of which has a <c>jas_note_id</c> field.</summary>
      public static string SelectEveryOf(IEnumerable<long> noteTypeIds) =>
         $"""
          select json_group_array(json_object('ankiNoteId', notes.id,
                                              'noteTypeId', notes.mid,
                                              'jasNoteId', field_at_index(notes.flds, fields.ord),
                                              'tags', notes.tags))
          from notes join fields on fields.ntid = notes.mid and fields.name = '{AnkiFieldNames.JasNoteId}'
          where notes.mid in {AnkiCollectionDatabase.SqlListOf(noteTypeIds)}
          """;

      /// <summary>Whether its <c>jas_note_id</c> field holds a corpus note's id: only a study-note whose field does is a corpus note's.</summary>
      public bool HoldsACorpusNoteId => Guid.TryParse(JasNoteId, out _);

      /// <summary>The id of the corpus note it is, a note of <paramref name="noteType"/>, its note type.</summary>
      public NoteId CorpusNoteIdAsANoteOf(NoteTypeInAnki noteType) => NoteTypes.IdFactoryFromName(noteType.Name)(Guid.Parse(JasNoteId));
   }
}
