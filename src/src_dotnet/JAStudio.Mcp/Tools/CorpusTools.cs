using System.ComponentModel;
using JAStudio.Core.Note.Collection;
using JAStudio.Mcp.Corpus;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools;

/// <summary>The tools about the corpus checkout a JAStudio works on, as git sees it.</summary>
[McpServerToolType]
public class CorpusTools
{
   /// <summary>How many notes of each type <see cref="ListChangedNotes"/> lists unless told otherwise, so that a change to most of the corpus doesn't flood Claude's context.</summary>
   internal const int NotesListedPerTypeByDefault = 20;

   readonly JPCollection _collection;
   readonly CorpusCheckout _corpusCheckout;

   public CorpusTools(JPCollection collection, CorpusCheckout corpusCheckout)
   {
      _collection = collection;
      _corpusCheckout = corpusCheckout;
   }

   [McpServerTool(Name = "list_changed_notes", ReadOnly = true, Idempotent = true)]
   [Description("""
                Lists the notes whose files in the corpus checkout differ from the checkout's git HEAD, changed, added or deleted, by note type:
                what changes to the notes since the last commit changed, such as a full rebuild. Counts every changed note, and lists the
                first of each type by question. Files of notes the collection doesn't have, such as deleted ones, are listed by note id.
                The corpus checkout's git diff shows how each note changed.
                """)]
   public ChangedNotes ListChangedNotes(
      [Description("The most notes of each type to list. Every changed note is counted either way.")] int maxNotesListedPerType = NotesListedPerTypeByDefault) =>
      ChangedNotes.In(_collection, _corpusCheckout.IdsOfTheNotesWhoseFilesDifferFromHead(), maxNotesListedPerType);
}
