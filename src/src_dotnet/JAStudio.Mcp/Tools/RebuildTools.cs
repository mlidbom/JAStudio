using System.ComponentModel;
using System.Diagnostics;
using JAStudio.Core.Batches;
using JAStudio.Core.Note.Collection;
using JAStudio.Mcp.Corpus;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools;

/// <summary>The tools that regenerate what JAStudio computes from the notes, such as sentences' parsing results.</summary>
[McpServerToolType]
public class RebuildTools
{
   readonly LocalNoteUpdater _localNoteUpdater;
   readonly JPCollection _collection;
   readonly CorpusCheckout _corpusCheckout;

   public RebuildTools(LocalNoteUpdater localNoteUpdater, JPCollection collection, CorpusCheckout corpusCheckout)
   {
      _localNoteUpdater = localNoteUpdater;
      _collection = collection;
      _corpusCheckout = corpusCheckout;
   }

   [McpServerTool(Name = "full_rebuild", Destructive = false, Idempotent = true)]
   [Description("""
                Runs JAStudio's full rebuild, as the workspace's Local Actions > Update > "All the above: Full rebuild" does: reparses every
                sentence, then updates every note's generated data and tags. Each note it changes is saved to the corpus checkout. Reports how
                long it took, and the notes whose files now differ from the checkout's git HEAD, as list_changed_notes does: run it on a clean
                checkout to see exactly what the rebuild changed.
                """)]
   public FullRebuildResult FullRebuild(
      [Description("The most changed notes of each type to list. Every changed note is counted either way.")] int maxNotesListedPerType = CorpusTools.NotesListedPerTypeByDefault)
   {
      var stopwatch = Stopwatch.StartNew();
      _localNoteUpdater.FullRebuild();
      var secondsTaken = stopwatch.Elapsed.TotalSeconds;

      return new FullRebuildResult(secondsTaken, ChangedNotes.In(_collection, _corpusCheckout.IdsOfTheNotesWhoseFilesDifferFromHead(), maxNotesListedPerType));
   }
}
