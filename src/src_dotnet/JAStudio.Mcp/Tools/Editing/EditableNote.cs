using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Search;
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

   /// <summary><paramref name="note"/>, with its editable fields, or, for a type of note whose fields can't be edited here yet, a failure saying so.</summary>
   public static EditableNote Of(JPNote note) => note switch
   {
      VocabNote vocab => new EditableNote(vocab, EditableFieldOfANote.EachOf(VocabEditableFields.All, vocab)),
      _ => throw new ArgumentException($"Only vocab have editable fields here so far, and this is a {TypeName(note)}.")
   };

   public JPNote Note { get; }
   public IReadOnlyList<EditableFieldOfANote> Fields { get; }

   /// <summary>The field named <paramref name="name"/>, or, when the note has none, a failure that lists the names of those it has.</summary>
   public EditableFieldOfANote FieldNamed(string name) =>
      Fields.SingleOrDefault(it => it.Name == name)
      ?? throw new ArgumentException($"A {TypeName(Note)} has no editable field named '{name}'. Its editable fields are: {string.Join(", ", Fields.Select(it => $"\"{it.Name}\""))}.");

   static string TypeName(JPNote note) => NoteConditionJson.NoteTypeNameOf(note.GetType());
}
