using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Storage;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Notes;

/// <summary>The tools that find notes and show them.</summary>
[McpServerToolType]
public class NoteTools
{
   /// <summary>How many notes <see cref="SearchNotes"/> lists unless told otherwise, so that a search matching most of the corpus doesn't flood Claude's context.</summary>
   public const int NotesListedByDefault = 50;

   readonly JPCollection _collection;
   readonly NoteSerializer _noteSerializer;

   public NoteTools(JPCollection collection, NoteSerializer noteSerializer)
   {
      _collection = collection;
      _noteSerializer = noteSerializer;
   }

   [McpServerTool(Name = "search_notes", ReadOnly = true, Idempotent = true)]
   [Description("""
                Searches the collection's notes, as the search in JAStudio's navigation pane does, and lists the notes found,
                ordered by note type, then question. The search is a tree of conditions in the JSON form JAStudio keeps searches in:
                - {"allOf": [condition, ...]}, {"anyOf": [condition, ...]}, {"not": condition}
                - {"text": "Question" | "Answer" | "Searchable text", plus one of "contains": "...", "is": "...", "matchesWildcards": "...",
                  "matchesRegex": "..."}. Searchable text is the question and answer of every note, and a vocab's or kanji's readings
                  in kana and in romaji, with their HTML stripped.
                - {"tagsInclude": "source::anime"}, which includes the tag's whole branch.
                - {"trueOrFalse": "Studying reading" | "Studying listening", "is": true | false}
                - {"noteTypeIs": "vocab" | "kanji" | "sentence"}
                {"allOf": []} matches every note.
                """)]
   public FoundNotes SearchNotes(
      [Description("The search: a tree of conditions, as described above.")] JsonObject search,
      [Description("The most notes to list. Every note found is counted either way.")] int maxNotesListed = NotesListedByDefault)
   {
      var notesFound = _collection.NotesMatching(NoteConditionJson.ConditionFrom(search))
                                  .OrderBy(it => NoteConditionJson.NoteTypeNameOf(it.GetType()), StringComparer.Ordinal)
                                  .ThenBy(it => it.GetQuestion(), StringComparer.Ordinal)
                                  .ToList();

      return new FoundNotes(notesFound.Count, [.. notesFound.Take(maxNotesListed).Select(NoteSummary.Of)]);
   }

   [McpServerTool(Name = "get_note", ReadOnly = true, Idempotent = true)]
   [Description("Gets a note as the corpus stores it: the JSON in its file in the corpus repository, as it is right now in JAStudio's memory.")]
   public string GetNote([Description("The note's id, a GUID, as search_notes lists it.")] string noteId) =>
      _noteSerializer.Serialize(_collection.NoteFromNoteId(NoteId.Parse(noteId))
                             ?? throw new ArgumentException($"No note has the id {noteId}.", nameof(noteId)));
}
