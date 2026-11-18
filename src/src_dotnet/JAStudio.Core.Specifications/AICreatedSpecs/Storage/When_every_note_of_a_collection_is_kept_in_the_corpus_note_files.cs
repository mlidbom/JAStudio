using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Must;
using Compze.Teventive.MemoryImageStateStore;
using Compze.Teventive.MemoryImageStateStore.Testing;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Storage;

/// <summary>
/// Every note of the specifications' collection, which the memory image state store wrote to its file in the specification's corpus checkout<br/>
/// (<see cref="CorpusNoteFiles"/>) as the unit of work that created it, or last changed it, committed.
/// </summary>
public class When_every_note_of_a_collection_is_kept_in_the_corpus_note_files : SpecificationUsingACollection, IAIGeneratedSpec
{
   readonly CorpusNoteFiles _noteFiles;
   readonly NoteSerializer _serializer;
   readonly IReadOnlyList<JPNote> _notes;

   public When_every_note_of_a_collection_is_kept_in_the_corpus_note_files()
   {
      _noteFiles = GetService<CorpusNoteFiles>();
      _serializer = GetService<NoteSerializer>();
      _notes = EveryNoteHeld();
   }

   IReadOnlyList<JPNote> EveryNoteHeld() => [.. NoteServices.Collection.Kanji.All(), .. NoteServices.Collection.Vocab.All(), .. NoteServices.Collection.Sentences.All()];

   /// <summary>Each note, as JAStudio writes a note's file (<see cref="NoteSerializer"/>), by its id.</summary>
   IReadOnlyDictionary<NoteId, string> AsJAStudioWritesThem(IEnumerable<JPNote> notes) => notes.ToDictionary(it => it.GetId(), _serializer.Serialize);

   [XF] public void the_collection_holds_notes_of_each_class() =>
      _notes.Select(it => it.GetType().Name).Distinct().Order().Must().SequenceEqual([nameof(KanjiNote), nameof(SentenceNote), nameof(VocabNote)]);

   [XF] public void each_notes_file_holds_exactly_what_JAStudio_writes_of_it() =>
      _notes.Where(it => TheStoredFileOf(it) != _serializer.Serialize(it)).Select(it => it.GetQuestion()).Must().BeEmpty();

   [XF] public void the_notes_folders_hold_a_file_for_each_note_and_no_other() =>
      GetService<IFileSystemInMemory>().Paths.Count(IsInAFolderOfNotes).Must().Be(_notes.Count);

   bool IsInAFolderOfNotes(string path) => new[] { "kanji", "vocab", "sentences" }.Any(it => path.StartsWith(Path.Combine(_noteFiles.CorpusCheckout.FullName, it) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));

   public class loading_the_collection_again_from_the_files : When_every_note_of_a_collection_is_kept_in_the_corpus_note_files
   {
      readonly IReadOnlyDictionary<NoteId, string> _heldBefore;
      readonly IReadOnlyList<JPNote> _notesChangedOnDisk;

      public loading_the_collection_again_from_the_files()
      {
         _heldBefore = AsJAStudioWritesThem(_notes);
         _notesChangedOnDisk = NoteServices.Collection.ReloadFromBackend();
      }

      [XF] public void finds_no_note_changed_on_disk() => _notesChangedOnDisk.Must().BeEmpty();
      [XF] public void holds_each_note_as_it_held_it_before() => AsJAStudioWritesThem(EveryNoteHeld()).Must().DeepEqual(_heldBefore);
   }
}
