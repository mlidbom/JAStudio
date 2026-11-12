using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using JAStudio.Core.Note.Collection;
using JAStudio.Mcp.Corpus;
using JAStudio.Mcp.Tools.Corpus;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Reloading;

/// <summary>
/// The tools that load the collection again from the corpus checkout, after the checkout changed on disk outside JAStudio: how a change made and<br/>
/// checked elsewhere, such as in the headless JAStudio's worktree, reaches a running JAStudio without restarting it.
/// </summary>
[McpServerToolType]
public class ReloadTools
{
   readonly JPCollection _collection;
   readonly CorpusCheckout _corpusCheckout;

   public ReloadTools(JPCollection collection, CorpusCheckout corpusCheckout)
   {
      _collection = collection;
      _corpusCheckout = corpusCheckout;
   }

   [McpServerTool(Name = "reload_the_collection", Destructive = false, Idempotent = true)]
   [Description("""
                Loads the collection again from the corpus checkout on disk, as the workspace's Local Actions > "Reload the collection from disk"
                does: takes in changes made to the checkout outside JAStudio, such as a git reset. Each note whose file changed is passed on as
                updated, so that its Anki note catches up. Reports how long it took, and the notes it found changed. To move the checkout to
                another commit, use fast_forward_the_corpus_checkout, which reloads too.
                """)]
   public ReloadResult ReloadTheCollection(
      [Description("The most changed notes of each type to list. Every changed note is counted either way.")] int maxNotesListedPerType = CorpusTools.NotesListedPerTypeByDefault)
   {
      var stopwatch = Stopwatch.StartNew();
      var notesChangedOnDisk = _collection.ReloadFromBackend();
      return new ReloadResult(stopwatch.Elapsed.TotalSeconds, NotesChangedOnDisk.Of(notesChangedOnDisk, maxNotesListedPerType));
   }

   [McpServerTool(Name = "fast_forward_the_corpus_checkout", Destructive = false, Idempotent = true)]
   [Description("""
                Fast-forwards the corpus checkout this JAStudio works on to a commit of the corpus repository, such as one made in the headless
                JAStudio's worktree, and loads the collection again from it, as reload_the_collection does: how a change to the notes made and
                checked elsewhere reaches this JAStudio without restarting it. The fast-forward is made while the collection holds no notes, so
                nothing JAStudio does meanwhile saves a note over it. Git refuses, changing nothing, when the commit isn't ahead of the checkout's
                HEAD, or when the fast-forward would overwrite changes not committed; the collection is loaded again either way. Only for changes
                to the notes that this JAStudio's code is meant for: a change that comes with new code needs Anki restarted on the new build,
                with the fast-forward made while Anki is closed. Reports the commits before and after, and the notes whose files it changed. A
                commit made in another clone of the corpus repository, such as a linked worktree's src/jas_database, is fetched from it first,
                given fetchingItFrom.
                """)]
   public FastForwardResult FastForwardTheCorpusCheckout(
      [Description("The commit to fast-forward to: its hash, or the name of a branch at it.")] string commit,
      [Description("""
                   The folder of the corpus checkout the commit was made in, when that's another clone of the corpus repository, such as a
                   linked worktree's src/jas_database: git keeps a clone of its own for each worktree's submodule. The commit is fetched from it
                   first. By default the commit must be in this checkout's repository already.
                   """)] string? fetchingItFrom = null,
      [Description("The most changed notes of each type to list. Every changed note is counted either way.")] int maxNotesListedPerType = CorpusTools.NotesListedPerTypeByDefault)
   {
      var stopwatch = Stopwatch.StartNew();
      var commitBefore = _corpusCheckout.Head();
      var commitToFastForwardTo = fetchingItFrom == null ? new CorpusCommit(commit) : _corpusCheckout.FetchedFrom(new DirectoryInfo(fetchingItFrom), new CorpusCommit(commit));
      _collection.ReloadFromBackendAfter(() => _corpusCheckout.FastForwardTo(commitToFastForwardTo));
      var commitAfter = _corpusCheckout.Head();

      return new FastForwardResult(commitBefore.Name,
                                   commitAfter.Name,
                                   stopwatch.Elapsed.TotalSeconds,
                                   ChangedNotes.In(_collection, _corpusCheckout.IdsOfTheNotesWhoseFilesDifferBetween(commitBefore, commitAfter), maxNotesListedPerType));
   }
}
