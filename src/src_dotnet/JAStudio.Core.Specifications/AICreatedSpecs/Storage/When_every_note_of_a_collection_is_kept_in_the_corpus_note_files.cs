using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Must;
using Compze.Teventive.StateStore.Storage;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using JAStudio.Core.Storage.Dto;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Storage;

/// <summary>
/// Every note of the specifications' collection, written to <see cref="CorpusNoteFiles"/> and loaded back, from the notes' files and then from<br/>
/// the snapshot caches the first load writes: each note loads as it was written, as JAStudio writes a note's file (<see cref="NoteSerializer"/>).
/// </summary>
public class When_every_note_of_a_collection_is_kept_in_the_corpus_note_files : SpecificationUsingACollection, IAIGeneratedSpec, IDisposable
{
   readonly DirectoryInfo _corpusCheckout = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"JAStudio_every_note_in_the_files_{Guid.NewGuid():N}"));
   readonly NoteSerializer _serializer;
   readonly IReadOnlyList<JPNote> _written;

   public When_every_note_of_a_collection_is_kept_in_the_corpus_note_files()
   {
      _serializer = GetService<NoteSerializer>();
      _written = [.. NoteServices.Collection.Kanji.All(), .. NoteServices.Collection.Vocab.All(), .. NoteServices.Collection.Sentences.All()];
      new CorpusNoteFiles(_corpusCheckout.FullName).Prepare([.. _written.Select(it => new TaggregateSnapshot(it.GetType(), it.Id, it.ToDto()))], []).Commit();
   }

   public new void Dispose()
   {
      base.Dispose();
      _corpusCheckout.Delete(recursive: true);
   }

   IReadOnlyList<TaggregateSnapshot> LoadEveryNote() => new CorpusNoteFiles(_corpusCheckout.FullName).LoadAll();

   /// <summary>The id of each note that <paramref name="loaded"/> holds otherwise than it was written, or that only one of the two has.</summary>
   IReadOnlyList<Guid> TheNotesLoadedOtherwiseThanWritten(IReadOnlyList<TaggregateSnapshot> loaded)
   {
      var written = _written.ToDictionary(it => it.GetId().Value, _serializer.Serialize);
      var asLoaded = loaded.ToDictionary(it => it.Id.Value, it => _serializer.Serialize(RestoredFrom(it.Snapshot)));
      return [.. written.Keys.Union(asLoaded.Keys).Where(id => written.GetValueOrDefault(id) != asLoaded.GetValueOrDefault(id))];
   }

   JPNote RestoredFrom(object snapshot) => snapshot switch
   {
      KanjiDto kanji => new KanjiNote(NoteServices, kanji),
      VocabDto vocab => new VocabNote(NoteServices, vocab),
      SentenceDto sentence => new SentenceNote(NoteServices, sentence),
      _ => throw new ArgumentException($"A {snapshot.GetType().FullName} isn't a note's DTO.", nameof(snapshot))
   };

   public class loading_every_note_from_the_files : When_every_note_of_a_collection_is_kept_in_the_corpus_note_files
   {
      [XF] public void gives_each_note_as_it_was_written() => TheNotesLoadedOtherwiseThanWritten(LoadEveryNote()).Must().BeEmpty();
   }

   public class loading_every_note_again_from_the_snapshot_caches : When_every_note_of_a_collection_is_kept_in_the_corpus_note_files
   {
      public loading_every_note_again_from_the_snapshot_caches() => LoadEveryNote();

      [XF] public void gives_each_note_as_it_was_written() => TheNotesLoadedOtherwiseThanWritten(LoadEveryNote()).Must().BeEmpty();
   }
}
