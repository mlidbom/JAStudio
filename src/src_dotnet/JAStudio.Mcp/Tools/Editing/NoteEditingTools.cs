using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Batches;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Editing.Fields;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// The tools that edit a note as its editor in the workspace does: each of the fields the editor edits (<see cref="EditableNote"/>), written<br/>
/// through the note's own operations, so each edit is saved to the corpus checkout and passed on to the note's Anki note, like any edit.
/// </summary>
[McpServerToolType]
public class NoteEditingTools
{
   readonly JPCollection _collection;
   readonly LocalNoteUpdater _localNoteUpdater;

   public NoteEditingTools(JPCollection collection, LocalNoteUpdater localNoteUpdater)
   {
      _collection = collection;
      _localNoteUpdater = localNoteUpdater;
   }

   [McpServerTool(Name = "get_the_editable_fields_of_a_note", ReadOnly = true, Idempotent = true)]
   [Description("""
                Lists the fields of a note that edit_a_note edits: every field a person edits, first those of its editor in the workspace, in the
                order of the editor's tabs, then those its menus edit, such as a vocab's related vocab and a sentence's highlighted vocab and
                incorrect and hidden matches. Each with its name, which is its path in the note as get_note shows it, such as "corpusAnswer",
                "matching.requiredOrForbidden.surface" or, for a flag in a list of flags, "matching.flags.isPoisonWord"; the label the editor
                shows; what value it takes; whether a change to it reparses the sentences that may contain the vocab; and its value now.
                """)]
   public EditableFieldsReported GetTheEditableFieldsOfANote([Description("The note's id, as search_notes lists it.")] string noteId) =>
      EditableFieldsReported.Of(EditableNote.WithTheId(noteId, _collection));

   [McpServerTool(Name = "edit_a_note", Destructive = true, Idempotent = true)]
   [Description("""
                Edits a note as its editor in the workspace does: writes each field given through the note's own operations, then does what the
                editor does after a change. When a field written changes how sentences parse, such as a vocab's forms or match-condition, the
                sentences that may contain the vocab are reparsed; then the note's generated data is updated. The note is saved to the corpus
                checkout, and its Anki note updated, as any edit is. Fields are named, and their values given, as
                get_the_editable_fields_of_a_note lists them. Every name and value is checked before anything is written; a value the note
                refuses, such as an unknown part of speech, fails the edit at that field, after the fields before it were written. Related
                vocab are linked both ways, as when a person edits them: writing a vocab's synonyms, perfect synonyms or ergative twins changes
                the vocab they name too, and writing a kanji's similar meaning or confused with changes the kanji they name. A sentence's
                highlighted vocab and incorrect and hidden matches reparse the sentence. Reports each field of the note that changed, before and
                after, in the order of its fields, those the edit didn't write included, such as a vocab's synonyms when its perfect synonyms are
                written.
                """)]
   public NoteEdit EditANote(
      [Description("The note's id, as search_notes lists it.")] string noteId,
      [Description("""The fields to write, by name, each with its new value, such as {"corpusAnswer": "went", "matching.requiredOrForbidden.surface": "required"}.""")]
      JsonObject changes)
   {
      var note = EditableNote.WithTheId(noteId, _collection);
      var writes = FieldWrite.EachIn(changes, note);
      var valuesBefore = NoteFieldValues.Of(note);

      foreach(var write in writes) write.Write();
      var sentencesReparsed = ReparseTheSentencesWhenAWriteChangesHowTheyParse(note.Note, writes);
      note.Note.UpdateGeneratedData();

      return new NoteEdit(note.Note.GetQuestion(), valuesBefore.ChangesTo(NoteFieldValues.Of(note)), sentencesReparsed);
   }

   /// <summary>
   /// Reparses the sentences that may contain <paramref name="note"/>, a vocab, when one of <paramref name="writes"/> changes how sentences parse,<br/>
   /// as the vocab's editor does. Whether it did.
   /// </summary>
   bool ReparseTheSentencesWhenAWriteChangesHowTheyParse(JPNote note, IReadOnlyList<FieldWrite> writes)
   {
      if(note is not VocabNote vocab || !writes.Any(it => it.Field.ChangesHowSentencesParse)) return false;

      _localNoteUpdater.ReparseSentencesForVocabWithoutProgressDialog(vocab);
      return true;
   }
}
