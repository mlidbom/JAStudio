using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Commands;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Commands;
using JAStudio.Core.Note.Vocabulary.EditableFields;
using JAStudio.Mcp.Tools.Editing.Commands;
using JAStudio.Mcp.Tools.Editing.Fields;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// A note, with what a person edits of it: the fields its editor in the workspace and its menus edit (<see cref="EditableFieldOfANote"/>), which<br/>
/// <c>edit_a_note</c> writes, and the commands of its menus (<see cref="CommandOfANote"/>), which <c>run_a_command_on_a_note</c> runs.
/// </summary>
public sealed class EditableNote
{
   readonly ITaggregateReference<JPNote> _note;

   EditableNote(ITaggregateReference<JPNote> note, IReadOnlyList<EditableFieldOfANote> fields, IReadOnlyList<CommandOfANote> commands)
   {
      _note = note;
      Fields = fields;
      Commands = commands;
   }

   /// <summary><paramref name="note"/>, with its editable fields and commands: those of its type, such as <see cref="VocabEditableFields"/> and <see cref="VocabCommands"/>.</summary>
   public static EditableNote Of(JPNote note) => note switch
   {
      VocabNote vocab => Of(vocab.Reference(), VocabEditableFields.All, VocabCommands.All),
      KanjiNote kanji => Of(kanji.Reference(), KanjiEditableFields.All, KanjiCommands.All),
      SentenceNote sentence => Of(sentence.Reference(), SentenceEditableFields.All, SentenceCommands.All),
      _ => throw new ArgumentOutOfRangeException(nameof(note), note.GetType(), "JAStudio has no notes of this type.")
   };

   /// <summary>The note in <paramref name="collection"/> with the id <paramref name="noteId"/>, as a tool takes it, or, when there's none, a failure saying so.</summary>
   public static EditableNote WithTheId(string noteId, JPCollection collection) =>
      Of(collection.NoteFromNoteId(NoteId.Parse(noteId)) ?? throw new ArgumentException($"No note has the id {noteId}.", nameof(noteId)));

   /// <summary>The note as it is now.</summary>
   public JPNote Note => _note.Current;

   public IReadOnlyList<EditableFieldOfANote> Fields { get; }
   public IReadOnlyList<CommandOfANote> Commands { get; }

   /// <summary>The field named <paramref name="name"/>, or, when the note has none, a failure that lists the names of those it has.</summary>
   public EditableFieldOfANote FieldNamed(string name) =>
      Fields.SingleOrDefault(it => it.Name == name)
      ?? throw new ArgumentException($"A {TypeName(Note)} has no editable field named '{name}'. Its editable fields are: {NamesOf(Fields.Select(it => it.Name))}.");

   /// <summary>The command named <paramref name="name"/>, or, when the note has none, a failure that lists the names of those it has.</summary>
   public CommandOfANote CommandNamed(string name) =>
      Commands.SingleOrDefault(it => it.Name == name)
      ?? throw new ArgumentException($"A {TypeName(Note)} has no command named '{name}'. Its commands are: {NamesOf(Commands.Select(it => it.Name))}.");

   static EditableNote Of<TNote>(ITaggregateReference<TNote> note, IEnumerable<IEditableField<TNote>> fields, IEnumerable<INoteCommand<TNote>> commands) where TNote : JPNote =>
      new(note, EditableFieldOfANote.EachOf(fields, note), CommandOfANote.EachOf(commands, note));

   static string NamesOf(IEnumerable<string> names) => string.Join(", ", names.Select(it => $"\"{it}\""));

   static string TypeName(JPNote note) => NoteConditionJson.NoteTypeNameOf(note.GetType());
}
