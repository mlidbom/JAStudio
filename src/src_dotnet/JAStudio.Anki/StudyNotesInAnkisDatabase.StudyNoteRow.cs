using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Anki;

public static partial class StudyNotesInAnkisDatabase
{
   /// <summary>A study-note as it's read: the id of its Anki note and of its note type, and its fields and its tags as Anki stores them.</summary>
   sealed record StudyNoteRow(long AnkiNoteId, long NoteTypeId, string Fields, string Tags)
   {
      /// <summary>Every study-note of the note types with <paramref name="noteTypeIds"/>: JAStudio's own.</summary>
      public static string SelectEveryOf(IEnumerable<long> noteTypeIds) =>
         $"select json_group_array(json_object('ankiNoteId', id, 'noteTypeId', mid, 'fields', flds, 'tags', tags)) from notes where mid in {AnkiCollectionDatabase.SqlListOf(noteTypeIds)}";

      /// <summary>The study-note, a note of <paramref name="noteType"/>, its note type, with <paramref name="itsCards"/>.</summary>
      public StudyNoteInTheSRS AsAStudyNoteOf(NoteTypeInAnki noteType, IEnumerable<CardRow> itsCards)
      {
         var fields = noteType.FieldsByNameIn(new NoteFieldsAsAnkiStoresThem(Fields));
         return new StudyNoteInTheSRS(AnkiNoteId,
                                      CorpusNoteIdIn(fields, noteType),
                                      fields,
                                      new NoteTagsAsAnkiStoresThem(Tags).Names,
                                      [.. itsCards.Select(it => it.StudyingStatusAsACardOf(noteType))]);
      }

      /// <summary>
      /// The id of the corpus note the study-note is, a note of <paramref name="noteType"/>, which its <c>jas_note_id</c> field holds, as each of<br/>
      /// JAStudio's own note types has one: null when the field holds no corpus note's id, and the study-note so is no corpus note's.
      /// </summary>
      static NoteId? CorpusNoteIdIn(IReadOnlyDictionary<string, string> fields, NoteTypeInAnki noteType) =>
         Guid.TryParse(fields[AnkiFieldNames.JasNoteId], out var corpusNoteId) ? NoteTypes.IdFactoryFromName(noteType.Name)(corpusNoteId) : null;
   }
}
