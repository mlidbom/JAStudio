using System;
using System.ComponentModel;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Mcp.Tools.Notes;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// The tools that do what the context menu's "Universal note actions" do to a note of any type: suspend all its cards, unsuspend them, and delete<br/>
/// the note. Each changes the note's Anki note too, so they work only in the live JAStudio, inside Anki: the headless one refuses them.
/// </summary>
[McpServerToolType]
public class UniversalNoteActionTools
{
   readonly JPCollection _collection;

   public UniversalNoteActionTools(JPCollection collection) => _collection = collection;

   [McpServerTool(Name = "suspend_all_cards_of_a_note", Idempotent = true)]
   [Description("""
                Suspends all the cards of a note in Anki, as the context menu's "Suspend all cards" does, so they're no longer studied. Only in the live
                JAStudio, inside Anki: the headless one has no Anki and refuses. Reports the note, with whether it has active and suspended cards now.
                """)]
   public NoteCards SuspendAllCardsOfANote([Description("The note's id, as search_notes lists it.")] string noteId)
   {
      var note = NoteWithTheId(noteId);
      note.SuspendAllCards();
      return NoteCards.Of(note);
   }

   [McpServerTool(Name = "unsuspend_all_cards_of_a_note", Idempotent = true)]
   [Description("""
                Unsuspends all the cards of a note in Anki, as the context menu's "Unsuspend all cards" does, so they're studied again. Only in the live
                JAStudio, inside Anki: the headless one has no Anki and refuses. Reports the note, with whether it has active and suspended cards now.
                """)]
   public NoteCards UnsuspendAllCardsOfANote([Description("The note's id, as search_notes lists it.")] string noteId)
   {
      var note = NoteWithTheId(noteId);
      note.UnsuspendAllCards();
      return NoteCards.Of(note);
   }

   [McpServerTool(Name = "delete_a_note", Destructive = true)]
   [Description("""
                Deletes a note from JAStudio, as the context menu's "Delete note" does: from the collection and the corpus checkout, with the media
                no other note uses. A note with an Anki note, which holds its cards and their review history, needs the choice the menu asks for:
                ankiNote "keep" leaves the Anki note as it is, with its history, and its cards then show that the note no longer exists in JAStudio;
                "delete" deletes it from Anki too, with its history, which can't be undone. Without the choice, deleting such a note fails, saying
                so. A note with no Anki note, such as one just created, needs no choice. The headless JAStudio can only keep an Anki note, having no
                Anki to delete it from. Check that it's the note meant before deleting it. Reports the note deleted.
                """)]
   public NoteSummary DeleteANote(
      [Description("The note's id, as search_notes lists it.")] string noteId,
      [Description("""What to do with the note's Anki note, if it has one: "keep" or "delete". Needed only for a note that has an Anki note.""")] string? ankiNote = null)
   {
      var note = NoteWithTheId(noteId);
      var deleted = NoteSummary.Of(note);
      _collection.Delete(note, WhatToDoWithTheAnkiNoteOf(note, ankiNote));
      return deleted;
   }

   JPNote NoteWithTheId(string noteId) => _collection.NoteFromNoteId(NoteId.Parse(noteId)) ?? throw new ArgumentException($"No note has the id {noteId}.", nameof(noteId));

   /// <summary>
   /// What <c>delete_a_note</c>'s <paramref name="ankiNote"/> says to do with the Anki note of <paramref name="note"/>, or, when the note has an Anki note<br/>
   /// and <paramref name="ankiNote"/> says nothing, a failure asking for the choice. A note without an Anki note needs none.
   /// </summary>
   static ItsAnkiNote WhatToDoWithTheAnkiNoteOf(JPNote note, string? ankiNote) => ankiNote switch
   {
      "keep"                         => ItsAnkiNote.IsKept,
      "delete"                       => ItsAnkiNote.IsDeleted,
      null when !note.HasAnAnkiNote() => ItsAnkiNote.IsKept,
      null => throw new ArgumentException($"""
                                            "{note.GetQuestion()}" has an Anki note, which holds its cards and their review history. Say what to do with it: ankiNote "keep" keeps it,
                                            with its history, and "delete" deletes it from Anki too, with its history, which can't be undone.
                                            """, nameof(ankiNote)),
      _ => throw new ArgumentException($"""ankiNote is "keep" or "delete", not "{ankiNote}".""", nameof(ankiNote))
   };
}
