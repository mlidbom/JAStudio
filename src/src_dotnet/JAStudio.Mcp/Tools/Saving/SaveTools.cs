using System.ComponentModel;
using System.Diagnostics;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage;
using JAStudio.Mcp.Corpus;
using JAStudio.Mcp.Tools.Corpus;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Saving;

/// <summary>The tools that write the notes to the corpus checkout as JAStudio stores them.</summary>
[McpServerToolType]
public class SaveTools
{
   readonly JPCollection _collection;
   readonly INoteRepository _noteRepository;
   readonly CorpusCheckout _corpusCheckout;

   public SaveTools(JPCollection collection, INoteRepository noteRepository, CorpusCheckout corpusCheckout)
   {
      _collection = collection;
      _noteRepository = noteRepository;
      _corpusCheckout = corpusCheckout;
   }

   [McpServerTool(Name = "save_every_note", Destructive = false, Idempotent = true)]
   [Description("""
                Writes every note to the corpus checkout as JAStudio stores it now, changed or not, which JAStudio otherwise never does: it writes a
                note's file only when the note changes. So it shows what loading and saving does to the notes' files, as after a change to how
                notes are stored, or a migration of them: run it on a clean checkout, and the notes whose files changed are those JAStudio would
                write differently from how their files are now, which list_changed_notes and diff_the_changed_notes then show. Reports how many
                notes it wrote, how long it took, and the notes whose files now differ from the checkout's git HEAD. Meant for the headless
                JAStudio's own worktree of the corpus repository.
                """)]
   public EveryNoteSaved SaveEveryNote(
      [Description("The most changed notes of each type to list. Every changed note is counted either way.")] int maxNotesListedPerType = CorpusTools.NotesListedPerTypeByDefault)
   {
      var stopwatch = Stopwatch.StartNew();
      var (vocab, kanji, sentences) = (_collection.Vocab.All(), _collection.Kanji.All(), _collection.Sentences.All());
      vocab.ForEach(_noteRepository.Save);
      kanji.ForEach(_noteRepository.Save);
      sentences.ForEach(_noteRepository.Save);
      var secondsTaken = stopwatch.Elapsed.TotalSeconds;

      return new EveryNoteSaved(vocab.Count + kanji.Count + sentences.Count,
                                secondsTaken,
                                ChangedNotes.In(_collection, _corpusCheckout.IdsOfTheNotesWhoseFilesDifferFromHead(), maxNotesListedPerType));
   }
}
