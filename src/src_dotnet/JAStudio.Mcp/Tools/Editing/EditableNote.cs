using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.EditableFields;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>A note, with the fields of it that its editor in the workspace edits (<see cref="EditableFieldOfANote"/>): what <c>edit_a_note</c> edits.</summary>
public sealed class EditableNote
{
   EditableNote(JPNote note, IReadOnlyList<EditableFieldOfANote> fields)
   {
      Note = note;
      Fields = fields;
   }

   /// <summary><paramref name="note"/>, with its editable fields: those of its type, such as <see cref="VocabEditableFields"/>.</summary>
   public static EditableNote Of(JPNote note) => note switch
   {
      VocabNote vocab => new EditableNote(vocab, EditableFieldOfANote.EachOf(VocabEditableFields.All, vocab)),
      KanjiNote kanji => new EditableNote(kanji, EditableFieldOfANote.EachOf(KanjiEditableFields.All, kanji)),
      SentenceNote sentence => new EditableNote(sentence, EditableFieldOfANote.EachOf(SentenceEditableFields.All, sentence)),
      _ => throw new ArgumentOutOfRangeException(nameof(note), note.GetType(), "JAStudio has no notes of this type.")
   };

   public JPNote Note { get; }
   public IReadOnlyList<EditableFieldOfANote> Fields { get; }

   /// <summary>The field named <paramref name="name"/>, or, when the note has none, a failure that lists the names of those it has.</summary>
   public EditableFieldOfANote FieldNamed(string name) =>
      Fields.SingleOrDefault(it => it.Name == name)
      ?? throw new ArgumentException($"A {TypeName(Note)} has no editable field named '{name}'. Its editable fields are: {string.Join(", ", Fields.Select(it => $"\"{it.Name}\""))}.");

   static string TypeName(JPNote note) => NoteConditionJson.NoteTypeNameOf(note.GetType());
}
